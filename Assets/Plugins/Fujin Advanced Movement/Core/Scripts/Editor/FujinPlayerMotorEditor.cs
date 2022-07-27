using UnityEditor;
using UnityEngine;
using Fujin;
using Supyrb;

/// <summary>
/// The custom editor used to draw a <code>FujinPlayerMotor</code> in the inspector
/// </summary>
[CustomEditor(typeof(FujinPlayerMotor))]
public class FujinPlayerMotorEditor : Editor
{
    #region Variables

    int tab = 0;
    float margin = 10;
    float count = 0;
    float updateRate = 100;
    bool initialized;

    Vector2[] recordedVelocities = new Vector2[60];

    Material mat;
    FujinPlayerMotor motor;
    FujinInputModule inputModule;
    FujinPhysicsModule physicModule;
    FujinJumpModule jumpModule;
    FujinSprintModule sprintModule;
    FujinCrouchModule crouchModule;
    FujinSlideModule slideModule;
    FujinDashModule dashModule;
    FujinPoundModule poundModule;
    FujinMantleModule mantleModule;
    FujinClimbModule climbModule;
    FujinWallrunModule wallrunModule;
    FujinAutorunModule autorunModule;
    FujinSlowwalkModule slowwalkModule;
    FujinLeanModule leanModule;
    FujinFreelookModule freelookModule;
    FujinGrappleModule grappleModule;
    FujinEffectsModule effectsModule;

    SerializedProperty inputProperty;
    SerializedProperty physicsProperty;
    SerializedProperty cameraContainer;
    SerializedProperty movementProperty;
    SerializedProperty jumpProperty;
    SerializedProperty sprintProperty;
    SerializedProperty crouchProperty;
    SerializedProperty slideProperty;
    SerializedProperty dashProperty;
    SerializedProperty poundProperty;
    SerializedProperty mantleProperty;
    SerializedProperty climbProperty;
    SerializedProperty wallrunProperty;
    SerializedProperty autorunProperty;
    SerializedProperty slowwalkProperty;
    SerializedProperty leanProperty;
    SerializedProperty freelookProperty;
    SerializedProperty grappleProperty;
    SerializedProperty effectsProperty;

    GUIStyle foldout;
    Texture2D backgroundTexture;
    Texture2D dropdownNormalTexture;
    Texture2D dropdownActiveTexture;
    Texture2D greenIcon;
    Texture2D redIcon;

    #endregion Variables

    #region Engine

    /// <summary>
    /// [ENGINE]
    /// </summary>
    private void OnEnable()
    {
        inputProperty = serializedObject.FindProperty("input");
        physicsProperty = serializedObject.FindProperty("physics");
        cameraContainer = serializedObject.FindProperty("cameraContainer");
        movementProperty = serializedObject.FindProperty("movement");
        jumpProperty = serializedObject.FindProperty("jump");
        sprintProperty = serializedObject.FindProperty("sprint");
        crouchProperty = serializedObject.FindProperty("crouch");
        slideProperty = serializedObject.FindProperty("slide");
        dashProperty = serializedObject.FindProperty("dash");
        poundProperty = serializedObject.FindProperty("pound");
        mantleProperty = serializedObject.FindProperty("mantle");
        climbProperty = serializedObject.FindProperty("climb");
        wallrunProperty = serializedObject.FindProperty("wallrun");
        autorunProperty = serializedObject.FindProperty("autorun");
        slowwalkProperty = serializedObject.FindProperty("slowwalk");
        leanProperty = serializedObject.FindProperty("lean");
        freelookProperty = serializedObject.FindProperty("freelook");
        grappleProperty = serializedObject.FindProperty("grapple");
        effectsProperty = serializedObject.FindProperty("effects");

        inputModule = inputProperty.GetValue<FujinInputModule>();
        physicModule = physicsProperty.GetValue<FujinPhysicsModule>();
        jumpModule = jumpProperty.GetValue<FujinJumpModule>();
        sprintModule = sprintProperty.GetValue<FujinSprintModule>();
        crouchModule = crouchProperty.GetValue<FujinCrouchModule>();
        slideModule = slideProperty.GetValue<FujinSlideModule>();
        dashModule = dashProperty.GetValue<FujinDashModule>();
        poundModule = poundProperty.GetValue<FujinPoundModule>();
        mantleModule = mantleProperty.GetValue<FujinMantleModule>();
        climbModule = climbProperty.GetValue<FujinClimbModule>();
        wallrunModule = wallrunProperty.GetValue<FujinWallrunModule>();
        autorunModule = autorunProperty.GetValue<FujinAutorunModule>();
        slowwalkModule = slowwalkProperty.GetValue<FujinSlowwalkModule>();
        leanModule = leanProperty.GetValue<FujinLeanModule>();
        freelookModule = freelookProperty.GetValue<FujinFreelookModule>();
        grappleModule = grappleProperty.GetValue<FujinGrappleModule>();
        effectsModule = effectsProperty.GetValue<FujinEffectsModule>();

        var shader = Shader.Find("Hidden/Internal-Colored");
        mat = new Material(shader);
    }

