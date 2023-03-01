using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(TransitionData))]
public class TransitionDataEditor : Editor
{
    private const string IgnoreElementNameCase = "___";
    private const string TransitedPanelNameLabel = "Style Target VisualElement";
    private const string TransitedClassesLabel = "Style Classes";
    
    private TransitionData _data;
    private ReorderableList _styleList;
    private ReorderableList _transitedPanelList;

    private SerializedProperty _uxml;
    private SerializedProperty _styleSheet;
    private SerializedProperty _transitedPanelNames;
    
    private bool _styleSheetIsInvalidated = true;
    private bool _styleSheetIsNull = true;
    private bool _transitedPanelIsNull = true;
    
    private GUIContent _transitedPanelLabel;
    private GUIContent _transitedClassesLabel;

    private static GUIStyle _basicStyle;
    private static GUIStyle _largeFontStyle;

    private List<string> _elementInfos = new List<string>(72);
    private List<int> _elementIndices = new List<int>(72);
    private void OnEnable()
    {
        _data = target as TransitionData;

        _elementIndices = Enumerable.Repeat(0, _elementIndices.Capacity - 1).ToList();
        _uxml = serializedObject.FindProperty("uxml");
        _styleSheet = serializedObject.FindProperty("styleSheet");
        _transitedPanelNames = serializedObject.FindProperty("transitedPanelNames");
        
        _transitedPanelLabel = new GUIContent(TransitedPanelNameLabel);
        _transitedClassesLabel = new GUIContent(TransitedClassesLabel);

        _styleList = new ReorderableList(serializedObject, serializedObject.FindProperty("styleClasses"), true, true, true,
            true);
        _styleList.drawHeaderCallback = DrawStyleListHeader;
        _styleList.drawElementCallback = DrawStyleListElements;
        _styleList.onAddCallback = AddStyleListElement;
        
        _transitedPanelList = new ReorderableList(serializedObject, serializedObject.FindProperty("transitedPanelNames"), true, true, true,
            true);
        _transitedPanelList.drawHeaderCallback = DrawPanelListHeader;
        _transitedPanelList.drawElementCallback = DrawPanelListElements;
    }

    private void AddStyleListElement(ReorderableList list)
    {
        int index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = 0;
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("styleName").stringValue = string.Empty;
        element.FindPropertyRelative("isTriggerStyle").boolValue = false;
        element.FindPropertyRelative("_selectedStyleClassIndex").intValue = 0;
        element.FindPropertyRelative("swappedClass").stringValue = string.Empty;
        element.FindPropertyRelative("isTriggerStyleOnStart").boolValue = false;
    }

