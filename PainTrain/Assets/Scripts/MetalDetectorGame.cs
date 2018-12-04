using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetalDetectorGame : MonoBehaviour {


    private Gyroscope Gyro;
    private Vector3 lookingAt;
    public float tolerance = 10.0f; // Degrees from the goal
    public float steps = 10.0f;

    public GameObject timerGameObject;
    private Timer timer;

    public Button startButton;
    public GameObject instructionCanvas;

    public GameObject minigameLauncherObject;
    private MinigameLauncher minigameLauncher;

    public GameObject redLight;
    public GameObject greenLight;
    public GameObject offLight;
    public GameObject yellowLight;

    public GameObject redCross;
    public GameObject yellowCross;
    public GameObject yellowCircle;
    public GameObject greenCircle;

    public GameObject cameraSprite;
    public GameObject crosshairSprite;

    public int gameTime = 30;

    private Vector3 goal;
    private float finished = -1.0f;
    private bool started = false;
    private WebCamTexture backCam = null;

    // Use this for initialization
    void Start ()
    {
        startButton.onClick.AddListener(StartGame);
        minigameLauncher = minigameLauncherObject.GetComponent<MinigameLauncher>();

        // Check hardware 
        Gyro = Input.gyro;
        Gyro.enabled = true;
        lookingAt = Vector3.forward;

        WebCamDevice[] webcams = WebCamTexture.devices;
        Debug.Log(webcams.Length);

        if (webcams.Length > 0)
        {
            backCam = new WebCamTexture(webcams[0].name);
        }

        if (backCam != null)
        {
            Debug.Log(backCam.videoVerticallyMirrored);
            backCam.Play();
        }

        cameraSprite.GetComponent<Renderer>().material.mainTexture = backCam;
        

        timer = timerGameObject.GetComponent<Timer>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!started)
        {
            return;
        }
        if (finished < 0)
        {
            CheckInput();
            CheckWin();
            CheckFail();
        } else
        {
            if (Time.time - finished > 3)
            {
                minigameLauncher.ShowHomeButton();
            }
        }
	}

    void CheckInput()
    {
        lookingAt = Quaternion.Euler(Gyro.rotationRateUnbiased.x, Gyro.rotationRateUnbiased.y, 0) * lookingAt;
    }

    void CheckWin()
    {
        float angle = Mathf.Abs(Vector3.Angle(goal, lookingAt));

        if (angle <= tolerance)
        {
            finished = Time.time;
            Vibrate(1.0f);
            timer.Pause();
            SetLight(LightColor.Green);
            if (backCam != null)
            {
                backCam.Pause();
            }
        }

        int numSteps = (int)Mathf.Floor(angle / tolerance);
        UpdateCrosshairs(numSteps);
        VibrationPattern(numSteps);
        BlinkPattern(numSteps);
    }

    void CheckFail()
    {
        if (timer.IsFinished())
        {
            finished = Time.time;
            SetLight(LightColor.Red);
            timer.Stop();
        }
    }

    void StartGame()
    {
        instructionCanvas.SetActive(false);
        finished = -1.0f;
        goal = Random.onUnitSphere;
        timer.Set(gameTime);
        timer.Go();
        started = true;
    }

    private float nextVibration = -1;
    private int vibrateCount = 0;
    void VibrationPattern(int steps)
    {
        // four beats per second, 8th of a second per vibration
        float frameTime = Time.time;
        if (steps == 1)
        {
            if (nextVibration < frameTime)
            {
                Vibrate(0.125f);
                nextVibration = frameTime + 0.25f;
            }
        }
        else if (steps == 2)
        {
            // Two beats per second, 4th of a second per vibration
            if (nextVibration < frameTime)
            {
                Vibrate(0.25f);
                nextVibration = frameTime + 0.5f;
            }
        }
        else if (steps == 3)
        {
            if (nextVibration < frameTime)
            {
                Vibrate(0.125f);
                if (vibrateCount == 0)
                {
                    vibrateCount = 1;
                    nextVibration = frameTime + 0.25f;
                } else
                {
                    vibrateCount = 0;
                    nextVibration = frameTime + 0.75f;
                }
            }
        }
        else
        {
            nextVibration = -1;
        }
    }

    void UpdateCrosshairs(int steps)
    {
        if (steps == 0)
        {
            redCross.SetActive(false);
            yellowCross.SetActive(false);
            yellowCircle.SetActive(false);
            greenCircle.SetActive(true);
        }
        else if (steps == 1)
        {
            redCross.SetActive(false);
            yellowCross.SetActive(false);
            yellowCircle.SetActive(true);
            greenCircle.SetActive(false);
        }
        else if (steps == 2)
        {
            redCross.SetActive(false);
            yellowCross.SetActive(true);
            yellowCircle.SetActive(false);
            greenCircle.SetActive(false);
        }
        else
        {
            redCross.SetActive(true);
            yellowCross.SetActive(false);
            yellowCircle.SetActive(false);
            greenCircle.SetActive(false);
        }
    }

    void BlinkPattern(int steps)
    {
        float frameTime = Time.time;
        float milliseconds = frameTime - Mathf.Floor(frameTime);
        if (steps == 1)
        {
            if (
                    milliseconds < 0.125f
                || (milliseconds > 0.250f && milliseconds < 0.375f)
                || (milliseconds > 0.500f && milliseconds < 0.625f)
                || (milliseconds > 0.750f && milliseconds < 0.875f)
                )
            {
                SetLight(LightColor.Yellow);
            } else
            {
                SetLight(LightColor.Off);
            }
        }
        if (steps == 2)
        {
            if (
                    milliseconds < 0.125f
                || (milliseconds > 0.500f && milliseconds < 0.625f)
                )
            {
                SetLight(LightColor.Yellow);
            }
            else
            {
                SetLight(LightColor.Off);
            }
        }
        if (steps == 3)
        {
            if (milliseconds <= .125)
            {
                SetLight(LightColor.Yellow);
            }
            else
            {
                SetLight(LightColor.Off);
            }
        }
    }

    enum LightColor
    {
        Off,
        Red,
        Yellow,
        Green
    }

    // Set light
    void SetLight(LightColor color)
    {
        switch (color)
        {
            case LightColor.Red:
                offLight.SetActive(false);
                redLight.SetActive(true);
                greenLight.SetActive(false);
                yellowLight.SetActive(false);
                break;
            case LightColor.Green:
                offLight.SetActive(false);
                redLight.SetActive(false);
                greenLight.SetActive(true);
                yellowLight.SetActive(false);
                break;
            case LightColor.Yellow:
                offLight.SetActive(false);
                redLight.SetActive(false);
                greenLight.SetActive(false);
                yellowLight.SetActive(true);
                break;
            case LightColor.Off:
            default:
                offLight.SetActive(true);
                redLight.SetActive(false);
                greenLight.SetActive(false);
                yellowLight.SetActive(false);
                break;
        }
    }

    // Use this method to set active the game objects that need to be set active but are shared across minigames
    void OnEnable()
    {
        timerGameObject.SetActive(true);
        timer = timerGameObject.GetComponent<Timer>();
        instructionCanvas.SetActive(true);
        transform.eulerAngles = Vector3.zero;
        started = false;
        SetLight(LightColor.Off);
    }

    void OnDisable()
    {
        if (timerGameObject != null)
            timerGameObject.SetActive(false);
    }

    // Unity by default doesn't do this, so we're gonna do it ourselves
    void Vibrate(float duration)
    {
        Debug.Log("Buzz: " + duration);
        // This will only work on Android
        if (Application.platform != RuntimePlatform.Android) return;

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = unity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass vibratorClass = new AndroidJavaClass("android.os.Vibrator");
        AndroidJavaObject vibratorService = ca.Call<AndroidJavaObject>("getSystemService", "vibrator");

        vibratorService.Call("vibrate", (long)(duration * 1000));
        unity.Dispose();
        ca.Dispose();
        vibratorClass.Dispose();
        vibratorService.Dispose();
    }
}
