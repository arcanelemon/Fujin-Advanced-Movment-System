
using UnityEngine;
using UnityEditor;
using Fujin;
using Supyrb;

/// <summary>
/// Collection of custom property drawers used to draw motor modules.
/// </summary>

[CustomPropertyDrawer(typeof(FujinMovementModule))]
public class FujinMovementModuleDrawer : FujinModuleDrawer
{
    FujinMovementModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);

        initialized = true;
        module = property.GetValue<FujinMovementModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Move", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speed = EditorGUILayout.FloatField("Move Speed", module.speed);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.useAcceleration = EditorGUILayout.Toggle("Move Acceleration", module.useAcceleration);
            GUI.enabled = module.useAcceleration;
            module.acceleration = EditorGUILayout.FloatField(module.acceleration);
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.useDecceleration = EditorGUILayout.Toggle("Move Decceleration", module.useDecceleration);
            GUI.enabled = module.useDecceleration;
            module.decceleration = EditorGUILayout.FloatField(module.decceleration);
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.useTilt = EditorGUILayout.Toggle("Movement Tilt", module.useTilt);
            GUI.enabled = module.useTilt;
            module.tiltAngle = EditorGUILayout.Slider(module.tiltAngle, 0, 30);
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.useTilt)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.tiltSpeed = EditorGUILayout.FloatField("Tilt Speed", Mathf.Max(0.01f, module.tiltSpeed));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinJumpModule))]
public class FujinJumpModuleDrawer : FujinModuleDrawer
{
    FujinJumpModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinJumpModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Jump", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.force = EditorGUILayout.FloatField("Jump Force", module.force);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.gracePeriod = EditorGUILayout.FloatField("Grace Period", module.gracePeriod);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal(); 
            GUILayout.Space(5);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.enableDoubleJumps = GUILayout.Toggle(module.enableDoubleJumps, "Lurch");
            GUI.enabled = module.enableDoubleJumps;
            module.doubleJumpCount = EditorGUILayout.IntField(module.doubleJumpCount);
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.enableLurch)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.lurchPeriod = EditorGUILayout.FloatField("Lurch Period", Mathf.Max(0, module.lurchPeriod));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.lurchStrength = EditorGUILayout.Slider("Lurch Strength", module.lurchStrength, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.lurchRedirect = EditorGUILayout.Slider("Lurch Redirect", module.lurchRedirect, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.enableDoubleJumps = GUILayout.Toggle(module.enableDoubleJumps, "Double Jumps");
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.enableDoubleJumps)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.doubleJumpCount = EditorGUILayout.IntField("Double Jump Amount", module.doubleJumpCount);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.doubleJumpforce = EditorGUILayout.FloatField("Double Jump Force", module.doubleJumpforce);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.enableWallJump = GUILayout.Toggle(module.enableWallJump, "Wall Jump");
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.enableWallJump)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.wallJumpforce = EditorGUILayout.FloatField("Wall Jump Force", module.wallJumpforce);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinSprintModule))]
public class FujinSprintModuleDrawer : FujinModuleDrawer
{
    FujinSprintModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinSprintModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Sprint", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speedMultiplier = EditorGUILayout.FloatField("Sprint Speed Multiplier", Mathf.Max(1, module.speedMultiplier));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinCrouchModule))]
public class FujinCrouchModuleDrawer : FujinModuleDrawer
{
    FujinCrouchModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinCrouchModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Crouch", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speedMultiplier = EditorGUILayout.Slider("Crouch Speed Multiplier", module.speedMultiplier, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.height = EditorGUILayout.Slider("Crouch Height", module.height, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinSlideModule))]
public class FujinSlideModuleDrawer : FujinModuleDrawer
{
    FujinSlideModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinSlideModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Slide", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.infinite = EditorGUILayout.Toggle("Infinite", module.infinite);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (!module.infinite)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.friction = EditorGUILayout.FloatField("Slide Friction", Mathf.Max(module.friction, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.force = EditorGUILayout.FloatField("Slide Force", Mathf.Max(module.force, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.control = EditorGUILayout.Slider("Slide Control", module.control, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.height = EditorGUILayout.Slider("Slide Height", module.height, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinDashModule))]
public class FujinDashModuleDrawer : FujinModuleDrawer
{
    Texture2D leftIcon;
    Texture2D rightIcon;
    Texture2D upIcon;
    Texture2D downIcon;

