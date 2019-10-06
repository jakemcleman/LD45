using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private float footstepTime;
    private float footstepTimeInterval = 0.4f;

    private MovementController movement;

    // Start is called before the first frame update
    void Start()
    {
        footstepTime = 0.4f;
        movement = GetComponent<MovementController>();
        movement.onMotionStateEvent.AddListener(Jump);
    }

    // Update is called once per frame
    void Update()
    {
        MovementAudio();
    }
    private void MovementAudio()
    {
        switch (movement.CurrentMotionState)
        {
            case MotionState.Running:
                if (footstepTime > 0)
                    footstepTime -= Time.deltaTime;
                else
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Run", this.transform.position);
                    footstepTime = footstepTimeInterval;
                }
                break;
        }
    }

    private void Jump(MotionStateEvent eMotion)
    {
        if (eMotion.nextState == MotionState.Jump)
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Jump", this.transform.position);
    }
}
