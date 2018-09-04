using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public class PoseEditorWindow : EditorWindow
    {
        public static void Init(Animator animator, Transform model)
        {
            PoseEditorWindow window = (PoseEditorWindow)EditorWindow.GetWindow(typeof(PoseEditorWindow), true);
            window.animator = animator;
            window.model = model;

            window.humanPose = new HumanPose();
            window.humanPoseHandler = new HumanPoseHandler(animator.avatar, model);
            window.humanPoseHandler.GetHumanPose(ref window.humanPose);

            window.Show();
        }

        private Animator animator;
        private Transform model;
        private HumanPose humanPose;
        private HumanPoseHandler humanPoseHandler;
        private Vector2 musclesScroll;

        private void OnEnable()
        {
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);

            musclesScroll = Vector2.zero;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("avatar_pose_editor"));
            GUILayout.Space(4);

            musclesScroll = GUILayout.BeginScrollView(musclesScroll);
            GUILayout.BeginVertical();
            for (int i = 0; i < PoseAsset.MusclesCount; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(HumanTrait.MuscleName[i],GUILayout.Width(200));
                //GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                humanPose.muscles[i] = GUILayout.HorizontalSlider(humanPose.muscles[i], -1f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int boneindex = 0; boneindex < PoseAsset.HumanBodyBoneCount; boneindex++)
                    {
                        var boneTrf = animator.GetBoneTransform((HumanBodyBones)boneindex);
                        if (boneTrf != null)
                            Undo.RecordObject(boneTrf, "Pose Editor");
                    }
                    humanPoseHandler.SetHumanPose(ref humanPose);
                    //this.Repaint();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            //Undo.RecordObject(boneTrf, "Apply Pose");
            //humanPoseHandler.SetHumanPose(ref humanPose);
            GUILayout.EndVertical();
        }

        private void OnDisable()
        {
            humanPoseHandler.Dispose();
        }
    }
}