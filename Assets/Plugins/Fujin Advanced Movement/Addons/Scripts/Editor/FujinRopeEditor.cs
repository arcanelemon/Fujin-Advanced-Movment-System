
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(FujinRope))]
public class FujinRopeEditor : Editor
{
    #region Variables

    private const float HANDLE_SIZE = 0.1f;
    private const float PICK_SIZE = 0.15f;
    private int selectedIndex = -1;

    bool initialized;
    bool shapeFoldout;
    Texture2D editIcon;
    Texture2D straightenIcon;
    Color selectColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f);
    Tool currentTool;

    FujinRope rope;

    #endregion Variables

    #region Engine

    void OnEnable()
    {
        rope = (FujinRope)target;
        rope.Init();
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
        rope.speed = EditorGUILayout.FloatField("Speed", rope.speed);
        rope.wobble = EditorGUILayout.FloatField("Wobble", rope.wobble);
        EditorGUI.BeginChangeCheck();
        rope.mat = (Material)EditorGUILayout.ObjectField("Material", rope.mat, typeof(Material), true);
        if (EditorGUI.EndChangeCheck())
            rope.SetMaterial(rope.mat);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.Space(12);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("anchors")); 
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck()) 
        {
            serializedObject.ApplyModifiedProperties();
            if (rope.anchors.Length > 2)
                rope.anchors = new Transform[] { rope.anchors[0], rope.anchors[1] };
            else if (rope.anchors.Length == 1)
                rope.anchors = new Transform[] { rope.anchors[0], null };
            else if (rope.anchors.Length == 0)
                rope.anchors = new Transform[] { null, null };
        }

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(12);
        shapeFoldout = EditorGUILayout.Foldout(shapeFoldout, "Shape", true);
        GUILayout.EndHorizontal();

        if (shapeFoldout)
        {
            EditorGUI.BeginChangeCheck();
            rope.orientation = (FujinRope.Orientation)EditorGUILayout.EnumPopup("Orientation", rope.orientation);
            if (EditorGUI.EndChangeCheck())
            {
                rope.FlipOrientation();
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            rope.thickness = EditorGUILayout.FloatField("Thickness", rope.thickness);
            rope.sides = EditorGUILayout.IntSlider("Sides", rope.sides, 3, 32);
            rope.quality = EditorGUILayout.IntSlider("Quality", rope.quality, 2, 512);
            GUILayout.Space(5); 
            if (EditorGUI.EndChangeCheck())
            {
                rope.UpdateFromCurve();
                SceneView.RepaintAll();
            }
        }

        GUI.enabled = true;
        GUILayout.EndVertical();
        GUILayout.Space(3);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUI.BeginChangeCheck();
        rope.edit = GUILayout.Toggle(rope.edit, editIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
        rope.show = GUILayout.Toggle(rope.show, EditorGUIUtility.IconContent("d_ViewToolOrbit On@2x"), "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));

        if (rope.show != rope.meshActive)
            rope.ToggleMesh();

        rope.straighten = GUILayout.Toggle(rope.straighten, straightenIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool On@2x"), GUILayout.Width(50), GUILayout.MaxHeight(20)))
            rope.Revert();
        if (EditorGUI.EndChangeCheck())
        {
            rope.UpdateCurve(rope.GetHandles());
            SceneView.RepaintAll();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        if (rope.edit && !SceneView.lastActiveSceneView.drawGizmos)
            SceneView.lastActiveSceneView.drawGizmos = true;
    }

    public void OnSceneGUI()
    {
        if (rope.edit)
        {
            Tools.current = Tool.None;

            // Draw Lines
            Handles.color = Color.yellow;
            Vector3 lineStart = rope.GetPoint(0f);
            for (int i = 1; i <= rope.quality; i++)
            {
                Vector3 lineEnd = rope.GetPoint(i / (float)rope.quality);
                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }

            // Draw Handles
            Vector3[] points = rope.GetHandles();
            for (int i = 0; i < points.Length; i++)
            {
                // Draw Lines
                Handles.color = new Color(255, 255, 255, 0.3f);
                if (i < points.Length - 1)
                    Handles.DrawLine(rope.GetHandle(i), rope.GetHandle(i + 1));

                // Draw Handles
                Handles.color = new Color(255,255,255,0.8f);
                Vector3 point = rope.transform.TransformPoint(points[i]);
                float size = HandleUtility.GetHandleSize(point);
                if (selectedIndex == i)
                {
                    EditorGUI.BeginChangeCheck();
                    point = Handles.DoPositionHandle(point, rope.transform.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(rope, "Move Point");
                        EditorUtility.SetDirty(rope);
                        points[i] = rope.transform.InverseTransformPoint(point);
                        rope.UpdateCurve(points);
                    }
                    Handles.color = selectColor;
                }
                if (Handles.Button(point, rope.transform.rotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
                {
                    selectedIndex = i;
                }
            }
        } else 
        {
            if (Tools.current != Tool.None &&
                Tools.current != currentTool)
                currentTool = Tools.current;

            Tools.current = currentTool;
        }
    }

    #endregion Engine

    #region Private Methods

    void Init() 
    {
        editIcon = Resources.Load("Fujin Res/Textures/edit") as Texture2D;
        straightenIcon = Resources.Load("Fujin Res/Textures/straighten") as Texture2D;
        initialized = true;
    }

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
