using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public static class ResetToTPose
    {
        public static void DoResetToTPose(Animator animator, GameObject model)
        {
            AnimationClip tPoseClip = Resources.Load("tPose") as AnimationClip;
            var allbindlings = AnimationUtility.GetCurveBindings(tPoseClip);

            HumanPoseHandler humanPoseHandler = new HumanPoseHandler(animator.avatar, model.transform);
            HumanPose humanPose = new HumanPose();
            humanPoseHandler.GetHumanPose(ref humanPose);

            int muscleCount = HumanTrait.MuscleCount;

            Debug.Log("COUNT : " + muscleCount);
            int co = 0;
            foreach (var bind in allbindlings)
            {
                var propertyName = bind.propertyName;
                if (propertyName.Contains("Stretched") || propertyName.Contains("Spread"))
                {
                    propertyName = propertyName.Replace("LeftHand", "Left");
                    propertyName = propertyName.Replace("RightHand", "Left");
                    propertyName = propertyName.Replace(".", " ");
                }

                bool find = false;
                for (int i = 0; i < muscleCount; i++)
                {
                    if (propertyName == HumanTrait.MuscleName[i])
                    {
                        co++;
                        find = true;
                        var curve = AnimationUtility.GetEditorCurve(tPoseClip, bind);
                        var value = curve.keys[0].value;

                        Debug.Log("CCC " + propertyName + " " +value);
                        humanPose.muscles[i] = value;

                        break;
                    }
                }
                if (!find)
                {
                    Debug.Log(bind.propertyName + " " + propertyName);
                }
            }
            Debug.Log("COUNcT : " + co);
            
            
            humanPoseHandler.SetHumanPose(ref humanPose);

            return;
            foreach (var bind in allbindlings)
            {
                Debug.Log(bind.path + " " + bind.propertyName);
                var curve = AnimationUtility.GetEditorCurve(tPoseClip, bind);


                //curve.keys[0].value;
            }


            return;
            var dummyModel = GameObject.Instantiate(model);
            dummyModel.name = "DummyModel(VRChatSDKExtension - Reset To Model Based Pose)";
            var baseAnimator = dummyModel.GetComponent<Animator>();

            List<Transform> baseBoneList = new List<Transform>();
            List<Transform> targetBoneList = new List<Transform>();
            int boneCount = (int)HumanBodyBones.LastBone + 1;
            for (int i = 0; i < boneCount; i++)
            {
                var bone = (HumanBodyBones)i;
                var targetTrf = animator.GetBoneTransform(bone);
                var baseTrf = baseAnimator.GetBoneTransform(bone);

                if (targetTrf == null || baseTrf == null)
                    continue;

                Undo.RecordObject(targetTrf, "Reset To T Pose");

                targetTrf.localPosition = baseTrf.localPosition;
                targetTrf.localRotation = baseTrf.localRotation;
                targetTrf.localScale = baseTrf.localScale;
            }

            GameObject.DestroyImmediate(dummyModel);
        }
    }
}