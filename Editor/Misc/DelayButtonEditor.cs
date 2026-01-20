using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(DelayButton))]
[CanEditMultipleObjects]
public sealed class DelayButtonEditor : ButtonEditor
{
    private SerializedProperty delayTime;
    private SerializedProperty body;
    private SerializedProperty soundType;
    private SerializedProperty btnAnimType;

    protected override void OnEnable()
    {
        base.OnEnable();
        delayTime = serializedObject.FindProperty("delayTime");
        body = serializedObject.FindProperty("body");
        soundType = serializedObject.FindProperty("SoundType");
        btnAnimType = serializedObject.FindProperty("BtnAnimType");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(delayTime);
        EditorGUILayout.PropertyField(body);
        EditorGUILayout.PropertyField(soundType);
        EditorGUILayout.PropertyField(btnAnimType);
        serializedObject.ApplyModifiedProperties();
    }
}
