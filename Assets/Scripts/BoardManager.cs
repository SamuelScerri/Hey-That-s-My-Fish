using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BoardManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public const byte ChooseAreaEvent = 1;
	public const byte ReadyEvent = 2;
	public const byte TileUpdateStateEvent = 3;

	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private Vector2 tileSize, tileMargin;
	[SerializeField] private float cameraOffset;

	[SerializeField] private Sprite[] tileSprites;

	public static BoardManager Board { get; set; }
	public Sprite[] TileSprites { get => tileSprites; }

	public TileView SelectedTile { get; set; }

	private byte playersReady = 0;

	private void Awake()
	{
		if (Board == null)
			Board = this;
		else
			Destroy(this.gameObject);
	}

	private void Start()
	{
		Camera.main.transform.position = new Vector3(boardSize.x * (tileSize.x + tileMargin.x) / 2 - cameraOffset, Camera.main.transform.position.y, boardSize.y * (tileSize.y + tileMargin.y) / 2);	
	}

	private IEnumerator InitializeBoard()
	{
		for (byte x = 0; x < boardSize.x; x++)
			for (byte y = (byte)(x % 2 == 0 ? 0 : 1); y < boardSize.y; y++)
			{
				TileView tile = PhotonNetwork.Instantiate("Tile", new Vector3(x * (tileSize.x + tileMargin.x), 0, y * (tileSize.y + tileMargin.y) + (x % 2 == 0 ? (tileSize.y + tileMargin.y) / 2 : 0)), Quaternion.identity).GetComponent<TileView>();
				tile.Amount = (byte)Random.Range(1, 4);
				tile.CurrentState = TileView.State.Full;
			}

		PhotonNetwork.RaiseEvent(ChooseAreaEvent, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

		yield return null;
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
					PhotonNetwork.RaiseEvent(TileUpdateStateEvent, PhotonView.Get(SelectedTile).ViewID, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
					penguinsLeft --;
				}
				
				SelectedTile = null;
			}

			yield return new WaitForEndOfFrame();
		}

		PhotonNetwork.RaiseEvent(ReadyEvent, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

		yield return null;
	}

	public void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == ChooseAreaEvent)
		{
			print("Please Choose Tiles");
			StartCoroutine(ChooseAreaCoroutine());
		}

		if (photonEvent.Code == ReadyEvent)
		{
			playersReady ++;

			if (playersReady == 2)
				print("All Players Ready");
		}

		if (photonEvent.Code == TileUpdateStateEvent)
			PhotonView.Find((int)photonEvent.CustomData).GetComponent<TileView>().CurrentState = TileView.State.Occupied;
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
