using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameLauncher : MonoBehaviour {

    public Button safeCrackingButton;
    public Button hackingButton;
    public Button metalDetectorButton;
    public Button homeButton;
    public GameObject safeGame;
    public GameObject hackingGame;
    public GameObject metalDetectorGame;
    public GameObject appScreenCanvas;

	// Use this for initialization
	void Start () {
        homeButton.onClick.AddListener(GoHome);
        safeCrackingButton.onClick.AddListener(LaunchSafeGame);
        hackingButton.onClick.AddListener(LaunchHackingGame);
        metalDetectorButton.onClick.AddListener(LaunchMetalDetector);
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
    void LaunchHackingGame()
    {
        hackingGame.SetActive(true);
        //SafeGame game = safeGame.transform.Find("Dial").gameObject.GetComponent<SafeGame>();
        //game.NewGame();
        HideAppScreen();
    }

    void LaunchMetalDetector()
    {
        metalDetectorGame.SetActive(true);
        HideAppScreen();
    }

    public void HideAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(false);
        hackingButton.gameObject.SetActive(false);
        metalDetectorButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
    }

    public void ShowAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(true);
        hackingButton.gameObject.SetActive(true);
        metalDetectorButton.gameObject.SetActive(true);
        homeButton.gameObject.SetActive(false);
    }

    public void ShowHomeButton()
    {
        homeButton.gameObject.SetActive(true);
    }

    void GoHome()
    {
        safeGame.SetActive(false);
        hackingGame.SetActive(false);
        metalDetectorGame.SetActive(false);
        ShowAppScreen();
    }
}
