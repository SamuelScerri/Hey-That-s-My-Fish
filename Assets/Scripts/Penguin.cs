using System;
using System.Collections;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	public bool Controllable { get; set; }

	private IEnumerator ControlPenguin()
	{
		foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
			penguin.Controllable = false;

		print("Begin Control Penguin Coroutine");

		while (!Input.GetKeyDown(KeyCode.Space))
		{
			yield return new WaitForEndOfFrame();
		}

		print("Cancel Control Penguin Coroutine");

		foreach (Penguin penguin in Singleton.GameManager.ClientPenguins)
			penguin.Controllable = true;
	}

	private void OnMouseDown()
	{
		if (Controllable)
			StartCoroutine(ControlPenguin());
	}
}
