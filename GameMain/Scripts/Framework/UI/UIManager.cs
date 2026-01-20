using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UIFormType
{
    BgUI,
    TopUI,
    BottomUI,
    LeftUI,
    RightUI,
    CenterUI,
    GeneralUI,
    PopupUI,
    Area1,
    None,
    GuideUI,
    EditorUI,
    FuzzyBoundaryUI,
    ChildUI,
}

public class UIManager : GameFrameworkModule, IUIManager
{
    public static int s_SerialId;

    private readonly Dictionary<string, UIGroup> m_UIGroups;
    private readonly Dictionary<string, bool> m_UIFormsBeingLoaded;
    private readonly LinkedList<UIForm> m_RecycleList;
    private IUIFormHelper m_UIFormHelper;
    private float autoReleaseInterval;
    private float recycleTimer;

    private bool isAsyncLoading = false;

    public bool IsAsyncLoading { get => isAsyncLoading; }

    public UIManager()
    {
        m_UIGroups = new Dictionary<string, UIGroup>();
        m_UIFormsBeingLoaded = new Dictionary<string, bool>();
        m_RecycleList = new LinkedList<UIForm>();
        m_UIFormHelper = null;
        recycleTimer = 0;

        autoReleaseInterval = 5f;
    }

    /// <summary>
    /// 界面组数量
    /// </summary>
    public int UIGroupCount
    {
        get
        {
            return m_UIGroups.Count;
        }
    }

    /// <summary>
    /// 自动释放可释放对象的间隔时间
    /// </summary>
    public float AutoReleaseInterval
    {
        get
        {
            return autoReleaseInterval;
        }
        set
        {
            autoReleaseInterval = value;
        }
    }

    public void OnReset()
    {
        foreach (var uiGroup in m_UIGroups)
        {
            uiGroup.Value.OnReset();
        }
    }

