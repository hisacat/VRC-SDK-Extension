using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public class DanceAsset : ScriptableObject
    {
        [SerializeField]
        private string danceName = ""; // animation clips name.

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private AnimationClip animation;
        [SerializeField]
        private AudioClip music;
        [SerializeField]
        private ONSPAudioSource onsp;

        public static void CreateDanceAsset(string path, string name, AnimationClip animation, AudioClip music, ONSPAudioSource onsp)
        {

        }
    }

    public class CreateDanceAssetWindow : EditorWindow
    {
        [MenuItem("Assets/Create/VRC Dance")]
        private static void CreateVRCDanceAsset()
        {
            var path = SelectionExtension.GetSelectedProjectPath();
            Init(path);
        }

        private static void Init(string path)
        {
            CreateDanceAssetWindow window = (CreateDanceAssetWindow)EditorWindow.GetWindow(typeof(CreateDanceAssetWindow), true);
            window.path = path;
            window.Show();
        }

        private void OnEnable()
        {
        }

        private string path = null;
        private AnimationClip animation;
        private AudioClip music;
        private string danceName = null;

        private string[] illegalCharacters = { @"/", @"\", @":", @"*", @"?", "\"", @"<", @">", @"|" };

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("util_create_vrcdance"));
                GUILayout.Space(4);

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Animation : ", GUILayout.Width(100));
                    animation = EditorGUILayout.ObjectField(animation, typeof(AnimationClip), false) as AnimationClip;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Music : ", GUILayout.Width(100));
                    music = EditorGUILayout.ObjectField(music, typeof(AudioClip), false) as AudioClip;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name : ", GUILayout.Width(100));
                    EditorGUI.BeginChangeCheck();
                    danceName = EditorGUILayout.TextField(danceName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var illegalCharacter in illegalCharacters)
                            if (danceName != null)
                                danceName = danceName.Replace(illegalCharacter, "");
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Create"))
                    {
                        foreach (var illegalCharacter in illegalCharacters)
                            if (danceName != null)
                                danceName = danceName.Replace(illegalCharacter, "");

                        //create prefab ? or... 어떻게매니징하지.
                        //프리팹은 꼭 만들지 않아도, 추가삭제를 해주면 돼.
                        //씬에 두기엔, 캐릭터가 프리팹이어서 다른곳에도 존재하면???
                        //오버라이드가 공유될시엔 어떡해. 씨발 옝외 존나많아.

                        //DanceAsset.CreateDanceAsset()

                        this.Close();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
    }
}