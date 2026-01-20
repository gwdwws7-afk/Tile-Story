using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateLink
{
    [MenuItem("Tools/CreateLink.xml")]
    public static void FindAllScripts()
    {
        EditorUtility.DisplayProgressBar("Progress", "Find Class...", 0);
        string[] dirs = { "Assets/TileMatch/Res", "Assets/GameMain" };
        var asstIds = AssetDatabase.FindAssets("t:Prefab", dirs);
        int count = 0;

        List<string> classList = new List<string>();
        for (int i = 0; i < asstIds.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(asstIds[i]);
            var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Transform item in pfb.transform)
            {
                var coms = item.GetComponentsInChildren<Component>();
                foreach (var com in coms)
                {
                    if (com != null)
                    {
                        string tName = com.GetType().FullName;
                        if (!classList.Contains(tName) && (tName.StartsWith("UnityEngine") || tName.StartsWith("TMPro")))
                        {
                            classList.Add(tName);
                        }
                    }
                }
            }
            count++;
            EditorUtility.DisplayProgressBar("Find Class", pfb.name, count / (float)asstIds.Length);
        }
        
        
        for (int i = 0; i < classList.Count; i++)
        {
            classList[i] = string.Format("<type fullname=\"{0}\" preserve=\"all\"/>", classList[i]);
        }
        System.IO.File.WriteAllLines(Application.dataPath + "/link.xml", GetClassList(classList)) ;
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
    }

    public static List<string> GetClassList(List<string> classList)
    {
        var list = new List<string>();
        list.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?> ");
        list.Add("<linker>");
        list.Add("<assembly fullname =\"DOTween\" preserve=\"all\"/>");//DOTween
        list.Add("<assembly fullname=\"Assembly-CSharp\" preserve=\"all\"/>");//Assembly-CSharp
        list.Add("<assembly fullname=\"UnityGameFramework\" preserve=\"all\"/>");//UnityGameFramework
        list.Add("<assembly fullname=\"UnityEngine\">");//UnityEngine

        list.AddRange(classList);

        list.Add("</assembly>");
        list.Add("</linker>");

        return list;
    }
}