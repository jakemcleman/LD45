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

    public float footstepTimeInterval = 0.5f;

    #endregion

    #region Internals
    private List<MovementInput> _inputQueue;
    private int _inputIndex = 0;

    [SerializeField]
    private MotionState _currMotionState;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _acceleration = Vector3.zero;
    private int _numJumpsRemaining = 2;
    private bool _uncapHorizontalSpeed;
    private bool _transitionBlocked = false;
    private bool _grounded;

    private CharacterController _cc;
    private PlayerCameraController _camera;
    private Vector3 _motion;
    private float _speedModifier;
    private float _wallRunTimer;
    private Vector3 _dirToWall;
    private float _wallResetTimer;
    private float _wallVelInterpTimer;

    private float _currHeight;

    private Ray _ray;
    private RaycastHit _hitInfo;

    private float footstepTime;


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
            if (CanWallTech(hit.normal))
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
        _camera = GetComponent<PlayerCameraController>();
        _currMotionState = MotionState.Falling;
        _numJumpsRemaining = numJumps;
        _uncapHorizontalSpeed = false;
        _wallVelInterpTimer = 0.0f;

        _currHeight = normalHeight;

        _ray = new Ray(transform.position, -Vector3.up);
        footstepTime = footstepTimeInterval;

        _inputQueue = new List<MovementInput>(10);
        for (int i = 0; i < 10; ++i)
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

        if (_inputQueue[_inputIndex].quickTurnInput)
            _camera.StartQuickTurn(Utility.Close(_dirToWall, Vector3.zero) ? transform.forward : _dirToWall);

        UpdateTimers();

        ApplyMotionStateModifiers();

        ApplyGravity();

        if (JumpIsTriggered()) Jump();
        ApplyAcceleration();

        StateUpdate();
        CapSpeed();
        //Apply Movement Speed Modifiers

        MovementAudio();

        //Move. Then check collision flags and do collision resolution.
        CollisionFlags flags = _cc.Move(_velocity * Time.deltaTime);
    }

    void UpdateTimers()
    {
        if (!Grounded)
        {
            _wallResetTimer -= Time.deltaTime;
            if (_wallResetTimer <= 0)
            {
                //Debug.Log("WallResetTimer");
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
            default:
                break;
        }
    }

    void ApplyGravity()
    {
        //If there is nothing below you.
        _ray.origin = transform.position;
        _ray.direction = -Vector3.up;
        if (!Physics.Raycast(_ray, out _hitInfo, _cc.height * 0.51f))
        {
            Grounded = false;
            if (_velocity.y < 0)
            {
                ChangeMotionState(MotionState.Falling);
            }
        }
        else
        {
            Grounded = true;
        }

        if ((!Grounded || InAirState()) && _enableGravity)
        {
            _velocity += (-gravity * Vector3.up);
        }
    }

    void Jump()
    {
        if (CanJump())
        {
            //Debug.Log("[Jump] Jump Called");
            _velocity.y = jumpSpeed;
            --_numJumpsRemaining;

            Grounded = false;
            ChangeMotionState(MotionState.Jump);
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
            //Moving on ground.
            if (Grounded)
            {
                ChangeMotionState(MotionState.Running);

                //Dot product motion and if it's negative, use ground decel instead.
                if (Vector3.Dot(horizontalVelocity, _motion) > 0)
                {
                    _velocity += _motion * groundAcceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] GroundAcceleration");
                }
                else
                {
                    _velocity += _motion * groundDeceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] GroundDeceleration");
                }
            }
            else //In Air
            {
                if (Vector3.Dot(horizontalVelocity, _motion) > 0)
                {
                    _velocity += _motion * airAcceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] AirAcceleration");
                }
                else
                {
                    _velocity += _motion * airDeceleration * Time.deltaTime;
                    //Debug.Log("[ApplyAcceleration] AirDeceleration");
                }
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
        if (!_uncapHorizontalSpeed)
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

    #region Audio
    private void MovementAudio()
    {
        switch (CurrentMotionState)
        {
            case MotionState.Running:
                float time = footstepTimeInterval;
                if (footstepTime > 0)
                    footstepTime -= Time.deltaTime;
                else
                {
                    Debug.Log("Step");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Player_Run", this.transform.position);
                    footstepTime = footstepTimeInterval;
                }
                break;
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
                MotionStateEvent motionStateEvent;
                motionStateEvent.prevState = _currMotionState;
                motionStateEvent.nextState = nextMotionState;
                motionStateEvent.dirToWall = InWallTech() ? _dirToWall : Vector3.zero;

                if (onMotionStateEvent != null)
                {
                    onMotionStateEvent.Invoke(motionStateEvent);
                }

                MotionStateInitialize(_currMotionState, nextMotionState);

                _currMotionState = nextMotionState;
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
                    _wallVelInterpTimer = 0;
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
            case MotionState.Wallrun:
            {
                _wallRunTimer = wallRunMaxTime;
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
        return InAirState()
            && Vector3.Angle(normal, Vector3.up) > _cc.slopeLimit
            && !Utility.Close(_dirToWall, -normal);
    }

    bool InAirState()
    {
        return _currMotionState == MotionState.Jump
            || _currMotionState == MotionState.Falling;
    }

    bool InGroundState()
    {
        return _currMotionState == MotionState.Idle
            || _currMotionState == MotionState.Running;
    }

    bool InWallTech()
    {
        return _currMotionState == MotionState.Wallrun
            || _currMotionState == MotionState.WallrunEnd;
    }
    #endregion

    #region MotionStateUpdates
    void WallrunUpdate()
    {
        _transitionBlocked = true;
        _wallRunTimer -= Time.deltaTime;
        if (_wallRunTimer >= 0.0f)
        {
            float raycastLength = 2 * _cc.radius;
            _ray.origin = transform.position;
            _ray.direction = _dirToWall;
            if (Physics.Raycast(_ray, out _hitInfo, raycastLength)) //If still there, keep running
            {
                //Check for grounded. If grounded, break wallrun.
                _ray.direction = -Vector3.up;
                if (Physics.Raycast(_ray, out _hitInfo, _cc.height * 0.51f))
                {
                    BreakWallrun();
                    Debug.Log($"[Wallrun Update] Hit ground. {_dirToWall}");
                    Grounded = true;
                    ChangeMotionState(MotionState.Running);
                    return;
                }

                //Apply wallrun.
                Vector3 velChange;

                //If not looking towards the wall
                if (Vector3.Dot(RemoveUpDir(transform.forward), _dirToWall) < 0
                    || Vector3.Angle(RemoveUpDir(transform.forward), _dirToWall) < 30)
                {
                    velChange = RemoveProjection(_velocity, _dirToWall);
                }
                else
                {
                    velChange = wallRunSpeed * RemoveProjection(transform.forward, _dirToWall).normalized;
                }

                float magnitude = velChange.magnitude;
                if (magnitude <= wallRunSpeed)
                {
                    velChange.y = wallRunSpeed - magnitude;
                    velChange.Normalize();
                    velChange *= wallRunSpeed;
                }
                
                _velocity = velChange;
                
                _uncapHorizontalSpeed = true;
            }
            else //Else drop off
            {
                BreakWallrun();
                //Debug.Log($"[Wallrun Update] Fall off.");
                ChangeMotionState(MotionState.Falling);
                return;
            }

            if (JumpIsTriggered())
            {
                //TODO: Make this not all happen at once, delay the triggered input and or other stuff.
                if (_wallRunTimer < wallRunMaxTime)
                {
                    BreakWallrun();
                    //Debug.Log("[Wallrun Update] Jump");
                    _numJumpsRemaining++;
                    Jump();

                    float horizontalLaunch = RemoveUpDir(_velocity).magnitude;
                    _velocity = RemoveProjection(_velocity, _dirToWall) + (-_dirToWall * horizontalLaunch);
                }
            }
        }
        else
        {
            BreakWallrun();
            //Debug.Log("[Wallrun Update] Timeout");
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

    void BreakWallrun()
    {
        _transitionBlocked = false;
        _numJumpsRemaining++;
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
    #endregion
}
