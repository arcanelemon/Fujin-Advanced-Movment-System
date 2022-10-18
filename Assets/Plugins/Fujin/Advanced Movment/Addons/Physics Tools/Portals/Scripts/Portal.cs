using UnityEngine; 
using UnityEngine.Rendering;
using System.Linq;

namespace Fujin.Addons
{
    [ExecuteInEditMode]
    [System.Serializable]
    [RequireComponent(typeof(Collider))]
    public class Portal : MonoBehaviour
    {
        #region Variables

        public bool renderCamera;
        public bool overrideColor;
        public Color color = Color.white;
        public Material material;

        public int recursionLimit;
        public float radius;
        public float sphererize;
        public LayerMask layers;

        // Instance
        public bool isPaired => target != null;
        Camera camera;
        MeshRenderer renderer;
        RenderTexture renderTexture;

        // Editor
        public Portal target;

        #endregion Variables

        #region Engine

        void OnEnable()
        {
            renderer = GetComponent<MeshRenderer>();
        }

        void OnDisable()
        {
            
        }

        #endregion Engine

        #region Public Methods

        public static void Pair(Portal portal1, Portal portal2) 
        {

        }

        public void SetColor(Color color)
        { 
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propBlock);

            this.color = color;
        }

        public void SetMaterial(Material material)
        {
            renderer.material = material;
            this.material = material;
        }

        #endregion Public Methods
    }
}
