using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Addons
{ 
    public class GravityLift : MonoBehaviour
    {
    #region Variables

    const float DIMINISHING_FACTOR = .1f;

    public enum Type 
    {
        Forward = 1,
        Backward = -1
    }

    public float speed;
    public float acceleration;
    public float centerForce;
    [Range(0, 1)]
    public float dampenFactor;
    public bool expandToFit;
    public float height;
    public float radius;
    public Type type;

    List<Rigidbody> rigidbodies = new List<Rigidbody>();

    // Rigidbody / Force
    Dictionary<Rigidbody, float> mapping = new Dictionary<Rigidbody, float>();

    #endregion Variables
    
    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Copy to remove modification warnings
        Dictionary<Rigidbody, float> newMapping = new Dictionary<Rigidbody, float>(mapping);

        // Where is a KeyValuePair of Rigidbody and Time
        foreach (KeyValuePair<Rigidbody, float> e in newMapping)
        {
            if (!e.Key) 
            {
                mapping.Remove(e.Key);
                continue;
            }

            //** Transform Vectors **//

            // Grab local velocity
            Vector3 velocity;
            velocity = transform.InverseTransformDirection(e.Key.velocity);

            // Lerp to target Y velocity
            velocity.y = type > 0? Mathf.Max(velocity.y, -speed) : Mathf.Min(velocity.y, speed);
            velocity.y = Mathf.Lerp(velocity.y, (float)type * speed, acceleration * 10 * Time.deltaTime);

            // Pull towards center
            Vector2 position = new Vector2(transform.InverseTransformPoint(e.Key.position).x, 
                transform.InverseTransformPoint(e.Key.position).z);
            Vector2 direction = (Vector2.zero - position).normalized;

            //** Apply forces **//
            // Result is direction * dampening
            float force = e.Value * Time.deltaTime;
            velocity.x += force * direction.x;
            velocity.z += force * direction.y;

            //** Dampen force **//

            // Edge Point
            Vector2 newVelocity = new Vector3(velocity.x + force * direction.x, velocity.z + force * direction.y);
            if (Vector2.Dot(new Vector2(velocity.x, velocity.z).normalized, newVelocity.normalized) < 0.1f)
            {
                // push force towards new direction
                float newForce = Vector2.Distance(Vector2.zero, position) / radius;
                mapping[e.Key] = e.Value * newForce;
            }
            else

            // Center Point
            {
                Vector2 newPosition = position + Time.deltaTime * new Vector2(velocity.x, velocity.z);
                Vector2 newDirection = (Vector2.zero - newPosition).normalized;

                if (Vector2.Dot(direction, newDirection) < 0)
                {
                    // pull velocity back by dampening factor
                    float dampenForce = 1 + dampenFactor;
                    mapping[e.Key] = e.Value * dampenForce;
                }
            }

            //** Finish up **//
            e.Key.velocity = transform.TransformDirection(velocity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            Vector3 velocity = 
                transform.InverseTransformDirection(other.attachedRigidbody.velocity);
            velocity.x = 0;
            velocity.z = 0;
            other.attachedRigidbody.velocity = velocity;

            mapping.Add(other.attachedRigidbody, centerForce);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
            mapping.Remove(other.attachedRigidbody);
    }

    #endregion Engine
    }
}
