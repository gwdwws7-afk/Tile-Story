using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets;
using System;

public class ResImportEditor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        ImportRes((str) =>
        {
            return (str.EndsWith(".json")) && (str.Contains("TileMatch/Res/Level"));
        }, "Level", false,"", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".json")) && (str.Contains("TileMatch/Res/Level_A"));
        }, "Level_A", false, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".json")) && (str.Contains("TileMatch/Res/Level_B"));
        }, "Level_B", false, "", importedAssets, movedAssets);
        
        ImportRes((str) =>
        {
            return (str.EndsWith(".json")) && (str.Contains("TileMatch/Res/Level_Challenge_Dif"));
        }, "Level_Challenge_Dif", false, "", importedAssets, movedAssets);
        
        ImportRes((str) =>
        {
            return str.EndsWith(".txt") && str.Contains("GameMain/Data/LevelsData");
        }, "Data", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".ogg")||str.EndsWith(".wav")|| str.EndsWith(".mp3")) && str.Contains("GameMain/Sounds");
        }, "Sounds", true, "", importedAssets, movedAssets);

        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".prefab")) && (str.Contains("TileMatch/Res/Prefab/UI/UICommon/Button"));
        //}, "Common", true, "", importedAssets, movedAssets);

        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".prefab")) && (str.Contains("TileMatch/Res/Prefab/UI/Map"));
        //}, "MapUI", true, "", importedAssets, movedAssets);

        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".prefab")) && (str.Contains("TileMatch/Res/Prefab/UI/Menu"));
        //}, "MenuUI",true,"", importedAssets, movedAssets);

        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".prefab")) && (str.Contains("TileMatch/Res/Prefab/UI/Shop"));
        //}, "ShopUI", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".png")||str.EndsWith(".jpg")) && (str.Contains("TileMatch/Res/UI/NoAtlas_UI/BG_Big"));
        }, "BGBigImage",true, "BG_", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".json")) && (str.Contains("GameMain/Data"));
        }, "Data", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".png")) && (str.Contains("TileMatch/Res/UI/NoAtlas_UI/HeadPortrait"));
        }, "HeadPortrait", true, "HeadPortrait_", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".prefab")) && (str.Contains("TileMatch/Res/Prefab/EffectPrefab"));
        }, "Effect", true, "", importedAssets, movedAssets);
        
        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".png")) && (str.Contains("TileMatch/Res/UI/NoAtlas_UI/StoryAvator"));
        //}, "StoryAvator", true, "", importedAssets, movedAssets);
        
        ImportRes((str) =>
        {
            return (str.EndsWith(".png")) && (str.Contains("TileMatch/Res/UI/NoAtlas_UI/AA-UICommonBigImage"));
        }, "NoAtlasImage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/NotoSeriDevanagari"));
        }, "HindiLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/BANGOPRO SDF"));
        }, "DefaultLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/Black_Han_Sans"));
        }, "KoreanLanguage", true, "", importedAssets, movedAssets);

        //ImportRes((str) =>
        //{
        //    return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/DelaGothicOne"));
        //}, "JapaneseLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/Lalezar"));
        }, "ArabicLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/Mitr-SemiBold"));
        }, "ThaiLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/SOURCEHANSANSCN-HEAVY"));
        }, "ChineseLanguage", true, "", importedAssets, movedAssets);

        ImportRes((str) =>
        {
            return (str.EndsWith(".mat")) && (str.Contains("GameMain/Fonts/SOURCEHANSANSCN-CN"));
        }, "CNLanguage", true, "", importedAssets, movedAssets);

        //ImportResForDecorationItemPrefab((str) =>
        //{
        //    return (str.EndsWith(".prefab")) && (str.Contains("GameMain/Prefabs/General/Decoration/DecorationAreas/"));
        //}, true, "", importedAssets, movedAssets);

        DeletedLevelAssets(deletedAssets);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    private static void DeletedLevelAssets(string[] deletedAssets)
    {
        if (deletedAssets == null|| deletedAssets.Length<=0) return;

        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach (var str in deletedAssets)
        {
            if (str.EndsWith(".json") && str.Contains("TileMatch/Res/Level/"))
            {
                var simpName = Path.GetFileNameWithoutExtension(str);
                if (int.TryParse(simpName,out int levelNum))
                {
                    string key = Path.GetDirectoryName(str);
                    if (dict.ContainsKey(key))
                    {
                        dict[key]=Mathf.Min(levelNum,dict[key]);
                    }else
                        dict.Add(Path.GetDirectoryName(str), levelNum);
                }
            }
        }
        foreach (var path in dict)
        {
            int curNum = path.Value;
            int index = path.Value;
            while (index<=5000)
            {
                string nowPath =$"{path.Key}/{index}.json";
                if (System.IO.File.Exists(nowPath))
                {
                    UnityEditor.AssetDatabase.RenameAsset(nowPath, $"{curNum}.json");
                    curNum++;
                }
                index++;
            }
        }
    }

    private static void ImportRes(System.Predicate<string> action,string groupName,bool isSimplifyName=true,string prefix="", params string[][] importedAssets)
    {
        if (importedAssets == null || importedAssets.Length <= 0) return;

        var setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
        var group = setting.FindGroup(groupName);

        if (importedAssets != null)
        {
            foreach (var paths in importedAssets)
            {
                foreach (var str in paths)
                {
                    if (action.Invoke(str))
                    {
                        //���GUID
                        var guid = AssetDatabase.AssetPathToGUID(str);
                        //ͨ��GUID����entry
                        var entry = setting.CreateOrMoveEntry(guid, group);

                        //entry.SetLabel(groupName, true);

                        var addressName =isSimplifyName? prefix + Path.GetFileNameWithoutExtension(str):str;

                        entry.SetAddress(addressName, true);

                        Log.Debug($"����{str} addressable is group:{groupName};name:{addressName}");
                    }
                }
            }
        }
    }

    //和ImportRes相比 groupname通过匹配获得
    private static void ImportResForDecorationItemPrefab(System.Predicate<string> action, bool isSimplifyName = true, string prefix = "", params string[][] importedAssets)
    {
        if (importedAssets == null || importedAssets.Length <= 0) return;

        if (importedAssets != null)
        {
            foreach (string[] paths in importedAssets)
            {
                foreach (string str in paths)
                {
                    if (action.Invoke(str))
                    {
                        //获取GroupName
                        string startStr = "GameMain/Prefabs/General/Decoration/DecorationAreas/Area";
                        string endString = "/Area";
                        Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=(" + endString + "))");
                        Match match = rg.Match(str);

                        if (match.Success)
                        {
                            string groupName = $"DecorationArea_{match.Value}";
                            var setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");

                            var group = setting.FindGroup(groupName);
                            if (group != null)
                            {
                                var guid = AssetDatabase.AssetPathToGUID(str);

                                var entry = setting.CreateOrMoveEntry(guid, group);

                                //entry.SetLabel(groupName, true);

                                var addressName = isSimplifyName ? prefix + Path.GetFileNameWithoutExtension(str) : str;

                                entry.SetAddress(addressName, true);

                                Log.Debug($"����{str} addressable is group:{groupName};name:{addressName}");
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("BubbleTools/Sound/CreateJsonTxtByNoUnloadAudio")]
    public static void CreateJsonTxtByNoUnloadAudio()
    {
        string path =  Application.dataPath+"/GameMain/Sounds/NoUnloadTileAudio";
        List<string> nameList= GetAllFilesAndDertorys(path);
        SaveLocal("NoUnloadSounds",nameList);
    }
    
    public static void SaveLocal(string fileName,List<string> noUnloadSoundNames)
    {
#if UNITY_EDITOR
        if (Application.platform == RuntimePlatform.WindowsEditor||Application.platform==RuntimePlatform.OSXEditor)
        {
            string path = Application.dataPath+$"/GameMain/Data/{fileName}.json";
            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                var json = JsonConvert.SerializeObject(noUnloadSoundNames,Formatting.Indented);
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
                fileStream.Write(byteArray, 0, byteArray.Length);
                fileStream.Close();
            }
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
    
    private static List<string> GetAllFilesAndDertorys(string path)
    {
        List<string> strs = new List<string>();
        //判断路径是否存在
        if (Directory.Exists(path))
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            //获取目标路径下的单层文件
            FileInfo[] files = dir.GetFiles("*");
 
            foreach(var item in files)
            {
                if (item.Name.EndsWith(".meta")) continue;
                //返回相对路径
                string assetsName =Path.GetFileNameWithoutExtension(item.Name);
                strs.Add(assetsName);
                
                Log.Debug($"GetAllFilesAndDertorys:{assetsName}");
            }
        }

        return strs;
    }
}

public static class LevelAddressableChecker
{
    private static Dictionary<string, string> AllLevelPaths = new Dictionary<string, string>()
    {
        {"Level","Assets/TileMatch/Res/Level"},
        {"Level_A","Assets/TileMatch/Res/Level_A"},
        {"Level_B","Assets/TileMatch/Res/Level_B"},
    };

    [MenuItem("Tools/Check Missing Level Json in Addressables")]
    public static void CheckAndAddMissingJson()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;

        int addedCount = 0;

        foreach (var kv in AllLevelPaths)
        {
            var paths = GetJsonFiles(kv.Value);
            var group = setting.FindGroup(kv.Key);
            if (group == null)
            {
                Debug.LogWarning($"⚠️ 找不到 Addressables Group: {kv.Key}");
                continue;
            }

            foreach (var path in paths)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);

                // 判断 entry 是否已经存在
                var entry = setting.FindAssetEntry(guid);
                if (entry != null)
                {
                    // 已经在某个 group 里了，跳过
                    if (entry.parentGroup == group && entry.address == path)
                    {
                        // 已经在目标 group 里 → 跳过
                        continue;
                    }
                    else
                    {
                        // 已经在别的 group 里 → 移动到当前 group
                        setting.MoveEntry(entry, group);
                        entry.SetAddress(path, true);
                        Debug.Log($"♻️ 资源 {path} 已存在于其他 Group，已移动到 {group.name}");
                        continue;
                    }
                }

                // 🚀 只在缺少时添加
                entry = setting.CreateOrMoveEntry(guid, group);

                //entry.SetLabel(kv.Key, true);

                // 用文件名作为 address
                entry.SetAddress(path, true);

                Debug.Log($"✅ 添加缺失 Json：{path} -> group:{kv.Key}, address:{path}");
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"🎯 查漏补缺完成，共新增 {addedCount} 个 json 到 Addressables。");
    }

    public static List<string> GetJsonFiles(params string[] directories)
    {
        List<string> results = new List<string>();

        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Debug.LogWarning($"⚠️ 目录不存在: {dir}");
                continue;
            }

            string[] files = Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string assetPath = file.Replace("\\", "/");
                if (assetPath.StartsWith(Application.dataPath))
                {
                    assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                }
                results.Add(assetPath);
            }
        }

        // 自然排序
        results.Sort(NaturalCompare);

        return results;
    }

    // 自然排序比较函数
    private static int NaturalCompare(string a, string b)
    {
        string nameA = Path.GetFileNameWithoutExtension(a);
        string nameB = Path.GetFileNameWithoutExtension(b);

        var regex = new Regex(@"\d+|\D+");
        var partsA = regex.Matches(nameA);
        var partsB = regex.Matches(nameB);

        int len = Mathf.Min(partsA.Count, partsB.Count);
        for (int i = 0; i < len; i++)
        {
            string sA = partsA[i].Value;
            string sB = partsB[i].Value;

            int nA, nB;
            bool isNumA = int.TryParse(sA, out nA);
            bool isNumB = int.TryParse(sB, out nB);

            int cmp = 0;
            if (isNumA && isNumB)
            {
                cmp = nA.CompareTo(nB);
            }
            else
            {
                cmp = string.Compare(sA, sB, StringComparison.Ordinal);
            }

            if (cmp != 0)
                return cmp;
        }

        // 如果前面部分都相等，则按长度排序
        return partsA.Count.CompareTo(partsB.Count);
    }
}
