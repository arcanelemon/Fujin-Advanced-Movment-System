using UnityEngine;
using UnityEngine.Rendering;

namespace Fujin.Utils
{
    public static class DecalPainter
    {
        static int TEXTURE_SIZE = 1024;

        static int maskTextureID = Shader.PropertyToID("_MaskTexture");
        static int prepareUVID = Shader.PropertyToID("_PrepareUV");
        static int positionID = Shader.PropertyToID("_PainterPosition");
        static int hardnessID = Shader.PropertyToID("_Hardness");
        static int strengthID = Shader.PropertyToID("_Strength");
        static int radiusID = Shader.PropertyToID("_Radius");
        static int blendOpID = Shader.PropertyToID("_BlendOp");
        static int colorID = Shader.PropertyToID("_PainterColor");
        static int textureID = Shader.PropertyToID("_MainTex");
        static int uvOffsetID = Shader.PropertyToID("_OffsetUV");
        static int uvIslandsID = Shader.PropertyToID("_UVIslands");

        static RenderTexture maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        static RenderTexture extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        static RenderTexture uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        static RenderTexture supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);

        static CommandBuffer command = new CommandBuffer();
        //static Shader texturePaint = Shader.Find("TNTC/TexturePainter");
        //static Shader extendIslands = Shader.Find("Fujin/Extend Islands Shader");
        static Material paintMaterial = new Material(Shader.Find("TNTC/TexturePainter"));
        static Material extendMaterial = new Material(Shader.Find("Fujin/Extend Islands Shader"));

        public static void Erase(DecalBrush brush, Vector3 position, Renderer objectRenderer) 
        {
        
        }

        public static void Paint(DecalBrush brush, Vector3 position, Renderer objectRenderer) 
        {
            if (!objectRenderer) 
            {
                Debug.LogWarning("[WARNING] Cannot paint object. No renderer attached.");
                return;
            }

            objectRenderer.material.SetTexture(maskTextureID, extendIslandsRenderTexture);

            paintMaterial.SetFloat(prepareUVID, 0);
            paintMaterial.SetVector(positionID, position);
            paintMaterial.SetFloat(hardnessID, brush.hardness);
            paintMaterial.SetFloat(strengthID, brush.strength);
            paintMaterial.SetFloat(radiusID, brush.radius);
            paintMaterial.SetTexture(textureID, supportTexture);
            paintMaterial.SetColor(colorID, brush.color);
            extendMaterial.SetFloat(uvOffsetID, 1);
            extendMaterial.SetTexture(uvIslandsID, uvIslandsRenderTexture);

            command.SetRenderTarget(maskRenderTexture);
            command.DrawRenderer(objectRenderer, paintMaterial, 0);

            command.SetRenderTarget(supportTexture);
            command.Blit(maskRenderTexture, supportTexture);

            command.SetRenderTarget(extendIslandsRenderTexture);
            command.Blit(maskRenderTexture, extendIslandsRenderTexture, extendMaterial);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }

        public class DecalBrush 
        {
            public Color color;
            public float metallicness;
            public float glossiness;

            public float radius;
            public float strength;
            public float hardness;

            public Texture2D decal;
            public Texture2D normal;

            public DecalBrush(Texture2D decal, Color color, float radius = 1, float strength = .5f, float hardness = .5f, float metallicness = 0, float glossiness = 0, Texture2D normal = null) 
            {
                this.decal = decal;
                this.color = color;
                this.radius = radius;
                this.strength = strength;
                this.hardness = hardness;
                this.metallicness = metallicness;
                this.glossiness = glossiness;
                this.normal = normal;
            }
        }
    }
}
