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
        if (GUILayout.Button("Play"))
        {
            _data.Play();
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reverse Play"))
        {
            _data.ReversePlay();
        }
        if (GUILayout.Button("ResetStyleClassesList"))
        {
            _data.OnUpdateStyle();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
