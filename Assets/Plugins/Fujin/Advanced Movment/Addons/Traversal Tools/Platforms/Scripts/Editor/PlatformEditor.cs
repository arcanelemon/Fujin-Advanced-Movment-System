using UnityEngine;
using UnityEditor;
using Fujin.Addons;

namespace Fujin.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Platform))]
    public class PlatformEditor : UnityEditor.Editor
    {
        #region Variables

        private const float HANDLE_SIZE = 0.1f;
        private const float PICK_SIZE = 0.15f;
        private int selectedIndex = -1;

        bool initialized;
        Platform platform;
        Texture2D editIcon;
        Texture2D rectIcon;
        Color selectColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f);
        Tool currentTool;

        #endregion Variables

        #region Engine

        void OnEnable()
        {
            platform = (Platform)target;
            currentTool = Tools.current;
        }

        void OnDisable()
        {
            Tools.current = currentTool;
        }

        public override void OnInspectorGUI()
        {
            if (!initialized)
                Init();

            GUILayout.Space(10);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.speed = EditorGUILayout.FloatField("Speed", Mathf.Max(platform.speed, 0));
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.delay = EditorGUILayout.FloatField("Start Delay", Mathf.Max(platform.delay, 0));
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.alignRotation = EditorGUILayout.Toggle("Align Rotation", platform.alignRotation);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.Label("Detection Extents", GUILayout.Width(EditorGUIUtility.labelWidth));
            platform.detectionFromScale = GUILayout.Toggle(platform.detectionFromScale, "From Scale");
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            if (!platform.detectionFromScale)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorUtils.MARGIN);
                GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
                platform.detectionExtents = EditorGUILayout.Vector3Field("", platform.detectionExtents);
                GUILayout.Space(EditorUtils.MARGIN);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.detectionOffset = EditorGUILayout.Vector3Field("Detection Offset", platform.detectionOffset);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.quality = EditorGUILayout.Slider("Path Quality", platform.quality, 0, 256);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.pathType = (Platform.PathType)EditorGUILayout.EnumPopup("Path Type", platform.pathType);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorUtils.MARGIN);
            platform.smoothType = (Platform.SmoothType)EditorGUILayout.EnumPopup("Smooth Type", platform.smoothType);
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Space(2*EditorUtils.MARGIN);
            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.clear;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waypoints"));
            GUI.backgroundColor = Color.white;
            GUILayout.Space(3);
            GUILayout.EndVertical();
            GUILayout.Space(EditorUtils.MARGIN);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            if (platform.edit) 
            {
                GUILayout.BeginVertical();
                GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus@2x"), GUILayout.Width(15), GUILayout.MaxHeight(9));
                GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), GUILayout.Width(15), GUILayout.MaxHeight(9));
                GUILayout.EndVertical();
            }

            platform.edit = GUILayout.Toggle(platform.edit, editIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
            GUILayout.Toggle(false, rectIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
            platform.show = GUILayout.Toggle(platform.show, EditorGUIUtility.IconContent("d_ViewToolOrbit On@2x"), "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool On@2x"), GUILayout.Width(50), GUILayout.MaxHeight(20)))
                platform.Revert();
            if (EditorGUI.EndChangeCheck())
            {
                platform.UpdateCurve(platform.GetHandles());
                SceneView.RepaintAll();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            if (platform.edit && !SceneView.lastActiveSceneView.drawGizmos)
                SceneView.lastActiveSceneView.drawGizmos = true;
        }

        #endregion Engine

        #region Private Methods

        void Init()
        {
            editIcon = Resources.Load("Fujin Res/Textures/edit") as Texture2D;
            rectIcon = Resources.Load("Fujin Res/Textures/rect") as Texture2D;
            initialized = true;
        }

        public void OnSceneGUI()
        {
            if (!platform.show)
                return;


            Tools.current = Tool.None;

            // Draw Lines
            Handles.color = Color.yellow;
            Vector3 lineStart = platform.GetPoint(0f);
            for (int i = 1; i <= platform.quality; i++)
            {
                Vector3 lineEnd = platform.GetPoint(i / (float)platform.quality);
                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }

            if (platform.edit)
            {
                // Draw Handles
                Vector3[] points = platform.GetHandles();
                for (int i = 0; i < points.Length; i++)
                {
                    // Draw Lines
                    Handles.color = new Color(255, 255, 255, 0.3f);
                    if (i < points.Length - 1)
                        Handles.DrawLine(platform.GetHandle(i), platform.GetHandle(i + 1));

                    // Draw Handles
                    Handles.color = new Color(255, 255, 255, 0.8f);
                    Vector3 point = platform.transform.TransformPoint(points[i]);
                    float size = HandleUtility.GetHandleSize(point);
                    if (selectedIndex == i)
                    {
                        EditorGUI.BeginChangeCheck();
                        point = Handles.DoPositionHandle(point, platform.transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(platform, "Move Point");
                            EditorUtility.SetDirty(platform);
                            points[i] = platform.transform.InverseTransformPoint(point);
                            platform.UpdateCurve(points);
                        }
                        Handles.color = selectColor;
                    }
                    if (Handles.Button(point, platform.transform.rotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
                    {
                        selectedIndex = i;
                    }
                }
            }
            else
            {
                if (Tools.current != Tool.None && Tools.current != currentTool)
                    currentTool = Tools.current;

                Tools.current = currentTool;
            }
        }

        #endregion Private Methods

        #region Public Methods

        #endregion Public Methods
    }
}
