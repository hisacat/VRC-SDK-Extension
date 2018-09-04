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

            private bool isTransformBinding = false;

            public void ResetEditPath()
            {
                editPath = binding.path;
            }
            public void RemoveProperty()
            {
                //ToDo. fix "MecanimDataWasBuilt()"; << when animator window's open (right - bot preview window is playing)
                if (isTransformBinding)
                {
                    if (binding.propertyName == "m_LocalPosition.x")
                        clip.SetCurve(binding.path, binding.type, "m_LocalPosition", null);
                    if (binding.propertyName == "localEulerAnglesRaw.x")
                        clip.SetCurve(binding.path, binding.type, "m_LocalEuler", null);
                    if (binding.propertyName == "m_LocalScale.x")
                        clip.SetCurve(binding.path, binding.type, "m_LocalScale", null);
                }
                else
                {
                    clip.SetCurve(binding.path, binding.type, binding.propertyName, null);
                }
                SceneView.RepaintAll();
            }
            public void ApplyPath()
            {
                if (isTransformBinding)
                {
                    if (binding.propertyName != "m_LocalPosition.x" &&
                        binding.propertyName != "localEulerAnglesRaw.x" &&
                        binding.propertyName != "m_LocalScale.x")
                    {
                        return;
                    }
                    AnimationClipCurveData xBinding = binding;
                    AnimationClipCurveData yBinding = null;
                    AnimationClipCurveData zBinding = null;
                    
                    var allbindlings = AnimationUtility.GetAllCurves(clip, true);
                    int bindCount = allbindlings.Length;
                    foreach (var bind in allbindlings)
                    {
                        if (bind == binding)
                            continue;
                        if (bind.type != typeof(Transform))
                            continue;
                        if (bind.path != binding.path)
                            continue;

                        switch(binding.propertyName)
                        {
                            case "m_LocalPosition.x":
                                if (bind.propertyName == "m_LocalPosition.y")
                                    yBinding = bind;
                                if (bind.propertyName == "m_LocalPosition.z")
                                    zBinding = bind;
                                break;
                            case "localEulerAnglesRaw.x":
                                if (bind.propertyName == "localEulerAnglesRaw.y")
                                    yBinding = bind;
                                if (bind.propertyName == "localEulerAnglesRaw.z")
                                    zBinding = bind;
                                break;
                            case "m_LocalScale.x":
                                if (bind.propertyName == "m_LocalScale.y")
                                    yBinding = bind;
                                if (bind.propertyName == "m_LocalScale.z")
                                    zBinding = bind;
                                break;
                        }
                    }

                    if (xBinding == null || yBinding == null || zBinding == null)
                        return;

                    RemoveProperty();
                    xBinding.path = editPath;
                    yBinding.path = editPath;
                    zBinding.path = editPath;

                    clip.SetCurve(xBinding.path, xBinding.type, xBinding.propertyName, xBinding.curve);
                    clip.SetCurve(yBinding.path, yBinding.type, yBinding.propertyName, yBinding.curve);
                    clip.SetCurve(zBinding.path, zBinding.type, zBinding.propertyName, zBinding.curve);
                }
                else
                {
                    RemoveProperty();
                    binding.path = editPath;

                    //ToDo. fix "MecanimDataWasBuilt()"; << when animator window's open (right - bot preview window is playing)
                    clip.SetCurve(binding.path, binding.type, binding.propertyName, binding.curve);
                }
                SceneView.RepaintAll();
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

                if (binding.type == typeof(Transform))
                {
                    this.isTransformBinding = true;
                    if (binding.propertyName == "m_LocalPosition.x")
                        this.shownPropertyName = "Position";
                    if (binding.propertyName == "localEulerAnglesRaw.x")
                        this.shownPropertyName = "Rotation";
                    if (binding.propertyName == "m_LocalScale.x")
                        this.shownPropertyName = "Scale";
                }

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

                if (bind.type == typeof(Transform))
                {
                    if (bind.propertyName == "m_LocalPosition.y" ||
                        bind.propertyName == "m_LocalPosition.z")
                        continue;
                    if (bind.propertyName == "localEulerAnglesRaw.y" ||
                        bind.propertyName == "localEulerAnglesRaw.z")
                        continue;
                    if (bind.propertyName == "m_LocalScale.y" ||
                        bind.propertyName == "m_LocalScale.z")
                        continue;
                }
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
                                if (GUILayout.Button("Apply"))
                                {
                                    binding.ApplyPath();
                                    GetAllBindlings();
                                    break;
                                }
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
                if (GUILayout.Button("Apply All", GUILayout.Height(30)))
                {
                    int bindingCount = bindings.Count;
                    for (int i = 0; i < bindingCount; i++)
                        bindings[i].ApplyPath();

                    GetAllBindlings();
                }
                GUILayout.Space(5);
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