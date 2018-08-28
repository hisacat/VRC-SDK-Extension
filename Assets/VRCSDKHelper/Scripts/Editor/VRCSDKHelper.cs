using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKHelper
{
    public enum SceneType : int
    {
        None = -1,
        Avatar = 0,
        World = 1,
    }
    public enum Language : int
    {
        En = 0,
        Ko = 1,
        Jp = 2,
    }

    public static class VRCSDKHelper
    {
        public const string versionStr = "0.1b";
        public const int version = 0;

        public static Language language;

        [MenuItem("VRChat SDK Helper/Open Helper Window")]
        public static void OpenHelperWindow()
        {
            HelperWindow.Init();
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
