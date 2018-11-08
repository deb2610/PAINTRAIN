using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {

    private float NumSeconds;
    private float timeRemaining;
    private float TimeRemaining {
        get
        {
            return timeRemaining;
        }
        set
        {
            timeRemaining = value;
            if (SevenSegmentDisplay != null)
            {
                SevenSegmentDisplay.SetDisplay((int)timeRemaining);
            }
        }
    }
    
    private bool IsRunning = false;
    private SevenSegment SevenSegmentDisplay;

    // Use this for initialization
	void Start () {
        SevenSegmentDisplay = gameObject.GetComponent<SevenSegment>();
	}
	
	// Update is called once per frame
	void Update () {
        if (IsRunning)
        {
            TimeRemaining = TimeRemaining - Time.deltaTime;
            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                IsRunning = false;
            }
        }
	}

    public void Set(float time)
    {
        NumSeconds = time;
        TimeRemaining = NumSeconds;
        IsRunning = false;
    }

    public void Reset()
    {
        IsRunning = false;
        TimeRemaining = NumSeconds;
    }

    public void Go()
    {
        IsRunning = true;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Stop()
    {
        IsRunning = false;
        TimeRemaining = 0;
    }

    public float GetTimeLeft()
    {
        return TimeRemaining;
    }

    public bool IsFinished()
    {
        return TimeRemaining <= 0;
    }
}
