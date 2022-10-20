using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Action = Fujin.Utils.Action;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Collection of <code>FujinMotorModule</code> that store that properties used by <code>FujinPlayerMotor</code>
/// </summary>
namespace Fujin.Movement
{
    [System.Serializable]
    public class MovementModule : MotorModule
    {
        public float speed;
        public float acceleration;
        public float airSpeed;
        public float airAcceleration;
    }

    [System.Serializable]
    public class InteractModule : MotorModule
    {
        // Config
        public float drawDistance;
        public float drawRadius;
        public Material highlightMaterial;
        public LayerMask interactLayer;
        public Action interactAction;

        public bool enableHold;
        public bool holdFoldout;
        public float holdFadeAmount;
        public float defaultHoldTime;
        public Action holdAction;

        public bool enablePickup;
        public bool pickupFoldout;
        public float pickupFadeAmount;
        public LayerMask pickupLayer;
        public float pickupForce;
        public bool pickupCenterObjects;
        public float pickupCenterDistance;
        public bool pickupLimitSpeed;
        public float maxPickupWeight;
        public float throwForce;
        public bool enableThrowCharge;
        public float throwChargeTime;
        public float throwGracePeriod;

        // Instance
        Collider focus;
        Renderer focusRenderer;
        Rigidbody focusRb;
        bool pickup;
        public bool isFocused => focus;
        public bool isPickedUp => pickup;
        public Rigidbody rigidbody => focusRb;
        float holdProgress;
        float throwProgress;

        public void DrawGizmos(Vector3 origin, Vector3 direction)
        {
            Gizmos.color = Color.magenta;
            Vector3 destination = origin + drawDistance * direction;
            Gizmos.DrawWireSphere(destination, drawRadius);
            Gizmos.DrawLine(origin, destination);
        }

        public float GetPickupMult() 
        {
            if (!pickupLimitSpeed)
                return 1;

            return Mathf.Clamp((rigidbody.mass / maxPickupWeight), 0 , 1);
        }

        public void Pickup() 
        {
            focusRb = focus.GetComponent<Rigidbody>();

            if (!focusRb)
                return;

            focusRb.useGravity = false;
            focusRb.freezeRotation = true;
            pickup = true;
        }

        public void Release() 
        {
            focusRb.useGravity = true;
            focusRb.freezeRotation = false;

            focusRb = null;
            pickup = false;
            ResetThrow();
        }

        public bool IncrementThrowCharge()
        {
            if (!enableThrowCharge)
                return true;

            throwProgress += Time.deltaTime;
            return GetThrowCharge() >= 1 + (throwGracePeriod / throwChargeTime);
        }

        public float GetThrowCharge() 
        {
            return throwProgress / (throwChargeTime + throwGracePeriod);
        }

        public float GetThrowMult() 
        {
            return enableThrowCharge ? GetThrowCharge() : 1;
        }

        public void Throw(Vector3 direction)
        {
            focusRb.velocity = Vector3.zero;
            Rigidbody rb = focusRb;
            focusRb.AddForce(throwForce * 10 * GetThrowMult() * direction);
            Release();
        }

        public void ResetThrow() 
        {
            throwProgress = 0;
        }

        public void Fire()
        {
            interactAction.Invoke(focus.gameObject);
            ResetHoldProgress();
        }

        public bool CheckHoldProgress()
        {
            if (enableHold)
            {
                if (GetHoldProgress() < 1.0f)
                    return false;
            }

            return true;
        }

        public void ResetHoldProgress()
        {
            holdProgress = 0;
        }

        public float GetHoldProgress()
        {
            float progress;
            object o = holdAction.Invoke(focus.gameObject);
            if (o == null)
            {
                // default timer
                holdProgress += Time.deltaTime;
                progress = holdProgress / defaultHoldTime;
            } else
                progress = (float)o;

            return progress;
        }

