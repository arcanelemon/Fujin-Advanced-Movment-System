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
        public float decceleration;
        public float tiltAngle;
        public float tiltSpeed;

        public bool useAcceleration;
        public bool useDecceleration;
        public bool useTilt;
    }

    [System.Serializable]
    public class FujinHeadbobModule : FujinMotorModule
    {
        public float verticalAmount;
        public float horizontalAmount;
        public float speedMultiplier;
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
        public float lurchPeriod;
        public float lurchStrength;
        public float lurchRedirect;
        public bool enableLurch;
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
        public float control;
        public bool infinite;
        public bool hold;
    }

    [System.Serializable]
    public class FujinDashModule : FujinMotorModule
    {
        public int count;
        public float force;
        public float duration;
        public float cooldownTime;
        public bool canDashInAir;
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
        public float minSpeed;
        public float accelaration;
        public float boost;
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
    }

    [System.Serializable]
    public class FujinSpeedlinesModule : FujinMotorModule
    {
        public float amount;
        public float thiness;
        public float radius;
        public float curve;
        public float minVelocity;
        public float maxVelocity;
        public Gradient gradient;
        public GameObject prefab;
    }

    [System.Serializable]
    public class FujinGrappleModule : FujinMotorModule
    {
        public float range;
        public float throwSpeed;
        public float speed;
        public float cooldownTime;
        public GameObject prefab;
    }

    [System.Serializable]
    public class FujinPhysicsModule : FujinMotorModule
    {
        public float gravityMultiplier;
        public bool limitVelocity;
        public Vector3 maxVelocity;
        public float airDrag;
        public float airTurnRate;
        public bool turnRateFromVelocity;
        public float stepHeight;
        public float stepDepth;
        public float slopeAngle;
        public float slopeForce;
        public float wallStick;
        public bool useWallStickDuration;
        public float wallStickDuration;
        public float wallStickTilt;
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
        [HideInInspector]
        public bool enable;
        [HideInInspector]
        public bool foldout; // inspector only

        //public abstract void Reset();
    }
}
