using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FujinGravityLift : MonoBehaviour
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
        // Where is a KeyValuePair of Rigidbody and Time
        foreach (KeyValuePair<Rigidbody, float> e in mapping) 
        {
            // Grab local velocity
            Vector3 velocity;
            velocity = transform.InverseTransformDirection(e.Key.velocity);

            // Lerp to target Y velocity
            velocity.y = type > 0? Mathf.Max(velocity.y, -speed) : Mathf.Min(velocity.y, speed);
            velocity.y = Mathf.Lerp(velocity.y, (float)type * speed, acceleration * 10 * Time.deltaTime);

            // Pull towards center
            Vector3 position = transform.InverseTransformPoint(e.Key.position);
            Vector2 direction = (Vector2.zero - new Vector2(position.x, position.z)).normalized;

            // Apply forces
            // Result is direction * dampening
            float force = e.Value * Time.deltaTime;
            velocity.x += direction.x * force;
            velocity.z += direction.y   * force;

            //** Dampen force **//

            // Center Point
            Vector2 newPosition = new Vector2(
                position.x + (velocity.x * Time.deltaTime), 
                position.z + (velocity.z * Time.deltaTime));
            Vector2 newDirection = (Vector2.zero - newPosition).normalized;
            //float dot = Vector3.Dot(direction, newDirection);

            if (Vector3.Dot(direction, newDirection) < 0)
            {
                // pull velocity back by dampening factor
                mapping[e.Key] = e.Value * (1 + dampenFactor);
            }

            // Edge Point
            Vector3 newVelocity = new Vector3
                (velocity.x + force * direction.x, velocity.y, velocity.z + force * direction.y);
            //dot = Vector3.Dot(velocity, newVelocity);

            if (Vector3.Dot(velocity, newVelocity) < 0)
            {
                // push velocity towards center
                mapping[e.Key] = centerForce;
            }

            // Finish up
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

    #region Private Methods

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
