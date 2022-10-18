using UnityEngine;
using DecalBrush = Fujin.Utils.DecalPainter.DecalBrush;

namespace Fujin.Addons
{
    public class PhysicsEmitter : MonoBehaviour
    {
        #region Variables

        // Config

        public LayerMask collisionLayers;

        public Color color;
        public float metallicness;
        public float glossiness;
        public float radius;
        public float strength;
        public float hardness;
        public bool fadeOverTime;
        public float fadeTime;
        public float fadeAmount;
        public Texture2D decal;
        public Texture2D normal;

        public bool playOnAwake;
        public bool loop;
        public bool simulateInWorldSpace;
        public int amount = 3;
        public int maxParticles = 16;
        public float size;
        public float force;
        public float delay;
        public float rate;
        public float spread;
        public float offset;

        // Instance

        bool play;
        public bool playing => play;
        bool initialized;
        int i;
        float timer;
        public DecalBrush brush;

        #endregion Variables

        #region Engine

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (initialized && play)
            {
                timer += Time.deltaTime;

                if (i >= amount)
                    Restart();
                else if ((i == 0 && timer > delay) || (i > 0 && timer > rate))
                    Spawn();
            }
        }

        #endregion Engine

        #region Private Methods

        void Init()
        {
            if (playOnAwake)
                play = true;

            brush = new DecalBrush(decal, color, radius, strength, hardness, metallicness, glossiness, normal);
            initialized = true;
        }

        void Spawn()
        {
            i++;
            timer = 0;

            GameObject emission = new GameObject();
            emission.transform.position = transform.position + offset * transform.forward + Random.Range(-spread, spread) * transform.right + Random.Range(-spread, spread) * transform.up;
            emission.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
            emission.transform.parent = transform;
            emission.transform.localScale *= size;

            emission.AddComponent<PhysicsParticle>().Init(this); 
            emission.GetComponent<Rigidbody>().AddForce(force * 100 * transform.forward);
        }

        void Restart()
        {
            i = 0;
            timer = 0;

            if (!loop)
                Stop();
        }

        #endregion Private Methods

        #region Public Methods

        public void Play()
        {
            play = true;
        }

        public void Stop()
        {
            play = false;
        }

        public void ForceUpdate()
        {
            Init();
        }

        #endregion Public Methods
    }
}
