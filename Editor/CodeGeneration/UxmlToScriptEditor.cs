using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    [CustomEditor(typeof(UxmlToScript))]
    public class UxmlToScriptEditor : UnityEditor.Editor
    {
        private const float ButtonHeight = 30;
        private const float ButtonSpacing = 10;

        private UxmlToScript _reader;

        public override VisualElement CreateInspectorGUI()
        {
            _reader = (UxmlToScript)target;

            var root = new VisualElement();
            root.Add(CreateUxmlConfigSection());
            root.Add(CreateMakingScriptSection());
            root.Add(CreateInstanceSection());
            root.Add(CreateExecuteSection());
            return root;
        }

        private Foldout CreateUxmlConfigSection()
        {
            var foldout = CreateSectionFoldout("Uxml Config");
            foldout.Add(new PropertyField(serializedObject.FindProperty("uxml"), "Uxml Asset"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("uxmlPath"), "Uxml Asset Path"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("panelSettings"), "Panel Setting Asset"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("sortOrder"), "Sort Order"));
            return foldout;
        }

        private Foldout CreateMakingScriptSection()
        {
            var foldout = CreateSectionFoldout("Making Script Config");
            foldout.Add(new PropertyField(serializedObject.FindProperty("modelScriptName"), "Model Script Name"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("controllerScriptName"), "Controller Script Name"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("savePath"), "To Save Path"));
            return foldout;
        }

        private Foldout CreateInstanceSection()
        {
            var foldout = CreateSectionFoldout("Create Instance Config");
            foldout.Add(new PropertyField(serializedObject.FindProperty("instanceParent"), "Parent of Instance Model's UI"));
            foldout.Add(new PropertyField(serializedObject.FindProperty("animatorPrefab"), "UI Animator Prefab"));
            return foldout;
        }

        private Foldout CreateExecuteSection()
        {
            var foldout = CreateSectionFoldout("Execute Buttons");
            foldout.Add(CreateActionButton("Get Uxml Path", OnGetUxmlPathClicked));
            foldout.Add(CreateActionButton("Create Model", _reader.CreateModel));
            foldout.Add(CreateActionButton("Create Model Controller", _reader.CreateModelCtrl));
            foldout.Add(CreateActionButton("Create Instance on the hierarchy", _reader.InstantiateModelWithController));
            return foldout;
        }

        private void OnGetUxmlPathClicked()
        {
            SerializedProperty uxmlProperty = serializedObject.FindProperty("uxml");
            if (uxmlProperty.objectReferenceValue is null) return;

            SerializedProperty uxmlPathProperty = serializedObject.FindProperty("uxmlPath");
            uxmlPathProperty.stringValue = AssetDatabase.GetAssetPath(uxmlProperty.objectReferenceValue);
            serializedObject.ApplyModifiedProperties();
        }

        private static Foldout CreateSectionFoldout(string title)
        {
            var foldout = new Foldout
            {
                text = title,
                value = false,
                style = { marginTop = ButtonSpacing }
            };
            return foldout;
        }

        private static Button CreateActionButton(string label, System.Action onClick)
        {
            return new Button(onClick)
            {
                text = label,
                style = { height = ButtonHeight, marginTop = ButtonSpacing }
            };
        }
    }
}
