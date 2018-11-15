using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameLauncher : MonoBehaviour {

    public Button safeCrackingButton;
    public Button homeButton;
    public GameObject safeGame;
    public GameObject appScreenCanvas;

	// Use this for initialization
	void Start () {
        homeButton.onClick.AddListener(GoHome);
        safeCrackingButton.onClick.AddListener(LaunchSafeGame);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LaunchSafeGame()
    {
        safeGame.SetActive(true);
        SafeGame game = safeGame.transform.Find("Dial").gameObject.GetComponent<SafeGame>();
        game.NewGame();
        HideAppScreen();
    }

    void HideAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
    }

    public void ShowAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(true);
        homeButton.gameObject.SetActive(false);
    }

    public void ShowHomeButton()
    {
        homeButton.gameObject.SetActive(true);
    }

    void GoHome()
    {
        safeGame.SetActive(false);
        ShowAppScreen();
    }
}