    private void DrawStyleListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, TransitedClassesLabel);
    }
    private void DrawPanelListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, TransitedPanelNameLabel);
    }

    // styleName
    // isTriggerStyle
    private void Read(Object uss)
    {
        if (uss is null) return;
        string path = AssetDatabase.GetAssetPath(uss);
        if (!Path.GetExtension(path).Contains(".uss"))
        {
            _styleSheetIsInvalidated = true;
            return;
        }

        _styleSheetIsInvalidated = false;
        using FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        using StreamReader reader = new StreamReader(fs);
        string css = reader.ReadToEnd();
        
        // .selector , must have block nested
        string pattern = @"(?<=^|\n)(?<className>\.[a-zA-Z0-9_\-]+)";

        MatchCollection matches = Regex.Matches(css, pattern, RegexOptions.Multiline);

        _data.styleSheetsClassNames.Clear();
        if (matches.Count == 0) return;
        
        foreach (Match match in matches)
        {
            string selector = match.Groups["className"].Value.TrimStart('.');
            if (!_data.styleSheetsClassNames.Contains(selector))
                _data.styleSheetsClassNames.Add(selector);
        }

        reader.Close();
        fs.Close();
    }

    public override void OnInspectorGUI()
    {
        _basicStyle = new GUIStyle(GUI.skin.box)
            { fontSize = 12, alignment = TextAnchor.MiddleCenter, richText = true, stretchWidth = true };
        _largeFontStyle = new GUIStyle(GUI.skin.box)
            { fontSize = 14, alignment = TextAnchor.MiddleLeft, richText = true, stretchWidth = true };
        
        serializedObject.Update();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(_uxml, new GUIContent("UXML : This is for editor work flow, get animated target Panel, not runtime. null is fine."));
        EditorGUILayout.Space(10);
        
        if(_styleSheet.objectReferenceValue is not null)
            Read(_styleSheet.objectReferenceValue);
        
        EditorGUILayout.PropertyField(_styleSheet);
        _styleSheetIsNull = _styleSheet.objectReferenceValue is null;
        if (_styleSheetIsNull)
        {
            EditorGUILayout.LabelField("There is no <color=red>styleSheet</color>. You Should assign the uss file.",
                _basicStyle);
        }

        EditorGUILayout.Space(15);

        // animated Panel
       // EditorGUILayout.PropertyField(_transitedPanelNames, _transitedPanelLabel);
        
       _transitedPanelList.DoLayoutList();
       
        _transitedPanelIsNull = _transitedPanelNames.arraySize == 0;
        if (_transitedPanelIsNull)
        {
            EditorGUILayout.LabelField(
                "There is no <color=red>VisualElement</color>. Assign the VisualElement's name in the Uxml Hierarchy..",
                _basicStyle);
        }

        EditorGUILayout.Space(15);
        _styleList.DoLayoutList();
        EditorGUILayout.LabelField(
            "When you need to use classes of <color=lime>styleSheet</color>, you save it on the scriptableObject.\n\n <color=lime>If the Animation Class toggle was on</color>, it could be added to styleSheet on runtime. \n\n will be executed in order .",
            _largeFontStyle);
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField(
            @"<color=lime>AnimationClass</color> : Transition class Name
<color=lime>Swap Class</color> : When AnimationClass transitioning go on, swapped class,not allowing null or empty, will be added or removed to styleSheet on runtime. <color=lime>It is interExchange with AnimationClass.</color>
<color=lime>Start Awake Class</color> : If start game, not swapped class but this class will be added to styleSheet on runtime.",
            _largeFontStyle);
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStyleListElements(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (_data.styleSheetsClassNames.Count == 0) return;
        
        SerializedProperty element = _styleList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty _selectedStyleClassIndexProperty = element.FindPropertyRelative("_selectedStyleClassIndex");
        // styleName
        string styleName = element.FindPropertyRelative("styleName").stringValue;
        
        Rect firstLabelRect = new Rect(rect.x, rect.y, 169, EditorGUIUtility.singleLineHeight);
        Rect firstPropertyRect = new Rect(rect.x + 169, rect.y, 200, EditorGUIUtility.singleLineHeight);
        Rect secondLabelRect = new Rect(rect.x + 400, rect.y, 128, EditorGUIUtility.singleLineHeight);
        Rect secondPropertyRect = new Rect(rect.x + 528, rect.y, 16, EditorGUIUtility.singleLineHeight);
        Rect thirdLabelRect = new Rect(rect.x + 560, rect.y, 100, EditorGUIUtility.singleLineHeight);
        Rect thirdPropertyRect = new Rect(rect.x + 660, rect.y, 169, EditorGUIUtility.singleLineHeight);
        Rect fourthLabelRect = new Rect(rect.x + 850, rect.y, 128, EditorGUIUtility.singleLineHeight);
        Rect fourthPropertyRect = new Rect(rect.x + 1000, rect.y, 16, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(firstLabelRect, $"{index}{(index == 2 ? "nd" : index == 3 ? "rd" : "st")} style class");
        // pop up style
        if (_styleSheet.objectReferenceValue is null)
        {
            // if there is no styleSheet, you can write styleName directly.
            element.FindPropertyRelative("styleName").stringValue = new string(EditorGUI.TextField(firstPropertyRect, styleName));
        }
        else
        {
            // if styleSheet is changed or invalidated, you cannot select.
            if (!_data.styleSheetsClassNames.Contains(styleName) && !string.IsNullOrEmpty(styleName) || _styleSheetIsInvalidated)
            {
                GUI.enabled = false;
                element.FindPropertyRelative("styleName").stringValue = new string(EditorGUI.TextField(firstPropertyRect, styleName));
                GUI.enabled = true;
            }
            // if styleSheet is valid, you can select.
            else
            {
                _selectedStyleClassIndexProperty.intValue = EditorGUI.Popup(
                    firstPropertyRect
                    , _selectedStyleClassIndexProperty.intValue
                    , _data.styleSheetsClassNames.ToArray());
                element.FindPropertyRelative("styleName").stringValue =  new string(_data.styleSheetsClassNames[_selectedStyleClassIndexProperty.intValue]);   
            }
        }

        // isTriggerStyle : animation class
        EditorGUI.LabelField(secondLabelRect, $"Is Animation Class");
        EditorGUI.PropertyField(secondPropertyRect, element.FindPropertyRelative("isTriggerStyle"), GUIContent.none);

        // initStyle : animation class
        EditorGUI.LabelField(new Rect(thirdLabelRect), $"Swapped Class");
        EditorGUI.PropertyField(thirdPropertyRect, element.FindPropertyRelative("swappedClass"), GUIContent.none);
        
        // initStyle : animation class
        EditorGUI.LabelField(fourthLabelRect, $"Start Awake Class");
        EditorGUI.PropertyField(fourthPropertyRect, element.FindPropertyRelative("isTriggerStyleOnStart"), GUIContent.none);
    }

    private void DrawPanelListElements(Rect rect, int index, bool isactive, bool isfocused)
    {
        string selectedName = "";
        SerializedProperty element = _transitedPanelList.serializedProperty.GetArrayElementAtIndex(index);
        EditorGUI.LabelField(new Rect(rect.x, rect.y, 256, EditorGUIUtility.singleLineHeight),
            "Animated Visual Element's name : ");

        if (index >= _elementIndices.Count)
        {
            _elementIndices.AddRange(Enumerable.Range(0, _elementInfos.Count));
        }
        if (_uxml.objectReferenceValue is not null)
        {
            GetVisualElementNames(_uxml.objectReferenceValue);
            // pop up style
            _elementIndices[index] = EditorGUI.Popup(new Rect(rect.x + 320, rect.y, 256, EditorGUIUtility.singleLineHeight),
                _elementIndices[index], _elementInfos.ToArray());
            element.stringValue = selectedName = new string(_elementInfos[_elementIndices[index]]);
        }
        else
        {
            GUI.enabled = false;
            selectedName = EditorGUI.TextField(new Rect(rect.x + 320, rect.y, 256, EditorGUIUtility.singleLineHeight),
                element.stringValue);
            GUI.enabled = true;
        }
    }
    private void GetVisualElementNames(Object uxml)
    {
#if UNITY_EDITOR
        if (!uxml) return;
        string path = UnityEditor.AssetDatabase.GetAssetPath(uxml);
#endif
        XDocument document = XDocument.Load(path);
        IEnumerable<string> elementNames = document.Elements().Descendants().Select(i => i.Attribute("name")?.Value);

        foreach (string elementName in elementNames)
        {
            // ignore empty name element
            if (string.IsNullOrEmpty(elementName)) continue;
            // ignore prefix :  "___"
            if (elementName.Contains(IgnoreElementNameCase)) continue;
            
            _elementInfos.Add(elementName);
        }
    }
}
