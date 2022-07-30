
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Movement", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.speed = EditorGUILayout.FloatField("Move Speed", module.speed);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.acceleration = EditorGUILayout.FloatField("Move Acceleration", module.acceleration);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.airSpeed = EditorGUILayout.FloatField("Air Speed", module.airSpeed);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.airAcceleration = EditorGUILayout.FloatField("Air Acceleration", module.airAcceleration);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.tiltAngle = EditorGUILayout.Slider("Tilt Angle", module.tiltAngle, 0, 30);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.tiltSpeed = EditorGUILayout.FloatField("Tilt Speed", Mathf.Max(0.01f, module.tiltSpeed));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Jump", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                module.enableDoubleJumps = EditorGUILayout.Toggle(module.enableDoubleJumps, GUILayout.Width(20));
                module.doubleJumpFoldout = EditorGUILayout.Foldout(module.doubleJumpFoldout, "Double Jump", true, "Label");
                GUILayout.EndHorizontal();
                GUI.enabled = module.enableDoubleJumps;

                if (module.doubleJumpFoldout && module.doubleJumpFadeAmount < 1)
                    module.doubleJumpFadeAmount += fadeSpeed;
                else if (!module.doubleJumpFoldout && module.doubleJumpFadeAmount > 0)
                    module.doubleJumpFadeAmount -= fadeSpeed;

                module.doubleJumpFadeAmount = Mathf.Clamp(module.doubleJumpFadeAmount, 0, 1);

                using (var subGroup = new EditorGUILayout.FadeGroupScope(module.doubleJumpFadeAmount))
                {
                    if (subGroup.visible)
                    {
                        module.doubleJumpCount = EditorGUILayout.IntField("Double Jump Amount", module.doubleJumpCount);
                        module.doubleJumpforce = EditorGUILayout.FloatField("Double Jump Force", module.doubleJumpforce);
                        GUILayout.Space(5);
                    }
                }

                GUI.enabled = true;
                GUILayout.EndVertical(); 
                GUILayout.Space(3);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                module.enableDrift = EditorGUILayout.Toggle(module.enableDrift, GUILayout.Width(20));
                module.driftFoldout = EditorGUILayout.Foldout(module.driftFoldout, "Drift", true, "Label");
                GUILayout.EndHorizontal();
                GUI.enabled = module.enableDrift;

                if (module.driftFoldout && module.driftFadeAmount < 1)
                    module.driftFadeAmount += fadeSpeed;
                else if (!module.driftFoldout && module.driftFadeAmount > 0)
                    module.driftFadeAmount -= fadeSpeed;

                module.driftFadeAmount = Mathf.Clamp(module.driftFadeAmount, 0, 1);

                using (var subGroup = new EditorGUILayout.FadeGroupScope(module.driftFadeAmount))
                {
                    if (subGroup.visible)
                    {
                        module.driftPeriod = EditorGUILayout.FloatField("Drift Period", Mathf.Max(0, module.driftPeriod));
                        module.driftStrength = EditorGUILayout.FloatField("Drift Strength", Mathf.Max(module.driftStrength, 0));
                        module.driftRedirect = EditorGUILayout.Slider("Drift Redirect", module.driftRedirect, 0, 1);
                        GUILayout.Space(5);
                    }
                }

                GUI.enabled = true;
                GUILayout.EndVertical();
                GUILayout.Space(3);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                module.enableWallJump = EditorGUILayout.Toggle(module.enableWallJump, GUILayout.Width(20));
                module.wallFoldout = EditorGUILayout.Foldout(module.wallFoldout, "Wall Jump", true, "Label");
                GUILayout.EndHorizontal();
                GUI.enabled = module.enableWallJump;

                if (module.wallFoldout && module.wallFadeAmount < 1)
                    module.wallFadeAmount += fadeSpeed;
                else if (!module.wallFoldout && module.wallFadeAmount > 0)
                    module.wallFadeAmount -= fadeSpeed;

                module.wallFadeAmount = Mathf.Clamp(module.wallFadeAmount, 0, 1);

                using (var subGroup = new EditorGUILayout.FadeGroupScope(module.wallFadeAmount))
                {
                    if (subGroup.visible)
                    {
                        module.wallJumpforce = EditorGUILayout.FloatField("Wall Jump Force", module.wallJumpforce);
                        GUILayout.Space(5);
                    }
                }

                GUI.enabled = true;
                GUILayout.EndVertical();
                GUILayout.Space(3);
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Sprint", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Crouch", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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
                module.height = EditorGUILayout.Slider("Crouch Height", module.height, 0.5f, 1.75f);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Slide", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.hold = EditorGUILayout.Toggle("Hold", module.hold);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.multidirectional = EditorGUILayout.Toggle("Multi-Directional", module.multidirectional);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.infinite = EditorGUILayout.Toggle("Infinite", module.infinite);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                GUI.enabled = !module.infinite;
                module.friction = EditorGUILayout.FloatField("Slide Friction", Mathf.Max(module.friction, 0));
                GUI.enabled = true;
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.force = EditorGUILayout.FloatField("Slide Force", Mathf.Max(module.force, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.maxSpeed = EditorGUILayout.FloatField("Max Speed", Mathf.Max(module.maxSpeed, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.control = EditorGUILayout.Slider("Slide Control", module.control, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.height = EditorGUILayout.Slider("Slide Height", module.height, 0.5f, 1.75f);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.tiltAngle = EditorGUILayout.Slider("Tilt Angle", module.tiltAngle, 0, 30);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.tiltSpeed = EditorGUILayout.FloatField("Tilt Speed", Mathf.Max(module.tiltSpeed, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Dash", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.canDashInAir = EditorGUILayout.Toggle("Dash in Air", module.canDashInAir);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.omnidirectional = EditorGUILayout.Toggle("Omnidirectional", module.omnidirectional);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                GUILayout.Label("Dash Axis");
                GUI.enabled = !module.omnidirectional;
                module.axis.y = GUILayout.Toggle(module.axis.y > 0, leftIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
                module.axis.x = GUILayout.Toggle(module.axis.x > 0, rightIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
                module.axis.z = GUILayout.Toggle(module.axis.z > 0, upIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
                module.axis.w = GUILayout.Toggle(module.axis.w > 0, downIcon, "Button", GUILayout.Width(22f), GUILayout.Height(13)) ? 1 : 0;
                GUI.enabled = true;
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.force = EditorGUILayout.FloatField("Dash Force", Mathf.Max(module.force, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.momentum = EditorGUILayout.Slider("Carry Momentum", module.momentum, 0, 1);
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Pound", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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

                GUILayout.Space(5);

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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Mantle", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Climb", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Wall Run", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.minHeight = EditorGUILayout.FloatField("Min Height", Mathf.Max(module.minHeight, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.speed = EditorGUILayout.FloatField("Wall Run Speed", Mathf.Max(module.speed, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.useAccelaration = EditorGUILayout.Toggle("Wall Run Acceleration", module.useAccelaration);
                module.accelaration = EditorGUILayout.FloatField(Mathf.Max(module.accelaration, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.useBoost = EditorGUILayout.Toggle("Wall Run Speed Boost", module.useBoost);
                module.boost = EditorGUILayout.FloatField(Mathf.Max(module.boost, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.useDrag = EditorGUILayout.Toggle("Wall Run Drag", module.useDrag);
                module.drag = EditorGUILayout.Slider(module.drag, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.useDuration = EditorGUILayout.Toggle("Wall Run Duration", module.useDuration);
                module.duration = EditorGUILayout.FloatField(Mathf.Max(module.duration, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Slow Walk", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Lean", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Free Look", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);
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

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.lookResistance = EditorGUILayout.Slider("Look Resistance", module.lookResistance, 0, 1);
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
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinGrappleModule))]
public class FujinGrappleModuleDrawer : FujinModuleDrawer
{
    FujinGrappleModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);

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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Grapple Hook", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.range = EditorGUILayout.FloatField("Grapple Range", Mathf.Max(0, module.range));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.throwSpeed = EditorGUILayout.FloatField("Throw Speed", Mathf.Max(0, module.throwSpeed));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.speed = EditorGUILayout.FloatField("Grapple Speed", Mathf.Max(0, module.speed));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.acceleration = EditorGUILayout.FloatField("Grapple Acceleration", Mathf.Max(0, module.acceleration));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.cooldownTime = EditorGUILayout.FloatField("Grapple Cooldown", Mathf.Max(0, module.cooldownTime));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.resetMomentum = EditorGUILayout.Toggle("Reset Momentum", module.resetMomentum);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.maxAngle = EditorGUILayout.Slider("Max Angle", module.maxAngle, 0, 180);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.control = EditorGUILayout.Slider("Grapple Control", module.control, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.lookResistance = EditorGUILayout.Slider("Look Resistance", module.lookResistance, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.pivot = (Transform)EditorGUILayout.ObjectField("Grapple Pivot", module.pivot, typeof(Transform), true);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.prefab = (GameObject)EditorGUILayout.ObjectField("Grapple Prefab", module.prefab, typeof(GameObject), false);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }

        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinEffectsModule))]
public class FujinEffectsModuleDrawer : FujinModuleDrawer
{
    FujinEffectsModule module;

    public override void Init(SerializedProperty property)
    {
        background = FujinEditorUtils.MakeTex(1, 1, backgroundColor);

        initialized = true;
        module = property.GetValue<FujinEffectsModule>();
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
        GUILayout.Space(margin);
        module.foldout = GUILayout.Toggle(module.foldout, "Effects", "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.foldout && module.fadeAmount < 1)
            module.fadeAmount += fadeSpeed;
        else if (!module.foldout && module.fadeAmount > 0)
            module.fadeAmount -= fadeSpeed;

        module.fadeAmount = Mathf.Clamp(module.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                DrawHeadbob();
                DrawSway();
                DrawFOV();
                DrawSpeedlines();

                GUILayout.Space(5);
            }
        }

        GUILayout.EndVertical();
    }

    void DrawHeadbob() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        module.headbob.enable = EditorGUILayout.Toggle(module.headbob.enable, GUILayout.Width(20));
        module.headbob.foldout = EditorGUILayout.Foldout(module.headbob.foldout, "Headbob", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();
        GUI.enabled = module.headbob.enable;

        if (module.headbob.foldout && module.headbob.fadeAmount < 1)
            module.headbob.fadeAmount += fadeSpeed;
        else if (!module.headbob.foldout && module.headbob.fadeAmount > 0)
            module.headbob.fadeAmount -= fadeSpeed;

        module.headbob.fadeAmount = Mathf.Clamp(module.headbob.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.headbob.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.headbob.verticalAmount = EditorGUILayout.Slider("Vertical Amount", module.headbob.verticalAmount, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.headbob.horizontalAmount = EditorGUILayout.Slider("Horizontal Amount", module.headbob.horizontalAmount, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.headbob.speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", module.headbob.speedMultiplier, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }
        GUI.enabled = true;
        GUILayout.EndVertical();
    }

    void DrawSway() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        module.sway.enable = EditorGUILayout.Toggle(module.sway.enable, GUILayout.Width(20));
        module.sway.foldout = EditorGUILayout.Foldout(module.sway.foldout, "Sway", true, "Label");
        GUILayout.EndHorizontal();
        GUI.enabled = module.sway.enable;

        if (module.sway.foldout && module.sway.fadeAmount < 1)
            module.sway.fadeAmount += fadeSpeed;
        else if (!module.sway.foldout && module.sway.fadeAmount > 0)
            module.sway.fadeAmount -= fadeSpeed;

        module.sway.fadeAmount = Mathf.Clamp(module.sway.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.sway.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.sway.container = (Transform)EditorGUILayout.ObjectField("Sway Container", module.sway.container, typeof(Transform), true);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(2*margin);
                module.sway.idleFoldout = EditorGUILayout.Foldout(module.sway.idleFoldout, "Idle Sway", true);
                GUILayout.EndHorizontal();

                if (module.sway.idleFoldout)
                {
                    GUILayout.BeginHorizontal();
                    module.sway.idleSpeed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.idleSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.idleTilt = EditorGUILayout.Vector2Field("Tilt", 
                        new Vector2(Mathf.Clamp(module.sway.idleTilt.x, 0, 45), Mathf.Clamp(module.sway.idleTilt.y, 0, 45)), GUILayout.MinWidth(0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.idleOffset = EditorGUILayout.Vector2Field("Offset", 
                        new Vector2(Mathf.Max(module.sway.idleOffset.x, 0), Mathf.Max(module.sway.idleOffset.y, 0)), GUILayout.MinWidth(0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(2 * margin);
                module.sway.lookFoldout = EditorGUILayout.Foldout(module.sway.lookFoldout, "Look Sway", true);
                GUILayout.EndHorizontal();

                if (module.sway.lookFoldout)
                {
                    GUILayout.BeginHorizontal();
                    module.sway.lookSpeed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.lookSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.lookTilt = EditorGUILayout.Vector2Field("Tilt",
                        new Vector2(Mathf.Clamp(module.sway.lookTilt.x, 0, 45), 
                        Mathf.Clamp(module.sway.lookTilt.y, 0, 45)), GUILayout.MinWidth(0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.lookOffset = EditorGUILayout.Vector2Field("Offset", 
                        new Vector2(Mathf.Max(module.sway.lookOffset.x, 0), Mathf.Max(module.sway.lookOffset.y, 0)), GUILayout.MinWidth(0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(2 * margin);
                module.sway.moveFoldout = EditorGUILayout.Foldout(module.sway.moveFoldout, "Movement Sway", true);
                GUILayout.EndHorizontal();

                if (module.sway.moveFoldout)
                {
                    GUILayout.BeginHorizontal();
                    module.sway.moveSpeed =  EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.moveSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.moveTilt = EditorGUILayout.Slider("Tilt", module.sway.moveTilt, 0, 45);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.moveOffset = EditorGUILayout.FloatField("Offset", Mathf.Max(module.sway.moveOffset, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(2 * margin);
                module.sway.fallFoldout = EditorGUILayout.Foldout(module.sway.fallFoldout, "Fall Sway", true);
                GUILayout.EndHorizontal();

                if (module.sway.fallFoldout)
                {
                    GUILayout.BeginHorizontal();
                    module.sway.fallSpeed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.fallSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.fallTilt = EditorGUILayout.Slider("Tilt", module.sway.fallTilt, 0, 45);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.fallOffset = EditorGUILayout.FloatField("Offset", Mathf.Max(module.sway.fallOffset, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(2 * margin);
                module.sway.impactFoldout = EditorGUILayout.Foldout(module.sway.impactFoldout, "Impact Sway", true);
                GUILayout.EndHorizontal();

                if (module.sway.impactFoldout)
                {
                    GUILayout.BeginHorizontal();
                    module.sway.impactSpeed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.impactSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.impactTilt = EditorGUILayout.Slider("Tilt", 0, 0, 45);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    module.sway.impactOffset = EditorGUILayout.FloatField("Offset", Mathf.Max(module.sway.impactOffset, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(5);
        }
        GUI.enabled = true;
        GUILayout.EndVertical();
    }

    void DrawFOV() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        module.fov.enable = EditorGUILayout.Toggle(module.fov.enable, GUILayout.Width(20));
        module.fov.foldout = EditorGUILayout.Foldout(module.fov.foldout, "FOV", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();
        GUI.enabled = module.fov.enable;

        if (module.fov.foldout && module.fov.fadeAmount < 1)
            module.fov.fadeAmount += fadeSpeed;
        else if (!module.fov.foldout && module.fov.fadeAmount > 0)
            module.fov.fadeAmount -= fadeSpeed;

        module.fov.fadeAmount = Mathf.Clamp(module.fov.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.fov.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.fov.multiplier = EditorGUILayout.Slider("FOV Multiplier", module.fov.multiplier, 1, 2);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.fov.speed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.fov.speed, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.fov.minVelocity = EditorGUILayout.FloatField("Min, Max Velocity", Mathf.Max(module.fov.minVelocity, 0));
                module.fov.maxVelocity = EditorGUILayout.FloatField(Mathf.Max(module.fov.maxVelocity, 0), GUILayout.MinWidth(0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }
        GUI.enabled = true;
        GUILayout.EndVertical();
    }

    void DrawSpeedlines() 
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        module.speedlines.enable = EditorGUILayout.Toggle(module.speedlines.enable, GUILayout.Width(20));
        module.speedlines.foldout = EditorGUILayout.Foldout(module.speedlines.foldout, "Speedlines", true, "Label");
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();
        GUI.enabled = module.speedlines.enable;

        if (module.speedlines.foldout && module.speedlines.fadeAmount < 1)
            module.speedlines.fadeAmount += fadeSpeed;
        else if (!module.speedlines.foldout && module.speedlines.fadeAmount > 0)
            module.speedlines.fadeAmount -= fadeSpeed;

        module.speedlines.fadeAmount = Mathf.Clamp(module.speedlines.fadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.speedlines.fadeAmount))
        {
            if (group.visible)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.speedlines.amount = EditorGUILayout.FloatField("Speed Line Amount", Mathf.Max(module.speedlines.amount, 1));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.speedlines.thiness = EditorGUILayout.Slider("Speed Line Thiness", module.speedlines.thiness, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.speedlines.radius = EditorGUILayout.Slider("Speed Line Radius", module.speedlines.radius, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.speedlines.curve = EditorGUILayout.Slider("Multiplier Curve", module.speedlines.curve, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.speedlines.minVelocity = EditorGUILayout.FloatField("Min, Max Velocity", Mathf.Max(module.speedlines.minVelocity, 0));
                module.speedlines.maxVelocity = EditorGUILayout.FloatField(Mathf.Max(module.speedlines.maxVelocity, 0), GUILayout.MinWidth(0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                module.speedlines.gradient = EditorGUILayout.GradientField("Color", module.speedlines.gradient);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                module.speedlines.prefab = (GameObject)EditorGUILayout.ObjectField("Speedlines Prefab", module.speedlines.prefab, typeof(GameObject), false);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }

        GUI.enabled = true;
        GUILayout.EndVertical();
    }
}

[CustomPropertyDrawer(typeof(FujinPhysicsModule))]
public class FujinPhysicsModuleDrawer : FujinModuleDrawer
{
    Material mat;
    SerializedProperty materialProperty;
    FujinPhysicsModule module;

    public override void Init(SerializedProperty property)
    {
        var shader = Shader.Find("Hidden/Internal-Colored");
        mat = new Material(shader);
        materialProperty = property.FindPropertyRelative("physicMaterial");

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
        module.generalFoldout = GUILayout.Toggle(module.generalFoldout, "General", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.generalFoldout && module.generalFadeAmount < 1)
            module.generalFadeAmount += fadeSpeed;
        else if (!module.generalFoldout && module.generalFadeAmount > 0)
            module.generalFadeAmount -= fadeSpeed;

        module.generalFadeAmount = Mathf.Clamp(module.generalFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.generalFadeAmount))
        {
            if (group.visible)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.gravityMultiplier = EditorGUILayout.FloatField("Gravity", Mathf.Max(0, module.gravityMultiplier));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.limitVelocity = EditorGUILayout.Toggle("Limit Max Velocity", module.limitVelocity);
                GUI.enabled = module.limitVelocity;
                module.maxVelocity = EditorGUILayout.Vector2Field("", new Vector2(Mathf.Max(module.maxVelocity.x, 0), Mathf.Max(module.maxVelocity.y, 0)), GUILayout.MinWidth(0));
                GUI.enabled = true;
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                EditorGUILayout.PropertyField(materialProperty);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.airFoldout = GUILayout.Toggle(module.airFoldout, "Air", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.airFoldout && module.airFadeAmount < 1)
            module.airFadeAmount += fadeSpeed;
        else if (!module.airFoldout && module.airFadeAmount > 0)
            module.airFadeAmount -= fadeSpeed;

        module.airFadeAmount = Mathf.Clamp(module.airFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.airFadeAmount))
        {
            if (group.visible)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.airDrag = EditorGUILayout.Slider("Air Drag", module.airDrag, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUI.enabled = !module.turnRateFromVelocity;
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.airTurnRate = EditorGUILayout.Slider("Air Turn Rate", module.airTurnRate, 0.01f, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.turnRateFromVelocity = EditorGUILayout.Toggle("Turn Rate from Velocity", module.turnRateFromVelocity);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.stepFoldout = GUILayout.Toggle(module.stepFoldout, "Steps", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.stepFoldout && module.stepFadeAmount < 1)
            module.stepFadeAmount += fadeSpeed;
        else if (!module.stepFoldout && module.stepFadeAmount > 0)
            module.stepFadeAmount -= fadeSpeed;

        module.stepFadeAmount = Mathf.Clamp(module.stepFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.stepFadeAmount))
        {
            if (group.visible)
            {
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
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.slopeFoldout = GUILayout.Toggle(module.slopeFoldout, "Slopes", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.slopeFoldout && module.slopeFadeAmount < 1)
            module.slopeFadeAmount += fadeSpeed;
        else if (!module.slopeFoldout && module.slopeFadeAmount > 0)
            module.slopeFadeAmount -= fadeSpeed;

        module.slopeFadeAmount = Mathf.Clamp(module.slopeFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.slopeFadeAmount))
        {
            if (group.visible)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.slopeForce = EditorGUILayout.FloatField("Slope Force", Mathf.Max(module.slopeForce, 0));
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.slopeAngle = EditorGUILayout.Slider("Max Slope Angle", module.slopeAngle, 0, 90);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.slopeFallOff = EditorGUILayout.Slider("Slope Fall Off", module.slopeFallOff, 0, 1);
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                #region Falloff Graph

                // Reserve GUI space with a width from 10 to 10000, and a fixed height of 200, and 
                // cache it as a rectangle.
                Rect layoutRectangle = GUILayoutUtility.GetRect(100, 100, 15, 15);
                float width = layoutRectangle.width - 2*margin;

                if (Event.current.type == EventType.Repaint)
                {

                    // If we are currently in the Repaint event, begin to draw a clip of the size of 
                    // previously reserved rectangle, and push the current matrix for drawing.
                    GUI.BeginClip(layoutRectangle);
                    GL.PushMatrix();

                    // Clear the current render buffer, setting a new background colour, and set our
                    // material for rendering.
                    GL.Clear(true, false, new Color(1, 1, 1, 0.1f));
                    mat.SetPass(0);

                    // Draw background
                    GL.Begin(GL.QUADS);
                    GL.Color(new Color(1, 1, 1, 0.1f));
                    GL.Vertex3(2 * margin, 0, 0);
                    GL.Vertex3(width, 0, 0);
                    GL.Vertex3(width, layoutRectangle.height, 0);
                    GL.Vertex3(2 * margin, layoutRectangle.height, 0);
                    GL.End();

                    // Draw Lines
                    GL.Begin(GL.LINES);

                    float x = 2 * margin;
                    float increment = (width - 2 * margin) / 18;

                    for (int i = 0; i <= 90; i++)
                    {
                        if (i % 5 != 0)
                            continue;

                        Color lineColor = (i % 15 == 0) ?
                            new Color(1, 1, 1, .3f) : new Color(1, 1, 1, .1f);

                        GL.Color(lineColor);
                        GL.Vertex3(x, 0, 0);
                        GL.Vertex3(x, layoutRectangle.height, 0);
                        x += increment;
                    }

                    GL.End();

                    // Draw Gradient
                    GL.Begin(GL.QUADS);
                    
                    increment = width / 90;
                    float diff = module.slopeAngle * module.slopeFallOff;
                    float fallOff = module.slopeAngle - diff;

                    GL.Color(new Color(0, 1, 0, 0.3f));
                    GL.Vertex3(increment * fallOff, 0, 0);
                    GL.Color(new Color(1, 1, 0, 0.3f));
                    GL.Vertex3(increment * (fallOff + diff / 2), 0, 0);
                    GL.Vertex3(increment * (fallOff + diff / 2), layoutRectangle.height, 0);
                    GL.Color(new Color(0, 1, 0, 0.3f));
                    GL.Vertex3(increment * fallOff, layoutRectangle.height, 0);

                    GL.Color(new Color(1, 1, 0, 0.3f));
                    GL.Vertex3(increment * (fallOff + diff / 2), 0, 0);
                    GL.Color(new Color(1, 0, 0, 0.3f));
                    GL.Vertex3(increment * module.slopeAngle, 0, 0);
                    GL.Vertex3(increment * module.slopeAngle, layoutRectangle.height, 0);
                    GL.Color(new Color(1, 1, 0, 0.3f));
                    GL.Vertex3(increment * (fallOff + diff / 2), layoutRectangle.height, 0);

                    GL.Color(new Color(1, 0, 0, 0.3f));
                    GL.Vertex3(increment * Mathf.Min(module.slopeAngle, 89), 0, 0);
                    GL.Vertex3(width, 0, 0);
                    GL.Vertex3(width, layoutRectangle.height, 0);
                    GL.Vertex3(increment * Mathf.Min(module.slopeAngle, 89), layoutRectangle.height, 0);

                    GL.End();

                    // Draw data

                    // Pop the current matrix for rendering, and end the drawing clip.
                    GL.PopMatrix();
                    GUI.EndClip();
                }

                EditorGUI.LabelField( new Rect(layoutRectangle.x + margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "0");
                EditorGUI.LabelField( new Rect(layoutRectangle.x + layoutRectangle.width/2 - 2*margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "45");
                EditorGUI.LabelField( new Rect(layoutRectangle.x + layoutRectangle.width - 4*margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "90");

                #endregion Falloff Graph

                GUILayout.Space(20);
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.wallFoldout = GUILayout.Toggle(module.wallFoldout, "Wall Stick", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.wallFoldout && module.wallFadeAmount < 1)
            module.wallFadeAmount += fadeSpeed;
        else if (!module.wallFoldout && module.wallFadeAmount > 0)
            module.wallFadeAmount -= fadeSpeed;

        module.wallFadeAmount = Mathf.Clamp(module.wallFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.wallFadeAmount))
        {
            if (group.visible)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                module.wallStick = EditorGUILayout.Slider("Wall Stick Amount", module.wallStick, 0, 1);
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
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Space(margin);
        module.layerFoldout = GUILayout.Toggle(module.layerFoldout, "Layers", EditorStyles.boldLabel);
        GUILayout.Space(margin);
        GUILayout.EndHorizontal();

        if (module.layerFoldout && module.layersFadeAmount < 1)
            module.layersFadeAmount += fadeSpeed;
        else if (!module.layerFoldout && module.layersFadeAmount > 0)
            module.layersFadeAmount -= fadeSpeed;

        module.layersFadeAmount = Mathf.Clamp(module.layersFadeAmount, 0, 1);

        using (var group = new EditorGUILayout.FadeGroupScope(module.layersFadeAmount))
        {
            if (group.visible)
            {

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

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                EditorGUILayout.Popup("Interact", 0, new string[] { });
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(margin);
                EditorGUILayout.Popup("Exclude", 0, new string[] { });
                GUILayout.Space(margin);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        GUILayout.EndVertical();
    }
}

public abstract class FujinModuleDrawer : PropertyDrawer
{
    public float margin = 5;
    public float fadeSpeed = 0.02f;
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
