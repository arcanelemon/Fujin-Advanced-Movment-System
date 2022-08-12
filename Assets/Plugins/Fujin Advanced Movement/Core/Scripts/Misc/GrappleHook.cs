using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{ 
    #region Variables

    [SerializeField]
    float rotationSpeed;

    float rotation;
    Vector3 previousPosition;

    #endregion Variables
    
    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    #endregion Engine

    #region Private Methods

    void Rotate() 
    {
        rotation += rotationSpeed * 10 * Vector3.Distance(transform.position, previousPosition);
        if (rotation > 360)
            rotation -= 360;

        Vector3 euler = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(euler.x, euler.y, rotation);

        previousPosition = transform.position;
    }

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
