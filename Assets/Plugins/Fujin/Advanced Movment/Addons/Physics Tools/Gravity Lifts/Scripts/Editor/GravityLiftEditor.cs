using UnityEngine;
using UnityEditor;
using Fujin.Addons;

namespace Fujin.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(GravityLift))]
    public class GravityLiftEditor : UnityEditor.Editor
    {
        #region Variables

        bool initialized;
        bool edit;
        GravityLift lift;


        #endregion Variables

        #region Engine

        void OnEnable()
        {
            lift = (GravityLift)target;
        }

        public override void OnInspectorGUI()
        {
            if (!initialized)
                Init();

            GUILayout.Space(10);
            lift.speed = EditorGUILayout.FloatField("Speed", Mathf.Max(lift.speed, 0));
            lift.acceleration = EditorGUILayout.FloatField("Acceleration", Mathf.Max(lift.acceleration, 0));
            lift.centerForce = EditorGUILayout.FloatField("Center Force", Mathf.Max(lift.centerForce, 0));
            lift.dampenFactor = EditorGUILayout.Slider("Dampen Factor", lift.dampenFactor, 0, 1);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(3);
            GUI.enabled = lift.expandToFit;
            lift.height = EditorGUILayout.FloatField("Height", Mathf.Max(lift.height, 0));
            GUI.enabled = true;
            lift.expandToFit = EditorGUILayout.Toggle("Expand To Fit", lift.expandToFit);
            lift.radius = EditorGUILayout.FloatField("Radius", Mathf.Max(lift.radius, 0));
            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); 
            GUILayout.Space(25);
            edit = GUILayout.Toggle(edit, new GUIContent("Edit Shape"), "Button");
            GUILayout.Space(25);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        public void OnSceneGUI()
        {

        }

        #endregion Engine

        #region Private Methods

        void Init()
        {
            initialized = true;
        }

        #endregion Private Methods

        #region Public Methods

        #endregion Public Methods
    }
}