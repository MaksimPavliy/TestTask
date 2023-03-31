using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	public bool isPlaying;

	private void Start()
	{
		if (instance == null)
		{ 
			instance = this; 
		}
		else if (instance == this)
		{ 
			Destroy(gameObject); 
		}

		isPlaying = true;
	}

	public void WinGame()
    {
		UIManager.instance.ShowWinScreen();
    }

	public void LoseGame()
    {
		UIManager.instance.ShowLoseScreen();
    }

	public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
