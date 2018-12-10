using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameLauncher : MonoBehaviour {

    public Button safeCrackingButton;
    public Button hackingButton;
    public Button homeButton;
    public GameObject safeGame;
    public GameObject hackingGame;
    public GameObject appScreenCanvas;

	// Use this for initialization
	void Start () {
        homeButton.onClick.AddListener(GoHome);
        safeCrackingButton.onClick.AddListener(LaunchSafeGame);
        hackingButton.onClick.AddListener(LaunchHackingGame);
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
        GameObject.Find("AudioHandler").GetComponent<AkPoster>().PostAkEvent("Safe_Music");
        
    }
    void LaunchHackingGame()
    {
        hackingGame.SetActive(true);
        //SafeGame game = safeGame.transform.Find("Dial").gameObject.GetComponent<SafeGame>();
        //game.NewGame();
        HideAppScreen();
    }

    public void HideAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(false);
        hackingButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
        GameObject.Find("AudioHandler").GetComponent<AkPoster>().PostAkEvent("Title_Pause");
    }

    public void ShowAppScreen()
    {
        safeCrackingButton.gameObject.SetActive(true);
        hackingButton.gameObject.SetActive(true);
        homeButton.gameObject.SetActive(false);
        GameObject.Find("AudioHandler").GetComponent<AkPoster>().PostAkEvent("Title_Resume");
    }

    public void ShowHomeButton()
    {
        homeButton.gameObject.SetActive(true);
    }

    void GoHome()
    {
        safeGame.SetActive(false);
        hackingGame.SetActive(false);
        ShowAppScreen();
    }
}
