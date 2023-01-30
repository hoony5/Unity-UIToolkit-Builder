using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(TransitionData))]
public class TransitionDataEditor : Editor
{
    private TransitionData _data;
    private ReorderableList _list;
    
    private bool isExpanded = false;
    private bool styleSheetIsNull = true;
    private bool transitedPanelIsNull = true;
    
    private const string TransitedPanelNameLabel = "VisualElement";
    private const string TransitedClassesLabel = "Style Classes";
    
    public GUIContent transitedPanelLabel;
    public GUIContent transitedClassesLabel;
    private void OnEnable()
    {
        _data ??= target as TransitionData;
        transitedPanelLabel ??= new GUIContent(TransitedPanelNameLabel);
        transitedClassesLabel ??= new GUIContent(TransitedClassesLabel);
        
        _list = new ReorderableList(serializedObject,serializedObject.FindProperty("styleClasses"), true, true, true, true);
        _list.drawHeaderCallback = DrawHeader;
        _list.drawElementCallback = DrawElements;
    }
        // styleName
        // isTriggerStyle
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugOn"));
        EditorGUILayout.Space(10);
        // 스타일 시트
        SerializedProperty styleSheet = serializedObject.FindProperty("styleSheet");
        EditorGUILayout.PropertyField(styleSheet);
        styleSheetIsNull = styleSheet.objectReferenceValue is null;
        if (styleSheetIsNull)
        {
            EditorGUILayout.LabelField("There is no <color=red>styleSheet</color>. You Should assign the uss file.", new GUIStyle(GUI.skin.box){fontSize = 12, alignment = TextAnchor.MiddleCenter, richText = true, stretchWidth = true});    
        }
        EditorGUILayout.Space(15);
        
        // 대상 패널
        SerializedProperty transitedPanelNames = serializedObject.FindProperty("transitedPanelNames");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("transitedPanelNames"), transitedPanelLabel);
        transitedPanelIsNull = transitedPanelNames.arraySize == 0;
        if (transitedPanelIsNull)
        {
            EditorGUILayout.LabelField("There is no <color=red>VisualElement</color>. Assign the VisualElement's name in the Uxml Hierarchy..", new GUIStyle(GUI.skin.box){fontSize = 12, alignment = TextAnchor.MiddleCenter, richText = true, stretchWidth = true});    
        }
        EditorGUILayout.Space(15);
        _list.DoLayoutList();
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("When you need to use classes of <color=lime>styleSheet</color>, you save it on the scriptableObject.\n\n <color=lime>If the Animation Class toggle was on</color>, it could be added to styleSheet on runtime. \n\n will be executed in order .", new GUIStyle(GUI.skin.box){fontSize = 14, alignment = TextAnchor.MiddleLeft, richText = true, stretchWidth = true});
        EditorGUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawElements(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
        
        EditorGUI.LabelField(new Rect(rect.x, rect.y, 169, EditorGUIUtility.singleLineHeight), $"{index}{(index == 2 ? "nd" : index == 3 ? "rd" : "st")} style class");
        EditorGUI.PropertyField(new Rect(rect.x + 169, rect.y, 200, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("styleName"),
            GUIContent.none);
        
        EditorGUI.LabelField(new Rect(rect.x + 400, rect.y, 128, EditorGUIUtility.singleLineHeight), $"Animation Class");
        EditorGUI.PropertyField(new Rect(rect.x + 528, rect.y, 16, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("isTriggerStyle"),
            GUIContent.none);
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, TransitedClassesLabel);
    }
}
