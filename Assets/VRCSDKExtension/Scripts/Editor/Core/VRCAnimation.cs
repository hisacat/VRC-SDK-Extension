using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension.Animation
{
    public enum VRCAnimationAll : int
    {
        CROUCHIDLE = 0,
        CROUCHWALKFWD = 1,
        CROUCHWALKRT = 2,
        EMOTE1 = 3,
        EMOTE2 = 4,
        EMOTE3 = 5,
        EMOTE4 = 6,
        EMOTE5 = 7,
        EMOTE6 = 8,
        EMOTE7 = 9,
        EMOTE8 = 10,
        FALL = 11,
        FINGERPOINT = 12,
        FIST = 13,
        HANDGUN = 14,
        HANDOPEN = 15,
        IDLE = 16,
        PRONEFWD = 17,
        PRONEIDLE = 18,
        RUNBACK = 20,
        RUNFWD = 21,
        RUNSTRAFELT45 = 22,
        RUNSTRAFELT135 = 23,
        RUNSTRAFERT45 = 24,
        RUNSTRAFERT135 = 25,
        SPRINTFWD = 26,
        STRAFELT45 = 27,
        STRAFELT135 = 28,
        STRAFERT = 29,
        STRAFERT45 = 30,
        STRAFERT135 = 31,
        THUMBSUP = 32,
        VICTORY = 33,
        WALKBACK = 34,
        WALKFWD = 35,
    }
    public enum VRCAnimationHand
    {
        FINGERPOINT = 12,
        FIST = 13,
        HANDGUN = 14,
        HANDOPEN = 15,
        ROCKNROLL = 19,
        THUMBSUP = 32,
        VICTORY = 33,
    }
    public enum VRCAnimationEmote
    {
        EMOTE1 = 3,
        EMOTE2 = 4,
        EMOTE3 = 5,
        EMOTE4 = 6,
        EMOTE5 = 7,
        EMOTE6 = 8,
        EMOTE7 = 9,
        EMOTE8 = 10,
    }
    public enum VRCAnimationDefault
    {
        CROUCHIDLE = 0,
        CROUCHWALKFWD = 1,
        CROUCHWALKRT = 2,
        FALL = 11,
        IDLE = 16,
        PRONEFWD = 17,
        PRONEIDLE = 18,
        ROCKNROLL = 19,
        RUNBACK = 20,
        RUNFWD = 21,
        RUNSTRAFELT45 = 22,
        RUNSTRAFELT135 = 23,
        RUNSTRAFERT45 = 24,
        RUNSTRAFERT135 = 25,
        SPRINTFWD = 26,
        STRAFELT45 = 27,
        STRAFELT135 = 28,
        STRAFERT = 29,
        STRAFERT45 = 30,
        STRAFERT135 = 31,
        WALKBACK = 34,
        WALKFWD = 35,
    }
    public enum VRCAnimationType : int
    {
        Hand = 0,
        Emote = 1,
        Default = 2,
    }

    public static class VRCAnimation
    {
        public const int VRCAnimationCount = 36;

        public static List<AnimatorOverrideController> GetControllerFromSelection()
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
                    controllers.Add(controller);
            }

            return controllers;
        }
        public static bool CheckVRCAnimatiorOverrideController(AnimatorOverrideController controller)
        {
            if (controller == null)
                return false;
            if (controller.runtimeAnimatorController == null)
                return false;
            if (controller.runtimeAnimatorController.name != "AvatarControllerTemplate")
                return false;
            if (controller.overridesCount != VRCAnimationCount)
                return false;

            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            controller.GetOverrides(overrides);

            for (int i = 0; i < VRCAnimationCount; i++)
            {
                if (!(overrides.Exists(x => x.Key.name == ((VRCAnimationAll)i).ToString())))
                    return false;
            }

            return true;
        }

        public static RuntimeAnimatorController GetVRCRuntimeAnimatorController()
        {
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(@"Assets\VRCSDK\Examples\Sample Assets\Animation\AvatarControllerTemplate.controller");
        }
    }
}