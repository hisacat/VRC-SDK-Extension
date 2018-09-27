using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using System.Reflection;
using System.Linq;
using System;

namespace VRCSDKExtension
{
    public class CopyAvatarFromNewModelFileWindow : EditorWindow
    {
        [MenuItem("GameObject/VRCSDK Extension/Copy Avatar From New Model File")]
        private static void OpenWithAnimatorOverrideControllerMenu()
        {
            Init(Selection.activeGameObject.GetComponent<VRC_AvatarDescriptor>());
        }

        public static void Init(VRC_AvatarDescriptor avatarObject)
        {
            CopyAvatarFromNewModelFileWindow window = (CopyAvatarFromNewModelFileWindow)EditorWindow.GetWindow(typeof(CopyAvatarFromNewModelFileWindow), true);
            window.avatarObject = avatarObject;
            window.Show();
        }

        private VRC_AvatarDescriptor avatarObject = null;
        private GameObject newAvatarModel = null;
        private bool copyPosition = false;
        private bool copyRotation = false;
        private bool copyScale = false;

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);
            var minSize = this.minSize;
            var maxSize = this.maxSize;
            minSize.x = 400;
            maxSize.x = 400;
            this.minSize = minSize;
            this.maxSize = maxSize;
        }
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"));
            GUILayout.Space(4);

            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Localization.GetLocalizedString("global_avatar"));
                GUILayout.FlexibleSpace();
                avatarObject = EditorGUILayout.ObjectField(avatarObject, typeof(VRC_AvatarDescriptor), true) as VRC_AvatarDescriptor;
                GUILayout.EndHorizontal();

            }

            if (avatarObject == null)
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("Please select Avatar"), MessageType.Warning);
            if (newAvatarModel == null)
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("Please select New Avatar Model"), MessageType.Warning);


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
