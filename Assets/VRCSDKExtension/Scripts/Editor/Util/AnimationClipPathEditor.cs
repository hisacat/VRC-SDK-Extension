using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCSDKExtension
{
    public class AnimationClipPathEditor : EditorWindow
    {
        [MenuItem("Assets/AnimationClip Path Edit", true)]
        public static bool OpenWithAnimationClipMenuValidation()
        {
            return Selection.activeObject is AnimationClip;
        }
        [MenuItem("Assets/AnimationClip Path Edit")]
        private static void OpenWithAnimationClipMenu()
        {
            Init(Selection.activeObject as AnimationClip);
        }

        public static void Init(AnimationClip clip)
        {
            AnimationClipPathEditor window = (AnimationClipPathEditor)EditorWindow.GetWindow(typeof(AnimationClipPathEditor), true);
            window.clip = clip;
            window.GetAllBindlings();
            window.Show();
        }

        private class PathExistsBinding
        {
            private AnimationClip clip;
            private AnimationClipCurveData binding;
            public string Path { get { return binding.path; } }

            public readonly System.Type type;
            public readonly string propertyName;
            public string editPath;

            public string shownTypeName;
            public string shownPropertyName;
            public string shownObjectName;

            public void ResetEditPath()
            {
                editPath = binding.path;
            }
            public void RemoveProperty()
            {
                //var obj = Selection.activeObject;
                //Selection.activeObject = null;

                //ToDo. fix "MecanimDataWasBuilt()"; << when animator window's open (right - bot preview window is playing)
                clip.SetCurve(binding.path, binding.type, binding.propertyName, null);

                SceneView.RepaintAll();
                //Selection.activeObject = obj;
            }
            public void SavePath()
            {
                //var obj = Selection.activeObject;
                //Selection.activeObject = null;

                RemoveProperty();
                binding.path = editPath;
                
                //ToDo. fix "MecanimDataWasBuilt()"; << when animator window's open (right - bot preview window is playing)
                clip.SetCurve(binding.path, binding.type, binding.propertyName, binding.curve);

                SceneView.RepaintAll();
                //Selection.activeObject = obj;
            }

            public PathExistsBinding(AnimationClip clip, AnimationClipCurveData binding)
            {
                this.clip = clip;
                this.binding = binding;

                this.type = binding.type;
                this.propertyName = binding.propertyName;
                this.editPath = binding.path;

                string[] splitTemp = null;

                splitTemp = binding.type.ToString().Split('.');
                this.shownTypeName = splitTemp[splitTemp.Length - 1];

                var pName = binding.propertyName;
                if (pName.Length >= 2 && pName[1] == '_')
                    pName = pName.Remove(0, 2);

                this.shownPropertyName = pName;
                splitTemp = binding.path.Split('/');

                this.shownObjectName = splitTemp[splitTemp.Length - 1];
            }
        }
        private AnimationClip clip = null;
        private Vector2 scrollValue;
        private List<PathExistsBinding> bindings;

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);

            var minSize = this.minSize;
            minSize.x = 300;
            this.minSize = minSize;

            var pos = this.position;
            pos.width = 400;
            this.position = pos;

            this.scrollValue = Vector2.zero;
        }

        private void GetAllBindlings()
        {
            if (bindings == null)
                bindings = new List<PathExistsBinding>();
            bindings.Clear();

            if (clip == null)
                return;

            var allbindlings = AnimationUtility.GetAllCurves(clip, true);

            int bindCount = allbindlings.Length;

            foreach (var bind in allbindlings)
            {
                if (string.IsNullOrEmpty(bind.path))
                    continue;
                bindings.Add(new PathExistsBinding(clip, bind));
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("util_animationclip_path_editor"));
                GUILayout.Space(4);

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Localization.GetLocalizedString("global_animationclip"));
                        GUI.enabled = false;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false);
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                //bindings
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    scrollValue = GUILayout.BeginScrollView(scrollValue);
                    {
                        int bindingCount = bindings == null ? 0 : bindings.Count;
                        for (int i = 0; i < bindingCount; i++)
                        {
                            var binding = bindings[i];

                            //Name
                            GUILayout.Label(binding.shownObjectName);
                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                GUILayout.Label(binding.shownTypeName + "." + binding.shownPropertyName);

                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("Path : ", binding.editPath == binding.Path ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.Width(40));
                                    binding.editPath = GUILayout.TextField(binding.editPath, GUILayout.Width(this.position.width - 75));
                                }
                                GUILayout.EndHorizontal();

                                if (GUILayout.Button("Remove"))
                                {
                                    binding.RemoveProperty();
                                    GetAllBindlings();
                                    break;
                                }
                                if (GUILayout.Button("Reset"))
                                    binding.ResetEditPath();
                                if (GUILayout.Button("Save"))
                                    binding.SavePath();
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button("Reset All", GUILayout.Height(30)))
                {
                    int bindingCount = bindings.Count;
                    for (int i = 0; i < bindingCount; i++)
                        bindings[i].ResetEditPath();

                    GetAllBindlings();
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Save All", GUILayout.Height(30)))
                {
                    int bindingCount = bindings.Count;
                    for (int i = 0; i < bindingCount; i++)
                        bindings[i].SavePath();

                    GetAllBindlings();
                }
            }
            GUILayout.EndVertical();
        }

        private void PlaymodeStateChanged()
        {
            if (Application.isPlaying)
            {
                if (this != null)
                    this.Close();
            }
        }
    }
}