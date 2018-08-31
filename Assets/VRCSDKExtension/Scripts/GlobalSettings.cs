using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace VRCSDKExtension
{
    public enum Language : int
    {
        En = 0,
        Ko = 1,
        Ja = 2,
    }

    public class GlobalSettings : ScriptableObject
    {
        public const string SettingFilePath = "Assets/VRCSDKExtension/Resources/settings.asset";

        [SerializeField]
        private Language language;
        public Language Language { get { return language; } }
        public void SetLanguage(Language language)
        {
            this.language = language;
        }

        private GlobalSettings() { }

        public static GlobalSettings Settings
        {
            get
            {
#if UNITY_EDITOR
                var settings = AssetDatabase.LoadAssetAtPath(SettingFilePath, typeof(GlobalSettings)) as GlobalSettings;
                if (settings == null)
                {
                    settings = new GlobalSettings();
                    AssetDatabase.CreateAsset(settings, SettingFilePath);
                }
                return settings;
#else
                return new GlobalSettings();
#endif
            }
        }
    }
}