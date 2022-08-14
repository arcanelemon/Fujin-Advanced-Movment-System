
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FujinRope))]
public class FujinRopeEditor : Editor
{
    #region Variables

    const int LINE_STEPS = 10; 
    private const float HANDLE_SIZE = 0.1f;
    private const float PICK_SIZE = 0.15f;
    private int selectedIndex = -1;

    bool initialized;
    Texture2D mixIcon;
    Texture2D forwardIcon;
    Texture2D backIcon;
    Texture2D editIcon;
    Texture2D resetIcon;
    Texture2D straightenIcon;
    Texture2D hideIcon;

    FujinRope rope;

    #endregion Variables

    #region Engine

    void OnEnable()
    {
        rope = (FujinRope)target;
        rope.Init();
    }

    public override void OnInspectorGUI()
    {
        if (!initialized)
            Init();

        GUILayout.Space(10);
        rope.speed = EditorGUILayout.FloatField("Speed", rope.speed);
        rope.wobble = EditorGUILayout.FloatField("Wobble", rope.wobble);
        rope.quality = EditorGUILayout.IntField("Quality", rope.quality); 
        rope.mat = (Material)EditorGUILayout.ObjectField("Material", rope.mat, typeof(Material), true);

        EditorGUI.BeginChangeCheck();
        rope.type = (FujinRope.Type)EditorGUILayout.EnumPopup("Orientation", rope.type);
        if (EditorGUI.EndChangeCheck())
        {
            rope.FlipOrientation();
            SceneView.RepaintAll();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Direction");
        rope.toolbarSelection = GUILayout.Toolbar(rope.toolbarSelection,
            new GUIContent[] { new GUIContent(mixIcon), new GUIContent(backIcon), new GUIContent(forwardIcon) }, 
            GUILayout.Height(18), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth 
            - EditorGUIUtility.labelWidth - 25));
        
        switch (rope.toolbarSelection) 
        {
            case 0:
                rope.forward = true;
                rope.backward = true;
                break;
            case 1:
                rope.forward = false;
                rope.backward = true;
                break;
            case 2:
                rope.forward = true;
                rope.backward = false;
                break;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        rope.anchorFoldout = EditorGUILayout.Foldout(rope.anchorFoldout, "Anchors");
        GUILayout.EndHorizontal();
        if (rope.anchorFoldout) 
        {
            rope.anchors[0] = (Transform)EditorGUILayout.ObjectField("Anchor 0", rope.anchors[0], typeof(Transform), true);
            rope.anchors[1] = (Transform)EditorGUILayout.ObjectField("Anchor 1", rope.anchors[1], typeof(Transform), true);
        }
        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUI.BeginChangeCheck();
        rope.edit = GUILayout.Toggle(rope.edit, editIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
        rope.show = GUILayout.Toggle(rope.show, hideIcon, "Button", GUILayout.Width(50), GUILayout.MaxHeight(20));
        if (GUILayout.Button(straightenIcon, GUILayout.Width(50), GUILayout.MaxHeight(20)))
            rope.Straighten();
        if (GUILayout.Button(resetIcon, GUILayout.Width(50), GUILayout.MaxHeight(20)))
            rope.Revert();
        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public void OnSceneGUI()
    {
        if (!rope.show)
            return;

        // Draw Lines
        Handles.color = Color.yellow;
        Vector3 lineStart = rope.GetPoint(0f);
        for (int i = 1; i <= LINE_STEPS; i++)
        {
            Vector3 lineEnd = rope.GetPoint(i / (float)LINE_STEPS);
            Handles.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }

        // Draw Handles
        if (rope.edit)
        {
            Vector3[] points = rope.GetHandles();
            for (int i = 0; i < points.Length; i++)
            {
                // Draw Lines
                Handles.color =  new Color(255, 255, 255, 0.3f);
                if (i < points.Length - 1)
                    Handles.DrawLine(rope.GetHandle(i), rope.GetHandle(i + 1));

                // Draw Handles
                Handles.color = Color.white;
                Vector3 point = rope.transform.TransformPoint(points[i]);
                float size = HandleUtility.GetHandleSize(point);
                if (Handles.Button(point, rope.transform.rotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
                {
                    selectedIndex = i;
                }
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
                }
            }
        }
    }

    #endregion Engine

    #region Private Methods

    void Init() 
    {
        mixIcon = Resources.Load("Fujin Res/Textures/mix") as Texture2D;
        backIcon = Resources.Load("Fujin Res/Textures/left") as Texture2D;
        forwardIcon = Resources.Load("Fujin Res/Textures/right") as Texture2D;
        editIcon = Resources.Load("Fujin Res/Textures/edit") as Texture2D;
        straightenIcon = Resources.Load("Fujin Res/Textures/straighten") as Texture2D;
        resetIcon = Resources.Load("Fujin Res/Textures/reset") as Texture2D;
        hideIcon = Resources.Load("Fujin Res/Textures/hide") as Texture2D;
        initialized = true;
    }

    #endregion Private Methods

    #region Public Methods

    #endregion Public Methods
}
