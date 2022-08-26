using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Addons
{ 
    public class Rotator : MonoBehaviour
    {
        public float rotationSpeed;
        
        void Update()
        {
            Vector3 rotation = Vector3.up;
            rotation *= rotationSpeed * 100 * Time.deltaTime;
            transform.Rotate(rotation, Space.Self);
        }
    }
}
