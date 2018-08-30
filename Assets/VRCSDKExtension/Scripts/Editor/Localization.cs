using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRChatSDKExtension
{
    public static class Localization
    {
        private static List<Dictionary<string, object>> localization = null;

        public static void Init()
        {
            localization = null;
            localization = CSVReader.Read("Localization");
        }

        public static string GetLocalizedString(string id)
        {
            var table = localization.Find(x => (string)x["id"] == id);
            if (table == null)
                return id;
            var local = table[VRChatSDKExtension.language.ToString()];
            if (local == null)
                return id;
            var str = local.ToString();
            if (string.IsNullOrEmpty(str))
            {
                var localen = table[Language.En.ToString()];
                if (localen == null)
                    return id;
                return Formatting(localen.ToString());
            }
            return Formatting(str);
        }

        private static string Formatting(string str)
        {
            str = str.Replace("<br>", "\r\n");
            return str;
        }
    }
}