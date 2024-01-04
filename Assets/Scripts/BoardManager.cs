
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private Vector2 tileSize, tileMargin;
	[SerializeField] private float cameraOffset;

	[SerializeField] private Sprite[] tileSprites;

	[SerializeField] private TextMeshProUGUI stateDebugger;

	
	public Sprite[] TileSprites { get => tileSprites; }

	public TileView SelectedTile { get; set; }

	private void Start()
	{
		Camera.main.transform.parent.position = new Vector3(boardSize.x * (tileSize.x + tileMargin.x) / 2 - cameraOffset, Camera.main.transform.parent.position.y, boardSize.y * (tileSize.y + tileMargin.y) / 2);	
	}

	private IEnumerator InitializeBoard()
	{
		for (byte x = 0; x < boardSize.x; x++)
			for (byte y = (byte)(x % 2 == 0 ? 0 : 1); y < boardSize.y; y++)
			{
				TileView tile = PhotonNetwork.Instantiate("Tile", new Vector3(x * (tileSize.x + tileMargin.x), 0, y * (tileSize.y + tileMargin.y) + (x % 2 == 0 ? (tileSize.y + tileMargin.y) / 2 : 0)), Quaternion.identity).GetComponent<TileView>();
				tile.Amount = (byte)Random.Range(1, 4);
				tile.CurrentState = TileView.State.Full;

				yield return new WaitForSeconds(.015625f);
			}

		PhotonNetwork.RaiseEvent(Singleton.ChooseAreaEvent, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

		yield return null;
	}

	private void SpawnPenguin(TileView tile)
	{
		Penguin newPenguin;

		if (PhotonNetwork.IsMasterClient)
			newPenguin = PhotonNetwork.Instantiate("Penguin Blue", tile.transform.position, Quaternion.identity).GetComponent<Penguin>();
		else
			newPenguin = PhotonNetwork.Instantiate("Penguin Red", tile.transform.position, Quaternion.identity).GetComponent<Penguin>();

		//newPenguin.GetComponent<ParentView>().ParentID = PhotonView.Get(tile).ViewID;
		newPenguin.CurrentTile = PhotonView.Get(tile).ViewID;

		Singleton.GameManager.ClientPenguins.Add(newPenguin.GetComponent<Penguin>());
	}

	private IEnumerator ChooseAreaCoroutine()
	{
		byte penguinsLeft = 4;

		while (penguinsLeft > 0)
		{
			if (SelectedTile)
			{
				if (SelectedTile.Amount == 1)
				{
					print("Penguins Left: " + penguinsLeft);
					
					SpawnPenguin(SelectedTile);
					
					penguinsLeft --;
				}
				
				SelectedTile = null;
			}

			yield return new WaitForEndOfFrame();
		}

		PhotonNetwork.RaiseEvent(Singleton.ReadyEvent, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

		yield return null;
	}

	public void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == Singleton.ChooseAreaEvent)
		{
			print("Please Choose Tiles");
			stateDebugger.SetText("Set Penguin Pawns");

			StartCoroutine(ChooseAreaCoroutine());
		}

		if (photonEvent.Code == Singleton.ReadyEvent)
		{
			Singleton.GameManager.CurrentPlayerID ++;

			stateDebugger.SetText("Waiting For Other Player");

			if (Singleton.GameManager.CurrentPlayerID == PhotonNetwork.PlayerList.Length)
			{
				stateDebugger.SetText("Penguin Pawns Ready");
				PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) 1, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
			}
		}

		//if (photonEvent.Code == Singleton.TileUpdateStateEvent)
		//	if (PhotonNetwork.IsMasterClient)
		//		PhotonView.Find((int)photonEvent.CustomData).GetComponent<TileView>().CurrentState = TileView.State.Occupied;
			
	
		if (photonEvent.Code == Singleton.SwitchPlayerEvent)
		{
			if ((byte) photonEvent.CustomData > PhotonNetwork.PlayerList.Length)
				Singleton.GameManager.CurrentPlayerID = 1;
			else
				Singleton.GameManager.CurrentPlayerID = (byte) photonEvent.CustomData;

			stateDebugger.SetText("Player's Turn: " + Singleton.GameManager.CurrentPlayerID);

			if (Singleton.GameManager.CurrentPlayerID == PhotonNetwork.LocalPlayer.ActorNumber)
				foreach(Penguin penguin in Singleton.GameManager.ClientPenguins)
					penguin.Controllable = true;
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		print("Exiting!");
		Application.Quit();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
			StartCoroutine(InitializeBoard());
	}
}
