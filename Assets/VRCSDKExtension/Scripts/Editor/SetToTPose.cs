using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public static class SetToTPose
    {
        public static void DoSetTPose(Animator animator, Transform avatarTransform)
        {
            PoseAsset.ApplyPose(animator, avatarTransform, PoseAsset.GetTPoseAsset());
        }
        public static void CreatePoseFromAnimationClip()
        {
            AnimationClip clip = Resources.Load("targetClip") as AnimationClip;
            PoseAsset.CreatePoseAssetFronAnimationClip(clip, 40, "targetClip");
        }
        public static void SaveTestPose(Animator animator, Transform avatarTransform)
        {
            PoseAsset.CreatePoseAsset(animator, avatarTransform, "testPose");
        }
        public static void ApplyTestPose(Animator animator, Transform avatarTransform)
        {
            PoseAsset.ApplyPose(animator, avatarTransform, PoseAsset.GetPoseAssetbyFileName("testPose"));
        }
    }
}