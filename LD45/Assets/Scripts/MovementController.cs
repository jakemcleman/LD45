using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MovementInput
{
    public Vector3 dirInput;
    public bool jumpInput;
}

public class MovementController : MonoBehaviour
{
    #region Constants
    static readonly int inputQueueSize = 10;

    public float maxGroundSpeed;      //Horizontal
    public float maxAirSpeed;         //Horizontal
    public float groundAcceleration;
    public float airAcceleration;
    public float groundDeceleration;
    public float airDeceleration;

    public float jumpSpeed;
    public int numJumps;

    public float sprintModifier;

    public float gravity;
    public float maxFallSpeed;

    public float normalHeight;
    public float crouchHeight;

    public float wallRunMaxTime;
    public float wallRunSpeed;
    public float wallRunRegrabTime;

    #endregion

    #region Internals
    private MovementInput[] inputQueue = new MovementInput[inputQueueSize];
    private int inputIndex = 0;

    [SerializeField]
    private MotionState currMotionState;
    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;
    private int numJumpsRemaining = 2;
    private bool uncapHorizontalSpeed;
    private bool transitionBlocked_ = false;
    private bool _grounded;

    private CharacterController _cc;
    private Vector3 motion;
    private float _speedModifier;
    private float _wallRunTimer;
    private Vector3 _dirToWall;
    private float _wallResetTimer;

    private Ray ray;
    private RaycastHit hitInfo;


    [SerializeField]
    private bool _enableGravity = true;

    #endregion

    #region Accessors
    public MotionState CurrentMotionState { get => currMotionState; }
    public bool Grounded
    {
        get => _grounded;
        private set
        {
            if (value == true)
            {
                numJumpsRemaining = numJumps;
                velocity.y = 0;
            }
            _grounded = value;
        }
    }
    #endregion

