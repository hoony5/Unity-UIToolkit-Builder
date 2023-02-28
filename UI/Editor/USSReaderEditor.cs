using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(USSReader))]
public class USSReaderEditor : Editor
{
    private USSReader _reader;
    private GUILayoutOption _buttonHeight = GUILayout.Height(30);
    private void OnEnable()
    {
        _reader = (USSReader)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(15);
        if (GUILayout.Button("read", _buttonHeight) && _reader.uss)
        {
            _reader.path = AssetDatabase.GetAssetPath(_reader.uss);
            _reader.Read();
        }
        EditorGUILayout.Space(15);
        EditorGUILayout.EndVertical();
    }
}