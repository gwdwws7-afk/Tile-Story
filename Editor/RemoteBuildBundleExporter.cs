#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class RemoteBuildBundleExporter
{
    [System.Serializable]
    public class RemoteBundleInfo
    {
        public string bundleName;
        public long size;
    }

    [System.Serializable]
    public class RemoteBundleList
    {
        public List<RemoteBundleInfo> bundles = new List<RemoteBundleInfo>();
    }

    [MenuItem("Tools/Addressables/导出 RemoteBuildPath 资源列表")]
    public static void ExportRemoteBundles()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string outputRoot = $"Library/com.unity.addressables/aa/{buildTarget}";

        var remoteBundleList = new RemoteBundleList();

        foreach (var group in settings.groups)
        {
            if (group == null || group.entries.Count == 0)
                continue;

            // 找出使用 RemoteBuildPath 的组
            var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundleSchema == null || !bundleSchema.BuildPath.GetValue(settings).Contains("Remote"))
                continue;

            string groupBuildPath = bundleSchema.BuildPath.GetValue(settings);
            string groupBuildFullPath = Path.Combine(outputRoot); // 实际生成都在 outputRoot

            // 遍历输出目录中所有 .bundle 文件
            string[] bundleFiles = Directory.GetFiles(groupBuildFullPath, "*.bundle", SearchOption.AllDirectories);

            foreach (var path in bundleFiles)
            {
                string fileName = Path.GetFileName(path);
                long size = new FileInfo(path).Length;

                remoteBundleList.bundles.Add(new RemoteBundleInfo
                {
                    bundleName = fileName,
                    size = size
                });
            }
        }

        // 保存到 Resources 或 StreamingAssets
        string json = JsonUtility.ToJson(remoteBundleList, true);
        string exportPath = Path.Combine(Application.dataPath, "Resources/remote_bundles.json");
        File.WriteAllText(exportPath, json);

        AssetDatabase.Refresh();

        Debug.Log($"✅ RemoteBuildPath 资源导出完成：共 {remoteBundleList.bundles.Count} 个包\n路径：{exportPath}");
    }
}
#endif
