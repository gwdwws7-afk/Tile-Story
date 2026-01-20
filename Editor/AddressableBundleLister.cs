using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;


[InitializeOnLoad]
public class AddressableBundleGenerator
{
    // 静态构造函数确保第一时间注册事件
    static AddressableBundleGenerator()
    {
        EditorApplication.delayCall += () =>
        {
            BuildScript.buildCompleted -= OnBuildCompleted;
            BuildScript.buildCompleted += OnBuildCompleted;
        };
    }
    
    private static void OnBuildCompleted(AddressableAssetBuildResult result)
    {
        if (result == null || !string.IsNullOrEmpty(result.Error)) return;
        
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        // 获取 catalog.json 路径
        HashSet<string> bundleNames = new  HashSet<string>();
         foreach (var group in settings.groups)
         {
             //如果组是打包没打到一起，则添加组中所有对象
             var schema = group.GetSchema<BundledAssetGroupSchema>();
             if (schema == null)
             {
                 Debug.Log($"组：{group.Name}（非 Bundle 类型组，无 BundledAssetGroupSchema）");
                 continue;
             }

             if (!schema.IncludeInBuild)
             {
                 continue;
             }

             if (IsLocalGroup(group))
             {
                 Debug.Log($"本地组：{group.Name}，跳过");
                 continue;
             }

             var mode = schema.BundleMode; // enum BundlePackingMode
             Debug.Log($"组：{group.Name}，BundleMode：{mode}");

             if (mode == BundledAssetGroupSchema.BundlePackingMode.PackTogether)
             {
                 var firstAB= group.entries.FirstOrDefault();
                 if (firstAB != null)
                 {
                     bundleNames.Add(group.Name);
                     Debug.Log($"   ▶ Address: {firstAB.address}, AssetPath: {firstAB.AssetPath}");
                 }
             }
             else if(mode == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
             {
                 foreach (var entry in group.entries)
                 {
                     bundleNames.Add(entry.address);
                     Debug.Log($"   ▶ Address: {entry.address}, AssetPath: {entry.AssetPath}");
                 }
             }
         }
     
        // 生成 JSON 到 Resources
        SaveJson(bundleNames);

        CheckAllLevelJson();
    }
    
    /// <summary>
    /// 检查所有关卡是否合理
    /// </summary>
    private static void CheckAllLevelJson()
    {
        CheckLevelJsonByPreBuild.CheckValidByCheckAllLevelJson();
    }

    public static void SaveJson( HashSet<string> bundleNames)
    {
        string path = Application.dataPath+@"\Resources\addressableBundles.json";
        Log.Info($"SaveJson: {path}");
        using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate))
        {
            var json = ToJson(bundleNames);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }
    
    public static string ToJson( HashSet<string> list)
    {
        return JsonConvert.SerializeObject(list,Formatting.Indented);
    }
    public static  HashSet<string> FromJson(string content)
    {
        return JsonConvert.DeserializeObject< HashSet<string>>(content);
    }
    
    public static bool IsRemoteGroup(BundledAssetGroupSchema schema)
    {
        return false;
    }
    //
    public static bool IsRemoteGroup(AddressableAssetGroup group)
    {
        // 获取组的打包模式schema
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema == null)
        {
            Debug.LogError("BundledAssetGroupSchema not found for group: " + group.name);
            return false;
        }

        if (!schema.IncludeInBuild) return false;
    
        // 获取Addressable设置
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable settings not found!");
            return false;
        }
    
        // 获取远程路径标识符
        string remoteLoadPathId = settings.profileSettings.GetProfileDataById(AddressableAssetSettings.kRemoteLoadPath).ProfileName;
        
        // 判断构建路径和加载路径是否为远程路径
        bool isRemoteLoadPath = schema.LoadPath.Id == remoteLoadPathId;
    
        // 只有当构建路径和加载路径都是远程路径时，才是真正的远程组
        return isRemoteLoadPath;
    }
    
    public static bool IsLocalGroup(AddressableAssetGroup group)
    {
        if (group == null) return false;
        
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema == null) return false;
        
        // 获取路径的实际值
        string buildPath = schema.BuildPath.GetValue(group.Settings);
        
        // 本地路径的典型特征
        bool isRemoteBuildPath = buildPath.Contains("ServerData");
        
        return !isRemoteBuildPath;
    }
}