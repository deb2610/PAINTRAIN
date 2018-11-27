using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackingGame : MonoBehaviour {
    public GameObject button1;
    public GameObject button2;

    public GameObject sevenSeg;
    private Timer Timer;
    public float gameTime = 30.0f;          // How many seconds the player has to unlock the safe

    public Text choice1;
    public Text choice2;
    public Text monitorText;
    public GameObject handheldText;

    public GameObject redLight;
    public GameObject greenLight;
    public GameObject offLight;

    public GameObject loserCanvas;
    public GameObject winnerCanvas;
    public GameObject instructionsCanvas;

    public int step;
    bool loser;
    bool winner;

    public GameObject minigameLauncherObject;
    private MinigameLauncher minigameLauncher;
    public GameObject menuCanvas;
    private float gameComplete = -1.0f;       // The time the player gets to the final tick
    public float gameEndDelay = 3.0f;       // Time between the game ending and the home button reappearing


    void OnEnable()
    {
        Restart();
        instructionsCanvas.SetActive(true);
        sevenSeg.SetActive(true);
        Timer = sevenSeg.GetComponent<Timer>();
        SpawnSteps();
    }
    // Use this for initialization
    void Start () {
        Timer = sevenSeg.GetComponent<Timer>();
        minigameLauncher = minigameLauncherObject.GetComponent<MinigameLauncher>();

        loser = false; 
        step = 0;
        Timer.Set(gameTime);
	}
	
	// Update is called once per frame
	void Update () {
        if (instructionsCanvas.activeInHierarchy)
        {
            Timer.Pause();
        }
        if (Timer.IsFinished() && winner ==false)
        {
            loser = true;
        }
        if (loser)
        {
            Lose();
            loser = true;
        }
        if (winner)
        {
            greenLight.SetActive(true);
            button1.SetActive(false);
            button2.SetActive(false);
            handheldText.SetActive(false);
            monitorText.gameObject.SetActive(false);
            winnerCanvas.SetActive(true);
            Timer.Stop();
            if (Time.time - gameEndDelay > gameComplete)
            {
                menuCanvas.SetActive(true);
                minigameLauncher.HideAppScreen();
                minigameLauncher.ShowHomeButton();
                sevenSeg.SetActive(false);
            }
        }
	}

    void SpawnSteps()
    {
        switch (step)
        {
            case 0:
                choice1.text = "Engage Quantum Harmonizer";
                choice2.text = "Bypass Employee Scanner";
                monitorText.text = "{Awaiting Instructions}... \n";
                break;
            case 1:
                choice1.text = "Decrypt Data Pipeline";
                choice2.text = "Disengage Primary Power";
                monitorText.text = "{Awaiting Instructions}... \n \n" +
                    "Employee Scanner Cracked! \n";
                break;
            case 2:
                choice1.text = "Disengage Quantum Harmonizer";
                choice2.text = "Reinforce Bitlevel Arrays";
                monitorText.text = "{Awaiting Instructions}... \n \n" +
                    "Employee Scanner Cracked! \n" +
                    "Pipeline Decrypted! \n";
                break;
            case 3:
                choice1.text = "Disable Photinic Resonator";
                choice2.text = "Activate Photonic Resonator";
                monitorText.text = "{Awaiting Instructions}... \n \n" +
                    "Employee Scanner Cracked! \n" +
                    "Pipeline Decrypted! \n" +
                    "Quantum Harmonizer Disengaged! \n";
                break;
            case 4:
                choice1.text = "PrintLine: 'I'm In!' ";
                choice2.text = "Engage Lock Again";
                monitorText.text = "{Awaiting Instructions}... \n \n" +
                    "Employee Scanner Cracked! \n" +
                    "Pipeline Decrypted! \n" +
                    "Quantum Harmonizer Disengaged! \n" +
                    "Photonics Resonating! ";

                break;
            default:
                break;
        }
    }
    void PickSomething(int choice)
    {
        switch (step)
        {
            case 0:// 2 is correct
                if (choice == 2)
                {
                    step = 1;
                }
                else
                {
                    loser = true;
                }
                break;
            case 1:// 1 is correct
                if (choice == 1)
                {
                    step = 2;
                }
                else
                {
                    loser = true;
                }
                break;
            case 2:// 1 is correct
                if (choice == 1)
                {
                    step = 3;
                }
                else
                {
                    loser = true;
                }
                break;
            case 3:// 2 is correct
                if (choice == 2)
                {
                    step = 4;
                }
                else
                {
                    loser = true;
                }
                break;
            case 4:// 1 is correct
                if (choice == 1)
                {
                    winner = true;
                    monitorText.text = "{Awaiting Instructions}... \n \n" +
                    "Employee Scanner Cracked! \n" +
                    "Choice 2 Correct \n" +
                    "Choice 3 Correct \n" +
                    "Choice 4 Correct \n" +
                    "\n I'M IN! \n";
                    gameComplete = Time.time;
                }
                else
                {
                    loser = true;
                }
                break;
            default:
                break;
        }
        SpawnSteps();
    }
    public void Choose1()
    {
        PickSomething(1);
    }

    public void Choose2()
    {
        PickSomething(2);
    }
    void Lose()
    {
        monitorText.gameObject.SetActive(false);
        redLight.SetActive(true);
        loserCanvas.SetActive(true);
        Timer.Stop();
    }
    public void Restart()
    {
        sevenSeg.SetActive(true);
        button1.SetActive(true);
        button2.SetActive(true);
        handheldText.SetActive(true);
        monitorText.gameObject.SetActive(true);
        loserCanvas.SetActive(false);
        winnerCanvas.SetActive(false);
        redLight.SetActive(false);
        greenLight.SetActive(false);
        loser = false;
        winner = false;
        step = 0;
        SpawnSteps();
        Timer.Set(gameTime);
        Timer.Go();
    }
    public void KillRules()
    {
        instructionsCanvas.SetActive(false);
        Timer.Go();
    }
}