    #region Events
    public OnMotionStateEvent onMotionStateEvent;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("[Collision] Normal of coliding object: " + rayResults.normal);
        //Debug.Log("[Collision] Angle of incidence: " + Vector3.Angle(rayResults.normal, Vector3.up));
        if ((_cc.collisionFlags & CollisionFlags.Below) != 0)
        {
            //Debug.Log("[CollisionFloor]");
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle <= _cc.slopeLimit)
            {
                //Debug.Log("[CollisionFloor] Grounding player.");
                Grounded = true;

                if (Utility.Close(_cc.velocity, Vector3.zero))
                {
                    ChangeMotionState(MotionState.Idle);
                }
                else
                {
                    ChangeMotionState(MotionState.Running);
                }
            }
        }

        if ((_cc.collisionFlags & CollisionFlags.Sides) != 0)
        {
            if (inputQueue[inputIndex].jumpInput && CanWallTech())
            {
                _dirToWall = -hit.normal;
                ChangeMotionState(MotionState.Wallrun);
            }
        }
    }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        _cc = GetComponent<CharacterController>();
        currMotionState = MotionState.Falling;
        numJumpsRemaining = numJumps;
        uncapHorizontalSpeed = false;
        ray = new Ray(transform.position, -Vector3.up);
    }

    void CheckInput()
    {
        inputIndex = (inputIndex + 1) % inputQueueSize;
        //Get inputs
        MovementInput inputThisFrame;
        inputThisFrame.dirInput = Vector3.zero;
        inputThisFrame.dirInput.z += Input.GetAxis("Vertical");
        inputThisFrame.dirInput.x += Input.GetAxis("Horizontal");
        inputThisFrame.jumpInput = Input.GetButton("Jump");

        inputQueue[inputIndex] = inputThisFrame;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            Debug.Log("DebugBreakPoint");
        CheckInput();
        motion = inputQueue[inputIndex].dirInput.z * transform.forward;
        motion += inputQueue[inputIndex].dirInput.x * transform.right;
        motion.y = 0;
        if (motion != Vector3.zero)
        {
            motion.Normalize();
        }

        velocity = _cc.velocity;

        UpdateTimers();

        ApplyMotionStateModifiers();

        ApplyGravity();

        if (inputQueue[inputIndex].jumpInput) Jump();
        ApplyAcceleration();

        StateUpdate();
        CapSpeed();
        //Apply Movement Speed Modifiers

        //Move. Then check collision flags and do collision resolution.
        CollisionFlags flags = _cc.Move(velocity * Time.deltaTime);
    }

    void UpdateTimers()
    {
        
    }

    #region MovementUpdate
    void ApplyMotionStateModifiers()
    {
        switch(currMotionState)
        {
            case MotionState.Wallrun:
            {
                _enableGravity = false;
                break;
            }
            default:
                break;
        }
    }

    void ApplyGravity()
    {
        if ((!Grounded || InAirState()) && _enableGravity)
        {
            velocity += (-gravity * Vector3.up);
        }

        //If there is nothing below you.
        ray.origin = transform.position;
        ray.direction = -Vector3.up;
        if (!Physics.Raycast(ray, out hitInfo, _cc.height * 0.5f))
        {
            Grounded = false;
            if (velocity.y < 0)
            {
                ChangeMotionState(MotionState.Falling);
            }
        }
    }

    void Jump()
    {
        if (CanJump())
        {
            //Debug.Log("[Jump] Jump Called");
            velocity.y = jumpSpeed;
            --numJumpsRemaining;

            Grounded = false;
            ChangeMotionState(MotionState.Jump);
        }
    }

    void ApplyAcceleration()
    {
        if (Utility.Close(motion, Vector3.zero) && Utility.Close(_cc.velocity, Vector3.zero))
        {
            ChangeMotionState(MotionState.Idle);
        }

        Vector3 horizontalVelocity = RemoveUpDir(velocity);

        //Movement buttons have been rayResults.
        if (!Utility.Close(motion, Vector3.zero))
        {
            //Moving on ground.
            if (_cc.isGrounded)
            {
                ChangeMotionState(MotionState.Running);

                //Dot product motion and if it's negative, use ground decel instead.
                if (Vector3.Dot(horizontalVelocity, motion) > 0)
                {
                    velocity += motion * groundAcceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] GroundAcceleration");
                }
                else
                {
                    velocity += motion * groundDeceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] GroundDeceleration");
                }
            }
            else //In Air
            {
                if (Vector3.Dot(horizontalVelocity, motion) > 0)
                {
                    velocity += motion * airAcceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] AirAcceleration");
                }
                else
                {
                    velocity += motion * airDeceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] AirDeceleration");
                }
            }
        }
        //Directional input is 0
        else
        {
            //if (velocity.sqrMagnitude < 3)
            //{
            //    //Debug.Log("[ApplyAcceleration] Zeroing horizontal velocity.");
            //    velocity.x = 0;
            //    velocity.z = 0;
            //    return;
            //}

            //Decelerate
            Vector3 velChange;
            if (_cc.isGrounded)
            {
                velChange = -horizontalVelocity.normalized * groundDeceleration * Time.deltaTime;
                //Debug.Log("[ApplyAcceleration] NoInput Ground Deceleration");
            }
            else
            {
                velChange = -horizontalVelocity.normalized * airDeceleration * Time.deltaTime;
                //Debug.Log("[ApplyAcceleration] NoInput Air Deceleration");
            }

            Vector3 velDir = velocity + velChange;
            if (Vector3.Dot(velocity, velDir) <= 0)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }
            velocity = velDir;
        }
        
    }

    void StateUpdate()
    {
        switch (currMotionState)
        {
            case MotionState.Idle:
                if (!Utility.Close(motion, Vector3.zero))
                    ChangeMotionState(MotionState.Running);
                break;
            case MotionState.Running:
                if (Utility.Close(motion, Vector3.zero))
                    ChangeMotionState(MotionState.Running);
                break;
            case MotionState.Jump:
                if (Vector3.Dot(velocity, Vector3.up) < 0)
                {
                    ChangeMotionState(MotionState.Falling);
                }
                break;
            case MotionState.Falling:
                break;
            case MotionState.Wallrun:
                WallrunUpdate();
                //Wallrun Update
                break;
            default:
                transitionBlocked_ = false;
                break;
        }
    }

    void CapSpeed()
    {
        Vector3 horizontalVelocity = RemoveUpDir(velocity);
        float maxSpeed = InAirState() ? maxAirSpeed : maxGroundSpeed;
        float length = horizontalVelocity.magnitude;
        if (!uncapHorizontalSpeed)
        {
            if (length > maxSpeed)
            {
                horizontalVelocity /= length;
                horizontalVelocity *= maxSpeed;

                velocity.x = horizontalVelocity.x;
                velocity.z = horizontalVelocity.z;
            }
        }

        if (Math.Abs(velocity.y) > maxFallSpeed && velocity.y < 0)
        {
            velocity.y = -maxFallSpeed;
        }
    }
    #endregion

    #region MotionStates
    void ChangeMotionState(MotionState nextMotionState)
    {
        if (!transitionBlocked_)
        {
            if (currMotionState != nextMotionState)
            {
                MotionStateEvent motionStateEvent;
                motionStateEvent.prevState = currMotionState;
                motionStateEvent.nextState = nextMotionState;
                motionStateEvent.dirToWall = InWallTech() ? _dirToWall : Vector3.zero;

                if (onMotionStateEvent != null)
                {
                    onMotionStateEvent.Invoke(motionStateEvent);
                }

                MotionStateInitialize(currMotionState, nextMotionState);

                currMotionState = nextMotionState;
            }
        }
    }

    void MotionStateInitialize(MotionState prevState, MotionState nextState)
    {
        //Leaving
        switch(prevState)
        {
            case MotionState.Wallrun:
            {
                uncapHorizontalSpeed = false;
                _enableGravity = true;
                break;
            }
            default:
            {
                break;
            }
        }

        //Entering
        switch(nextState)
        {
            case MotionState.Wallrun:
            {
                _wallRunTimer = wallRunMaxTime;
                transitionBlocked_ = true;
                break;
            }
            default:
            {
                break;
            }
        }
    }
    #endregion

    #region MotionStateQueries
    bool CanJump()
    {
        return numJumpsRemaining > 0
            || currMotionState == MotionState.Idle
            || currMotionState == MotionState.Running;
    }
    
    bool CanWallTech()
    {
        return currMotionState == MotionState.Running
            || InAirState();
    }

    bool InAirState()
    {
        return currMotionState == MotionState.Jump
            || currMotionState == MotionState.Falling;
    }

    bool InGroundState()
    {
        return currMotionState == MotionState.Idle
            || currMotionState == MotionState.Running;
    }

    bool InWallTech()
    {
        return currMotionState == MotionState.Wallrun
            || currMotionState == MotionState.WallrunEnd;
    }
    #endregion

    #region MotionStateUpdates
    void WallrunUpdate()
    {
        transitionBlocked_ = true;
        _wallRunTimer -= Time.deltaTime;
        if (_wallRunTimer >= 0.0f)
        {
            float raycastLength = 2 * _cc.radius;
            ray.origin = transform.position;
            ray.direction = _dirToWall;
            if (Physics.Raycast(ray, out hitInfo, raycastLength)) //If still there, keep running
            {
                Vector3 velChange = wallRunSpeed * RemoveProjection(transform.forward, _dirToWall).normalized;
                velocity = velChange;
                uncapHorizontalSpeed = true;
            }
            else //Else drop off
            {
                //BreakWallRun();
                transitionBlocked_ = false;
                numJumpsRemaining++;
                Jump();
                //ChangeMotionState(MotionState.Falling);
                Debug.Log("[Wallrun Update] Fall off");
                return;
            }

            if (inputQueue[inputIndex].jumpInput)
            {
                //TODO: Make this not all happen at once, delay the triggered input and or other stuff.
                if (_wallRunTimer < wallRunMaxTime)
                {
                    //BreakWallRun();
                    transitionBlocked_ = false;
                    Debug.Log("[Wallrun Update] Jump");
                    numJumpsRemaining++;
                    Jump();
                }
            }
        }
        else
        {
            //BreakWallRun();
            transitionBlocked_ = false;
            Debug.Log("[Wallrun Update] Timeout");
            ChangeMotionState(MotionState.Falling);
        }
    }
    #endregion

    #region HelperFunctions
    Vector3 RemoveUpDir(Vector3 vec)
    {
        return vec - Vector3.Project(vec, Vector3.up);
    }

    Vector3 RemoveProjection(Vector3 vec, Vector3 remove)
    {
        return vec - Vector3.Project(vec, remove);
    }
    #endregion
}
