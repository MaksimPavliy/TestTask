using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas WinScreen;
    [SerializeField] private Canvas LoseScreen;

	public static UIManager instance = null;

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
	}

	public void ShowWinScreen()
    {
		Time.timeScale = 0;
		WinScreen.gameObject.SetActive(true);
    }

	public void ShowLoseScreen()
	{
		Time.timeScale = 0;
		LoseScreen.gameObject.SetActive(true);
	}

	public void HideWinScreen()
    {
		Time.timeScale = 1;
		WinScreen.gameObject.SetActive(false);
	}

	public void HideLoseScreen()
    {
		Time.timeScale = 1;
		LoseScreen.gameObject.SetActive(false);
	}

	public void Restart()
	{
		GameManager.instance.Restart();
		HideWinScreen();
		HideLoseScreen();
	}

}
