using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景组件
/// </summary>
public sealed class SceneComponent : GameFrameworkComponent
{
    private const int cameraSize = 1;
    private SceneType sceneType;
    private SceneChangeType sceneChangeType;
    private Camera mainCamera;
    private AsyncOperationHandle<SceneInstance> sceneHandle;
    private int gameToMapTimes;

    /// <summary>
    /// 场景类型
    /// </summary>
    public SceneType SceneType { get => sceneType; }

    /// <summary>
    /// 场景转换类型
    /// </summary>
    public SceneChangeType SceneChangeType { get => sceneChangeType; }

    /// <summary>
    /// 当前场景主摄像机
    /// </summary>
    public Camera MainCamera { get => mainCamera; }

    /// <summary>
    /// 当前场景名称
    /// </summary>
    public string ActiveSceneName { get => SceneManager.GetActiveScene().name; }

    /// <summary>
    /// 主摄像机是否在移动
    /// </summary>
    public bool IsMainCameraMove { get; set; }

    /// <summary>
    /// 返回Map场景次数
    /// </summary>
    public int GameToMapTimes { get => gameToMapTimes; }

    /// <summary>
    /// 设置当前所在场景类型
    /// </summary>
    public void SetSceneType(SceneType type)
    {
        SetSceneChangeType(type);
        sceneType = type;
    }

    /// <summary>
    /// 刷新主摄像机
    /// </summary>
    public void RefreshMainCamera()
    {
        mainCamera = Camera.main;

        if (mainCamera == null) return;

        float aspect = Screen.height / (float)Screen.width;
        float standardAspect = 1920 / 1080f;

        if (aspect > 1920 / 1080f)
        {
            mainCamera.orthographicSize = cameraSize * aspect / standardAspect;
        }
        else
        {
            mainCamera.orthographicSize = cameraSize;
        }

        IsMainCameraMove = false;
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    public void LoadSceneAsync(string nextSceneName, Action<AsyncOperationHandle<SceneInstance>> completeAction)
    {
        sceneHandle = Addressables.LoadSceneAsync(nextSceneName);
        sceneHandle.Completed += completeAction;
    }

    /// <summary>
    /// 加载场景的进度
    /// </summary>
    public float GetPercentComplete()
    {
        if (sceneHandle.IsValid())
            return sceneHandle.PercentComplete;
        return 0;
    }

    private void SetSceneChangeType(SceneType type)
    {
        Log.Info($"SetSceneChangeType:{sceneType}=>{type}");

        if (sceneType == SceneType.Menu)
        {
            if (type == SceneType.Map)
            {
                sceneChangeType = SceneChangeType.MenuToMap;
            }
        }
        else if (sceneType == SceneType.Map)
        {
            if (type == SceneType.Game)
            {
                sceneChangeType = SceneChangeType.MapToGame;
            }
        }
        else if (sceneType == SceneType.Game) 
        {
            if (type == SceneType.Map)
            {
                sceneChangeType = SceneChangeType.GameToMap;
            }
        }
    }
}
