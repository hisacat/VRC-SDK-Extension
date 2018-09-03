using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using VRCSDKExtension.Animation;

namespace VRCSDKExtension
{
    public class VRCAnimatorOverrideControllerEditorWindow : EditorWindow
    {
        [MenuItem("Assets/VRC AnimatorOverrideController Edit", true)]
        public static bool OpenWithAnimatorOverrideControllerMenuValidation()
        {
            var controllers = VRCAnimation.GetControllerFromSelection();
            foreach (var controller in controllers)
            {
                if (!VRCAnimation.CheckVRCAnimatiorOverrideController(controller))
                    return false;
            }
            return (controllers.Count > 0 && controllers.Count == Selection.assetGUIDs.Length);
        }
        [MenuItem("Assets/VRC AnimatorOverrideController Edit")]
        private static void OpenWithAnimatorOverrideControllerMenu()
        {
            var controllers = VRCAnimation.GetControllerFromSelection();
            Init(controllers.ToArray());
        }
        [MenuItem("Assets/Create/VRC Animator Override Controller", false)]
        private static void CreateVRCAnimatorOverrideController()
        {
            var runtimeAnimatorController = VRCAnimation.GetVRCRuntimeAnimatorController();
            if(runtimeAnimatorController == null)
            {
                Debug.LogError("Cannot found AvatarControllerTemplate Asset!");
                return;
            }

            var path = SelectionExtension.GetSelectedProjectPath();
            var controller = new AnimatorOverrideController();
            controller.runtimeAnimatorController = runtimeAnimatorController;

            path += @"\VRC Animator Override Controller.overrideController";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(controller, path);
        }

        private GUIStyle viveControllerMappingImage;

        private VRCAnimationType selectedTab;
        private bool controllersFoldOut = true;
        private Vector2 animationsScroll;

        public static void Init(AnimatorOverrideController[] controllers)
        {
            if (controllers == null || controllers.Length <= 0)
            {
                Debug.LogError("there are any VRCAnimatorOverrideControllers!");
                return;
            }
            int count = controllers.Length;
            for (int i = 0; i < count; i++)
            {
                var controller = controllers[i];
                if (!VRCAnimation.CheckVRCAnimatiorOverrideController(controller))
                {
                    Debug.LogError(controller.name + " is not a VRCAnimatorOverrideController!");
                    return;
                }
            }

            VRCAnimatorOverrideControllerEditorWindow window = (VRCAnimatorOverrideControllerEditorWindow)EditorWindow.GetWindow(typeof(VRCAnimatorOverrideControllerEditorWindow), true);
            window.selectedTab = VRCAnimationType.Hand;
            window.controllersFoldOut = false;
            window.animationsScroll = Vector2.zero;

            window.controllers = controllers;
            window.controllersCount = controllers.Length;
            window.editedOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>[window.controllersCount];

            for (int i = 0; i < window.controllersCount; i++)
            {
                var controller = window.controllers[i];
                List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                controller.GetOverrides(overrides);
                window.editedOverrides[i] = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrides);
            }
            {
                List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                controllers[0].GetOverrides(overrides);

                window.overrideKeys = new AnimationClip[VRCAnimation.VRCAnimationCount];
                for (int i = 0; i < VRCAnimation.VRCAnimationCount; i++)
                    window.overrideKeys[i] = overrides[i].Key;
            }
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);

            var minSize = this.minSize;
            minSize.x = 470;
            minSize.y = 500;
            this.minSize = minSize;

            var pos = this.position;
            pos.width = 470;
            this.position = pos;

