#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class iOSBuildEditorGUI : EditorWindow
{
    private static iOSBuildEditorGUI window;

    public iOSBuildEditorGUI()
    {
        this.titleContent = new GUIContent("iOSBuildEditorGUI");
        titleContent.text = "iOS打包设置";
    }
    
    /// <summary>
    /// 打包版本
    /// </summary>
    public static string m_BuildVersion = "";

    /// <summary>
    /// 打包版本号
    /// </summary>
    public static string m_BuildNumber = "";
    
    /// <summary>
    /// 是否要重新打AB包
    /// </summary>
    public bool m_BuildAssetBundle = true;
    
    /// <summary>
    /// 是否需要修改Xcode设置
    /// </summary>
    public bool m_SetXcodeSetting = true;
    
    Vector3 ScrollViewVector;
    
    [MenuItem("BubbleTools/Build/Build iOS")]
    public static void Init()
    {
        ReadVersion();

        // Get existing open window or if none, make a new one:
        window = (iOSBuildEditorGUI)EditorWindow.GetWindow(typeof(iOSBuildEditorGUI));

        window.Show();
    }
    
    private static void ReadVersion()
    {
        m_BuildVersion = PlayerSettings.bundleVersion;

        m_BuildNumber = PlayerSettings.iOS.buildNumber;
    }
    
    private static void SaveVersion()
    {
        PlayerSettings.bundleVersion = m_BuildVersion;

        PlayerSettings.iOS.buildNumber = m_BuildNumber;
    }
    
    private void OnGUI()
    {
        ScrollViewVector = GUILayout.BeginScrollView(ScrollViewVector, GUILayout.Width(position.width));
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build Version", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
        m_BuildVersion = GUILayout.TextField(m_BuildVersion, new GUILayoutOption[] { GUILayout.Width(150) });
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Build Version Code", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
        m_BuildNumber = GUILayout.TextField(m_BuildNumber, new GUILayoutOption[] { GUILayout.Width(150) });
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("BuildAssetBundle", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
        m_BuildAssetBundle = EditorGUILayout.Toggle(m_BuildAssetBundle);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Set Xcode Setting", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
        m_SetXcodeSetting = EditorGUILayout.Toggle(m_SetXcodeSetting);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Read Version", new GUILayoutOption[] { GUILayout.Width(150) }))
        {
            ReadVersion();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Version", new GUILayoutOption[] { GUILayout.Width(150) }))
        {
            SaveVersion();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Delete File", new GUILayoutOption[] { GUILayout.Width(150) }))
        {
            iOSBatchmode.DeleteNoNeedFile();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build IOS Project", new GUILayoutOption[] { GUILayout.Width(150) }))
        {
            iOSBatchmode.iOSBuildForEditor(m_BuildAssetBundle, m_SetXcodeSetting, m_BuildNumber);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Xcode Project", new GUILayoutOption[] { GUILayout.Width(150) }))
        {
            iOSBatchmode.SetXcodeProcess(m_SetXcodeSetting, m_BuildNumber);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }
}
#endif