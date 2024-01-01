using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Tile
{
	public Tile(State state, byte amount)
	{
		CurrentState = state;
		Amount = amount;
	}

	public enum State
	{
		Empty,
		Occupied,
		Full,
	}

	public byte Amount { get; }
	public State CurrentState { get; set; }
}

public class BoardManager : MonoBehaviour
{
	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private Vector2 tileSize, tileMargin;
	[SerializeField] private float cameraOffset;

	private Tile[,] tiles;

	private void Start()
	{
		tiles = new Tile[boardSize.x, boardSize.y];

		Camera.main.transform.position = new Vector3(boardSize.x * (tileSize.x + tileMargin.x) / 2 - cameraOffset, Camera.main.transform.position.y, boardSize.y * (tileSize.y + tileMargin.y) / 2);

		if (PhotonNetwork.IsMasterClient)
			StartCoroutine(InitializeBoard());
	}

	private IEnumerator InitializeBoard()
	{
		for (byte x = 0; x < boardSize.x; x++)
			for (byte y = (byte)(x % 2 == 0 ? 0 : 1); y < boardSize.y; y++)
			{
				PhotonNetwork.Instantiate("Tile", new Vector3(x * (tileSize.x + tileMargin.x), 0, y * (tileSize.y + tileMargin.y) + (x % 2 == 0 ? (tileSize.y + tileMargin.y) / 2 : 0)), Quaternion.identity);
				yield return new WaitForSeconds(.03125f);
			}
	}
}
