using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    [CustomEditor(typeof(TransitionData))]
    public class TransitionDataEditor : UnityEditor.Editor
    {
        private const int CacheRefreshIntervalMs = 300;
        private const float SectionSpacing = 15f;
        private const float PanelRowHeight = 22f;
        private const float StyleRowHeight = 46f;
        private const string UssFileExtension = ".uss";

        private TransitionData _data;

        private SerializedProperty _uxmlProperty;
        private SerializedProperty _styleSheetProperty;
        private SerializedProperty _transitedPanelNamesProperty;
        private SerializedProperty _styleClassesProperty;

        private ListView _panelListView;
        private ListView _styleListView;
        private HelpBox _styleSheetHelp;

        private Object _cachedUxml;
        private Object _cachedStyleSheet;
        private readonly List<string> _elementChoices = new List<string>();

        public override VisualElement CreateInspectorGUI()
        {
            _data = (TransitionData)target;

            _uxmlProperty = serializedObject.FindProperty("uxml");
            _styleSheetProperty = serializedObject.FindProperty("styleSheet");
            _transitedPanelNamesProperty = serializedObject.FindProperty("transitedPanelNames");
            _styleClassesProperty = serializedObject.FindProperty("styleClasses");

            var root = new VisualElement();

            root.Add(new PropertyField(_uxmlProperty,
                "UXML : editor workflow only, used to pick animated target panels. Not required at runtime."));
            root.Add(new PropertyField(_styleSheetProperty));

            _styleSheetHelp = new HelpBox(
                "There is no styleSheet. You should assign the uss file.",
                HelpBoxMessageType.Warning);
            root.Add(_styleSheetHelp);

            AddSectionSpace(root);
            root.Add(CreateHeaderLabel("Style Target VisualElement"));
            root.Add(CreatePanelList());

            AddSectionSpace(root);
            root.Add(CreateHeaderLabel("Style Classes"));
            root.Add(CreateStyleList());
            root.Add(CreateHelpTexts());

            RefreshCachesIfDirty();
            root.schedule.Execute(RefreshCachesIfDirty).Every(CacheRefreshIntervalMs);

            return root;
        }

        private ListView CreatePanelList()
        {
            _panelListView = new ListView
            {
                bindingPath = "transitedPanelNames",
                fixedItemHeight = PanelRowHeight,
                reorderable = true,
                showAddRemoveFooter = true,
                makeItem = () => new PanelNameRow(),
                bindItem = (element, index) =>
                    ((PanelNameRow)element).Bind(_transitedPanelNamesProperty.GetArrayElementAtIndex(index), _elementChoices),
                unbindItem = (element, _) => ((PanelNameRow)element).Unbind(),
            };
            return _panelListView;
        }

        private ListView CreateStyleList()
        {
            _styleListView = new ListView
            {
                bindingPath = "styleClasses",
                fixedItemHeight = StyleRowHeight,
                reorderable = true,
                showAddRemoveFooter = true,
                makeItem = () => new StyleClassRow(),
                bindItem = (element, index) =>
                    ((StyleClassRow)element).Bind(_styleClassesProperty.GetArrayElementAtIndex(index), _data.styleSheetsClassNames),
                unbindItem = (element, _) => ((StyleClassRow)element).Unbind(),
            };
            return _styleListView;
        }

        private void RefreshCachesIfDirty()
        {
            if (_uxmlProperty.objectReferenceValue != _cachedUxml)
            {
                _cachedUxml = _uxmlProperty.objectReferenceValue;
                RebuildElementChoices();
            }

            if (_styleSheetProperty.objectReferenceValue != _cachedStyleSheet)
            {
                _cachedStyleSheet = _styleSheetProperty.objectReferenceValue;
                RebuildStyleSheetClassChoices();
            }
        }

        private void RebuildElementChoices()
        {
            _elementChoices.Clear();

            if (_cachedUxml is not null)
            {
                string uxmlPath = AssetDatabase.GetAssetPath(_cachedUxml);
                if (!string.IsNullOrEmpty(uxmlPath) && File.Exists(uxmlPath))
                {
                    foreach (UxmlElementInfo info in UxmlElementScanner.Scan(File.ReadAllText(uxmlPath)))
                        _elementChoices.Add(info.ElementName);
                }
            }

            _panelListView?.RefreshItems();
        }

        private void RebuildStyleSheetClassChoices()
        {
            _data.styleSheetsClassNames.Clear();

            if (_cachedStyleSheet is not null)
            {
                string ussPath = AssetDatabase.GetAssetPath(_cachedStyleSheet);
                if (!string.IsNullOrEmpty(ussPath)
                    && Path.GetExtension(ussPath) == UssFileExtension
                    && File.Exists(ussPath))
                {
                    _data.styleSheetsClassNames.AddRange(UssClassParser.ParseClassNames(File.ReadAllText(ussPath)));
                }
            }

            _styleSheetHelp.style.display = _cachedStyleSheet is null ? DisplayStyle.Flex : DisplayStyle.None;
            _styleListView?.RefreshItems();
        }

        private static void AddSectionSpace(VisualElement root)
        {
            root.Add(new VisualElement { style = { height = SectionSpacing } });
        }

        private static Label CreateHeaderLabel(string text)
        {
            return new Label(text)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            };
        }

        private VisualElement CreateHelpTexts()
        {
            var container = new VisualElement();
            container.style.marginTop = SectionSpacing;

            container.Add(new HelpBox(
                "Save the styleSheet classes you want to drive here.\n" +
                "When an Animation Class toggle is on, the class can be added to the element at runtime.\n" +
                "Classes are applied in order.",
                HelpBoxMessageType.None));

            container.Add(new HelpBox(
                "Animation Class : transition class name.\n" +
                "Swapped Class : while the Animation Class transition runs, this class is added or removed as its counterpart. It is interchanged with the Animation Class.\n" +
                "Start Awake Class : on game start this class is applied instead of the swapped class.",
                HelpBoxMessageType.None));

            return container;
        }
    }
}
