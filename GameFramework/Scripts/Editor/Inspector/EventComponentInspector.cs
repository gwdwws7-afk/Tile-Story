using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityGameFramework.Runtime;

[CustomEditor(typeof(EventComponent))]
public sealed class EventComponentInspector : GameframeworkInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
            return;
        }

        EventComponent t = (EventComponent)target;

        if (IsPrefabInHierarchy(t.gameObject))
        {
            EditorGUILayout.LabelField("Event Handler Count", t.EventHandlerCount.ToString());
            EditorGUILayout.LabelField("Event Count", t.EventCount.ToString());
        }

        Repaint();
    }
}
