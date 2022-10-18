using UnityEngine;
using UnityEditor;
using Fujin.Addons;

namespace Fujin.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PhysicsParticle))]
    public class PhysicsParticleEditor : UnityEditor.Editor
    {
        #region Variables

        bool initialized;

        PhysicsParticle particle;

        #endregion Variables

        #region Engine

        void OnEnable()
        {
            particle = (PhysicsParticle)target;
        }

        public override void OnInspectorGUI()
        {
            if (!initialized)
                Init();

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Surface", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (Application.isPlaying)
                    particle.Init();
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