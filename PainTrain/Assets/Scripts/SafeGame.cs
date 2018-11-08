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

    public int numDialTicks;
    public float sensitivity = 1.0f;
    public bool useMotionControls = false; // :P
    public bool useMouse = false;          // For testing on a computer
    public float gameTolerance = 5.0f;     // How many degrees off is allowed;
    public float buzzDuration = 0.1f;       // Seconds the phone should vibrate for

    private float CurrentGoal;

    private int[] SafeCombo;
    private Gyroscope Gyro;
    private Camera Camera;
    private Vector2 DialPosition;
    private int index = 0;

    // Use this for initialization
    void Start () {
        // Randomize the safe combo
        SafeCombo = new int[3];
        SafeCombo[0] = (int)(Random.value * (numDialTicks - 15)) + 15;  // Let's keep the first number always past 15
        SafeCombo[1] = (int)(Random.value * numDialTicks);
        SafeCombo[2] = (int)(Random.value * numDialTicks);

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
        ProcessInput();
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
        float currentRot = clampAngle(transform.eulerAngles.z - 90);

        AngleAwayFromCorrect = Mathf.Abs(CurrentGoal - currentRot);
        
        if (AngleAwayFromCorrect < gameTolerance)
        {
            Debug.Log("Buzz: " + transform.eulerAngles.z);
            Vibrate(buzzDuration);
            CurrentGoal = TickToRotation(SafeCombo[++index % 3]);
        }
    }

    float TickToRotation(int tick)
    {
        return (float)tick / numDialTicks * 360 - 90;
    }

    float clampAngle(float anyAngle)
    {
        return anyAngle % 360;
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
}
