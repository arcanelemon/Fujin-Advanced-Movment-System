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

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            GUILayout.Space(5);
            emitter.overrideMaterialColor = EditorGUILayout.Toggle("Override Surface Color", emitter.overrideMaterialColor);
            emitter.color = EditorGUILayout.ColorField("Color", emitter.color);
            emitter.material = (Material)EditorGUILayout.ObjectField("Material", emitter.material, typeof(Material), false);
            GUILayout.BeginHorizontal();
            emitter.fadeOnOverlap = EditorGUILayout.Toggle("Fade On Overlap", emitter.fadeOnOverlap);
            GUI.enabled = emitter.fadeOnOverlap;
            emitter.fadeOverlapAmount = EditorGUILayout.Slider(emitter.fadeOverlapAmount, 0, 1);
            GUI.enabled = true;
            GUILayout.EndHorizontal(); 
            emitter.maxOverlaps = EditorGUILayout.IntSlider("Max Overlaps", emitter.maxOverlaps, 0, 10);
            emitter.splashRadius = EditorGUILayout.Slider("Splash Radius", emitter.splashRadius, 0, 2); 
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shapes"));
            GUILayout.EndHorizontal();
            emitter.surface = (GameObject)EditorGUILayout.ObjectField("Physics Surface", emitter.surface, typeof(GameObject), true);

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

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (Application.isPlaying)
                    emitter.ForceUpdate();
            }

            GUILayout.Space(5);
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