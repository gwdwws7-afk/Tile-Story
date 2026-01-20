using GameFramework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityGameFramework.Runtime;

[CustomEditor(typeof(ReferencePoolComponent))]
public sealed class ReferencePoolComponentInspector : GameframeworkInspector
{
    private readonly Dictionary<string, List<ReferencePoolInfo>> m_ReferencePoolInfos = new Dictionary<string, List<ReferencePoolInfo>>(StringComparer.Ordinal);
    private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

    private bool m_ShowFullClassName = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
            return;
        }

        serializedObject.Update();

        ReferencePoolComponent t = (ReferencePoolComponent)target;

        if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
        {
            EditorGUILayout.LabelField("Reference Pool Count", ReferencePool.Count.ToString());
            m_ShowFullClassName = EditorGUILayout.Toggle("Show Full Class Name", m_ShowFullClassName);
            m_ReferencePoolInfos.Clear();
            ReferencePoolInfo[] referencePoolInfos = ReferencePool.GetAllReferencePoolInfos();
            foreach (ReferencePoolInfo referencePoolInfo in referencePoolInfos)
            {
                string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                List<ReferencePoolInfo> results = null;
                if (!m_ReferencePoolInfos.TryGetValue(assemblyName, out results))
                {
                    results = new List<ReferencePoolInfo>();
                    m_ReferencePoolInfos.Add(assemblyName, results);
                }

                results.Add(referencePoolInfo);
            }

            foreach (KeyValuePair<string, List<ReferencePoolInfo>> assemblyReferencePoolInfo in m_ReferencePoolInfos)
            {
                bool lastState = m_OpenedItems.Contains(assemblyReferencePoolInfo.Key);
                bool currentState = EditorGUILayout.Foldout(lastState, assemblyReferencePoolInfo.Key);
                if (currentState != lastState)
                {
                    if (currentState)
                    {
                        m_OpenedItems.Add(assemblyReferencePoolInfo.Key);
                    }
                    else
                    {
                        m_OpenedItems.Remove(assemblyReferencePoolInfo.Key);
                    }
                }

                if (currentState)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name", "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                        assemblyReferencePoolInfo.Value.Sort(Comparison);
                        foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                        {
                            EditorGUILayout.LabelField(m_ShowFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", referencePoolInfo.UnusedReferenceCount, referencePoolInfo.UsingReferenceCount, referencePoolInfo.AcquireReferenceCount, referencePoolInfo.ReleaseReferenceCount, referencePoolInfo.AddReferenceCount, referencePoolInfo.RemoveReferenceCount));
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Separator();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        Repaint();
    }

    private int Comparison(ReferencePoolInfo a, ReferencePoolInfo b)
    {
        if (m_ShowFullClassName)
        {
            return a.Type.FullName.CompareTo(b.Type.FullName);
        }
        else
        {
            return a.Type.Name.CompareTo(b.Type.Name);
        }
    }
}
