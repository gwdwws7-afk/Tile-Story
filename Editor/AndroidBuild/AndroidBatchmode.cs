using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidBatchmode : EditorWindow
{
    public static bool AndroidBuildForEditor(bool isBuildAB, bool isResolve, bool isBuildAAB, int bundleVersionCode,
        bool isJenkins = false, string bundleVersion = "", string buildPath = "")
    {
        SetProjectSetting(bundleVersion, bundleVersionCode, isBuildAAB);
        
        AndroidResolve(isResolve, (success) =>
        {
            if (!success) return;
            if (!SetGoogleSignAAR()) return;
            
            BuildAssetBundle(isBuildAB);
            
            bool result = AndroidBuild(isBuildAAB, isJenkins, buildPath);
            if (result)
            {
                PlayerSettings.Android.bundleVersionCode += 1;
            }
            
            if(isJenkins)
            {
                EditorApplication.Exit(0);
            }
        });
        return true;
    }

    /// <summary>
    /// 命令行调用的打包方法
    /// </summary>
    public static void RunAndroidBuildFromJenkins()
    {
        string[] args = Environment.GetCommandLineArgs();

        bool isBuildAB = true;
        bool isBuildAAB = GetArgValue(args, "-isBuildAAB", false);
        int bundleVersionCode = GetArgValue(args, "-bundleVersionCode", 108);
        string bundleVersion = GetArgValue(args, "-bundleVersion", "0.3.2.1392");
        string buildPath = GetArgValue(args, "-buildPath", "");

        buildPath = GetPath(bundleVersion.ToString(), true, buildPath);
        
        // 设置打包文件的类型
        EditorUserBuildSettings.buildAppBundle = isBuildAAB;
        Debug.Log(
            $"isBuildAB:{isBuildAB}  isBuildAAB:{isBuildAAB}  bundleVersionCode:{bundleVersionCode}  bundleVersion:{bundleVersion}  buildPath:{buildPath}");
        AndroidBuildForEditor(isBuildAB, true, isBuildAAB, bundleVersionCode, true, bundleVersion, buildPath);
    }

    // 通用参数解析工具
    private static T GetArgValue<T>(string[] args, string argName, T defaultValue)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string[] str = args[i].Split('=');
            if (str[0].Equals(argName))
            {
                if (str.Length < 2) return defaultValue;
                string value = str[1];
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 修改Unity Project Settings的设置
    /// </summary>
    /// <param name="bundleVersion">打包的版本号</param>
    /// <param name="bundleVersionCode"></param>
    /// <param name="useKeystore">是否使用Keystore</param>
    public static void SetProjectSetting(string bundleVersion, int bundleVersionCode, bool useKeystore)
    {
        if (bundleVersion != "")
        {
            PlayerSettings.bundleVersion = bundleVersion;
        }

        if (bundleVersionCode > 0)
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }
        
        // keystore或密码设置错误
        PlayerSettings.Android.useCustomKeystore = useKeystore;
        if (useKeystore)
        {
            PlayerSettings.Android.keystoreName = @"Assets/GoogleKeyStore/Google.keystore";
            PlayerSettings.Android.keystorePass = "Yshssjx100";
            PlayerSettings.Android.keyaliasName = "bubble shooter viking pop";
            PlayerSettings.Android.keyaliasPass = "Yshssjx100";
        }
    }

    /// <summary>
    /// 删除不需要的文件
    /// </summary>
    public static void DeleteNoNeedFile()
    {
        string[] path =
        {
            Path.Combine(Application.dataPath, "Amazon"), 
            Path.Combine(Application.dataPath, "MaxSdk"),
        };
        for (int i = 0; i < path.Length; i++)
        {
            if(Directory.Exists(path[i]))
            {
                Directory.Delete(path[i], true);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 重新打AB包，防止冲突，会将旧的AB包删除
    /// </summary>
    /// <param name="buildAB">是否需要重新生成AB包</param>
    private static void BuildAssetBundle(bool buildAB)
    {
        if (!buildAB) return;
        AddressableAssetSettings.BuildPlayerContent();
    }

    public static void AndroidResolve(bool isResolve, Action<bool> completeAction)
    {
        if (!isResolve)
        {
            completeAction?.Invoke(true);
            return;
        }
        // 调用 Android Resolver 的 Delete Resolved Libraries 功能
        GooglePlayServices.PlayServicesResolver.DeleteResolvedLibraries(() =>
        {
            // 调用 Android Resolver 的 Resolve 功能
            GooglePlayServices.PlayServicesResolver.Resolve(null, false, completeAction);
        });
    }
    
    /// <summary>
    /// Android打包
    /// </summary>
    /// <param name="buildAAB">生成包的类型</param>
    /// <param name="isJenkins">是否来自Jenkins</param>
    /// <param name="bundleVersionCode"></param>
    /// <param name="path">生成包的存放路径</param>
    /// <returns>打包的结果</returns>
    private static bool AndroidBuild(bool buildAAB, bool isJenkins, string path)
    {
        if(!isJenkins) ClearConsole();
        var scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
        string bundleName = PlayerSettings.Android.bundleVersionCode.ToString();
        if (path == "") path = GetPath(bundleName, isJenkins, path);
        if (path == null || path == "")
        {
            Debug.LogError($"获取的打包文件存放路径为空");
            return false;
        }
        
        BuildReport report = BuildPipeline.BuildPlayer(scenes, Path.Combine(path, $"{bundleName}.{(buildAAB ? "aab":"apk")}"), BuildTarget.Android, BuildOptions.None);
        return report.summary.result == BuildResult.Succeeded;
    }

    static System.Reflection.MethodInfo clearMethod = null;
    /// <summary>
    /// 清空控制台日志
    /// </summary>
    public static void ClearConsole()
    {
        if (clearMethod == null)
        {
            Type log = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");

            clearMethod = log.GetMethod("Clear");
        }
        clearMethod.Invoke(null, null);
    }

    /// <summary>
    /// 获取项目存放路径
    /// </summary>
    /// <param name="buildName">生成包的名称</param>
    /// <param name="isJenkins">是否来自Jenkins</param>
    /// <param name="path">生成包的存放路径</param>
    /// <returns>存放路径</returns>
    private static string GetPath(string buildName, bool isJenkins = false, string path = "")
    {
        if (path != null && path != "")
        {
            string path1 = Path.Combine(path, "BuildAndroid", $"{buildName}");
            if (Directory.Exists(path1))
            {
                Directory.Delete(path1, true);
            }
            Directory.CreateDirectory(path1);
            return path1;
        }
        
        string parentDirectory = "";
        if (isJenkins)
        {
            // 获取Asset的上级目录
            parentDirectory = Path.GetDirectoryName(Application.dataPath);

            if (string.IsNullOrEmpty(parentDirectory))
            {
                return null;
            }
            Debug.Log($"{parentDirectory}");
        }
        else
        {
            parentDirectory = Path.GetFullPath($@"../../");
            Debug.Log($"{parentDirectory}");
        }
        string buildPath = Path.Combine(parentDirectory, "BuildAndroid", $"{buildName}");
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }
        Directory.CreateDirectory(buildPath);
        return buildPath;
    }

    private static bool SetGoogleSignAAR()
    {
        string path = @"Assets/GeneratedLocalRepo/GoogleSignIn/Editor/m2repository/com/google/signin/google-signin-support/1.0.4/google-signin-support-1.0.4.aar";
        
        try
        {
            // AssetImporter.GetAtPath 需要相对于 Assets 文件夹的路径，而不是文件系统的绝对路径。
            PluginImporter pluginImporter = AssetImporter.GetAtPath(path) as PluginImporter;
        
            if (pluginImporter == null)
            {
                Debug.LogError($"未找到AAR 文件: {path}");
                return true;
            }
            
            // 设置 Android 平台兼容性
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, true);
        
            // 保存并重新导入
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AssetImporter.GetAtPath has question。{e}");
            return false;
        }
    }
}
