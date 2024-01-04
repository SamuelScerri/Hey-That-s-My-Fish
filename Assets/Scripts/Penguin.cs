using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	public bool Controllable { get; set; }
	private int currentTile;

	[PunRPC]
	public void JumpToTile(int tileID)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (CurrentTile != 0)
				PhotonView.Find(CurrentTile).GetComponent<TileView>().CurrentState = TileView.State.Empty;
			PhotonView.Find(tileID).GetComponent<TileView>().CurrentState = TileView.State.Occupied;
		}

		currentTile = tileID;

		transform.SetParent(PhotonView.Find(currentTile).transform);
		transform.localPosition = Vector3.up * .1f;

		transform.localEulerAngles = Vector3.down * 90;
	}

	public int CurrentTile
	{
		get => currentTile;
		set => PhotonView.Get(this).RPC("JumpToTile", RpcTarget.All, value);
	}

	private IEnumerator ControlPenguin()
	{
		foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
			penguin.Controllable = false;

		print("Begin Control Penguin Coroutine");

		Singleton.BoardManager.SelectedTile = null;

		while (true)
		{
			if (Singleton.BoardManager.SelectedTile)
			{
				CurrentTile = PhotonView.Get(Singleton.BoardManager.SelectedTile).ViewID;
				Singleton.BoardManager.SelectedTile = null;

				PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) (Singleton.GameManager.CurrentPlayerID + 1), new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
				
				break;
			}

			else if (Input.GetKeyDown(KeyCode.Space))
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
}
