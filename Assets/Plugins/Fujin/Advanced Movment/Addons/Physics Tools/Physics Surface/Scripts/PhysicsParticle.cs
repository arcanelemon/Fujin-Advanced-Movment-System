using System;
using System.Collections.Generic;
using UnityEngine;
using DecalPainter = Fujin.Utils.DecalPainter;

namespace Fujin.Addons
{
    public class PhysicsParticle : MonoBehaviour
    {
        #region Variables

        Vector3 previousPosition;
        PhysicsEmitter emitter;

        #endregion Variables

        #region Engine

        void FixedUpdate()
        {
            previousPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (emitter.simulateInWorldSpace)
            {
                Vector3 direction = (previousPosition - transform.position).normalized;
                transform.position += Vector3.Distance(previousPosition, transform.position) * direction;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((emitter.collisionLayers & (1 << collision.gameObject.layer)) != 0)
                return;

            DecalPainter.Paint(emitter.brush, collision.contacts[0].point, collision.gameObject.GetComponent<Renderer>());
            Destroy(gameObject);
        }

        #endregion Engine

        #region Public Methods

        public void Init() 
        {
            previousPosition = transform.position;
            gameObject.layer = LayerMask.NameToLayer("Physics Particle");
            
            // Add Components
            gameObject.AddComponent<SphereCollider>();
            gameObject.AddComponent<Rigidbody>();
            Renderer renderer = gameObject.AddComponent<MeshRenderer>();

            // Create Shape
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", emitter.color);
            renderer.SetPropertyBlock(propBlock);
        }

        public void Init(PhysicsEmitter emitter)
        {
            this.emitter = emitter;
            Init();
        }

        #endregion Public Methods
    }
}
