using System.Collections;
using System.Collections.Generic;
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

	public List<TileView> AllTiles { get; set; }

	private void Start()
	{
		AllTiles = new List<TileView>();
		Camera.main.transform.parent.position = new Vector3(boardSize.x * (tileSize.x + tileMargin.x) / 2 - cameraOffset, Camera.main.transform.parent.position.y, boardSize.y * (tileSize.y + tileMargin.y) / 2);	
	}

	private IEnumerator InitializeBoard()
	{
		for (byte x = 0; x < boardSize.x; x++)
			for (byte y = (byte)(x % 2 == 0 ? 0 : 1); y < boardSize.y; y++)
			{
				TileView tile = PhotonNetwork.Instantiate("Tile", new Vector3(x * (tileSize.x + tileMargin.x), 0, y * (tileSize.y + tileMargin.y) + (x % 2 == 0 ? (tileSize.y + tileMargin.y) / 2 : 0)), Quaternion.identity).GetComponent<TileView>();
				tile.Amount = (byte)Random.Range(1, 4);

				if (tile.Amount == 1)
					tile.CurrentState = TileView.State.Active;
				else tile.CurrentState = TileView.State.Inactive;

				AllTiles.Add(tile);

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
		else newPenguin = PhotonNetwork.Instantiate("Penguin Red", tile.transform.position, Quaternion.identity).GetComponent<Penguin>();

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
			stateDebugger.SetText("Set Penguin Pawns");
			StartCoroutine(ChooseAreaCoroutine());
		}

		if (photonEvent.Code == Singleton.ReadyEvent)
		{
			Singleton.GameManager.CurrentPlayerID ++;
			stateDebugger.SetText("Waiting For Other Player");

			if (Singleton.GameManager.CurrentPlayerID == PhotonNetwork.PlayerList.Length)
			{
				foreach(TileView tile in AllTiles)
					if (tile.CurrentState == TileView.State.Active)
						tile.CurrentState = TileView.State.Inactive;

				stateDebugger.SetText("Penguin Pawns Ready");
				Singleton.GameManager.Scores = new int[PhotonNetwork.PlayerList.Length];
				print("Allocated Score List With Capacity: " + Singleton.GameManager.Scores.Length);

				PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) 1, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
			}
		}
	
		if (photonEvent.Code == Singleton.SwitchPlayerEvent)
		{
			if ((byte) photonEvent.CustomData > PhotonNetwork.PlayerList.Length)
				Singleton.GameManager.CurrentPlayerID = 1;
			else Singleton.GameManager.CurrentPlayerID = (byte) photonEvent.CustomData;

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
