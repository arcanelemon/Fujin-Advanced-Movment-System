using System.Collections;
using UnityEngine;
using UnityEditor;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


/// <summary>
/// Physics-based motor that controls player movement. Configured by accesing <code>FujinMotorModule</code>.
/// </summary>
namespace Fujin
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FujinPlayerMotor : MonoBehaviour
    {
        #region Variables

        #region Serialized

        #region Components

        // Modules
        public FujinInputModule input = new FujinInputModule();
        public FujinPhysicsModule physics = new FujinPhysicsModule();
        public FujinMovementModule movement = new FujinMovementModule();
        public FujinJumpModule jump = new FujinJumpModule();
        public FujinSprintModule sprint = new FujinSprintModule();
        public FujinCrouchModule crouch = new FujinCrouchModule();
        public FujinSlideModule slide = new FujinSlideModule();
        public FujinDashModule dash = new FujinDashModule();
        public FujinPoundModule pound = new FujinPoundModule();
        public FujinMantleModule mantle = new FujinMantleModule();
        public FujinClimbModule climb = new FujinClimbModule();
        public FujinWallrunModule wallrun = new FujinWallrunModule();
        public FujinHeadbobModule headbob = new FujinHeadbobModule();
        public FujinAutorunModule autorun = new FujinAutorunModule();
        public FujinLeanModule lean = new FujinLeanModule();
        public FujinSlowwalkModule slowwalk = new FujinSlowwalkModule();
        public FujinFreelookModule freelook = new FujinFreelookModule();
        public FujinGrappleModule grapple = new FujinGrappleModule();
        public FujinSpeedlinesModule speedlines = new FujinSpeedlinesModule();

        [SerializeField]
        Transform cameraContainer;

        #endregion Components

        #region Debug

        public Vector3 Velocity => previousVelocity;
        public bool Grounded => grounded;
        public float NormalAngle => normalAngle;
        public MovementState State => movementState;
        public bool drawGizmos;
        public float Speed => currSpeed;
        public int Jumps => numJumps;
        public int Dashes => numDashes;

        #endregion Debug

        #endregion Serialized

        #region Instance

        public enum MovementState
        {
            Default = 0, //-------|
            Sprinting = 1, //-----| 
            Sliding = 2, //-------|
            Crouching = 3, //-----|States that use gravity
            SlowWalk = 4, //------|
            Pounding = 5, //------|
            ThrowingGrapple = 6,//| 
            Dashing = 7,
            Mantling = 8,
            Climbing = 9,
            WallRunning = 10,
            Grappling = 11,
        }

        MovementState movementState = MovementState.Default;

        // Input Queue
        bool jumpInput;
        bool sprintInput;
        bool slideInput;
        bool autoRunInput;
        bool leanInput;
        bool freeLookInput;

        bool enable;
        bool grounded;
        bool onStep;
        bool onWall;
        bool cueStand;
        bool headspaceAvailable;
        bool controlFOV = true;
        bool controlHeight = true;
        bool canJump = false;
        bool canPound = true;
        bool returnGrapple;

        float d;
        float yRot;
        float xRot;
        float currSpeed;
        float normalAngle;
        float groundCheckRadius = 0.49f;
        float groundCheckDrawDistance = 0.02f;
        float collisionDrawOffset = 0.5f;
        float collisionThreshold = 0.01f;
        float heightOffset;
        float targetHeight;
        float defaultHeight;
        float heightSmoothing = 1;
        float FOV;
        float targetFOV;
        float defaultFOV;
        float FOVsmoothing = 5;
        float tiltRotation;
        float poundHeight;
        float leanDirection;
        float grappleDistance;

        int numJumps;
        int numDashes;

        Vector2 moveInputs;
        Vector2 lookOffset;
        Vector2 cameraOffset;
        Vector3 normal;
        Vector3 previousVelocity;
        Vector3 dashDirection;
        Vector3 poundOrigin;
        Vector3 grapplePoint;

        LayerMask collisionExclusionMask;
        Collider lastClimbCollider;
        ParticleSystem.EmissionModule speedLinesEmission;
        GameObject grappleObj;
        LineRenderer grapplelineRenderer;
        CapsuleCollider col;
        Camera cam;
        Rigidbody rb;

        // Editor
        public bool inputFoldout;
        public bool featuresFoldout;
        public bool morefeaturesFoldout;
        public bool configFoldout;
        public bool debugFoldout = true;
        public bool hideMouseOnAwake = true;

        #endregion Instance

        #endregion Variables

        #region Input

#if ENABLE_INPUT_SYSTEM
        #region New Input
        void OnLookChange(InputAction.CallbackContext context)
        {
            Look(context.ReadValue<Vector2>() * input.lookSensitivity);
        }

        void OnMoveChange(InputAction.CallbackContext context)
        {
            moveInputs = context.ReadValue<Vector2>();
        }

        void OnSprintPress(InputAction.CallbackContext context)
        {
            int state = (int)movementState;
            if (!sprint.enable || (moveInputs.y < 0.1f && !autoRunInput) || state > 4)
                return;

            if (sprint.hold) // hold sprint
            {
                if (grounded)
                {
                    sprintInput = true;

                    if (headspaceAvailable)
                        Sprint();
                    else
                        cueStand = true;
                }
            }
            else // toggle sprint
            {
                if (state == 1) // reset movement but must account for auto run
                {
                    sprintInput = false;

                    if (grounded)
                    {
                        bool autoRun = this.autoRunInput;
                        ResetMovement();
                        this.autoRunInput = autoRun;
                    }
                }
                else
                {
                    if (grounded)
                    {
                        sprintInput = true;

                        if (headspaceAvailable)
                            Sprint();
                        else
                            cueStand = true;
                    }
                }
            }
        }

        void OnSprintRelease(InputAction.CallbackContext context)
        {
            if (!sprint.enable || !sprint.hold)
                return;

            sprintInput = false;

            if (grounded && movementState == MovementState.Sprinting)
            {
                bool autoRun = this.autoRunInput;
                ResetMovement();
                this.autoRunInput = autoRun;
            }
        }

        void OnJumpPress(InputAction.CallbackContext context)
        {
            if (!jump.enable)
                return;

            if (grounded || (jump.enableWallJump && onWall) || numJumps > 0)
                jumpInput = true;
        }

        void OnCrouchPress(InputAction.CallbackContext context)
        {
            if (!crouch.enable && !slide.enable)
                return;

            if (!grounded)
            {
                slideInput = true;
                return;
            }

            if (!crouch.hold && (movementState == MovementState.Crouching || movementState == MovementState.Sliding))
                cueStand = true;
            else
            {
                if (crouch.enable && (!slide.enable || (slide.enable && movementState != MovementState.Sprinting)))
                {
                        Crouch();
                }
                else if (slide.enable)
                {
                    StartSlide();
                }
            }
        }

        void OnCrouchRelease(InputAction.CallbackContext context)
        {
            if ((crouch.enable && crouch.hold && movementState == MovementState.Crouching) || (slide.enable && slide.hold && movementState == MovementState.Sliding))
                cueStand = true;
        }

        void OnDashPress(InputAction.CallbackContext context)
        {
            if (!dash.enable || (!grounded && !dash.canDashInAir) || (int)movementState > 1 || numDashes == 0)
                return;

            StartDash();
        }

        void OnPoundPress(InputAction.CallbackContext context)
        {
            if (!pound.enable || grounded || !canPound)
                return;

            StartPound();
        }

        void OnAutoRunPress(InputAction.CallbackContext context)
        {
            int state = (int)movementState;
            if (!autorun.enable || !grounded || state > 4)
                return;

            autoRunInput = !autoRunInput;
        }

        void OnLeanPress(InputAction.CallbackContext context)
        {
            int state = (int)movementState;
            if (!lean.enable || !grounded || (state <= 4 && state == 1))
                return;

            Lean(context.ReadValue<float>());
        }

        void OnLeanRelease(InputAction.CallbackContext context)
        {
            int state = (int)movementState;
            if (!lean.enable || !lean.hold || state >= 4)
                return;

            StopLean();
        }

        void OnFreeLookPress(InputAction.CallbackContext context)
        {
            if (!freelook.enable)
                return;

            if (!freelook.hold && freeLookInput)
                freeLookInput = false;
            else
                freeLookInput = true;
        }

        void OnFreeLookRelease(InputAction.CallbackContext context)
        {
            if (!freelook.enable || !freelook.hold)
                return;

            freeLookInput = false;
        }

        void OnSlowWalkPress(InputAction.CallbackContext context)
        {
            int state = (int)movementState;
            if (!slowwalk.enable || !grounded || state > 4 || state == 1)
                return;

            if (!slowwalk.hold && state == 4)
            {
                ResetMovement();
            }
            else
            {
                SlowWalk();
            }
        }

        void OnSlowWalkRelease(InputAction.CallbackContext context)
        {
            if (!slowwalk.enable || !slowwalk.hold || !grounded || movementState != MovementState.SlowWalk)
                return;

            ResetMovement();
        }

        void OnGrapplePress(InputAction.CallbackContext context)
        {
            if (!grapple.enable)
                return;
            int state = (int)movementState;
            if (state == 11)
                EndGrapple();
            else if (state <= 4)
                StartGrapple();
        }

        // Initilize Input
        void InitInput()
        {
            input.lookAction.performed += OnLookChange;
            input.moveAction.performed += OnMoveChange;
            input.moveAction.canceled += OnMoveChange;
            input.jumpAction.performed += OnJumpPress;
            input.sprintAction.performed += OnSprintPress;
            input.sprintAction.canceled += OnSprintRelease;
            input.crouchSlideAction.performed += OnCrouchPress;
            input.crouchSlideAction.canceled += OnCrouchRelease;
            input.dashAction.performed += OnDashPress;
            input.poundAction.performed += OnPoundPress;
            input.autorunAction.performed += OnAutoRunPress;
            input.leanAction.performed += OnLeanPress;
            input.leanAction.canceled += OnLeanRelease;
            input.freelookAction.performed += OnFreeLookPress;
            input.freelookAction.canceled += OnFreeLookRelease;
            input.slowwalkAction.performed += OnSlowWalkPress;
            input.slowwalkAction.canceled += OnSlowWalkRelease;
            input.grappleAction.performed += OnGrapplePress;
        }

        /// <summary>
        /// Enable
        /// Enables player input.
        /// </summary>
        public void Enable()
        {
            input.moveAction.Enable();
            input.lookAction.Enable();
            input.jumpAction.Enable();
            input.sprintAction.Enable();
            input.crouchSlideAction.Enable();
            input.dashAction.Enable();
            input.poundAction.Enable();
            input.autorunAction.Enable();
            input.leanAction.Enable();
            input.freelookAction.Enable();
            input.slowwalkAction.Enable();
            input.grappleAction.Enable();
        }

        /// <summary>
        /// Disable
        /// Disables player input.
        /// </summary>
        public void Disable()
        {
            input.moveAction.Disable();
            input.lookAction.Disable();
            input.jumpAction.Disable();
            input.sprintAction.Disable();
            input.crouchSlideAction.Disable();
            input.dashAction.Disable();
            input.poundAction.Disable();
            input.autorunAction.Disable();
            input.leanAction.Disable();
            input.freelookAction.Disable();
            input.slowwalkAction.Disable();
            input.grappleAction.Disable();
        }

        #endregion New Input
#else
        #region Old Input
    void OnLookChange(Vector2 lookInputs)
    {
        if (movementState == MovementState.Grappling) // don't want to move camera if grappling. Other states can be applied here as needed.
            return;

        Look(lookInputs * input.lookSensitivity / 10);
    }

    void OnMoveChange(Vector2 moveInputs)
    {
        this.moveInputs = moveInputs;
    }

    void OnSprintPress()
    {
        int state = (int)movementState;
        if (!sprint.enable || (moveInputs.y < 0.1f && !autoRun) || state > 4)
            return;

        if (sprint.hold) // hold sprint
        {
            if (grounded)
            {
                sprint = true;

                if (headspaceAvailable)
                    Sprint();
                else
                    stand = true;
            }
        } else // toggle sprint
        {
            if (state == 1) // reset movement but must account for auto run
            {
                sprint = false;

                if (grounded)
                {
                    bool autoRun = this.autoRun;
                    ResetMovement();
                    this.autoRun = autoRun;
                } 
            }
            else
            {
                if (grounded)
                {
                    sprint = true;

                    if (headspaceAvailable)
                        Sprint();
                    else
                        stand = true;
                }
            }
        }
    }

    void OnSprintRelease()
    {
        if (!sprint.enable || !sprint.hold)
            return;

        sprint = false;

        if (grounded && movementState == MovementState.Sprinting) 
        {
            bool autoRun = this.autoRun;
            ResetMovement();
            this.autoRun = autoRun;
        }
    }

    void OnJumpPress() 
    {
        if (!jump.enable)
            return;

        if (grounded || movementState == MovementState.Climbing)
            jump = true;
        else if (numJumps > 0) 
        {
            numJumps--;
            jump = true;
        }
    }

    void OnCrouchPress()
    {
        if (!crouch.enable)
            return;

        if (!grounded)
        {
            slide = true;
            return;
        }

        if (!crouch.hold && (movementState == MovementState.Crouching || movementState == MovementState.Sliding))
            stand = true;
        else
        {
            if (sprint.enable && slide.enable && movementState == MovementState.Sprinting)
                Slide();
            else
                Crouch();
        }
    }

    void OnCrouchRelease()
    {
        if (!crouch.enable || !crouch.hold)
            return;

        if (!grounded)
        {
            slide = true;
            return;

        }

        if (movementState == MovementState.Crouching || movementState == MovementState.Sliding)
            stand = true;
    }

    void OnDashPress()
    {
        if (!dash.enable || (!grounded && !dash.canDashInAir) || (int)movementState > 1 || numDashes == 0)
            return;

        Dash();
    }

    void OnPoundPress()
    {
        if (!pound.enable || grounded || !canPound)
            return;

        Pound();
    }

    void OnAutoRunPress()
    {
        int state = (int)movementState;
        if (!autorun.enable || !grounded || state > 4)
            return;

        autoRun = !autoRun;
    }

    void OnLeanPress(float value)
    {
        int state = (int)movementState;
        if (!lean.enable || !grounded || (state <= 4 && state != 1))
            return;

        Lean(value);
    }
    
    void OnLeanRelease()
    {
        int state = (int)movementState;
        if (!lean.enable || !lean.hold || state <= 4)
            return;

        StopLean();
    }

    void OnFreeLookPress()
    {
        if (!freelook.enable)
            return;

        if (!freelook.hold && freelookInput)
            freelook = false;
        else
            freelookInput = true;
    }
    
    void OnFreeLookRelease()
    {
        if (!freelook.enable || !freelook.hold)
            return;

        freelookInput = false;
    }
    
    void OnSlowWalkPress()
    {
        int state = (int)movementState;
        if (!slowwalk.enable || !grounded || state > 4 || state != 1)
            return;

        if (!slowwalk.hold && state == 4) 
        {
            ResetMovement();
        } else 
        {
            SlowWalk();
        }
    }
    
    void OnSlowWalkRelease()
    {
        if (!slowwalk.enable || !slowwalk.hold || !grounded || movementState != MovementState.SlowWalk)
            return;

        ResetMovement();
    }
    
    void OnGrapplePress()
    {
        if (!grapple.enable)
            return;
        int state = (int)movementState;
        if (state == 12)
            EndGrapple();
        else if (state <= 4)
            ThrowGrapple();
    }

    public void Enable()
    {
        enable = true;
    }

    public void Disable()
    {
        enable = false;
    }
        #endregion Old Input
#endif
        /// <summary>
        /// HandleInput
        /// Controls the active state of the current input system
        /// </summary>
        void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (!input.enable && enable)
                Disable();
            else if (input.enable && !enable)
                Enable();

#else
        if (!input.enable)
            return;

        OnMoveChange(new Vector2(Input.GetAxis(input.moveAction.horizontal), Input.GetAxis(input.moveAction.vertical)));

        if (Input.GetButtonDown(input.jumpAction))
            OnJumpPress();
        if (Input.GetButtonDown(input.sprintAction))
            OnSprintPress();
        if (Input.GetButtonUp(input.sprintAction))
            OnSprintRelease();
        if (Input.GetButtonDown(input.crouchAction))
            OnCrouchPress();
        if (Input.GetButtonUp(input.crouchAction))
            OnCrouchRelease();
        if (Input.GetButtonDown(input.dashAction))
            OnDashPress();
        if (Input.GetButtonDown(input.poundAction))
            OnPoundPress();
        if (Input.GetButtonDown(input.autoRunAction))
            OnAutoRunPress();
        OnLeanPress(Input.GetAxis(input.leanAction));
        if (Input.GetButtonDown(input.freeLookAction))
            OnFreeLookPress();
        if (Input.GetButtonUp(input.freeLookAction))
            OnFreeLookRelease();
        if (Input.GetButtonDown(input.slowWalkAction))
            OnSlowWalkPress();
        if (Input.GetButtonUp(input.slowWalkAction))
            OnSlowWalkRelease();
        if (Input.GetButtonDown(input.grappleAction))
            OnGrapplePress();
#endif

            enable = input.enable;
        }

        #endregion Input

        #region Engine

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void Start()
        {
#if ENABLE_INPUT_SYSTEM
            InitInput();
#endif

            // Initilize Variables
            col = GetComponent<CapsuleCollider>();
            cam = GetComponentInChildren<Camera>();
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;

            currSpeed = movement.speed;
            numJumps = jump.doubleJumpCount;
            numDashes = dash.count;
            defaultHeight = cameraContainer.localPosition.y;
            heightOffset = col.height * 2 / defaultHeight;
            targetHeight = defaultHeight;
            cameraOffset.y = defaultHeight;
            FOV = cam.fieldOfView;
            targetFOV = FOV;
            defaultFOV = FOV;
            collisionExclusionMask = ~LayerMask.GetMask("Player");
            rb.useGravity = false;

            if (grapple.enable && grapple.prefab.GetComponent<LineRenderer>() == null) // Check grapple
            {
                Debug.LogError("[Error] Grapple requires component of type <LineRenderer>.");
            }

            if (speedlines.enable) // Instantiate Speedlines Particle System
            {
                if (speedlines.prefab == null)
                {
                    Debug.LogError("[ERROR] Speed lines prefab cannot be null.");
                    return;
                }

                ParticleSystem p = Instantiate(speedlines.prefab, cam.transform).GetComponent<ParticleSystem>();
                p.transform.localPosition = Vector3.forward;
                speedLinesEmission = p.emission;
                var m = p.main;
                m.startColor = speedlines.gradient;
                ParticleSystemRenderer renderer = p.GetComponent<ParticleSystemRenderer>();
                renderer.lengthScale = 5 + (45 * speedlines.thiness); // clamps thickness between 10 and 50
                ParticleSystem.ShapeModule shape = p.shape;
                shape.radius = renderer.lengthScale + speedlines.radius;
            }

            // Initilize Functions
            Enable();
            if (hideMouseOnAwake)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }


        /// <summary>
        /// [ENGINE]
        /// </summary>
        void Update()
        {
            HandleInput();
            HandleCollisions();
        }

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void FixedUpdate()
        {
            d = Time.deltaTime;
            HandleCamera();

            HandleMovementStates();
            HandleInputQueue();
            HandlePhysics();
        }

        #endregion Engine

        #region Private Methods

        #region Actions

        /// <summary>
        /// Look
        /// Rotates <code>cameraContainer</code> from mouseDelta.
        /// </summary>
        /// <param name="lookInputs">The <code>Vector2</code> containing mouseDelta.</param>
        void Look(Vector2 lookInputs)
        {
            if (freelook.enable)
            {
                if (freeLookInput)
                {
                    lookOffset.y -= lookInputs.y * input.lookAxis.y * freelook.axis.y;
                    lookOffset.x += lookInputs.x * input.lookAxis.x * freelook.axis.x;
                    float y = Mathf.Clamp(yRot + lookOffset.y, -freelook.verticalAngle, freelook.verticalAngle);
                    float x = Mathf.Clamp(lookOffset.x, -freelook.verticalAngle, freelook.verticalAngle);
                    lookOffset.y = y - yRot;
                    lookOffset.x = x;
                    cameraContainer.localRotation = Quaternion.Euler(y, x, cameraContainer.localEulerAngles.z);
                }
                else
                {
                    yRot -= lookInputs.y * input.lookAxis.y;
                    yRot = Mathf.Clamp(yRot, -90, 90);
                    xRot += lookInputs.x * input.lookAxis.x;

                    cameraContainer.localRotation = Quaternion.Euler(yRot + lookOffset.y, lookOffset.x, cameraContainer.localEulerAngles.z);
                    transform.rotation = Quaternion.Euler(0, xRot, 0);
                }
            }
            else
            {
                yRot -= lookInputs.y * input.lookAxis.y;
                yRot = Mathf.Clamp(yRot, -90, 90);

                xRot += lookInputs.x * input.lookAxis.x;

                cameraContainer.localRotation = Quaternion.Euler(yRot, cameraContainer.localEulerAngles.y, cameraContainer.localEulerAngles.z);
                transform.rotation = Quaternion.Euler(0, xRot, 0);
            }
        }

        /// <summary>
        /// Move
        /// Applies a movement force to the player based on <code>moveInputs</code>.
        /// If <code>!grounded</code> use <code>airDrag</code> to Lerp along <code>moveInputs</code>.
        /// Else project along normal vector and Lerp using <code>movementAcceleration && movementDecceleration</code>.
        /// </summary>
        void Move()
        {
            // movement variables
            float acceleration;
            Vector3 targetVelocity;
            Vector3 direction;

            if (autoRunInput) // Auto Run 
                direction = (Mathf.Min(1 + moveInputs.y, 1) * transform.forward + moveInputs.x * transform.right).normalized;
            else
                direction = (moveInputs.y * transform.forward + moveInputs.x * transform.right).normalized;

            #region Movement Tilt

            // Movement Tilt
            if (movement.useTilt && !leanInput) // have to check if leaning since it also affects tilt
            {
                tiltRotation = moveInputs.x * -movement.tiltAngle;
                Vector3 targetRotation = cameraContainer.localEulerAngles;
                targetRotation.z = tiltRotation;
                cameraContainer.localRotation = Quaternion.Lerp(cameraContainer.localRotation, Quaternion.Euler(targetRotation), movement.tiltSpeed * d);
            }

            #endregion Movement Tilt

            #region Direction and Headbob 

            if (grounded)
            {
                direction = Vector3.ProjectOnPlane(direction, normal); // project onto normal if grounded

                // Headbob
                if (headbob.enable && currSpeed > 0) // we run into issues when speed is 0
                {
                    float yBobAmount;
                    float xBobAmount;
                    Vector3 targetPosition = cameraContainer.localPosition;

                    if (moveInputs != Vector2.zero)
                    {
                        float t = Time.time;
                        float moveMultiplier = Mathf.Max((currSpeed / movement.speed / 6) * headbob.speedMultiplier, 1 / 6f);
                        yBobAmount = cameraOffset.y + (headbob.verticalAmount * moveMultiplier * Mathf.Sin(currSpeed * 2 * t));
                        xBobAmount = cameraOffset.x + (headbob.horizontalAmount * moveMultiplier * Mathf.Sin(currSpeed * t));
                    }
                    else
                    {
                        yBobAmount = Mathf.Lerp(targetPosition.y, cameraOffset.y, currSpeed * d);
                        xBobAmount = Mathf.Lerp(targetPosition.x, cameraOffset.x, currSpeed * d);
                    }

                    targetPosition.y = yBobAmount;
                    targetPosition.x = xBobAmount;
                    cameraContainer.localPosition = targetPosition;
                }
            }

            #endregion Direction and Headbob

            #region Acceleration

            //if we are no longer pressing and input, carryon moving in the last direction we were set to move in
            if (moveInputs.x == 0 && moveInputs.y == 0 && !autoRunInput) // need to account for auto run since using input calcs
            {
                acceleration = movement.useDecceleration ? movement.decceleration : 1000;
                targetVelocity = Vector3.zero;
            }
            else
            {
                acceleration = movement.useAcceleration ? movement.acceleration : 1000;
                targetVelocity = direction;
                targetVelocity *= currSpeed;
            }

            #endregion Acceleration

            #region Apply

            // Apply movement force
            acceleration = 1 / acceleration;
            Vector3 velocity = Vector3.Lerp(rb.velocity, targetVelocity, (grounded ? acceleration : (1 - physics.airDrag)) * d);
            rb.velocity = velocity;

            #endregion Apply
        }

        /// <summary>
        /// Jump
        /// Applies upward force to player. Additional horizontal force is calculated by state.
        /// IF <code>enableDoubleJumps</code> and <code>!grounded</code>, cancel latest velocity.
        /// </summary>
        void Jump()
        {
            float force = jump.force;

            #region Double Jump

            //  Double jump if not grounded and enabled
            if (jump.enableDoubleJumps && (!grounded && !canJump))
            {
                numJumps--;
                force = jump.doubleJumpforce;
            }

            #endregion Double Jump

            #region Jump Direction

            // Calculate jump direction from movement state
            if (movementState == MovementState.Mantling  || movementState == MovementState.Dashing)
                return;

            if (movementState == MovementState.Crouching || movementState == MovementState.Sliding)
                Stand();

            Vector3 direction = Vector3.up;

            if (jump.enableWallJump && onWall)
            {
                if (movementState == MovementState.Climbing)
                {
                    ExitClimb();
                    direction = (-transform.forward + Vector3.up).normalized;
                } else
                {
                    /////
                }
            } else 
            {
                if (movementState == MovementState.Sprinting)
                {
                    sprintInput = true; // queue sprint input state
                }
                else if (movementState == MovementState.Pounding)
                {
                    ResetMovement();
                }
                else if (movementState == MovementState.Grappling)
                {
                    EndGrapple();
                }
             }

            #endregion Jump Direction

            #region Apply Force

            // Reset y velocity and add forces
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(direction * force, ForceMode.Impulse);
            jumpInput = false;
            canJump = false;

            #endregion Apply Force
        }

        void Lurch()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            Vector3 direction = localVelocity.normalized;

            // Redirect velocity based on input for
            if (moveInputs != Vector2.zero)
            {
                Vector3 temp = localVelocity;

                // Redirect velocitiees
                if (moveInputs.x != 0 && Mathf.Abs(localVelocity.z) > Mathf.Abs(localVelocity.x))
                    temp.x = Mathf.Abs(localVelocity.z) * moveInputs.x;
                else if ((direction.x > 0.1 && moveInputs.x < -0.1) || (direction.x < -0.1 && moveInputs.x > 0.1))
                    temp.x = -localVelocity.x;
                else if (moveInputs.x == 0)
                    temp.x = 0;

                if (moveInputs.y != 0 && Mathf.Abs(localVelocity.x) > Mathf.Abs(localVelocity.z))
                    temp.z = Mathf.Abs(localVelocity.x) * moveInputs.y;
                else if ((direction.z > 0.1 && moveInputs.y < -0.1) || (direction.z < -0.1 && moveInputs.y > 0.1))
                    temp.z = -localVelocity.z;
                else if (moveInputs.y == 0)
                    temp.z = 0;

                localVelocity = temp;
            }

            rb.velocity = transform.TransformDirection(localVelocity);
        }

        /// <summary>
        /// Crouch
        /// Sets <code>movementState</code> to <code>MovementState.Sprinting</code> and associated variables.
        /// </summary>
        void Sprint()
        {
            bool autoRun = this.autoRunInput;
            ResetMovement();
            this.autoRunInput = autoRun;

            sprintInput = true;
            targetFOV = defaultFOV * 1.1f;
            currSpeed = movement.speed * sprint.speedMultiplier;
            movementState = MovementState.Sprinting;
        }

        /// <summary>
        /// Crouch
        /// Sets <code>movementState</code> to <code>MovementState.Crouching</code> and associated variables.
        /// </summary>
        void Crouch()
        {
            currSpeed = movement.speed * crouch.speedMultiplier;
            targetHeight = defaultHeight * crouch.height;
            movementState = MovementState.Crouching;
        }

        /// <summary>
        /// SlowWalk
        /// Sets <code>movementState</code> to <code>MovementState.Slowwalk</code> and associated variables.
        /// </summary>
        void SlowWalk()
        {
            currSpeed = movement.speed * slowwalk.speedMultiplier;
            movementState = MovementState.SlowWalk;
        }

        /// <summary>
        /// StartSlide
        /// Sets <code>movementState</code> to <code>MovementState.Sliding</code> and associated variables.
        /// </summary>
        void StartSlide() 
        {
            targetFOV = defaultFOV * 1.1f;
            targetHeight = defaultHeight * slide.height;

            rb.AddForce(Vector3.ProjectOnPlane(transform.forward, normal) * slide.force, ForceMode.Impulse);
            movementState = MovementState.Sliding;
        }

        /// <summary>
        /// Slide
        /// Adds slide forces to player.
        /// </summary>
        void Slide()
        {
            // Slow down
            if (!slide.infinite)
                rb.AddForce(-rb.velocity.normalized * slide.friction * 100);

            // Slide Control
            rb.MovePosition(rb.position + (transform.right * moveInputs.x) * rb.velocity.magnitude * slide.control * d);

            // Slide down slopes
            if (normalAngle > 0)
                ProjectDownSlope(Mathf.Min(normalAngle / physics.slopeAngle, 1) * physics.slopeForce);

            // If force is decellarating and less than threshold, stop
            if ((Mathf.Abs(previousVelocity.x) > Mathf.Abs(rb.velocity.x) && Mathf.Abs(previousVelocity.z) > Mathf.Abs(rb.velocity.z))
                && (Mathf.Abs(rb.velocity.x) < 2f && Mathf.Abs(rb.velocity.z) < 2f))
            {
                ResetMovement();

                if (crouch.enable)
                    Crouch();
            }
        }

        /// <summary>
        /// StartDash
        /// Sets <code>movementState</code> to <code>MovementState.Dashing</code> and inits state.
        /// </summary>
        void StartDash() 
        {
            // Calculate Dash Direction based on input and axis
            Vector3 direction = Vector3.zero;

            direction += transform.forward * (moveInputs.y > 0.1f && dash.axis.z > 0 ? 1 : 0);
            direction += -transform.forward * (moveInputs.y < -0.1f && dash.axis.w > 0 ? 1 : 0);
            direction += transform.right * (moveInputs.x > 0.1f && dash.axis.x > 0 ? 1 : 0);
            direction += -transform.right * (moveInputs.x < -0.1f && dash.axis.y > 0 ? 1 : 0);

            // If no input, set direction to only enabled axis
            if (direction == Vector3.zero)
            {
                if (dash.axis.z > 0)
                    direction += transform.forward;
                else if (dash.axis.w > 0)
                    direction -= transform.forward;
            }

            // Set direction and control state
            rb.velocity = Vector3.zero;
            targetFOV = defaultFOV * 1.1f;
            dashDirection = direction;
            StartCoroutine(YieldDashTime());
            StartCoroutine(YieldDashCooldown());

            movementState = MovementState.Dashing;
        }

        /// <summary>
        /// Dash
        /// Moves player by dash force.
        /// </summary>
        void Dash()
        {
            Vector3 velocity = dashDirection;

            // project onto normal if grounded
            if (grounded)
                velocity = Vector3.ProjectOnPlane(dashDirection, normal);

            velocity = dash.force * 10 * velocity.normalized;
            rb.velocity = velocity;
        }

        /// <summary>
        /// StartPound
        /// Sets <code>movementState</code> to <code>MovementState.Pounding</code> and inits state.
        /// </summary>
        void StartPound()
        {
            if (!pound.carryMomentum)
            {
                Vector3 velocity = rb.velocity;
                velocity.x = 0;
                velocity.y = 0;
                rb.velocity = velocity;
            }

            if (pound.useDuration)
                StartCoroutine(YieldPoundTime());

            canPound = false;
            poundOrigin = transform.position;
            movementState = MovementState.Pounding;
        }

        /// <summary>
        /// Pound
        /// Applys downward pound force to player.
        /// </summary>
        void Pound() 
        {
            poundHeight = Vector3.Distance(transform.position, poundOrigin);
            float heightMultiplier = pound.multiplyByHeight ? poundHeight * 30 : 0;
            rb.AddForce(Vector3.down * (pound.force * 10 + heightMultiplier));
        }

        /// <summary>
        /// EndPound
        /// Applys bounce force to player and knockback to nearby objects. Calls <code>RestMovement</code>
        /// </summary>
        void EndPound()
        {
            float heightMultiplier = pound.multiplyByHeight ? poundHeight * 30 : 0;

            // Create knockback
            if (pound.knockback > 0)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, pound.knockbackRadius);

                foreach (Collider collider in colliders)
                {
                    Rigidbody entity = collider.GetComponent<Rigidbody>();
                    if (entity != null && entity != rb)
                        entity.AddExplosionForce(pound.knockback * 100 + heightMultiplier, transform.position, pound.knockbackRadius, pound.knockbackUpwardsModifier);
                }
            }

            // Create Bounce
            if (pound.bounce > 0)
                rb.AddForce(Vector3.up * (pound.bounce * 100 + heightMultiplier));

            ResetMovement();
        }

        /// <summary>
        /// StartMantle
        /// Sets <code>movementState</code> to <code>MovementState.Mantling</code> and inits state based on height param.
        /// </summary>
        /// <param name="height">The height of object that is being mantled</param>
        void StartMantle(float height) 
        {
            if (height < 1) // half the size of the player
            {
                // Vault
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // keep velocity
            }
            else
            {
                // Mantle
                rb.velocity = Vector3.zero; // reset velocity
            }

            // play correct anim

            if (movementState == MovementState.Climbing)
                ExitClimb();

            movementState = MovementState.Mantling;
        }

        /// <summary>
        /// Mantle
        /// Apply upward mantle force to player.
        /// </summary>
        void Mantle()
        {
            rb.velocity = Vector3.up * mantle.speed;
        }

        /// <summary>
        /// ExitMantle
        /// Apply forward force to player and reset movement
        /// </summary>
        void ExitMantle()
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Reset velocity - same reason for reseting following climb.
            rb.AddForce(transform.forward * 100);
            ResetMovement();
        }

        /// <summary>
        /// StartClimb
        /// Sets <code>movementState</code> to <code>MovementState.Climbing</code> and inits state.
        /// </summary>
        void StartClimb() 
        {
            rb.velocity = Vector3.zero;

            if (climb.useDuration)
                StartExitClimbCoroutine();

            numJumps = jump.doubleJumpCount;
            movementState = MovementState.Climbing;
        }

        /// <summary>
        /// Climb
        /// Applys an upwards force of <FujinClimbModule.speed> to player
        /// </summary>
        void Climb()
        {
            rb.velocity = Vector3.up * climb.speed;
        }

        /// <summary>
        /// ExitClimb
        /// Calls <code>StopExitClimbCoroutine</code> and rests movement state.
        /// </summary>
        void ExitClimb()
        {
            StopExitClimbCoroutine();

            // Reset Y velocity (if not, velocity carries upward) - Should Probably show this
            Vector3 temp = rb.velocity;
            temp.y = 0;
            rb.velocity = temp;
            ResetMovement();
        }

        /// <summary>
        /// Lean
        /// Queues <code>leanInput</code> for auxilary state execution and either sets direction or calls <code>StopLean</code> based on input
        /// </summary>
        /// <param name="direction">The input direction to lean towards</param>
        void Lean(float direction)
        {
            leanInput = true;

            if (leanDirection > 0.1f && direction > 0.1f || leanDirection < -0.1f && direction < -0.1f)
                StopLean();
            else
                leanDirection = direction;
        }

        /// <summary>
        /// StopLean
        /// Resets lean direction and movement state.
        /// </summary>
        void StopLean()
        {
            leanInput = false;
            leanDirection = 0;

            if (movementState == MovementState.Default)
                currSpeed = movement.speed;
            else if (movementState == MovementState.SlowWalk)
                currSpeed = movement.speed * slowwalk.speedMultiplier;
        }

        /// <summary>
        /// StartGrapple
        /// Sets <code>movementState</code> to <code>MovementState.ThrowinGrapple</code> and inits state. This includes spawning and initializing the grapple object.
        /// </summary>
        void StartGrapple()
        {
            grapplePoint = cam.transform.position + cam.transform.forward * grapple.range;

            grappleObj = Instantiate(grapple.prefab);
            grapplelineRenderer = grappleObj.GetComponent<LineRenderer>();
            grappleObj.transform.position = transform.position + -transform.right + Vector3.up * cameraOffset.y;
            grappleDistance = Vector3.Distance(grappleObj.transform.position, grapplePoint);
            movementState = MovementState.ThrowingGrapple;
        }
        
        /// <summary>
        /// ThrowGrapple
        /// Monitors the distance between <code>grapplePoint</code> and the <code>grappleObj</code>. Controls the direction of the grapple accordingly.
        /// </summary>
        void ThrowGrapple()
        {
            if (!returnGrapple && grappleDistance < 0.1f) // fire backward
            {
                returnGrapple = true;
                grapplePoint = transform.position + -transform.right + Vector3.up * cameraOffset.y;
            }
            else if (returnGrapple && grappleDistance < 0.1f) // end
                EndGrapple();

            CheckGrapple();
            StretchGrapple(grapple.throwSpeed);
            grappleDistance = Vector3.Distance(grappleObj.transform.position, grapplePoint);
        }

        /// <summary>
        /// Grapple
        /// Calls <code>StretchGrapple</code> and applys grapple force along grapple direction vector.
        /// </summary>
        void Grapple()
        {
            // Move Grapple
            StretchGrapple(grapple.speed);

            // Limit player rotation

            // Move Player
            Vector3 direction = ((grapplePoint - cam.transform.position) + cam.transform.forward).normalized;
            rb.velocity = direction * grapple.speed;

            if (drawGizmos)
                Debug.DrawLine(cam.transform.position, direction * rb.velocity.magnitude, Color.cyan, d);

            // Check if End Conditions are  met
            float angle = Vector3.Angle(cam.transform.position, grapplePoint);
            float distance = Vector3.Distance(cam.transform.position, grapplePoint);
            bool inView = Physics.Raycast(cam.transform.position, direction, grapple.range, collisionExclusionMask);
            if (!inView || angle > 90 || rb.velocity.magnitude < 5 || distance < 1 || distance > grapple.range)
            {
                EndGrapple();
                return; 
            }
        }

        /// <summary>
        /// CheckGrapple
        /// Fires a ray along the grapple to check for collisions. Controls grapple state accordingly.
        /// </summary>
        void CheckGrapple()
        {
            RaycastHit hit;
            Vector3 castOrigin = cam.transform.position;
            Vector3 direction = grappleObj.transform.position - castOrigin;
            float distance = Vector3.Distance(castOrigin, grappleObj.transform.position);
            if (Physics.Raycast(castOrigin, direction, out hit, distance, collisionExclusionMask))
            {
                targetFOV = defaultFOV * 1.1f;
                grappleObj.transform.position = hit.point;
                grapplePoint = hit.point;
                rb.velocity = Vector3.zero;
                movementState = MovementState.Grappling;

                if (drawGizmos)
                    Debug.DrawLine(castOrigin, castOrigin + direction * distance, Color.red, d);

            } else if (drawGizmos) 
            {
                Debug.DrawLine(castOrigin, castOrigin + direction * distance, Color.green, d);

            }
        }

        /// <summary>
        /// StretchGrapple
        /// Moves the <code>grappleObj</code> along the grapple direction and sets the <code>grappleLineRenderer</code> points to the objects position and grapple origin.
        /// </summary>
        /// <param name="speed"></param>
        void StretchGrapple(float speed)
        {
            // Move Grapple
            Vector3 direction = grapplePoint - grappleObj.transform.position;
            grappleObj.transform.Translate(direction * speed * d);
            grapplelineRenderer.SetPosition(0, transform.position + -transform.right + Vector3.up * cameraOffset.y);
            grapplelineRenderer.SetPosition(1, grappleObj.transform.position);
        }

        /// <summary>
        /// EndGrapple
        /// Destroys the <code>grappleObj</code> and resets movement state.
        /// </summary>
        void EndGrapple()
        {
            returnGrapple = false;
            Destroy(grappleObj);
            ResetMovement();
        }

        /// <summary>
        /// Stand
        /// Similar to <code>ResetMovement</code>. Used carry input when transitioning from <code>movementState == MovementState.Crouching || movementState == MovementState.Sliding</code>
        /// </summary>
        void Stand()
        {
            bool sprint = this.sprintInput;
            bool autoRun = this.autoRunInput;

            ResetMovement();

            this.sprintInput = sprint;
            this.autoRunInput = autoRun;

            if (sprint)
                Sprint();
        }

        /// <summary>
        /// ResetMovement
        /// Resets <code>movementState</code> and associated variables to default.
        /// </summary>
        void ResetMovement()
        {
            jumpInput = false;
            sprintInput = false;
            slideInput = false;
            cueStand = false;
            autoRunInput = false;

            targetFOV = defaultFOV;
            targetHeight = defaultHeight;
            currSpeed = movement.speed;
            movementState = MovementState.Default;
        }

        /// <summary>
        /// HandleInputQueue
        /// Reserved for actions that cannot be executed immediately or rely on conditions being executed. These conditions may include functions that require a variable be met first, or simply physics-based functions being passed to execute in Fixed-Update.
        /// </summary>
        void HandleInputQueue()
        {
            if (jumpInput)
                Jump();

            if (cueStand && headspaceAvailable)
                Stand();
        }

        /// <summary>
        /// HandleMovementStates
        /// State machine that all state-specific functions of <code>movementState</code>.
        /// </summary>
        void HandleMovementStates()
        {
            #region Movement States 

            // Core States
            switch (movementState)
            {
                case MovementState.Sprinting:
                    if (moveInputs.y < 0.1f && !autoRunInput) // account for no input or auto run
                        ResetMovement();
                    else
                        Move();
                    break;
                case MovementState.Sliding:
                    Slide();
                    break;
                case MovementState.Dashing:
                    Dash();
                    break;
                case MovementState.Pounding:
                    Pound();
                    break;
                case MovementState.Mantling:
                    Mantle();
                    break;
                case MovementState.Climbing:
                    Climb();
                    break;
                case MovementState.ThrowingGrapple:
                    ThrowGrapple();
                    Move();
                    break;
                case MovementState.Grappling:
                    Grapple();
                    break;
                default:
                    Move();
                    break;
            }

            #endregion Movement States

            #region Auxillary States

            // Auxillary States - States that can be overlayed onto a core state. We must handle these seperately

            // LEANING
            if (lean.enable && (leanInput || leanDirection == 0))
            {
                int state = (int)movementState;
                if (leanInput && (!grounded || (state > 4 || state == 1)))
                {
                    StopLean();
                }
                else
                {
                    if (leanDirection != 0)
                    {
                        if (movementState == MovementState.Default)
                            currSpeed = movement.speed * lean.speedMultiplier;
                        else if (movementState == MovementState.Crouching)
                            currSpeed = movement.speed * crouch.speedMultiplier * lean.speedMultiplier;
                        else if (movementState == MovementState.SlowWalk)
                            currSpeed = movement.speed * slowwalk.speedMultiplier * lean.speedMultiplier;
                    }

                    Vector3 targetRotation = cameraContainer.localEulerAngles;
                    targetRotation.z = lean.angle * -leanDirection;
                    cameraContainer.localRotation = Quaternion.Lerp(cameraContainer.localRotation, Quaternion.Euler(targetRotation), lean.speed * d);
                    tiltRotation = cameraContainer.localEulerAngles.z;

                    Vector3 targetPosition = cameraContainer.localPosition;
                    cameraOffset.x = Mathf.Lerp(cameraOffset.x, lean.offset * leanDirection, lean.speed * d);
                    targetPosition.x = cameraOffset.x;
                    cameraContainer.localPosition = targetPosition;
                }
            }

            // FREELOOK
            if (freelook.enable)
            {
                if (!freeLookInput && lookOffset != Vector2.zero)
                {
                    if (Vector2.Distance(lookOffset, Vector2.zero) < 0.1f)
                        lookOffset = Vector2.zero;

                    lookOffset = Vector2.Lerp(lookOffset, Vector2.zero, freelook.resetSpeed * d);
                    cameraContainer.localRotation = Quaternion.Euler(yRot + lookOffset.y, lookOffset.x, tiltRotation);
                }
            }

            // SPEED LINES
            if (speedlines.enable)
            {
                float speed = Mathf.Clamp(rb.velocity.magnitude - speedlines.minVelocity, 0, speedlines.maxVelocity);
                speedLinesEmission.rateOverTimeMultiplier = Mathf.Pow(speed, 1 + speedlines.curve) * speedlines.amount;
            }

            #endregion Auxillary States
        }

        #endregion Actions

        #region Collision

        /// <summary>
        /// Handle Collisions
        /// Checks and handles the collisions status for the player motor. <remarks>Should not be called Update.</remarks>
        /// </summary>
        void HandleCollisions()
        {
            CheckGround();
            CheckHeight();

            if (CheckStep())
                return;

            if (mantle.enable && CheckMantle())
                return;

            if (climb.enable && CheckClimb())
                return;
        }

        /// <summary>
        /// CheckGround
        /// Checks for ground collision via a <code>SphereCast</code>. Calculates new normal vectors and handles instanse properties on collision.
        /// </summary>
        void CheckGround()
        {

            Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.01f);
            Vector3 direction = Vector3.down;

            RaycastHit hit;
            if (Physics.SphereCast(origin, groundCheckRadius, direction, out hit, groundCheckDrawDistance, collisionExclusionMask))
            {
                Vector3 contactPoint = origin + (direction * groundCheckDrawDistance);
                Vector3 contactDirection = hit.point - origin;
                float contactOffset = contactDirection.magnitude + 0.01f;

                RaycastHit spherePoint;
                if (Physics.Raycast(contactPoint, contactDirection, out spherePoint, contactOffset, collisionExclusionMask))
                {
                    normal = spherePoint.normal;
                }

                // Slopes & Ledges
                normalAngle = Vector3.Angle(normal, Vector3.up);
                if (Mathf.Approximately(normalAngle, 90))
                {
                    normalAngle = 0;
                    normal = Vector3.up;
                }

                // Handle State
                if (!grounded)
                {
                    if (sprintInput && movementState != MovementState.Sprinting)
                        Sprint();
                    else if (!sprintInput && movementState == MovementState.Sprinting)
                        ResetMovement();

                    if (slideInput || movementState == MovementState.Sliding)
                    {
                        float slideAngle = Vector3.Angle(previousVelocity.normalized, -Vector3.up);

                        Vector3 localVelocity = transform.InverseTransformDirection(previousVelocity);
                        localVelocity = new Vector3(Mathf.Abs(localVelocity.x), Mathf.Abs(localVelocity.y), Mathf.Abs(localVelocity.z));
                        float additionalSlideForce = localVelocity.z > localVelocity.y ? localVelocity.z : localVelocity.y;

                        if (slideAngle > 0)
                            rb.AddForce(transform.forward * additionalSlideForce * 1 / slideAngle);

                        if (slideInput)
                        {
                            rb.velocity = new Vector3(previousVelocity.x, 0, previousVelocity.z);
                            StartSlide();
                        }
                    }

                    if (movementState == MovementState.Pounding)
                        EndPound();

                    //slideInput = false;

                    canJump = true;
                    canPound = true;
                    numJumps = jump.doubleJumpCount;
                    lastClimbCollider = null;
                    grounded = true;
                }

                // Gizmos
                if (drawGizmos)
                {
                    Vector3 displayVector = Vector3.ProjectOnPlane(transform.forward, normal);
                    Debug.DrawLine(spherePoint.point, spherePoint.point + displayVector, Color.cyan, d);
                }

                StopJumpGraceCoroutine();
            }
            else if (grounded)
            {
                grounded = false;
                StartYieldJumpGraceCoroutine();
            }
        }

        /**void CheckStep() 
        {
            this.step = false;
            bool step = false;
            Vector3 direction = (moveInputs.y * transform.forward + moveInputs.x * transform.right).normalized;

            // Step Up
            for (float i = 0; i <= stepHeight; i += 0.05f)
            {
                Vector3 castOrigin = transform.position + Vector3.up * i;
                Vector3 destination = castOrigin + (direction * (groundCheckRadius + collisionDrawOffset));

                // Check Height
                RaycastHit hit;
                if (Physics.Raycast(castOrigin, direction, out hit, groundCheckRadius + collisionDrawOffset, collisionExclusionMask))
                {
                    step = true;

                    if (drawGizmos) 
                        Debug.DrawLine(castOrigin, destination, Color.green, Time.deltaTime);
                }

                // Check Depth
                else if (step && !Physics.Raycast(castOrigin, direction, groundCheckRadius + collisionDrawOffset + stepDepth, collisionExclusionMask))
                {
                    if (drawGizmos) 
                        Debug.DrawLine(castOrigin, destination, Color.red, Time.deltaTime);

                    // set new normal
                    this.step = true;
                    normal = Vector3.Cross(transform.position, destination + Vector3.up * .05f);
                    normalAngle = Vector3.Angle(normal, Vector3.up);

                    Debug.Log("hit");
                    return;
                }
            }

            // Step Down (reverse of Step up)
        }
        **/

        /// <summary>
        /// CheckHeight
        /// Checks for head collisions and handles updates to height.
        /// </summary>
        void CheckHeight()
        {
            if (!controlHeight)
                return;

            // Check Headspace
            float maxHeight = (defaultHeight * heightOffset / 2); // the players maximum height 
            float minHeight = 4 * col.radius / heightOffset; // the players minimum height - will use this later (limited by collider radius)

            float range = maxHeight + collisionThreshold;
            headspaceAvailable = !Physics.Raycast(transform.position, Vector3.up, range, collisionExclusionMask);

            if (!headspaceAvailable)
            {
                if (drawGizmos)
                    Debug.DrawLine(transform.position, transform.position + Vector3.up * range, Color.red);

                return;
            }
            else if (drawGizmos)
            {
                Debug.DrawLine(transform.position, transform.position + Vector3.up * range, Color.green);
            }

            // Handle Transition
            if (Mathf.Abs(targetHeight - cameraOffset.y) > 0.01f)
            {
                cameraOffset.y = Mathf.Lerp(cameraOffset.y, targetHeight, heightSmoothing * d);
                cameraOffset.y = Mathf.Clamp(cameraOffset.y, minHeight, maxHeight); // limit height to collider bounds

                col.height = (cameraOffset.y * heightOffset) / 2;
                col.center = new Vector3(col.center.x, col.height / 2, col.center.z);
                cameraContainer.localPosition = new Vector3(cameraContainer.localPosition.x, cameraOffset.y, cameraContainer.localPosition.z);

                return;
            }

            if (cameraOffset.y != targetHeight)
            {
                cameraOffset.y = targetHeight;
                cameraContainer.localPosition = new Vector3(cameraContainer.localPosition.x, cameraOffset.y, cameraContainer.localPosition.z);

                col.height = (cameraOffset.y * heightOffset) / 2;
                col.center = new Vector3(col.center.x, col.height / 2, col.center.z);
            }
        }

        /// <summary>
        /// CheckStep
        /// Checks for collisions at step height. Projects the player along step normal accordingly.
        /// </summary>
        bool CheckStep() 
        {
            return false;
        }

        bool CheckWall() 
        {
            return false;
        }

        /// <summary>
        /// CheckStep
        /// Checks for collisions elligibale for Mantle. Controls mantle state accordingly.
        /// </summary>
        bool CheckMantle()
        {
            onWall = false;
            bool canMantle = false;

            #region Raycast
            float range = collisionDrawOffset + collisionThreshold;
            Vector3 castOrigin = transform.position;
            castOrigin.y += physics.stepHeight;

            // Shoot ray
            RaycastHit hit;
            float step = 0.1f;
            float height = castOrigin.y + mantle.height - physics.stepHeight;
            while ( castOrigin.y < height)
            {
                if (Physics.Raycast(castOrigin, transform.forward, out hit, range, collisionExclusionMask))
                {
                    onWall = true;

                    if (!Physics.Raycast(castOrigin + Vector3.up * step, transform.forward, range, collisionExclusionMask))
                    {
                        canMantle = true;

                        if (drawGizmos)
                            Debug.DrawLine(castOrigin, castOrigin + transform.forward * range, Color.magenta, d);
                        break;
                    } else if (drawGizmos)
                        Debug.DrawLine(castOrigin, hit.point, Color.red, d);
                }
                else if (drawGizmos)
                    Debug.DrawLine(castOrigin, castOrigin + (transform.forward * range), Color.green, d);

                castOrigin.y += step;
            }
            #endregion Raycast

            #region Control state
            if (movementState != MovementState.Mantling && canMantle && (!grounded && !mantle.transitionFromGround || mantle.transitionFromGround) && headspaceAvailable && moveInputs.y > 0.1f)
            {
                StartMantle(castOrigin.y);
            }
            else if (movementState == MovementState.Mantling)
            {
                if (!canMantle)
                    ExitMantle();
                else if (!headspaceAvailable || (grounded && !mantle.transitionFromGround))
                    ResetMovement();
            }
            #endregion Control state

            return canMantle;
        }

        /// <summary>
        /// CheckClimb
        /// Checks for collisions elligibale for Climb. Controls climb state accordingly.
        /// </summary>
        bool CheckClimb()
        {
            onWall = false;

            #region Raycast
            float range = collisionDrawOffset + collisionThreshold;
            float angleRange = range * 1.035f;
            Vector3 castOrigin = transform.position + Vector3.up * cameraOffset.y;

            // Calculate direction vector of 30 degrees
            Vector3 negDirection = (3*transform.forward - transform.right).normalized;
            Vector3 posDirection = (3*transform.forward + transform.right).normalized;
            Collider wallCollider = null;

            // Shoot ray
            RaycastHit hit;
            if (Physics.Raycast(castOrigin, transform.forward, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, negDirection, out hit, angleRange, collisionExclusionMask)
                || Physics.Raycast(castOrigin, posDirection, out hit, angleRange, collisionExclusionMask))
            {
                onWall = true;
                wallCollider = hit.collider;

                if (drawGizmos)
                    Debug.DrawLine(castOrigin, hit.point, Color.red, d);
            } else if (drawGizmos) 
            {
                Debug.DrawLine(castOrigin, castOrigin + (transform.forward * range), Color.green, d);
                Debug.DrawLine(castOrigin, castOrigin + (negDirection * angleRange), Color.green, d);
                Debug.DrawLine(castOrigin, castOrigin + (posDirection * angleRange), Color.green, d);
            }
            #endregion Raycast

            #region Control states
            if ((!grounded && !climb.transitionFromGround || climb.transitionFromGround) && onWall && headspaceAvailable && moveInputs.y > 0 && movementState != MovementState.Climbing && lastClimbCollider != wallCollider)
            {
                lastClimbCollider = wallCollider;
                StartClimb();
            }
            else if (movementState == MovementState.Climbing && ((!onWall || moveInputs.y < 0.1 || !headspaceAvailable) || (!climb.transitionFromGround && grounded)))
            {
                ExitClimb();
            }
            #endregion Control states

            return onWall;
        }

        #endregion Collision

        #region Physics

        /// <summary>
        /// HandlePhysics
        /// Set and limit physics based on gravity, slopes, and maximum Velocity.
        /// <remarks>Should be called last in FixedUpdate.</remarks>
        /// </summary>
        void HandlePhysics()
        {

            #region Counter Physics

            // Gravity
            if ((int)movementState <= 6) // gravity states
            {
                if (!grounded && !onStep)
                    rb.AddForce(Vector3.down * (physics.gravityMultiplier - (onWall? physics.gravityMultiplier * physics.wallStick : 0)));
            }

            // Slopes
            if (grounded && normalAngle > physics.slopeAngle)
            {
                ProjectDownSlope(physics.slopeForce * (normalAngle % 45));
            }

            // Cap Player Max Velocity
            if (physics.limitVelocity)
            {
                Vector3 newVelocity = rb.velocity;
                Vector3 maxVelocity = physics.maxVelocity;

                if (Mathf.Abs(rb.velocity.x) > maxVelocity.x)
                    newVelocity.x = maxVelocity.x * Mathf.Sign(rb.velocity.x);
                if (Mathf.Abs(rb.velocity.z) > maxVelocity.z)
                    newVelocity.z = maxVelocity.z * Mathf.Sign(rb.velocity.z);
                if (Mathf.Abs(rb.velocity.y) > maxVelocity.y)
                    newVelocity.y = maxVelocity.y * Mathf.Sign(rb.velocity.y);

                rb.velocity = newVelocity;
            }

            #endregion Counter Physics

            previousVelocity = rb.velocity;
        }

        /// <summary>
        /// Adds downward force along slope normal.
        /// </summary>
        /// <param name="force">The force added.</param>
        void ProjectDownSlope(float force)
        {
            Vector3 slopeSideways = Vector3.Cross(Vector3.down, normal);
            Vector3 slopeDown = Vector3.Cross(normal, slopeSideways).normalized;
            rb.AddForce(slopeDown * force);
        }

        #endregion Physics

        /// <summary>
        /// HandleCamera
        /// Handles values driven to camera.
        /// </summary>
        void HandleCamera()
        {
            // Control FOV
            if (controlFOV && !Mathf.Approximately(FOV, targetFOV))
            {
                FOV = Mathf.Lerp(FOV, targetFOV, FOVsmoothing * d);
                cam.fieldOfView = FOV;
            }
        }

        #region Coroutines

        IEnumerator wallStickCoroutine;
        IEnumerator jumpGraceCoroutine;
        IEnumerator exitClimbCoroutine;

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartWallStickCoroutine()
        {
            StopWallStickCoroutine();
            wallStickCoroutine = YieldWallStickTime();
            StartCoroutine(wallStickCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopWallStickCoroutine()
        {
            if (exitClimbCoroutine != null)
                StopCoroutine(wallStickCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldWallStickTime()
        {
            yield return new WaitForSeconds(physics.wallStickDuration);
        }
        
        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartYieldJumpGraceCoroutine()
        {
            StopJumpGraceCoroutine();
            jumpGraceCoroutine = YieldJumpGraceTime();
            StartCoroutine(jumpGraceCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopJumpGraceCoroutine()
        {
            if (jumpGraceCoroutine != null)
                StopCoroutine(jumpGraceCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldJumpGraceTime()
        {
            yield return new WaitForSeconds(jump.gracePeriod);
            canJump = false;
        }

        /// <summary>
        /// YieldDashTime
        /// Waits for <code>FujinDashModule.duration</code> and resets movement.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldDashTime()
        {
            yield return new WaitForSeconds(dash.duration);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            ResetMovement();
        }

        /// <summary>
        /// Waits for <code>FujinDashModule.cooldownTime</code> and increments <code>numDashes</code>.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldDashCooldown()
        {
            numDashes--;
            yield return new WaitForSeconds(dash.cooldownTime);
            numDashes++;
        }

        /// <summary>
        /// Waits for <code>FujinPoundModule.duration</code> and resets movement.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldPoundTime()
        {
            yield return new WaitForSeconds(pound.duration);
            ResetMovement();
        }

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartExitClimbCoroutine()
        {
            StopExitClimbCoroutine();
            exitClimbCoroutine = YieldExitClimbTime();
            StartCoroutine(exitClimbCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopExitClimbCoroutine()
        {
            if (exitClimbCoroutine != null)
                StopCoroutine(exitClimbCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldExitClimbTime()
        {
            yield return new WaitForSeconds(climb.duration);
            ExitClimb();
        }

        #endregion Coroutines

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// SetTargetFOV
        /// Sets the target fov of the main camera to lerp towards.
        /// </summary>
        /// <param name="fov">the target fov</param>
        public void SetTargetFOV(float fov)
        {
            targetFOV = fov;
        }

        /// <summary>
        /// ControlFOV
        /// Turn off/on fov control of the main camera
        /// </summary>
        /// <param name="on">the control state toggle</param>
        public void ControlFOV(bool on)
        {
            controlFOV = on;
        }

        /// <summary>
        /// ControlHeight
        /// Turn off/on height control over the main camera and player collider
        /// </summary>
        /// <param name="on">the control state toggle</param>
        public void ControlHeight(bool on)
        {
            controlHeight = on;
        }

        /// <summary>
        /// [EDITOR ONLY]
        /// CreateNewMotor
        /// Spawns and initializes a new motor gameobject from the Create Asset menu under the active selection.
        /// </summary>
        [MenuItem("GameObject/3D Object/Player")]
        public static void CreateNewMotor()
        {
            Transform parent = null;
            if (Selection.gameObjects.Length > 0)
                parent = Selection.gameObjects[0].transform;

            GameObject obj = new GameObject();
            obj.AddComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            obj.AddComponent<Animator>();
            obj.AddComponent<FujinPlayerMotor>();
            obj.name = "Player";

            if (parent != null)
                obj.transform.parent = parent.transform;

            GameObject child = new GameObject();
            child.AddComponent<Camera>();
            child.transform.parent = obj.transform;

            Selection.activeGameObject = obj;
        }
        #endregion Public Methods
    }
}
