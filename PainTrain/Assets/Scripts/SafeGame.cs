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
    public bool useMotionControls = false;  // :P
    public bool useMouse = false;           // For testing on a computer
    public float gameTolerance = 5.0f;      // How many degrees off is allowed;
    public float buzzDuration = 0.1f;       // Seconds the phone should vibrate for success
    public float failBuzzDuration = 0.5f;   // Seconds the phone should vibrate for fail
    public int tickTolerance = 3;
    public float holdWinTime = 0.25f;       // A couple of milliseconds so that the player can't just win the last section by spinning really quickly
    public float penaltyTime = 2.0f;        // Seconds to penalize the player

    private float CurrentGoal;

    private int[] SafeCombo;
    private Gyroscope Gyro;
    private Camera Camera;
    private Vector2 DialPosition;
    private int index = 0;
    private bool InputActive = true;
    private float winReached = -1.0f;       // The time the player gets to the final tick
    private float timeOfLastFail = -10.0f;
    private float rotationAtFail;

    // Use this for initialization
    void Start () {
        // Randomize the safe combo
        float tenPercent = numDialTicks / 10;
        SafeCombo = new int[4];
        SafeCombo[0] = mod((int)(Random.Range(tenPercent, numDialTicks - tenPercent)), numDialTicks);
        int nextOffset = (int)(Random.Range(tenPercent, numDialTicks - 2 * tenPercent));
        SafeCombo[1] = mod((SafeCombo[0] + nextOffset), numDialTicks);
        nextOffset = (int)(Random.Range(tenPercent, numDialTicks - 2 * tenPercent));
        SafeCombo[2] = mod((SafeCombo[1] - nextOffset), numDialTicks);
        Debug.Log("SafeCombo: [ " + SafeCombo[0] + ", " + SafeCombo[1] + ", " + SafeCombo[2] + " ]");

        SafeCombo[3] = 2 * numDialTicks; // Makes it impossible while preventing index out of bounds errors

        // Check hardware 
        Gyro = Input.gyro;
        Gyro.enabled = true;

        //Debug.Log("Start tick: " + SafeCombo[0]);

        CurrentGoal = TickToRotation(SafeCombo[0]);
        //Debug.Log("Current Goal: " + SafeCombo[0]);
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
        if (!HandleFailedGameReset())
        {
            CheckState();
        }
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
        if (winReached > 0)
        {
            if (CurrentState != GameState.failed && Time.time - holdWinTime > winReached)
            {
                // Make sure the game hasn't been failed and that the time has been waited
                SucceedGame();
            }
        } else if (GetCurrentTick() == SafeCombo[index])
        {
            Debug.Log("Buzz: " + GetCurrentTick());
            Vibrate(buzzDuration);
            CurrentGoal = TickToRotation(SafeCombo[mod(++index, 3)]);

            // Advance Game State
            if (index == 1)
            {
                CurrentState = GameState.secondNumber;
                //Debug.Log("Current Goal: " + SafeCombo[index]);
            }
            if (index == 2)
            {
                CurrentState = GameState.thirdNumber;
                //Debug.Log("Current Goal: " + SafeCombo[index]);
            }
            if (index > 2)
            {
                if (winReached < 0)
                {
                    winReached = Time.time;
                }
            }
        }
        

        // Check Fail
        if (CurrentState == GameState.firstNumber)
        {
            int tickFailure = 3 * tickTolerance;     // In the first stage, let the player accidentally move left a little
            int tickGoalTol = mod(SafeCombo[0] - tickTolerance, numDialTicks);
            int currentTick = GetCurrentTick();
            if (!IsInRange(tickFailure, tickGoalTol, currentTick, true))
            {
                //Debug.Log("Failed at first stage");
                //Debug.Log("Goal: " + SafeCombo[0]);
                FailGame();
            }
        }
        else if(CurrentState == GameState.secondNumber)
        {
            int tickFailure = mod(SafeCombo[0] - tickTolerance, numDialTicks);
            int tickGoalTol = mod(SafeCombo[1] + tickTolerance, numDialTicks);
            int currentTick = GetCurrentTick();
            if (!IsInRange(tickFailure, tickGoalTol, currentTick, false))
            {
                //Debug.Log("Failed at second stage");
                //Debug.Log("Goal: " + SafeCombo[1]);
                FailGame();
            }
        } 
        else if (CurrentState == GameState.thirdNumber)
        {
            int tickFailure = mod(SafeCombo[1] + tickTolerance, numDialTicks);
            int overshootTick = mod(SafeCombo[2] - tickTolerance, numDialTicks);
            int currentTick = GetCurrentTick();

            if (!IsInRange(tickFailure, overshootTick, currentTick, true))
            {
                //Debug.Log("Failed at third stage");
                //Debug.Log("Goal: " + SafeCombo[2]);
                FailGame();
            }
        }
    }

    bool HandleFailedGameReset()
    {
        if (CurrentState == GameState.failed)
        {
            transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(rotationAtFail, 0, (Time.time - timeOfLastFail) / penaltyTime));
            if (Time.time - timeOfLastFail > penaltyTime)
            {
                Debug.Log("Penalty time complete");
                Reset();
                transform.eulerAngles = Vector3.zero;
                InputActive = true;
            }
            return true;
        }
        return false;
    }

    void FailGame()
    {
        timeOfLastFail = Time.time;
        rotationAtFail = transform.eulerAngles.z;
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

    bool IsInRange(int lastTick, int nextTick, int currentTick, bool clockwise)
    {
        bool res;
        // Get rid of having to move past 0
        if (clockwise)
        {
            // next tick should be "lower" than the last tick
            if (nextTick > lastTick)
            {
                // We have to cross zero to get here. 
                res = (currentTick < lastTick)     // current is between 0 and last tick
                    ^ (currentTick > nextTick);     // current is between numDialTicks and nextTick
            }
            else
            {
                // We don't have to cross zero
                res = (currentTick < lastTick)
                    && (currentTick > nextTick);
            }
        }
        else
        {
            // next tick should be "higher" than the last tick
            if (nextTick < lastTick)
            {
                // We have to cross zero to get here
                res = currentTick > lastTick       // current is between last and numDialTicks
                    ^  currentTick < nextTick;      // current is between 0 and nextTick
            }
            else
            {
                // We don't have to cross zero
                res = currentTick > lastTick
                    && currentTick < nextTick;
            }
        }
        //if (!res)
        //{
        //    Debug.Log("current: " + currentTick);
        //    Debug.Log("last: " + lastTick);
        //    Debug.Log("next: " + nextTick);
        //    Debug.Log(clockwise ? "clockwise" : "counterclockwise");
        //}
        return res;
    }

    public void Reset()
    {
        CurrentGoal = TickToRotation(SafeCombo[0]);
        CurrentState = GameState.firstNumber;
        offLight.SetActive(true);
        redLight.SetActive(false);
        greenLight.SetActive(false);
        index = 0;
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
