using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SearchRefrenceEditorWindow : EditorWindow
{
    /// <summary>
    /// 查找引用
    /// </summary>
    [MenuItem("Assets/SearchRefrence")]
    static void SearchRefrence()
    {
        SearchRefrenceEditorWindow window = (SearchRefrenceEditorWindow)EditorWindow.GetWindow(typeof(SearchRefrenceEditorWindow), false, "Searching", true);
        window.Show();
    }

    private static Object searchObject;
    private List<Object> result = new List<Object>();
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        searchObject = EditorGUILayout.ObjectField(searchObject, typeof(Object), true, GUILayout.Width(200));
        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            result.Clear();

            if (searchObject == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(searchObject);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayCancelableProgressBar("Checking", filePath, i / guids.Length * 1.0f);

                string content = File.ReadAllText(filePath);
                if (content.Contains(assetGuid))
                {
                    Object fileObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object));
                    result.Add(fileObject);
                }
            }
            EditorUtility.ClearProgressBar();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        for (int i = 0; i < result.Count; i++)
        {
            EditorGUILayout.ObjectField(result[i], typeof(Object), true, GUILayout.Width(300));
        }
        EditorGUILayout.EndHorizontal();
    }

}
