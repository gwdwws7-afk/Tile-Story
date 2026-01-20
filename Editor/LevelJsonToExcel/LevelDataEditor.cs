using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
// using NPOI.SS.UserModel;
// using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

public class LevelEditorData
{
    public int Level;//关卡数
    public int LayerCount;
    public int TotalTileCount;//总棋子数
    public int TotalTypeCount;//总的资源类型数
}

public class LevelDataEditor : Editor
{
    private static int LevelConfigMax => Constant.GameConfig.MaxLevel;

    [MenuItem("Tools/获取关卡数据信息Json")]
    /// <summary>
    /// 获取level json文件
    /// </summary>
    /// <returns></returns>
    public static void GetLevelJson()
    {
        List<LevelEditorData> list = new List<LevelEditorData>();
        int levelNum = 1;
        while (levelNum<=LevelConfigMax)
        {
            EditorUtility.DisplayProgressBar("LevelData", $"进度...({levelNum}/{LevelConfigMax})", (float)levelNum/ (float)LevelConfigMax);
            string path = $"Assets/TileMatch/Res/Level/{levelNum}.json";
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Log.Info($"Path:{path}:{text.text}");
            TileMatch_LevelData data = FromJson(text.text);
            if (data != null)
            {
                list.Add(new LevelEditorData
                {
                    Level = levelNum,
                    LayerCount = data.AllLayerTileDict.Count,
                    TotalTileCount= data.TotalCount,
                    TotalTypeCount= data.TotalItemNum,
                });
                Log.Info($"Level:{levelNum}:{data.TotalCount}:{data.TotalItemNum}");
            }
            levelNum++;
        }
        EditorUtility.ClearProgressBar();

        string filePath = Path.Combine(Application.persistentDataPath, "LevelEditorData.xlsx");
        // GenerateExcelFile(list,filePath);
        SaveJson(list);
    }
    

    public static void SaveJson(List<LevelEditorData> list)
    {
        string path = Application.dataPath+@"\LevelEditorData.json";
        Log.Info($"SaveJson: {path}");
        using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
        {
            var json = ToJson(list);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }
    
    public static string ToJson(List<LevelEditorData> list)
    {
        return JsonConvert.SerializeObject(list,Formatting.Indented);
    }
    public static TileMatch_LevelData FromJson(string content)
    {
        return JsonConvert.DeserializeObject<TileMatch_LevelData>(content);
    }
    
    // public static void GenerateExcelFile(List<LevelEditorData> levelDatas, string filePath)
    // {
    //     IWorkbook workbook = new XSSFWorkbook(); // For .xlsx files
    //     // IWorkbook workbook = new HSSFWorkbook(); // Uncomment for .xls files
    //     ISheet sheet = workbook.CreateSheet("Sheet1");
    //
    //     // Create the header row
    //     IRow headerRow = sheet.CreateRow(0);
    //     headerRow.CreateCell(0).SetCellValue("Name");
    //     headerRow.CreateCell(1).SetCellValue("Age");
    //     headerRow.CreateCell(2).SetCellValue("Email");
    //
    //     // Fill data
    //     for (int i = 0; i < levelDatas.Count; i++)
    //     {
    //         IRow row = sheet.CreateRow(i + 1);
    //         row.CreateCell(0).SetCellValue(levelDatas[i].Level);
    //         row.CreateCell(1).SetCellValue(levelDatas[i].LayerCount);
    //         row.CreateCell(2).SetCellValue(levelDatas[i].TotalTypeCount);
    //         row.CreateCell(2).SetCellValue(levelDatas[i].TotalTileCount);
    //     }
    //
    //     // Auto-size columns
    //     for (int i = 0; i < 3; i++)
    //     {
    //         sheet.AutoSizeColumn(i);
    //     }
    //
    //     using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
    //     {
    //         workbook.Write(stream);
    //     }
    //
    //     workbook.Close();
    // }
}
