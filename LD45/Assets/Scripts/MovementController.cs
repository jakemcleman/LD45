using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MovementInput
{
    public Vector3 dirInput;
    public bool jumpInput;
    public bool quickTurnInput;
}

public class MovementController : MonoBehaviour
{
    #region Constants
    static readonly int inputQueueSize = 10;

    public float maxGroundSpeed;      //Horizontal
    public float maxAirSpeed;         //Horizontal

    public float maxGroundAcceleration = 250;

    [Tooltip("X = Speed initial speed (1 is maxGroundSpeed).\nY = Acceleration amount (1 = maxGroundAcceleration).")]
    public AnimationCurve groundAcceleration;
    public float maxAirAcceleration;

    [Tooltip("X = Speed initial speed  (1 is maxAirSpeed).\nY = Acceleration amount (1 = maxAirAcceleration).")]
    public AnimationCurve airAcceleration;
    public float groundDeceleration;
    public float airDeceleration;

    public float jumpSpeed;
    public int numJumps;

    public float sprintModifier;

    public float gravity;
    public float maxFallSpeed;

    public float normalHeight;
    public float crouchHeight;

    public float wallRunMaxTime = 2.0f;
    public float wallRunBaseSpeed = 30.0f;
    public float wallRunRegrabTime = 2.0f;
    public AnimationCurve wallRunVerticalDrift;
    public float wallRunVerticalDriftIntensity = 0.5f;

    public float wallClimbMaxTime = 2.0f;
    public float wallClimbSpeed = 10.0f;

    #endregion

    #region Internals
    private List<MovementInput> _inputQueue;
    private int _inputIndex = 0;
    private readonly float offset = 0.25f;

    [SerializeField]
    private MotionState _currMotionState;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _acceleration = Vector3.zero;
    private int _numJumpsRemaining = 2;
    private bool _uncapHorizontalSpeed;
    private float _speedSoftCap;
    private bool _transitionBlocked = false;
    private bool _grounded;

    private CharacterController _cc;
    private PlayerCameraController _camera;

    private Vector3 _motion;
    private float _speedModifier;
    private float _wallRunTimer;
    private float _wallRunSpeed;
    private float _wallClimbTimer;
    private Vector3 _dirToWall;
    private Vector3 _wallRunDir;
    private float _wallResetTimer;

    private float _currHeight;

    private Ray _ray;
    private RaycastHit _hitInfo;

    private Vector3 externalVelocity;


    [SerializeField]
    private bool _enableGravity = true;

    #endregion

    #region Accessors
    public MotionState CurrentMotionState { get => _currMotionState; }
    public bool Grounded
    {
        get => _grounded;
        private set
        {
            if (value == true)
            {
                _numJumpsRemaining = numJumps;
                _velocity.y = 0;
                _dirToWall = Vector3.zero;
                _wallResetTimer = wallRunRegrabTime;
                _speedSoftCap = 0;
            }
            _grounded = value;
        }
    }
    #endregion

    #region Events
    public OnMotionStateEvent onMotionStateEvent;

