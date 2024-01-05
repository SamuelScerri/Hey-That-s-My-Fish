using Photon.Pun;
using UnityEngine;

public class TileView : MonoBehaviour, IPunObservable
{
	private byte amount;
	private State currentState;
	private SpriteRenderer spriteRenderer;
	private float bobbing;

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
				PhotonNetwork.Destroy(PhotonView.Get(this));
		}
	}

	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		bobbing = Random.Range(0, 255);
	}

	[PunRPC]
	public void UpdateState(State state)
	{
		CurrentState = state;
	}

	private void Update()
	{
		bobbing += Time.deltaTime;
		transform.position = new Vector3(transform.position.x, Mathf.Sin(bobbing) * .03125f, transform.position.z);
		transform.localEulerAngles = new Vector3(Mathf.Sin(bobbing) * 3.75f, 0,  -Mathf.Sin(bobbing * .5f) * 3.75f);
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
