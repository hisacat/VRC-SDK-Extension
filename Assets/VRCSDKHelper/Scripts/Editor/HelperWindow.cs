using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKHelper
{
    public class HelperWindow : EditorWindow
    {
        public static void Init()
        {
            HelperWindow window = (HelperWindow)EditorWindow.GetWindow(typeof(HelperWindow), true);
            window.Show();
        }

        public static bool foldout_Avatar = false;
        public static bool foldout_World = false;
        public static bool foldout_Util = false;

        public static VRC_AvatarDescriptor avatarObject;
        public static Animator avatarAnimator;
        public static Avatar avatarAnimatorAvatar;
        public static GameObject avatarModel;

        private static Vector2 changeLogScroll;

        private static void FindAvatar()
        {
            if (avatarObject == null)
                avatarObject = FindObjectOfType<VRC_AvatarDescriptor>();

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

        private static GUIStyle vrcSdkHelperHeader;

        private void OnEnable()
        {
            titleContent = new GUIContent("VRChat SDK Helper");
            maxSize = new Vector2(400, 1000);
            minSize = new Vector2(400, 400);

            FindAvatar();
            vrcSdkHelperHeader = new GUIStyle
            {
                normal =
                    {
                        background = Resources.Load("vrcSdkHelperHeader") as Texture2D,
                        textColor = Color.white
                    },
                fixedWidth = 400,
                fixedHeight = 200
            };
        }
        void OnGUI()
        {
            GUILayout.Box("", vrcSdkHelperHeader);
            GUILayout.Space(4);

            Localization.Init();

            #region Set Language
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("English"))
            {
                VRCSDKHelper.SetLanguage(Language.En);
            }
            if (GUILayout.Button("한국어"))
            {
                VRCSDKHelper.SetLanguage(Language.Ko);
            }
            if (GUILayout.Button("日本語"))
            {
                VRCSDKHelper.SetLanguage(Language.Jp);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            #endregion

            GUILayout.Label("VRC SDK Helper. " + Localization.GetLocalizedString("global_version") + " " + VRCSDKHelper.versionStr, EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Editor is Playing", MessageType.Warning);
                return;
            }

            var sceneType = VRCSDKHelper.CheckSceneType();

            #region Avatar
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_Avatar = EditorGUILayout.Foldout(foldout_Avatar, Localization.GetLocalizedString("mainmenu_avatar"), true);
                if (foldout_Avatar)
                {
                    FindAvatar();
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
                        //Reset to base pose
                        if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_reset_to_base_pose")))
                        {
                            ResetToBasePose.DoResetToBasePose(avatarAnimator, avatarModel);

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
                    GUILayout.Label(Localization.GetLocalizedString("global_nottingyet"));
                }
            }
            GUILayout.EndVertical();
            #endregion

            if (GUILayout.Button(Localization.GetLocalizedString("mainmenu_opengithub")))
            {
                Application.OpenURL("https://github.com/hisacat/VRC-SDK-Helper/");
            }
            if (GUILayout.Button(Localization.GetLocalizedString("mainmenu_checkforupdates")))
            {
                if (EditorUtility.DisplayDialog("VRC SDK Helper", "Update check function is preparing\r\nOpen Github?", "Yes", "No"))
                {
                    Application.OpenURL("https://github.com/hisacat/VRC-SDK-Helper/");
                }
            }
            changeLogScroll = GUILayout.BeginScrollView(changeLogScroll);
            GUILayout.Label(
    @"Changelog:
2018.08.29
First Release.
    -Make Helper Window
    -Added 'Detect Viseme Blend Shape' function
    -Added 'Reset to Base Pose(T-Pose)' function
    -Added 'Test EyeTracking' function
    -Added Localization"
            );
            GUILayout.EndScrollView();
        }

    }
}