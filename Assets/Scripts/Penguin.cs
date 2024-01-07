using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEditorInternal;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	public bool Controllable { get; set; }
	private int currentTile;

	public bool Highlight
	{
		set
		{
			foreach(SkinnedMeshRenderer mesh in GetComponentsInChildren<SkinnedMeshRenderer>())
				mesh.material.color = value ? Color.green : Color.white;
		}
	}

	[PunRPC]
	public void JumpToTile(int tileID)
	{
		transform.SetParent(PhotonView.Find(tileID).transform);
		transform.localPosition = Vector3.up * .1f;

		transform.localEulerAngles = Vector3.down * 90;

		if (PhotonNetwork.IsMasterClient)
			PhotonView.Find(tileID).RPC("SetState", RpcTarget.AllViaServer, TileView.State.Occupied);

		if (CurrentTile != 0)
		{
			StartCoroutine(PhotonView.Find(CurrentTile).GetComponent<TileView>().MoveToScoreHolder((byte) PhotonView.Get(this).OwnerActorNr));

			if (PhotonView.Get(this).IsMine)
			{
				bool possibleMovesAvailable = false;

				//Check If Any Of The Player's Penguins Can Move
				foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
					if(PhotonView.Find(penguin.CurrentTile).GetComponent<TileView>().CheckAndShowAvailableTiles())
						possibleMovesAvailable = true;

				Singleton.BoardManager.ResetTiles();

				if (possibleMovesAvailable)
					PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) (Singleton.GameManager.CurrentPlayerID + 1), new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
				else PhotonNetwork.RaiseEvent(Singleton.EndGameEvent, (byte) (Singleton.GameManager.CurrentPlayerID + 1), new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
			}
		}

		currentTile = tileID;
	}

	public int CurrentTile
	{
		get => currentTile;
		set => PhotonView.Get(this).RPC("JumpToTile", RpcTarget.AllViaServer, value);
	}

	private IEnumerator ControlPenguin()
	{
		foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
			penguin.Controllable = false;

		print("Begin Control Penguin Coroutine");

		Singleton.BoardManager.SelectedTile = null;

		PhotonView.Find(CurrentTile).GetComponent<TileView>().CheckAndShowAvailableTiles();

		while (true)
		{
			if (Singleton.BoardManager.SelectedTile)
			{
				CurrentTile = PhotonView.Get(Singleton.BoardManager.SelectedTile).ViewID;
				Singleton.BoardManager.SelectedTile = null;
				
				break;
			}

			if (Input.GetKeyDown("space"))
			{
				foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
					penguin.Controllable = true;

				break;
			}
			
			else yield return new WaitForEndOfFrame();
		}

		print("End Control Penguin Coroutine");
	}

	private void OnMouseDown()
	{
		//Check If It's The Player's Turn & Is On A Tile
		if (Controllable && transform.parent != null)
			StartCoroutine(ControlPenguin());
	}

	public void OnMouseEnter()
	{
		Highlight = Controllable ? true : false;
	}

	public void OnMouseExit()
	{
		Highlight = false;
	}
}
