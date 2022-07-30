using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Collection of <code>FujinMotorModule</code> that store that properties used by <code>FujinPlayerMotor</code>
/// </summary>
namespace Fujin
{
    [System.Serializable]
    public class FujinMovementModule : FujinMotorModule
    {
        public float speed;
        public float acceleration;
        public float airSpeed;
        public float airAcceleration;
        public float tiltAngle;
        public float tiltSpeed;

        public bool useAcceleration;
        public bool useTilt;
    }

    [System.Serializable]
    public class FujinJumpModule : FujinMotorModule
    {
        public int doubleJumpCount;
        public float force;
        public float gracePeriod;
        public float doubleJumpforce;
        public bool enableDoubleJumps;
        public float wallJumpforce;
        public bool enableWallJump;
        public float driftPeriod;
        public float driftStrength;
        public float driftRedirect;
        public bool enableDrift;

        public bool driftFoldout;
        public bool doubleJumpFoldout;
        public bool wallFoldout;

        public float doubleJumpFadeAmount;
        public float driftFadeAmount;
        public float wallFadeAmount;
    }

    [System.Serializable]
    public class FujinSprintModule : FujinMotorModule
    {
        public float speedMultiplier;
        public bool hold;
    }

    [System.Serializable]
    public class FujinCrouchModule : FujinMotorModule
    {
        public bool hold;
        public float height;
        public float speedMultiplier;
    }

    [System.Serializable]
    public class FujinSlideModule : FujinMotorModule
    {
        public float height;
        public float force;
        public float friction;
        public float maxSpeed;
        public float tiltAngle;
        public float tiltSpeed;
        public float control;
        public bool infinite;
        public bool hold;
    }

    [System.Serializable]
    public class FujinDashModule : FujinMotorModule
    {
        public int count;
        public float force;
        public float momentum;
        public float duration;
        public float cooldownTime;
        public bool canDashInAir;
        public bool omnidirectional;
        public Vector4 axis;
    }

    [System.Serializable]
    public class FujinPoundModule : FujinMotorModule
    {
        public float force;
        public float duration;
        public float bounce;
        public float knockback;
        public float knockbackUpwardsModifier;
        public float knockbackRadius;
        public bool carryMomentum;
        public bool multiplyByHeight;
        public bool useDuration;
    }

    [System.Serializable]
    public class FujinMantleModule : FujinMotorModule
    {
        public float speed;
        public float height;
        public bool transitionFromGround;
    }

    [System.Serializable]
    public class FujinClimbModule : FujinMotorModule
    {
        public float speed;
        public bool useDuration;
        public float duration; 
        public bool transitionFromGround;
    }

    [System.Serializable]
    public class FujinWallrunModule : FujinMotorModule
    {
        public float speed;
        public float accelaration;
        public bool useAccelaration;
        public float boost;
        public bool useBoost;
        public float minHeight;
        public bool useDrag;
        public float drag;
        public bool useDuration;
        public float duration;
    }

    [System.Serializable]
    public class FujinSlowwalkModule : FujinMotorModule
    {
        public bool hold;
        public float speedMultiplier;
    }

    [System.Serializable]
    public class FujinAutorunModule : FujinMotorModule
    {
    }

    [System.Serializable]
    public class FujinLeanModule : FujinMotorModule
    {
        public bool hold;
        public float speed;
        public float angle;
        public float offset;
        public float speedMultiplier;
    }

    [System.Serializable]
    public class FujinFreelookModule : FujinMotorModule
    {
        public bool hold;
        public Vector2 axis;
        public float resetSpeed;
        public float horizontalAngle;
        public float verticalAngle;
        public float lookResistance;
    }

    [System.Serializable]
    public class FujinGrappleModule : FujinMotorModule
    {
        public float range;
        public float throwSpeed;
        public float speed;
        public float maxAngle;
        public float control;
        public float lookResistance;
        public float cooldownTime;
        public Vector3 offset;
        public GameObject prefab;
    }

    [System.Serializable]
    public class FujinEffectsModule : FujinMotorModule
    {
        public Headbob headbob = new Headbob();
        public Sway sway = new Sway();
        public FOV fov = new FOV();
        public Speedlines speedlines = new Speedlines();

        [System.Serializable]
        public class Headbob
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public float verticalAmount;
            public float horizontalAmount;
            public float speedMultiplier;
        }

        [System.Serializable]
        public class Sway
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public bool idleFoldout;
            public bool lookFoldout;
            public bool moveFoldout;
            public bool fallFoldout;
            public bool impactFoldout;
            public Vector2 idleTilt;
            public Vector2 idleOffset; 
            public float idleSpeed;
            public Vector2 lookTilt;
            public Vector2 lookOffset; 
            public float lookSpeed;
            public float moveTilt;
            public float moveOffset; 
            public float moveSpeed;
            public float fallTilt;
            public float fallOffset; 
            public float fallSpeed;
            public float impactTilt;
            public float impactOffset; 
            public float impactSpeed;

            public Transform container;
        }

        [System.Serializable]
        public class FOV 
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public float multiplier;
            public float speed;
            public float minVelocity;
            public float maxVelocity;
        }

        [System.Serializable]
        public class Speedlines
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public float amount;
            public float thiness;
            public float radius;
            public float curve;
            public float minVelocity;
            public float maxVelocity;
            public Gradient gradient;
            public GameObject prefab;
        }
    }

    [System.Serializable]
    public class FujinPhysicsModule : FujinMotorModule
    {
        public bool generalFoldout = true;
        public bool airFoldout = true;
        public bool stepFoldout = true;
        public bool slopeFoldout = true;
        public bool wallFoldout = true;
        public bool layerFoldout = true;

        public float generalFadeAmount;
        public float airFadeAmount;
        public float stepFadeAmount;
        public float slopeFadeAmount;
        public float wallFadeAmount;
        public float layersFadeAmount;

        public float gravityMultiplier;
        public bool limitVelocity;
        public Vector2 maxVelocity;
        public float airDrag;
        public float airTurnRate;
        public bool turnRateFromVelocity;
        public float stepHeight;
        public float stepDepth;
        public float slopeAngle;
        public float slopeForce;
        public float slopeFallOff;
        public float wallStick;
        public bool useWallStickDuration;
        public float wallStickDuration;
        public PhysicMaterial physicMaterial;
    }

    [System.Serializable]
    public class FujinInputModule : FujinMotorModule
    {
        public float lookSensitivity;
        public Vector2 lookAxis = Vector2.one;

#if ENABLE_INPUT_SYSTEM
        public InputAction lookAction;
        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction sprintAction;
        public InputAction crouchSlideAction;
        public InputAction dashAction;
        public InputAction poundAction;
        public InputAction autorunAction;
        public InputAction leanAction;
        public InputAction freelookAction;
        public InputAction slowwalkAction;
        public InputAction grappleAction;
#else
        public InputAxis lookAction;
        public InputAxis moveAction;
        public string jumpAction;
        public string sprintAction;
        public string crouchSlideAction;
        public string dashAction;
        public string poundAction;
        public string autorunAction;
        public string leanAction;
        public string freelookAction;
        public string slowwalkAction;
        public string grappleAction;

        public class InputAxis 
        {
            public string horizontal;
            public string vertical;
        }
#endif
    }

    public abstract class FujinMotorModule
    {
        public bool enable;

        // inspector only
        public bool foldout; 
        public float fadeAmount;
    }
}
