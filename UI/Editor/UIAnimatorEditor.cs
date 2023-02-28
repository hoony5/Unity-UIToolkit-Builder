using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIAnimator))]
public class UIAnimatorEditor : Editor
{
    private static GUIContent _isTestModeLabel = new GUIContent("Test Mode"); 
    private static GUIContent _controllerLabel = new GUIContent("Animator Controller"); 
    private static GUIContent _visualElementNameLabel = new GUIContent("visualElementNames");
    
    private UIAnimator _data;

    private SerializedProperty _isTestModeProperty;
    private SerializedProperty _dataControllerProperty;
    private SerializedProperty _visualElementNamesProperty;

    private GUILayoutOption _buttonHeight = GUILayout.Height(30);

    private void OnEnable()
    {
        _data = target as UIAnimator;
        _isTestModeProperty = serializedObject.FindProperty("isTestMode");
        _dataControllerProperty = serializedObject.FindProperty("dataController");
        _visualElementNamesProperty = serializedObject.FindProperty("visualElementNames");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(_dataControllerProperty, _controllerLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(_isTestModeProperty, _isTestModeLabel);
        EditorGUILayout.Space(10);

        if (_isTestModeProperty.boolValue)
        {
            EditorGUILayout.PropertyField(_visualElementNamesProperty, _visualElementNameLabel);
            EditorGUILayout.Space(10);
            if (GUILayout.Button($"Play Test", _buttonHeight))
            {
                foreach (string visualElementName in _data.visualElementNames)
                {
                    _data.Play(visualElementName);
                }
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button($"Reverse Play Test", _buttonHeight))
            {
                foreach (string visualElementName in _data.visualElementNames)
                {
                    _data.ReversePlay(visualElementName);
                }
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("ResetStyleClassesList", _buttonHeight))
            {
                _data.OnUpdateStyle();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Debug ClassList", _buttonHeight))
            {
                foreach (string visualElementName in _data.visualElementNames)
                {
                    _data.GetClassList(visualElementName);
                }
            }
        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
