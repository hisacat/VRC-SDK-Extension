using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public static class Localization
    {
        private static List<Dictionary<string, object>> localization = null;

        public static void Init()
        {
            localization = null;
            localization = CSVReader.Read("Localization");
        }

        public static string GetLocalizedString(string id, params object[] arg)
        {
            if (localization == null)
                return id;

            var table = localization.Find(x => (string)x["id"] == id);
            if (table == null)
                return id;
            var local = table[GlobalSettings.Settings.Language.ToString()];
            if (local == null)
                return id;
            var str = local.ToString();
            if (string.IsNullOrEmpty(str))
            {
                var localen = table[Language.En.ToString()];
                if (localen == null)
                    return id;
                return Formatting(localen.ToString(), arg);
            }
            return Formatting(str, arg);
        }

        private static string Formatting(string str, params object[] arg)
        {
            str = str.Replace("<br>", "\r\n");
            try
            {
                return string.Format(str, arg);
            }
            catch
            {
                return str;
            }
        }
    }
}