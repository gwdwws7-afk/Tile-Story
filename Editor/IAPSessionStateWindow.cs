using UnityEditor;
using UnityEngine;

public class IAPSessionStateWindow : EditorWindow
{
    private bool globalDisabled;
    private bool purchasingDisabled;

    [MenuItem("Tools/IAP SessionState Manager")]
    public static void ShowWindow()
    {
        GetWindow<IAPSessionStateWindow>("IAP SessionState");
    }

    private void OnEnable()
    {
        RefreshValues();
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity IAP SessionState Manager", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 全局禁用
        EditorGUILayout.BeginHorizontal();
        globalDisabled = EditorGUILayout.Toggle("SelfDeclaredAndroidDependenciesDisabled", globalDisabled);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
        {
            SessionState.SetBool("SelfDeclaredAndroidDependenciesDisabled", globalDisabled);
            Debug.Log($"Set SelfDeclaredAndroidDependenciesDisabled = {globalDisabled}");
        }
        EditorGUILayout.EndHorizontal();

        // IAP 特定禁用
        EditorGUILayout.BeginHorizontal();
        purchasingDisabled = EditorGUILayout.Toggle("SelfDeclaredAndroidDependenciesDisabled:com.unity.purchasing", purchasingDisabled);
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
        {
            SessionState.SetBool("SelfDeclaredAndroidDependenciesDisabled:com.unity.purchasing", purchasingDisabled);
            Debug.Log($"Set SelfDeclaredAndroidDependenciesDisabled:com.unity.purchasing = {purchasingDisabled}");
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh"))
        {
            RefreshValues();
        }
    }

    private void RefreshValues()
    {
        globalDisabled = SessionState.GetBool("SelfDeclaredAndroidDependenciesDisabled", false);
        purchasingDisabled = SessionState.GetBool("SelfDeclaredAndroidDependenciesDisabled:com.unity.purchasing", false);
    }
}