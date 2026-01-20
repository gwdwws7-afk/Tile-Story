// Save as: Assets/Editor/Misc/GenerateTotalCountArray.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateTotalCountArray : EditorWindow
{
    private string mainFolderPath = "";
    private string levelAFolderPath = "";
    private string levelBFolderPath = "";
    private string outputScriptPath = "Assets/Scripts/GeneratedTotalCounts.cs";

    [MenuItem("Tools/Generate TotalCount Array")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<GenerateTotalCountArray>();
        wnd.titleContent = new GUIContent("Generate TotalCount Array");
        wnd.minSize = new Vector2(620, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("扫描文件夹并生成静态 TotalCount 数组与 Level 字典（按文件名数字排序）", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Main folder
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("主文件夹 (生成 Counts/FileNames)：", GUILayout.Width(260));
        mainFolderPath = EditorGUILayout.TextField(mainFolderPath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择主文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(picked)) { mainFolderPath = picked; Repaint(); }
        }
        EditorGUILayout.EndHorizontal();

        // Level A folder
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LevelA 文件夹 (生成 LevelADict)：", GUILayout.Width(260));
        levelAFolderPath = EditorGUILayout.TextField(levelAFolderPath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择 LevelA 文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(picked)) { levelAFolderPath = picked; Repaint(); }
        }
        EditorGUILayout.EndHorizontal();

        // Level B folder
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LevelB 文件夹 (生成 LevelBDict)：", GUILayout.Width(260));
        levelBFolderPath = EditorGUILayout.TextField(levelBFolderPath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择 LevelB 文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(picked)) { levelBFolderPath = picked; Repaint(); }
        }
        EditorGUILayout.EndHorizontal();

        // Output path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输出脚本路径：", GUILayout.Width(120));
        outputScriptPath = EditorGUILayout.TextField(outputScriptPath);
        if (GUILayout.Button("定位到Assets", GUILayout.Width(100)))
        {
            string defaultDir = "Assets/Scripts";
            if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);
            outputScriptPath = Path.Combine(defaultDir, "GeneratedTotalCounts.cs");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("开始生成", GUILayout.Height(32)))
        {
            if (string.IsNullOrEmpty(mainFolderPath) && string.IsNullOrEmpty(levelAFolderPath) && string.IsNullOrEmpty(levelBFolderPath))
            {
                EditorUtility.DisplayDialog("错误", "请至少选择一个要扫描的文件夹（主文件夹或 LevelA 或 LevelB）。", "OK");
                return;
            }

            try
            {
                GenerateAll(mainFolderPath, levelAFolderPath, levelBFolderPath, outputScriptPath);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("完成", $"生成成功：{outputScriptPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError("生成失败: " + ex);
                EditorUtility.DisplayDialog("失败", "生成过程中出现异常，请查看 Console。", "OK");
            }
        }
    }

    // 用于 JsonUtility 快速解析 TotalCount
    [Serializable]
    private class JsonRootForTotal
    {
        public int TotalCount = 0;
    }

    private static void GenerateAll(string mainFolder, string levelAFolder, string levelBFolder, string outputPath)
    {
        // 主文件夹解析（如果提供）
        int[] mainCounts = new int[0];
        string[] mainFileNames = new string[0];
        if (!string.IsNullOrEmpty(mainFolder) && Directory.Exists(mainFolder))
        {
            var mainFiles = GetJsonFilesSortedByNumericName(mainFolder);
            var countsList = new List<int>();
            var namesList = new List<string>();

            for (int i = 0; i < mainFiles.Length; i++)
            {
                string f = mainFiles[i];
                string text = File.ReadAllText(f, Encoding.UTF8);
                countsList.Add(ExtractTotalCount(text));
                namesList.Add(Path.GetFileName(f));
                if (EditorUtility.DisplayCancelableProgressBar("生成 TotalCount 数组", $"处理 主文件夹: {Path.GetFileName(f)} ({i+1}/{mainFiles.Length})", (i+1f)/mainFiles.Length))
                {
                    EditorUtility.ClearProgressBar();
                    throw new OperationCanceledException("用户取消操作。");
                }
            }

            EditorUtility.ClearProgressBar();
            mainCounts = countsList.ToArray();
            mainFileNames = namesList.ToArray();
        }

        // LevelA 解析为字典<int,int>
        var levelADict = new SortedDictionary<int, int>();
        if (!string.IsNullOrEmpty(levelAFolder) && Directory.Exists(levelAFolder))
        {
            var filesA = GetJsonFilesSortedByNumericName(levelAFolder);
            for (int i = 0; i < filesA.Length; i++)
            {
                string f = filesA[i];
                string text = File.ReadAllText(f, Encoding.UTF8);
                int total = ExtractTotalCount(text);
                int key = GetNumericFileName(f);
                if (key != int.MinValue)
                {
                    levelADict[key] = total;
                }
                if (EditorUtility.DisplayCancelableProgressBar("生成 LevelA 字典", $"处理 LevelA: {Path.GetFileName(f)} ({i+1}/{filesA.Length})", (i+1f)/filesA.Length))
                {
                    EditorUtility.ClearProgressBar();
                    throw new OperationCanceledException("用户取消操作。");
                }
            }
            EditorUtility.ClearProgressBar();
        }

        // LevelB 解析为字典<int,int>
        var levelBDict = new SortedDictionary<int, int>();
        if (!string.IsNullOrEmpty(levelBFolder) && Directory.Exists(levelBFolder))
        {
            var filesB = GetJsonFilesSortedByNumericName(levelBFolder);
            for (int i = 0; i < filesB.Length; i++)
            {
                string f = filesB[i];
                string text = File.ReadAllText(f, Encoding.UTF8);
                int total = ExtractTotalCount(text);
                int key = GetNumericFileName(f);
                if (key != int.MinValue)
                {
                    levelBDict[key] = total;
                }
                if (EditorUtility.DisplayCancelableProgressBar("生成 LevelB 字典", $"处理 LevelB: {Path.GetFileName(f)} ({i+1}/{filesB.Length})", (i+1f)/filesB.Length))
                {
                    EditorUtility.ClearProgressBar();
                    throw new OperationCanceledException("用户取消操作。");
                }
            }
            EditorUtility.ClearProgressBar();
        }

        // 生成最终脚本
        string scriptText = BuildOutputScript(mainCounts, mainFileNames, levelADict, levelBDict);

        string outDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        File.WriteAllText(outputPath, scriptText, Encoding.UTF8);
        Debug.Log($"✅ 生成完成：{outputPath}");
    }

    // 获取文件并按文件名数字排序（无法解析为数字的放到最后）
    private static string[] GetJsonFilesSortedByNumericName(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                return ext == ".json" || ext == ".txt";
            })
            .ToArray();

        return files.OrderBy(f =>
        {
            int n = GetNumericFileName(f);
            return n == int.MinValue ? int.MaxValue : n;
        }).ToArray();
    }

    // 从文件名（无扩展名）尝试解析数字，失败返回 int.MinValue
    private static int GetNumericFileName(string filePath)
    {
        string name = Path.GetFileNameWithoutExtension(filePath);
        if (int.TryParse(name, out int num)) return num;
        // 如果文件名可能包含前缀/suffix，可以做额外的正则抽取数字（这里保持简单）
        var m = Regex.Match(name, @"-?(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out int v)) return v;
        return int.MinValue;
    }

    // 提取 TotalCount（先 JsonUtility，再正则）
    private static int ExtractTotalCount(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText)) return 0;

        try
        {
            var parsed = JsonUtility.FromJson<JsonRootForTotal>(jsonText);
            if (parsed != null && jsonText.IndexOf("\"TotalCount\"", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return parsed.TotalCount;
            }
        }
        catch
        {
            // ignore and fallback
        }

        var regex = new Regex(@"""TotalCount""\s*:\s*(-?\d+)", RegexOptions.IgnoreCase);
        var m = regex.Match(jsonText);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int v))
        {
            return v;
        }

        // 如果没找到，返回 0 并记录一次警告（避免大量重复日志，可在需要时改为只在调试打开时打印）
        Debug.LogWarning("未在 json 中找到 TotalCount 字段，返回 0。片段预览: " + TruncateForLog(jsonText, 200));
        return 0;
    }

    private static string TruncateForLog(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }

    // 生成 C# 输出脚本文本（包含 Counts/FileNames/LevelADict/LevelBDict）
    private static string BuildOutputScript(int[] counts, string[] fileNames, SortedDictionary<int,int> levelA, SortedDictionary<int,int> levelB)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// 自动生成，不要手动修改（可在需要时重新生成）");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("public static class GeneratedTotalCounts");
        sb.AppendLine("{");

        // 主 Counts & FileNames
        sb.AppendLine("    // 主文件夹按文件名数字升序得到的 Counts 与 FileNames");
        sb.AppendLine("    public static readonly int[] Counts = new int[] { " + (counts.Length > 0 ? string.Join(", ", counts) : "") + " };");
        var escapedNames = fileNames.Select(fn => "\"" + fn.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
        sb.AppendLine("    public static readonly string[] FileNames = new string[] { " + (fileNames.Length > 0 ? string.Join(", ", escapedNames) : "") + " };");
        sb.AppendLine();

        // LevelA dictionary
        sb.AppendLine("    // LevelA: 键 = 关卡编号（文件名数字），值 = TotalCount");
        sb.AppendLine("    public static readonly Dictionary<int,int> LevelADict = new Dictionary<int,int>()");
        sb.AppendLine("    {");
        if (levelA != null && levelA.Count > 0)
        {
            foreach (var kv in levelA)
            {
                sb.AppendLine($"        {{ {kv.Key}, {kv.Value} }},");
            }
        }
        sb.AppendLine("    };");
        sb.AppendLine();

        // LevelB dictionary
        sb.AppendLine("    // LevelB: 键 = 关卡编号（文件名数字），值 = TotalCount");
        sb.AppendLine("    public static readonly Dictionary<int,int> LevelBDict = new Dictionary<int,int>()");
        sb.AppendLine("    {");
        if (levelB != null && levelB.Count > 0)
        {
            foreach (var kv in levelB)
            {
                sb.AppendLine($"        {{ {kv.Key}, {kv.Value} }},");
            }
        }
        sb.AppendLine("    };");
        sb.AppendLine();

        // 辅助方法
        sb.AppendLine("    public static int GetCountByFileName(string fileName)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.IsNullOrEmpty(fileName)) return -1;");
        sb.AppendLine("        for (int i = 0; i < FileNames.Length; i++)");
        sb.AppendLine("            if (string.Equals(FileNames[i], fileName, StringComparison.InvariantCultureIgnoreCase)) return Counts[i];");
        sb.AppendLine("        return -1;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public static int GetLevelACount(int levelNumber)");
        sb.AppendLine("    {");
        sb.AppendLine("        return LevelADict != null && LevelADict.TryGetValue(levelNumber, out int v) ? v : -1;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public static int GetLevelBCount(int levelNumber)");
        sb.AppendLine("    {");
        sb.AppendLine("        return LevelBDict != null && LevelBDict.TryGetValue(levelNumber, out int v) ? v : -1;");
        sb.AppendLine("    }");

        sb.AppendLine("}"); // end class

        return sb.ToString();
    }
}
