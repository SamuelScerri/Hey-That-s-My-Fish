using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BoardManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public const byte ChooseAreaEvent = 1;

	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private Vector2 tileSize, tileMargin;
	[SerializeField] private float cameraOffset;

	[SerializeField] private Sprite[] tileSprites;

	public static BoardManager Board { get; set; }
	public Sprite[] TileSprites { get => tileSprites; }



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
			yield return new WaitForEndOfFrame();
		}

		yield return null;
	}

	public void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == ChooseAreaEvent)
		{
			print("Please Choose Tiles");
			StartCoroutine(ChooseAreaCoroutine());
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
