using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MotionState
{
    Idle,
    Running,
    Jump,
    Falling,
    Wallrun,
    Wallclimb,
    SlideStart,
    Slide,
    SlideEnd,

    NumStates,
}

public struct MotionStateEvent
{
    public MotionState nextState;
    public MotionState prevState;
    //Extra information here.
    //Entity doing the state change
    public Vector3 dirToWall;
}

[System.Serializable]
public class OnMotionStateEvent : UnityEvent<MotionStateEvent>
{
}
