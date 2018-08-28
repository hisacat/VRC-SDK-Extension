using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRCSDKHelper
{
    public class EyeTrackingPoint : MonoBehaviour
    {
        [SerializeField]
        private Transform scaleAnchor;

        private Transform leftEye;
        private Transform rightEye;

        public void SetEyes(Transform leftEye, Transform rightEye)
        {
            this.leftEye = leftEye;
            this.rightEye = rightEye;
        }
        public void SetScale(float scale)
        {
            scaleAnchor.transform.localScale = new Vector3(scale, scale, scale);
        }
        private void Update()
        {
            if (leftEye != null)
                leftEye.LookAt(transform.position);
            if (rightEye != null)
                rightEye.LookAt(transform.position);
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.05f);
        }
    }
}