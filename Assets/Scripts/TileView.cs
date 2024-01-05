using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TileView : MonoBehaviour, IPunObservable
{
	private byte amount;
	private State currentState;
	private SpriteRenderer spriteRenderer;

	private Coroutine coroutine;

	public enum State
	{
		Empty,
		Occupied,
		Full,
	}

	public byte Amount
	{
		get => amount;
		set
		{
			amount = value;
			spriteRenderer.sprite = Singleton.BoardManager.TileSprites[value];
		}
	}

	public State CurrentState
	{
		get => currentState;
		set
		{
			currentState = value;

			if (value == State.Empty)
			{
				//;
				//StartCoroutine(MoveToScoreHolder(GameManager));
			}
				//PhotonNetwork.Destroy(PhotonView.Get(this));
		}
	}

	public IEnumerator FloatAnimation()
	{
		float bobbing = Random.Range(0, 255);

		while (true)
		{
			bobbing += Time.deltaTime;
			transform.position = new Vector3(transform.position.x, Mathf.Sin(bobbing) * .03125f, transform.position.z);
			transform.localEulerAngles = new Vector3(Mathf.Sin(bobbing) * 3.75f, 0,  -Mathf.Sin(bobbing * .5f) * 3.75f);

			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator MoveToScoreHolder(byte id)
	{
		//Transform sprite = transform.GetChild(0);
		//sprite.SetParent(null);

		StopCoroutine(coroutine);

		transform.SetParent(Singleton.GameManager.ScoreHolders[id - 1].transform);
		transform.localEulerAngles = Vector3.zero;

		Vector3 currentVelocity = Vector3.zero;

		int yOffset = Singleton.GameManager.ScoreHolders[id - 1].transform.childCount;

		while (true)
		{
			transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.up * yOffset * .125f, ref currentVelocity, .125f);

			yield return new WaitForEndOfFrame();
		}
	}

	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Start()
	{
		coroutine = StartCoroutine(FloatAnimation());
	}

	public void ShowAvailableTiles()
	{
		//Physics.Raycast(transform.position, )
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(Amount);
			stream.SendNext(CurrentState);
		}
			
		else if (stream.IsReading)
		{
			Amount = (byte)stream.ReceiveNext();
			CurrentState = (State)stream.ReceiveNext();
		}
	}

	public void OnMouseDown()
	{
		if (CurrentState == State.Full)
			Singleton.BoardManager.SelectedTile = this;
	}
}
