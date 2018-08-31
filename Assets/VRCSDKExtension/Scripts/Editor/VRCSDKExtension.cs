using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKExtension
{
    public enum SceneType : int
    {
        None = -1,
        Avatar = 0,
        World = 1,
    }
    
    public static class VRChatSDKExtension
    {
        public const string ProjectName = "VRChat SDK Extension";

        public const string versionStr = "0.1b";
        public const int version = 0;
        
        [MenuItem("VRChat SDK Extension/Open Helper Window")]
        public static void OpenHelperWindow()
        { 
            MainWindow.Init();
        }

        public static SceneType CheckSceneType()
        {
            if (GameObject.FindObjectOfType<VRC_AvatarDescriptor>() != null)
                return SceneType.Avatar;
            else if (GameObject.FindObjectOfType<VRC_SceneDescriptor>() != null)
                return SceneType.World;
            else
                return SceneType.None;
        }
    }
}
