using UnityEngine;
using UnityEditor;
using Fujin.Addons;

namespace Fujin.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PhysicsEmitter))]
    public class PhysicsEmitterEditor : UnityEditor.Editor
    {
        #region Variables

        bool initialized;

        PhysicsEmitter emitter;

        #endregion Variables

        #region Engine

        void OnEnable()
        {
            emitter = (PhysicsEmitter)target;
        }

        public override void OnInspectorGUI()
        {
            if (!initialized)
                Init();

            EditorUtility.SetDirty(emitter);
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Surface", EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionLayers"));
            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            GUILayout.Space(5);
            emitter.decal = (Texture2D)EditorGUILayout.ObjectField("Decal", emitter.decal, typeof(Texture2D), false);
            emitter.normal = (Texture2D)EditorGUILayout.ObjectField("Normal", emitter.normal, typeof(Texture2D), false);
            GUILayout.Space(5);
            emitter.color = EditorGUILayout.ColorField("Color", emitter.color);
            emitter.metallicness = EditorGUILayout.Slider("Metallic", emitter.metallicness, 0, 1);
            emitter.glossiness = EditorGUILayout.Slider("Glossiness", emitter.glossiness, 0, 1);
            emitter.strength = EditorGUILayout.Slider("Strength", emitter.strength, 0, 1);
            emitter.hardness = EditorGUILayout.Slider("Hardness", emitter.hardness, 0, 1);
            emitter.radius = EditorGUILayout.Slider("Radius", emitter.radius, 0, 2);
            GUILayout.BeginHorizontal();
            emitter.fadeOverTime = EditorGUILayout.Toggle("Fade Over Time", emitter.fadeOverTime);
            GUI.enabled = emitter.fadeOverTime;
            emitter.fadeTime = EditorGUILayout.FloatField(Mathf.Max(emitter.fadeTime, 0));
            GUILayout.EndHorizontal();
            if (emitter.fadeOverTime)
                emitter.fadeAmount = EditorGUILayout.Slider("Fade Amount", emitter.fadeAmount, 0, 1);
            GUI.enabled = true;
            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Simulation", EditorStyles.boldLabel);
            GUILayout.Space(5);
            emitter.playOnAwake = EditorGUILayout.Toggle("Play On Awake", emitter.playOnAwake);
            emitter.loop = EditorGUILayout.Toggle("Loop", emitter.loop);
            emitter.simulateInWorldSpace = EditorGUILayout.Toggle("World Space", emitter.simulateInWorldSpace);
            emitter.amount = EditorGUILayout.IntField("Amount", Mathf.Max(emitter.amount, 0));
            emitter.maxParticles = EditorGUILayout.IntField("Max Particles", Mathf.Max(emitter.maxParticles, 0));
            emitter.size = EditorGUILayout.FloatField("Size", Mathf.Max(emitter.size, 0));
            emitter.force = EditorGUILayout.FloatField("Force", Mathf.Max(emitter.force, 0));
            emitter.delay = EditorGUILayout.FloatField("Delay", Mathf.Max(emitter.delay, 0));
            emitter.rate = EditorGUILayout.FloatField("Rate", Mathf.Max(emitter.rate, 0));
            emitter.spread = EditorGUILayout.Slider("Spread", emitter.spread, 0, 1);
            emitter.offset = EditorGUILayout.FloatField("Offset", Mathf.Max(emitter.offset, 0));
            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Play"))
                emitter.Play();
            if(GUILayout.Button("Stop"))
                emitter.Stop();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (Application.isPlaying)
                    emitter.ForceUpdate();
            }
        }

        #endregion Engine

        #region Private Methods

        void Init()
        {
            initialized = true;
        }

        #endregion Private Methods
    }
}