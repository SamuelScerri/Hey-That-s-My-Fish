using Photon.Pun;
using UnityEngine;

public class TileView : MonoBehaviour, IPunObservable
{
	private byte amount;
	private State currentState;
	private SpriteRenderer spriteRenderer;

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
			spriteRenderer.sprite = BoardManager.Board.TileSprites[value];
		}
	}

	public State CurrentState
	{
		get => currentState;
		set
		{
			currentState = value;

			if (value == State.Occupied)
				spriteRenderer.sprite = BoardManager.Board.TileSprites[0];
		}
	}

	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	[PunRPC]
	public void UpdateState(State state)
	{
		CurrentState = state;
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
			BoardManager.Board.SelectedTile = this;
	}
}