        public void SetFocus(Collider focus)
        {
            int i = 0;
            Material[] materials, newMaterials;

            if (this.focus && highlightMaterial != null)
            {
                // remove material
                materials = focusRenderer.materials;
                newMaterials = new Material[materials.Length - 1];
                foreach (Material m in materials)
                {
                    if (m.name.Replace(" (Instance)", "") != highlightMaterial.name)
                    {
                        newMaterials[i] = m;
                        i++;
                    }
                }

                focusRenderer.materials = newMaterials;
            }

            // new focus
           this.focus = focus;

            // add material
            if (highlightMaterial != null)
            {
                i = 0;
                focusRenderer = focus.GetComponent<Renderer>();
                materials = focusRenderer.materials;
                newMaterials = new Material[materials.Length + 1];
                foreach (Material m in materials)
                {
                    newMaterials[i] = m;
                    i++;
                }
                newMaterials[materials.Length] = highlightMaterial;
                focusRenderer.materials = newMaterials;
            }
        }

        public Collider GetFocus() 
        {
            return focus;
        }

        public void ClearFocus() 
        {
            // clear material
            if (focus && highlightMaterial != null)
            {
                int i = 0;
                Material[] materials = focusRenderer.materials;
                Material[] newMaterials = new Material[materials.Length - 1];
                foreach (Material m in materials)
                {
                    if (m.name.Replace(" (Instance)", "") != highlightMaterial.name)
                    {
                        newMaterials[i] = m;
                        i++;
                    }
                }

                focusRenderer.materials = newMaterials;
                focusRenderer = null;
            }

            // remove focus
            focus = null;
        }
    }

    [System.Serializable]
    public class JumpModule : MotorModule
    {
        public float force;
        public float gracePeriod;
        public DoubleJump doubleJump = new DoubleJump();
        public Hover hover = new Hover();
        public WallJump wallJump = new WallJump();
        
        [System.Serializable]
        public class DoubleJump
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public int amount;
            public float force;
            public float redirect;
        }

