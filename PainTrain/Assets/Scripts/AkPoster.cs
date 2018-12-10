using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkPoster : MonoBehaviour { 

	// Use this for initialization
	void Start () {
        RTPCs = new Dictionary<string, float>();
        PostAkEvent("Title_Start");
    }

    public Dictionary<string, float> RTPCs;

    void FixedUpdate()
    {
        foreach(KeyValuePair<string, float> RTPC in RTPCs)
        {
            AkSoundEngine.SetRTPCValue(RTPC.Key, RTPC.Value);
        }
    }

    public void PostAkEvent(string name)
    {
        AkSoundEngine.PostEvent(name, gameObject);
    }

    public void SetRTPC(string name, float val)
    {
        if (!RTPCs.ContainsKey(name)) RTPCs.Add(name, val);
        else RTPCs[name] = val;
    }
}
