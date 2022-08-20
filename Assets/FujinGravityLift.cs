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
    public bool expandToFit;
    public float height;
    public Type type;

    List<Rigidbody> rigidbodies = new List<Rigidbody>();

    #endregion Variables
    
    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Rigidbody rb in rigidbodies) 
        {
            Vector3 velocity;
            velocity = transform.InverseTransformDirection(rb.velocity);
            velocity.y = type > 0? Mathf.Max(velocity.y, -speed) : Mathf.Min(velocity.y, speed);
            velocity.y = Mathf.Lerp(velocity.y, (float)type * speed, acceleration * 10 * Time.deltaTime);

            // Pull towards center
            Vector3 position = transform.InverseTransformPoint(rb.position);
            Vector3 direction = (Vector3.zero - position).normalized;

            velocity.x += direction.x
                * centerForce * Time.deltaTime;
            velocity.z += direction.z
                * centerForce * Time.deltaTime;

            rb.velocity = transform.TransformDirection(velocity);
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

            rigidbodies.Add(other.attachedRigidbody);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null) 
            rigidbodies.Remove(other.attachedRigidbody);
    }

    #endregion Engine

    #region Private Methods

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