            viveControllerMappingImage = new GUIStyle
            {
                normal =
                    {
                        background = Resources.Load("handanimvivemapping") as Texture2D,
                        textColor = Color.white
                    },
                fixedWidth = 440,
                fixedHeight = 222
            };
        }

        //ToDo : Check controllers modified with AssetModificationProcessor
        private AnimatorOverrideController[] controllers = null;
        private List<KeyValuePair<AnimationClip, AnimationClip>>[] editedOverrides;
        private AnimationClip[] overrideKeys;
        private int controllersCount;

        private void OnGUI()
        {
            if (controllers == null || editedOverrides == null)
            {
                this.Close();
                return;
            }
            if (controllers.Length != controllersCount || editedOverrides.Length != controllersCount)
            {
                this.Close();
                return;
            }
            for (int i = 0; i < controllersCount; i++)
            {
                if (controllers[i] == null)
                {
                    this.Close();
                    return;
                }
                if (editedOverrides[i] == null)
                {
                    this.Close();
                    return;
                }
            }

            GUILayout.BeginVertical();
            {
                GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("util_vrcanimatoroverridecontroller_editor"));
                GUILayout.Space(4);

                #region selected controllers field
                GUILayout.BeginVertical(GUI.skin.box);
                controllersFoldOut = EditorGUILayout.Foldout(controllersFoldOut, "Controllers", true);
                if (controllersFoldOut)
                {
                    for (int i = 0; i < controllersCount; i++)
                    {
                        var controller = controllers[i];
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.ObjectField(controller, typeof(AnimatorOverrideController), false);
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                if (controllersCount > 1)
                    EditorGUILayout.HelpBox("Multiple Controllers Selected!\r\nIt will be Multi edit mode.", MessageType.Info);

                GUILayout.EndVertical();
                #endregion

                #region Set Animation Tab
                GUILayout.BeginHorizontal();

                GUI.backgroundColor = selectedTab == VRCAnimationType.Hand ? Color.green : Color.gray;
                if (GUILayout.Button("Hand"))
                    selectedTab = VRCAnimationType.Hand;

                GUI.backgroundColor = selectedTab == VRCAnimationType.Emote ? Color.green : Color.gray;
                if (GUILayout.Button("Emote"))
                    selectedTab = VRCAnimationType.Emote;

                GUI.backgroundColor = selectedTab == VRCAnimationType.Default ? Color.green : Color.gray;
                if (GUILayout.Button("Default"))
                    selectedTab = VRCAnimationType.Default;

                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                #endregion

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    animationsScroll = GUILayout.BeginScrollView(animationsScroll);
                    switch (selectedTab)
                    {
                        case VRCAnimationType.Hand:
                            GUIHandTab();
                            break;
                        case VRCAnimationType.Emote:
                            GUIEmoteTab();
                            break;
                        case VRCAnimationType.Default:
                            GUIIDefault();
                            break;
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.Space(5);
                if (GUILayout.Button("Reset", GUILayout.Height(30)))
                {
                    for (int controllerIndex = 0; controllerIndex < controllersCount; controllerIndex++)
                    {
                        var controller = controllers[controllerIndex];
                        List<KeyValuePair<AnimationClip, AnimationClip>> baseOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                        controller.GetOverrides(baseOverrides);

                        for (int animationIndex = 0; animationIndex < VRCAnimation.VRCAnimationCount; animationIndex++)
                            editedOverrides[controllerIndex][animationIndex] = new KeyValuePair<AnimationClip, AnimationClip>(baseOverrides[animationIndex].Key, baseOverrides[animationIndex].Value);
                    }
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Apply", GUILayout.Height(30)))
                {
                    for (int controllerIndex = 0; controllerIndex < controllersCount; controllerIndex++)
                    {
                        var controller = controllers[controllerIndex];
                        List<KeyValuePair<AnimationClip, AnimationClip>> baseOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                        controller.GetOverrides(baseOverrides);

                        //handanimvivemapping
                        controller.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>(editedOverrides[controllerIndex]));
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void GUIHandTab()
        {
            GUIDrawDefaultAnimationClipUI<VRCAnimationHand>();

            Debug.Log(position.height);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", viveControllerMappingImage);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        private void GUIEmoteTab()
        {
            GUIDrawDefaultAnimationClipUI<VRCAnimationEmote>();
        }
        private void GUIIDefault()
        {
            GUIDrawDefaultAnimationClipUI<VRCAnimationDefault>();
        }

        private void GUIDrawDefaultAnimationClipUI<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            GUILayout.BeginVertical();
            {
                var names = Enum.GetNames(typeof(T));
                int count = names.Length;

                for (int i = 0; i < count; i++)
                {
                    var animationType = (Enum)(Enum.Parse(typeof(T), names[i]));
                    var animationIndex = (int)(object)animationType;
                    var animationKey = overrideKeys[animationIndex];
                    var animationName = animationKey.name;

                    GUILayout.BeginHorizontal();
                    {
                        bool isEdited = false;
                        for (int controllerIndex = 0; controllerIndex < controllersCount; controllerIndex++)
                        {
                            List<KeyValuePair<AnimationClip, AnimationClip>> baseOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                            controllers[controllerIndex].GetOverrides(baseOverrides);

                            if (baseOverrides[animationIndex].Value != editedOverrides[controllerIndex][animationIndex].Value)
                            {
                                isEdited = true;
                                break;
                            }
                        }

                        EditorGUILayout.LabelField(animationName, isEdited ? EditorStyles.boldLabel : EditorStyles.label);
                        GUILayout.FlexibleSpace();

                        bool isMixed = false;
                        var baseAnim = editedOverrides[0][animationIndex].Value;
                        for (int controllerIndex = 1; controllerIndex < controllersCount; controllerIndex++)
                        {
                            var curAnim = editedOverrides[controllerIndex][animationIndex].Value;
                            if (baseAnim != curAnim)
                            {
                                isMixed = true;
                                break;
                            }
                        }

                        EditorGUI.showMixedValue = isMixed;
                        EditorGUI.BeginChangeCheck();

                        var selectedAnim = EditorGUILayout.ObjectField(baseAnim, typeof(AnimationClip), false) as AnimationClip;
                        if (EditorGUI.EndChangeCheck())
                        {
                            for (int controllerIndex = 0; controllerIndex < controllersCount; controllerIndex++)
                                editedOverrides[controllerIndex][animationIndex] = new KeyValuePair<AnimationClip, AnimationClip>(editedOverrides[controllerIndex][animationIndex].Key, selectedAnim);
                        }

                        EditorGUI.showMixedValue = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void PlaymodeStateChanged()
        {
            if (Application.isPlaying)
            {
                if (this != null)
                    this.Close();
            }
        }
    }
}
