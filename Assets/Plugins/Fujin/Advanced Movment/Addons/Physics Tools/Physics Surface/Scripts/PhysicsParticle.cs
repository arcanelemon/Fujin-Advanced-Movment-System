using System.Collections.Generic;
using UnityEngine;

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

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger)
                return;

            SpawnSurface();
            Destroy(gameObject);
        }

        #endregion Engine

        #region Private Methods

        void SpawnSurface()
        {
            int i = 0;
            PhysicsSurface[] otherSurfaces = GameObject.FindObjectsOfType<PhysicsSurface>();
            List<PhysicsSurface> overlappingSurfaces = new List<PhysicsSurface>();
            foreach (PhysicsSurface otherSurface in otherSurfaces)
            {
                float distance = Vector3.Distance(otherSurface.transform.position + emitter.splashRadius * (transform.position - otherSurface.transform.position).normalized,
                  transform.position);
                if (distance <= emitter.splashRadius)
                {
                    i++;
                    if (i > emitter.maxOverlaps)
                        return;

                    overlappingSurfaces.Add(otherSurface);
                }
            }

            if (emitter.fadeOnOverlap)
            {
                foreach (PhysicsSurface overlappingSurface in overlappingSurfaces)
                {
                    Renderer renderer = overlappingSurface.GetComponent<Renderer>();
                    if (renderer)
                    {
                        float distance = Vector3.Distance(overlappingSurface.transform.position + emitter.splashRadius * (transform.position - overlappingSurface.transform.position).normalized,
                        transform.position);
                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(propBlock);
                        Color color = propBlock.GetColor("_Color");
                        float a = color.a - (emitter.fadeOverlapAmount * (distance / emitter.splashRadius));

                        if (a <= 0)
                        {
                            Destroy(GetComponent<Collider>().gameObject);
                            continue;
                        }

                        color.a = a;
                        propBlock.SetColor("_Color", color);
                        renderer.SetPropertyBlock(propBlock);
                    }
                }
            }

            GameObject surface = Instantiate(emitter.surface, transform.position,
               Quaternion.Euler(new Vector3(0, Random.Range(0f, 360f), 0)), transform.parent);
            surface.transform.localScale *= emitter.splashRadius;

            if (emitter.overrideMaterialColor)
            {
                Renderer renderer = surface.GetComponent<Renderer>();

                if (renderer != null)
                {
                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(propBlock);
                    propBlock.SetColor("_Color", emitter.color);
                    propBlock.SetFloat("_Alpha", emitter.color.a);
                    renderer.SetPropertyBlock(propBlock);
                }
            }
        }

        #endregion Private Methods

        #region Public Methods

        public void Init(PhysicsEmitter emitter)
        {
            this.emitter = emitter;
            previousPosition = transform.position;
        }

        #endregion Public Methods
    }
}
