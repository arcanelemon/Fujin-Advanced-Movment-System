
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Fujin.Movement;
using Supyrb;

namespace Fujin.Editor
{

    /// <summary>
    /// Collection of custom property drawers used to draw motor modules.
    /// </summary>

    [CustomPropertyDrawer(typeof(InteractModule))]
    class InteractModuleDrawer : ModuleDrawer
    {
        InteractModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);

            initialized = true;
            module = property.GetValue<InteractModule>();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!initialized)
                Init(property);

            GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            GUILayout.Space(margin);
            module.foldout = GUILayout.Toggle(module.foldout, "Interact", "Label");
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
                    module.drawDistance = EditorGUILayout.Slider("Draw Distance", module.drawDistance, 0, 10);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.drawRadius = EditorGUILayout.Slider("Draw Radius", module.drawRadius, 0, 2);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.highlightMaterial = (Material)EditorGUILayout.ObjectField("Highlight Material", module.highlightMaterial, typeof(Material), false);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("interactLayer"));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("interactAction"), new GUIContent("Interact Action"));

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(3);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    module.enableHold = EditorGUILayout.Toggle(module.enableHold, GUILayout.Width(20));
                    module.holdFoldout = EditorGUILayout.Foldout(module.holdFoldout, "Hold", true, "Label");
                    GUILayout.EndHorizontal();
                    GUI.enabled = module.holdFoldout;

                    if (module.holdFoldout && module.holdFadeAmount < 1)
                        module.holdFadeAmount += fadeSpeed;
                    else if (!module.holdFoldout && module.holdFadeAmount > 0)
                        module.holdFadeAmount -= fadeSpeed;

                    module.holdFadeAmount = Mathf.Clamp(module.holdFadeAmount, 0, 1);

                    using (var subGroup = new EditorGUILayout.FadeGroupScope(module.holdFadeAmount))
                    {
                        if (subGroup.visible)
                        {
                            GUI.enabled = module.enableHold; 
                            module.defaultHoldTime = EditorGUILayout.FloatField("Default Hold Time", Mathf.Max(module.defaultHoldTime, 0));
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("holdAction"), new GUIContent("Hold Action"));
                            EditorGUILayout.HelpBox("Query object for hold time before executing interact action.", MessageType.Info);

                            GUILayout.Space(3);
                        }
                    }
                    GUI.enabled = true;
                    GUILayout.EndVertical();
                    GUILayout.Space(3);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    module.enablePickup = EditorGUILayout.Toggle(module.enablePickup, GUILayout.Width(20));
                    module.pickupFoldout = EditorGUILayout.Foldout(module.pickupFoldout, "Pickup", true, "Label");
                    GUILayout.EndHorizontal();
                    GUI.enabled = module.pickupFoldout;

                    if (module.pickupFoldout && module.pickupFadeAmount < 1)
                        module.pickupFadeAmount += fadeSpeed;
                    else if (!module.pickupFoldout && module.pickupFadeAmount > 0)
                        module.pickupFadeAmount -= fadeSpeed;

                    module.pickupFadeAmount = Mathf.Clamp(module.pickupFadeAmount, 0, 1);

                    using (var subGroup = new EditorGUILayout.FadeGroupScope(module.pickupFadeAmount))
                    {
                        if (subGroup.visible)
                        {
                            GUI.enabled = module.enablePickup;
                            module.pickupForce = EditorGUILayout.FloatField("Pickup Force", Mathf.Max(module.pickupForce, 0));
                            GUILayout.BeginHorizontal();
                            module.pickupCenterObjects = EditorGUILayout.Toggle("Center Object", module.pickupCenterObjects);
                            if (!module.pickupCenterObjects)
                                GUI.enabled = false;
                            module.pickupCenterDistance = EditorGUILayout.Slider("Offset", module.pickupCenterDistance, 0, module.drawDistance);
                            GUI.enabled = module.enablePickup;
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            module.pickupLimitSpeed = EditorGUILayout.Toggle("Weight-Based Turn Rate", module.pickupLimitSpeed);
                            if (!module.pickupLimitSpeed)
                                GUI.enabled = false;
                            module.maxPickupWeight = EditorGUILayout.FloatField("Max Weight", module.maxPickupWeight, GUILayout.MinWidth(0));
                            GUI.enabled = module.enablePickup;
                            GUILayout.EndHorizontal();

                            module.throwForce = EditorGUILayout.FloatField("Throw Force", module.throwForce);

                            GUILayout.BeginHorizontal();
                            module.enableThrowCharge = EditorGUILayout.Toggle("Charge Throw", module.enableThrowCharge);
                            if (!module.enableThrowCharge)
                                GUI.enabled = false;
                            module.throwChargeTime = EditorGUILayout.FloatField(Mathf.Max(module.throwChargeTime, 0), GUILayout.MinWidth(0));
                            GUI.enabled = module.enablePickup;
                            GUILayout.EndHorizontal();

                            if (module.enableThrowCharge) 
                            {
                                module.throwGracePeriod = EditorGUILayout.FloatField("Throw Grace Period", Mathf.Max(module.throwGracePeriod, 0), GUILayout.MinWidth(0));
                            }

                            EditorGUILayout.PropertyField(property.FindPropertyRelative("pickupLayer"));

                            GUILayout.Space(3);
                            GUI.enabled = true;
                        }
                    }

                    GUI.enabled = true;
                    GUILayout.EndVertical();
                    GUILayout.Space(3);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(MovementModule))]
    public class MovementModuleDrawer : ModuleDrawer
    {
        MovementModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);

            initialized = true;
            module = property.GetValue<MovementModule>();
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
                    module.speed = EditorGUILayout.FloatField("Move Speed", Mathf.Max(module.speed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.acceleration = EditorGUILayout.FloatField("Move Acceleration", Mathf.Max(module.acceleration, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.airSpeed = EditorGUILayout.FloatField("Air Speed", Mathf.Max(module.airSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.airAcceleration = EditorGUILayout.FloatField("Air Acceleration", Mathf.Max(module.airAcceleration, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(JumpModule))]
    public class JumpModuleDrawer : ModuleDrawer
    {
        Texture2D horizontalIcon;
        Texture2D upIcon;
        Texture2D downIcon;
        JumpModule module;

        public override void Init(SerializedProperty property)
        {
            horizontalIcon = Resources.Load("Fujin Res/Textures/horizontal") as Texture2D;
            upIcon = Resources.Load("Fujin Res/Textures/up") as Texture2D;
            downIcon = Resources.Load("Fujin Res/Textures/down") as Texture2D;
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<JumpModule>();
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
                    module.doubleJump.enable = EditorGUILayout.Toggle(module.doubleJump.enable, GUILayout.Width(20));
                    module.doubleJump.foldout = EditorGUILayout.Foldout(module.doubleJump.foldout, "Double Jump", true, "Label");
                    GUILayout.EndHorizontal();
                    GUI.enabled = module.doubleJump.enable;

                    if (module.doubleJump.foldout && module.doubleJump.fadeAmount < 1)
                        module.doubleJump.fadeAmount += fadeSpeed;
                    else if (!module.doubleJump.foldout && module.doubleJump.fadeAmount > 0)
                        module.doubleJump.fadeAmount -= fadeSpeed;

                    module.doubleJump.fadeAmount = Mathf.Clamp(module.doubleJump.fadeAmount, 0, 1);

                    using (var subGroup = new EditorGUILayout.FadeGroupScope(module.doubleJump.fadeAmount))
                    {
                        if (subGroup.visible)
                        {
                            module.doubleJump.amount = EditorGUILayout.IntField("Double Jump Amount", module.doubleJump.amount);
                            module.doubleJump.force = EditorGUILayout.FloatField("Double Jump Force", module.doubleJump.force);
                            module.doubleJump.redirect = EditorGUILayout.Slider("Double Jump Redirect", module.doubleJump.redirect, 0, 1);
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
                    module.wallJump.enable = EditorGUILayout.Toggle(module.wallJump.enable, GUILayout.Width(20));
                    module.wallJump.foldout = EditorGUILayout.Foldout(module.wallJump.foldout, "Wall Jump", true, "Label");
                    GUILayout.EndHorizontal();
                    GUI.enabled = module.wallJump.enable;

                    if (module.wallJump.foldout && module.wallJump.fadeAmount < 1)
                        module.wallJump.fadeAmount += fadeSpeed;
                    else if (!module.wallJump.foldout && module.wallJump.fadeAmount > 0)
                        module.wallJump.fadeAmount -= fadeSpeed;

                    module.wallJump.fadeAmount = Mathf.Clamp(module.wallJump.fadeAmount, 0, 1);

                    using (var subGroup = new EditorGUILayout.FadeGroupScope(module.wallJump.fadeAmount))
                    {
                        if (subGroup.visible)
                        {
                            module.wallJump.force = EditorGUILayout.FloatField("Wall Jump Force", module.wallJump.force);
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
                    module.hover.enable = EditorGUILayout.Toggle(module.hover.enable, GUILayout.Width(20));
                    module.hover.foldout = EditorGUILayout.Foldout(module.hover.foldout, "Hover", true, "Label");
                    GUILayout.EndHorizontal();
                    GUI.enabled = module.hover.enable;

                    if (module.hover.foldout && module.hover.fadeAmount < 1)
                        module.hover.fadeAmount += fadeSpeed;
                    else if (!module.hover.foldout && module.hover.fadeAmount > 0)
                        module.hover.fadeAmount -= fadeSpeed;

                    module.hover.fadeAmount = Mathf.Clamp(module.hover.fadeAmount, 0, 1);

                    using (var subGroup = new EditorGUILayout.FadeGroupScope(module.hover.fadeAmount))
                    {
                        if (subGroup.visible)
                        {
                            module.hover.fallForce = EditorGUILayout.FloatField("Fall Force",
                                Mathf.Max(module.hover.fallForce, 0));
                            GUILayout.BeginHorizontal();
                            module.hover.useDuration = EditorGUILayout.Toggle("Use Hover Duration",
                                module.hover.useDuration);
                            GUI.enabled = module.hover.useDuration;
                            module.hover.duration = EditorGUILayout.FloatField(Mathf.Max(module.hover.duration, 0));
                            GUI.enabled = true;
                            GUILayout.EndHorizontal();
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

    [CustomPropertyDrawer(typeof(SprintModule))]
    public class SprintModuleDrawer : ModuleDrawer
    {
        SprintModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<SprintModule>();
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
                    module.speedMultiplier = EditorGUILayout.FloatField("Sprint Speed Multiplier", Mathf.Max(1, module.speedMultiplier));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(CrouchModule))]
    public class CrouchModuleDrawer : ModuleDrawer
    {
        CrouchModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<CrouchModule>();
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

    [CustomPropertyDrawer(typeof(SlideModule))]
    public class SlideModuleDrawer : ModuleDrawer
    {
        SlideModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<SlideModule>();
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
                    module.multidirectional = EditorGUILayout.Toggle("Multi-Directional", module.multidirectional);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.boost = EditorGUILayout.FloatField("Slide Boost", Mathf.Max(module.boost, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.friction = EditorGUILayout.FloatField("Slide Friction", Mathf.Max(module.friction, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.frictionDelay = EditorGUILayout.FloatField("Slide Friction Delay", Mathf.Max(module.frictionDelay, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.slopeAcceleration = EditorGUILayout.FloatField("Slope Accelleration", Mathf.Max(module.slopeAcceleration, 0));
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

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.cooldown = EditorGUILayout.FloatField("Slide Cooldown", Mathf.Max(module.cooldown, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(DashModule))]
    public class DashModuleDrawer : ModuleDrawer
    {
        Texture2D omniIcon;
        Texture2D horizontal;
        Texture2D upIcon;
        Texture2D downIcon;

        DashModule module;

        public override void Init(SerializedProperty property)
        {
            omniIcon = Resources.Load("Fujin Res/Textures/omni") as Texture2D;
            horizontal = Resources.Load("Fujin Res/Textures/horizontal") as Texture2D;
            upIcon = Resources.Load("Fujin Res/Textures/up") as Texture2D;
            downIcon = Resources.Load("Fujin Res/Textures/down") as Texture2D;
            background = EditorUtils.MakeTex(1, 1, backgroundColor);

            initialized = true;
            module = property.GetValue<DashModule>();
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
                    GUILayout.Label("Dash Axis");
                    module.omnidirectional = GUILayout.Toggle(module.omnidirectional, omniIcon, "Button", GUILayout.Width(22f), GUILayout.Height(15));
                    GUI.enabled = !module.omnidirectional;
                    module.axis.y = GUILayout.Toggle(module.axis.y > 0, horizontal, "Button", GUILayout.Width(22f), GUILayout.Height(15)) ? 1 : 0;
                    module.axis.x = GUILayout.Toggle(module.axis.x > 0, upIcon, "Button", GUILayout.Width(22f), GUILayout.Height(15)) ? 1 : 0;
                    module.axis.z = GUILayout.Toggle(module.axis.z > 0, downIcon, "Button", GUILayout.Width(22f), GUILayout.Height(15)) ? 1 : 0;
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
                    module.duration = EditorGUILayout.FloatField("Dash Duration", Mathf.Max(module.duration, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.momentum = EditorGUILayout.Slider("Carry Momentum", module.momentum, 0, 1);
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

    [CustomPropertyDrawer(typeof(PoundModule))]
    public class PoundModuleDrawer : ModuleDrawer
    {
        PoundModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<PoundModule>();
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

    [CustomPropertyDrawer(typeof(MantleModule))]
    public class MantleModuleDrawer : ModuleDrawer
    {
        MantleModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<MantleModule>();
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
                    module.speed = EditorGUILayout.FloatField("Mantle Speed", Mathf.Max(0, module.speed));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.height = EditorGUILayout.Slider("Max Mantle Height", module.height, 0, 3);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(ClimbModule))]
    public class ClimbModuleDrawer : ModuleDrawer
    {
        ClimbModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<ClimbModule>();
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

    [CustomPropertyDrawer(typeof(WallrunModule))]
    public class WallrunModuleDrawer : ModuleDrawer
    {
        WallrunModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<WallrunModule>();
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
                    module.speed = EditorGUILayout.FloatField("Wall Run Speed", Mathf.Max(module.speed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.accelaration = EditorGUILayout.FloatField("Wall Run Acceleration", Mathf.Max(module.accelaration, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.decelleration = EditorGUILayout.FloatField("Wall Run Decceleration", Mathf.Max(module.decelleration, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.breakSpeed = EditorGUILayout.FloatField("Wall Run Break Speed", Mathf.Max(module.breakSpeed, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.pushForce = EditorGUILayout.FloatField("Wall Run Push Force", Mathf.Max(module.pushForce, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.boost = EditorGUILayout.FloatField("Wall Run Speed Boost", Mathf.Max(module.boost, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.drag = EditorGUILayout.Slider("Wall Run Drag", module.drag, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.minHeight = EditorGUILayout.FloatField("Wall Min Height", Mathf.Max(module.minHeight, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.useDuration = EditorGUILayout.Toggle("Wall Run Duration", module.useDuration);
                    GUI.enabled = module.useDuration;
                    module.duration = EditorGUILayout.FloatField(Mathf.Max(module.duration, 0));
                    GUI.enabled = true;
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(KickModule))]
    public class KickModuleDrawer : ModuleDrawer
    {
        KickModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<KickModule>();
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
            module.foldout = GUILayout.Toggle(module.foldout, "Kick", "Label");
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
                    module.force = EditorGUILayout.FloatField("Kick Force", Mathf.Max(module.force, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.time = EditorGUILayout.FloatField("Kick Time", Mathf.Max(module.time, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.resetTime = EditorGUILayout.FloatField("Reset Time", Mathf.Max(module.resetTime, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", module.speedMultiplier, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.lookResistance = EditorGUILayout.Slider("Look Resistance", module.lookResistance, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("kickLayer"));;
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("action"), new GUIContent("Kick Action"));

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(SlowwalkModule))]
    public class SlowwalkModuleDrawer : ModuleDrawer
    {
        SlowwalkModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<SlowwalkModule>();
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
                    module.speedMultiplier = EditorGUILayout.Slider("Slow Walk Multiplier", module.speedMultiplier, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(LeanModule))]
    public class LeanModuleDrawer : ModuleDrawer
    {
        LeanModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<LeanModule>();
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

    [CustomPropertyDrawer(typeof(FreelookModule))]
    public class FujinFreelookModuleDrawer : ModuleDrawer
    {
        FreelookModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);
            initialized = true;
            module = property.GetValue<FreelookModule>();
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

    [CustomPropertyDrawer(typeof(GrappleModule))]
    public class GrappleModuleDrawer : ModuleDrawer
    {
        GrappleModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);

            initialized = true;
            module = property.GetValue<GrappleModule>();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!initialized)
                Init(property);

            if (!module.enable)
                return;

            GUI.DrawTexture(new Rect(position.x, position.y + 5, position.width, 20), background, ScaleMode.StretchToFill, true, 0, Color.white, 0, 3);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();

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
                    module.cooldownTime = EditorGUILayout.FloatField("Cooldown Time", Mathf.Max(0, module.cooldownTime));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.wrap = EditorGUILayout.Toggle("Wrap Grapple", module.wrap);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.wrap)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(margin);
                        module.wrapCount = EditorGUILayout.IntField("Max Wraps", Mathf.Max(module.wrapCount, 1));
                        GUILayout.Space(margin);
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.maxAngle = EditorGUILayout.Slider("Max Angle", module.maxAngle, 0, 180);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.elasticity = EditorGUILayout.Slider("Elasticity", module.elasticity, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.controlFoldout = EditorGUILayout.Foldout(module.controlFoldout, "Control", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.controlFoldout)
                    {
                        module.viewControl = EditorGUILayout.Slider("View Control", module.viewControl, 0, 1);
                        module.inputControl = EditorGUILayout.Slider("Input Control", module.inputControl, 0, 1);
                        module.lookResistance = EditorGUILayout.Slider("Look Resistance", module.lookResistance, 0, 1);
                        GUILayout.Space(2.5f);
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.simFoldout = EditorGUILayout.Foldout(module.simFoldout, "Simulation", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.simFoldout)
                    {
                        module.simType = (GrappleModule.SimulationType)EditorGUILayout.EnumPopup(module.simType);
                        if (module.simType == GrappleModule.SimulationType.Wobble)
                        {
                            module.wobbleAmount = EditorGUILayout.FloatField("Wobble Amount", Mathf.Max(module.wobbleAmount, 0));
                            module.wobbleWaveCount = EditorGUILayout.IntField("Wobble Wave Count", Mathf.Max((int)module.wobbleWaveCount, 0));
                            module.wobbleWaveHeight = EditorGUILayout.FloatField("Wobble Wave Height", Mathf.Max(module.wobbleWaveHeight, 0));

                        }
                        else if (module.simType == GrappleModule.SimulationType.Spiral)
                        {
                            module.spiralAmount = EditorGUILayout.FloatField("Spiral Amount", Mathf.Max(module.spiralAmount, 0));
                            module.spiralOffset = EditorGUILayout.FloatField("Spiral Offset", Mathf.Max(module.spiralOffset, 0));
                            module.spiralRotationSpeed = EditorGUILayout.FloatField("Spiral Rotation Speed", Mathf.Max(module.spiralRotationSpeed, 0));
                            module.spiralDamper = EditorGUILayout.Slider("Spiral Damper", module.spiralDamper, 0, 1);

                        }

                        module.sides = EditorGUILayout.IntField("Sides", Mathf.Max(module.sides, 3));
                        module.thickness = EditorGUILayout.FloatField("Thickness", Mathf.Max(module.thickness, 0));
                        module.quality = (int)EditorGUILayout.Slider("Quality", module.quality, 1, 1028);
                        GUILayout.Space(2.5f);
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.componentFoldout = EditorGUILayout.Foldout(module.componentFoldout, "Components", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.componentFoldout)
                    {
                        module.pivot = (Transform)EditorGUILayout.ObjectField("Grapple Pivot", module.pivot, typeof(Transform), true);
                        module.impactEffect = (GameObject)EditorGUILayout.ObjectField("Grapple Impact Effect", module.impactEffect, typeof(GameObject), false);
                        module.breakEffect = (GameObject)EditorGUILayout.ObjectField("Grapple Break Effect", module.breakEffect, typeof(GameObject), false);
                        module.prefab = (GameObject)EditorGUILayout.ObjectField("Grapple Prefab", module.prefab, typeof(GameObject), false);
                        GUILayout.Space(2.5f);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(margin);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
        }
    }

    [CustomPropertyDrawer(typeof(EffectsModule))]
    public class EffectsModuleDrawer : ModuleDrawer
    {
        EffectsModule module;

        public override void Init(SerializedProperty property)
        {
            background = EditorUtils.MakeTex(1, 1, backgroundColor);

            initialized = true;
            module = property.GetValue<EffectsModule>();
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

                    DrawCameraFX();
                    DrawSway();
                    DrawFootsteps();
                    DrawSpeedlines();

                    GUILayout.Space(5);
                }
            }

            GUILayout.EndVertical();
        }

        void DrawCameraFX()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            module.camera.enable = EditorGUILayout.Toggle(module.camera.enable, GUILayout.Width(20));
            module.camera.foldout = EditorGUILayout.Foldout(module.camera.foldout, "Camera FX", true, "Label");
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
            GUI.enabled = module.camera.enable;

            if (module.camera.foldout && module.camera.fadeAmount < 1)
                module.camera.fadeAmount += fadeSpeed;
            else if (!module.camera.foldout && module.camera.fadeAmount > 0)
                module.camera.fadeAmount -= fadeSpeed;

            module.camera.fadeAmount = Mathf.Clamp(module.camera.fadeAmount, 0, 1);

            using (var group = new EditorGUILayout.FadeGroupScope(module.camera.fadeAmount))
            {
                if (group.visible)
                {
                    GUILayout.Space(margin);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.camera.tilt.foldout = EditorGUILayout.Foldout(module.camera.tilt.foldout, "Tilt & Noise", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.camera.tilt.foldout)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(2);
                        GUILayout.BeginHorizontal();
                        module.camera.tilt.tab = EditorGUILayout.Popup(module.camera.tilt.tab, new string[]{ "Move", "Jump", "Impact", "Crouch", "Slide", "Dash", "Mantle", "Wall Run", "Wall Stick", "Kick", "Grapple"});
                        GUILayout.EndHorizontal();

                        switch (module.camera.tilt.tab) 
                        {
                            case 0:
                                module.camera.tilt.movementTiltHorizontal = EditorGUILayout.Slider("Move Tilt Multiplier (Horizontal)", module.camera.tilt.movementTiltHorizontal, 0, 1);
                                module.camera.tilt.movementTiltVertical = EditorGUILayout.Slider("Move Tilt Multiplier (Vertical)", module.camera.tilt.movementTiltVertical, 0, 1);
                                break;
                            case 1:
                                module.camera.tilt.jumpTiltHorizontal = EditorGUILayout.Slider("Jump Tilt Multiplier (Horizontal)", module.camera.tilt.jumpTiltHorizontal, 0, 1);
                                module.camera.tilt.jumpTiltVertical = EditorGUILayout.Slider("Jump Tilt Multiplier (Vertical)", module.camera.tilt.jumpTiltVertical, 0, 1);
                                break;
                            case 2:
                                module.camera.tilt.impactTiltHorizontal = EditorGUILayout.Slider("Impact Tilt Multiplier (Horizontal)", module.camera.tilt.impactTiltHorizontal, 0, 1);
                                module.camera.tilt.impactTiltVertical = EditorGUILayout.Slider("Impact Tilt Multiplier (Vertical)", module.camera.tilt.impactTiltVertical, 0, 1);
                                break;
                            case 3:
                                module.camera.tilt.crouchTiltHorizontal = EditorGUILayout.Slider("Crouch Tilt Multiplier (Horizontal)", module.camera.tilt.crouchTiltHorizontal, 0, 1);
                                module.camera.tilt.crouchTiltVertical = EditorGUILayout.Slider("Crouch Tilt Multiplier (Vertical)", module.camera.tilt.crouchTiltVertical, 0, 1);
                                break;
                            case 4:
                                module.camera.tilt.slideTilt = EditorGUILayout.Slider("Slide Tilt Multiplier", module.camera.tilt.slideTilt, 0, 1);
                                module.camera.tilt.slideShakeAmount = EditorGUILayout.Slider("Slide Shake Multiplier", module.camera.tilt.slideShakeAmount, 0, 1);
                                break;
                            case 5:
                                module.camera.tilt.dashTilt = EditorGUILayout.Slider("Dash Tilt Multiplier", module.camera.tilt.dashTilt, 0, 1);
                                break;
                            case 6:
                                module.camera.tilt.mantleTiltHorizontal = EditorGUILayout.Slider("Mantle Tilt Multiplier (Horizontal)", module.camera.tilt.mantleTiltHorizontal, 0, 1);
                                module.camera.tilt.mantleTiltVertical = EditorGUILayout.Slider("Mantle Tilt Multiplier (Vertical)", module.camera.tilt.mantleTiltVertical, 0, 1);
                                break;
                            case 7:
                                module.camera.tilt.wallRunTiltHorizontal = EditorGUILayout.Slider("Wall Run Tilt Multiplier", module.camera.tilt.wallRunTiltHorizontal, 0, 1);
                                break;
                            case 8:
                                module.camera.tilt.wallStickTilt = EditorGUILayout.Slider("Wall Stick Tilt Multiplier", module.camera.tilt.wallStickTilt, 0, 1);
                                break;
                            case 9:
                                module.camera.tilt.kickTilt = EditorGUILayout.Slider("Kick Tilt Multiplier", module.camera.tilt.kickTilt, 0, 1);
                                module.camera.tilt.kickShakeAmount = EditorGUILayout.Slider("Kick Shake Multiplier", module.camera.tilt.kickShakeAmount, 0, 1);
                                module.camera.tilt.kickShakeSpeed = EditorGUILayout.FloatField("Kick Shake Speed", Mathf.Max(module.camera.tilt.kickShakeSpeed, 0));
                                break;
                            case 10:
                                module.camera.tilt.grappleTilt = EditorGUILayout.Slider("Grapple Tilt Multiplier", module.camera.tilt.grappleTilt, 0, 1);
                                break;
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Noise");

                        module.camera.tilt.enableNoise = EditorGUILayout.Toggle("Enable Noise", module.camera.tilt.enableNoise);

                        if (module.camera.tilt.enableNoise)
                        {
                            module.camera.tilt.noiseMultiplier = EditorGUILayout.Slider("Noise Multiplier", module.camera.tilt.noiseMultiplier, 0, 1);
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Headbob");

                        GUILayout.BeginHorizontal();
                        module.camera.tilt.enableHeadbob = EditorGUILayout.Toggle("Enable Headbob", module.camera.tilt.enableHeadbob);
                        GUILayout.EndHorizontal();

                        if (module.camera.tilt.enableHeadbob)
                        {
                            module.camera.tilt.bobFrequency = EditorGUILayout.FloatField("Frequency", Mathf.Max(module.camera.tilt.bobFrequency, 0));
                            module.camera.tilt.horizontalBob = EditorGUILayout.Slider("Horizontal Bob", module.camera.tilt.horizontalBob, 0, 1);
                            module.camera.tilt.verticalBob = EditorGUILayout.Slider("Vertical Bob", module.camera.tilt.verticalBob, 0, 1);
                        }
                        GUILayout.EndVertical();

                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.camera.dynamicFOV.foldout = EditorGUILayout.Foldout(module.camera.dynamicFOV.foldout, "Dynamic FOV", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    if (module.camera.dynamicFOV.foldout)
                    {
                        module.camera.dynamicFOV.enable = EditorGUILayout.Toggle("Enable Dynamic FOV", module.camera.dynamicFOV.enable);
                        if (module.camera.dynamicFOV.enable) 
                        {
                            module.camera.dynamicFOV.multiplier = EditorGUILayout.Slider("FOV Multiplier", module.camera.dynamicFOV.multiplier, 1, 2);
                            module.camera.dynamicFOV.speed = EditorGUILayout.FloatField("Max FOV Velocity", Mathf.Max(module.camera.dynamicFOV.speed, 0));
                        }

                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin); 
                    EditorGUILayout.Foldout(false, "Lens", true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(margin);
                }
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        void DrawFootsteps()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            module.footsteps.enable = EditorGUILayout.Toggle(module.footsteps.enable, GUILayout.Width(20));
            module.footsteps.foldout = EditorGUILayout.Foldout(module.footsteps.foldout, "Footstep SFX", true, "Label");
            GUILayout.Space(margin);
            GUILayout.EndHorizontal();
            GUI.enabled = module.footsteps.enable;

            if (module.footsteps.foldout && module.footsteps.fadeAmount < 1)
                module.footsteps.fadeAmount += fadeSpeed;
            else if (!module.footsteps.foldout && module.footsteps.fadeAmount > 0)
                module.footsteps.fadeAmount -= fadeSpeed;

            module.footsteps.fadeAmount = Mathf.Clamp(module.footsteps.fadeAmount, 0, 1);

            using (var group = new EditorGUILayout.FadeGroupScope(module.footsteps.fadeAmount))
            {
                if (group.visible)
                {
                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.footsteps.frequency = EditorGUILayout.FloatField("Frequency", Mathf.Max(module.footsteps.frequency, 0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.footsteps.volumeRange = EditorGUILayout.Vector2Field("Volume Range",
                        new Vector2(Mathf.Clamp(module.footsteps.volumeRange.x, 0, module.footsteps.volumeRange.y),
                        Mathf.Clamp(module.footsteps.volumeRange.y, module.footsteps.volumeRange.x, 1)));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.footsteps.pitchRange = EditorGUILayout.Vector2Field("Pitch Range",
                        new Vector2(Mathf.Clamp(module.footsteps.pitchRange.x, -3, module.footsteps.pitchRange.y),
                        Mathf.Clamp(module.footsteps.pitchRange.y, module.footsteps.pitchRange.x, 3)));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.footsteps.sound = (AudioClip)EditorGUILayout.ObjectField("Sound", module.footsteps.sound, typeof(AudioClip), false);
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
                    GUILayout.Space(margin);
                    module.sway.container = (Transform)EditorGUILayout.ObjectField("Sway Container", module.sway.container, typeof(Transform), true);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
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
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
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
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2 * margin);
                    module.sway.moveFoldout = EditorGUILayout.Foldout(module.sway.moveFoldout, "Movement Sway", true);
                    GUILayout.EndHorizontal();

                    if (module.sway.moveFoldout)
                    {
                        GUILayout.BeginHorizontal();
                        module.sway.moveSpeed = EditorGUILayout.FloatField("Transition Speed", Mathf.Max(module.sway.moveSpeed, 0));
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
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
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
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(2*margin);
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
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
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
                    GUILayout.Space(margin);
                    module.speedlines.amount = EditorGUILayout.FloatField("Speed Line Amount", Mathf.Max(module.speedlines.amount, 1));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedlines.thiness = EditorGUILayout.Slider("Speed Line Thiness", module.speedlines.thiness, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedlines.radius = EditorGUILayout.Slider("Speed Line Radius", module.speedlines.radius, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedlines.curve = EditorGUILayout.Slider("Multiplier Curve", module.speedlines.curve, 0, 1);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedlines.minVelocity = EditorGUILayout.FloatField("Min, Max Velocity", Mathf.Max(module.speedlines.minVelocity, 0));
                    module.speedlines.maxVelocity = EditorGUILayout.FloatField(Mathf.Max(module.speedlines.maxVelocity, 0), GUILayout.MinWidth(0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.speedlines.gradient = EditorGUILayout.GradientField("Color", module.speedlines.gradient);
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
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

    [CustomPropertyDrawer(typeof(PhysicsModule))]
    public class PhysicsModuleDrawer : ModuleDrawer
    {
        Material mat;
        SerializedProperty materialProperty;
        PhysicsModule module;

        public override void Init(SerializedProperty property)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            mat = new Material(shader);
            materialProperty = property.FindPropertyRelative("physicMaterial");

            initialized = true;
            module = property.GetValue<PhysicsModule>();
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
                    module.friction = EditorGUILayout.FloatField("Friction", Mathf.Max(0, module.friction));
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
                    module.stepHeight = EditorGUILayout.FloatField("Step Height", Mathf.Max(module.stepHeight,0));
                    GUILayout.Space(margin);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(margin);
                    module.stepDepth = EditorGUILayout.FloatField("Step Depth", Mathf.Max(module.stepDepth, 0));
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
                    float width = layoutRectangle.width - 2 * margin;

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

                        x = 2 * margin;
                        increment = (width - 2 * margin) / 90;
                        float diff = module.slopeAngle * module.slopeFallOff;
                        float fallOff = module.slopeAngle - diff;

                        GL.Color(new Color(0, 1, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min(fallOff, 89), 0, 0);
                        GL.Color(new Color(1, 1, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min(fallOff + diff / 2, 89), 0, 0);
                        GL.Vertex3(x + increment * Mathf.Min(fallOff + diff / 2, 89), layoutRectangle.height, 0);
                        GL.Color(new Color(0, 1, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min(fallOff, 89), layoutRectangle.height, 0);

                        GL.Color(new Color(1, 1, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min((fallOff + diff / 2), 89), 0, 0);
                        GL.Color(new Color(1, 0, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min(module.slopeAngle, 89), 0, 0);
                        GL.Vertex3(x + increment * Mathf.Min(module.slopeAngle, 89), layoutRectangle.height, 0);
                        GL.Color(new Color(1, 1, 0, 0.3f));
                        GL.Vertex3(x + increment * Mathf.Min((fallOff + diff / 2), 89), layoutRectangle.height, 0);

                        GL.Color(new Color(1, 0, 0, 0.4f));
                        GL.Vertex3(x + increment * Mathf.Min(module.slopeAngle, 89), 0, 0);
                        GL.Vertex3(width, 0, 0);
                        GL.Vertex3(width, layoutRectangle.height, 0);
                        GL.Vertex3(x + increment * Mathf.Min(module.slopeAngle, 89), layoutRectangle.height, 0);

                        GL.End();

                        // Draw data

                        // Pop the current matrix for rendering, and end the drawing clip.
                        GL.PopMatrix();
                        GUI.EndClip();
                    }

                    EditorGUI.LabelField(new Rect(layoutRectangle.x + margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "0");
                    EditorGUI.LabelField(new Rect(layoutRectangle.x + layoutRectangle.width / 2 - 2 * margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "45");
                    EditorGUI.LabelField(new Rect(layoutRectangle.x + layoutRectangle.width - 4 * margin, layoutRectangle.y + layoutRectangle.height, 20, 20), "90");

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
                    EditorGUILayout.Popup("Grapple", 0, new string[] { }); 
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

    public abstract class ModuleDrawer : PropertyDrawer
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
}
