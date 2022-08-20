using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{ 
    #region Variables

    [SerializeField]
    float rotationSpeed;

    float rotation;

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
        rotation += rotationSpeed * 100 * Time.deltaTime;
        if (rotation > 360)
            rotation -= 360;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 euler = child.localEulerAngles;
            child.localRotation = Quaternion.Euler(euler.x, euler.y, rotation);
        }
    }

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
