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
        float scaleSliderValue = 1;

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += LogPlayModeState;
            titleContent = new GUIContent("VRChat SDK Helper - Test EyeTracking");
            minSize = maxSize = new Vector2(150, 150);
            testEyeTracking = null;

            EditorApplication.isPlaying = true;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            if (Application.isEditor && Application.isPlaying)
            {
                if (testEyeTracking == null)
                {
                    testEyeTracking = GameObject.Instantiate(Resources.Load("TestEyeTrackingPrefab")) as GameObject;
                    testEyeTracking.name = "TestEyeTracking";

                    var headTrf = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
                    testEyeTracking.transform.position = headTrf.position - (headTrf.forward * -2f);

                    var ep = testEyeTracking.GetComponentInChildren<EyeTrackingPoint>();
                    ep.SetEyes(avatarAnimator.GetBoneTransform(HumanBodyBones.LeftEye), avatarAnimator.GetBoneTransform(HumanBodyBones.RightEye));
                }

                GUILayout.Label("Scale");
                scaleSliderValue = GUILayout.HorizontalSlider(scaleSliderValue, 0, 2);
                testEyeTracking.GetComponentInChildren<EyeTrackingPoint>().SetScale(scaleSliderValue);

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