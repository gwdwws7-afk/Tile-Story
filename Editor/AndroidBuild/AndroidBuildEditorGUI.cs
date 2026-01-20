using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class AndroidBuildEditorGUI : EditorWindow
{
    private static AndroidBuildEditorGUI window;

    public AndroidBuildEditorGUI()
    {
        this.titleContent = new GUIContent("AndroidBuildEditorGUI");
        titleContent.text = "Android打包设置";
    }

    /// <summary>
    /// 打包版本
    /// </summary>
    public static string BuildVersion = "";

    /// <summary>
    /// 打包版本号
    /// </summary>
    public static int bundleVersionCode = -1;
    public static string BundleVersionCode
    {
        get
        {
            if (bundleVersionCode == -1)
                return "";
            return bundleVersionCode.ToString();
        }
        set
        {
            if (int.TryParse(value, out int code))
            {
                bundleVersionCode = code;
            }
        }
    }

    /// <summary>
    /// 生成包的存放路径
    /// </summary>
    private static string buildPath = "";

    public static string BuildPath
    {
        get
        {
            if (buildPath == "")
            {
                buildPath = PlayerPrefs.GetString("AndroidBuildPath", "");
            }

            return buildPath;
        }
        set
        {
            buildPath = value;
            PlayerPrefs.SetString("AndroidBuildPath", buildPath);
        }
    }
    
    /// <summary>
    /// 是否要重新打AB包(StreamingAssets目录下的)
    /// </summary>
    public bool buildAssetBundle = true;

    public bool isResolve = true;

    /// <summary>
    /// 是否需要打包成aab文件
    /// </summary>
    public static bool buildAAB = true;

    Vector3 ScrollViewVector;

    [MenuItem("BubbleTools/Build/Build Android")]
    public static void Init()
    {
        ReadVersion();

        // Get existing open window or if none, make a new one:
        window = (AndroidBuildEditorGUI)EditorWindow.GetWindow(typeof(AndroidBuildEditorGUI));

        window.Show();
    }

    private static void ReadVersion()
    {
        BuildVersion = PlayerSettings.bundleVersion;

        BundleVersionCode = PlayerSettings.Android.bundleVersionCode.ToString();

        buildAAB = EditorUserBuildSettings.buildAppBundle;
    }

    private static void SaveVersion()
    {
        PlayerSettings.bundleVersion = BuildVersion;

        if (int.TryParse(BundleVersionCode, out int code))
        {
            PlayerSettings.Android.bundleVersionCode = code;
        }
    }

    private void OnGUI()
    {
        ScrollViewVector = GUILayout.BeginScrollView(ScrollViewVector, GUILayout.Width(position.width));
        
        HorizontalLayout(() =>
        {
            GUILayout.Label("Build Version", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            BuildVersion = GUILayout.TextField(BuildVersion, new GUILayoutOption[] { GUILayout.Width(150) });
        });

        HorizontalLayout(() =>
        {
            GUILayout.Label("Build Version Code", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            BundleVersionCode = GUILayout.TextField(BundleVersionCode, new GUILayoutOption[] { GUILayout.Width(150) });
        });
            
        HorizontalLayout(() =>
        {
            GUILayout.Label("Build Path", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            BuildPath = GUILayout.TextField(BuildPath, new GUILayoutOption[] { GUILayout.Width(130) });
            if (GUILayout.Button("*", GUILayout.Width(20)))
            {
                BuildPath = EditorUtility.OpenFolderPanel(
                    title: "选择生成文件夹",
                    folder: BuildPath, // 默认从Assets目录打开
                    defaultName: ""
                );
            }
        });
        
        HorizontalLayout(() =>
        {
            GUILayout.Label("Build AAB", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            buildAAB = EditorGUILayout.Toggle(buildAAB);
            EditorUserBuildSettings.buildAppBundle = buildAAB;
        });

        HorizontalLayout(() =>
        {
            GUILayout.Label("BuildAssetBundle", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            buildAssetBundle = EditorGUILayout.Toggle(buildAssetBundle);
        });
        
        HorizontalLayout(() =>
        {
            GUILayout.Label("Resolve", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
            isResolve = EditorGUILayout.Toggle(isResolve);
        });

        HorizontalLayout(() =>
        {
            if (GUILayout.Button("Read Version", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                ReadVersion();
            }
        });

        GUILayout.Space(10);
        
        HorizontalLayout(() =>
        {
            if (GUILayout.Button("Save Version", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                SaveVersion();
            }
        });

        GUILayout.Space(10);

        HorizontalLayout(() =>
        {
            if (GUILayout.Button("Delete File", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                AndroidBatchmode.DeleteNoNeedFile();
            }
        });

        GUILayout.Space(10);

        HorizontalLayout(() =>
        {
            if (GUILayout.Button("Build Android Project", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                AndroidBatchmode.AndroidBuildForEditor(buildAssetBundle, isResolve, buildAAB, bundleVersionCode, false,
                    "", BuildPath);
            }
        });

        GUILayout.EndScrollView();
    }

    public void HorizontalLayout(Action action)
    {
        GUILayout.BeginHorizontal();
        action?.Invoke();
        GUILayout.EndHorizontal();
    }
}
