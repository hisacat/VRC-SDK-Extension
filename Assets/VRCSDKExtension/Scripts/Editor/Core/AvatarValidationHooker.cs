using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using System.Reflection;
using System.IO;
using System.Linq;

namespace VRCSDKExtension
{
    //or just use tag & layers? or, scriptableasset.


    public static class AvatarValidationHooker
    {
        public static readonly string[] ExtensionWhiteList = new string[]
        {
            "A",
            "B",
            "C"
        };

        public static void Hook()
        {
            AssetDatabase.Refresh();
            /*
            AssetDatabase.LoadAssetAtPath<TextAsset>(""));
            var assets = AssetDatabase.FindAssets("AvatarValidation", new string[] { "Assets/VRCSDK" });
            if (assets.)
            {
                Debug.Log("Cant found AvatarValidation.cs");
            }else
            {
            }
            */
            /*
            Debug.Log(test.Length);

            //we know path.
            //hook AvatarValidation
            DirectoryInfo dir = new DirectoryInfo(typeof(AvatarValidation).Assembly.Location).Parent.Parent.Parent;
            dir = new System.IO.DirectoryInfo(dir.ToString() + @"\Assets\VRCSDK\Dependencies\VRChat\Scripts\AvatarValidation.cs");

            TextReader reader = new System.IO.StreamReader(dir.ToString());
            string code = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();

            string hookCheckText = "//VRCSDKExtension";
            if (!code.Contains(hookCheckText))
            {
                string targetText = "\"RealisticEyeMovements.EyeAndHeadAnimator\"";
                int insertIndex = code.IndexOf(targetText) + targetText.Length;
                string insertText = ",\r\n" + hookCheckText;

                string[] ExtWhiteList = { "A", "B", "C" };
                foreach (string extWhiteList in ExtWhiteList)
                    insertText += "\r\n" + "\"" + extWhiteList + "\",";

                code = code.Insert(insertIndex, insertText);
            }

            TextWriter writer = new StreamWriter(dir.ToString());
            writer.Write(code);
            writer.Close();
            writer.Dispose();

            Debug.Log(code);
            */
        }
    }
}