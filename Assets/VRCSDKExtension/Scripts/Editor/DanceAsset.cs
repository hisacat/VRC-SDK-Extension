using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    //this is on dev
    //need check why ONSPAudioSource auto added when add audiosource in scene
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
        //Todo ONSPAudioSource managed?

        public static void CreateDanceAsset(string path, string name, AnimationClip animation, AudioClip music, ONSPAudioSource onsp)
        {

        }
    }

    public class CreateDanceAssetWindow : EditorWindow
    {
        //[MenuItem("Assets/Create/VRC Dance")]
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
                    #region Animation, Music, Name set fields
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
                    #endregion

                    GUI.enabled = !string.IsNullOrEmpty(danceName);
                    if (GUILayout.Button("Create"))
                    {
                        foreach (var illegalCharacter in illegalCharacters)
                            if (danceName != null)
                                danceName = danceName.Replace(illegalCharacter, "");

                        var prefabPath = path +"/" + danceName + ".prefab";

                        GameObject danceSoundObj = new GameObject(danceName);
                        var audioSource = danceSoundObj.AddComponent<AudioSource>();
                        audioSource.clip = music;

                        var Prefab = PrefabUtility.CreatePrefab(prefabPath,danceSoundObj,ReplacePrefabOptions.ConnectToPrefab);
                        GameObject.DestroyImmediate(danceSoundObj);
                        
                        this.Close();
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
    }
}