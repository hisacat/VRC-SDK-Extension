using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRCSDK2;
using VRC.Core;
using System.Reflection;
using System.Linq;
using System;

namespace VRCSDKExtension
{
    public class LogWindow : EditorWindow
    {
        public static void Init(string log)
        {
            LogWindow window = (LogWindow)EditorWindow.GetWindow(typeof(LogWindow), true);
            window.log = log;
            window.Show();
        }

        private string log;
        Vector2 scrollValue;

        private void OnEnable()
        {
            titleContent = new GUIContent(VRChatSDKExtension.ProjectName);
            scrollValue = Vector2.zero;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("VRChat SDK Extension\r\n" + Localization.GetLocalizedString("avatar_helper_copy_avatar_from_new_model_file"));
            GUILayout.Space(4);

            GUILayout.BeginScrollView(scrollValue);
            GUILayout.TextField(log);
            GUILayout.EndScrollView();

            if (GUILayout.Button("Copy to Clipboard"))
                GUIUtility.systemCopyBuffer = log;

            GUILayout.EndVertical();
        }
    }
    public class CopyAvatarFromNewModelFileWindow_Lagacy : EditorWindow
    {
        public static void Init(VRC_AvatarDescriptor avatarObject, List<SkinnedMeshRenderer> avatarSkins, GameObject avatarModel)
        {
            CopyAvatarFromNewModelFileWindow_Lagacy window = (CopyAvatarFromNewModelFileWindow_Lagacy)EditorWindow.GetWindow(typeof(CopyAvatarFromNewModelFileWindow_Lagacy), true);
            window.avatarObject = avatarObject;
            window.avatarSkins = avatarSkins;
            window.Show();
        }

        private VRC_AvatarDescriptor avatarObject;
        private List<SkinnedMeshRenderer> avatarSkins;
        private GameObject selectedModelAsset = null;

        private bool sameNameFoldOut = false;
        private bool traceConsoleLog = false;
        private string consoleLog = "";
        private List<string> consoleLogList = null;
        private int traceConsoleLogCount = 5;

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

            traceConsoleLog = EditorGUILayout.ToggleLeft("Trace Log", traceConsoleLog);
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

                Application.logMessageReceived += HandleLog;
                consoleLog = "AvatarCopyFromNewModelFile Trace Console Log\r\nversion : " + VRChatSDKExtension.versionStr + "\r\n\r\n";
                if (consoleLogList == null)
                    consoleLogList = new List<string>();
                consoleLogList.Clear();

                try
                {
                    CopyAvatar();
                }
                catch { }

