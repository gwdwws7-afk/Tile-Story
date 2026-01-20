using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProcessComponent))]
public class ProcessComponentInspector : GameframeworkInspector
{
    private SerializedProperty m_CurrentProcessName = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
            return;
        }

        ProcessComponent t = (ProcessComponent)target;

        if (IsPrefabInHierarchy(t.gameObject))
        {
            EditorGUILayout.LabelField("Process Count", t.Count.ToString());
            EditorGUILayout.LabelField("Current Process", t.CurrentProcessName == null ? "None" : t.CurrentProcessName);
        }

        Repaint();
    }

    private void OnEnable()
    {
        m_CurrentProcessName = serializedObject.FindProperty("CurrentProcessName");
    }
}
