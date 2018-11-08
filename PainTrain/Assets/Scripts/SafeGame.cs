using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeGame : MonoBehaviour {

    public enum GameState
    {
        firstNumber,
        secondNumber,
        thirdNumber,
        failed,
        completed
    }

    public GameState CurrentState { get; private set; }
    public float AngleAwayFromCorrect { get; private set; }

    public GameObject redLight;
    public GameObject greenLight;
    public GameObject offLight;

    public int numDialTicks;
    public float sensitivity = 1.0f;
    public bool useMotionControls = false; // :P
    public bool useMouse = false;          // For testing on a computer
    public float gameTolerance = 5.0f;     // How many degrees off is allowed;
    public float buzzDuration = 0.1f;       // Seconds the phone should vibrate for
    public int tickTolerance = 3;

    private float CurrentGoal;

    private int[] SafeCombo;
    private Gyroscope Gyro;
    private Camera Camera;
    private Vector2 DialPosition;
    private int index = 0;
    private bool InputActive = true;

    // Use this for initialization
    void Start () {
        // Randomize the safe combo
        float tenPercent = numDialTicks / 10;
        SafeCombo = new int[3];
        SafeCombo[0] = mod((int)(Random.Range(tenPercent, numDialTicks - tenPercent)), numDialTicks);
        int nextOffset = (int)(Random.Range(tenPercent, numDialTicks - 2 * tenPercent));
        SafeCombo[1] = mod((SafeCombo[0] + nextOffset), numDialTicks);
        nextOffset = (int)(Random.Range(tenPercent, numDialTicks - 2 * tenPercent));
        SafeCombo[2] = mod((SafeCombo[1] - nextOffset), numDialTicks);
        Debug.Log("SafeCombo: [ " + SafeCombo[0] + ", " + SafeCombo[1] + ", " + SafeCombo[2] + " ]");

        // Check hardware 
        Gyro = Input.gyro;
        Gyro.enabled = true;

        Debug.Log("Start tick: " + SafeCombo[0]);

        CurrentGoal = TickToRotation(SafeCombo[0]);
        Debug.Log("Current Goal: " + CurrentGoal);
        CurrentState = GameState.firstNumber;
        Camera = Camera.main;
        DialPosition = Camera.WorldToScreenPoint(transform.position);
    }
	
	// Update is called once per frame
	void Update () {
        if (InputActive)
        {
            ProcessInput();
        }
        CheckState();
	}

    void ProcessInput()
    {
        // Grab the Gyro rotation
        // I'm disabling this and switching to touch countrols because it is not great
        if (useMotionControls)
        {
            Quaternion phoneRot = Gyro.attitude;
            transform.eulerAngles = new Vector3(0, 0, -90 - phoneRot.eulerAngles.z);
        }

        int touchCount = Input.touchCount;

        if (touchCount > 0)
        {
            // Lol, we're just going to assume that the first touch is the only touch for now
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Moved)
            {
                // Get the position
                Vector2 touchPos = touch.position;
                Vector2 touchVel = touch.deltaPosition;

                // Figure out just how much the swipe should affect the dial. Essentially,
                // the more circularly the person swipes, the more smoothly the dial will rotate
                // Check discord for a screenshot of the diagram I drew explaining this math

                Vector2 radius = DialPosition - touchPos;
                Vector2 tangent = new Vector2(radius.y, -radius.x);

                float projectionLength = Vector2.Dot(tangent, touchVel) / tangent.magnitude;
                float dialSpeed = sensitivity * projectionLength / touch.deltaTime;

                transform.Rotate(new Vector3(0, 0, dialSpeed));
            }
        }
    }
    
    void CheckState()
    {
        float currentRot = ClampAngle(transform.eulerAngles.z - 90);

        AngleAwayFromCorrect = Mathf.Abs(CurrentGoal - currentRot);
        
        // Check success
        if (GetCurrentTick() == SafeCombo[index])
        {
            Debug.Log("Buzz: " + GetCurrentTick());
            Vibrate(buzzDuration);
            CurrentGoal = TickToRotation(SafeCombo[mod(++index, 3)]);

            // Advance Game State
            if (index == 1)
            {
                CurrentState = GameState.secondNumber;
                Debug.Log("Current Goal: " + SafeCombo[index]);
            }
            if (index == 2)
            {
                CurrentState = GameState.thirdNumber;
                Debug.Log("Current Goal: " + SafeCombo[index]);
            }
            if (index > 2)
            {
                SucceedGame();
            }
        }

        // Check Fail
        if(CurrentState == GameState.firstNumber)
        {
            int tickFailure = SafeCombo[0] - 3 * tickTolerance;
            int currentTick = GetCurrentTick();
            if(currentTick > 10 && currentTick <= tickFailure)
            {
                Debug.Log("Failed at first stage");
                Debug.Log("Current Tick: " + currentTick);
                Debug.Log("tickFailure: " + tickFailure);
                Debug.Log("Goal: " + SafeCombo[0]);
                FailGame();
            }
        }
        else if(CurrentState == GameState.secondNumber)
        {
            int tickFailure = SafeCombo[0] - tickTolerance;
            int currentTick = GetCurrentTick();
            if(currentTick <= tickFailure && currentTick > SafeCombo[1])
            {
                Debug.Log("Failed at second stage");
                Debug.Log("Current Tick: " + currentTick);
                Debug.Log("tickFailure: " + tickFailure);
                Debug.Log("Goal: " + SafeCombo[1]);
                FailGame();
            }
        } 
        else if (CurrentState == GameState.thirdNumber)
        {
            int tickFailure = SafeCombo[1] + tickTolerance;
            int currentTick = GetCurrentTick();
            if (currentTick >= tickFailure && currentTick < SafeCombo[2])
            {
                Debug.Log("Failed at third stage");
                Debug.Log("Current Tick: " + currentTick);
                Debug.Log("tickFailure: " + tickFailure);
                Debug.Log("Goal: " + SafeCombo[2]);
                FailGame();
            }
        }
    }

    void FailGame()
    {
        offLight.SetActive(false);
        redLight.SetActive(true);
        CurrentState = GameState.failed;
        InputActive = false;
        Vibrate(1.0f);
    }

    void SucceedGame()
    {
        offLight.SetActive(false);
        greenLight.SetActive(true);
        CurrentState = GameState.completed;
        InputActive = false;
    }

    float TickToRotation(int tick)
    {
        return (float)tick / numDialTicks * 360 - 90;
    }

    float ClampAngle(float anyAngle)
    {
        return mod(anyAngle, 360);
    }

    // Unity by default doesn't do this, so we're gonna do it ourselves
    void Vibrate(float duration)
    {
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

    // A public method in case we want to display the ticks on the screen. 
    public int GetCurrentTick()
    {
        float currentRot = ClampAngle(transform.eulerAngles.z);
        int tick = (int)(currentRot / 360.0f * numDialTicks);
        return tick;
    }

    #region WTF C#
    int mod(int a, int b)
    {
        int m = a - (int)(b * Mathf.Floor((float)a / b));
        return m;
    }
    
    float mod(float a, float b)
    {
        return a - (b * Mathf.Floor(a / b));
    }
    #endregion
}