    FujinDashModule module;

    public override void Init(SerializedProperty property)
    {
        leftIcon = Resources.Load("Fujin Res/Textures/left") as Texture2D;
        rightIcon = Resources.Load("Fujin Res/Textures/right") as Texture2D;
        upIcon = Resources.Load("Fujin Res/Textures/up") as Texture2D;
        downIcon = Resources.Load("Fujin Res/Textures/down") as Texture2D;
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);

        initialized = true;
        module = property.GetValue<FujinDashModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Dash", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            GUILayout.Label("Dash Axis");
            module.axis.y = GUILayout.Toggle(module.axis.y > 0, leftIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
            module.axis.x = GUILayout.Toggle(module.axis.x > 0, rightIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
            module.axis.z = GUILayout.Toggle(module.axis.z > 0, upIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
            module.axis.w = GUILayout.Toggle(module.axis.w > 0, downIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.canDashInAir = EditorGUILayout.Toggle("Dash in Air", module.canDashInAir);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.force = EditorGUILayout.FloatField("Dash Force", Mathf.Max(module.force, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.duration = EditorGUILayout.FloatField("Dash Time", Mathf.Max(module.duration, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.count = EditorGUILayout.IntField("Num Dashes", (int)Mathf.Max(module.count, 1));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.cooldownTime = EditorGUILayout.FloatField("Dash Cooldown", Mathf.Max(module.cooldownTime, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinPoundModule))]
public class FujinPoundModuleDrawer : FujinModuleDrawer
{
    FujinPoundModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinPoundModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Pound", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.carryMomentum = EditorGUILayout.Toggle("Carry Momentum", module.carryMomentum);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.multiplyByHeight = EditorGUILayout.Toggle("Multiply By Height", module.multiplyByHeight);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.force = EditorGUILayout.FloatField("Pound Force", Mathf.Max(module.force, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.useDuration = EditorGUILayout.Toggle("Use Pound Time", module.useDuration);
            GUI.enabled = module.useDuration;
            module.duration = EditorGUILayout.FloatField(Mathf.Max(module.duration, 0));
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.bounce = EditorGUILayout.FloatField("Pound Bounce", Mathf.Max(module.bounce, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.knockback = EditorGUILayout.FloatField("Knockback", Mathf.Max(module.knockback, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.knockbackUpwardsModifier = EditorGUILayout.Slider("Knockback Upwards Multiplier", module.knockbackUpwardsModifier, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.knockbackRadius = EditorGUILayout.FloatField("Knockback Radius", Mathf.Max(module.knockbackRadius, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinMantleModule))]
public class FujinMantleModuleDrawer : FujinModuleDrawer
{
    FujinMantleModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinMantleModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Mantle", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.transitionFromGround = EditorGUILayout.Toggle("Transition From Ground", module.transitionFromGround);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.transitionFromGround)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                EditorGUILayout.Popup("Transition States", 0, new string[] { });
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speed = EditorGUILayout.FloatField("Mantle Speed", Mathf.Max(0, module.speed));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.height = EditorGUILayout.FloatField("Max Height", Mathf.Max(0, module.height));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinClimbModule))]
public class FujinClimbModuleDrawer : FujinModuleDrawer
{
    FujinClimbModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinClimbModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Climb", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.transitionFromGround = EditorGUILayout.Toggle("Transition From Ground", module.transitionFromGround);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.transitionFromGround)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                EditorGUILayout.Popup("Transition States", 0, new string[] { });
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speed = EditorGUILayout.FloatField("Climb Speed", Mathf.Max(module.speed, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.useDuration = EditorGUILayout.Toggle("Use Climb Duration", module.useDuration);
            GUI.enabled = module.useDuration;
            module.duration = EditorGUILayout.FloatField(module.duration);
            GUI.enabled = true;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinWallrunModule))]
public class FujinWallrunModuleDrawer : FujinModuleDrawer
{
    FujinWallrunModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinWallrunModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Wall Run", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.FloatField("Min Wall Run Speed", 0);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.Toggle("Wall Run Acceleration", true);
            EditorGUILayout.FloatField(0);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.Toggle("Wall Run Speed Boost", true);
            EditorGUILayout.FloatField(0);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinHeadbobModule))]
public class FujinHeadbobModuleDrawer : FujinModuleDrawer
{
    FujinHeadbobModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinHeadbobModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Headbob", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.verticalAmount = EditorGUILayout.Slider("Vertical Amount", module.verticalAmount, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.horizontalAmount = EditorGUILayout.Slider("Horizontal Amount", module.horizontalAmount, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", module.speedMultiplier, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinSlowwalkModule))]
public class FujinSlowwalkModuleDrawer : FujinModuleDrawer
{
    FujinSlowwalkModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinSlowwalkModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Slow Walk", true, "Label"); ;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speedMultiplier = EditorGUILayout.Slider("Slow Walk Multiplier", module.speedMultiplier, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinLeanModule))]
public class FujinLeanModuleDrawer : FujinModuleDrawer
{
    FujinLeanModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinLeanModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Leaning", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.angle = EditorGUILayout.Slider("Lean Angle", module.angle, 0, 30);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speed = EditorGUILayout.FloatField("Lean Speed", Mathf.Max(module.speed, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.offset = EditorGUILayout.Slider("Lean Offset", module.offset, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speedMultiplier = EditorGUILayout.Slider("Lean Speed Multiplier", module.speedMultiplier, 0.1f, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinFreelookModule))]
public class FujinFreelookModuleDrawer : FujinModuleDrawer
{
    FujinFreelookModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        initialized = true;
        module = property.GetValue<FujinFreelookModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Free Look", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.hold = EditorGUILayout.Toggle("Hold", module.hold);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            GUILayout.Label("Free Look Axis");
            GUILayout.FlexibleSpace();
            module.axis.x = GUILayout.Toggle(module.axis.x > 0, "X", "Button", GUILayout.Width(23.5f), GUILayout.Height(18f)) ? 1 : 0;
            module.axis.y = GUILayout.Toggle(module.axis.y > 0, "Y", "Button", GUILayout.Width(23.5f), GUILayout.Height(18f)) ? 1 : 0;
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.resetSpeed = EditorGUILayout.FloatField("Reset Speed", Mathf.Max(module.resetSpeed, 1));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            if (module.axis.x > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.horizontalAngle = EditorGUILayout.Slider("Max Horizontal Angle", module.horizontalAngle, 0, 90);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            if (module.axis.y > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.verticalAngle = EditorGUILayout.Slider("Max Vertical Angle", module.verticalAngle, 0, 90);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinGrappleModule))]
public class FujinGrappleModuleDrawer : FujinModuleDrawer
{
    SerializedProperty prefabProperty;

    FujinGrappleModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        prefabProperty = property.FindPropertyRelative("prefab");

        initialized = true;
        module = property.GetValue<FujinGrappleModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Grapple Hook", true, "Label"); ;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.range = EditorGUILayout.FloatField("Grapple Range", Mathf.Max(0, module.range));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.throwSpeed = EditorGUILayout.FloatField("Grapple Throw Speed", Mathf.Max(0, module.throwSpeed));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.speed = EditorGUILayout.FloatField("Grapple Speed", Mathf.Max(0, module.speed));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(prefabProperty);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinSpeedlinesModule))]
public class FujinSpeedlinesModuleDrawer : FujinModuleDrawer
{
    SerializedProperty prefabProperty;

    FujinSpeedlinesModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);
        prefabProperty = property.FindPropertyRelative("prefab");

        initialized = true;
        module = property.GetValue<FujinSpeedlinesModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        if (!module.enable)
            return;

        GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin + 5);
        module.foldout = EditorGUILayout.Foldout(module.foldout, "Speed Lines", true, "Label"); ;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.amount = EditorGUILayout.FloatField("Speed Line Amount", Mathf.Max(module.amount, 1));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.thiness = EditorGUILayout.Slider("Speed Line Thiness", module.thiness, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.radius = EditorGUILayout.Slider("Speed Line Radius", module.radius, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.curve = EditorGUILayout.Slider("Multiplier Curve", module.curve, 0, 1);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.minVelocity = EditorGUILayout.FloatField("Min, Max Velocity", Mathf.Max(module.minVelocity, 0));
            module.maxVelocity = EditorGUILayout.FloatField(Mathf.Max(module.maxVelocity, 0));
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.gradient = EditorGUILayout.GradientField("Color", module.gradient);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            EditorGUILayout.PropertyField(prefabProperty);
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinPhysicsModule))]
public class FujinPhysicsModuleDrawer : FujinModuleDrawer
{
    FujinPhysicsModule module;

    public override void Init(SerializedProperty property)
    {
        initialized = true;
        module = property.GetValue<FujinPhysicsModule>();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!initialized)
            Init(property);

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("General");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.gravityMultiplier = EditorGUILayout.FloatField("Gravity", Mathf.Max(0, module.gravityMultiplier));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.limitVelocity = EditorGUILayout.Toggle("Limit Max Velocity", module.limitVelocity);
        GUI.enabled = module.limitVelocity;
        module.maxVelocity = EditorGUILayout.Vector3Field("", new Vector3(Mathf.Max(module.maxVelocity.x, 0), Mathf.Max(module.maxVelocity.y, 0), Mathf.Max(module.maxVelocity.z, 0)), GUILayout.MinWidth(0));
        GUI.enabled = true;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Air");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.airDrag = EditorGUILayout.Slider("Air Drag", module.airDrag, 0, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.airTurnRate = EditorGUILayout.Slider("Air Turn Rate", module.airTurnRate, 0.01f, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.turnRateFromVelocity = EditorGUILayout.Toggle("Turn Rate from Velocity", module.turnRateFromVelocity);
        GUI.enabled = true;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Steps");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.stepHeight = EditorGUILayout.Slider("Step Height", module.stepHeight, 0.1f, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.stepDepth = EditorGUILayout.Slider("Step Depth", module.stepDepth, 0, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Slopes");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.slopeAngle = EditorGUILayout.Slider("Max Slope Angle", module.slopeAngle, 0, 90);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.slopeForce = EditorGUILayout.FloatField("Downward Slope Force", Mathf.Max(module.slopeForce, 0));
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Wall Stick");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.wallStick = EditorGUILayout.Slider("Wall Stick Amount", module.wallStick, 0, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.wallStickTilt = EditorGUILayout.Slider("Wall Stick Tilt", module.wallStickTilt, 0, 1);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.useWallStickDuration = EditorGUILayout.Toggle("Wall Stick Duration", module.useWallStickDuration);
        GUI.enabled = module.useWallStickDuration;
        module.wallStickDuration = EditorGUILayout.FloatField(Mathf.Max(module.wallStickDuration, 0));
        GUI.enabled = true;
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5); 
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        GUILayout.Label("Layers");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.Popup("Ground", 0, new string[] { });
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        EditorGUILayout.Popup("Walls", 0, new string[] { });
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.EndVertical();
    }
}

public abstract class FujinModuleDrawer : PropertyDrawer
{
    public float margin = 5;
    public Color backgroundColor = new Color32(90, 90, 90, 255);
    public Texture2D background;
    public bool initialized;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }

    public abstract void Init(SerializedProperty property);
    public abstract override void OnGUI(Rect position, SerializedProperty property, GUIContent label);
}
