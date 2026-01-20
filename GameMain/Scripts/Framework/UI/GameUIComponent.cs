using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI组件
/// </summary>
public class GameUIComponent : GameFrameworkComponent
{
    private IUIManager m_UIManager;

    private readonly List<UIForm> m_InternalUIFormResults = new List<UIForm>();

    /// <summary>
    /// 获取界面组数量
    /// </summary>
    public int UIGroupCount
    {
        get
        {
            return m_UIManager.UIGroupCount;
        }
    }

    /// <summary>
    /// 自动释放可释放对象的间隔时间
    /// </summary>
    public float AutoReleaseInterval
    {
        get
        {
            return m_UIManager.AutoReleaseInterval;
        }
        set
        {
            m_UIManager.AutoReleaseInterval = value;
        }
    }

    public bool IsAsyncLoading 
    {
        get
        {
            return m_UIManager.IsAsyncLoading;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        m_UIManager = GameFrameworkEntry.GetModule<UIManager>();
        if (m_UIManager == null)
        {
            Log.Fatal("UI manager is invalid.");
            return;
        }
    }

    private void Start()
    {
        DefaultUIFormHelper helper = new DefaultUIFormHelper();
        m_UIManager.SetUIFormHelper(helper);

        UIGroup[] uiGroups = GetComponentsInChildren<UIGroup>();

        for (int i = 0; i < uiGroups.Length; i++)
        {
            AddUIGroup(uiGroups[i]);
        }
    }

    /// <summary>
    /// 是否存在界面组
    /// </summary>
    public bool HasUIGroup(string uiGroupName)
    {
        return m_UIManager.HasUIGroup(uiGroupName);
    }

    /// <summary>
    /// 获取界面组
    /// </summary>
    /// <param name="groupType"></param>
    /// <returns></returns>
    public UIGroup GetUIGroup(UIFormType groupType)
    {
        return m_UIManager.GetUIGroup(UnityUtility.GetEnumName(groupType));
    }
    
    public UIGroup GetUIGroup(string uiGroupName)
    {
        return m_UIManager.GetUIGroup(uiGroupName);
    }

    /// <summary>
    /// 获取所有界面组
    /// </summary>
    public UIGroup[] GetAllUIGroups()
    {
        return m_UIManager.GetAllUIGroups();
    }

    /// <summary>
    /// 获取所有界面组
    /// </summary>
    public void GetAllUIGroups(List<UIGroup> results)
    {
        m_UIManager.GetAllUIGroups(results);
    }

    /// <summary>
    /// 增加界面组
    /// </summary>
    public bool AddUIGroup(UIGroup uiGroup)
    {
        return m_UIManager.AddUIGroup(uiGroup);
    }

    /// <summary>
    /// 是否存在界面
    /// </summary>
    public bool HasUIForm(string uiName)
    {
        return m_UIManager.HasUIForm(uiName);
    }

    /// <summary>
    /// 获取界面
    /// </summary>
    public UIForm GetUIForm(string uiName)
    {
        return m_UIManager.GetUIForm(uiName);
    }
    
    /// <summary>
    /// 获取所有已加载的界面
    /// </summary>
    /// <returns>所有已加载的界面</returns>
    public UIForm[] GetAllLoadedUIForms()
    {
        UIForm[] uiForms = m_UIManager.GetAllLoadedUIForms();
        UIForm[] uiFormImpls = new UIForm[uiForms.Length];
        for (int i = 0; i < uiForms.Length; i++)
        {
            uiFormImpls[i] = uiForms[i];
        }

        return uiFormImpls;
    }

    /// <summary>
    /// 获取所有已加载的界面
    /// </summary>
    /// <param name="results">所有已加载的界面</param>
    public void GetAllLoadedUIForms(List<UIForm> results)
    {
        if (results == null)
        {
            Log.Error("Results is invalid.");
            return;
        }

        results.Clear();
        m_UIManager.GetAllLoadedUIForms(m_InternalUIFormResults);
        foreach (UIForm uiForm in m_InternalUIFormResults)
        {
            results.Add(uiForm);
        }
    }

    /// <summary>
    /// 是否正在加载界面
    /// </summary>
    public bool IsLoadingUIForm(string uiFormName)
    {
        return m_UIManager.IsLoadingUIForm(uiFormName);
    }

    public void ShowUIForm(string uiName, Action<UIForm> showSuccessAction = null, Action showFailAction = null, object userData = null)
    {
        m_UIManager.ShowUIForm(uiName, UIFormType.None, showSuccessAction, showFailAction, userData);
    }
    
    public void ShowUIForm(string uiName, UIFormType uiFormType, Action<UIForm> showSuccessAction = null, Action showFailAction = null, object userData = null)
    {
        m_UIManager.ShowUIForm(uiName, uiFormType, showSuccessAction, showFailAction, userData);
    }

    /// <summary>
    /// 隐藏界面
    /// </summary>
    /// <param name="onHideComplete">隐藏完毕事件</param>
    /// <param name="userData">自定义数据</param>
    public void HideUIForm(string uiFormName, Action onHideComplete = null, object userData = null)
    {
        m_UIManager.HideUIForm(uiFormName, onHideComplete, userData);
    }

    /// <summary>
    /// 隐藏界面
    /// </summary>
    /// <param name="uiForm">界面</param>
    /// <param name="onHideComplete">隐藏完毕事件</param>
    /// <param name="userData">自定义数据</param>
    public void HideUIForm(UIForm uiForm, Action onHideComplete = null, object userData = null)
    {
        m_UIManager.HideUIForm(uiForm, onHideComplete, userData);
    }

    public void OnReset()
    {
        m_UIManager.OnReset();
    }

	public void OnInit()
	{
        m_UIManager.OnInit();

        //进程回调注册
        if (UnityEngine.Screen.height * 1080 < 1920 * UnityEngine.Screen.width)
        {
            GameManager.UI.ShowUIForm("FuzzyBoundary");
        }

        GameManager.UI.ShowUIForm("GlobalMaskPanel",(u) =>
        {
            GameManager.UI.HideUIForm("GlobalMaskPanel");
            GameManager.Process.SetProcessCallBack(() =>
            {
                GameManager.UI.ShowUIForm("GlobalMaskPanel");
            }, () =>
            {
                GameManager.UI.HideUIForm("GlobalMaskPanel");
            });
        });
    }

    public void CreateUIForm(string uiName,UIFormType uiFormType, Action<UIForm> createSuccessAction = null, Action createFailAction = null)
    {
        m_UIManager.CreateUIForm(uiName, uiFormType, createSuccessAction, createFailAction);
    }

    public void HideAllUIForm(string[] uiExceptFormsName)
    {
        m_UIManager.HideAllUIForm(uiExceptFormsName);
    }

    public void HideAllUIForm(params UIForm[] uis)
    {
        m_UIManager.HideAllUIForm(uis);
    }

    public bool IsHasFormInGroup(string uiGroupName)
    {
        return m_UIManager.IsHasFormInGroup(uiGroupName);
    }

    public void ReleaseAllUIForm(params UIForm[] uis)
    {
        m_UIManager.ReleaseAllUIForm(uis);
    }

    public void RefreshUIByFirebaseLogin()
    {
        m_UIManager.RefreshAllUIByLogin();
    }

    #region GC

    private bool isStartGC;

    public void StartGarbageCollection()
    {
        if (isStartGC)
            return;

        StartCoroutine(GarbageCollectionCor());
    }

    IEnumerator GarbageCollectionCor()
    {
        isStartGC = true;

        yield return Resources.UnloadUnusedAssets();

        if(!GameManager.Ads.IsRequestingAd)
            GC.Collect();

        isStartGC = false;
    }

    private bool isUnloadingUnusedAssets;

    public bool IsUnloadingUnusedAssets
    {
        get => isUnloadingUnusedAssets;
    }

    public void StartUnloadingUnusedAssets()
    {
        if (isUnloadingUnusedAssets || isStartGC) 
            return;

        isUnloadingUnusedAssets = true;

        StartCoroutine(UnloadingUnusedAssetsCor());
    }

    IEnumerator UnloadingUnusedAssetsCor()
    {
        yield return Resources.UnloadUnusedAssets();

        yield return null;

        GC.Collect();

        isUnloadingUnusedAssets = false;
    }

    #endregion
}
