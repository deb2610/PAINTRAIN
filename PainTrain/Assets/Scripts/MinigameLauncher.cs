using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameLauncher : MonoBehaviour {

    public Button safeCrackingButton;
    public GameObject safeGame;
    public GameObject appScreenCanvas;

	// Use this for initialization
	void Start () {
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
        appScreenCanvas.SetActive(false);
    }
}
