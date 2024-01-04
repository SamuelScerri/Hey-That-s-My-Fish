using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public byte CurrentPlayerID { get; set; }

	public static BoardManager Board { get; set; }
	public static GameManager Game { get; set; }

	public List<Penguin> ClientPenguins { get; set; }

	private void Start()
	{
		ClientPenguins = new List<Penguin>();
		CurrentPlayerID = 0;
	}
}
