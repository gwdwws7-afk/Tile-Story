using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(SwitchButton))]
public sealed class SwitchButtonEditor : ButtonEditor
{
    private SerializedProperty on;
    private SerializedProperty off;
    private SerializedProperty body;
    private SerializedProperty soundType;

    protected override void OnEnable()
    {
        base.OnEnable();
        on = serializedObject.FindProperty("on");
        off = serializedObject.FindProperty("off");
        body = serializedObject.FindProperty("body");
        soundType = serializedObject.FindProperty("SoundType");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(on);
        EditorGUILayout.PropertyField(off);
        EditorGUILayout.PropertyField(body);
        EditorGUILayout.PropertyField(soundType);
        serializedObject.ApplyModifiedProperties();
    }
}
