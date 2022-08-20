using System.Linq;
using System.Collections;
using System.Collections.Generic;
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

        #region Editor
        // Editor
        public float inputFadeAmount;
        public float featuresFadeAmount;
        public float morefeaturesFadeAmount;
        public float settingsFadeAmount;
        public float debugFadeAmount;

        public int tab;
        public bool inputFoldout;
        public bool featuresFoldout;
        public bool morefeaturesFoldout;
        public bool settingsFoldout;
        public bool debugFoldout = true;
        public bool hideMouseOnAwake = true;

        public Vector3 Velocity => previousVelocity;
        public Vector3 Position => previousPosition;
        public bool Grounded => grounded;
        public int GroundLayer => groundLayer;
        public float NormalAngle => normalAngle;
        public bool OnWall => onWall;
        public MovementState State => movementState;
        public bool drawGizmos;
        public float Speed => currSpeed;
        public int Jumps => numJumps;
        public int Dashes => numDashes;
        #endregion Editor

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
        public FujinAutorunModule autorun = new FujinAutorunModule();
        public FujinLeanModule lean = new FujinLeanModule();
        public FujinSlowwalkModule slowwalk = new FujinSlowwalkModule();
        public FujinFreelookModule freelook = new FujinFreelookModule();
        public FujinGrappleModule grapple = new FujinGrappleModule();
        public FujinEffectsModule effects = new FujinEffectsModule();

        [SerializeField]
        Transform cameraContainer;

        #endregion Components

        #region Instance

        public enum MovementState
        {
            Default = 0, //-------|
            Sprinting = 1, //-----| 
            Sliding = 2, //-------|
            Crouching = 3, //-----|States that use gravity
            Pounding = 5, //------|
            Dashing = 7,
            Mantling = 8,
            Climbing = 9,
            WallRunning = 10,
            Grappling = 11,
            Ziplining = 12,
            Hovering = 13
        }

        MovementState movementState = MovementState.Default;

        // Input Queue
        bool lookInput;
        bool jumpInput;
        bool sprintInput;
        bool slideInput;
        bool autoRunInput;
        bool slowWalkInput;
        bool leanInput;
        bool freeLookInput;

        bool enable;
        bool grounded;
        bool onStep;
        bool onWall;
        bool applyWallstick;
        bool cueStand;
        bool headspaceAvailable;
        bool controlTime = true;
        
        float t, d;
        float currSpeed;
        float normalAngle;
        float groundCheckRadius = 0.49f;
        float groundCheckDrawDistance = 0.05f;
        float collisionDrawOffset = 0.5f;
        float collisionThreshold = 0.01f;
        float heightOffset;
        float targetHeight;
        float defaultHeight;
        float heightSmoothing = 5;
        float time = 1;
        float targetTime = 1;
        float timeSmoothing = 10;
        float defaultFOV;
        float tiltRotation;
        float leanDirection;
        int groundLayer;
        bool previousWallCheck;

        // counters

        Vector2 cameraOffset;
        Vector3 normal;
        Vector3 previousVelocity;
        Vector3 previousPosition;

        LayerMask collisionExclusionMask;
        Collider lastClimbCollider;
        ParticleSystem.EmissionModule speedLinesEmission;
        CapsuleCollider col;
        Camera cam;
        Rigidbody rb;

        #endregion Instance

        #endregion Variables

        #region Input