    public void ResetState()
    {
        ChangeMotionState(MotionState.Falling);
        _numJumpsRemaining = numJumps;
        _uncapHorizontalSpeed = false;
        _wallResetTimer = wallRunRegrabTime;
        _velocity = Vector3.zero;
    }

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
            if (CanWallTech(hit.normal))
            {
                Vector3 horizontalForward = RemoveUpDir(transform.forward);
                //Angle between normal and forward
                float angle = Vector3.Angle(-hit.normal, horizontalForward);
                if (angle < 30.0f)
                {
                    StartWallclimb(horizontalForward, hit.normal);
                }
                else
                {
                    StartWallrun(horizontalForward, hit.normal);
                }
            }
        }
    }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _camera = GetComponent<PlayerCameraController>();

        ResetState();

        _currHeight = normalHeight;

        _ray = new Ray(transform.position, -Vector3.up);

        _inputQueue = new List<MovementInput>(10);
        for (int i = 0; i < inputQueueSize; ++i)
        {
            _inputQueue.Add(new MovementInput());
        }
    }

    void CheckInput()
    {
        _inputIndex = (_inputIndex + 1) % inputQueueSize;
        //Get inputs
        MovementInput inputThisFrame;
        inputThisFrame.dirInput = Vector3.zero;
        inputThisFrame.dirInput.z += Input.GetAxis("Vertical");
        inputThisFrame.dirInput.x += Input.GetAxis("Horizontal");
        inputThisFrame.jumpInput = Input.GetButton("Jump");
        inputThisFrame.quickTurnInput = Input.GetKeyDown(KeyCode.Q);

        _inputQueue[_inputIndex] = inputThisFrame;
    }

    // Update is called once per frame 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            Debug.Log("DebugBreakPoint");
        CheckInput();
        _motion = _inputQueue[_inputIndex].dirInput.z * transform.forward;
        _motion += _inputQueue[_inputIndex].dirInput.x * transform.right;
        _motion.y = 0;
        if (_motion != Vector3.zero)
        {
            _motion.Normalize();
        }
        _velocity = _cc.velocity;
        _velocity -= externalVelocity;

        if (_inputQueue[_inputIndex].quickTurnInput)
            _camera.StartQuickTurn(Utility.Close(_dirToWall, Vector3.zero) ? transform.forward : _dirToWall);

        UpdateTimers();

        ApplyMotionStateModifiers();

        ApplyGravity();

        if (JumpIsTriggered() && !InWallTech()) Jump();
        ApplyAcceleration();

        StateUpdate();
        CapSpeed();
        //Apply Movement Speed Modifiers

        //Move. Then check collision flags and do collision resolution.
        CollisionFlags flags = _cc.Move((_velocity + externalVelocity)* Time.deltaTime);
    }

    void UpdateTimers()
    {
        if (!Grounded && !InWallTech())
        {
            _wallResetTimer -= Time.deltaTime;
            if (_wallResetTimer <= 0)
            {
                _dirToWall = Vector3.zero;
                _wallResetTimer = wallRunRegrabTime;
            }
        }
    }

    #region MovementUpdate
    void ApplyMotionStateModifiers()
    {
        switch(_currMotionState)
        {
            case MotionState.Wallrun:
            {
                _enableGravity = false;
                break;
            }
            case MotionState.Wallclimb:
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
        //If there is nothing below you.
        _ray.origin = transform.position;
        _ray.direction = -Vector3.up;
        if (!Physics.Raycast(_ray, out _hitInfo, (_cc.height * 0.5f) + offset))
        {
            Grounded = false;
            if (_velocity.y < 0)
            {
                ChangeMotionState(MotionState.Falling);
            }
        }
        // Don't become grounded when moving upwards
        else 
        {
            if (!Grounded && _velocity.y <= 0)
                Grounded = true;
            externalVelocity = _hitInfo.collider.GetComponent<IRidable>() != null ?
                _hitInfo.collider.GetComponent<IRidable>().GetVelocity() : Vector3.zero;
        }
        

        if ((!Grounded || InAirState()) && _enableGravity)
        {
            _velocity += (-gravity * Vector3.up) * Time.deltaTime;
        }
    }

    void Jump()
    {
        if (CanJump())
        {
            //Debug.Log("[Jump] Jump Called");
            _velocity.y = jumpSpeed;

            if (!Utility.Close(externalVelocity, Vector3.zero))
            {
                _velocity += externalVelocity;
                externalVelocity = Vector3.zero;
                _speedSoftCap = _velocity.magnitude;
            }

            --_numJumpsRemaining;

            Grounded = false;
            ChangeMotionState(MotionState.JumpStart);
        }
    }

    void ApplyAcceleration()
    {
        if (Utility.Close(_motion, Vector3.zero) && Utility.Close(_cc.velocity, Vector3.zero))
        {
            ChangeMotionState(MotionState.Idle);
        }

        Vector3 horizontalVelocity = RemoveUpDir(_velocity);

        //Movement buttons have been rayResults.
        if (!Utility.Close(_motion, Vector3.zero))
        {
            Vector3 forwardVelProjection = Vector3.Project(_velocity, transform.forward);
            Vector3 rest = horizontalVelocity - forwardVelProjection;

            //Moving on ground.
            if (Grounded)
            {
                ChangeMotionState(MotionState.Running);

                float speedRatio = _velocity.magnitude / maxGroundSpeed;
                float accelAmount = maxGroundAcceleration * groundAcceleration.Evaluate(speedRatio) + groundDeceleration;

                _velocity += -RemoveUpDir(_velocity.normalized) * groundDeceleration * Time.deltaTime;
                _velocity += _motion * accelAmount * Time.deltaTime;
                //Debug.Log("[ApplyAcceleration] GroundDeceleration");
            }
            else //In Air
            {
                float speedRatio = _velocity.magnitude / maxAirSpeed;
                float accelAmount = maxAirAcceleration * airAcceleration.Evaluate(speedRatio) + airDeceleration;
                
                _velocity += -RemoveUpDir(_velocity.normalized) * airDeceleration * Time.deltaTime;
                _velocity += _motion * accelAmount * Time.deltaTime;
            }
        }
        //Directional input is 0
        else
        {
            //Decelerate
            Vector3 velChange;
            if (Grounded)
            {
                velChange = -horizontalVelocity.normalized * groundDeceleration * Time.deltaTime;
                //Debug.Log("[ApplyAcceleration] NoInput Ground Deceleration");
            }
            else
            {
                velChange = -horizontalVelocity.normalized * airDeceleration * Time.deltaTime;
                //Debug.Log("[ApplyAcceleration] NoInput Air Deceleration");
            }

            Vector3 velDir = _velocity + velChange;
            if (Vector3.Dot(_velocity, velDir) <= 0)
            {
                _velocity.x = 0;
                _velocity.z = 0;
                return;
            }
            _velocity = velDir;
        }
        
    }

    void StateUpdate()
    {
        switch (_currMotionState)
        {
            case MotionState.Idle:
                if (!Utility.Close(_motion, Vector3.zero))
                    ChangeMotionState(MotionState.Running);
                break;
            case MotionState.Running:
                if (Utility.Close(_motion, Vector3.zero))
                    ChangeMotionState(MotionState.Running);
                break;
            case MotionState.Jump:
                if (Vector3.Dot(_velocity, Vector3.up) < 0)
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
            case MotionState.Wallclimb:
                WallclimbUpdate();
                break;
            default:
                _transitionBlocked = false;
                break;
        }
    }

    void CapSpeed()
    {
        Vector3 horizontalVelocity = RemoveUpDir(_velocity);
        float maxSpeed = InAirState() ? maxAirSpeed : maxGroundSpeed;
        float length = horizontalVelocity.magnitude;

        if (_speedSoftCap > 0)
        {
            if (length > _speedSoftCap)
            {
                horizontalVelocity /= length;
                horizontalVelocity *= _speedSoftCap;

                _velocity.x = horizontalVelocity.x;
                _velocity.z = horizontalVelocity.z;
            }
        }
        else if (!_uncapHorizontalSpeed)
        {
            if (length > maxSpeed)
            {
                horizontalVelocity /= length;
                horizontalVelocity *= maxSpeed;

                _velocity.x = horizontalVelocity.x;
                _velocity.z = horizontalVelocity.z;
            }
        }

        if (Math.Abs(_velocity.y) > maxFallSpeed && _velocity.y < 0)
        {
            _velocity.y = -maxFallSpeed;
        }
    }
    #endregion

    #region MotionStates
    void ChangeMotionState(MotionState nextMotionState)
    {
        if (!_transitionBlocked)
        {
            if (_currMotionState != nextMotionState)
            {
                MotionState prevState = _currMotionState;

                InvokeMotionStateEvent(_currMotionState, nextMotionState);

                _currMotionState = nextMotionState;

                MotionStateInitialize(prevState, nextMotionState);
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
                    _uncapHorizontalSpeed = false;
                    _enableGravity = true;
                    _wallResetTimer = wallRunRegrabTime;
                    break;
            }
            case MotionState.Wallclimb:
            {
                    _enableGravity = true;
                    _wallResetTimer = wallRunRegrabTime;
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
            case MotionState.JumpStart:
            {
                    ChangeMotionState(MotionState.Jump);
                    break;
            }
            case MotionState.WallrunStart:
            {
                    ChangeMotionState(MotionState.Wallrun);
                    break;
            }
            case MotionState.Wallrun:
            {
                    _wallRunTimer = wallRunMaxTime;
                    _transitionBlocked = true;
                    break;
            }
            case MotionState.WallclimbStart:
            {
                    ChangeMotionState(MotionState.Wallclimb);
                    break;
            }
            case MotionState.Wallclimb:
            {
                    _wallClimbTimer = wallClimbMaxTime;
                    _transitionBlocked = true;
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
        return _numJumpsRemaining > 0
            || _currMotionState == MotionState.Idle
            || _currMotionState == MotionState.Running;
    }
    
    bool CanWallTech(Vector3 normal)
    {
        if (!Grounded && !InWallTech())
        {
            if (Vector3.Angle(normal, Vector3.up) > _cc.slopeLimit)
            {
                return true;
            }
        }
        return false;
    }

    bool InAirState()
    {
        return _currMotionState == MotionState.Jump
            || _currMotionState == MotionState.Falling
            || _currMotionState == MotionState.JumpStart;
    }

    bool InGroundState()
    {
        return _currMotionState == MotionState.Idle
            || _currMotionState == MotionState.Running;
    }

    bool InWallTech()
    {
        return _currMotionState == MotionState.Wallrun
            || _currMotionState == MotionState.WallrunStart
            || _currMotionState == MotionState.Wallclimb
            || _currMotionState == MotionState.WallclimbStart;
    }
    #endregion

    #region MotionStateUpdates
    void WallrunUpdate()
    {
        _transitionBlocked = true;
        _wallRunTimer -= Time.deltaTime;
        if (_wallRunTimer >= 0.0f)
        {
            float raycastLength = 3 * _cc.radius;
            _ray.origin = transform.position;
            _ray.direction = _dirToWall;
            if (Physics.Raycast(_ray, out _hitInfo, raycastLength)) //If still there, keep running
            {
                //Check for grounded. If grounded, break wallrun.
                _ray.direction = -Vector3.up;
                if (Physics.Raycast(_ray, out _hitInfo, _cc.height * 0.5f + offset))
                {
                    _transitionBlocked = false;
                    //Debug.Log($"[Wallrun Update] Hit ground. {_dirToWall}");
                    Grounded = true;
                    ChangeMotionState(MotionState.Running);
                    return;
                }

                //Apply wallrun.
                Vector3 horizontalVelocity = RemoveUpDir(_velocity);
                _velocity = _wallRunDir * _wallRunSpeed;
                _uncapHorizontalSpeed = true;

                float t = 1 - (_wallRunTimer / wallRunMaxTime);
                float verticalSpeed = _velocity.magnitude * wallRunVerticalDriftIntensity;
                _velocity.y = verticalSpeed * wallRunVerticalDrift.Evaluate(t);
            }
            else //Else drop off
            {
                //Debug.Log($"[Wallrun Update] Fall off.");
                _transitionBlocked = false;
                ChangeMotionState(MotionState.Falling);
                return;
            }

            if (JumpIsTriggered())
            {
                //TODO: Make this not all happen at once, delay the triggered input and or other stuff.
                if (_wallRunTimer < wallRunMaxTime)
                {
                    //Debug.Log("[Wallrun Update] Jump");
                    _transitionBlocked = false;
                    _numJumpsRemaining++;
                    Jump();

                    float horizontalLaunch = RemoveUpDir(_velocity).magnitude;
                    _velocity = RemoveProjection(_velocity, _dirToWall) + (-_dirToWall * horizontalLaunch);
                }
            }
        }
        else
        {
            //Debug.Log("[Wallrun Update] Timeout");
            _transitionBlocked = false;
            ChangeMotionState(MotionState.Falling);
        }
    }
    
    void WallclimbUpdate()
    {
        _transitionBlocked = true;
        _wallClimbTimer -= Time.deltaTime;
        if (_wallClimbTimer >= 0.0f)
        {
            float raycastLength = 2 * _cc.radius;
            _ray.origin = transform.position;
            _ray.direction = _dirToWall;
            if (Physics.Raycast(_ray, out _hitInfo, raycastLength)) //If still there, keep climbing
            {
                if (_wallClimbTimer <= 1.0f)
                {
                    _velocity = Vector3.up * (wallClimbSpeed * _wallClimbTimer);
                }
                else
                {
                    _velocity = Vector3.up * wallClimbSpeed;
                }

                //Ledgegrab here?
            }
            else //Else drop off
            {
                //Debug.Log($"[Wallclimb Update] Fall off.");
                _dirToWall = Vector3.zero;
                _transitionBlocked = false;
                ChangeMotionState(MotionState.Falling);
                return;
            }

            if (JumpIsTriggered())
            {
                //TODO: Make this not all happen at once, delay the triggered input and or other stuff.
                if (_wallClimbTimer < wallClimbMaxTime - 0.25f)
                {
                    //Debug.Log("[Wallclimb Update] Jump");
                    _transitionBlocked = false;
                    _numJumpsRemaining++;
                    Jump();

                    Vector3 horizontalLaunch = -_dirToWall * maxAirSpeed;
                    _velocity += horizontalLaunch;
                }
            }
        }
        else
        {
            //Debug.Log("[Wallclimb Update] Timeout");
            _transitionBlocked = false;
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

    bool JumpIsTriggered()
    {
        if (_inputQueue[_inputIndex].jumpInput)
        {
            int index = _inputIndex - 1;
            if (index < 0)
            {
                index = inputQueueSize - 1;
            }
            index = index % inputQueueSize;
            return !_inputQueue[index].jumpInput;
        }
        return false;
    }

    void StartWallrun(Vector3 horizontalForward, Vector3 surfaceNormal)
    {
        //If the angle between the previous wall and the current wall is < 10 degree
        float angleBetween = Vector3.Angle(_dirToWall, Vector3.Project(_motion, surfaceNormal).normalized);
        if (angleBetween > 10.0f || _dirToWall == Vector3.zero)
        {
            _dirToWall = -surfaceNormal;
            Vector3 horizontalVelocity = RemoveUpDir(_velocity);
            _wallRunDir = (horizontalVelocity - Vector3.Project(horizontalVelocity, -surfaceNormal)).normalized;

            _wallRunSpeed = horizontalVelocity.magnitude;
            if (_wallRunSpeed < wallRunBaseSpeed)
            {
                _wallRunSpeed = wallRunBaseSpeed;
            }
            ChangeMotionState(MotionState.WallrunStart);
        }
    }

    void StartWallclimb(Vector3 horizontalForward, Vector3 surfaceNormal)
    {
        float angleBetween = Vector3.Angle(_dirToWall, Vector3.Project(_motion, surfaceNormal).normalized);
        if (angleBetween > 10.0f || _dirToWall == Vector3.zero)
        {
            _dirToWall = -surfaceNormal;

            ChangeMotionState(MotionState.WallclimbStart);
        }
    }

    void InvokeMotionStateEvent(MotionState prev, MotionState next)
    {
        MotionStateEvent motionStateEvent;
        motionStateEvent.prevState = prev;
        motionStateEvent.nextState = next;
        motionStateEvent.dirToWall = InWallTech() ? _dirToWall : Vector3.zero;

        if (onMotionStateEvent != null)
        {
            onMotionStateEvent.Invoke(motionStateEvent);
        }
    }
    #endregion
}
