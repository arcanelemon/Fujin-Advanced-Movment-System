
using UnityEngine;
using UnityEditor;
using Fujin.Addons;

namespace Fujin.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Portal))]
    public class PortalEditor : UnityEditor.Editor
    {
        #region Variables

        bool initialized;
        Portal portal;

        #endregion Variables

        #region Engine

        public override void OnInspectorGUI()
        {
            if (!initialized)
                Init();

            GUILayout.Space(10);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.renderCamera = EditorGUILayout.Toggle("Render Camera", portal.renderCamera);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.overrideColor = EditorGUILayout.Toggle("Override Color", portal.overrideColor);
            GUI.enabled = portal.overrideColor;
            EditorGUI.BeginChangeCheck();
            portal.color = EditorGUILayout.ColorField(portal.color);
            GUI.enabled = true;
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                portal.SetColor(portal.color);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.material = (Material)EditorGUILayout.ObjectField("Material", portal.material, typeof(Material), false);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                portal.SetMaterial(portal.material);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.recursionLimit = EditorGUILayout.IntSlider("Recursion Limit", portal.recursionLimit, 0, 10);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.radius = EditorGUILayout.FloatField("Portal Radius", Mathf.Max(portal.radius, 0 ));
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            portal.sphererize = EditorGUILayout.Slider("Sphererize", portal.sphererize, 0, 1);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("layers"));
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(10);
            if (EditorGUI.EndChangeCheck()) 
                serializedObject.ApplyModifiedProperties();
        }

        #endregion Engine

        #region Private Methods

        void Init()
        {
            portal = (Portal)target;
            initialized = true;
        }

        #endregion Private Methods

        #region Public Methods

        #endregion Public Methods
    }
}
