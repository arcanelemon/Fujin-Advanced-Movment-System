using UnityEngine;

namespace Fujin.Addons
{
    public class PhysicsEmitter : MonoBehaviour
    {
        #region Variables

        // Config

        public bool overrideMaterialColor;
        public Color color;
        public Material material;
        public bool fadeOnOverlap;
        public float fadeOverlapAmount;
        public int maxOverlaps;
        public float splashRadius;
        public Mesh[] shapes;
        public GameObject surface;

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
            if (!material)
            {
                Debug.LogError("[ERROR] Material cannot be NULL.");
                return;
            }
            else if (shapes.Length == 0)
            {
                Debug.LogWarning("[Warning] Please assign mesh shapes before instantiation.");
                return;
            }
            else if (!surface)
            {
                Debug.LogError("[ERROR] Surface cannot be NULL.");
                return;
            }
            else if (!surface.GetComponent<PhysicsSurface>())
            {
                Debug.LogError("[ERROR] Surface requires component of type <FujinPhysicsSurface>.");
                return;
            }

            if (playOnAwake)
                play = true;

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

            emission.AddComponent<SphereCollider>().isTrigger = true;
            emission.AddComponent<Rigidbody>().AddForce(force * 100 * transform.forward);
            emission.AddComponent<MeshFilter>().sharedMesh = shapes[Random.Range(0, shapes.Length - 1)];

            Renderer renderer = emission.AddComponent<MeshRenderer>();
            renderer.material = material;
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propBlock);

            emission.AddComponent<PhysicsParticle>().Init(this);
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
