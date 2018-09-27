﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKExtension
{
    public class MainWindow : EditorWindow
    {
        public static void Init()
        {
            MainWindow window = (MainWindow)EditorWindow.GetWindow(typeof(MainWindow), true);
            window.Show();
        }

        public static bool foldout_Avatar = false;
        public static bool foldout_Avatar_SkinnedMeshRenderers = false;
        public static bool foldout_World = false;
        public static bool foldout_Util = false;

        public static VRC_AvatarDescriptor avatarObject;
        public static Transform AvatarTransform { get { return avatarObject == null ? null : avatarObject.transform; } }
        public static List<SkinnedMeshRenderer> avatarSkins;
        public static Animator avatarAnimator;
        public static Avatar avatarAnimatorAvatar;
        public static GameObject avatarModel;

        private static Vector2 changeLogScroll;

        private static GUIStyle vrcSdkExtensionHeader;
        private static Texture2D twitterThumbnail;
        private static Texture2D githubThumbnail;

        private static string changelog = null;

        private static void FindAvatar()
        {
            if (avatarObject == null)
                avatarObject = FindObjectOfType<VRC_AvatarDescriptor>();

            if (avatarSkins == null)
                avatarSkins = new List<SkinnedMeshRenderer>();
            avatarSkins.Clear();
            if (avatarObject != null)
            {
                int childCount = avatarObject.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var obj = avatarObject.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                    if (obj != null)
                        avatarSkins.Add(obj);
                }
            }

            avatarAnimator = null;
            if (avatarObject != null)
                avatarAnimator = avatarObject.GetComponent<Animator>();

            avatarAnimatorAvatar = null;
            if (avatarAnimator != null)
                avatarAnimatorAvatar = avatarAnimator.avatar;

            avatarModel = null;
            if (avatarObject != null)
            {
                var mrs = avatarObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                SkinnedMeshRenderer body = null;
                foreach (var mr in mrs)
                {
                    if (mr.name.ToLower() == "body")
                    {
                        body = mr;
                        break;
                    }
                }

                if (body != null)
                {
                    Mesh sm = body.sharedMesh;
                    avatarModel = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(sm.GetInstanceID()), typeof(GameObject)) as GameObject;
                }
            }
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);
            maxSize = new Vector2(400, 1000);
            minSize = new Vector2(400, 400);

            FindAvatar();
            vrcSdkExtensionHeader = new GUIStyle
            {
                normal =
                    {
                        background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRCSDKExtension/Media/VRCSDKExtension_Header.png"),
                        textColor = Color.white
                    },
                fixedWidth = 400,
                fixedHeight = 200
            };

            twitterThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRCSDKExtension/Media/Thumbnail/twitter.png");
            githubThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRCSDKExtension/Media/Thumbnail/github.png");

            changelog = (AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/VRCSDKExtension/Changelog.txt")).text;
        }
        void OnGUI()
        {
            GUILayout.Box("", vrcSdkExtensionHeader);
            GUILayout.Space(4);

            Localization.Init();

            #region Set Language
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = GlobalSettings.Settings.Language == Language.En ? Color.green : Color.gray;
            if (GUILayout.Button("English"))
                GlobalSettings.Settings.SetLanguage(Language.En);
            GUI.backgroundColor = GlobalSettings.Settings.Language == Language.Ko ? Color.green : Color.gray;

            if (GUILayout.Button("한국어"))
                GlobalSettings.Settings.SetLanguage(Language.Ko);
            GUI.backgroundColor = GlobalSettings.Settings.Language == Language.Ja ? Color.green : Color.gray;

            if (GUILayout.Button("日本語"))
                GlobalSettings.Settings.SetLanguage(Language.Ja);

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            #endregion

            GUILayout.Label("VRC SDK Extension. " + Localization.GetLocalizedString("global_version") + " " + VRChatSDKExtension.versionStr, EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("warnning_editor_is_playing"), MessageType.Warning);
                return;
            }

            var sceneType = VRChatSDKExtension.CheckSceneType();

            #region Avatar
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_Avatar = EditorGUILayout.Foldout(foldout_Avatar, Localization.GetLocalizedString("mainmenu_avatar"), true);
                if (foldout_Avatar)
                {
                    FindAvatar();

                    #region Show Avatar's Info
                    #region Avatar Object Field
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localization.GetLocalizedString("global_avatar"));
                    GUI.enabled = false;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.ObjectField(avatarObject, typeof(VRC_AvatarDescriptor), true);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    #endregion
                    #region Avatar Model Field
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localization.GetLocalizedString("global_model"));
                    GUILayout.FlexibleSpace();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(avatarModel, typeof(GameObject), true);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    #endregion
                    #region Avatar SkinnedMeshRenderer<List> Field
                    GUILayout.BeginVertical();
                    foldout_Avatar_SkinnedMeshRenderers = EditorGUILayout.Foldout(foldout_Avatar_SkinnedMeshRenderers,
                        Localization.GetLocalizedString("global_skinnedmeshrenderer_list"), true);
                    GUILayout.FlexibleSpace();
                    if (foldout_Avatar_SkinnedMeshRenderers)
                    {
                        GUI.enabled = false;
                        foreach (var smr in avatarSkins)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.ObjectField(smr, typeof(SkinnedMeshRenderer), true);
                            GUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                    #endregion
                    #endregion

                    if (avatarObject == null)
                        EditorGUILayout.HelpBox(Localization.GetLocalizedString("warnning_vrc_avatardescriptor_missing"), MessageType.Warning);
                    if (avatarObject != null && avatarAnimator == null)
                        EditorGUILayout.HelpBox(Localization.GetLocalizedString("warnning_avatar_animator_missing"), MessageType.Warning);
                    if (avatarAnimator != null && avatarAnimatorAvatar == null)
                        EditorGUILayout.HelpBox(Localization.GetLocalizedString("warnning_avatar_animator_avatar_missing"), MessageType.Warning);
                    if (avatarObject != null && avatarModel == null)
                        EditorGUILayout.HelpBox(Localization.GetLocalizedString("warnning_avatar_model_missing"), MessageType.Warning);

                    GUI.enabled = (avatarObject != null && avatarAnimator != null &&
                                        avatarAnimatorAvatar != null && avatarModel != null);
                    {
                        #region Help Functions
                        GUILayout.Label(Localization.GetLocalizedString("global_helper_function"), EditorStyles.boldLabel);
                        //Detect viseme blend shape
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape")))
                        {
                            AutoDetectVisemeBlendShape.DoAutoDetectVisemeBlendShape(avatarObject);
                        }
                        //Reset to model based pose
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_reset_to_model_based_pose")))
                        {
                            ResetToModelBasePose.DoResetToModelBasePose(avatarSkins, avatarModel);
                        }
                        //Set to T-Pose pose
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_set_to_t_pose")))
                        {
                            SetToTPose.DoSetTPose(avatarAnimator, AvatarTransform);
                        }
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_pose_editor")))
                        {
                            PoseEditorWindow.Init(avatarAnimator, AvatarTransform);
                        }
                        /*
                        if (GUILayout.Button("Create Test Pose From AnimationClip"))
                        {
                            SetToTPose.CreatePoseFromAnimationClip();
                        }
                        if (GUILayout.Button("Save Test Pose"))
                        {
                            SetToTPose.SaveTestPose(avatarAnimator, AvatarTransform);
                        }
                        if (GUILayout.Button("Apply Test Pose"))
                        {
                            SetToTPose.ApplyTestPose(avatarAnimator, AvatarTransform);
                        }
                        */
                        //Copy Avatar from New Model file
                        GUILayout.Label("This Function on develop. Danger!");
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file")))
                        {
                            CopyAvatarFromNewModelFileWindow_Lagacy.Init(avatarObject, avatarSkins, avatarModel);
                        }
                        #endregion

                        #region Testing Functions
                        GUILayout.Label(Localization.GetLocalizedString("global_testing_function"), EditorStyles.boldLabel);
                        //Test EyeTracking
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_testing_test_eyetracking")))
                        {
                            EyeTrackingTestWindow.Init(avatarAnimator);
                        }
                        #endregion
                    }
                    GUI.enabled = true;
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region World
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_World = EditorGUILayout.Foldout(foldout_World, Localization.GetLocalizedString("mainmenu_world"), true);
                if (foldout_World)
                {
                    GUILayout.Label(Localization.GetLocalizedString("global_nottingyet"));
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region Util
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_Util = EditorGUILayout.Foldout(foldout_Util, Localization.GetLocalizedString("mainmenu_util"), true);
                if (foldout_Util)
                {
                    if (GUILayout.Button(Localization.GetLocalizedString("util_copy_avatar_from_new_model_file")))
                    {
                        CopyAvatarFromNewModelFileWindow.Init(null);
                    }

                    //GUILayout.Label(Localization.GetLocalizedString("global_nottingyet"));
                }
            }
            GUILayout.EndVertical();
            #endregion

            if (GUILayout.Button(new GUIContent(Localization.GetLocalizedString("mainmenu_opengithub"), githubThumbnail)))
            {
                Application.OpenURL("https://github.com/hisacat/VRC-SDK-Helper/");
            }
            if (GUILayout.Button(new GUIContent(Localization.GetLocalizedString("mainmenu_opentwitter"), twitterThumbnail)))
            {
                Application.OpenURL("https://twitter.com/ahisacat/");
            }
            if (GUILayout.Button(Localization.GetLocalizedString("mainmenu_checkforupdates")))
            {
                if (EditorUtility.DisplayDialog("VRC SDK Extension", "Update check function is preparing\r\nOpen Github?", "Yes", "No"))
                {
                    Application.OpenURL("https://github.com/hisacat/VRC-SDK-Helper/");
                }
            }
            changeLogScroll = GUILayout.BeginScrollView(changeLogScroll);
            GUILayout.Label(changelog);
            GUILayout.EndScrollView();
        }
    }
}