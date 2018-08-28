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

            //GUILayout.Label("VRC SDK Helper", EditorStyles.boldLabel);
            GUILayout.Label("VRC SDK Helper. Version " + VRCSDKHelper.versionStr, EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Editor is Playing", MessageType.Warning);
                return;
            }

            var sceneType = VRCSDKHelper.CheckSceneType();

            #region Avatar
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_Avatar = EditorGUILayout.Foldout(foldout_Avatar, "Avatar", true);
                if (foldout_Avatar)
                {
                    FindAvatar();
                    #region Avatar Object Field
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Avatar");
                    GUI.enabled = false;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.ObjectField(avatarObject, typeof(VRC_AvatarDescriptor), true);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    #endregion
                    #region Avatar Model Field
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Model");
                    GUILayout.FlexibleSpace();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(avatarModel, typeof(GameObject), true);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    #endregion

                    if (avatarObject == null)
                        EditorGUILayout.HelpBox("VRC_AvatarDescriptor is Missing!", MessageType.Warning);
                    if (avatarAnimator == null)
                        EditorGUILayout.HelpBox("Avatar's Animator is Missing!", MessageType.Warning);
                    if (avatarAnimator != null && avatarAnimatorAvatar == null)
                        EditorGUILayout.HelpBox("Avatar's Animator Avatar Missing!", MessageType.Warning);
                    if (avatarObject != null && avatarModel == null)
                        EditorGUILayout.HelpBox("Avatar's Model is Missing!", MessageType.Warning);

                    GUI.enabled = (avatarObject != null && avatarAnimator != null &&
                                        avatarAnimatorAvatar != null && avatarModel != null);
                    {
                        GUILayout.Label("Helper", EditorStyles.boldLabel);
                        if (GUILayout.Button("Detect Viseme Blend Shape"))
                        {
                            AutoDetectVisemeBlendShape.DoAutoDetectVisemeBlendShape(avatarObject);
                        }
                        if (GUILayout.Button("Reset to Base Pose(T-Pose)"))
                        {
                            ResetToBasePose.DoResetToBasePose(avatarAnimator, avatarModel);

                        }

                        GUILayout.Label("Testing", EditorStyles.boldLabel);
                        if (GUILayout.Button("Test EyeTracking"))
                        {
                            EyeTrackingTestWindow.Init(avatarAnimator);
                        }
                    }
                    GUI.enabled = true;
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region World
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_World = EditorGUILayout.Foldout(foldout_World, "World", true);
                if (foldout_World)
                {
                    GUILayout.Label("Notting yet...");
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region Util
            GUILayout.BeginVertical(GUI.skin.box);
            {
                foldout_Util = EditorGUILayout.Foldout(foldout_Util, "Util", true);
                if (foldout_Util)
                {

                }
            }
            GUILayout.EndVertical();
            #endregion

            if (GUILayout.Button("Open GitHub"))
            {
                Application.OpenURL("https://github.com/hisacat/VRC-SDK-Helper/");
            }
            if (GUILayout.Button("Check for Updates"))
            {
                if(EditorUtility.DisplayDialog("VRC SDK Helper", "Update check function is preparing\r\nOpen Github?", "Yes", "No"))
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
    -Added 'Reset to Base Pose(T-Pose)' function"
            );
            GUILayout.EndScrollView();
        }

    }
}