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

    FMOD.Studio.EventInstance footstep_event;

    int layerMask = 1 << 9;

    // Start is called before the first frame update
    void Start()
    {
        footstep_event = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Player/Player_Run");
        footstep_event.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));

        movement = GetComponent<MovementController>();
        movement.onMotionStateEvent.AddListener(MotionStateChange);

        groundFootstepTimeInterval = baseFootstepTime;
        wallrunFootstepTimeInterval = baseFootstepTime - (movement.wallRunBaseSpeed * 0.004f);
        wallclimbFootstepTimeInterval = baseFootstepTime - (movement.wallClimbSpeed * 0.01f);

        groundFootstepTime = groundFootstepTimeInterval;
        wallrunFootstepTime = wallrunFootstepTimeInterval;
        wallclimbFootstepTime = wallclimbFootstepTimeInterval;

        layerMask = ~layerMask;
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
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 3.0f, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        //Debug.Log(hit.collider.gameObject.tag);
                        SetSurfaceParameter(hit);
                    }
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
            case MotionState.Wallrun:
            case MotionState.WallrunStart:
            case MotionState.Wallclimb:
            case MotionState.WallclimbStart:
                RaycastHit hit;
                if (Physics.Raycast(transform.position, eMotion.dirToWall, out hit, 10.0f, layerMask, QueryTriggerInteraction.Ignore))
                {
                    SetSurfaceParameter(hit);
                }
                break;
        }
            
    }

    #region Audio Events
    private void PlayJump()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Jump", transform.position);
    }
    
    private void PlayFootstep()
    {
        footstep_event.start();
    }

    private void SetSurfaceParameter(RaycastHit hit)
    {
        AudioDefs.Surface surface = AudioDefs.Surface.None;
        switch (hit.collider.gameObject.tag)
        {
            case "Wood":
                surface = AudioDefs.Surface.Wood;
                break;
            case "Dirt":
                surface = AudioDefs.Surface.Dirt;
                break;
            case "Stone":
                surface = AudioDefs.Surface.Stone;
                break;
            case "Metal":
                surface = AudioDefs.Surface.Metal;
                break;
            case "Sandbag":
                surface = AudioDefs.Surface.Sandbag;
                break;
            default:
                surface = AudioDefs.Surface.None;
                break;
        }

        footstep_event.setParameterByName("Surface", (int)surface);
    }
    #endregion
}
