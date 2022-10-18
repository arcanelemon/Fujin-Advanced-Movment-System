using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fujin.Utils;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


/// <summary>
/// Physics-based motor that controls player movement. Configured by accesing <code>FujinMotorModule</code>.
/// </summary>
namespace Fujin.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMotor : MonoBehaviour
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
        public bool OnStep => onStep;
        public MovementState State => movementState;
        public bool drawGizmos;
        public float Speed => currSpeed;
        public int Jumps => numJumps;
        public int Dashes => numDashes;
        #endregion Editor

        #region Components

        // Modules
        public InputModule input = new InputModule();
        public PhysicsModule physics = new PhysicsModule();
        public MovementModule movement = new MovementModule();
        public JumpModule jump = new JumpModule();
        public SprintModule sprint = new SprintModule();
        public CrouchModule crouch = new CrouchModule();
        public SlideModule slide = new SlideModule();
        public DashModule dash = new DashModule();
        public PoundModule pound = new PoundModule();
        public MantleModule mantle = new MantleModule();
        public ClimbModule climb = new ClimbModule();
        public WallrunModule wallrun = new WallrunModule();
        public InteractModule interact = new InteractModule();
        public KickModule kick = new KickModule();
        public AutorunModule autorun = new AutorunModule();
        public LeanModule lean = new LeanModule();
        public SlowwalkModule slowwalk = new SlowwalkModule();
        public FreelookModule freelook = new FreelookModule();
        public GrappleModule grapple = new GrappleModule();
        public EffectsModule effects = new EffectsModule();

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
            Pounding = 4, //------|
            Kicking = 5, //------|
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
        bool jumpInput;
        bool sprintInput;
        bool slideInput;
        bool autoRunInput;
        bool slowWalkInput;
        bool leanInput;
        bool freeLookInput;

        bool enable;
        bool grounded;
        bool blockLook;
        bool onStep;
        bool onWall;
        bool canApplyWallstick = true;
        bool applyWallstick;
        bool standing = true;
        bool cueStand;
        bool headspaceAvailable;

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
        float defaultFOV;
        float tiltRotation;
        float leanDirection;
        int groundLayer;

        // counters

        Vector2 cameraOffset;
        Vector3 normal;
        Vector3 wallNormal;
        Vector3 previousVelocity;
        Vector3 previousPosition;

        LayerMask collisionExclusionMask;
        Collider wallCollider;
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

        void OnInteractPress(InputAction.CallbackContext context)
        {
            if (!interact.enable)
                return;

            if (interact.isPickedUp)
                throwInput = true;
            else
                interactInput = true;
        }

        void OnInteractRelease(InputAction.CallbackContext context)
        {
            if (!interact.enable)
                return;

            if (interact.isFocused)
            {
                if (!interact.isPickedUp)
                {
                    int layer = (1 << interact.GetFocus().gameObject.layer);
                    if (interact.interactLayer == layer && interact.interactLayer == interact.pickupLayer)
                        interact.Fire();
                } else if (throwInput)
                {
                    throwInput = false;
                    float mult = interact.GetPickupMult();
                    speedMultiplier += mult;
                    lookResistance += mult;
                    interact.Throw(cam.transform.forward);
                }

                interact.ResetHoldProgress();
            }

            interactInput = false;
        }

        void OnToggleSprintPress(InputAction.CallbackContext context)
        {
            if (!sprint.enable)
                return;

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
            else if (grounded)
            {
                sprintInput = true;
                if (movementState == MovementState.Crouching
                || movementState == MovementState.Sliding)
                    cueStand = true;
            }
        }

        void OnHoldSprintPress(InputAction.CallbackContext context)
        {
            if (!sprint.enable)
                return;

            if (grounded)
            {
                sprintInput = true;
                if (movementState == MovementState.Crouching
                || movementState == MovementState.Sliding)
                    cueStand = true;
            }
        }

        void OnHoldSprintRelease(InputAction.CallbackContext context)
        {
            if (!sprint.enable)
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

        void OnToggleCrouchPress(InputAction.CallbackContext context)
        {
            if (!crouch.enable && !slide.enable)
                return;

            if (movementState == MovementState.Crouching || movementState == MovementState.Sliding)
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

        void OnHoldCrouchPress(InputAction.CallbackContext context)
        {
            if (!crouch.enable && !slide.enable)
                return;

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

        void OnHoldCrouchRelease(InputAction.CallbackContext context)
        {
            if ((!crouch.enable && !slide.enable) ||
                (movementState != MovementState.Crouching && movementState != MovementState.Sliding))
                return;

            cueStand = true;
            if (slide.enable)
                slideInput = false;
        }

        void OnDashPress(InputAction.CallbackContext context)
        {
            if (!dash.enable || (!grounded && !dash.canDashInAir) || movementState > MovementState.Sprinting || numDashes == 0)
                return;

            StartDash();
        }

        void OnPoundPress(InputAction.CallbackContext context)
        {
            if (!pound.enable || grounded || !canPound)
                return;

            StartPound();
        }

        void OnKickPress(InputAction.CallbackContext context)
        {
            if (!kick.enable ||
                (movementState != MovementState.Default && movementState != MovementState.Sprinting && movementState != MovementState.Crouching))
                return;

            StartKick();
        }

        void OnAutoRunPress(InputAction.CallbackContext context)
        {
            if (!autorun.enable || !grounded)
                return;

            autoRunInput = !autoRunInput;
        }

        void OnToggleLeanPress(InputAction.CallbackContext context)
        {
            if (!lean.enable || !grounded || movementState == MovementState.Sprinting)
                return;

            if (leanInput)
                StopLean();
            else
                Lean(context.ReadValue<float>());
        }

        void OnHoldLeanPress(InputAction.CallbackContext context)
        {
            if (!lean.enable || !grounded || movementState == MovementState.Sprinting)
                return;

            Lean(context.ReadValue<float>());
        }

        void OnHoldLeanRelease(InputAction.CallbackContext context)
        {
            if (!lean.enable)
                return;

            StopLean();
        }

        void OnToggleFreeLookPress(InputAction.CallbackContext context)
        {
            if (!freelook.enable)
                return;

            if (freeLookInput)
                freeLookInput = false;
            else
                freeLookInput = true;
        }

        void OnHoldFreeLookPress(InputAction.CallbackContext context)
        {
            if (!freelook.enable)
                return;

            freeLookInput = true;
        }

        void OnHoldFreeLookRelease(InputAction.CallbackContext context)
        {
            if (!freelook.enable)
                return;

            freeLookInput = false;
        }

        void OnToggleSlowWalkPress(InputAction.CallbackContext context)
        {
            if (!slowwalk.enable || !grounded || movementState > MovementState.Crouching
                || movementState == MovementState.Sprinting || movementState == MovementState.Sliding)
                return;

            if (slowWalkInput)
            {
                slowWalkInput = false;
                speedMultiplier /= slowwalk.speedMultiplier;
                if (crouch.enable && targetHeight == crouch.height)
                {
                    Crouch();
                    return;
                }
            }
            else
            {
                slowWalkInput = true;
            }
        }

        void OnHoldSlowWalkPress(InputAction.CallbackContext context)
        {
            if (!slowwalk.enable || !grounded || movementState > MovementState.Crouching
                || movementState == MovementState.Sprinting || movementState == MovementState.Sliding)
                return;

            slowWalkInput = true;
        }

        void OnHoldSlowWalkRelease(InputAction.CallbackContext context)
        {
            if (!slowwalk.enable || !grounded || !slowWalkInput)
                return;

            slowWalkInput = false;
            speedMultiplier /= slowwalk.speedMultiplier;
            if (crouch.enable && targetHeight == crouch.height)
            {
                Crouch();
                return;
            }
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
            input.interactAction.performed += OnInteractPress;
            input.interactAction.canceled += OnInteractRelease;
            input.jumpAction.performed += OnJumpPress;
            input.jumpAction.canceled += OnJumpRelease;
            input.toggleSprintAction.performed += OnToggleSprintPress;
            input.holdSprintAction.performed += OnHoldSprintPress;
            input.holdSprintAction.canceled += OnHoldSprintRelease;
            input.toggleCrouchSlideAction.performed += OnToggleCrouchPress;
            input.holdCrouchSlideAction.performed += OnHoldCrouchPress;
            input.holdCrouchSlideAction.canceled += OnHoldCrouchRelease;
            input.dashAction.performed += OnDashPress;
            input.poundAction.performed += OnPoundPress;
            input.kickAction.performed += OnKickPress;
            input.autorunAction.performed += OnAutoRunPress;
            input.toggleLeanAction.performed += OnToggleLeanPress;
            input.holdLeanAction.performed += OnHoldLeanPress;
            input.holdLeanAction.canceled += OnHoldLeanRelease;
            input.toggleFreelookAction.performed += OnToggleFreeLookPress;
            input.holdFreelookAction.performed += OnHoldFreeLookPress;
            input.holdFreelookAction.canceled += OnHoldFreeLookRelease;
            input.toggleSlowwalkAction.performed += OnToggleSlowWalkPress;
            input.holdSlowwalkAction.performed += OnHoldSlowWalkPress;
            input.holdSlowwalkAction.canceled += OnHoldSlowWalkRelease;
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
            input.interactAction.Enable();
            input.jumpAction.Enable();
            input.toggleSprintAction.Enable();
            input.holdSprintAction.Enable();
            input.toggleCrouchSlideAction.Enable();
            input.holdCrouchSlideAction.Enable();
            input.dashAction.Enable();
            input.poundAction.Enable();
            input.kickAction.Enable();
            input.autorunAction.Enable();
            input.toggleLeanAction.Enable();
            input.holdLeanAction.Enable();
            input.toggleFreelookAction.Enable();
            input.holdFreelookAction.Enable();
            input.toggleSlowwalkAction.Enable();
            input.holdSlowwalkAction.Enable();
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
            input.interactAction.Disable();
            input.jumpAction.Disable();
            input.toggleSprintAction.Disable();
            input.holdSprintAction.Disable();
            input.toggleCrouchSlideAction.Disable();
            input.holdCrouchSlideAction.Disable();
            input.dashAction.Disable();
            input.poundAction.Disable();
            input.kickAction.Disable();
            input.autorunAction.Disable();
            input.toggleLeanAction.Disable();
            input.holdLeanAction.Disable();
            input.toggleFreelookAction.Disable();
            input.holdFreelookAction.Disable();
            input.toggleSlowwalkAction.Disable();
            input.holdSlowwalkAction.Disable();
            input.grappleAction.Disable();
        }

        #endregion New Input
#else
        #region Old Input

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
            HandleInput();
        }

        /// <summary>
        /// [ENGINE]
        /// </summary>
        void FixedUpdate()
        {
            t = Time.time;
            d = Time.deltaTime;

            CheckFocus();
            HandleCollisions();
            HandleEffects();
            HandleMovementStates();
            HandleInputQueue();
            HandleCamera();
            HandleOther();
            CounterPhysics();

            lookInputs = Vector2.zero;
            previousVelocity = rb.velocity;
            previousPosition = rb.position;
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
            if (!Application.isPlaying || !drawGizmos)
                return;

            Color cyan = Color.cyan;
            Color red = Color.red;
            cyan.a = 0.3f;
            red.a = 0.3f;

            // Ground Check
            Gizmos.color = red;

            if (grounded)
                Gizmos.color = cyan;
            Vector3 origin = transform.position + Vector3.up * groundCheckRadius;
            Gizmos.DrawSphere(origin + (Vector3.down * groundCheckDrawDistance), groundCheckRadius);

            // Interact
            if (interact.enable)
                interact.DrawGizmos(cam.transform.position, cam.transform.forward);

            // Step Check
            if (OnStep)
            {
                Gizmos.color = Color.magenta;
                //Gizmos.DrawMesh(MeshGenerator.GenerateTriangles(true, new Mesh(), transform.position));
            }

            if (!grounded)
            {
                Gizmos.color = Color.red;
                origin = transform.position + collisionDrawOffset / 2 * Vector3.down;
                Gizmos.DrawCube(origin, 2 * new Vector3(.35f, .25f, .35f));
            }

            if (!headspaceAvailable)
            {
                Gizmos.color = Color.red;
                float maxHeight = (defaultHeight * heightOffset / 2); // the players maximum height 
                origin = transform.position + (maxHeight + collisionDrawOffset / 2) * Vector3.up;
                Gizmos.DrawCube(origin, 2 * new Vector3(.35f, .25f, .35f));
            }

            // Wall Check
            if (onWall)
            {
                Gizmos.color = Color.green;
                //Gizmos.DrawCube(transform.position)

                if (movementState == MovementState.Mantling)
                {
                    Gizmos.DrawCube(ledgePosition + Vector3.up, 2 * new Vector3(collisionDrawOffset, 1, collisionDrawOffset));
                }
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

        float lookResistance;

        Vector2 lookInputs;
        Vector2 lookOffset;

        /// <summary>
        /// Look
        /// Rotates <code>cameraContainer</code> from mouseDelta.
        /// </summary>
        void Look()
        {
            if (blockLook)
                return;

            if (freelook.enable)
            {
                if (freeLookInput)
                {
                    //lookOffset.y -= lookInputs.y * input.lookAxis.y * freelook.axis.y;
                    //lookOffset.x += lookInputs.x * input.lookAxis.x * freelook.axis.x;
                    //float y = Mathf.Clamp(yRot + lookOffset.y, -freelook.verticalAngle, freelook.verticalAngle);
                    //float x = Mathf.Clamp(lookOffset.x, -freelook.horizontalAngle, freelook.horizontalAngle);
                    //lookOffset.y = y - yRot;
                    //lookOffset.x = x;
                    //cam.transform.localRotation = Quaternion.Euler(y, x, cameraContainer.localEulerAngles.z);
                }
                else
                {
                    // y rotation
                    float localX = (cameraContainer.localEulerAngles.x > 180) ? cameraContainer.localEulerAngles.x - 360f : cameraContainer.localEulerAngles.x;
                    float yRot = localX + lookInputs.y * -input.lookAxis.y;
                    yRot = Mathf.LerpAngle(localX, yRot, 10 * d);
                    yRot = Mathf.Clamp(yRot, -90, 90);
                    cameraContainer.localRotation = Quaternion.Euler(yRot + lookOffset.y, lookOffset.x, cameraContainer.localEulerAngles.z);

                    // x rotation
                    float xRot = rb.rotation.eulerAngles.y + lookInputs.x * input.lookAxis.x;
                    xRot = Mathf.LerpAngle(rb.rotation.eulerAngles.y, xRot, 10 * d);
                    rb.MoveRotation(Quaternion.Euler(0, xRot, 0));
                }
            }
            else
            {
                // y rotation
                float localX = (cameraContainer.localEulerAngles.x > 180) ? cameraContainer.localEulerAngles.x - 360f : cameraContainer.localEulerAngles.x;
                float yRot = localX + lookInputs.y * -input.lookAxis.y;
                yRot = Mathf.LerpAngle(localX, yRot, 10 * d);
                yRot = Mathf.Clamp(yRot, -90, 90);
                cameraContainer.localRotation = Quaternion.Euler(yRot + lookOffset.y, lookOffset.x, cameraContainer.localEulerAngles.z);

                // x rotation
                float xRot = rb.rotation.eulerAngles.y + lookInputs.x * input.lookAxis.x;
                xRot = Mathf.LerpAngle(rb.rotation.eulerAngles.y, xRot, 10 * d);
                rb.MoveRotation(Quaternion.Euler(0, xRot, 0));
            }
        }

        #endregion Look

        #region Move
        float speedMultiplier = 1;
        Vector2 moveInputs;

        /// <summary>
        /// Move
        /// Applies a movement force to the player based on <code>moveInputs</code>.
        /// If <code>!grounded</code> use <code>airDrag</code> to Lerp along <code>moveInputs</code>.
        /// Else project along normal vector and Lerp using <code>movementAcceleration && movementDecceleration</code>.
        /// </summary>
        void TryMove()
        {
            Vector3 direction;

            if (autoRunInput) // Auto Run 
                direction = (Mathf.Min(1 + moveInputs.y, 1) * transform.forward + moveInputs.x * transform.right).normalized;
            else
                direction = (moveInputs.y * transform.forward + moveInputs.x * transform.right).normalized;

            // ground
            if (grounded)
                Move(direction);

            // wall
            else if (applyWallstick)
            {
                if (Vector3.Dot(wallNormal, direction) > 0)
                    applyWallstick = false;
            }

            // air
            else
            {
                AirMove(direction);
            }
        }

        void Move(Vector3 direction)
        {

            if (rb.velocity.magnitude > 0 &&
                (movementState == MovementState.Sprinting || movementState == MovementState.Default))
            {
                if (moveInputs.y != 0)
                    if (previousVelocity.magnitude < movement.acceleration || Vector3.Dot(previousVelocity, rb.velocity) < -0.9f)
                        DipTilt(moveInputs.y * effects.camera.tilt.movementTiltVertical);
                    else if (moveInputs.y == 0)
                        ResetDip();

                SideTilt(moveInputs.x * effects.camera.tilt.movementTiltHorizontal);
            }

            direction = Vector3.ProjectOnPlane(direction, normal); // project onto normal
            Vector3 velocity = currSpeed * speedMultiplier * direction;
            Debug.DrawLine(transform.position, transform.position + velocity, Color.magenta);
            rb.velocity = Vector3.Lerp(rb.velocity, velocity, movement.acceleration * 100 * d * physics.friction);
        }

        void AirMove(Vector3 direction)
        {
            float speed = Vector3.Dot(rb.velocity, direction);
            speed = Mathf.Clamp(movement.airSpeed - speed, 0, movement.airAcceleration);
            Vector3 velocity = rb.velocity + speed * direction;

            rb.velocity = velocity;
        }

        #endregion Move

        #region Interact

        bool interactInput;
        bool throwInput;

        void Interact()
        {
            if (interact.CheckHoldProgress())
            {
                if (interact.pickupLayer == (1 << interact.GetFocus().gameObject.layer))
                {
                    if (!interact.isPickedUp)
                    {
                        interact.Pickup();
                        if (interact.rigidbody)
                        {
                            float mult = interact.GetPickupMult();
                            speedMultiplier -= mult;
                            lookResistance -= mult;
                        }
                    }
                }
                else
                    interact.Fire();

                interactInput = false;
            }
        }

        void CheckFocus()
        {
            if (!interact.enable)
                return;

            RaycastHit hit;
            bool interactable;
            Vector3 origin = cam.transform.position + cam.transform.forward;
            Vector3 direction = cam.transform.forward;
            if (interact.drawRadius > 0)
                interactable = Physics.SphereCast(origin, interact.drawRadius, direction, out hit, interact.drawDistance, interact.interactLayer) ||
                    Physics.SphereCast(origin, interact.drawRadius, direction, out hit, interact.drawDistance, interact.pickupLayer);
            else
                interactable = Physics.Raycast(origin, direction, out hit, interact.drawDistance, interact.interactLayer) ||
                    Physics.Raycast(origin, direction, out hit, interact.drawDistance, interact.pickupLayer);

            if (interactable)
            {
                if (hit.collider != interact.GetFocus() && !interact.isPickedUp)
                    interact.SetFocus(hit.collider);

                if (interactInput)
                    Interact();
                else if (interact.isPickedUp)
                    Carry();
            }
            else if (interact.isFocused) 
            {
                if (!interact.isPickedUp)
                    interact.ClearFocus();
                else 
                {
                    if (Vector3.Distance(interact.rigidbody.position, cam.transform.position + cam.transform.forward) > interact.drawDistance)
                    {
                        float mult = interact.GetPickupMult();
                        speedMultiplier += mult;
                        lookResistance += mult;
                        interact.rigidbody.velocity = Vector3.zero;
                        interact.Release();
                    }
                    else
                        Carry();
                }
            }
        }

        void Carry() 
        {
            if (throwInput && interact.enableThrowCharge && interact.IncrementThrowCharge())
            {
                throwInput = false;
                interact.ResetThrow();
            }

            // Move rigibody
            Vector3 direction;
            float force = interact.pickupForce * interact.GetPickupMult() * 10 * d;

            // Move towards pickup to center
            if (interact.pickupCenterObjects)
            {
                Vector3 targetPosition = cam.transform.position + (1 + interact.pickupCenterDistance) * cam.transform.forward;
                float distance = Vector3.Distance(interact.rigidbody.position, targetPosition);
                if (distance < 1)
                    force *= distance;

                direction = 100 * (targetPosition - interact.rigidbody.position).normalized;
            }

            // Move based on input
            else
            {
                direction = (lookInputs.x * cam.transform.right + lookInputs.y * cam.transform.up).normalized;
            }

            interact.rigidbody.velocity = force * direction;
        }

        #endregion Interact

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
                    direction = wallNormal + Vector3.up;

                    if (movementState == MovementState.Climbing)
                        ExitClimb();
                    else if (movementState == MovementState.WallRunning) 
                        ExitWallRun();
                }
                else if (!grounded && jump.doubleJump.enable && numJumps > 0)
                {
                    --numJumps;
                    force = jump.doubleJump.force;

                    // double jump direction
                    Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    Vector3 input = (moveInputs.x * transform.right + moveInputs.y * transform.forward).normalized;
                    
                    float dot = Vector3.Dot(velocity.normalized, input);
                    float facing = Mathf.Min(2 - (dot + 1), 1);
                    Vector3 inputVelocity = currSpeed * facing * (input - Mathf.Max(dot, 0) * velocity.normalized).normalized;
                    Vector3 counterVelocity = jump.doubleJump.redirect * facing * velocity;
                    rb.velocity += inputVelocity - counterVelocity;
                }
                else
                    return;
            }

            // Grounded
            else
            {
                if (onWall) // TODO: wall hop
                {
                    
                }

                DipTilt(effects.camera.tilt.jumpTiltVertical);
                RollTilt(effects.camera.tilt.jumpTiltHorizontal);
            }

            #endregion Double Jump & Wall Jump

            #region State Control

            // Calculate jump direction from movement state
            if (movementState == MovementState.Mantling || movementState == MovementState.Dashing)
                return;

            else if (movementState == MovementState.Crouching)
                Stand();
            
            else if (movementState == MovementState.Pounding || movementState == MovementState.Grappling)
                ResetMovement();


            #endregion State Control

            #region Apply Force

            // Reset y velocity and add forces
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(direction.normalized * force * Mathf.Max(speedMultiplier, 0.5f), ForceMode.Impulse);

            if (grounded)
                StartYieldJumpGraceCoroutine();

            #endregion Apply Force
        }

        void StartHover() 
        {
            StartYieldJumpHoverCoroutine();
            movementState = MovementState.Hovering;
        }

        void Hover()
        {
            TryMove();

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
            standing = false;
            currSpeed = movement.speed * crouch.speedMultiplier;
            targetHeight = crouch.height;
            if (movementState != MovementState.Sliding)
            {
                DipTilt(-effects.camera.tilt.crouchTiltVertical);
                RollTilt(effects.camera.tilt.crouchTiltHorizontal);
            }
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
            standing = false;
            targetHeight = slide.height;
            slideDirection = Vector3.ProjectOnPlane(direction, normal).normalized;
            rb.AddForce(slideDirection * (slide.boosted ? 0 : slide.boost), ForceMode.Impulse);
            movementState = MovementState.Sliding;
            StartSlideCooldownCoroutine();
            StartSlideFrictionCoroutine();
        }

        /// <summary>
        /// Slide
        /// Adds slide forces to player.
        /// </summary>
        void Slide()
        {
            if (!grounded)
                return;

            // Slide Control
            float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
            slideSpeed = Mathf.Lerp(slideSpeed, moveInputs.x, slide.control * d);
            Vector3 direction = slideDirection + transform.right * slideSpeed;
            Vector3 velocity = direction.normalized * magnitude;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
            SideTilt(moveInputs.x * effects.camera.tilt.slideTilt);

            // Slide Friction
            if (slide.applyFriction)
            {
                rb.velocity += -rb.velocity.normalized * slide.friction;
                Shake(effects.camera.tilt.slideShakeAmount, rb.velocity.magnitude * d);
            }

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
                    cueStand = true;
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
            if (!grounded && !dash.canDashInAir)
                return;

            // Calculate Dash Direction based on input and axis
            Vector3 direction = Vector3.zero;

            if (dash.omnidirectional)
            {
                direction = cam.transform.forward;
            }
            else
            {
                direction += transform.forward * (moveInputs.y > 0.1f && dash.axis.y > 0 ? 1 : 0);
                direction += -transform.forward * (moveInputs.y < -0.1f && dash.axis.z > 0 ? 1 : 0);
                direction += transform.right * (moveInputs.x > 0.1f && dash.axis.x > 0 ? 1 : 0);
                direction += -transform.right * (moveInputs.x < -0.1f && dash.axis.x > 0 ? 1 : 0);
            }

            // If no input, set direction to only enabled axis
            if (direction == Vector3.zero)
            {
                if (dash.axis.y > 0)
                    direction += transform.forward;
                else if (dash.axis.z > 0)
                    direction -= transform.forward;
            }

            // Set direction and control state
            rb.velocity = Vector3.zero;
            dashDirection = direction;
            float yDot = Mathf.Abs(Vector3.Dot(dashDirection, transform.forward));
            float tilt = effects.camera.tilt.dashTilt;
            DipTilt(yDot * tilt);
            StartCoroutine(YieldDashTime());

            movementState = MovementState.Dashing;
        }

        /// <summary>
        /// Dash
        /// Moves player by dash force.
        /// </summary>
        void Dash()
        {
            // Add force
            if (onWall)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                Vector3 direction = dashDirection;

                // project onto normal if grounded
                if (grounded && !dash.omnidirectional)
                    direction = Vector3.ProjectOnPlane(dashDirection, normal);

                Vector3 velocity = dash.force * 10 * direction.normalized;
                rb.velocity = velocity;
            }

            // apply tilt
            if (grounded)
            {
                float xDot = Vector3.Dot(dashDirection, transform.right);
                float tilt = effects.camera.tilt.dashTilt;
                SideTilt(xDot * tilt);
            }
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
                velocity.z = 0;
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

        bool pullup;
        Vector3 ledgePosition;

        /// <summary>
        /// Mantle
        /// Apply upward mantle force to player.
        /// </summary>
        void Mantle()
        {
            #region End Conditions
            // End Mantle
            bool distanceCheck = Vector3.Distance(transform.position, ledgePosition) < 0.1f;
            if (distanceCheck && pullup)
                    rb.velocity = Vector3.zero;

            if (!headspaceAvailable || distanceCheck)
            {

                ResetMovement();
                return;
            }
            #endregion End Conditions

            #region Move
            //Move Player
            float speed;
            Vector3 direction;
            Vector3 ledgeNormal = (new Vector3(ledgePosition.x, transform.position.y, ledgePosition.z) - transform.position).normalized;

            #region Pullup
            // Pullup
            if (pullup) 
            {
                speed = mantle.speed * 100 * d;
                if (!Physics.Raycast(transform.position, ledgeNormal,
                    collisionThreshold + collisionDrawOffset, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                    direction = Vector3.Lerp(transform.position, ledgePosition, mantle.speed * d);
                else 
                    direction = Vector3.Lerp(transform.position, new Vector3(transform.position.x, ledgePosition.y, transform.position.z), mantle.speed * d);

                if (blockLook)
                {
                    // we can use the blockLook toggle to also check if we have already matched the ledgeheight
                    Vector3 cameraPosition = transform.position + cameraOffset.y * transform.up;
                    if (!Physics.Raycast(cameraPosition, ledgeNormal,
                        1, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                    {
                        // play anim

                        // roll
                        DipTilt(effects.camera.tilt.mantleTiltVertical);
                        RollTilt(-1.5f * effects.camera.tilt.mantleTiltHorizontal);
                        blockLook = false;
                    }

                    // rotate player
                    Quaternion targetRotation = Quaternion.LookRotation(ledgeNormal);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * d);
                }
            }
            #endregion Pullup

            #region Vault
            // Vault/Stepup
            else
            {
                // redirect along spline
                speed = rb.velocity.magnitude < mantle.speed ? mantle.speed * 100 * d : rb.velocity.magnitude;
                if (!Physics.Raycast(transform.position, ledgeNormal, collisionThreshold + collisionDrawOffset, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                    direction = Vector3.Lerp(transform.position, ledgePosition, speed);
                else
                    direction = Vector3.Lerp(transform.position, new Vector3(transform.position.x, ledgePosition.y, transform.position.z), speed);

                float multiplier = -2 * (ledgePosition.y - transform.position.y) / cameraOffset.y;
                SideTilt(multiplier * effects.camera.tilt.mantleTiltHorizontal);
            }
            #endregion Vault

            // move along spline
            direction = (direction - transform.position).normalized;
            rb.velocity = speed * direction;
            #endregion Move
        }

        void StartPullup() 
        {
            if (!standing)
                Stand();

            if (movementState == MovementState.Climbing)
                ExitClimb();

            pullup = true;
            blockLook = true;
            rb.velocity = Vector3.zero;
            movementState = MovementState.Mantling;
        }

        void StartVault() 
        {
            float multiplier = (ledgePosition.y - transform.position.y) / cameraOffset.y;
            DipTilt(multiplier * effects.camera.tilt.mantleTiltVertical);

            pullup = false;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            movementState = MovementState.Mantling;
        }

        #endregion Mantle

        #region Climb

        /// <summary>
        /// Climb
        /// Applys an upwards force of <FujinClimbModule.speed> to player
        /// </summary>
        void Climb()
        {
            if (!onWall || moveInputs.y < 0.1 || !headspaceAvailable)
                ExitClimb();
            else 
                rb.velocity = 100 * climb.speed * d * Vector3.up;
        }

        void StartClimb() 
        {
            if (jump.wallJump.enable)
                numJumps = jump.doubleJump.amount;
            rb.velocity = Vector3.zero;

            climb.collider = wallCollider;
            StartExitClimbCoroutine();
            movementState = MovementState.Climbing;
        }

        /// <summary>
        /// ExitClimb
        /// Calls <code>StopExitClimbCoroutine</code> and rests movement state.
        /// </summary>
        void ExitClimb()
        {
            StopExitClimbCoroutine();
            ResetMovement();
        }

        #endregion Climb

        #region Wall Run

        float wallRunBreakTimer;

        void WallRun()
        {
            #region Vectors

            // Grab vectors
            float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
            Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up);
            float sign = Mathf.Sign(Vector3.Dot(wallDirection, transform.forward));
            wallDirection *= sign;
            Vector3 velocity = rb.velocity - Vector3.Dot(rb.velocity, wallDirection) * wallDirection;
            SideTilt(effects.camera.tilt.wallRunTiltHorizontal * sign * 2);

            #endregion Vectors

            #region End Conditions
            // break if not on wall, decellerated, or being blocked

            // Check if blocked
            float delta = Vector3.Distance(transform.position, previousPosition);
            if (delta < wallrun.accelaration * d)
                wallRunBreakTimer += d;
            else
                wallRunBreakTimer = 0;

            // grounded, decellerated, blocked, moving away, or look angle
            if (grounded || 
                ((wallrun.decellerate && speed < wallrun.breakSpeed) || 
                wallRunBreakTimer >= 1 || Vector3.Dot(moveInputs.x * transform.right, wallNormal) > 0))
            {
                ExitWallRun();
                return;
            }

            // not on wall
            if (!onWall)
            {
                Vector3 origin = transform.position + cameraOffset.y * Vector3.up;
                if (!Physics.Raycast(origin, -wallNormal, out RaycastHit hit, 1,  collisionExclusionMask, QueryTriggerInteraction.Ignore)) 
                {
                    // not on wall or wall angle too steep
                    ExitWallRun();
                    return;
                }

                // change wall normal
                blockLook = true;
                wallNormal = hit.normal;
                wallrun.rotated = false;
                velocity = Vector3.ProjectOnPlane(velocity, wallNormal);
            }

            #endregion End Conditions

            #region Calculate

            float targetSpeed;
            float smoothing = 10 * d;
            float inputDot = Vector3.Dot(transform.TransformDirection(new Vector3(moveInputs.x, 0, moveInputs.y).normalized), wallDirection);

            if (wallrun.decelleration > 0) {
                if (wallrun.decellerate && inputDot > 0.5f)
                    wallrun.decellerate = false;
                else if (!wallrun.decellerate && (wallrun.speed - speed) < wallrun.accelaration)
                    wallrun.decellerate = true;
            }

            // decelerate
            if (wallrun.decellerate) 
            {
                if (inputDot < -0.5f)
                    smoothing *= 2;

                targetSpeed = 0;
                velocity.y -= wallrun.drag * 100 * d;
                smoothing *= wallrun.decelleration;
            } else 
            {
                targetSpeed = wallrun.speed;
                smoothing *= wallrun.accelaration;
            }
            
            speed = Mathf.Lerp(speed * Mathf.Sign(Vector3.Dot(rb.velocity.normalized, wallDirection)), targetSpeed, smoothing);
            velocity += speed * wallDirection;

            #endregion Calculate

            #region Apply

            // move perpandicular to wall normal
            rb.velocity = velocity;

            // rotate player
            if (!wallrun.rotated) 
            {
                Quaternion targetRotation = Quaternion.LookRotation(wallDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * d);
                if (Quaternion.Angle(transform.rotation, targetRotation) <= 5f)
                { 
                    if (lookInputs.magnitude > 0.2f) 
                    {
                        blockLook = false;
                        wallrun.rotated = true;
                    }
                }
            }

            #endregion Apply
        }

        void StartWallRun() 
        {
            blockLook = true;
            if (jump.wallJump.enable)
                numJumps = jump.doubleJump.amount;

            // redirect horizontal velocity and add boost impulse
            Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            float speed = velocity.magnitude;
            speed += wallrun.boost;
            rb.velocity = Vector3.ProjectOnPlane(speed * velocity.normalized, wallNormal);

            wallrun.collider = wallCollider;
            StartExitWallRunCoroutine();
            movementState = MovementState.WallRunning;
        }

        void ExitWallRun()
        {
            wallrun.decellerate = false;
            wallrun.rotated = false;
            wallRunBreakTimer = 0;
            blockLook = false;
            ResetMovement();
        }

        #endregion Wall Run

        #region Kick

        void Kick()
        {
            if (movementState != MovementState.Kicking)
                return;
            
            Collider[] cols = Physics.OverlapSphere(cam.transform.position + cam.transform.forward, 1, collisionExclusionMask, QueryTriggerInteraction.Ignore);
            foreach(Collider col in cols) 
            {
                if (kick.kickLayer != (kick.kickLayer | (1 << col.gameObject.layer)))
                    continue;

                kick.action.Invoke(col.gameObject);
                Rigidbody colRb = col.GetComponent<Rigidbody>();
                if (colRb)
                    colRb.AddForce(kick.force * 100 * cam.transform.forward);
            }

            if (grounded)
                DipTilt(effects.camera.tilt.kickTilt);
            StartCoroutine(YieldKickResetTime(cols.Length > 0));
        }

        void StartKick() 
        {
            if (movementState == MovementState.Sprinting)
                ResetMovement();
            StartCoroutine(YieldKickTime());
            if (grounded)
                DipTilt(-effects.camera.tilt.kickTilt);
            lookResistance += kick.lookResistance;
            speedMultiplier *= kick.speedMultiplier;
            movementState = MovementState.Kicking;
        }

        void EndKick()
        {
            if (movementState != MovementState.Kicking)
                return;

            lookResistance -= kick.lookResistance;
            speedMultiplier /= kick.speedMultiplier;
            ResetMovement();
        }

        #endregion Kick

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

            #region Wrap
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
            #endregion Wrap

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
            SideTilt(effects.camera.tilt.grappleTilt * Mathf.Abs(velocityDot) * 2);

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
                || grapple.simType == GrappleModule.SimulationType.None)
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
                    if (grapple.simType == GrappleModule.SimulationType.Wobble)
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

            MeshGenerator.GenerateRope(grapple.sides, grapple.thickness, vertices.ToArray(), grappleMesh, new Vector2(100, 100));
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

            if (!standing && !slideInput)
                Stand();
            else 
            {
                bool s = slideInput;
                ResetMovement();
                slideInput = s;
            }

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
            standing = true;

            if (grounded)
            {
                DipTilt(effects.camera.tilt.crouchTiltVertical);
                RollTilt(effects.camera.tilt.crouchTiltHorizontal);
            }

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
            rb.isKinematic = false;
            blockLook = false;
            movementState = MovementState.Default;
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
                    if ((moveInputs.y < 0.1f && !autoRunInput) || (moveInputs.y < -0.1f && autoRunInput)) // account for no input or auto run
                        ResetMovement();
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
                case MovementState.WallRunning:
                    WallRun();
                    break;
                case MovementState.Grappling:
                    Grapple();
                    break;
                case MovementState.Hovering:
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
            if (sprint.enable && sprintInput && (moveInputs.y > 0.1f || (autoRunInput && moveInputs.y > -0.1f))
                && movementState == MovementState.Default)
                Sprint();

            // SLOW WALK
            if (slowwalk.enable && slowWalkInput) 
                speedMultiplier *= slowwalk.speedMultiplier;
                
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
            CheckWall();
        }

        Vector3 stepDirection;

        /// <summary>
        /// CheckGround
        /// Checks for ground collision via a <code>SphereCast</code>. Calculates new normal vectors and handles instanse properties on collision.
        /// </summary>
        void CheckGround()
        {
            bool grounded = false;

            #region Legacy

            #region Steps
            //onStep = false;
            //float increment = 0.01f;
            //float range = collisionDrawOffset + collisionThreshold + physics.stepDepth;

            //Vector3 castDirection = (moveInputs.x * transform.right + moveInputs.y * transform.forward).normalized;
            //if (castDirection == Vector3.zero)
            //    castDirection = stepDirection;
            //else
            //    stepDirection = castDirection;

            //// Ascending
            //RaycastHit hit;
            //Vector3 castOrigin = transform.position + physics.stepHeight * Vector3.up;
            //while (castOrigin.y > transform.position.y)
            //{
            //    Vector3 nextStep = castOrigin - increment * Vector3.up;
            //    if (!Physics.Raycast(castOrigin, castDirection, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            //    {
            //        if (Physics.Raycast(nextStep, castDirection, out hit, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            //        {
            //            nextStep = hit.point;
            //            Physics.Linecast(hit.point, transform.position, out hit, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
            //            nextStep.y += hit.point.y - transform.position.y;

            //            Debug.DrawLine(transform.position, nextStep, Color.cyan);
            //            normal = Vector3.Cross(nextStep - transform.position, Vector3.right).normalized;
            //            normalAngle = Vector3.Angle(normal, Vector3.up);
            //            onStep = true;
            //            break;
            //        }
            //    }

            //    castOrigin = nextStep;
            //}

            //// Descending
            //if (!onStep)
            //{
            //    castOrigin = transform.position + physics.stepHeight * Vector3.up;
            //    while (castOrigin.y > transform.position.y)
            //    {
            //        Vector3 nextStep = castOrigin - increment * Vector3.up;
            //        if (!Physics.Raycast(castOrigin, -castDirection, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            //        {
            //            if (Physics.Raycast(nextStep, -castDirection, out hit, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            //            {
            //                nextStep = hit.point;
            //                Physics.Linecast(hit.point, transform.position, out hit, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
            //                nextStep.y += hit.point.y - transform.position.y;

            //                Debug.DrawLine(transform.position, nextStep, Color.cyan);
            //                normal = Vector3.Cross(nextStep - transform.position, Vector3.right).normalized;
            //                normalAngle = Vector3.Angle(normal, Vector3.up);
            //                onStep = true;
            //                break;
            //            }
            //        }

            //        castOrigin = nextStep;
            //    }
            //}

            //grounded = onStep;
            #endregion Steps

            #region Ground Check
            if (!grounded)
            {
                float radius = collisionDrawOffset;
                float distance = 2 * collisionThreshold;
                Vector3 origin = transform.position + (radius + collisionThreshold) * Vector3.up;
                if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 contactPoint = origin + (Vector3.down * distance);
                    Vector3 contactDirection = hit.point - origin;
                    float contactOffset = contactDirection.magnitude + distance;

                    // Slopes
                    RaycastHit spherePoint;
                    if (Physics.Raycast(contactPoint, contactDirection, out spherePoint, contactOffset, collisionExclusionMask))
                    {
                        normal = spherePoint.normal;
                        normalAngle = Vector3.Angle(normal, Vector3.up);
                    }

                    grounded = true;
                    groundLayer = hit.collider.gameObject.layer;
                }
            }
            #endregion Ground Check

            #endregion Legacy

            #region States

            if (Mathf.Approximately(normalAngle, 90))
            {
                normalAngle = 0;
                normal = Vector3.up;
            }

            if (grounded && !this.grounded)
            {
                canJump = true;
                canHover = true;
                canPound = true;
                numJumps = jump.doubleJump.amount;
                canApplyWallstick = true;
                applyWallstick = false;

                if (jump.enable)
                    StopJumpGraceCoroutine();

                if ((slideInput && (movementState == MovementState.Default || movementState == MovementState.Sprinting)) 
                    || movementState == MovementState.Sliding)
                {
                    Vector3 direction = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    float magnitude = direction.magnitude;
                    if (magnitude < slide.boost)
                        direction = transform.forward;

                    StartSlide(direction);
                }
                else if (sprintInput && movementState == MovementState.Default)
                    Sprint();
                else if (!sprintInput && movementState == MovementState.Sprinting)
                    ResetMovement();
                else if (movementState == MovementState.Pounding)
                    EndPound();
                else if (movementState == MovementState.Hovering)
                    StopJumpHoverCoroutine();
            }

            this.grounded = grounded;
            #endregion States
        }

        /// <summary>
        /// CheckHeight
        /// Checks for head collisions and handles updates to height.
        /// </summary>
        void CheckHeight()
        {
            // Check Headspace
            float maxHeight = (defaultHeight * heightOffset / 2); // the players maximum height 

            Vector3 origin = transform.position + (maxHeight + collisionDrawOffset / 2) * Vector3.up;
            headspaceAvailable = !Physics.CheckBox(origin, new Vector3(.35f, .25f, .35f), Quaternion.identity, collisionExclusionMask, 
                QueryTriggerInteraction.Ignore);

            if (!headspaceAvailable && cameraOffset.y < targetHeight)
                return;

            float minHeight = 4 * col.radius / heightOffset; // the players minimum height - will use this later (limited by collider radius)

            //// Handle Transition
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
        /// 
        /// </summary>
        void CheckWall()
        {
            onWall = false;
            wallCollider = null;
            Vector3 castOrigin;
            Vector3 castDirection;
            RaycastHit hit;

            #region Mantle

            if (mantle.enable && !grounded && headspaceAvailable && moveInputs.y > 0.1f && movementState != MovementState.Mantling)
            {
                float step = 0.1f;
                float pullupHeight = transform.position.y + cameraOffset.y;
                castOrigin = transform.position + mantle.height * Vector3.up;
                castDirection = (moveInputs.x * transform.right + moveInputs.y * transform.forward);
                castDirection *= Mathf.Max(Vector3.Dot(transform.forward, castDirection), 0);

                while (castOrigin.y > transform.position.y)
                {
                    // Pullup
                    if (castOrigin.y > pullupHeight)
                    {
                        if (!Physics.Raycast(castOrigin, castDirection, 1, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                        {
                            if (Physics.Raycast(castOrigin + step * Vector3.down, castDirection, out hit, 1, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                            {
                                ledgePosition = hit.point + step * Vector3.up + collisionDrawOffset * castDirection;

                                if (CheckBoundingBox(ledgePosition + Vector3.up))
                                {
                                    onWall = true;
                                    wallNormal = hit.normal;
                                    wallCollider = hit.collider;
                                    StartPullup();
                                }

                                return;
                            }
                        }
                    }

                    // Vault / Stepup
                    else
                    {
                        if (!Physics.Raycast(castOrigin, castDirection, 1.5f, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                        {
                            if (Physics.Raycast(castOrigin + step * Vector3.down, castDirection, out hit, 1.5f, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                            {
                                ledgePosition = hit.point + step * Vector3.up + collisionDrawOffset * castDirection;

                                if (CheckBoundingBox(ledgePosition + Vector3.up))
                                {
                                    onWall = true;
                                    wallNormal = hit.normal;
                                    wallCollider = hit.collider;
                                    StartVault();
                                }

                                return;
                            }
                        }
                    }

                    castOrigin.y -= step;
                }
            }

            #endregion Mantle

            #region Wall

            // Shoot ray
            float degrees = 0;
            float range = collisionDrawOffset + collisionThreshold;
            castOrigin = transform.position + Vector3.up * cameraOffset.y;
            castDirection = transform.forward;
            while (degrees < 360)
            {
                if (Physics.Raycast(castOrigin, castDirection, out hit, range, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                {
                    if (CheckBoundingBox(hit.point - range * castDirection))
                    {
                        onWall = true;
                        wallNormal = hit.normal;
                        wallCollider = hit.collider;
                        break;
                    }
                }

                degrees += 15;
                castDirection = Quaternion.Euler(0, 15, 0) * castDirection;
            }

            if (!onWall)
            {
                climb.collider = null;
                wallrun.collider = null;
                return;
            }

            Vector3 origin = transform.position + collisionDrawOffset / 2 * Vector3.down;
            if (!grounded &&
                !Physics.CheckBox(origin, new Vector3(.35f, .25f, .35f), Quaternion.identity, collisionExclusionMask, QueryTriggerInteraction.Ignore))
            {
    
                #region Climb

                if (climb.enable && movementState != MovementState.Climbing
                    && headspaceAvailable && moveInputs.y > 0 && climb.collider != wallCollider &&
                    Vector3.Dot(wallNormal, transform.forward) < -0.7f)
                {
                    StartClimb();
                }

                #endregion Climb

                #region Wall Run

                else if (wallrun.enable && movementState != MovementState.WallRunning && wallrun.collider != wallCollider
                && Mathf.Abs(Vector3.Dot(wallNormal, transform.right)) >= 0.7f && !Physics.Raycast(transform.position, -transform.up, wallrun.minHeight, collisionExclusionMask, QueryTriggerInteraction.Ignore))
                {
                    StartWallRun();
                }

                #endregion Wall Run

                #region Wall Stick
                { 
                    // begin stick
                    if (canApplyWallstick && !applyWallstick && movementState <= MovementState.Crouching &&
                    Vector3.Dot((moveInputs.x * transform.right + moveInputs.y * transform.forward).normalized, -wallNormal) > 0.5f)
                    {
                        if (jump.wallJump.enable)
                            numJumps = jump.doubleJump.amount;
                        cueStand = true;
                        applyWallstick = true;
                        canApplyWallstick = false;
                        rb.velocity = Vector3.zero;
                        if (physics.useWallStickDuration)
                            StartYieldWallStickCoroutine();
                    }

                    // apply tilt
                    else if (applyWallstick) 
                    {
                        SideTilt(effects.camera.tilt.wallStickTilt *
                               Vector3.Dot(Vector3.Cross(wallNormal, Vector3.up), transform.forward) * 2);

                        if (Vector3.Dot((moveInputs.x * transform.right + moveInputs.y * transform.forward).normalized, -wallNormal) < 0.5f)
                            applyWallstick = false;
                    }
                        
                }

                #endregion Wall Stick
            }

            #endregion Wall
        }

        bool CheckBoundingBox(Vector3 origin) 
        {
            float halfExtents = collisionDrawOffset / 2;
            return !Physics.CheckBox(origin, new Vector3(halfExtents, 1, halfExtents), Quaternion.identity, collisionExclusionMask);
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
            if (!grounded && movementState <= MovementState.Kicking)
            {
                float gravity = physics.gravityMultiplier * 100;

                if (onWall && applyWallstick)
                    gravity *= 1 - physics.wallStick;

                rb.AddForce(gravity * d * Vector3.down);
            }

            // Drag
            if (!grounded)
            {
                Vector3 drag = new Vector3(-rb.velocity.x, 0, -rb.velocity.z);
                rb.AddForce(drag.normalized * physics.airDrag * 100);
            }

            // Slopes
            if (grounded && !onStep && normalAngle > 0)
            {
                float angle = physics.slopeAngle;
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

        float sideTiltAngle;
        float rollImpulse;
        float rollAngle;
        float dipImpulse;
        float dipAngle;
        float shakeAmount;
        float shakeSpeed;

        Vector3 cameraShake;

        void HandleCamera() 
        {
            if (!effects.camera.enable)
                return;

            // side tilt
            Vector3 targetRotation = cameraContainer.localEulerAngles;
            targetRotation.z = Mathf.LerpAngle(targetRotation.z, sideTiltAngle, 3 * d);
            sideTiltAngle = 0;

            // roll impulse
            if (rollImpulse != 0)
            {
                // lerp added angle to zero
                float smoothing = 50 * d;
                float sign = Mathf.Sign(rollImpulse);
                targetRotation.z += sign * smoothing;
                rollImpulse -= sign * smoothing;

                if (Mathf.Abs(rollImpulse) <= 1f)
                {
                    // arrived at origin
                    if (sign != Mathf.Sign(rollAngle))
                    {
                        rollAngle = 0;
                        rollImpulse = 0;
                    }
                    else
                        rollImpulse = -rollAngle;
                }
            }

            // dip impulse
            if (dipImpulse != 0)
            {
                // lerp added angle to zero
                float smoothing = 10 * d;
                targetRotation.x = Mathf.LerpAngle(targetRotation.x, targetRotation.x + dipImpulse, smoothing);
                dipImpulse = Mathf.LerpAngle(dipImpulse, 0, smoothing);

                if (Mathf.Abs(dipImpulse) <= 1f)
                {
                    // arrived at peak
                    if (Mathf.Abs(dipAngle) > 0)
                    {
                        dipImpulse = -dipAngle;
                        dipAngle = 0;
                    }
                    else
                    {
                        // arrived at origin
                        dipImpulse = 0;
                    }
                }
            }

            // Shake
            Vector3 targetPosition;
            if (shakeAmount > 0)
            {
                cameraShake = Vector3.Lerp(cameraShake, Vector3.zero, shakeSpeed);
                if (Vector3.Distance(cameraShake, Vector3.zero) <= shakeAmount)
                    cameraShake = shakeAmount * Random.insideUnitCircle;

                targetPosition = cameraContainer.localPosition + cameraShake;
                cameraContainer.localPosition = Vector3.Lerp(cameraContainer.localPosition, targetPosition, shakeSpeed);
                shakeAmount = 0;
            }

            #region Noise
            if (effects.camera.tilt.enableNoise)
            {
                float smoothing = 5 * d;
                float x = effects.camera.tilt.noiseMultiplier * Mathf.Sin(t) / 5;
                float y = effects.camera.tilt.noiseMultiplier * Mathf.Cos(2 * t);

                targetRotation.x = Mathf.LerpAngle(targetRotation.x, targetRotation.x + x, smoothing);
                targetRotation.z = Mathf.LerpAngle(targetRotation.z, targetRotation.z + y, smoothing);
            }
            #endregion Noise

            // apply tilt
            cameraContainer.localRotation = Quaternion.Euler(targetRotation);

            #region Headbob
            if (effects.camera.tilt.enableHeadbob)
            {
                float yBobAmount = 0;
                float xBobAmount = 0;
                float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
                float moveMultiplier = (currSpeed / movement.speed);
                float speed = effects.camera.tilt.bobFrequency;
                targetPosition = cameraContainer.localPosition;

                if (magnitude > 1 && grounded &&
                    ((movementState <= MovementState.Crouching && movementState != MovementState.Sliding)))
                {
                    yBobAmount = effects.camera.tilt.verticalBob * (1 + moveMultiplier) / 4 *
                        Mathf.Sin(speed * moveMultiplier * t);
                    xBobAmount = effects.camera.tilt.horizontalBob * (1 + moveMultiplier) / 4 *
                        Mathf.Sin(speed * moveMultiplier / 2 * t);
                }

                targetPosition.y = Mathf.Lerp(targetPosition.y, cameraOffset.y + yBobAmount, speed * d);
                targetPosition.x = Mathf.Lerp(targetPosition.x, cameraOffset.x + xBobAmount, speed * d);
                cameraContainer.localPosition = targetPosition;
            }
            #endregion Headbob

            #region FOV
            if (effects.camera.dynamicFOV.enable)
            {
                float targetFov;
                float speed = rb.velocity.magnitude;
                float diff = effects.camera.dynamicFOV.speed - movement.speed;

                if (speed <= movement.speed || diff <= 0)
                    targetFov = defaultFOV;
                else if (speed >= effects.camera.dynamicFOV.speed)
                    targetFov = defaultFOV * effects.camera.dynamicFOV.multiplier;
                else
                    targetFov = defaultFOV
                        * (1 + ((effects.camera.dynamicFOV.multiplier - 1)
                        * (speed - movement.speed) / diff));

                float fov = Mathf.Lerp(cam.fieldOfView, targetFov, 5 * d);
                cam.fieldOfView = fov;
            }
            #endregion FOV
        }

        void SideTilt(float angle)
        {
            if (!leanInput) // have to check if leaning since it also affects tilt
            {
                sideTiltAngle = angle * -30;
            }
        }

        void RollTilt(float angle)
        {
            rollAngle = angle * -10;
            rollImpulse = rollAngle;
        }

        void ResetRoll() 
        {

            rollImpulse -= rollAngle;
            rollImpulse = 0;
        }

        void DipTilt(float angle) 
        {
            dipAngle = angle * 10;
            dipImpulse = dipAngle;
        }

        void ResetDip() 
        {
            dipImpulse -= dipAngle;
            dipAngle = 0;
        }

        void Shake(float amount, float speed) 
        {
            shakeAmount = amount;
            shakeSpeed = speed * d;
        }

        void HandleEffects() 
        {
            if (!effects.enable)
                return;

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
        IEnumerator slideFrictionCoroutine;
        IEnumerator exitClimbCoroutine;
        IEnumerator exitWallRunCoroutine;

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
            if (wallStickCoroutine != null)
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
        void StartSlideCooldownCoroutine()
        {
            StopSlideCooldownCoroutine();
            slideBoostCoroutine = YieldSlideCooldown();
            StartCoroutine(slideBoostCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopSlideCooldownCoroutine()
        {
            if (slideBoostCoroutine != null)
                StopCoroutine(slideBoostCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldSlideCooldown()
        {
            slide.boosted = true;
            yield return new WaitForSeconds(slide.cooldown);
            slide.boosted = false;
        }

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartSlideFrictionCoroutine()
        {
            StopSlideFrictionCoroutine();
            slideFrictionCoroutine = YieldSlideFrictionTime();
            StartCoroutine(slideFrictionCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopSlideFrictionCoroutine()
        {
            if (slideFrictionCoroutine != null)
                StopCoroutine(slideFrictionCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldSlideFrictionTime()
        {
            slide.applyFriction = false;
            yield return new WaitForSeconds(slide.frictionDelay);
            slide.applyFriction = true;
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
            StartCoroutine(YieldDashCooldown());
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
        /// YieldDashTime
        /// Waits for <code>FujinDashModule.duration</code> and resets movement.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldKickTime()
        {
            yield return new WaitForSeconds(kick.time);
            Kick();
        }

        /// <summary>
        /// Waits for <code>FujinDashModule.cooldownTime</code> and increments <code>numDashes</code>.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldKickResetTime(bool shake)
        {
            if (shake)
            {
                float resetTime = 0;
                while (resetTime < kick.resetTime)
                {
                    yield return new WaitForFixedUpdate();
                    Shake(effects.camera.tilt.kickShakeAmount, effects.camera.tilt.kickShakeSpeed);
                    resetTime += d;
                }
            } else
                yield return new WaitForSeconds(kick.resetTime);

            EndKick();
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

        /// <summary>
        /// Restarts the coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StartExitWallRunCoroutine()
        {
            if (!wallrun.useDuration)
                return;

            StopExitWallRunCoroutine();
            exitWallRunCoroutine = YieldExitWallRunTime();
            StartCoroutine(exitWallRunCoroutine);
        }

        /// <summary>
        /// Stops the current coroutine <code>exitClimbCoroutine</code>
        /// </summary>
        void StopExitWallRunCoroutine()
        {
            if (exitWallRunCoroutine != null)
                StopCoroutine(exitWallRunCoroutine);
        }

        /// <summary>
        /// Waits for <code>FujinClimbModule.duration</code> and exits climb.
        /// </summary>
        /// <returns></returns>
        IEnumerator YieldExitWallRunTime()
        {
            yield return new WaitForSeconds(wallrun.duration);
            ExitWallRun();
        }

        #endregion Coroutines

        #endregion Private Methods

        #region Public Methods

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
            obj.AddComponent<PlayerMotor>();
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

