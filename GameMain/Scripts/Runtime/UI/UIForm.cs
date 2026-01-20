using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI界面基类
/// </summary>
public abstract class UIForm : MonoBehaviour
{
    public bool IsGuide = false;
    [HideInInspector]
    public string UIFormName;
    private int m_SerialId;
    private UIGroup m_UIGroup;
    private int m_DepthInUIGroup;

    public Action m_InitCompleteAction;
    public Action m_OnHideCompleteAction;

    private bool m_StartInitProcess;
    protected bool m_IsAvailable;

    public Action m_ProcessFinishAction;

    [SerializeField]
    private List<UIForm> childUIForms;
    [SerializeField]
    private UIFormType UIType = UIFormType.CenterUI;

#if UNITY_EDITOR
    public void OnValidate()
	{
        Awake();
    }

    [ContextMenu("GetTypeName")]
    private void SetName()
    {
        gameObject.name = this.GetType().Name;
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif

    public string UIName
    {
        get
        {
            return string.IsNullOrEmpty(UIFormName) ? string.Empty : UIFormName;
        }
    }

    /// <summary>
    /// 获取界面序列编号
    /// </summary>
    public int SerialId
    {
        get
        {
            return m_SerialId;
        }
    }

    /// <summary>
    /// 获取界面所属的界面组
    /// </summary>
    public UIGroup UIGroup
    {
        get
        {
            return m_UIGroup;
        }
    }

    /// <summary>
    /// 获取界面深度
    /// </summary>
    public int DepthInUIGroup
    {
        get
        {
            return m_DepthInUIGroup;
        }
    }

    public UIFormType UIFormType
    {
        get => UIType;
        protected set => UIType = value;
    }

    /// <summary>
    /// 获取界面是否可用
    /// </summary>
    public bool IsAvailable { get => m_IsAvailable; }

    public virtual bool IsAutoRelease => true;

    /// <summary>
    /// 创建
    /// </summary>
    public void OnCreate(int serialId)
    {
        m_SerialId = serialId;
    }

	protected virtual void Awake()
	{
        childUIForms = new List<UIForm>(0);

        if (UIType != UIFormType.ChildUI)
        {
            var uiForms = GetComponentsInChildren<UIForm>();
            if (uiForms != null)
            {
                foreach (var child in uiForms)
                {
                    if (child != this)
                    {
                        childUIForms.Add(child);
                    }
                }
            }
        }
    }

    public virtual void Clear()
    {
        if(childUIForms!=null)
            foreach (var form in childUIForms)
            {
                form.Clear();
            }
        m_OnHideCompleteAction = null;
        m_InitCompleteAction = null;
        m_UIGroup = null;
        childUIForms.Clear();
    }

    public virtual void SetGroup(UIGroup uiGroup)
    {
        m_UIGroup = uiGroup;
    }

    public void SetUIFormName(string name)
    {
        UIFormName = name;
    }

    /// <summary>
	/// 初始化
	/// </summary>
	public virtual void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        m_UIGroup = uiGroup;
        m_DepthInUIGroup = 0;

        if (completeAction != null)
        {
            m_InitCompleteAction = completeAction;
        }
        m_StartInitProcess = true;

        foreach (var form in childUIForms)
        {
#if UNITY_EDITOR
            form.OnInit(uiGroup, null, null);
#else
            try
            {
                form.OnInit(uiGroup, null, null);
            }
            catch(Exception e)
            {
                Debug.LogError($"child form {form.name} init error - {e.Message}");
            }
#endif
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public virtual void OnReset()
    {
        m_InitCompleteAction = null;

        foreach (var form in childUIForms)
        {
#if UNITY_EDITOR
            form.OnReset();
#else
            try
            {
                form.OnReset();
            }
            catch (Exception e)
            {
                Debug.LogError($"child form {form.name} reset error - {e.Message}");
            }
#endif
        }
    }

    public virtual void OnShowInit(Action<UIForm> showInitSuccessAction=null ,object userData=null)
    {
        foreach (var form in childUIForms)
        {
#if UNITY_EDITOR
            form.OnShowInit(null, null);
#else
            try
            {
                form.OnShowInit(null, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"child form {form.name} ShowInit error - {e.Message}");
            }
#endif
        }
    }

    /// <summary>
    /// 显示
    /// </summary>
    public virtual void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        m_IsAvailable = true;

#if UNITY_EDITOR
        showSuccessAction?.Invoke(this);
#else
        try
        {
            showSuccessAction?.Invoke(this);
        }
        catch (Exception e)
        {
            Debug.LogError($"form {UIName} showSuccessAction error - {e.Message}");
        }
#endif

        foreach (var form in childUIForms)
        {
            form.OnShow(null, null);
        }
    }

    /// <summary>
    /// 隐藏
    /// </summary>
    public virtual void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        m_IsAvailable = false;

#if UNITY_EDITOR
        hideSuccessAction?.Invoke();
#else
        try
        {
            hideSuccessAction?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"form {UIName} hideSuccessAction error - {e.Message}");
        }
#endif

#if UNITY_EDITOR
        m_OnHideCompleteAction?.Invoke();
#else
        try
        {
            m_OnHideCompleteAction?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"form {UIName} OnHideCompleteAction error - {e.Message}");
        }
