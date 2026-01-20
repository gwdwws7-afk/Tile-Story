#if UNITY_IOS
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class iOSBatchmode : EditorWindow
{
    public static void iOSBuildForEditor(bool buildAB, bool setXcode, string buildNumber)
    {
        BuildAssetBundle(buildAB);
        
        IosBuild(buildNumber);

        SetXcodeProcess(setXcode, buildNumber);
    }

    private static void BuildAssetBundle(bool buildAB)
    {
        if (!buildAB) return;
        AddressableAssetSettings.BuildPlayerContent();
    }

    public static void DeleteNoNeedFile()
    {
        // 需要删除的文件夹路径(包括其中的子文件)
        string[] directoryPath = new string[]
        {
            Path.Combine(Application.dataPath, @"GameMain/Scripts/Framework/Ads/AdsChannel/Admob"),
            Path.Combine(Application.dataPath, @"GameMain/Scripts/Framework/Ads/AdsChannel/Yandex"),
            Path.Combine(Application.dataPath, @"GameMain/Scripts/Framework/Ads/AdsChannel/YandexMyTargetMix"),
            Path.Combine(Application.dataPath, "GoogleMobileAds"),
            Path.Combine(Application.dataPath, "GoogleSignIn"),
            Path.Combine(Application.dataPath, "Mycom.Target.Unity"),
            Path.Combine(Application.dataPath, "YandexMobileAds"),
        };
        // 需要删除的文件路径
        string[] filePath = new string[]
        {
            Path.Combine(Application.dataPath, @"Plugins/iOS/googlemobileadsnative-plugin-library.a"),
        };
        
        for (int i = 0; i < directoryPath.Length; i++)
        {
            if(Directory.Exists(directoryPath[i]))
            {
                Directory.Delete(directoryPath[i], true);
            }
        }

        for (int i = 0; i < filePath.Length; i++)
        {
            if (File.Exists(filePath[i]))
            {
                File.Delete(filePath[i]);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static void IosBuild(string buildNumber)
    {
        var scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
        BuildPipeline.BuildPlayer(scenes, GetPath(buildNumber), BuildTarget.iOS, BuildOptions.None);
    }
    
    /// <summary>
    /// 获取项目存放路径
    /// </summary>
    /// <param name="buildName"></param>
    /// <returns></returns>
    private static string GetPath(string buildName)
    {
        return Path.Combine("/Volumes/mdisk2T/TM/iOS", PlayerSettings.bundleVersion, buildName);
    }
    
    /// <summary>
    /// 设置Xcode项目的参数
    /// </summary>
    /// <param name="setXcode">是否设置</param>
    /// <param name="path">Xcode项目文件夹名称</param>
    public static void SetXcodeProcess(bool setXcode, string path)
    {
        if (!setXcode) return;
        path = GetPath(path);
        PBXProject proj = new PBXProject();
        // 返回给定 Unity 构建路径中 PBX 项目的路径。
        string project_file_name = PBXProject.GetPBXProjectPath(path);
        // 从给定路径标识的文件中读取项目。
        proj.ReadFromFile(project_file_name);
        // 返回 Unity 項目中框架目標的 GUID。
        string guid = proj.GetUnityFrameworkTargetGuid();
        string mainGuid = proj.GetUnityMainTargetGuid();

        //添加Apple ID 登录框架   proj.AddCapability()无效
        // entitlementFilePath 中的名称根据项目不同，名称也不同，TM中为Entitlements.entitlements，可在生成的Xcode项目中直接查看
        ProjectCapabilityManager manager = new ProjectCapabilityManager(project_file_name, $"Entitlements.entitlements", targetGuid: mainGuid);
        manager.AddSignInWithApple();
        // development：启用或禁用开发模式。测试 App Store 以外的应用程序时，应使用开发模式。
        manager.AddPushNotifications(false);
        manager.WriteToFile();

        //设置开发者ID
        proj.SetBuildProperty(mainGuid, "DEVELOPMENT_TEAM", "RLH82TZ358");

        //修改属性
        proj.SetBuildProperty(guid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(mainGuid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(guid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        //保存修改后的文件
        proj.WriteToFile(project_file_name);

        #region 添加framework
        // proj.AddFrameworkToProject(guid, "GameKit.framework", false);
        // proj.AddFrameworkToProject(guid, "AppTrackingTransparency.framework", false);
        #endregion
        
        #region 修改info.plist
        // // 读取Info.plist文件
        // string plistPath = Path.Combine(path, "Info.plist");
        // try
        // {
        //     PlistDocument plist = new PlistDocument();
        //     plist.ReadFromFile(plistPath);
        //     // 修改数据
        //     //  修改 bool 值
        //     // plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        //     //  修改 string 值
        //     plist.root.SetString("NSUserTrackingUsageDescription", "Your data will only be used to deliver personalized ads to you.");
        //     //  修改数组
        //     PlistElementArray requiredArray = plist.root.CreateArray("UIRequiredDeviceCapabilities");
        //     requiredArray.AddString("arm64");
        //     //保存修改后的Info.plist
        //     plist.WriteToFile(plistPath);
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError($"发生错误: {ex.Message}");
        // } 
        #endregion

        RepairError(path);
        
        AddRubyToPodfile(path);

        FixedBug_iOS_18(path);
    }
    
    // podfile的问题
    public static void AddRubyToPodfile(string path)
    {
        string podfilePath = Path.Combine(path, "Podfile");
        string rubyCode =@"
post_install do |installer|
  installer.pods_project.targets.each do |target|
    if target.name == 'BoringSSL-GRPC'
      target.source_build_phase.files.each do |file|
        if file.settings && file.settings['COMPILER_FLAGS']
          flags = file.settings['COMPILER_FLAGS'].split
          flags.reject! { |flag| flag == '-GCC_WARN_INHIBIT_ALL_WARNINGS' }
          file.settings['COMPILER_FLAGS'] = flags.join(' ')
        end
      end
    end
  end
end";
        try
        {
            // 读取Podfile内容
            var lines = File.ReadAllLines(podfilePath).ToList();
            
            // 检查是否已包含post_install代码
            bool hasPostInstall = lines.Any(line => line.Trim().StartsWith("post_install"));
            
            if (!hasPostInstall)
            {
                // 查找最后一个end的位置（通常在文件末尾）
                int lastEndIndex = lines.FindLastIndex(line => line.Trim() == "end");
                
                if (lastEndIndex >= 0)
                {
                    // 在最后一个end之前插入我们的代码
                    lines.Insert(lastEndIndex + 1, rubyCode);
                    File.WriteAllLines(podfilePath, lines);
                    Debug.Log("成功添加代码到Podfile");
                    
                    // 执行pod install
                    ExecuteTerminalCommand("pod install", path);
                }
                else
                {
                    // 如果没有找到end，直接追加到文件末尾
                    File.AppendAllText(podfilePath, rubyCode);
                    Debug.Log("成功添加代码到Podfile末尾");
                    
                    // 执行pod install
                    ExecuteTerminalCommand("pod install", path);
                }
            }
            else
            {
                Debug.Log("Podfile中已包含post_install代码");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"发生错误: {ex.Message}");
        }
    }
    
    static void ExecuteTerminalCommand(string command, string workingDirectory)
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"export LANG=en_US.UTF-8 && export LC_ALL=en_US.UTF-8 && {command}\"";
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        
        Debug.Log($"正在执行: {command}");
        
        process.Start();
        
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        
        process.WaitForExit();
        
        if (!string.IsNullOrEmpty(output))
            Debug.Log("输出: " + output);
        
        // 错误：通常ExitCode不为0。警告：error不为空，但ExitCode等于0。
        if (!string.IsNullOrEmpty(error))
            Debug.Log($"{(process.ExitCode != 0 ? "错误: " : "警告：")}" + error);
        
        Debug.Log($"命令执行完成，退出代码: {process.ExitCode}");
    }
    
    // 修复 AppsFlyer+AppController.m 文件中的报错
    public static void RepairError(string path)
    {
        string filePath = Path.Combine(path, "Libraries/AppsFlyer/Plugins/iOS/AppsFlyer+AppController.m"); // 替换为实际路径
        
        try
        {
            // 读取文件内容
            string fileContent = File.ReadAllText(filePath);
            
            // 定义要查找和替换的代码
            string originalCode = "NSDictionary*, (UIBackgroundFetchResult)";
            string modifiedCode = "NSDictionary*, int(UIBackgroundFetchResult)";
            
            // 检查文件是否包含原始代码
            if (fileContent.Contains(originalCode))
            {
                // 执行替换
                fileContent = fileContent.Replace(originalCode, modifiedCode);
                
                // 写入修改后的内容
                File.WriteAllText(filePath, fileContent);
                
                Debug.Log("文件修改成功！");
            }
            else
            {
                Debug.Log("未找到需要修改的代码。");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"发生错误: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 修复iOS 18.3 播放全屏广告后频繁出现 NSInternalInconsistencyException 崩溃
    /// 修复方案：注释UnityAppController.mm文件中的[_UnityAppController window].rootViewController = nil;代码
    /// </summary>
    public static void FixedBug_iOS_18(string path)
    {
        string filePath = Path.Combine(path, "Classes/UnityAppController.mm"); // 替换为实际路径
        
        try
        {
            // 读取文件内容
            string fileContent = File.ReadAllText(filePath);
            
            // 定义要查找和替换的代码
            string originalCode = "[_UnityAppController window].rootViewController = nil;";
            string modifiedCode = "//[_UnityAppController window].rootViewController = nil;";
            
            // 检查文件是否包含原始代码
            if (fileContent.Contains(originalCode))
            {
                // 执行替换
                fileContent = fileContent.Replace(originalCode, modifiedCode);
                
                // 写入修改后的内容
                File.WriteAllText(filePath, fileContent);
                
                Debug.Log("文件修改成功！");
            }
            else
            {
                Debug.Log("未找到需要修改的代码。");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"发生错误: {ex.Message}");
        }
    }
}
#endif