    public void OnInit()
    {
        foreach (var uiGroup in m_UIGroups)
        {
            uiGroup.Value.OnReset();
        }
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (m_RecycleList.Count > 0)
        {
            if (recycleTimer <= autoReleaseInterval)
            {
                recycleTimer += 0.02f;
            }
            else
            {
                recycleTimer = 0;
                var form = m_RecycleList.First;
                while (form!=null&&form.Value!=null&&!form.Value.IsAutoRelease)
                {
                    form = form.Next;
                }
                if(form==null||form.Value==null)return;
                
                UIForm uiForm = form.Value;
                m_RecycleList.Remove(form);
                if (uiForm != null)
                {
                    // uiForm.OnRelease();

                    m_UIFormHelper.ReleaseUIForm(uiForm);
                }
                else
                {
                    Log.Warning("UIform recycle fail");
                }
            }
        }

        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            uiGroup.Value.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Action showQuitPanel = () =>
            {
                GameManager.UI.ShowUIForm("TopSureQuitPanel");
            };
            try
            {
                var loadingPanel = GameManager.UI.GetUIForm("LoadingMenu");
                if (loadingPanel != null && loadingPanel.gameObject.activeInHierarchy)
                {
                    return;
                }

                var topQuitPanel=GameManager.UI.GetUIForm("TopSureQuitPanel");
                if (topQuitPanel != null&&topQuitPanel.gameObject.activeInHierarchy)
                {
                    GameManager.UI.HideUIForm(topQuitPanel);
                    return;
                }

                if (!RewardManager.Instance.CheckLoadComplete())
                {
                    return;
                }

                if (RewardManager.Instance.RewardPanel != null)
                {
                    RewardManager.Instance.RewardPanel.blackBg.clickButton.onClick?.Invoke();
                    return;
                }
                
                if(m_UIGroups!=null&&m_UIGroups.TryGetValue(UnityUtility.GetEnumName(UIFormType.GuideUI),out UIGroup uiGroup1)&&
                   (uiGroup1.CurrentUIForm != null && uiGroup1.CurrentUIForm.gameObject.activeInHierarchy))
                    return;

                if (m_UIGroups != null&&
                    m_UIGroups.TryGetValue(UnityUtility.GetEnumName(UIFormType.PopupUI),out UIGroup uiGroup)&&
                    (uiGroup.CurrentUIForm != null && uiGroup.CurrentUIForm.gameObject.activeInHierarchy))
                {
                    if (!uiGroup.CurrentUIForm.IsGuide)
                    {
                        if (uiGroup.CurrentUIForm is ICustomOnEscapeBtnClicked)
                        {
                            (uiGroup.CurrentUIForm as ICustomOnEscapeBtnClicked).OnEscapeBtnClicked();
                        }
                        else
                        {
                            GameManager.UI.HideUIForm(uiGroup.CurrentUIForm);
                        }
                    }
                    else
                    {
                        if (HasUIForm("GameWellDonePanel"))
                        {
                            HideUIForm("GetBGPanel");
                            (GetUIForm("GameWellDonePanel") as GameWellDonePanel)?.OnHomeButtonClick();
                        }
                        return;
                    }
                }
                else if (m_UIGroups.TryGetValue(UnityUtility.GetEnumName(UIFormType.CenterUI), out UIGroup centerGroup)&&
                          centerGroup.CurrentUIForm!=null && !centerGroup.CurrentUIForm.IsGuide)
                {
                    if (!centerGroup.CurrentUIForm.gameObject.activeInHierarchy)
                        return;

                    if (centerGroup.CurrentUIForm is ICustomOnEscapeBtnClicked)
                    {
                        (centerGroup.CurrentUIForm as ICustomOnEscapeBtnClicked).OnEscapeBtnClicked();
                    }
                    else
                    {
                        GameManager.UI.HideUIForm(centerGroup.CurrentUIForm);
                    }
                }
                else
                {
                    if (HasUIForm("TileMatchPanel"))
                    {
                        GameManager.UI.ShowUIForm("GameSettingPanel",null,null,"ShowByClick");
                    }
                    else
                    {

                        showQuitPanel?.Invoke();
                    }
                }
            }
            catch
            {
                showQuitPanel?.Invoke();
            }
        }
    }

    public override void Shutdown()
    {
        m_UIGroups.Clear();
        m_UIFormsBeingLoaded.Clear();
        m_RecycleList.Clear();
    }

    /// <summary>
    /// 设置界面辅助器
    /// </summary>
    /// <param name="uiFormHelper">界面辅助器</param>
    public void SetUIFormHelper(IUIFormHelper uiFormHelper)
    {
        if (uiFormHelper == null)
        {
            throw new Exception("UI form helper is invalid.");
        }

        m_UIFormHelper = uiFormHelper;
    }

    /// <summary>
    /// 是否存在界面组
    /// </summary>
    public bool HasUIGroup(string uiGroupName)
    {
        if (string.IsNullOrEmpty(uiGroupName))
        {
            throw new Exception("UI group name is invalid.");
        }

        return m_UIGroups.ContainsKey(uiGroupName);
    }

    public UIGroup GetUIGroup(UIFormType type)
    {
        return GetUIGroup(UnityUtility.GetEnumName(type));
    }

    public bool IsHasFormInGroup(string uiGroupName)
    {
        if (HasUIGroup(uiGroupName))
        {
            var form = m_UIGroups[uiGroupName].CurrentUIForm;
            if (form != null && form.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取界面组
    /// </summary>
    public UIGroup GetUIGroup(string uiGroupName)
    {
        if (string.IsNullOrEmpty(uiGroupName))
        {
            throw new Exception("UI group name is invalid.");
        }

        if (m_UIGroups.TryGetValue(uiGroupName, out UIGroup uiGroup))
        {
            return uiGroup;
        }

        return null;
    }

    /// <summary>
    /// 获取所有界面组
    /// </summary>
    public UIGroup[] GetAllUIGroups()
    {
        int index = 0;
        UIGroup[] results = new UIGroup[m_UIGroups.Count];
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            results[index++] = uiGroup.Value;
        }

        return results;
    }

    /// <summary>
    /// 获取所有界面组
    /// </summary>
    public void GetAllUIGroups(List<UIGroup> results)
    {
        if (results == null)
        {
            throw new Exception("Results is invalid.");
        }

        results.Clear();
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            results.Add(uiGroup.Value);
        }
    }

    /// <summary>
    /// 增加界面组
    /// </summary>
    public bool AddUIGroup(UIGroup uiGroup)
    {
        if (HasUIGroup(uiGroup.GroupName))
        {
            return false;
        }

        m_UIGroups.Add(uiGroup.GroupName, uiGroup);

        return true;
    }

    /// <summary>
    /// 是否存在界面
    /// </summary>
    public bool HasUIForm(string uiName)
    {
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            if (uiGroup.Value.HasUIForm(uiName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 是否存在界面
    /// </summary>
    public bool HasUIForm(string uiName, string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            throw new Exception("UI group name is invalid.");
        }

        UIGroup uiGroup = GetUIGroup(groupName);
        if (uiGroup == null)
        {
            Log.Warning("uiGroup {0} is null", groupName);
            return false;
        }

        if (uiGroup.HasUIForm(uiName))
        {
            return true;
        }

        return false;
    }

    public UIForm GetUIForm(string uiName)
    {
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            UIForm form = uiGroup.Value.GetUIForm(uiName);

            if (form != null)
                return form;
        }

        return null;
    }

    private UIForm Create(string uiName)
    {
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            if (uiGroup.Value.HasUIForm(uiName))
            {
                return GetUIForm(uiName,uiGroup.Value);
            }
        }
        
        if (m_RecycleList.Count > 0)
        {
            UIForm uiForm = null;
            foreach (UIForm recycleForm in m_RecycleList)
            {
                if (recycleForm.UIFormName.Equals(uiName))
                {
                    uiForm = recycleForm;
                    UnityUtility.InvokeSafely(uiForm.OnResume);
                    break;
                }
            }
            m_RecycleList.Remove(uiForm);
            return uiForm;
        }
        return null;
    }

    public UIForm GetUIForm(string uiName, UIGroup group)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            throw new Exception("uiFormName is invalid");
        }

        if (group == null)
        {
            Log.Error("UI group is null");
            return null;
        }

        UIForm uiForm = group.GetUIForm(uiName);
        return uiForm;
    }

    /// <summary>
    /// 获取所有已加载的界面
    /// </summary>
    /// <returns>所有已加载的界面</returns>
    public UIForm[] GetAllLoadedUIForms()
    {
        List<UIForm> results = new List<UIForm>();
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            results.AddRange(uiGroup.Value.GetAllUIForms());
        }

        return results.ToArray();
    }

    /// <summary>
    /// 获取所有已加载的界面
    /// </summary>
    /// <param name="results">所有已加载的界面</param>
    public void GetAllLoadedUIForms(List<UIForm> results)
    {
        if (results == null)
        {
            throw new Exception("Results is invalid.");
        }

        results.Clear();
        foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
        {
            uiGroup.Value.GetAllUIForms(results);
        }
    }

    /// <summary>
    /// 是否正在加载界面
    /// </summary>
    public bool IsLoadingUIForm(string uiFormName)
    {
        if (string.IsNullOrEmpty(uiFormName))
        {
            throw new Exception("UI form name is invalid.");
        }

        if (m_UIFormsBeingLoaded.TryGetValue(uiFormName, out bool isLoading)) 
        {
            return isLoading;
        }

        return false;
    }
    
    public void CreateUIForm(string uiName,UIFormType uiFormType, Action<UIForm> createSuccessAction, Action createFailAction)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Log.Error($"UIName is Null!");
            return;
        }

        if (IsLoadingUIForm(uiName))
        {
            Log.Warning($"{uiName} is Loading!!!");
            return;
        }

        UIForm uiForm = Create(uiName);
        if (uiForm == null)
        {
            int serialId = ++s_SerialId;
            m_UIFormsBeingLoaded[uiName] = true;

            isAsyncLoading = true;

            UIGroup uiGroup = uiFormType != UIFormType.None ? GetUIGroup(uiFormType) : null;
            
            m_UIFormHelper.CreateUIForm(uiName,uiGroup, fm =>
            {
                if (uiGroup != null && fm.UIFormType == uiFormType) 
                    SetUIGroup(fm, uiName, uiGroup);
                else
                    SetUIGroup(fm,uiName);
                
                m_UIFormsBeingLoaded.Remove(uiName);
                isAsyncLoading = false;
                
                fm.gameObject.SetActive(false);
                fm.OnCreate(serialId);
                createSuccessAction?.Invoke(fm);
            }, () =>
            {
                m_UIFormsBeingLoaded.Remove(uiName);
                createFailAction?.Invoke();
            });
        }
        else
        {
            uiForm.OnReset();
            uiForm.OnRelease();

            SetUIGroup(uiForm,uiName);
            createSuccessAction?.Invoke(uiForm);
        }
    }

    private void SetUIGroup(UIForm uiForm, string uiFormName)
    {
        var uiGroup = GetUIGroup(uiForm.UIFormType);
        uiForm.transform.SetParent(uiGroup.transform, false);
        SetUIGroup(uiForm, uiFormName, uiGroup);
    }

    private void SetUIGroup(UIForm uiForm, string uiFormName, UIGroup uiGroup)
    {
        uiForm.SetGroup(uiGroup);
        uiForm.SetUIFormName(uiFormName);
        uiGroup.AddUIForm(uiForm);
    }
    
    public void ShowUIForm(string uiName,UIFormType uiFormType, Action<UIForm> showSuccessAction = null, Action showFailAction = null,
        object userData = null)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Log.Error($"UIName is Null!");
            return;
        }

        CreateUIForm(uiName,uiFormType,form =>
        {
            form.OnInit(GetUIGroup(form.UIFormType), () =>
            {
                form.OnShowInit(userData:null);
                form.gameObject.SetActive(true);
                form.UIGroup.Refresh(false);
                form.OnShow(showSuccessAction, userData);
            }, userData);
        }, showFailAction);
    }

    /// <summary>
    /// 隐藏界面
    /// </summary>
    /// <param name="uiFormName">界面名称</param>
    /// <param name="onHideComplete">隐藏完毕事件</param>
    /// <param name="userData">自定义数据</param>
    public void HideUIForm(string uiFormName, Action onHideComplete = null, object userData = null)
    {
        UIForm uiForm = GetUIForm(uiFormName);

        if (uiForm == null)
        {
            //Log.Error("UI Form {0} is null.", uiFormName);
            onHideComplete?.Invoke();
            return;
        }

        HideUIForm(uiForm, onHideComplete, userData);
    }

    /// <summary>
    /// 隐藏界面
    /// </summary>
    /// <param name="uiForm">界面</param>
    /// <param name="onHideComplete">隐藏完毕事件</param>
    /// <param name="userData">自定义数据</param>
    public void HideUIForm(UIForm uiForm, Action onHideComplete = null, object userData = null)
    {
        if(uiForm==null)return;
            
        if(uiForm.UIFormType==UIFormType.FuzzyBoundaryUI)return;
        
        if (m_RecycleList.Contains(uiForm))
        {
            uiForm.gameObject.SetActive(false);
            return;
        }

        UIGroup uiGroup = uiForm.UIGroup;

        if (uiGroup == null)
        {
            Log.Error("UI group is invalid.{0}", UnityUtility.GetTypeName(uiForm.GetType()));
            uiForm.gameObject.SetActive(false);
            uiForm.OnRelease();
            onHideComplete?.Invoke();
            return;
        }

        uiForm.OnResume();
        uiForm.OnReset();
        uiForm.OnHide(onHideComplete, userData);
        uiForm.OnRelease();
        uiForm.gameObject.SetActive(false);
        
        uiGroup.RemoveUIForm(uiForm);
        uiGroup.Refresh();
        m_RecycleList.AddLast(uiForm);
        onHideComplete?.Invoke();
    }

    public void HideAllUIForm(string[] uiExceptFormsName)
    {
        foreach (var group in m_UIGroups)
        {
            foreach (var form in group.Value.GetAllUIForms())
            {
                bool isHide = true;
                if (uiExceptFormsName != null)
                    foreach (var ui in uiExceptFormsName)
                    {
                        if (ui.Equals(form.UIFormName))
                        {
                            isHide = false;
                            break;
                        }
                    }
                if (isHide) HideUIForm(form);
            }
        }
    }

    public void HideAllUIForm(params UIForm[] uiExceptForms)
    {
        foreach (var group in m_UIGroups)
        {
            foreach (var form in group.Value.GetAllUIForms())
            {
                bool isHide = true;
                if(uiExceptForms!=null)
                    foreach (var ui in uiExceptForms)
                    {
                        if (ui!=null&&ui.Equals(form))
                        {
                            isHide = false;
                            break;
                        }
                    }
                if(isHide) HideUIForm(form);
            }
        }
    }

    public void ReleaseUIForm(UIForm uiForm)
    {
        if (uiForm == null) return;

        if (uiForm.UIFormType == UIFormType.FuzzyBoundaryUI) return;

        if (m_RecycleList.Contains(uiForm))
        {
            return;
        }

        UIGroup uiGroup = uiForm.UIGroup;
        if (uiGroup == null)
        {
            Log.Error("UI group is invalid.{0}", UnityUtility.GetTypeName(uiForm.GetType()));
            uiForm.OnRelease();
            m_UIFormHelper.ReleaseUIForm(uiForm);
            return;
        }

        uiForm.OnReset();
        uiForm.OnRelease();
        uiGroup.RemoveUIForm(uiForm);
        uiGroup.Refresh();

        m_UIFormHelper.ReleaseUIForm(uiForm);
    }
    
    public void ReleaseAllUIForm(params UIForm[] uiExceptForms)
    {
        HideAllUIForm(uiExceptForms);

        List<UIForm> recordForms = new List<UIForm>();
        while (m_RecycleList.Count>0)
        {
            var form = m_RecycleList.First.Value;
            if (uiExceptForms!=null&&!uiExceptForms.Contains(form))
            {
                UnityUtility.UnloadInstance(form.gameObject);
            }
            else
            {
                recordForms.Add(form);
            }
            m_RecycleList.RemoveFirst();
        }

        while (recordForms.Count>0)
        {
            m_RecycleList.AddLast(recordForms[0]);
            recordForms.RemoveAt(0);
        }
        GameManager.UI.StartGarbageCollection();
    }

    public void RefreshAllUIByLogin()
    {
        foreach (var group in m_UIGroups)
        {
            foreach (var form in group.Value.GetAllUIForms())
            {
                var uiName = form.UIFormName;
                GameManager.UI.HideUIForm(form);
                GameManager.UI.ShowUIForm(uiName);
            }
        }
    }
}