    /// <summary>
    /// [ENGINE]
    /// </summary>
    private void OnDisable()
    {
        DestroyImmediate(mat);
    }

    /// <summary>
    /// [ENGINE]
    /// </summary>
    public override void OnInspectorGUI() 
    {
        if (!initialized)
            Init();

        GUILayout.Space(10);

        EditorGUILayout.Popup("Preset", 0, new string[] { "None", "Fast", "Classic", "Tactical" });

        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();

        if (FoldoutButton("Features", ref motor.featuresFoldout))
            DrawFeatures();

        if (FoldoutButton("Input", ref motor.inputFoldout)) 
            DrawInput();

        if (FoldoutButton("Settings", ref motor.configFoldout))
            DrawSettings();

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        if (FoldoutButton("Debug", ref motor.debugFoldout))
            DrawDebug();

        GUILayout.Space(10);
    }

    #endregion Engine

    #region Private Methods

    /// <summary>
    /// FoldoutButton
    /// Draws a custom foldout button with a given label and value. Controls the toggle state of the value.
    /// </summary>
    /// <param name="label">The label of the foldout</param>
    /// <param name="value">The value of the foldout visibility</param>
    /// <returns></returns>
    bool FoldoutButton(string label, ref bool value) 
    {
        GUILayout.Label("", GUILayout.Height(0));
        Rect rect = GUILayoutUtility.GetLastRect();

        GUI.DrawTexture(new Rect(rect.x, rect.y + 2.5f, rect.width, 24), backgroundTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUI.DrawTexture(new Rect(rect.x + rect.width - 17.5f, rect.y + 5, 17.5f, 17.5f), value ? dropdownActiveTexture : dropdownNormalTexture);
        GUILayout.BeginHorizontal(EditorStyles.helpBox); 
        if (GUILayout.Button(label, foldout))
            value = !value;
        GUILayout.EndHorizontal();

        return value;
    }

    /// <summary>
    /// Init
    /// Initializes textures and GUI dependent variables
    /// </summary>
    void Init()
    {
        motor = (FujinPlayerMotor)target;
        initialized = true;

        foldout = new GUIStyle("Button");
        foldout.alignment = TextAnchor.MiddleLeft;
        foldout.normal.background = FujinEditorUtils.MakeTex(1, 1, Color.clear);
        foldout.hover.background = FujinEditorUtils.MakeTex(1, 1, new Color(1, 1, 1, .1f));

        greenIcon = Resources.Load("Fujin Res/Textures/green") as Texture2D;
        redIcon = Resources.Load("Fujin Res/Textures/red") as Texture2D;
        dropdownNormalTexture = Resources.Load("Fujin Res/Textures/dropdown up") as Texture2D;
        dropdownActiveTexture = Resources.Load("Fujin Res/Textures/dropdown down") as Texture2D;
        backgroundTexture = FujinEditorUtils.MakeTex(1, 1, new Color32(80, 80, 80, 255));
    }

    /// <summary>
    /// DrawInput
    /// Draws a input properties from currently enabled modules
    /// </summary>
    void DrawInput() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Invert Axis");
        GUILayout.FlexibleSpace();
        inputModule.lookAxis.x = GUILayout.Toggle(inputModule.lookAxis.x < 0, "X", "Button", GUILayout.Width(23.5f), GUILayout.Height(18f))? -1 : 1;
        inputModule.lookAxis.y = GUILayout.Toggle(inputModule.lookAxis.y < 0, "Y", "Button", GUILayout.Width(23.5f), GUILayout.Height(18f)) ? -1 : 1;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Look Sensitivity");
        inputModule.lookSensitivity = EditorGUILayout.Slider(inputModule.lookSensitivity, 0.01f, 2);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("lookAction"));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("moveAction"));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (jumpModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("jumpAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (sprintModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("sprintAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (crouchModule.enable | slideModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("crouchSlideAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (dashModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("dashAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (poundModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("poundAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (autorunModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("autorunAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (leanModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("leanAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (freelookModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("freelookAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (slowwalkModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("slowwalkAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (grappleModule.enable)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(inputProperty.FindPropertyRelative("grappleAction"));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    /// <summary>
    /// DrawFeatures
    /// Draws a series of toggle buttons that enable/disable a respective <code>FujinMotorModule</code>
    /// </summary>
    void DrawFeatures()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);

        #region Foldout 1
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        bool none = !jumpModule.enable && !sprintModule.enable && !crouchModule.enable && !dashModule.enable && !slideModule.enable
            && !poundModule.enable && !mantleModule.enable && !climbModule.enable && !wallrunModule.enable && !autorunModule.enable
            && !slowwalkModule.enable && !leanModule.enable && !freelookModule.enable && !grappleModule.enable && !effectsModule.enable;
        if (none)
            GUI.contentColor = Color.red;
        if (GUILayout.Toggle(none, "None", "Button"))
        {
           jumpModule.enable = false;
           sprintModule.enable = false;
           crouchModule.enable = false;
           poundModule.enable = false;
           dashModule.enable = false;
           slideModule.enable = false;
           poundModule.enable = false;
           mantleModule.enable = false;
           climbModule.enable = false;
           wallrunModule.enable = false; 
           autorunModule.enable = false;
           slowwalkModule.enable = false;
           leanModule.enable = false;
           freelookModule.enable = false;
           grappleModule.enable = false;
           effectsModule.enable = false;
        }

        GUI.contentColor = Color.white;

        bool all = jumpModule.enable && sprintModule.enable && crouchModule.enable && dashModule.enable && slideModule.enable
            && poundModule.enable && mantleModule.enable && climbModule.enable && wallrunModule.enable && autorunModule.enable
            && slowwalkModule.enable && leanModule.enable && freelookModule.enable && grappleModule.enable && effectsModule.enable;
        if (all)
            GUI.contentColor = Color.green;
        if (GUILayout.Toggle(all, "All", "Button"))
        {
            jumpModule.enable = true;
            sprintModule.enable = true;
            crouchModule.enable = true;
            dashModule.enable = true;
            poundModule.enable = false;
            slideModule.enable = true;
            poundModule.enable = true;
            mantleModule.enable = true;
            climbModule.enable = true;
            wallrunModule.enable = true;
            autorunModule.enable = true;
            slowwalkModule.enable = true;
            leanModule.enable = true;
            freelookModule.enable = true;
            grappleModule.enable = true;
            effectsModule.enable = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (jumpModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        jumpModule.enable = GUILayout.Toggle(jumpModule.enable, "Jump", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (sprintModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        sprintModule.enable = GUILayout.Toggle(sprintModule.enable, "Sprint", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (crouchModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        crouchModule.enable = GUILayout.Toggle(crouchModule.enable, "Crouch", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (slideModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        slideModule.enable = GUILayout.Toggle(slideModule.enable, "Slide", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (dashModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        dashModule.enable = GUILayout.Toggle(dashModule.enable, "Dash", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (poundModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        poundModule.enable = GUILayout.Toggle(poundModule.enable, "Pound", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (mantleModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        mantleModule.enable = GUILayout.Toggle(mantleModule.enable, "Mantle", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (climbModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        climbModule.enable = GUILayout.Toggle(climbModule.enable, "Climb", "Button");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (wallrunModule.enable)
            GUI.contentColor = Color.green;
        else
            GUI.contentColor = Color.white;
        wallrunModule.enable = GUILayout.Toggle(wallrunModule.enable, "WallRun", "Button");
        GUILayout.EndHorizontal();

        GUI.contentColor = Color.white;

        GUILayout.EndVertical();
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        #endregion Foldout 1

        GUILayout.BeginHorizontal();
        GUILayout.Space(2*margin);
        motor.morefeaturesFoldout = EditorGUILayout.Foldout(motor.morefeaturesFoldout, "More", true);
        GUILayout.EndHorizontal();

        if (motor.morefeaturesFoldout)
        {
            #region Foldout 2
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            if (autorunModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            autorunModule.enable = GUILayout.Toggle(autorunModule.enable, "Auto Run", "Button");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (slowwalkModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            slowwalkModule.enable = GUILayout.Toggle(slowwalkModule.enable, "Slow Walk", "Button");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (leanModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            leanModule.enable = GUILayout.Toggle(leanModule.enable, "Leaning", "Button");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (freelookModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            freelookModule.enable = GUILayout.Toggle(freelookModule.enable, "Free Look", "Button");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (grappleModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            grappleModule.enable = GUILayout.Toggle(grappleModule.enable, "Grapple Hook", "Button");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (effectsModule.enable)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            effectsModule.enable = GUILayout.Toggle(effectsModule.enable, "Effects", "Button");
            GUILayout.EndHorizontal();

            GUI.contentColor = Color.white;
            GUILayout.EndVertical();
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
            #endregion Foldout 2
        }

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    /// <summary>
    /// DrawSettings
    /// Draws the 'core' configurable properties of the <code>FujinPlayerMotor</code>
    /// <remark>Note: The name of this function does not reflect its purpose. Consider changing.</remark>
    /// </summary>
    void DrawSettings()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);
        tab = GUILayout.Toolbar(tab, new string[] { "Config", "Physics", "Components" });

        if (tab == 0)
            DrawModules();
        else if (tab == 1)
            DrawPhysics();
        else if (tab == 2)
            DrawComponents();

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    /// <summary>
    /// DrawModules
    /// Draws enabled <code>FujinMotorModule</code>.
    /// </summary>
    void DrawModules()
    {
        EditorGUILayout.PropertyField(movementProperty);
        if (jumpModule.enable)
            EditorGUILayout.PropertyField(jumpProperty);
        if (sprintModule.enable)
            EditorGUILayout.PropertyField(sprintProperty);
        if (crouchModule.enable)
            EditorGUILayout.PropertyField(crouchProperty);
        if (slideModule.enable)
            EditorGUILayout.PropertyField(slideProperty);
        if (dashModule.enable)
            EditorGUILayout.PropertyField(dashProperty);
        if (poundModule.enable)
            EditorGUILayout.PropertyField(poundProperty);
        if (mantleModule.enable)
            EditorGUILayout.PropertyField(mantleProperty);
        if (climbModule.enable)
            EditorGUILayout.PropertyField(climbProperty);
        if (wallrunModule.enable)
            EditorGUILayout.PropertyField(wallrunProperty);
        if (slowwalkModule.enable)
            EditorGUILayout.PropertyField(slowwalkProperty);
        if (leanModule.enable)
            EditorGUILayout.PropertyField(leanProperty);
        if (freelookModule.enable)
            EditorGUILayout.PropertyField(freelookProperty);
        if (grappleModule.enable)
            EditorGUILayout.PropertyField(grappleProperty);
        if (effectsModule.enable)
            EditorGUILayout.PropertyField(effectsProperty);
    }

    /// <summary>
    /// DrawPhysics
    /// Draws the <code>FujinPhysicsModule</code>
    /// </summary>
    void DrawPhysics()
    {
        EditorGUILayout.PropertyField(physicsProperty);
    }

    /// <summary>
    /// DrawComponents
    /// Draw miscellaneous configurable fields and components related to <code>FujinPlayerMotor</code>
    /// </summary>
    void DrawComponents()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        motor.hideMouseOnAwake = EditorGUILayout.Toggle("Hide Mouse On Awake", motor.hideMouseOnAwake);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.Toggle("Control Height", true);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.PropertyField(cameraContainer);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Animations");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
    }

    /// <summary>
    /// DrawDebug
    /// Draws a debug section that monitors various data fields of <code>FujinPlayerMotor</code>
    /// </summary>
    void DrawDebug() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        motor.drawGizmos = GUILayout.Toggle(motor.drawGizmos, "Draw Gizmos");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        #region Velocity Graph

        // Reserve GUI space with a width from 10 to 10000, and a fixed height of 200, and 
        // cache it as a rectangle.
        Rect layoutRectangle = GUILayoutUtility.GetRect(100, 100, 100, 100);
        float width = layoutRectangle.width - margin;

        if (Event.current.type == EventType.Repaint)
        {

            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(layoutRectangle);
            GL.PushMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            GL.Clear(true, false, new Color(.33f, .33f, .33f));
            mat.SetPass(0);

            #region Draw Background
            GL.Begin(GL.QUADS);
            GL.Color(new Color(.33f, .33f, .33f));
            GL.Vertex3(margin, 0, 0);
            GL.Vertex3(width, 0, 0);
            GL.Vertex3(width, layoutRectangle.height, 0);
            GL.Vertex3(margin, layoutRectangle.height, 0);
            GL.End();
            #endregion Draw Background

            #region Draw Lines
            // Draw Lines
            GL.Begin(GL.LINES);

            float x = margin;
            float increment = (width - margin) / 60;

            for (int i = 0; i <= 60; i++)
            {
                Color lineColor = (i % 10 == 0) ?
                    new Color(0, 0, 0, 1) : new Color(0, 0, 0, .3f);

                GL.Color(lineColor);
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, layoutRectangle.height, 0);
                x += increment;
            }

            float y = 0;
            increment = 10;

            for (int i = 0; i < 10; i++)
            {
                Color lineColor = (i % 5 == 0) ?
                       new Color(0, 0, 0, 1) : new Color(0, 0, 0, .3f);
                GL.Color(lineColor);
                GL.Vertex3(margin, y, 0);
                GL.Vertex3(width, y, 0);
                y += increment;
            }

            GL.End();
            #endregion Draw Lines

            #region Draw Data
            // Draw data
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                float horiztontalMag = Mathf.Min(new Vector2(motor.Velocity.x, motor.Velocity.z).magnitude, 50);
                float verticalMag = Mathf.Clamp(motor.Velocity.y, -50, 50);
                Vector2 velocity = new Vector2(horiztontalMag, verticalMag);
                for (int i = 59; i >= 0; i--)
                {
                    // shift data
                    if (i == 0)
                        recordedVelocities[i] = velocity;
                    else
                        recordedVelocities[i] = recordedVelocities[i - 1];
                }
            }

            x = width;
            y = layoutRectangle.height / 2;
            increment = (width - margin) / 60;

            GL.Begin(GL.LINE_STRIP); 
            GL.Color(Color.magenta);
            for (int i = 0; i < 60; i++) 
            {
                GL.Vertex3(x, y - recordedVelocities[i].x, 0);
                GL.Vertex3(x - increment, y - recordedVelocities[i == 59 ? i : i + 1].x, 0);
                x -= increment;
            }
            GL.End();

            x = width;
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.green);
            for (int i = 0; i < 60; i++) 
            {
                GL.Vertex3(x, y - recordedVelocities[i].y, 0);
                GL.Vertex3(x - increment, y - recordedVelocities[i == 59 ? i : i + 1].y, 0);
                x -= increment;
            }
            GL.End();
            #endregion Draw Data

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
            GUI.EndClip();
        }

        #endregion Velocity Grpah

        GUI.enabled = false;

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Velocity");
        EditorGUILayout.Vector3Field("", motor.Velocity, GUILayout.MinWidth(0));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Position");
        EditorGUILayout.Vector3Field("", motor.Position, GUILayout.MinWidth(0));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin); 
        GUILayout.Label("", GUILayout.Height(0));
        Rect rect = GUILayoutUtility.GetLastRect();
        rect.y += 6;
        Texture2D currentIcon = motor.Grounded ? greenIcon : redIcon;
        GUI.DrawTexture(new Rect(rect.position, new Vector2(8, 8)), currentIcon,
            ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        rect.y -= 6;
        rect.x += 10;
        GUI.Label(new Rect(rect.position, new Vector2(rect.width, 18)), "Grounded");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(18);

        if (physicModule.wallStick > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            GUILayout.Toggle(motor.OnWall, "On Wall");
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.FloatField("Normal", motor.NormalAngle);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.EnumPopup("Movement State", motor.State);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.FloatField("Target Speed", motor.Speed);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (jumpModule.enable && jumpModule.enableDoubleJumps && !motor.Grounded) 
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.IntField("Jumps", motor.Jumps);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        if (dashModule.enable) 
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.IntField("Dashes", motor.Dashes);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
        GUILayout.EndVertical();
    }

    #endregion Private Methods
}