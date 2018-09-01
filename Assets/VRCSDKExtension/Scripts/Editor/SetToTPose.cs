using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public static class SetToTPose
    {
        public static void DoSetTPose(Animator animator, GameObject model)
        {
            HumanPoseHandler humanPoseHandler = new HumanPoseHandler(animator.avatar, model.transform);
            HumanPose humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);


        }
    }
}