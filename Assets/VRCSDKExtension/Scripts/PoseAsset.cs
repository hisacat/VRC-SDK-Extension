using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace VRCSDKExtension
{
    public class PoseAsset : ScriptableObject
    {
        private const string TPoseFilePath = "Assets/VRCSDKExtension/Resources/tpose.asset";
        private const string PoseAssetDir = "Assets/VRCSDKExtension/Resources/Pose/";

        [SerializeField]
        private string poseName = null;
        public string PoseName { get { return poseName; } }

        [SerializeField]
        private Texture2D thumbnail = null;
        public Texture2D Thumbnail { get { return thumbnail; } }

        [SerializeField]
        private float[] muscles = null;
        public float[] Muschles { get { return muscles; } }

        [SerializeField]
        private bool[] isValied = null;
        public bool[] IsValied { get { return isValied; } }

        public const int HumanBodyBoneCount = 56;
        public const int MusclesCount = 95;

        public static void CreatePoseAsset(Animator animator, Transform model, string name, Texture2D thumbnail = null, string path = "")
        {
            //Todo - check another file in same path
            if (string.IsNullOrEmpty(path))
                path = PoseAssetDir;
            if (!(path.EndsWith("/") || path.EndsWith("\\")))
                path = path + "/";
            path = path + name + ".asset";

            using (HumanPoseHandler humanPoseHandler = new HumanPoseHandler(animator.avatar, model))
            {
                if (!animator.isHuman)
                {
                    Debug.LogError(animator.gameObject.name + " is not human!");
                    return;
                }

                HumanPose humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);

                var pose = CreateInstance<PoseAsset>();
                pose.poseName = name;
                pose.thumbnail = thumbnail;
                pose.muscles = new float[MusclesCount];
                pose.isValied = new bool[MusclesCount];

                for (int i = 0; i < MusclesCount; i++)
                {
                    pose.muscles[i] = humanPose.muscles[i];
                    pose.isValied[i] = true;
                }
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(pose, path);
#endif
            }
        }

        public static void CreatePoseAssetFronAnimationClip(AnimationClip clip, float time, string name, Texture2D thumbnail = null, string path = "")
        {
            //Todo - check another file in same path
            if (string.IsNullOrEmpty(path))
                path = PoseAssetDir;
            if (!(path.EndsWith("/") || path.EndsWith("\\")))
                path = path + "/";
            path = path + name + ".asset";

            if (clip == null)
            {
                Debug.LogError("Clip dose not exist");
                return;
            }

            var pose = CreateInstance<PoseAsset>();
            pose.poseName = name;
            pose.thumbnail = thumbnail;
            pose.muscles = new float[MusclesCount];
            pose.isValied = new bool[MusclesCount];


            var allbindlings = AnimationUtility.GetCurveBindings(clip);
            foreach (var bind in allbindlings)
            {
                var propertyName = bind.propertyName;
                if (propertyName.Contains("Stretched") || propertyName.Contains("Spread"))
                {
                    propertyName = propertyName.Replace("LeftHand", "Left");
                    propertyName = propertyName.Replace("RightHand", "Left");
                    propertyName = propertyName.Replace(".", " ");
                }

                for (int i = 0; i < MusclesCount; i++)
                {
                    if (propertyName == HumanTrait.MuscleName[i])
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, bind);
                        pose.muscles[i] = curve.Evaluate(time);
                        pose.isValied[i] = true;
                        break;
                    }
                }
            }
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(pose, path);
#endif
        }

        public static PoseAsset GetPoseAssetbyFileName(string name)
        {
            var path = PoseAssetDir + name + ".asset";
            var pose = AssetDatabase.LoadAssetAtPath(path, typeof(PoseAsset)) as PoseAsset;
            if (pose == null)
                Debug.LogError("Cannot find pose asset at" + path);
            return pose;
        }

        public static PoseAsset GetTPoseAsset()
        {
            var pose = AssetDatabase.LoadAssetAtPath(TPoseFilePath, typeof(PoseAsset)) as PoseAsset;
            if (pose == null)
                Debug.LogError("Cannot find t-pose asset");
            return pose;
        }

        public static void ApplyPose(Animator animator, Transform model, PoseAsset pose)
        {
            using (HumanPoseHandler humanPoseHandler = new HumanPoseHandler(animator.avatar, model))
            {
                if (!animator.isHuman)
                {
                    Debug.LogError(animator.gameObject.name + " is not human!");
                    return;
                }

                HumanPose humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);

                //Todo - need Cause Analysis why hip bone is moving…
                var hipTrf = animator.GetBoneTransform(HumanBodyBones.Hips);
                var hipPosition = hipTrf == null ? Vector3.zero : hipTrf.localPosition;
                var hipRotation = hipTrf == null ? Quaternion.identity : hipTrf.localRotation; //anyway, the rotation dose not changed with HumanPoseHandler

                if (pose.isValied == null || pose.isValied.Length <= 0)
                {
                    Debug.LogWarning("Pose asset's isValied field is crashed. set all is true");
                    pose.isValied = new bool[MusclesCount];
                    for (int i = 0; i < MusclesCount; i++)
                        pose.isValied[i] = true;
                }

                for (int i = 0; i < MusclesCount; i++)
                {
                    if (pose.isValied[i])
                        humanPose.muscles[i] = pose.muscles[i];
                }
                for (int i = 0; i < HumanBodyBoneCount; i++)
                {
                    var boneTrf = animator.GetBoneTransform((HumanBodyBones)i);
                    if (boneTrf != null)
                        Undo.RecordObject(boneTrf, "Apply Pose");
                }

                humanPoseHandler.SetHumanPose(ref humanPose);
                hipTrf.transform.localPosition = hipPosition;
                hipTrf.transform.localRotation = hipRotation;
                SceneView.RepaintAll();
            }
        }

        //ToDo
        public static void ExportPose()
        {

        }
    }
}