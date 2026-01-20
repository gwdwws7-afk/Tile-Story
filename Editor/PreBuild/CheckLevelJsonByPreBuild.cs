using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CheckLevelJsonByPreBuild
{
    /// <summary>
    /// 关卡是否有效
    /// </summary>
    /// <returns></returns>
    public static void CheckValidByCheckAllLevelJson()
    {
        Dictionary<string, List<string>> recordDict = new Dictionary<string, List<string>>();
        var dict = GetJsonTextGrouped();
        if (dict != null)
        {
            foreach (var data in dict)
            {
                foreach (var file in data.Value)
                {
                    var isValid = IsValidByLevelJson(file.Value);
                    if (!isValid)
                    {
                        if (!recordDict.ContainsKey(data.Key))recordDict.Add(data.Key,new List<string>());
                        recordDict[data.Key].Add(file.Key);
                    }
                }
            }

            if (recordDict.Count > 0)
            {
                string str = null;
                string strContent = null;
                foreach (var data in recordDict)
                {
                    strContent += $"目录：{data.Key}:::关卡";
                    foreach (var content in data.Value)
                    {
                        strContent +=content.ToString();
                    }
                    str += strContent + "\n";
                    strContent = null;
                }
                DisplayDialog(str);
            }

            string dialog = null;
            foreach (var group in dict)
            {
                string content=CheckValueDuplicates(group.Key,group.Value);
                if (!string.IsNullOrEmpty(content))
                {
                    dialog+=content+"\n";
                }
            }
            if(dialog!=null) DisplayDialogBySameLevel(dialog);
        }
    }

    private static bool IsValidByLevelJson(string levelJson)
    {
        var data = TileMatch_LevelData.FromJson(levelJson);
        if (data == null)
        {
            return false;
        }
        else
        {
            int fireworkCount = 0;
            foreach (var layer in data.AllLayerTileDict)
            {
                foreach (var map in layer.Value)
                {
                    if (map.Value.TileID == 20)
                        fireworkCount++;
                }
            }

            return data.TotalItemNum > 0 &&
                   data.TotalCount >= 9 &&
                   (data.TotalCount - fireworkCount) % 3 == 0;
        }
    }
    
    public static string CheckValueDuplicates(string dirName, Dictionary<string, string> dict)
    {
        if (dict == null || dict.Count == 0)
        {
            Debug.Log("字典为空");
            return null;
        }

        // 按 value 分组并排序
        var grouped = dict.GroupBy(kv => kv.Value)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        StringBuilder sb = new StringBuilder();
        sb.Append($"{dirName}::\n");

        bool hasDuplicate = false; // 标记是否有重复

        foreach (var group in grouped)
        {
            if (group.Count() <= 1) continue; // 只处理重复的组

            hasDuplicate = true;
            var keys = string.Join(",", group.Select(kv => kv.Key));
            sb.Append(keys + "\n");
        }

        if (!hasDuplicate)
        {
            return null; // 没有重复，返回空
        }

        return sb.ToString();
    }
    
    private static void DisplayDialogBySameLevel(string str)
    {
        EditorUtility.DisplayDialog($"关卡相同", str, "确定");
    }

    private static void DisplayDialog(string str)
    {
        EditorUtility.DisplayDialog("有异常关卡", str, "确定");
    }

    private static Dictionary<string, Dictionary<string, string>> GetJsonTextGrouped()
    {
        string rootPath = Application.dataPath + "/TileMatch/Res";
        
        if (!Directory.Exists(rootPath))
        {
            Debug.LogWarning("目录不存在: " + rootPath);
            return null;
        }

        // 获取带 "Level" 的目录
        var levelDirs = Directory.GetDirectories(rootPath, "*Level*", SearchOption.AllDirectories);

        // 分类存储： key = 父目录名，value = (文件名->内容)
        Dictionary<string, Dictionary<string, string>> groupedJsonTexts = new Dictionary<string, Dictionary<string, string>>();

        foreach (var dir in levelDirs)
        {
            var files = Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta"))
                .ToList();

            if (files.Count > 0)
            {
                string dirName = Path.GetFileName(dir); // 父目录名字，例如 Level1

                // 排序：按文件名排序
                var sortedFiles = files
                    .Select(f => new { FileName = Path.GetFileName(f), Path = f })
                    .OrderBy(f => f.FileName, System.StringComparer.OrdinalIgnoreCase);

                Dictionary<string, string> jsonTextDict = new Dictionary<string, string>();

                foreach (var file in sortedFiles)
                {
                    try
                    {
                        string text = File.ReadAllText(file.Path);
                        jsonTextDict[file.FileName] = text;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"读取失败: {file.Path}, 错误: {e.Message}");
                    }
                }

                groupedJsonTexts[dirName] = jsonTextDict;
            }
        }

        return groupedJsonTexts;
    }

}