        [System.Serializable]
        public class Hover
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public float fallForce;
            public float duration;
            public bool useDuration;
        }

        [System.Serializable]
        public class WallJump
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public float force;
        }
    }

    [System.Serializable]
    public class SprintModule : MotorModule
    {
        public float speedMultiplier;
    }

    [System.Serializable]
    public class CrouchModule : MotorModule
    {
        public float height;
        public float speedMultiplier;
    }

    [System.Serializable]
    public class SlideModule : MotorModule
    {
        // Config
        public float height;
        public float boost;
        public float cooldown;
        public float slopeAcceleration;
        public float friction;
        public float frictionDelay;
        public float control;
        public bool multidirectional;

        // Instance
        public bool boosted;
        public bool applyFriction;
    }

    [System.Serializable]
    public class DashModule : MotorModule
    {
        public int count;
        public float force;
        public float duration;
        public float momentum;
        public float cooldownTime;
        public bool canDashInAir;
        public bool omnidirectional;
        public Vector3 axis;
    }

    [System.Serializable]
    public class PoundModule : MotorModule
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
    public class MantleModule : MotorModule
    {
        public float speed;
        public float height;
    }

    [System.Serializable]
    public class ClimbModule : MotorModule
    {
        // Config
        public float speed;
        public bool useDuration;
        public float duration;

        // Instance
        public Collider collider;
    }

    [System.Serializable]
    public class WallrunModule : MotorModule
    {
        public float speed;
        public float accelaration;
        public float decelleration;
        public float breakSpeed;
        public float pushForce;
        public float boost;
        public float drag;
        public float minHeight;
        public bool useDuration;
        public float duration;

        // Instance
        public bool decellerate;
        public bool rotated;
        public Collider collider;
    }

    [System.Serializable]
    public class KickModule : MotorModule
    {
        public float force;
        public float time;
        public float resetTime;
        public float speedMultiplier;
        public float lookResistance;
        public LayerMask kickLayer;
        public Action action = new Action();
    }

    [System.Serializable]
    public class SlowwalkModule : MotorModule
    {
        public float speedMultiplier;
    }

    [System.Serializable]
    public class AutorunModule : MotorModule
    {
    }

    [System.Serializable]
    public class LeanModule : MotorModule
    {
        public float speed;
        public float angle;
        public float offset;
        public float speedMultiplier;
    }

    [System.Serializable]
    public class FreelookModule : MotorModule
    {
        public Vector2 axis;
        public float resetSpeed;
        public float horizontalAngle;
        public float verticalAngle;
        public float lookResistance;
    }

    [System.Serializable]
    public class GrappleModule : MotorModule
    {
        public bool controlFoldout;
        public bool simFoldout;
        public bool componentFoldout;
        public bool wrap;
        public int wrapCount;
        public float range;
        public float throwSpeed;
        public float speed;
        public float acceleration;
        public float maxAngle;
        public float viewControl;
        public float inputControl;
        public float lookResistance;
        public float elasticity;
        public float cooldownTime;
        public float thickness;
        public int sides;
        public int quality;
        public float wobbleAmount;
        public float wobbleWaveCount;
        public float wobbleWaveHeight;
        public float spiralAmount;
        public float spiralOffset;
        public float spiralRotationSpeed;
        public float spiralDamper;
        public SimulationType simType;
        public Transform pivot;
        public GameObject impactEffect;
        public GameObject breakEffect;
        public GameObject prefab;

        public enum SimulationType 
        {
            None = 0,
            Wobble = 1,
            Spiral = 2
        }
    }

    [System.Serializable]
    public class EffectsModule : MotorModule
    {
        public CameraFX camera = new CameraFX();
        public Sway sway = new Sway();
        public Footsteps footsteps = new Footsteps();
        public Speedlines speedlines = new Speedlines();

        [System.Serializable]
        public class CameraFX
        {
            public bool enable;
            public bool foldout;
            public float fadeAmount;
            public Tilt tilt = new Tilt();
            public DynamicFOV dynamicFOV = new DynamicFOV();

            [System.Serializable]
            public class Tilt 
            {
                // Editor
                public bool foldout;
                public int tab;

                public float movementTiltVertical;
                public float movementTiltHorizontal;
                public float jumpTiltVertical;
                public float jumpTiltHorizontal;
                public float impactTiltVertical;
                public float impactTiltHorizontal;
                public float crouchTiltVertical;
                public float crouchTiltHorizontal;
                public float slideTilt;
                public float slideShakeAmount;
                public float dashTilt;
                public float mantleTiltVertical;
                public float mantleTiltHorizontal;
                public float wallRunTiltHorizontal;
                public float wallStickTilt;
                public float kickTilt;
                public float kickShakeAmount;
                public float kickShakeSpeed;
                public float grappleTilt;

                public bool enableNoise;
                public float noiseMultiplier;

                public bool enableHeadbob;
                public float bobFrequency;
                public float verticalBob;
                public float horizontalBob;
            }

            [System.Serializable]
            public class DynamicFOV 
            {
                public bool enable;
                public bool foldout;
                public float multiplier;
                public float speed;
            }
        }

        [System.Serializable]
        public class Footsteps
        {
            // Editor
            public bool enable;
            public bool foldout;
            public float fadeAmount;

            // Fields
            public float frequency;
            public Vector2 volumeRange;
            public Vector2 pitchRange;
            public AudioClip sound;

            // Instance
            public AudioSource source;
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
    public class PhysicsModule : MotorModule
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
        public float friction;
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
    public class InputModule : MotorModule
    {
        public float lookSensitivity;
        public Vector2 lookAxis = Vector2.one;

#if ENABLE_INPUT_SYSTEM
        public InputAction lookAction;
        public InputAction moveAction;
        public InputAction interactAction;
        public InputAction jumpAction;
        public InputAction toggleSprintAction;
        public InputAction holdSprintAction;
        public InputAction toggleCrouchSlideAction;
        public InputAction holdCrouchSlideAction;
        public InputAction dashAction;
        public InputAction poundAction;
        public InputAction kickAction;
        public InputAction autorunAction;
        public InputAction toggleLeanAction;
        public InputAction holdLeanAction;
        public InputAction toggleFreelookAction;
        public InputAction holdFreelookAction;
        public InputAction toggleSlowwalkAction;
        public InputAction holdSlowwalkAction;
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

    public abstract class MotorModule
    {
        public bool enable;

        // inspector only
        public bool foldout; 
        public float fadeAmount;
    }
}
