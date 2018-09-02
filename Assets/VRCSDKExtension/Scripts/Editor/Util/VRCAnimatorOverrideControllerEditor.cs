using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public class VRCAnimatorOverrideControllerEditorWindow : EditorWindow
    {
        private static List<AnimatorOverrideController> GetControllerFromSelection()
        {
            var controllers = new List<AnimatorOverrideController>();

            var guids = Selection.assetGUIDs;
            int count = guids.Length;
            for (int i = 0; i < count; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                var controller = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimatorOverrideController)) as AnimatorOverrideController;
                if (controller != null)
                {
                    Debug.Log(controller.runtimeAnimatorController.name);
                    controllers.Add(controller);
                }
            }
            
            return controllers;
        }

        [MenuItem("Assets/VRChat AnimatorOverrideController Edit", true)]
        public static bool OpenWithAnimatorOverrideControllerMenuValidation()
        {
            var controllers = GetControllerFromSelection();
            return (controllers.Count > 0 && controllers.Count == Selection.assetGUIDs.Length);
        }
        [MenuItem("Assets/VRChat AnimatorOverrideController Edit")]
        private static void OpenWithAnimatorOverrideControllerMenu()
        {
            var controllers = GetControllerFromSelection();
            Init(controllers.ToArray());
        }

        public static void Init(AnimatorOverrideController[] controllers)
        {
            VRCAnimatorOverrideControllerEditorWindow window = (VRCAnimatorOverrideControllerEditorWindow)EditorWindow.GetWindow(typeof(VRCAnimatorOverrideControllerEditorWindow), true);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("util_animationclip_path_editor"));
                GUILayout.Space(4);

                GUILayout.BeginVertical(GUI.skin.box);
                {
                }
            }
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);

            var minSize = this.minSize;
            minSize.x = 300;
            this.minSize = minSize;

            var pos = this.position;
            pos.width = 400;
            this.position = pos;
        }
    }
}
