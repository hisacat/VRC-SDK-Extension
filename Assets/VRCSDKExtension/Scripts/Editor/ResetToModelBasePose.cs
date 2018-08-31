using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace VRCSDKExtension
{
    public static class ResetToModelBasePose
    {
        public static void DoResetToModelBasePose(List<SkinnedMeshRenderer> skins, GameObject model)
        {
            var dummyModel = GameObject.Instantiate(model);
            dummyModel.name = "DummyModel(VRChatSDKExtension - Reset To T Pose)";
            var baseSkins = dummyModel.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

            foreach (var skin in skins)
            {
                foreach (var baseSkin in baseSkins)
                {
                    if (skin.transform.name == baseSkin.transform.name)
                    {
                        var bones = skin.bones;
                        var baseBones = baseSkin.bones;
                        foreach (var bone in bones)
                        {
                            foreach (var baseBone in baseBones)
                            {
                                if (bone.name == baseBone.name)
                                {
                                    Undo.RecordObject(bone, "Reset To Model Base Pose");
                                    bone.transform.localPosition = baseBone.transform.localPosition;
                                    bone.transform.localRotation = baseBone.transform.localRotation;
                                    bone.transform.localRotation = baseBone.transform.localRotation;
                                    continue;
                                }
                            }
                        }

                        continue;
                    }
                }
            }
            GameObject.DestroyImmediate(dummyModel);
        }
    }
}