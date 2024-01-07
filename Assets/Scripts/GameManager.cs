using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public byte CurrentPlayerID { get; set; }
	public List<Penguin> ClientPenguins { get; set; }
	public int[] Scores { get; set; }

	[SerializeField] private GameObject[] scoreHolders;
	[SerializeField] private GameObject gameOverScreen;

	[SerializeField] private GameObject scoreHolder1;
	[SerializeField] private GameObject scoreHolder2;
	[SerializeField] private GameObject cheatMenu;

	public GameObject[] ScoreHolders { get => scoreHolders; }
	public GameObject GameOverScreen { get => gameOverScreen; }

	private void Start()
	{
		ClientPenguins = new List<Penguin>();
		CurrentPlayerID = 0;
	}

	private float timePressed = 0.0f;
	public  float timeDelayThreshold = 1.0f;
 
	private void Update()
	{
		if (Input.touchCount > 0)
			CheckForCheatMenu(timeDelayThreshold);
	}
 
	void CheckForCheatMenu(float tim)
	{
		if (Input.GetTouch(0).phase == TouchPhase.Stationary)
		{
			timePressed += Time.deltaTime;
		}
 
  		if (Input.GetTouch(0).phase == TouchPhase.Ended)
		{
			if (timePressed > tim)
			{
				timePressed = 0;
				EnableCheatMenu();
			}
  		}
	}

	public void EnableCheatMenu()
	{
		cheatMenu.SetActive(true);
	}
 

	public void ShowGameOverScreen()
	{
		GameOverScreen.SetActive(true);

		int playerOneScore = 0;
		int playerTwoScore = 0;

		foreach(TileView obj in scoreHolder1.GetComponentsInChildren<TileView>())
			playerOneScore += obj.Amount;
		
		foreach(TileView obj in scoreHolder2.GetComponentsInChildren<TileView>())
			playerTwoScore += obj.Amount;
		
		if (playerOneScore > playerTwoScore)
			gameOverScreen.GetComponentInChildren<TextMeshProUGUI>().SetText(PhotonView.Get(scoreHolder1.GetComponentInChildren<TileView>()).Owner.NickName + " Won With: " + playerOneScore + " Fish!");
		else gameOverScreen.GetComponentInChildren<TextMeshProUGUI>().SetText(PhotonView.Get(scoreHolder2.GetComponentInChildren<TileView>()).Owner.NickName + " Won With: " + playerTwoScore + " Fish!");
	}

	public void SkipTurn()
	{
		cheatMenu.SetActive(false);
		PhotonNetwork.RaiseEvent(Singleton.SwitchPlayerEvent, (byte) (Singleton.GameManager.CurrentPlayerID + 1), new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
	}

	public void EnableAllTiles()
	{
		cheatMenu.SetActive(true);
		Singleton.BoardManager.EnableAllTiles();
	}
}
