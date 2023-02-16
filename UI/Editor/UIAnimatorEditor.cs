using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIAnimator))]
public class UIAnimatorEditor : Editor
{
    private UIAnimator _data;

    private void OnEnable()
    {
        _data ??= target as UIAnimator;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Play Test : root"))
        {
            _data.Play("root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reverse Play Test : root"))
        {
            _data.ReversePlay("root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Play Test : dynamic-root"))
        {
            _data.Play("dynamic-root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reverse Play Test : dynamic-root"))
        {
            _data.ReversePlay("dynamic-root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Play Test : Together"))
        {
            _data.Play("root");
            _data.Play("dynamic-root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reverse Play Test : Together"))
        {
            _data.ReversePlay("root");
            _data.ReversePlay("dynamic-root");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("ResetStyleClassesList"))
        {
            _data.OnUpdateStyle();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