#if ENABLE_INPUT_SYSTEM
        #region New Input
        void OnLookChange(InputAction.CallbackContext context)
        {
            lookInputs = context.ReadValue<Vector2>() 
                * (input.lookSensitivity - (input.lookSensitivity * lookResistance)) * 10 * d;
            Look();
        }

        void OnMoveChange(InputAction.CallbackContext context)
        {
            moveInputs = context.ReadValue<Vector2>();
        }

        void OnSprintPress(InputAction.CallbackContext context)
        {
            if (!sprint.enable)
                return;

            if (sprint.hold) // hold sprint
            {
                if (grounded)
                {
                    sprintInput = true;
                    if (movementState == MovementState.Crouching
                    || movementState == MovementState.Sliding)
                        cueStand = true;
                }
            }
            else // toggle sprint
            {
                if (movementState == MovementState.Sprinting) // reset movement but must account for auto run
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
                        if (movementState == MovementState.Crouching
                        || movementState == MovementState.Sliding)
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

            if (jump.hover.enable && canHover && !grounded && numJumps == 0)
                StartHover();
            else
                jumpInput = true;
        }
        
        void OnJumpRelease(InputAction.CallbackContext context)
        {
            if (!jump.enable || !jump.hover.enable || movementState != MovementState.Hovering)
                return;

            canHover = false;
            ResetMovement();
        }

        void OnCrouchPress(InputAction.CallbackContext context)
        {
            if (!crouch.enable && !slide.enable)
                return;

            if (((crouch.enable && !crouch.hold) || (!crouch.enable && !slide.hold))
                && (movementState == MovementState.Crouching || movementState == MovementState.Sliding))
            {
                cueStand = true;
                if (slide.enable)
                    slideInput = false;
            }
            else
            {
                if (slide.enable)
                    slideInput = true;
                if (!grounded)
                    return;

                if (crouch.enable && (!slide.enable || (slide.enable && movementState != MovementState.Sprinting)))
                {
                    Crouch();
                }
                else if (slide.enable)
                {
                    Vector3 direction = transform.forward;
                    if (slide.multidirectional)
                    {
                        Vector3 newDirection = (transform.forward * moveInputs.y + transform.right * moveInputs.x).normalized;
                        if (newDirection != Vector3.zero)
                            direction = newDirection;
                    }

                    StartSlide(direction);
                }
            }
        }

        void OnCrouchRelease(InputAction.CallbackContext context)
        {
            if (((crouch.enable && crouch.hold) || (!crouch.enable && slide.hold))
               && (movementState == MovementState.Crouching || movementState == MovementState.Sliding))
            {
                cueStand = true; 
                if (slide.enable)
                    slideInput = false;
            }
        }

        void OnDashPress(InputAction.CallbackContext context)
        {
            if (!dash.enable || (!grounded && !dash.canDashInAir) || movementState > MovementState.Sprinting || numDashes == 0)
                return;

            if (dash.slowTime.enable) 
            {
                targetTime = 1 * dash.slowTime.timeMultiplier;
                timeSmoothing = 1 * dash.slowTime.transistionSpeed;
            } else
                StartDash();
        }

        void OnDashRelease(InputAction.CallbackContext context)
        {
            if (!dash.enable || (!grounded && !dash.canDashInAir) || movementState > MovementState.Sprinting || numDashes == 0)
                return;

            if (dash.slowTime.enable) 
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
            if (!autorun.enable || !grounded)
                return;

            autoRunInput = !autoRunInput;
        }

        void OnLeanPress(InputAction.CallbackContext context)
        {
            if (!lean.enable || !grounded || movementState == MovementState.Sprinting)
                return;

            Lean(context.ReadValue<float>());
        }

        void OnLeanRelease(InputAction.CallbackContext context)
        {
            if (!lean.enable || !lean.hold)
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
            if (!slowwalk.enable || !grounded || movementState > MovementState.Crouching 
                || movementState == MovementState.Sprinting || movementState == MovementState.Sliding)
                return;

            if (!slowwalk.hold && slowWalkInput)
            {
                slowWalkInput = false;
                if (crouch.enable && targetHeight == crouch.height) 
                {
                    Crouch();
                    return;
                }

                ResetMovement();
            }
            else
            {
                slowWalkInput = true;
            }
        }

        void OnSlowWalkRelease(InputAction.CallbackContext context)
        {
            if (!slowwalk.enable || !slowwalk.hold || !grounded || !slowWalkInput)
                return;

            slowWalkInput = false;
            if (crouch.enable && targetHeight == crouch.height)
            {
                Crouch();
                return;
            }

            ResetMovement();
        }

        void OnGrapplePress(InputAction.CallbackContext context)
        {
            if (!grapple.enable)
                return;

            if (movementState == MovementState.Grappling)
            {
                EndGrapple();
            }
            else if (movementState <= MovementState.Crouching && !throwGrapple && canGrapple)
                StartThrowGrapple();
        }

        // Initilize Input
        void InitInput()
        {
            input.lookAction.performed += OnLookChange;
            input.moveAction.performed += OnMoveChange;
            input.moveAction.canceled += OnMoveChange;
            input.jumpAction.performed += OnJumpPress;
            input.jumpAction.canceled += OnJumpRelease;
            input.sprintAction.performed += OnSprintPress;
            input.sprintAction.canceled += OnSprintRelease;
            input.crouchSlideAction.performed += OnCrouchPress;
            input.crouchSlideAction.canceled += OnCrouchRelease;
            input.dashAction.performed += OnDashPress;
            input.dashAction.canceled += OnDashRelease;
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
            input.enable = true;
            enable = true;
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
            input.enable = false;
            enable = false;

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
            col.material = physics.physicMaterial;

            currSpeed = movement.speed;
            numJumps = jump.doubleJump.amount;
            numDashes = dash.count;
            defaultHeight = cameraContainer.localPosition.y;
            heightOffset = col.height * 2 / defaultHeight;
            targetHeight = defaultHeight;
            cameraOffset.y = defaultHeight;
            defaultFOV = cam.fieldOfView;
            collisionExclusionMask = ~LayerMask.GetMask("Player");
            rb.useGravity = false;

            if (grapple.enable) // Check grapple
            {
                if (grapple.prefab.GetComponent<MeshFilter>() == null)
                    Debug.LogError("[Error] Grapple requires component of type <MeshFilter>.");
                else if (grapple.prefab.GetComponent<MeshRenderer>() == null)
                    Debug.LogError("[Error] Grapple requires component of type <MeshRenderer>.");
            }

            if (effects.enable) // INIT EFFECTS
            {
                if (effects.speedlines.enable) // Instantiate Speedlines Particle System
                {
                    if (effects.speedlines.prefab == null)
                    {
                        Debug.LogError("[ERROR] Speed lines prefab cannot be null.");
                        return;
                    }

                    ParticleSystem p = Instantiate(effects.speedlines.prefab, cam.transform).GetComponent<ParticleSystem>();
                    p.transform.localPosition = Vector3.forward;
                    speedLinesEmission = p.emission;
                    var m = p.main;
                    m.startColor = effects.speedlines.gradient;
                    ParticleSystemRenderer renderer = p.GetComponent<ParticleSystemRenderer>();
                    renderer.lengthScale = 5 + (45 * effects.speedlines.thiness); // clamps thickness between 10 and 50
                    ParticleSystem.ShapeModule shape = p.shape;
                    shape.radius = renderer.lengthScale + effects.speedlines.radius;
                }
            }

            // Initilize Functions
            if (input.enable)
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
            HandleTime();
            HandleInput();
            HandleAnims();
            HandleCollisions();
        }

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void FixedUpdate()
        {
            t = Time.time;
            d = Time.deltaTime;
            HandleHeight();
            HandleEffects();

            HandleMovementStates();
            HandleInputQueue();
            HandleOther();
            CounterPhysics();
        }

        void LateUpdate()
        {
            if (throwGrapple || movementState == MovementState.Grappling)
                StretchGrapple();
        }

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void OnDestroy()
        {
            // Clean up 
            if (grapple.enable && grappleObj != null)
                Destroy(grappleObj);
        }

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            Color cyan = Color.cyan;
            Color red = Color.red;
            cyan.a = 0.3f;
            red.a = 0.3f;

            // Direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + normal);

            // Ground Check
            Gizmos.color = cyan;
            if (grounded)
                Gizmos.color = red;
            Vector3 origin = transform.position + Vector3.up * groundCheckRadius;
            Gizmos.DrawSphere(origin + (Vector3.down * groundCheckDrawDistance), groundCheckRadius);

            // Step Check
            Gizmos.color = cyan;

            // Wall Check
            Gizmos.color = cyan;
            { 
                // Mantle Check
                // Climb Check
                // Wall stick check
            }

            // Grapple chck
            Gizmos.color = cyan;
            if (throwGrapple || movementState == MovementState.Grappling) 
            {
                Gizmos.DrawIcon(grappleObj.transform.position, "grappling-hook.png");

                if (movementState == MovementState.Grappling)
                    Gizmos.color = red;
                Gizmos.DrawLine(grapple.pivot.position, grappleObj.transform.position);
            }

            Gizmos.color = cyan;
        }

        #endregion Engine

        #region Private Methods

        #region Actions

        #region Look

        float yRot;
        float xRot;
        float lookResistance;

        Vector2 lookInputs;
        Vector2 lookOffset;

        /// <summary>
        /// Look
        /// Rotates <code>cameraContainer</code> from mouseDelta.
        /// </summary>
        void Look()
        {
            if (freelook.enable)
            {
                if (freeLookInput)
                {
                    lookOffset.y -= lookInputs.y * input.lookAxis.y * freelook.axis.y;
                    lookOffset.x += lookInputs.x * input.lookAxis.x * freelook.axis.x;
                    float y = Mathf.Clamp(yRot + lookOffset.y, -freelook.verticalAngle, freelook.verticalAngle);
                    float x = Mathf.Clamp(lookOffset.x, -freelook.horizontalAngle, freelook.horizontalAngle);
                    lookOffset.y = y - yRot;
                    lookOffset.x = x;
                    cam.transform.localRotation = Quaternion.Euler(y, x, cameraContainer.localEulerAngles.z);
                }
                else
                {
                    yRot -= lookInputs.y * input.lookAxis.y;
                    yRot = Mathf.Clamp(yRot, -90, 90);
                    xRot += lookInputs.x * input.lookAxis.x;

                    cameraContainer.localRotation = Quaternion.Euler(yRot + lookOffset.y, lookOffset.x, cameraContainer.localEulerAngles.z);
                    rb.MoveRotation(Quaternion.Euler(0, xRot, 0));
                }
            }
            else
            {
                yRot -= lookInputs.y * input.lookAxis.y;
                yRot = Mathf.Clamp(yRot, -90, 90);
                xRot += lookInputs.x * input.lookAxis.x;

                cameraContainer.localRotation = Quaternion.Euler(yRot, cameraContainer.localEulerAngles.y, cameraContainer.localEulerAngles.z);
                rb.MoveRotation(Quaternion.Euler(0, xRot, 0));
            }
        }

        #endregion Look

        #region Move
        Vector2 moveInputs;

        /// <summary>
        /// Move
        /// Applies a movement force to the player based on <code>moveInputs</code>.
        /// If <code>!grounded</code> use <code>airDrag</code> to Lerp along <code>moveInputs</code>.
        /// Else project along normal vector and Lerp using <code>movementAcceleration && movementDecceleration</code>.
        /// </summary>
        void TryMove()
        {
            // movement variables
            #region Direction
            Vector3 direction;

            if (autoRunInput) // Auto Run 
                direction = (Mathf.Min(1 + moveInputs.y, 1) * transform.forward + moveInputs.x * transform.right).normalized;
            else
                direction = (moveInputs.y * transform.forward + moveInputs.x * transform.right).normalized;

            if (grounded)
                direction = Vector3.ProjectOnPlane(direction, normal); // project onto normal if grounded

            #endregion Direction

            #region Auxillary

            if (rb.velocity.magnitude > 0)
                Tilt(movement.tiltAngle, movement.tiltSpeed);

            #endregion Auxillary

            #region Apply

            if (grounded)
                Move(direction);
            else 
                AirMove(direction);

            #endregion Apply
        }

        void Move(Vector3 direction)
        {
            Vector3 velocity = Vector3.ClampMagnitude(rb.velocity - (rb.velocity.normalized * physics.friction), rb.velocity.magnitude);
            float speed = Vector3.Dot(velocity, direction);
            speed = Mathf.Clamp(currSpeed - speed, 0,  movement.acceleration);

            rb.velocity = velocity + speed * direction;
        }

        void AirMove(Vector3 direction)
        {
            float speed = Vector3.Dot(rb.velocity, direction);
            speed = Mathf.Clamp(movement.airSpeed - speed, 0, movement.airAcceleration);
            Vector3 velocity = rb.velocity + speed * direction;

            rb.velocity = velocity;
        }

        #endregion Move

        #region Jump
        bool canJump = true;
        bool canHover = true;
        int numJumps;

        /// <summary>
        /// Jump
        /// Applies upward force to player. Additional horizontal force is calculated by state.
        /// IF <code>enableDoubleJumps</code> and <code>!grounded</code>, cancel latest velocity.
        /// </summary>
        void Jump()
        {
            jumpInput = false;
            float force = jump.force;
            Vector3 direction = Vector3.up;

            #region Double Jump & Wall Jump

            //  Double jump if not grounded and enabled
            if (!canJump)
            {
                if (!grounded && onWall && jump.wallJump.enable)
                {
                    force = jump.wallJump.force;
                    //direction = (-transform.forward + Vector3.up).normalized;
                }
                else if (!grounded && jump.doubleJump.enable && numJumps > 0)
                {
                    --numJumps;
                    force = jump.doubleJump.force;

                    // double jump direction
                    Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    Vector3 input = 
                        (moveInputs.x * transform.right + moveInputs.y * transform.forward).normalized;

                    float dot = Vector3.Dot(velocity.normalized, input);
                    if (dot < 1)
                    {
                        float facing = (2 - (dot + 1)) / 2f;
                        float inputForce = currSpeed * facing;
                        float counterForce = jump.doubleJump.redirect * facing;
                        rb.velocity += inputForce * input - counterForce * velocity;
                    }
                }
                else
                    return;
            }

            #endregion Double Jump & Wall Jump

            #region State Control

            // Calculate jump direction from movement state
            if (movementState == MovementState.Mantling || movementState == MovementState.Dashing)
                return;

            if (movementState == MovementState.Crouching)
                Stand();

            if (jump.wallJump.enable && onWall)
            {
                if (movementState == MovementState.Climbing)
                {
                    ExitClimb();
                }
            }
            else if (movementState == MovementState.Pounding || movementState == MovementState.Grappling)
            {
                ResetMovement();
            }

            #endregion State Control

            #region Apply Force

            // Reset y velocity and add forces

            if (rb.velocity.y < 0)
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(direction * force, ForceMode.Impulse);
            StopJumpGraceCoroutine();

            #endregion Apply Force
        }

        void StartHover() 
        {
            StartYieldJumpHoverCoroutine();
            movementState = MovementState.Hovering;
        }

        void Hover() 
        {
            Vector3 velocity = new Vector3(rb.velocity.x, -jump.hover.fallForce, rb.velocity.z);
            rb.velocity = velocity;
        }

        #endregion Jump

        #region Sprint

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
            currSpeed = movement.speed * sprint.speedMultiplier;
            if (movementState == MovementState.Crouching
                || movementState == MovementState.Sliding)
                cueStand = true;
            movementState = MovementState.Sprinting;
        }

        #endregion Sprint

        #region Crouch

        /// <summary>
        /// Crouch
        /// Sets <code>movementState</code> to <code>MovementState.Crouching</code> and associated variables.
        /// </summary>
        void Crouch()
        {
            currSpeed = movement.speed * crouch.speedMultiplier;
            targetHeight = crouch.height;
            movementState = MovementState.Crouching;
        }

        #endregion Crouch

        #region Slide

        float slideSpeed;
        Vector3 slideDirection;

        /// <summary>
        /// StartSlide
        /// Sets <code>movementState</code> to <code>MovementState.Sliding</code> and associated variables.
        /// </summary>
        void StartSlide(Vector3 direction)
        {
            targetHeight = slide.height;
            slideDirection = Vector3.ProjectOnPlane(direction, normal).normalized;
            rb.AddForce(slideDirection * (slide.boosted ? 0 : slide.boost), ForceMode.Impulse);
            movementState = MovementState.Sliding;
            StartSlideBoostCoroutine();
        }

        /// <summary>
        /// Slide
        /// Adds slide forces to player.
        /// </summary>
        void Slide()
        {
            if (slide.tiltAngle > 0)
                Tilt(slide.tiltAngle, slide.tiltSpeed);

            if (!grounded)
                return;

            // Slide Control
            float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
            slideSpeed = Mathf.Lerp(slideSpeed, moveInputs.x, slide.control * d);
            Vector3 direction = slideDirection + transform.right * slideSpeed;
            Vector3 velocity = direction.normalized * magnitude;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;

            // Slide Friction
            rb.velocity += -rb.velocity.normalized * slide.friction;

            // Slide down slopes 
            if (normalAngle > 0)
            {
                ProjectDownSlope(Mathf.Min(normalAngle / physics.slopeAngle, 1) * slide.slopeAcceleration * 100);
            }

            // If force is decellarating and less than threshold, stop
            if ((Mathf.Abs(previousVelocity.x) > Mathf.Abs(rb.velocity.x) && Mathf.Abs(previousVelocity.z) > Mathf.Abs(rb.velocity.z))
                && (Mathf.Abs(rb.velocity.x) < 2f && Mathf.Abs(rb.velocity.z) < 2f))
            {
                if (crouch.enable)
                    Crouch();
                else
                    Stand();
            }
        }

        #endregion Slide

        #region Dash

        int numDashes;
        Vector3 dashDirection;

        /// <summary>
        /// StartDash
        /// Sets <code>movementState</code> to <code>MovementState.Dashing</code> and inits state.
        /// </summary>
        void StartDash()
        {
            // Calculate Dash Direction based on input and axis
            Vector3 direction = Vector3.zero;

            if (dash.slowTime.enable) 
            {
                targetTime = 1;
                timeSmoothing = 10;
            }

            if (dash.omnidirectional)
            {
                direction = cam.transform.forward;
            }
            else
            {
                direction += transform.forward * (moveInputs.y > 0.1f && dash.axis.z > 0 ? 1 : 0);
                direction += -transform.forward * (moveInputs.y < -0.1f && dash.axis.w > 0 ? 1 : 0);
                direction += transform.right * (moveInputs.x > 0.1f && dash.axis.x > 0 ? 1 : 0);
                direction += -transform.right * (moveInputs.x < -0.1f && dash.axis.y > 0 ? 1 : 0);
            }

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
            if (grounded && !dash.omnidirectional)
                velocity = Vector3.ProjectOnPlane(dashDirection, normal);

            velocity = dash.force * 10 * velocity.normalized;
            rb.velocity = velocity;
        }

        #endregion Dash

        #region Pound

        bool canPound = true;
        float poundHeight;
        Vector3 poundOrigin;

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

        #endregion Pound

        #region Mantle

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

        #endregion Mantle

        #region Climb

        /// <summary>
        /// StartClimb
        /// Sets <code>movementState</code> to <code>MovementState.Climbing</code> and inits state.
        /// </summary>
        void StartClimb()
        {
            rb.velocity = Vector3.zero;
            StartExitClimbCoroutine();

            numJumps = jump.doubleJump.amount;
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

        #endregion Climb

        #region Lean

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
        /// Resets lean direction and state speed.
        /// </summary>
        void StopLean()
        {
            leanInput = false;
            leanDirection = 0;

            if (movementState == MovementState.Default)
                currSpeed = movement.speed;
            else if (movementState == MovementState.Crouching)
                currSpeed = movement.speed * crouch.speedMultiplier;
        }

        #endregion Lean

        #region Grapple

        bool canGrapple = true;
        bool throwGrapple;
        bool returnGrapple;
        float grappleBreakTimer;
        Vector3 grapplePoint;
        Vector3 previousGrapplePosition;

        GameObject grappleObj;
        Mesh grappleMesh;

        /// <summary>
        /// StartGrapple
        /// Sets <code>movementState</code> to <code>MovementState.ThrowingGrapple</code> and inits state. This includes spawning and initializing the grapple object.
        /// </summary>
        void StartThrowGrapple()
        {
            grappleObj = Instantiate(grapple.prefab, grapple.pivot.position, grapple.pivot.rotation);
            grapplePoint = cam.transform.position + grapple.range * cam.transform.forward;
            grappleMesh = grappleObj.GetComponent<MeshFilter>().mesh;
            throwGrapple = true;
        }

        /// <summary>
        /// ThrowGrapple
        /// Monitors the distance between <code>grapplePoint</code> and the <code>grappleObj</code>. Controls the direction of the grapple accordingly.
        /// </summary>
        void ThrowGrapple()
        {
            Vector3 origin = grapple.pivot.position;
            float distance = Vector3.Distance(grapplePoint, grappleObj.transform.position);
            Vector3 direction = (grapplePoint - grappleObj.transform.position).normalized;

            if (returnGrapple) // grapple has returned
            {
                grapplePoint = origin;

                if (distance < 1)
                {
                    EndGrapple();
                    return;
                }
            } 
            else if (distance < 1) // grapple surpasses grapple range
            {
                returnGrapple = true;
                grapplePoint = origin;
            }

            float speed = grapple.throwSpeed * 10;
            Vector3 rotation = (grappleObj.transform.position - grapple.pivot.position).normalized;
            if (rotation != Vector3.zero)
                grappleObj.transform.rotation = Quaternion.LookRotation(rotation, Vector3.up);
            grappleObj.transform.Translate(grappleObj.transform.InverseTransformDirection(direction) * speed * d); // move grapple
            CheckGrapple(); // check for collisions
        }

        /// <summary>
        /// 
        /// </summary>
        void StartGrapple()
        {
            Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            float speed = velocity.magnitude;

            if (speed > grapple.speed) 
            {
                float angle =
                        Vector3.Angle((grapple.pivot.position + cam.transform.forward).normalized, grappleObj.transform.position);

                float clamp = grapple.speed;
                if (angle < 90) 
                {
                    float diff = speed - grapple.speed;
                    clamp = speed - (diff * angle / 90);
                }

                velocity = Vector3.ClampMagnitude(velocity, clamp);
            }

            if (grapple.impactEffect != null)
                Instantiate(grapple.impactEffect, grappleObj.transform.position, grappleObj.transform.rotation);
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); ;
            grapplePoint = grappleObj.transform.position;
            canGrapple = false;
            numJumps = jump.doubleJump.amount;
            movementState = MovementState.Grappling;
            throwGrapple = false;
        }

        /// <summary>
        /// Grapple
        /// Calls <code>StretchGrapple</code> and applys grapple force along grapple direction vector.
        /// </summary>
        void Grapple()
        {
            float distance = Vector3.Distance(grapplePoint, cam.transform.position);
            Vector3 targetDirection = (grapplePoint - grapple.pivot.position).normalized;

            //#region Wrap
            //if (grapple.wrap)
            //{
            //    // Wrap Grapple
            //    RaycastHit hit;
            //    if (Physics.Raycast(grapple.pivot.position, targetDirection, out hit, distance, collisionExclusionMask)
            //        && Vector3.Distance(hit.point, grapplePoint) > 1) // return if wrapping is too close.
            //                                                          // This doubles as a threshold and prevents wonky behavior from very small wraps.
            //    {
            //        // Create new grapple point
            //        grapplelineRenderer.positionCount += 1;

            //        // offset new point from collision
            //        Vector3 position = hit.point + .2f * hit.normal;

            //        // Set line renderer
            //        grapplelineRenderer.SetPosition(grapplelineRenderer.positionCount - 2, position);
            //        grapplelineRenderer.SetPosition(grapplelineRenderer.positionCount - 1, grapple.pivot.position);
            //        grapplePoint = grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 2);
            //    }

            //    // Straighten Grapple
            //    else if (grapplelineRenderer.positionCount > 2)
            //    {
            //        Vector3 line1 = (grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 2)
            //            - grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 1)).normalized;

            //        Vector3 line2 = (grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 3)
            //            - grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 2)).normalized;

            //        float range = Vector3.Distance(grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 1),
            //            grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 2)) + 1f;
            //        float facing = Vector3.Dot(line1, line2);

            //        //// Remove grapple point
            //        if ((!Physics.Raycast(grapple.pivot.position, line1, range, collisionExclusionMask)
            //            && facing > 0.9f) || Vector3.Distance(grapplePoint, grapple.pivot.position) < 0.1f) // if no longer colliding and roughly in same direction
            //        {
            //            grapplelineRenderer.positionCount -= 1;
            //            grapplelineRenderer.SetPosition(grapplelineRenderer.positionCount - 1, grapple.pivot.position);
            //            grapplePoint = grapplelineRenderer.GetPosition(grapplelineRenderer.positionCount - 2);
            //        }
            //    }
            //}
            //#endregion Wrap

            #region Physics
            // Grapple Physics

            // direction
            Vector3 counterDirection = (cam.transform.forward * grapple.viewControl) + ((transform.right * moveInputs.x) * grapple.inputControl);
            Vector3 direction = (targetDirection + counterDirection).normalized;

            // elasticity
            float velocityDot = Vector3.Dot(rb.velocity.normalized, targetDirection);
            if (velocityDot < 0) 
            {
                rb.velocity -= rb.velocity.magnitude * (1 - grapple.elasticity) * velocityDot * targetDirection;
            }
            float directionDot = Vector3.Dot(direction, targetDirection);
            if (directionDot < 0) 
            {
                direction -= (1 - grapple.elasticity) * directionDot * targetDirection;
            }

            // Tilt
            float tilt = grapple.tilt * -Mathf.Abs(velocityDot);
            Tilt(tilt, 1);

            // Move Player at angle between grapple and camera
            float speed = Vector3.Dot(rb.velocity, direction);
            speed = Mathf.Clamp(grapple.speed - speed, 0, grapple.acceleration);
            rb.velocity += speed * direction;
            #endregion Physics

            #region End Conditions
            // End Conditions
            float delta = Vector3.Distance(transform.position, previousGrapplePosition);
            previousGrapplePosition = transform.position;
            if (delta < 0.1f)
                grappleBreakTimer += d;
            else
                grappleBreakTimer = 0;

            float angle = Vector3.Angle(targetDirection, cam.transform.forward);
            if (angle > 180)
                angle -= 180;

            distance = Vector3.Distance(grappleObj.transform.position, cam.transform.position);
            if (grappleBreakTimer > 1f || angle > grapple.maxAngle 
                || distance < 1f || distance > grapple.range + 1f)// ||
                //grapplelineRenderer.positionCount > grapple.wrapCount + 2)
            // End grpple if angle > 90, grapple blocked, about to collide, or moving too slow.
            {
                if (distance < 1f) // reset velocity if about to collide
                    rb.velocity = Vector3.zero;
                EndGrapple();
            }
            #endregion End Conditions
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
            if (Physics.Raycast(castOrigin, direction, out hit, distance, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            {
                grappleObj.transform.position = hit.point;
                StartGrapple();
            }
        }

        /// <summary>
        /// StretchGrapple
        /// Moves the <code>grappleObj</code> along the grapple direction and sets the <code>grappleLineRenderer</code> points to the objects position and grapple origin. Must be called in <code>LateUpdate</code>.
        /// </summary>
        void StretchGrapple()
        {
            List<Vector3> vertices = new List<Vector3>();

            // Grappling
            if (movementState == MovementState.Grappling 
                || grapple.simType == FujinGrappleModule.SimulationType.None)
            {
                vertices.Add(grappleObj.transform.InverseTransformPoint(grapple.pivot.position));
                vertices.Add(Vector3.zero);
            }

            // Throwing Grapple
            // Draw Sim
            else
            {
                float length = Vector3.Distance(grappleObj.transform.position, grapple.pivot.position);
                Vector3 direction = (grappleObj.transform.position - grapple.pivot.position).normalized;
                Vector3 up = (direction == Vector3.zero ?
                    Quaternion.identity :
                    Quaternion.LookRotation(direction))
                    * Vector3.up;

                for (int i = 0; i < grapple.quality + 1; i++)
                {
                    Vector3 horizontal;
                    Vector3 vertical;

                    // Wobble
                    if (grapple.simType == FujinGrappleModule.SimulationType.Wobble)
                    {
                        float x = i / (float)grapple.quality * length;
                        horizontal = grapple.pivot.position + direction * x;

                        float percent = 1 - (length / grapple.range);
                        float a = grapple.wobbleWaveHeight * Mathf.Sin(percent * grapple.wobbleAmount * Mathf.PI);
                        float y = a * Mathf.Sin(grapple.wobbleWaveCount * i / grapple.quality * 2 * Mathf.PI * percent);
                        vertical = up * y;
                    } 

                    // Spiral
                    else
                    {
                        float increment = i/(float)grapple.quality;
                        float percent = 1 - (length / grapple.range);
                        float x = i / (float)grapple.quality * length;
                        float rotation = 1 + (grapple.spiralRotationSpeed * Mathf.PI * t);
                        float time = grapple.spiralAmount * Mathf.PI * increment;
                        float damper =  1 - (grapple.spiralDamper * increment);

                        float offset = grapple.spiralOffset * damper * percent * Mathf.Sin(rotation);
                        float xOffset = offset * Mathf.Cos(time);
                        float yOffset = offset * Mathf.Sin(time);

                        horizontal = grapple.pivot.position + xOffset * grapple.pivot.right + x * direction;
                        vertical = yOffset * up;
                    }

                    vertices.Add(grappleObj.transform.InverseTransformPoint(horizontal + vertical));
                }
            }

            FujinMeshGenerator.GenerateRope(grapple.sides, grapple.thickness, vertices.ToArray(), grappleMesh, new Vector2(100, 100));
        }

        /// <summary>
        /// EndGrapple
        /// Destroys the <code>grappleObj</code> and resets movement state.
        /// </summary>
        void EndGrapple()
        {
            if (movementState == MovementState.Grappling)
            {
                StartCoroutine(YieldGrappleCooldown());
                if (grapple.breakEffect != null)
                    Instantiate(grapple.breakEffect, grappleObj.transform.position, grappleObj.transform.rotation);
            } else 
            {
                canGrapple = true;
            }

            if (slideInput) 
            {
                ResetMovement();
                slideInput = true;
            } else
                Stand();

            throwGrapple = false;
            returnGrapple = false;
            Destroy(grappleObj);
        }
        #endregion Grapple

        #region Utils
        /// <summary>
        /// Stand
        /// Similar to <code>ResetMovement</code>. Used carry input when transitioning from <code>movementState == MovementState.Crouching || movementState == MovementState.Sliding</code>
        /// </summary>
        void Stand()
        {
            bool slowWalk = slowWalkInput;
            bool autoRun = autoRunInput;

            ResetMovement();

            slowWalkInput = slowWalk;
            autoRunInput = autoRun;
            targetHeight = defaultHeight;
            slideInput = false;

            if (sprintInput)
                Sprint();
        }

        /// <summary>
        /// ResetMovement
        /// Resets <code>movementState</code> and associated variables to default.
        /// </summary>
        void ResetMovement()
        {
            jumpInput = false;
            cueStand = false;
            slowWalkInput = false;
            autoRunInput = false;
            slideSpeed = 0;

            currSpeed = movement.speed;
            movementState = MovementState.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="speed"></param>
        void Tilt(float angle, float speed)
        {
            // Movement Tilt
            if (!leanInput && rb.velocity.magnitude > 0) // have to check if leaning since it also affects tilt
            {
                tiltRotation = moveInputs.x * -angle;
                Vector3 targetRotation = cameraContainer.localEulerAngles;
                targetRotation.z = tiltRotation;
                cameraContainer.localRotation = Quaternion.Lerp(cameraContainer.localRotation, Quaternion.Euler(targetRotation), speed * d);
            }
        }
        #endregion Utils

        /// <summary>
        /// HandleInputQueue
        /// Reserved for actions that cannot be executed immediately or rely on conditions being executed. These conditions may include functions that require a variable be met first, or simply physics-based functions being passed to execute in Fixed-Update.
        /// </summary>
        void HandleInputQueue()
        {
            if (jumpInput)
                Jump();

            if (cueStand && headspaceAvailable && grounded)
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
                    if ((moveInputs.y < 0.1f || moveInputs.x != 0) && !autoRunInput) // account for no input or auto run
                        ResetMovement();
                    else
                        TryMove();
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
                case MovementState.Grappling:
                    Grapple();
                    break;
                case MovementState.Hovering:
                    TryMove();
                    Hover();
                    break;
                default:
                    TryMove();
                    break;
            }

            #endregion Movement States
        }

        void HandleOther()
        {
            // SPRINTING
            // Note: this is a 'core' state, however sprintInput has to be kept track over multiple states.
            // we handle this below.
            if (sprint.enable && sprintInput && ((moveInputs.y > 0.1f && moveInputs.x == 0) || autoRunInput)
                && movementState == MovementState.Default)
                Sprint();

            // SLOW WALK
            if (slowwalk.enable && slowWalkInput) 
            {
                float speed = movement.speed;
                if (movementState == MovementState.Crouching)
                    speed = movement.speed * crouch.speedMultiplier;

                currSpeed = speed * slowwalk.speedMultiplier;
            }
                

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
                        
                        if (slowWalkInput) 
                        {
                            if (movementState == MovementState.Default)
                                currSpeed = movement.speed * lean.speedMultiplier * slowwalk.speedMultiplier;
                            else if (movementState == MovementState.Crouching)
                                currSpeed = movement.speed * crouch.speedMultiplier * lean.speedMultiplier * slowwalk.speedMultiplier;
                        }

                        else 
                        {
                            if (movementState == MovementState.Default)
                                currSpeed = movement.speed * lean.speedMultiplier;
                            else if (movementState == MovementState.Crouching)
                                currSpeed = movement.speed * crouch.speedMultiplier * lean.speedMultiplier;
                        }
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
                    cam.transform.localRotation = Quaternion.Euler(lookOffset.y, lookOffset.x, 0);
                }
            }

            // GRAPPLE
            // Note: this is a 'core' state, however various actions can cause the grapple to not self-delete properly
            if (grapple.enable)
            {
                if (throwGrapple)
                {
                    ThrowGrapple(); // execute throw
                }
                else if (grappleObj != null &&
                    movementState != MovementState.Grappling)
                    EndGrapple(); // clean up
            }
        }

        void HandleAnims() 
        {
        
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

            previousWallCheck = onWall;
            if (mantle.enable && CheckMantle())
                return;

            if (climb.enable && CheckClimb())
                return;

            CheckStep();
            CheckWall();
        }

        /// <summary>
        /// CheckGround
        /// Checks for ground collision via a <code>SphereCast</code>. Calculates new normal vectors and handles instanse properties on collision.
        /// </summary>
        void CheckGround()
        {
            Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + collisionThreshold);
            Vector3 direction = Vector3.down;

            RaycastHit hit;
            if (Physics.SphereCast(origin, groundCheckRadius, direction, out hit, groundCheckDrawDistance, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 contactPoint = origin + (direction * groundCheckDrawDistance);
                Vector3 contactDirection = hit.point - origin;
                float contactOffset = contactDirection.magnitude + collisionThreshold;
                groundLayer = hit.collider.gameObject.layer;

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
                    if (slideInput || movementState == MovementState.Sliding)
                    {
                        direction = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                        float magnitude = direction.magnitude;
                        if (magnitude < slide.boost)
                        {
                            direction = transform.forward;
                        }

                        StartSlide(direction);
                    }
                    else if (sprintInput && movementState != MovementState.Sprinting)
                        Sprint();
                    else if (!sprintInput && movementState == MovementState.Sprinting)
                        ResetMovement();
                    else if (movementState == MovementState.Pounding)
                        EndPound();
                    else if (movementState == MovementState.Hovering)
                        StopJumpHoverCoroutine();

                    if (jump.enable)
                        StopJumpGraceCoroutine();

                    canJump = true;
                    canHover = true;
                    canPound = true;
                    numJumps = jump.doubleJump.amount;
                    lastClimbCollider = null;
                    applyWallstick = false;
                    grounded = true;
                }
            }
            else if (grounded)
            {
                grounded = false;
                if (jump.enable)
                    StartYieldJumpGraceCoroutine();
            }
        }

        /// <summary>
        /// CheckHeight
        /// Checks for head collisions and handles updates to height.
        /// </summary>
        void CheckHeight()
        {
            // Check Headspace
            float maxHeight = (defaultHeight * heightOffset / 2); // the players maximum height 
            float minHeight = 4 * col.radius / heightOffset; // the players minimum height - will use this later (limited by collider radius)

            float range = maxHeight + collisionThreshold;
            headspaceAvailable = !Physics.Raycast(transform.position, Vector3.up, range, collisionExclusionMask);
        }

        /// <summary>
        /// CheckStep
        /// Checks for collisions at step height. Projects the player along step normal accordingly.
        /// </summary>
        void CheckStep()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void CheckWall()
        {
            onWall = false;

            #region Raycast
            float range = collisionDrawOffset + collisionThreshold;
            Vector3 castOrigin = transform.position + Vector3.up * cameraOffset.y;

            // Shoot ray
            RaycastHit hit;
            if (Physics.Raycast(castOrigin, transform.forward, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, -transform.forward, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, transform.right, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, -transform.right, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, (transform.forward + transform.right).normalized, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, (transform.forward - transform.right).normalized, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, (-transform.forward + transform.right).normalized, out hit, range, collisionExclusionMask)
                || Physics.Raycast(castOrigin, (-transform.forward - transform.right).normalized, out hit, range, collisionExclusionMask))
            {
                onWall = true;
            }
            #endregion Raycast
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
            Vector3 castOrigin = transform.position + Vector3.up * (physics.stepHeight > 0.5f ? physics.stepHeight : 0.5f);
            castOrigin.y += physics.stepHeight;

            // Shoot ray
            RaycastHit hit;
            float step = 0.1f;
            float height = castOrigin.y + mantle.height - physics.stepHeight;
            while (castOrigin.y < height)
            {
                if (Physics.Raycast(castOrigin, transform.forward, out hit, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                {
                    if (!Physics.Raycast(castOrigin + Vector3.up * step, transform.forward, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                    {
                        canMantle = true;
                        onWall = true;

                        break;
                    }
                }

                castOrigin.y += step;
            }
            #endregion Raycast

            #region Control state
            if (movementState != MovementState.Mantling && canMantle && (!grounded && !mantle.transitionFromGround || mantle.transitionFromGround) && headspaceAvailable)
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
            Vector3 negDirection = (3 * transform.forward - transform.right).normalized;
            Vector3 posDirection = (3 * transform.forward + transform.right).normalized;
            Collider wallCollider = null;

            // Shoot ray
            RaycastHit hit;
            if (Physics.Raycast(castOrigin, transform.forward, out hit, range, collisionExclusionMask, QueryTriggerInteraction.Ignore)
                || Physics.Raycast(castOrigin, negDirection, out hit, angleRange, collisionExclusionMask, QueryTriggerInteraction.Ignore)
                || Physics.Raycast(castOrigin, posDirection, out hit, angleRange, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            {
                onWall = true;
                wallCollider = hit.collider;
            }
            #endregion Raycast

            #region Control states
            if ((!grounded || climb.transitionFromGround) && onWall && headspaceAvailable && moveInputs.y > 0 && movementState != MovementState.Climbing && lastClimbCollider != wallCollider)
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
        void CounterPhysics()
        {
            // Gravity
            if (movementState <= MovementState.Pounding) // gravity states
            {
                // Wall stick
                if (!previousWallCheck && onWall && physics.wallStick > 0)
                {
                    applyWallstick = true;
                    rb.velocity = Vector3.zero;
                    if (physics.useWallStickDuration)
                        StartYieldWallStickCoroutine();
                }

                float gravity = physics.gravityMultiplier;
                if (onWall && applyWallstick)
                    gravity -= gravity * physics.wallStick;

                if (!grounded && !onStep)
                    rb.AddForce(Vector3.down * gravity);
            }

            if (!grounded)// Drag
            {
                Vector3 drag = new Vector3(-rb.velocity.x, 0, -rb.velocity.z);
                rb.AddForce(drag.normalized * physics.airDrag * 100);
            }

            // Slopes
            if (grounded && normalAngle > 0)
            {
                float angle = Mathf.Max(physics.slopeAngle, 0.01f);
                float slopeDiff = angle * physics.slopeFallOff;
                float slopeRange = angle - slopeDiff;
                if (normalAngle > slopeRange)
                {
                    float fallOffForce = (Mathf.Min(normalAngle, angle) - slopeRange) / Mathf.Max(slopeDiff, 0.01f);
                    ProjectDownSlope(physics.slopeForce * fallOffForce);
                }
            }

            // Cap Player Max Velocity
            if (physics.limitVelocity)
            {
                Vector3 newVelocity = rb.velocity;
                Vector2 maxVelocity = physics.maxVelocity;

                if (Mathf.Abs(rb.velocity.x) > maxVelocity.x)
                    newVelocity.x = maxVelocity.x * Mathf.Sign(rb.velocity.x);
                if (Mathf.Abs(rb.velocity.z) > maxVelocity.x)
                    newVelocity.z = maxVelocity.x * Mathf.Sign(rb.velocity.z);
                if (Mathf.Abs(rb.velocity.y) > maxVelocity.y)
                    newVelocity.y = maxVelocity.y * Mathf.Sign(rb.velocity.y);

                rb.velocity = newVelocity;
            }

            // Cap Turn rate
            lookResistance = 0;
            if (movementState == MovementState.Grappling)
                lookResistance += grapple.lookResistance;
            else if (freeLookInput)
                lookResistance += freelook.lookResistance;
            else if (!grounded)
            {
                if (physics.turnRateFromVelocity)
                    lookResistance += rb.velocity.magnitude / physics.maxVelocity.magnitude;
                else if (physics.airTurnRate < 1)
                    lookResistance += 1 - physics.airTurnRate;
            }

            previousVelocity = rb.velocity;
            previousPosition = rb.position;
        }

        /// <summary>
        /// Adds downward force along slope normal.
        /// </summary>
        /// <param name="force">The force added.</param>
        void ProjectDownSlope(float force)
        {
            Vector3 slopeSideways = Vector3.Cross(Vector3.down, normal);
            Vector3 slopeDown = Vector3.Cross(normal, slopeSideways).normalized;
            rb.velocity += slopeDown * force * 10 * d;
        }

        #endregion Physics

        #region Effects

        void HandleHeight() 
        {
            if (!headspaceAvailable)
                return;

            float maxHeight = (defaultHeight * heightOffset / 2); // the players maximum height 
            float minHeight = 4 * col.radius / heightOffset; // the players minimum height - will use this later (limited by collider radius)
            
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

        void HandleTime() 
        {
            if (controlTime && time != targetTime) 
            {
                if (Mathf.Abs(time - targetTime) < 0.01f)
                    time = targetTime;

                time = Mathf.Lerp(time, targetTime, timeSmoothing * d);
                Time.timeScale = time;
            }
        }

        void HandleEffects() 
        {
            if (!effects.enable)
                return;

            #region Headbob
            if (effects.headbob.enable)
            {
                float yBobAmount = 0;
                float xBobAmount = 0;
                float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
                float moveMultiplier = (currSpeed / movement.speed);
                float speed = effects.headbob.frequency;
                Vector3 targetPosition = cameraContainer.localPosition;

                if (magnitude > 1 && grounded &&
                    ((movementState <= MovementState.Crouching && movementState != MovementState.Sliding)))
                {
                    yBobAmount = effects.headbob.verticalAmount * (1 + moveMultiplier)/4 *  
                        Mathf.Sin(speed * moveMultiplier * t);
                    xBobAmount = effects.headbob.horizontalAmount * (1 + moveMultiplier)/4 * 
                        Mathf.Sin(speed * moveMultiplier / 2 * t);
                }

                targetPosition.y = Mathf.Lerp(targetPosition.y, cameraOffset.y + yBobAmount, speed * d);
                targetPosition.x = Mathf.Lerp(targetPosition.x, cameraOffset.x + xBobAmount, speed * d);
                cameraContainer.localPosition = targetPosition;
            }
            #endregion Headbob

            #region Footsteps
            if (effects.footsteps.enable && effects.footsteps.sound != null)
            {
                if (effects.footsteps.source == null) 
                {
                    effects.footsteps.source = gameObject.AddComponent<AudioSource>();
                }

                float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

                if (!effects.footsteps.source.isPlaying && magnitude > 1 && grounded &&
                    ((movementState <= MovementState.Crouching && movementState != MovementState.Sliding)))
                {
                    // Handle speed
                    float speed = effects.footsteps.frequency;
                    float moveMultiplier = (currSpeed / movement.speed);
                    float y = Mathf.Sin(speed * moveMultiplier * t);
                    if (y < -.99f) // trough
                    {
                        // Start audio
                        effects.footsteps.source.volume =
                            Random.Range(effects.footsteps.volumeRange.x, effects.footsteps.volumeRange.y);
                        effects.footsteps.source.pitch =
                            Random.Range(effects.footsteps.pitchRange.x, effects.footsteps.pitchRange.y);
                        effects.footsteps.source.PlayOneShot(effects.footsteps.sound);
                    }
                }
            }
            #endregion Footsteps

            #region Movement Sway

            if (effects.sway.enable && movementState <= MovementState.Crouching)
            {
                Vector3 idleSwayPosition;
                Vector3 idleSwayRotation;
                Vector3 lookSwayPosition;
                Vector3 lookSwayRotation;
                Vector3 moveSwayPosition;
                Vector3 moveSwayRotation;
                Vector3 fallSwayPosition;
                Vector3 fallSwayRotation;

                // Idle Sway
                Vector3 targetIdlePosition;
                Vector3 targetIdleRotation;

                float a = 3;
                float b = 4;
                float t = Time.time;
                Vector2 idleOffset = new Vector3(Mathf.Sin(a * t + Mathf.PI), Mathf.Sin(b * t));

                targetIdlePosition =
                    new Vector3(idleOffset.x * effects.sway.idleOffset.x,
                    idleOffset.y * effects.sway.idleOffset.y, 0);
                targetIdleRotation =
                    new Vector3(idleOffset.y * effects.sway.idleTilt.y, 0, idleOffset.x * effects.sway.idleTilt.x);

                // Look Sway
                Vector3 targetLookPosition;
                Vector3 targetLookRotation;

                targetLookPosition =
                    new Vector3(
                    Mathf.Clamp(-lookInputs.x * effects.sway.lookOffset.x, -effects.sway.lookOffset.x, effects.sway.lookOffset.x),
                    Mathf.Clamp(-lookInputs.y * effects.sway.lookOffset.y, -effects.sway.lookOffset.y, effects.sway.lookOffset.y), 0);
                targetLookRotation =
                    new Vector3(
                        Mathf.Clamp(effects.sway.lookOffset.y - lookInputs.y * effects.sway.lookTilt.y, -effects.sway.lookTilt.y, effects.sway.lookTilt.y), 0,
                        Mathf.Clamp(-lookInputs.x * effects.sway.lookTilt.x, -effects.sway.lookTilt.x, effects.sway.lookTilt.x));

                // Move Sway
                Vector3 targetMovePosition;
                Vector3 targetMoveRotation;

                targetMovePosition =
                    new Vector3((grounded ? -moveInputs.x : 0) * effects.sway.moveOffset, 0, 0);
                targetMoveRotation =
                     new Vector3(0, (grounded ? -moveInputs.x : 0) * effects.sway.moveTilt, 0);

                // Fall Sway
                Vector3 targetFallPosition;
                Vector3 targetFallRotation;

                targetFallPosition =
                    new Vector3(0, Mathf.Abs(rb.velocity.y) < 5f ? 0 : -Mathf.Sign(rb.velocity.y) * effects.sway.fallOffset, 0);
                targetFallRotation =
                    new Vector3(Mathf.Abs(rb.velocity.y) < 5f ? 0 : -Mathf.Sign(rb.velocity.y) * effects.sway.fallTilt, 0, 0);


                // Impact Sway
                // Vector3 targetImpactPosition;
                // Vector3 targetImpactRotation;

                // targetPosition = new Vector3(0, grounded && previousVelocity.y < -1? -effects.sway.impactOffset : 0, 0);
                //    targetRotation = new Vector3(grounded && previousVelocity.y < -1? -effects.sway.impactTilt : 0, 0, 0);

                for (int i = 0; i < effects.sway.container.childCount; i++)
                {

                    Transform pivot = effects.sway.container.GetChild(i);
                    for (int j = 0; j < pivot.childCount; j++)
                    {
                        Transform child = pivot.GetChild(j);
                            
                        idleSwayPosition =
                            Vector3.Lerp(child.localPosition, targetIdlePosition, effects.sway.idleSpeed * d);
                        idleSwayRotation =
                            new Vector3(
                            Mathf.LerpAngle(child.localEulerAngles.x, targetIdleRotation.x, effects.sway.idleSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.y, targetIdleRotation.y, effects.sway.idleSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.z, targetIdleRotation.z, effects.sway.idleSpeed * d));

                        lookSwayPosition =
                            Vector3.Lerp(child.localPosition, targetLookPosition, effects.sway.lookSpeed * d);
                        lookSwayRotation =
                            new Vector3(
                            Mathf.LerpAngle(child.localEulerAngles.x, targetLookRotation.x, effects.sway.lookSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.y, targetLookRotation.y, effects.sway.lookSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.z, targetLookRotation.z, effects.sway.lookSpeed * d));

                        moveSwayPosition =
                            Vector3.Lerp(child.localPosition, targetMovePosition, effects.sway.moveSpeed * d);
                        moveSwayRotation =
                            new Vector3(
                            Mathf.LerpAngle(child.localEulerAngles.x, targetMoveRotation.x, effects.sway.moveSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.y, targetMoveRotation.y, effects.sway.moveSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.z, targetMoveRotation.z, effects.sway.moveSpeed * d));

                        fallSwayPosition =
                            Vector3.Lerp(child.localPosition, targetFallPosition, effects.sway.fallSpeed * d);
                        fallSwayRotation =
                            new Vector3(
                            Mathf.LerpAngle(child.localEulerAngles.x, targetFallRotation.x, effects.sway.fallSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.y, targetFallRotation.y, effects.sway.fallSpeed * d),
                            Mathf.LerpAngle(child.localEulerAngles.z, targetFallRotation.z, effects.sway.fallSpeed * d));

                        //    impactSwayPosition =
                        //        Vector3.Lerp(effects.sway.container.localPosition, targetPosition, effects.sway.impactSpeed * d);
                        //    impactSwayRotation =
                        //        Quaternion.Euler(Vector3.Lerp(effects.sway.container.localEulerAngles, targetRotation, effects.sway.impactSpeed * d));

                        // Do Sway
                        child.localPosition = (idleSwayPosition + lookSwayPosition
                            + moveSwayPosition + fallSwayPosition /*+ impactSwayPosition*/) 
                            / 4;
                        child.localRotation = Quaternion.Euler((idleSwayRotation + lookSwayRotation
                            + moveSwayRotation + fallSwayRotation /** impactSwayRotation*/) / 4);
                    }
                }
            }

            #endregion Movement Sway

            #region FOV
            if(effects.fov.enable)
            {
                float targetFov;
                float speed = rb.velocity.magnitude;
                float diff = effects.fov.maxVelocity - effects.fov.minVelocity;

                if (speed <= effects.fov.minVelocity || diff == 0)
                    targetFov = defaultFOV;
                else if (speed >= effects.fov.maxVelocity)
                    targetFov = defaultFOV * effects.fov.multiplier;
                else
                    targetFov = defaultFOV
                        * (1 + ((effects.fov.multiplier - 1)
                        * (speed - effects.fov.minVelocity) / diff));

                float fov = Mathf.Lerp(cam.fieldOfView, targetFov, effects.fov.speed * d);
                cam.fieldOfView = fov;
            }
            #endregion FOV

            #region Speed lines
            // SPEED LINES
            if (effects.speedlines.enable)
            {
                float speed = Mathf.Clamp(rb.velocity.magnitude - effects.speedlines.minVelocity, 0, effects.speedlines.maxVelocity);
                speedLinesEmission.rateOverTimeMultiplier = Mathf.Pow(speed, 1 + effects.speedlines.curve) * effects.speedlines.amount;
            }
            #endregion Speed lines
        }

        #endregion Effects

        #region Coroutines

        IEnumerator wallStickCoroutine;
        IEnumerator jumpGraceCoroutine;
        IEnumerator jumpHoverCoroutine;
        IEnumerator slideBoostCoroutine;
        IEnumerator exitClimbCoroutine;

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartYieldWallStickCoroutine()
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
            applyWallstick = false;
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

            canJump = false;
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
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartYieldJumpHoverCoroutine()
        {
            if (!jump.hover.useDuration)
                return;

            StopJumpHoverCoroutine();
            jumpHoverCoroutine = YieldJumpHoverTime();
            StartCoroutine(jumpHoverCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopJumpHoverCoroutine()
        {
            if (jumpHoverCoroutine != null)
                StopCoroutine(jumpHoverCoroutine);
            ResetMovement();
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldJumpHoverTime()
        {
            yield return new WaitForSeconds(jump.hover.duration);
            ResetMovement();
        }

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartSlideBoostCoroutine()
        {
            StopSlideBoostCoroutine();
            slideBoostCoroutine = YieldSlideBoostTime();
            StartCoroutine(slideBoostCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopSlideBoostCoroutine()
        {
            if (slideBoostCoroutine != null)
                StopCoroutine(slideBoostCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldSlideBoostTime()
        {
            slide.boosted = true;
            yield return new WaitForSeconds(slide.boostDelay);
            slide.boosted = false;
        }

        /// <summary>
        /// YieldDashTime
        /// Waits for <code>FujinDashModule.duration</code> and resets movement.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldDashTime()
        {
            yield return new WaitForSeconds(dash.duration);
            rb.velocity -= rb.velocity.normalized * (1-dash.momentum) * dash.force * dash.duration / d;
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
        /// Waits for <code>FujinPoundModule.duration</code> and resets movement.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldGrappleCooldown()
        {
            yield return new WaitForSeconds(grapple.cooldownTime);
            canGrapple = true;
        }

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartExitClimbCoroutine()
        {
            if (!climb.useDuration)
                return;

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
        /// ControlHeight
        /// Turn off/on height control over the main camera and player collider
        /// </summary>
        /// <param name="on">the control state toggle</param>
        public void ControlTime(bool on)
        {
            controlTime = on;
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
