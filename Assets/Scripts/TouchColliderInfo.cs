using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

public class TouchColliderInfo : MonoBehaviour
{
    public TouchFinger _touchFinger;
    public TouchFinger touchFinger {
        get { return _touchFinger; }
        set { _touchFinger = value; }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
     void Update()
    {
        
    }
}


public class TouchFinger
{
    private Handedness _handedness;
    public Handedness handedness {
        get { return _handedness; }
        set { _handedness = value; }
    }

    private HandFinger _handFinger;
    public HandFinger handFinger {
        get { return _handFinger; }
        set { _handFinger = value; }
    }

    public TouchFinger(Handedness handedness, HandFinger handFinger) {
        _handedness = handedness;
        _handFinger = handFinger;
    }
}