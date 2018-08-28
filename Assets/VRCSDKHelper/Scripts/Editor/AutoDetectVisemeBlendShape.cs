using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using VRCSDK2;

namespace VRCSDKHelper
{
    public static class AutoDetectVisemeBlendShape
    {
        public static void DoAutoDetectVisemeBlendShape(VRC_AvatarDescriptor avatar)
        {
            Undo.RecordObject(avatar, "Auto Detect Viseme Blend Shape");
            if (avatar.VisemeSkinnedMesh == null)
            {
                if (EditorUtility.DisplayDialog("Auto Detect Viseme Blend Shape", "Face Mesh is not exist, Use Mesh named by \"Body\"?", "Ok", "Cancel"))
                {
                    var body = avatar.transform.Find("Body");
                    if (body != null)
                    {
                        var sm = body.GetComponent<SkinnedMeshRenderer>();
                        if (sm == null)
                        {
                            EditorUtility.DisplayDialog("Auto Detect Viseme Blend Shape", "Body GameObject is not exist", "Ok");
                            return;
                        }

                        avatar.VisemeSkinnedMesh = sm;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Auto Detect Viseme Blend Shape", "There are No Skinned Mesh Renderer in Body GameObject", "Ok");
                        return;
                    }
                }
            }

            if (avatar.VisemeSkinnedMesh == null)
            {
                EditorUtility.DisplayDialog("Auto Detect Viseme Blend Shape", "Please set Face Mesh,", "Ok");
                return;
            }
            
            var blendShapeCount = avatar.VisemeSkinnedMesh.sharedMesh.blendShapeCount;
            string[] blendShapeNames = new string[blendShapeCount];
            for (int i = 0; i < blendShapeCount; i++)
                blendShapeNames[i] = avatar.VisemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);

            int visemeCount = (int)VRC_AvatarDescriptor.Viseme.Count;
            avatar.VisemeBlendShapes = new string[visemeCount];

            int findCount = 0;
            for (int i = 0; i < visemeCount; i++)
            {
                for (int j = 0; j < blendShapeCount; j++)
                {
                    if (blendShapeNames[j].ToLower() == ("vrc.v_" + (VRC_AvatarDescriptor.Viseme)i).ToLower())
                    {
                        avatar.VisemeBlendShapes[i] = blendShapeNames[j];
                        findCount++;
                        continue;
                    }
                }
            }
            EditorUtility.DisplayDialog("Auto Detect Viseme Blend Shape", "Done.\r\nFind " + findCount + " of " + visemeCount + ".", "Ok");
        }
    }
}