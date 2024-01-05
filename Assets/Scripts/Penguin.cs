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

		if (CurrentTile != 0)
			StartCoroutine(PhotonView.Find(CurrentTile).GetComponent<TileView>().MoveToScoreHolder((byte) PhotonView.Get(this).OwnerActorNr));

		if (PhotonNetwork.IsMasterClient)
			PhotonView.Find(tileID).RPC("SetState", RpcTarget.All, TileView.State.Occupied);

		currentTile = tileID;
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

		PhotonView.Find(CurrentTile).GetComponent<TileView>().ShowAvailableTiles();

		while (true)
		{
			if (Singleton.BoardManager.SelectedTile)
			{
				CurrentTile = PhotonView.Get(Singleton.BoardManager.SelectedTile).ViewID;
				Singleton.BoardManager.SelectedTile = null;

				PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) (Singleton.GameManager.CurrentPlayerID + 1), new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
				
				break;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
					penguin.Controllable = true;

				break;
			}
			
			else yield return new WaitForEndOfFrame();
		}

		Singleton.BoardManager.ResetTiles();
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
