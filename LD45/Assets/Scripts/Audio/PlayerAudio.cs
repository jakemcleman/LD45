using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private float baseFootstepTime = 0.4f;

    private float groundFootstepTime;
    private float wallrunFootstepTime;
    private float wallclimbFootstepTime;

    private float groundFootstepTimeInterval;
    private float wallrunFootstepTimeInterval;
    private float wallclimbFootstepTimeInterval;

    private MovementController movement;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<MovementController>();
        movement.onMotionStateEvent.AddListener(MotionStateChange);

        groundFootstepTimeInterval = baseFootstepTime;
        wallrunFootstepTimeInterval = baseFootstepTime - (movement.wallRunBaseSpeed * 0.004f);
        wallclimbFootstepTimeInterval = baseFootstepTime - (movement.wallClimbSpeed * 0.01f);

        groundFootstepTime = groundFootstepTimeInterval;
        wallrunFootstepTime = wallrunFootstepTimeInterval;
        wallclimbFootstepTime = wallclimbFootstepTimeInterval;
    }

    // Update is called once per frame
    void Update()
    {
        FootstepAudio();
    }
    private void FootstepAudio()
    {
        switch (movement.CurrentMotionState)
        {
            case MotionState.Running:
                if (groundFootstepTime > 0)
                    groundFootstepTime -= Time.deltaTime;
                else
                {
                    PlayFootstep();
                    groundFootstepTime = groundFootstepTimeInterval;
                }
                break;
            case MotionState.Wallrun:
                if (wallrunFootstepTime > 0)
                    wallrunFootstepTime -= Time.deltaTime;
                else
                {
                    PlayFootstep();
                    wallrunFootstepTime = wallrunFootstepTimeInterval;
                }
                break;
            case MotionState.Wallclimb:
                if (wallclimbFootstepTime > 0)
                    wallclimbFootstepTime -= Time.deltaTime;
                else
                {
                    PlayFootstep();
                    wallclimbFootstepTime = wallclimbFootstepTimeInterval;
                }
                break;
        }
    }

    private void MotionStateChange(MotionStateEvent eMotion)
    {
        switch (eMotion.nextState)
        {
            case MotionState.JumpStart:
                PlayJump();
                break;
        }
            
    }

    private void PlayJump()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Jump", transform.position);
    }

    private void PlayFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Run", transform.position);
    }
}
