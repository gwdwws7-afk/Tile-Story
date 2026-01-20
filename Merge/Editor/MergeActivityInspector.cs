using Merge;
using UnityEditor;

[CustomEditor(typeof(MergeActivityBase),true)]
public class MergeActivityInspector : Editor
{
    SerializedProperty includeInBuild;

    private void OnEnable()
    {
        includeInBuild = serializedObject.FindProperty("IncludeInBuild");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(includeInBuild);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ((MergeActivityBase)target).OnIncludeInBuildChangedInEditor();
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