                Application.logMessageReceived -= HandleLog;
                if (traceConsoleLog)
                    LogWindow.Init(consoleLog);
            }
            GUI.enabled = true;
        }
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            consoleLogList.Add(type.ToString() + " : " + logString + "\r\n");

            if (type == LogType.Warning || type == LogType.Error)
            {
                var allLogCount = consoleLogList.Count;
                var traceCount = traceConsoleLogCount;
                if (allLogCount <= traceCount)
                    traceCount = allLogCount;

                for (int i = 0; i < traceCount; i++)
                    consoleLog += consoleLogList[allLogCount - (traceCount - i)];
                consoleLog += "------------------------------\r\n";
            }
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

                //add root
                if (AvatarObj == avatarTrf)
                {
                    avatarTrfDic.Add(AvatarObj, newAvatarTrf);
                }
                //create user edited gameobject
                else if (!avatarTrfDic.ContainsKey(AvatarObj))
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
                            #region skip some Fields
                            if (component.GetType() == typeof(UnityEngine.Video.VideoPlayer))
                            {
                                if (field.Name == "prepareCompleted")
                                    continue;
                            }
                            #endregion
                            if (traceConsoleLog)
                                Debug.Log("VRCSDKExtension : Copy field " + field.Name + " in " + component.GetType());
                            value = CreateReferencedValue(value, avatarTrfDic, component.GetType());
                            newField.SetValue(addedcomponent, value);
                        }
                        #endregion

                        #region copy properties
                        var properties = component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        int propertiesCount = properties.Length;

                        for (int propertyIndex = 0; propertyIndex < propertiesCount; propertyIndex++)
                        {
                            var property = properties[propertyIndex];
                            #region skip some properties
                            if (component.GetType() == typeof(Rigidbody))
                            {
                                if (property.Name == "sleepVelocity")
                                    continue;
                                if (property.Name == "sleepAngularVelocity")
                                    continue;
                                if (property.Name == "useConeFriction")
                                    continue;
                            }
                            if (component.GetType() == typeof(MeshFilter))
                            {
                                if (property.Name == "mesh")
                                    continue;
                            }
                            if (component.GetType() == typeof(MeshRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(SkinnedMeshRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(ParticleSystemRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(ParticleRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(LineRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(TrailRenderer))
                            {
                                if (property.Name == "material")
                                    continue;
                                if (property.Name == "materials")
                                    continue;
                            }
                            if (component.GetType() == typeof(Animator))
                            {
                                if (property.Name == "bodyPosition")
                                    continue;
                                if (property.Name == "bodyRotation")
                                    continue;
                                if (property.Name == "playbackTime")
                                    continue;
                            }
                            if (component.GetType() == typeof(UnityEngine.Video.VideoPlayer))
                            {
                                if (property.Name == "url")
                                    continue;
                            }
                            if (component.GetType() == typeof(Camera))
                            {
                                if (property.Name == "layerCullDistances")
                                    continue;
                            }
                            #endregion
                            if (traceConsoleLog)
                                Debug.Log("VRCSDKExtension : Copy property " + property.Name + " in " + component.GetType());
                            var newProperty = addedcomponent.GetType().GetProperty(property.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            if (!property.CanWrite || !property.CanRead)
                                continue;

                            var value = property.GetValue(component, null);
                            value = CreateReferencedValue(value, avatarTrfDic, component.GetType());

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

        private object CreateReferencedValue(object value, Dictionary<Transform, Transform> avatarTrfDic, Type debugtype)
        {
            //if it null
            if (value == null)
                return value;

            //object reference change by transform dictionary.
            if (value is Component)
            {
                var componentField = (value as Component); //It contains Transform
                if (componentField == null)
                    return value;

                if (avatarTrfDic.ContainsKey(componentField.transform))
                    value = avatarTrfDic[componentField.transform].GetComponent(value.GetType());
                return value;
            }
            else if (value is GameObject)
            {
                var gameobjectField = (value as GameObject);
                if (gameobjectField == null)
                    return value;

                if (avatarTrfDic.ContainsKey(gameobjectField.transform))
                    value = avatarTrfDic[gameobjectField.transform].gameObject;
                return value;
            }
            else if (value is Transform)
            {
                var transformField = (value as Transform);
                if (transformField == null)
                    return value;

                if (avatarTrfDic.ContainsKey(transformField))
                    value = avatarTrfDic[transformField].gameObject;
                return value;
            }
            //skip UnityEngine's Components
            else if (value.GetType().Namespace == "UnityEngine")
            {
                return value;
            }

            //if it user-created struct or Class
            if (value.GetType().Assembly.GetName().Name != "mscorlib" &&
           ((value.GetType().IsValueType && !value.GetType().IsPrimitive) ||
            (value.GetType().IsClass && !(value is System.Collections.IList))))
            {
                var cloneValue = Activator.CreateInstance(value.GetType());

                var fields = value.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                int fieldCount = fields.Length;
                for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                {
                    var field = fields[fieldIndex];
                    var filedValue = field.GetValue(value);

                    filedValue = CreateReferencedValue(filedValue, avatarTrfDic, debugtype);

                    field.SetValue(cloneValue, filedValue);
                }
                return cloneValue;
            }

            //if it iList (array, List<T>)
            if (value is System.Collections.IList)
            {
                //Clone iList
                {
                    var enumerableValues = value as System.Collections.IList;

                    //when Array
                    if (value is object[])
                        value = (value as object[]).Clone();
                    //when List<T>
                    else if (value is IList && value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                    {
                        Type elementType = enumerableValues.AsQueryable().ElementType;
                        Type listGenericType = typeof(List<>);
                        Type listType = listGenericType.MakeGenericType(elementType);
                        ConstructorInfo ci = listType.GetConstructor(new Type[] { });
                        var list = ci.Invoke(new object[] { });

                        var resultEnumerableValues = list as System.Collections.IList;
                        foreach (var enumerableValue in enumerableValues)
                            resultEnumerableValues.Add(enumerableValue);

                        value = list;
                    }
                    else //The another iList?
                    {
                        if (traceConsoleLog)
                            Debug.LogError("Unknown IList exists. type : " + value.GetType() + ", element type : " + enumerableValues.AsQueryable().ElementType);
                    }
                }
                //Recursion for Array-Inside object reference change by transform dictionary. 
                {
                    var enumerableValues = value as System.Collections.IList;
                    int count = enumerableValues.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var referencedObj = CreateReferencedValue(enumerableValues[i], avatarTrfDic, debugtype);
                        enumerableValues[i] = referencedObj;
                    }
                }
            }
            return value;
        }

        private static object CheckReferencedValue(object value, Dictionary<Transform, Transform> avatarTrfDic)
        {
            if (value != null)
            {
                if (value is System.Collections.IList)
                {
                    var enumerableValues = value as System.Collections.IList;
                    foreach (var enumerableValue in enumerableValues)
                        CheckReferencedValue(enumerableValue, avatarTrfDic);

                    if (value is object[])
                        return (value as object[]).Clone();

                    //Todo List COPY!!!!
                    if (value is IList && value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                    {
                        //FUCK
                        Type elementType = enumerableValues.AsQueryable().ElementType;
                        Type listGenericType = typeof(List<>);
                        Type listType = listGenericType.MakeGenericType(elementType);
                        ConstructorInfo ci = listType.GetConstructor(new Type[] { });
                        var list = ci.Invoke(new object[] { });

                        var resultEnumerableValues = list as System.Collections.IList;
                        foreach (var enumerableValue in enumerableValues)
                            resultEnumerableValues.Add(enumerableValue);

                        return list;
                        //return (value as List<Rigidbody>).;
                    }
                    else
                    {
                        Debug.Log(enumerableValues.AsQueryable().ElementType);
                        Debug.Log(value.GetType());
                        Debug.Log("Unknown IList exists");
                    }
                }

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