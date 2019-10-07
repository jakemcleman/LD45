using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private float groundFootstepTime;
    private float wallrunFootstepTime;
    private float groundFootstepTimeInterval = 0.4f;
    private float wallrunFootstepTimeInterval;

    private MovementController movement;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<MovementController>();
        movement.onMotionStateEvent.AddListener(MotionStateChange);

        wallrunFootstepTimeInterval = movement.wallRunBaseSpeed / 100.0f;

        groundFootstepTime = groundFootstepTimeInterval;
        wallrunFootstepTime = wallrunFootstepTimeInterval;
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
