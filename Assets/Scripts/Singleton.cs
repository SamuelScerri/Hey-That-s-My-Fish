using UnityEngine;

public class Singleton : MonoBehaviour
{
	public const byte ChooseAreaEvent = 1;
	public const byte ReadyEvent = 2;
	public const byte TileUpdateStateEvent = 3;
	public const byte SwitchPlayerEvent = 4;
	public const byte SetTileIndex = 5;

	public static Singleton Reference {get; set;}
	public static GameManager GameManager { get; set; }
	public static BoardManager BoardManager { get; set; }

	private void Awake()
	{
		if (Reference == null)
		{
			GameManager = GetComponent<GameManager>();
			BoardManager = GetComponent<BoardManager>();
		}
			
		else
			Destroy(this.gameObject);
	}
}