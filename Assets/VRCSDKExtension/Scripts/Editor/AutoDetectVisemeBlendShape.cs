using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using VRCSDK2;

namespace VRCSDKExtension
{
    public static class AutoDetectVisemeBlendShape
    {
        public static void DoAutoDetectVisemeBlendShape(VRC_AvatarDescriptor avatar)
        {
            Undo.RecordObject(avatar, "Auto Detect Viseme Blend Shape");
            if (avatar.VisemeSkinnedMesh == null)
            {
                if (EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape"),
                    Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape_dialog_face_mesh_not_exits"),
                    "Ok", "Cancel"))
                {
                    var body = avatar.transform.Find("Body");
                    if (body != null)
                    {
                        var sm = body.GetComponent<SkinnedMeshRenderer>();
                        if (sm == null)
                        {
                            EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape"),
                                Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape_dialog_body_object_not_exits"),
                                "Ok");
                            return;
                        }

                        avatar.VisemeSkinnedMesh = sm;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape"),
                            Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape_dialog_skinned_mesh_renderer_not_exits"),
                            "Ok");
                        return;
                    }
                }
            }

            if (avatar.VisemeSkinnedMesh == null)
            {
                EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape"),
                    Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape_dialog_please_set_face_mesh"),
                    "Ok");
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

            //vrc.v_e & vrc.v_ee (is lagacy?)
            var visume_E_index = (int)VRC_AvatarDescriptor.Viseme.E;
            var visume_E_lagacy = "vrc.v_ee";
            if (string.IsNullOrEmpty(avatar.VisemeBlendShapes[visume_E_index]))
            {
                for (int j = 0; j < blendShapeCount; j++)
                {
                    if (blendShapeNames[j].ToLower() == visume_E_lagacy)
                    {
                        avatar.VisemeBlendShapes[visume_E_index] = blendShapeNames[j];
                        findCount++;
                        break;
                    }
                }
            }

            EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_detect_viseme_blend_shape")
                , Localization.GetLocalizedString("global_done") +
                "\r\nFind " + findCount + " of " + visemeCount + ".", "Ok");
        }
    }
}