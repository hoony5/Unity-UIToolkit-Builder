using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UxmlToScript))]
public class UxmlToScriptEditor : Editor
{
    private UxmlToScript _reader;

    private SerializedProperty _uxmlAssetProperty;
    private SerializedProperty _uxmlAssetPathProperty;
    private SerializedProperty _panelSettingAssetProperty;
    private SerializedProperty _sortOrderProperty;
    
    private SerializedProperty _modelScriptNameProperty;
    private SerializedProperty _controllerScriptNameProperty;
    private SerializedProperty _savePathProperty;
    
    private SerializedProperty _instanceParentProperty;
    private SerializedProperty _animatorPrefabProperty;
    
    private GUIContent _uxmlAssetLabel = new GUIContent("Uxml Asset");
    private GUIContent _uxmlAssetPathLabel = new GUIContent("Uxml Asset Path");
    private GUIContent _panelSettingAssetLabel = new GUIContent("Panel Setting Asset");
    private GUIContent _sortOrderLabel = new GUIContent("Sort Order");
    
    private GUIContent _modelScriptNameLabel = new GUIContent("Model Script Name");
    private GUIContent _controllerScriptNameLabel = new GUIContent("Controller Script Name");
    private GUIContent _savePathLabel = new GUIContent("To Save Path");
    
    private GUIContent _instanceParentLabel = new GUIContent("Parent of Instance Model's UI ");
    private GUIContent _animatorPrefabLabel = new GUIContent("UI Animator Prefab");
    
    private GUILayoutOption _buttonHeight = GUILayout.Height(30);
    
    private bool _editUxmlConfig;
    private bool _makeScriptConfig;
    private bool _createInstanceConfig;
    private bool _testMode;

    private const int LargeFontSize = 20;
    private const int FontSize = 16;
    private void OnEnable()
    {
        _reader = (UxmlToScript)target;
        
        _uxmlAssetProperty = serializedObject.FindProperty("uxml");
        _uxmlAssetPathProperty = serializedObject.FindProperty("uxmlPath");
        _panelSettingAssetProperty = serializedObject.FindProperty("panelSettings");
        _sortOrderProperty = serializedObject.FindProperty("sortOrder");
        
        _modelScriptNameProperty = serializedObject.FindProperty("modelScriptName");
        _controllerScriptNameProperty = serializedObject.FindProperty("controllerScriptName");
        _savePathProperty = serializedObject.FindProperty("savePath");
        
        _instanceParentProperty = serializedObject.FindProperty("instanceParent");
        _animatorPrefabProperty = serializedObject.FindProperty("animatorPrefab");
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        
        DrawUXmlConfigArea();
        EditorGUILayout.Space(10);
        DrawMakingScriptConfig();
        EditorGUILayout.Space(10);
        DrawCreateInstanceConfig();
        
        EditorGUILayout.Space(10);
        DrawButtons();
        EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUXmlConfigArea()
    {
        GUIStyle uxmlConfigStyle = new  GUIStyle(GUI.skin.button){fixedHeight = 40, fontSize = FontSize, richText = true};
        if(_editUxmlConfig)
            uxmlConfigStyle = new GUIStyle(EditorStyles.foldoutHeader){stretchWidth = true,fixedHeight = 40, fontSize = LargeFontSize, richText = true, alignment = TextAnchor.MiddleCenter};
        
        _editUxmlConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_editUxmlConfig,"<color=lime>Uxml Config</color>",uxmlConfigStyle);
        if (_editUxmlConfig)
        {
            EditorGUILayout.PropertyField(_uxmlAssetProperty, _uxmlAssetLabel);
            EditorGUILayout.PropertyField(_uxmlAssetPathProperty, _uxmlAssetPathLabel);
            EditorGUILayout.PropertyField(_panelSettingAssetProperty, _panelSettingAssetLabel);
            EditorGUILayout.PropertyField(_sortOrderProperty, _sortOrderLabel);
        } 
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawMakingScriptConfig()
    {
        GUIStyle uxmlConfigStyle = new  GUIStyle(GUI.skin.button){fixedHeight = 40, fontSize = FontSize,richText = true};
        if(_makeScriptConfig)
            uxmlConfigStyle = new GUIStyle(EditorStyles.foldoutHeader){stretchWidth = true,fixedHeight = 40, fontSize = LargeFontSize, richText = true, alignment = TextAnchor.MiddleCenter};
        
        _makeScriptConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_makeScriptConfig,"<color=lime>Making Script Config</color>",uxmlConfigStyle);
        if (_makeScriptConfig)
        {
            EditorGUILayout.PropertyField(_modelScriptNameProperty, _modelScriptNameLabel);
            EditorGUILayout.PropertyField(_controllerScriptNameProperty, _controllerScriptNameLabel);
            EditorGUILayout.PropertyField(_savePathProperty, _savePathLabel);
        } 
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void DrawCreateInstanceConfig()
    {
        GUIStyle uxmlConfigStyle = new  GUIStyle(GUI.skin.button){fixedHeight = 40,fontSize = FontSize, richText = true};
        if(_createInstanceConfig)
            uxmlConfigStyle = new GUIStyle(EditorStyles.foldoutHeader){stretchWidth = true,fixedHeight = 40, fontSize = LargeFontSize, richText = true, alignment = TextAnchor.MiddleCenter};
        
        _createInstanceConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_createInstanceConfig,"<color=lime>Create Instance Config</color>",uxmlConfigStyle);
        if (_createInstanceConfig)
        {
            EditorGUILayout.PropertyField(_instanceParentProperty, _instanceParentLabel);
            EditorGUILayout.PropertyField(_animatorPrefabProperty, _animatorPrefabLabel);
        } 
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void DrawButtons()
    {
        GUIStyle uxmlConfigStyle = new  GUIStyle(GUI.skin.button){fixedHeight = 40,fontSize = FontSize, richText = true};
        if(_testMode)
            uxmlConfigStyle = new GUIStyle(EditorStyles.foldoutHeader){stretchWidth = true,fixedHeight = 40, fontSize = LargeFontSize, richText = true, alignment = TextAnchor.MiddleCenter};
        
        _testMode = EditorGUILayout.BeginFoldoutHeaderGroup(_testMode,"<color=lime>Execute Buttons</color>",uxmlConfigStyle);
        if (_testMode)
        {
            if (GUILayout.Button("Get Uxml Path", _buttonHeight))
            {
                _reader.GetUxmlPath();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Model", _buttonHeight))
            {
                _reader.CreateModel();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Model Controller", _buttonHeight))
            {
                _reader.CreateModelCtrl();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Instance on the hierarchy", _buttonHeight))
            {
                _reader.InstantiateModelWithController();
            }
        } 
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}