#endif
        m_OnHideCompleteAction = null;

        foreach (var form in childUIForms)
        {
            form.OnHide(null, null);
        }
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public virtual void OnPause()
    {
        foreach (var form in childUIForms)
        {
            form.OnPause();
        }
    }

    /// <summary>
    /// 暂停恢复
    /// </summary>
    public virtual void OnResume()
    {
        foreach (var form in childUIForms)
        {
            form.OnResume();
        }
    }

    /// <summary>
    /// 遮挡
    /// </summary>
    public virtual void OnCover()
    {
        foreach (var form in childUIForms)
        {
            form.OnCover();
        }
    }

    /// <summary>
    /// 遮挡恢复
    /// </summary>
    public virtual void OnReveal()
    {
        foreach (var form in childUIForms)
        {
            form.OnReveal();
        }
    }

    /// <summary>
    /// 关闭请求触发
    /// </summary>
    public virtual void OnClose()
    {
        foreach (var form in childUIForms)
        {
            form.OnClose();
        }
    }

    /// <summary>
    /// 重新激活
    /// </summary>
    public virtual void OnRefocus()
    {
        foreach (var form in childUIForms)
        {
            form.OnRefocus();
        }
    }

    /// <summary>
    /// 轮询
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
    public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (m_StartInitProcess && CheckInitComplete()) 
        {
            m_StartInitProcess = false;

#if UNITY_EDITOR
            m_InitCompleteAction?.Invoke();
#else
            try
            {
                m_InitCompleteAction?.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError($"form {UIName} init error - {e.Message}");
            }
#endif

            m_InitCompleteAction = null;
        }

        foreach (var form in childUIForms)
        {
            form.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    public virtual void OnRelease()
    {
        try
        {
            m_InitCompleteAction = null;
            m_ProcessFinishAction?.Invoke();
            m_ProcessFinishAction = null;

            foreach (var form in childUIForms)form.OnRelease();
        }
        catch (Exception e)
        {
            Debug.LogError($"form {UIName} release error - {e.Message}");
        }
    }
    
    /// <summary>
    /// 界面深度改变
    /// </summary>
    /// <param name="uiGroupDepth">界面组深度</param>
    /// <param name="depthInUIGroup">界面在界面组中的深度</param>
    public virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        m_DepthInUIGroup = depthInUIGroup;
        try
        {
            transform.SetSiblingIndex(depthInUIGroup);
        }
        catch (Exception e)
        {
            Log.Debug($"{e.Message}");
        }

        foreach (var form in childUIForms)form.OnDepthChanged(uiGroupDepth, depthInUIGroup);
    }

    /// <summary>
    /// 检测是否初始化完毕
    /// </summary>
    public virtual bool CheckInitComplete()
    {
        return true;
    }

    public void SetHideAction(Action action)
    {
        m_ProcessFinishAction = action;
    }
}
