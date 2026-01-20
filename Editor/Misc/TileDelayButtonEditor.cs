using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(TileDelayButton))]
[CanEditMultipleObjects]
public sealed class TileDelayButtonEditor : ButtonEditor
{
    private SerializedProperty delayTime;
    private SerializedProperty body;
    private SerializedProperty soundType;
    private SerializedProperty btnAnimType;
    private SerializedProperty isRecordSiblingIndex;
    private SerializedProperty hoverAnimationCurve;
    private SerializedProperty hoverAnimationCurve1;
    private SerializedProperty noHoverAnimationCurve;

    protected override void OnEnable()
    {
        base.OnEnable();
        delayTime = serializedObject.FindProperty("delayTime");
        body = serializedObject.FindProperty("body");
        soundType = serializedObject.FindProperty("SoundType");
        btnAnimType = serializedObject.FindProperty("BtnAnimType");
        isRecordSiblingIndex = serializedObject.FindProperty("IsRecordSiblingIndex");
        hoverAnimationCurve = serializedObject.FindProperty("HoverAnimationCurve");
        hoverAnimationCurve1 = serializedObject.FindProperty("HoverAnimationCurve1");
        noHoverAnimationCurve = serializedObject.FindProperty("NoHoverAnimationCurve");
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
        EditorGUILayout.PropertyField(isRecordSiblingIndex);
        EditorGUILayout.PropertyField(hoverAnimationCurve);
        EditorGUILayout.PropertyField(hoverAnimationCurve1);
        EditorGUILayout.PropertyField(noHoverAnimationCurve);
        serializedObject.ApplyModifiedProperties();
    }
}
