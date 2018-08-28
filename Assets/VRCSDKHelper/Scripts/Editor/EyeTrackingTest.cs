using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;

namespace VRCSDKHelper
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
            EditorApplication.playmodeStateChanged += LogPlayModeState;
            titleContent = new GUIContent("Test EyeTracking");
            minSize = maxSize = new Vector2(150, 160);
            testEyeTracking = null;

            EditorApplication.isPlaying = true;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Helper\r\nTest EyeTracking");
            GUILayout.Space(4);

            if (Application.isEditor && Application.isPlaying)
            {

                if (testEyeTracking == null)
                {
                    testEyeTracking = GameObject.Instantiate(Resources.Load("TestEyeTrackingPrefab")) as GameObject;
                    testEyeTracking.name = "TestEyeTracking";

                    var headTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
                    testEyeTracking.transform.position = headTrf.position - (headTrf.forward * -2f);

                    leftEyeTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftEye);
                    rightEyeTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.RightEye);
                }

                GUILayout.Label("Up - Down : " + (int)updownSliderValue);
                updownSliderValue = GUILayout.HorizontalSlider(updownSliderValue, -19, 25);
                GUILayout.Label("Left - Right : " + (int)leftRightSliderValue);
                leftRightSliderValue = GUILayout.HorizontalSlider(leftRightSliderValue, -19, 19);

                rightEyeTrf.localEulerAngles = leftEyeTrf.localEulerAngles = new Vector3(updownSliderValue, -leftRightSliderValue, 0);

                if (GUILayout.Button("Reset to Default"))
                {
                    updownSliderValue = 0;
                    leftRightSliderValue = 0;
                }
                if (GUILayout.Button("Stop"))
                {
                    this.Close();
                }
            }
            else
            {
                GUILayout.Label("Loading…");
            }
            GUILayout.EndVertical();
        }

        private void LogPlayModeState()
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