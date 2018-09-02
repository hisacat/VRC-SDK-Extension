using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using System.Reflection;

namespace VRCSDKExtension
{
    public class CopyAvatarFromNewModelFileWindow : EditorWindow
    {
        public static void Init(VRC_AvatarDescriptor avatarObject, List<SkinnedMeshRenderer> avatarSkins, GameObject avatarModel)
        {
            CopyAvatarFromNewModelFileWindow window = (CopyAvatarFromNewModelFileWindow)EditorWindow.GetWindow(typeof(CopyAvatarFromNewModelFileWindow), true);
            window.avatarObject = avatarObject;
            window.avatarSkins = avatarSkins;
            window.Show();
        }

        private VRC_AvatarDescriptor avatarObject;
        private List<SkinnedMeshRenderer> avatarSkins;
        private GameObject selectedModelAsset = null;

        private bool sameNameFoldOut = false;

        private void OnEnable()
        {
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);
            var minSize = this.minSize;
            var maxSize = this.maxSize;
            minSize.x = 400;
            maxSize.x = 400;
            this.minSize = minSize;
            this.maxSize = maxSize;
            selectedModelAsset = null;
        }

        private void OnGUI()
        {
            if (avatarObject == null)
            {
                this.Close();
                return;
            }

            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"));
            GUILayout.Space(4);

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.GetLocalizedString("global_avatar"));
            GUI.enabled = false;
            GUILayout.FlexibleSpace();
            EditorGUILayout.ObjectField(avatarObject, typeof(VRC_AvatarDescriptor), true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_new_model"));
            GUILayout.FlexibleSpace();
            //ToDo : only selectable model(fbx, blender, etc…)
            selectedModelAsset = EditorGUILayout.ObjectField(selectedModelAsset, typeof(GameObject), false) as GameObject;
            GUILayout.EndHorizontal();

            //check avatar's Gameobject has same name.
            List<TwoTransform> sames = new List<TwoTransform>();
            GetSameNamesTransform(avatarObject.transform, ref sames);
            if (sames.Count > 0)
            {
                GUIStyle box = new GUIStyle(GUI.skin.box);
                box.margin = new RectOffset(10, 10, 10, 10);
                GUILayout.BeginVertical(box);
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_warnning_same_gameobject_name"), MessageType.Warning);
                GUILayout.Space(5);
                sameNameFoldOut = EditorGUILayout.Foldout(sameNameFoldOut, Localization.GetLocalizedString("global_list"), true);
                if (sameNameFoldOut)
                {
                    GUI.enabled = false;
                    GUILayout.BeginVertical();
                    for (int i = 0; i < sames.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(sames[i].A, typeof(GameObject), true);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.ObjectField(sames[i].B, typeof(GameObject), true);
                        GUILayout.EndHorizontal();
                    }
                    GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.Space(5);
                if (GUILayout.Button(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_same_same_fix_with_randomize")))
                {
                    if (EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"),
                        Localization.GetLocalizedString("global_are_you_sure?"),
                        "Yes", "No"))
                    {
                        for (int i = 0; i < sames.Count; i++)
                        {
                            var target = sames[i].B;
                            Undo.RecordObject(target.gameObject, "Fix same name");
                            target.name = target.name + "_VRCSDKEXT_" + target.GetInstanceID().ToString();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_info_no_same_gameobject_name"), MessageType.Info);
            }
            if (selectedModelAsset == null)
                EditorGUILayout.HelpBox(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_error_no_model_file"), MessageType.Error);

            GUILayout.EndVertical();

            GUI.enabled = (avatarObject != null && selectedModelAsset != null && sames.Count <= 0);
            if (GUILayout.Button(Localization.GetLocalizedString("global_create"), GUILayout.Height(30)))
            {
                //check selectedModelAsset is model file
                {
                    var avatarSkins = new List<SkinnedMeshRenderer>();
                    int childCount = selectedModelAsset.transform.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        var obj = selectedModelAsset.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                        if (obj != null)
                            avatarSkins.Add(obj);
                    }

                    if (avatarSkins.Count <= 0)
                    {
                        EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"),
                            Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_warnning_not_model_file", selectedModelAsset.name),
                            "Ok");
                        return;
                    }
                }
                //check model's Gameobject has same name.
                {
                    List<TwoTransform> modelSames = new List<TwoTransform>();
                    GetSameNamesTransform(selectedModelAsset.transform, ref sames);
                    if (modelSames.Count > 0)
                    {
                        EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"),
                            Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file_warnning_same_gameobject_name_in_model_file", selectedModelAsset.name),
                            "Ok");
                        return;
                    }
                }
                CopyAvatar();
            }
            GUI.enabled = true;
        }

        private void CopyAvatar()
        {
            var avatarTrf = avatarObject.transform;
            var newAvatarTrf = (GameObject.Instantiate(selectedModelAsset) as GameObject).transform;

            newAvatarTrf.name = selectedModelAsset.name;

            var newAvatarSkins = new List<SkinnedMeshRenderer>();
            int childCount = newAvatarTrf.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var obj = newAvatarTrf.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (obj != null)
                    newAvatarSkins.Add(obj);
            }

            List<Transform> avatarObjects = new List<Transform>();
            List<Transform> newAvatarObjects = new List<Transform>();
            GetAllChild(avatarTrf, ref avatarObjects);
            GetAllChild(newAvatarTrf, ref newAvatarObjects);
            int avatarObjCount = avatarObjects.Count;
            int newAvatarObjCount = newAvatarObjects.Count;

            //detach parent.
            int count = newAvatarObjects.Count;
            for (int i = 0; i < count; i++)
            {
                if (newAvatarObjects[i].transform != newAvatarTrf)
                    newAvatarObjects[i].transform.parent = newAvatarTrf;
            }

            #region Make Dictionary pair & create user edited gameobjects
            Dictionary<Transform, Transform> avatarTrfDic = new Dictionary<Transform, Transform>();
            for (int i = 0; i < avatarObjCount; i++)
            {
                var AvatarObj = avatarObjects[i];
                for (int j = 0; j < newAvatarObjCount; j++)
                {
                    var newAvatarObj = newAvatarObjects[j];
                    if (AvatarObj.name == newAvatarObj.name)
                    {
                        newAvatarObj.gameObject.SetActive(AvatarObj.gameObject.activeSelf);
                        newAvatarObj.gameObject.layer = AvatarObj.gameObject.layer;
                        newAvatarObj.gameObject.tag = AvatarObj.gameObject.tag;
                        avatarTrfDic.Add(AvatarObj, newAvatarObj);
                        break;
                    }
                }
                //create user edited gameobject
                if (!avatarTrfDic.ContainsKey(AvatarObj))
                {
                    var newAvatarObj = new GameObject(AvatarObj.name).transform;
                    newAvatarObj.parent = newAvatarTrf;
                    newAvatarObj.gameObject.SetActive(AvatarObj.gameObject.activeSelf);
                    newAvatarObj.gameObject.layer = AvatarObj.gameObject.layer;
                    newAvatarObj.gameObject.tag = AvatarObj.gameObject.tag;
                    avatarTrfDic.Add(AvatarObj, newAvatarObj);
                }
            }
            if (avatarTrfDic.ContainsKey(avatarTrf))
                avatarTrfDic.Remove(avatarTrf);
            newAvatarTrf.gameObject.SetActive(avatarTrf.gameObject.activeSelf);
            newAvatarTrf.gameObject.layer = avatarTrf.gameObject.layer;
            newAvatarTrf.gameObject.tag = avatarTrf.gameObject.tag;
            avatarTrfDic.Add(avatarTrf, newAvatarTrf);
            #endregion

            #region set parent & copy position, rotation, scale
            for (int i = 0; i < avatarObjCount; i++)
            {
                var obj = avatarObjects[i];
                if (obj == avatarTrf)
                    continue;

                if (avatarTrfDic.ContainsKey(obj))
                {
                    avatarTrfDic[obj].parent = avatarTrfDic[obj.parent];
                    avatarTrfDic[obj].localPosition = obj.localPosition;
                    avatarTrfDic[obj].localRotation = obj.localRotation;
                    avatarTrfDic[obj].localScale = obj.localScale;
                }
            }
            #endregion

            List<Component> newAddedComponents = new List<Component>();
            #region add components
            for (int avatarObjIndex = 0; avatarObjIndex < avatarObjCount; avatarObjIndex++)
            {
                var AvatarObj = avatarObjects[avatarObjIndex];
                var components = AvatarObj.GetComponents(typeof(Component));
                int componentsCount = components.Length;

                for (int componentIndex = 0; componentIndex < componentsCount; componentIndex++)
                {
                    var component = components[componentIndex];

                    //skip missing component
                    if (component == null)
                        continue;

                    if (component.GetType() == typeof(Transform) ||
                        component.GetType() == typeof(PipelineManager))
                        continue;

                    var addedcomponent = avatarTrfDic[AvatarObj].gameObject.GetComponent(component.GetType());
                    if (addedcomponent == null)
                    {
                        addedcomponent = avatarTrfDic[AvatarObj].gameObject.AddComponent(component.GetType());
                        newAddedComponents.Add(addedcomponent);
                    }
                }
            }
            #endregion

            #region copy components field & properties
            for (int avatarObjIndex = 0; avatarObjIndex < avatarObjCount; avatarObjIndex++)
            {
                var AvatarObj = avatarObjects[avatarObjIndex];
                var components = AvatarObj.GetComponents(typeof(Component));

                int componentsCount = components.Length;
                for (int componentIndex = 0; componentIndex < componentsCount; componentIndex++)
                {
                    var component = components[componentIndex];

                    //skip missing component
                    if (component == null)
                        continue;

                    if (component.GetType() == typeof(Transform) ||
                        component.GetType() == typeof(PipelineManager))
                        continue;

                    var addedcomponent = avatarTrfDic[AvatarObj].gameObject.GetComponent(component.GetType());
                    if (addedcomponent == null)
                    {
                        Debug.Log(component.name + "missing");
                    }

                    //manage root's Animator
                    if (AvatarObj == avatarTrf && component.GetType() == typeof(Animator))
                    {
                        #region copy Animator fields
                        var animator = component as Animator;
                        var addedAnimator = addedcomponent as Animator;
                        addedAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
                        addedAnimator.applyRootMotion = animator.applyRootMotion;
                        addedAnimator.updateMode = animator.updateMode;
                        addedAnimator.cullingMode = animator.cullingMode;
                        #endregion
                    }

                    else
                    {
                        #region copy fields
                        var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        int fieldCount = fields.Length;

                        for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                        {
                            var field = fields[fieldIndex];
                            var newField = addedcomponent.GetType().GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            var value = field.GetValue(component);
                            value = CheckReferencedValue(value, avatarTrfDic);
                            newField.SetValue(addedcomponent, value);
                        }
                        #endregion

                        #region copy properties
                        var properties = component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        int propertiesCount = properties.Length;

                        for (int propertyIndex = 0; propertyIndex < propertiesCount; propertyIndex++)
                        {
                            var property = properties[propertyIndex];
                            var newProperty = addedcomponent.GetType().GetProperty(property.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            if (!property.CanWrite || !property.CanRead)
                                continue;

                            #region skip some properties
                            if (property.ReflectedType == typeof(Rigidbody))
                            {
                                if (property.Name == "sleepVelocity")
                                    continue;
                                if (property.Name == "sleepAngularVelocity")
                                    continue;
                                if (property.Name == "useConeFriction")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(MeshFilter))
                            {
                                if (property.Name == "mesh")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(MeshRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(SkinnedMeshRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(ParticleSystemRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(ParticleRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(LineRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(TrailRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (property.ReflectedType == typeof(Animator))
                            {
                                if (property.Name == "bodyPosition")
                                    continue;
                                if (property.Name == "bodyRotation")
                                    continue;
                                if (property.Name == "playbackTime")
                                    continue;
                            }
                            #endregion

                            var value = property.GetValue(component, null);
                            value = CheckReferencedValue(value, avatarTrfDic);

                            newProperty.SetValue(addedcomponent, value, null);
                        }
                        #endregion
                    }
                }
            }
            #endregion

            EditorUtility.DisplayDialog(Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"),
            Localization.GetLocalizedString("global_done"),
            "Ok");
        }

        private static object CheckReferencedValue(object value, Dictionary<Transform, Transform> avatarTrfDic)
        {
            if (value != null)
            {
                //Debug.Log(value is object[]);

                var componentField = (value as Component);
                if (componentField != null)
                {
                    if (avatarTrfDic.ContainsKey(componentField.transform))
                        value = avatarTrfDic[componentField.transform].GetComponent(value.GetType());
                    return value;
                }
                var gameobjectField = (value as GameObject);
                if (gameobjectField != null)
                {
                    if (avatarTrfDic.ContainsKey(gameobjectField.transform))
                        value = avatarTrfDic[gameobjectField.transform].gameObject;
                    return value;
                }
                var transformField = (value as Transform);
                if (transformField != null)
                {
                    if (avatarTrfDic.ContainsKey(transformField.transform))
                        value = avatarTrfDic[transformField.transform].transform;
                    return value;
                }

                var componentArrayField = (value as Component[]);
                if (componentArrayField != null)
                {
                    int count = componentArrayField.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (avatarTrfDic.ContainsKey((value as Component[])[i].transform))
                            (value as Component[])[i] = avatarTrfDic[(value as Component[])[i].transform].GetComponent((value as Component[])[i].GetType());
                    }
                    return value;
                }
                var gameobjectArrayField = (value as GameObject[]);
                if (gameobjectArrayField != null)
                {
                    int count = gameobjectArrayField.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (avatarTrfDic.ContainsKey((value as GameObject[])[i].transform))
                            (value as GameObject[])[i] = avatarTrfDic[(value as GameObject[])[i].transform].gameObject;
                    }
                    return value;
                }
                var transformArrayField = (value as Transform[]);
                if (transformArrayField != null)
                {
                    int count = transformArrayField.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (avatarTrfDic.ContainsKey((value as Transform[])[i]))
                            (value as Transform[])[i] = avatarTrfDic[(value as Transform[])[i]].transform;
                    }
                    return value;
                }
            }
            return value;
        }

        private struct HierarchyTransform
        {
            public Transform transform;
            public List<HierarchyTransform> children;

            public HierarchyTransform(Transform copyTarget)
            {
                this.transform = copyTarget;
                this.children = new List<HierarchyTransform>();
                int childCount = copyTarget.childCount;
                for (int i = 0; i < childCount; i++)
                    children.Add(new HierarchyTransform(copyTarget.GetChild(i)));
            }
        }

        private struct TwoTransform
        {
            public Transform A;
            public Transform B;
            public TwoTransform(Transform A, Transform B)
            {
                this.A = A;
                this.B = B;
            }
        }

        private void GetSameNamesTransform(Transform trf, ref List<TwoTransform> sames, Dictionary<string, Transform> dic = null)
        {
            if (dic == null)
                dic = new Dictionary<string, Transform>();

            if (dic.ContainsKey(trf.name))
                sames.Add(new TwoTransform(dic[trf.name], trf));
            else
            {
                dic.Add(trf.name, trf);
                int childCount = trf.childCount;
                for (int i = 0; i < childCount; i++)
                    GetSameNamesTransform(trf.GetChild(i), ref sames, dic);
            }
        }

        private void GetAllChild(Transform target, ref List<Transform> childs)
        {
            childs.Add(target);
            int childCount = target.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = target.GetChild(i);
                GetAllChild(child, ref childs);
            }
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