using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public byte CurrentPlayerID { get; set; }
	public List<Penguin> ClientPenguins { get; set; }
	public int[] Scores { get; set; }

	[SerializeField] private GameObject[] scoreHolders;
	public GameObject[] ScoreHolders { get => scoreHolders; }

	private void Start()
	{
		ClientPenguins = new List<Penguin>();
		CurrentPlayerID = 0;
	}
}
