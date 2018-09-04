using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKExtension
{
    public class EyeTrackingTestWindow : EditorWindow
    {
        public static void Init(Animator avatarAnimator)
        {
            EyeTrackingTestWindow window = (EyeTrackingTestWindow)EditorWindow.GetWindow(typeof(EyeTrackingTestWindow), true);
            window.avatarAnimator = avatarAnimator;
            window.Show();
        }

        private GameObject testEyeTracking = null;
        private Animator avatarAnimator;
        float updownSliderValue = 0;
        float leftRightSliderValue = 0;

        private Transform leftEyeTrf = null;
        private Transform rightEyeTrf = null;

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);
            minSize = maxSize = new Vector2(150, 160);
            testEyeTracking = null;
            
            EditorApplication.isPlaying = true;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("avatar_testing_test_eyetracking"));
            GUILayout.Space(4);

            if (Application.isEditor && Application.isPlaying)
            {

                if (testEyeTracking == null)
                {
                    testEyeTracking = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VRCSDKExtension/EditorPrefabs/TestEyeTrackingPrefab.prefab"));
                    testEyeTracking.name = "TestEyeTracking";

                    var headTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
                    testEyeTracking.transform.position = headTrf.position - (headTrf.forward * -2f);

                    leftEyeTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftEye);
                    rightEyeTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.RightEye);
                }

                GUILayout.Label(Localization.GetLocalizedString("avatar_testing_test_eyetracking_up_down") + " : " + (int)updownSliderValue);
                updownSliderValue = GUILayout.HorizontalSlider(updownSliderValue, -19, 25);
                GUILayout.Label(Localization.GetLocalizedString("avatar_testing_test_eyetracking_left_right") + " : " + (int)leftRightSliderValue);
                leftRightSliderValue = GUILayout.HorizontalSlider(leftRightSliderValue, -19, 19);

                rightEyeTrf.localEulerAngles = leftEyeTrf.localEulerAngles = new Vector3(updownSliderValue, -leftRightSliderValue, 0);

                if (GUILayout.Button(Localization.GetLocalizedString("global_reset_to_default")))
                {
                    updownSliderValue = 0;
                    leftRightSliderValue = 0;
                }
                if (GUILayout.Button(Localization.GetLocalizedString("global_stop")))
                {
                    this.Close();
                }
            }
            else
            {
                GUILayout.Label(Localization.GetLocalizedString("global_loading"));
            }
            GUILayout.EndVertical();
        }

        private void PlaymodeStateChanged()
        {
            if (!Application.isPlaying)
            {
                if (this != null)
                    this.Close();
            }
        }

        private void OnDestroy()
        {
            EditorApplication.isPlaying = false;
        }
    }
}