using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TileView : MonoBehaviour
{
	private byte amount;
	private State currentState;
	private SpriteRenderer spriteRenderer;
	private Coroutine coroutine;

	public Vector2Int TileIndex { get; set; }

	public enum State
	{
		Active,
		Inactive,
		Occupied,
	}

	public byte Amount
	{
		get => amount;
		set
		{
			amount = value;
			spriteRenderer.sprite = Singleton.BoardManager.TileSprites[value];
		}
	}

	public bool Highlight
	{
		set
		{
			if (value)
			{
				GetComponent<MeshRenderer>().material.color = Color.green;
				spriteRenderer.color = Color.green;
			}

			else
			{
				GetComponent<MeshRenderer>().material.color = Color.white;
				spriteRenderer.color = Color.white;
			}
		}
	}

	[PunRPC]
	public void SetState(State state)
	{
		CurrentState = state;
	}

	[PunRPC]
	public void SetAmount(byte amount)
	{
		Amount = amount;
	}

	public State CurrentState
	{
		get => currentState;
		set
		{
			if (value == State.Active)
			{
				GetComponent<MeshRenderer>().material.color = Color.green;
				spriteRenderer.color = Color.green;
			}

			else
			{
				GetComponent<MeshRenderer>().material.color = Color.white;
				spriteRenderer.color = Color.white;
			}

			currentState = value;
		}
	}

	public IEnumerator FloatAnimation()
	{
		float bobbing = Random.Range(0, 255);

		while (true)
		{
			bobbing += Time.deltaTime;
			transform.position = new Vector3(transform.position.x, Mathf.Sin(bobbing) * .03125f, transform.position.z);
			transform.localEulerAngles = new Vector3(Mathf.Sin(bobbing) * 3.75f, 0,  -Mathf.Sin(bobbing * .5f) * 3.75f);

			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator MoveToScoreHolder(byte id)
	{
		StopCoroutine(coroutine);

		transform.SetParent(Singleton.GameManager.ScoreHolders[id - 1].transform);
		transform.localEulerAngles = Vector3.zero;

		Vector3 currentVelocity = Vector3.zero;

		int yOffset = Singleton.GameManager.ScoreHolders[id - 1].transform.childCount;

		while (true)
		{
			transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.up * yOffset * .125f, ref currentVelocity, .125f);
			yield return new WaitForEndOfFrame();
		}
	}

	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Start()
	{
		coroutine = StartCoroutine(FloatAnimation());
	}

	private bool CheckAndShowHorizontalTiles()
	{
		bool notStuck = false;

		//Check All Available Tiles On The Left
		for (int y = TileIndex.y + 1; y < Singleton.BoardManager.BoardSize.y; y++)
		{
			if (Singleton.BoardManager.TileIndexes[TileIndex.x, y].CurrentState == State.Inactive)
			{
				Singleton.BoardManager.TileIndexes[TileIndex.x, y].CurrentState = State.Active;
				notStuck = true;
			}

			else break;
		}

		//Check All Available Tiles On The Right
		for (int y = TileIndex.y - 1; y > (TileIndex.x % 2 == 0 ? -1 : 0); y--)
		{
			if (Singleton.BoardManager.TileIndexes[TileIndex.x, y].CurrentState == State.Inactive)
			{
				Singleton.BoardManager.TileIndexes[TileIndex.x, y].CurrentState = State.Active;
				notStuck = true;
			}

			else break;
		}

		return notStuck;
	}

	private bool CheckAndShowVerticalTiles()
	{
		bool notStuck = false;
		int offsetLeft = 0, offsetRight = 0;
		bool availableLeft = true, availableRight = true;

		//Check All Available Tiles On Top
		for (int x = TileIndex.x + 1; x < Singleton.BoardManager.BoardSize.x; x++)
		{
			if (x % 2 == 0)
				offsetRight ++;
			else offsetLeft ++;

			//Check All Left Diagonal Tiles
			if (TileIndex.y - offsetRight > (x % 2 == 0 ? -1 : 0) && availableLeft)
				if (Singleton.BoardManager.TileIndexes[x, TileIndex.y - offsetRight].CurrentState == State.Inactive)
				{
						Singleton.BoardManager.TileIndexes[x, TileIndex.y - offsetRight].CurrentState = State.Active;
						notStuck = true;
				}

				else availableLeft = false;
			
			//Check All Right Diagonal Tiles
			if (TileIndex.y + offsetLeft < Singleton.BoardManager.BoardSize.y && availableRight)
				if (Singleton.BoardManager.TileIndexes[x, TileIndex.y + offsetLeft].CurrentState == State.Inactive)
				{
					Singleton.BoardManager.TileIndexes[x, TileIndex.y + offsetLeft].CurrentState = State.Active;
					notStuck = true;
				}
				
				else availableRight = false;
		}

		availableLeft = true;
		availableRight = true;

		offsetLeft = 0;
		offsetRight = 0;

		//Check All Available Tiles On Bottom
		for (int x = TileIndex.x - 1; x > -1; x--)
		{
			if (x % 2 == 0)
				offsetRight ++;
			else offsetLeft ++;

			//Check All Left Diagonal Tiles
			if (TileIndex.y - offsetRight > (x % 2 == 0 ? -1 : 0) && availableLeft)
				if (Singleton.BoardManager.TileIndexes[x, TileIndex.y - offsetRight].CurrentState == State.Inactive)
				{
						Singleton.BoardManager.TileIndexes[x, TileIndex.y - offsetRight].CurrentState = State.Active;
						notStuck = true;
				}

				else availableLeft = false;
			
			//Check All Right Diagonal Tiles
			if (TileIndex.y + offsetLeft < Singleton.BoardManager.BoardSize.y && availableRight)
				if (Singleton.BoardManager.TileIndexes[x, TileIndex.y + offsetLeft].CurrentState == State.Inactive)
				{
					Singleton.BoardManager.TileIndexes[x, TileIndex.y + offsetLeft].CurrentState = State.Active;
					notStuck = true;
				}
				
				else availableRight = false;
		}

		return notStuck;
	}

	public bool CheckAndShowAvailableTiles()
	{
		bool foundHorizontal = CheckAndShowHorizontalTiles();
		bool foundVertical = CheckAndShowVerticalTiles();

		return foundHorizontal || foundVertical;
	}

	public void OnMouseDown()
	{
		if (CurrentState == State.Active)
			Singleton.BoardManager.SelectedTile = this;
	}

	public void OnMouseEnter()
	{
		if (CurrentState == State.Active)
		{
			GetComponent<MeshRenderer>().material.color = Color.red;
			spriteRenderer.color = Color.red;
		}
	}

	public void OnMouseExit()
	{
		if (CurrentState == State.Active)
			CurrentState = State.Active;
	}
}
