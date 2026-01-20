using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 界面组
/// </summary>
public abstract class UIGroup : MonoBehaviour, IUIGroup
{
    protected readonly LinkedList<UIFormInfo> uiFormInfos = new LinkedList<UIFormInfo>();
    private LinkedListNode<UIFormInfo> cachedNode = null;
    private bool isInit = false;
    private bool isPause = false;
    private bool isCover = false;

    /// <summary>
    /// 界面组名称
    /// </summary>
    public abstract string GroupName { get; }

    /// <summary>
    /// 界面组类型
    /// </summary>
    public abstract UIGroupType GroupType { get; }

    /// <summary>
    /// 获取当前界面
    /// </summary>
    public UIForm CurrentUIForm
    {
        get
        {
            if (uiFormInfos.First == null)
            {
                return null;
            }
            return uiFormInfos.First.Value.UIForm;
        }
    }

    /// <summary>
    /// 获取界面组中界面数量。
    /// </summary>
    public int UIFormCount
    {
        get
        {
            return uiFormInfos.Count;
        }
    }

    public bool Init
    {
        get
        {
            return isInit;
        }
    }

    public bool Pause
    {
        get
        {
            return isPause;
        }
        set
        {
            if (isPause == value)
            {
                return;
            }

            isPause = value;
            Refresh();
        }
    }

    public bool Cover
    {
        get
        {
            return isCover;
        }
        set
        {
            if (isCover == value)
            {
                return;
            }

            isCover = value;
            Refresh();
        }
    }

	public virtual void OnInit()
    {
        if (isInit)
        {
            Log.Warning("Group {0} is already init", GroupName);
            return;
        }
        isInit = true;

        if (uiFormInfos.Count > 0)
        {
            LinkedListNode<UIFormInfo> current = uiFormInfos.First;
            while (current != null)
            {
                current.Value.UIForm.OnInit(this);
                current = current.Next;
            }
        }
    }

    public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            if (current.Value.Paused)
            {
                break;
            }

            cachedNode = current.Next;
            current.Value.UIForm.OnUpdate(elapseSeconds, realElapseSeconds);
            current = cachedNode;
            cachedNode = null;
        }
    }

    public virtual void OnReset()
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            current.Value.UIForm.OnReset();
            current = current.Next;
        }

        isInit = false;
    }

    public virtual void OnRelease()
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            LinkedListNode<UIFormInfo> next = current.Next;
            GameFramework.ReferencePool.Release(current.Value);

            current = next;
        }
        uiFormInfos.Clear();

        isInit = false;
    }

    public virtual void OnShow()
    {
    }

    public virtual void OnHide(bool skipAnim)
    {
    }

    /// <summary>
    /// 显示所有已加载的界面
    /// </summary>
    public void ShowAllLoadedUIForms()
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            if (current.Value.Paused)
            {
                current.Value.Paused = false;
                current.Value.UIForm.OnResume();
            }

            if (current.Value.Covered)
            {
                current.Value.Covered = false;
                current.Value.UIForm.OnReveal();
            }

            current.Value.UIForm.OnShow();
            current = current.Next;
        }
    }

    /// <summary>
    /// 重新激活所有已加载的界面
    /// </summary>
    public void RefocusAllLoadedUIForms()
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            current.Value.UIForm.OnRefocus();
            current = current.Next;
        }
    }

    /// <summary>
    /// 移除组中所有遮挡的界面
    /// </summary>
    public void RemoveAllCoveredUIForms()
    {
        LinkedListNode<UIFormInfo> current = uiFormInfos.First;
        while (current != null)
        {
            LinkedListNode<UIFormInfo> next = current.Next;
            if (current.Value.Covered)
            {
                GameManager.UI.HideUIForm(current.Value.UIForm);
            }
            current = next;
        }
    }

    public bool HasUIForm(string uiName)
    {
        foreach (UIFormInfo uiFormInfo in uiFormInfos)
        {
            if (uiFormInfo.UIForm.UIFormName.Equals(uiName))
            {
                return true;
            }
        }

        return false;
    }
    
    public UIForm GetUIForm(string uiFormName)
    {
        foreach (UIFormInfo uiFormInfo in uiFormInfos)
        {
            if (uiFormInfo.UIForm.UIFormName.Equals(uiFormName))
            {
                return uiFormInfo.UIForm;
            }
        }

        return null;
    }

    /// <summary>
    /// 从界面组中获取所有界面
    /// </summary>
    /// <returns>界面组中的所有界面</returns>
    public UIForm[] GetAllUIForms()
    {
        List<UIForm> results = new List<UIForm>();
        foreach (UIFormInfo uiFormInfo in uiFormInfos)
        {
            results.Add(uiFormInfo.UIForm);
        }

        return results.ToArray();
    }

    /// <summary>
    /// 从界面组中获取所有界面
    /// </summary>
    /// <param name="results">界面组中的所有界面</param>
    public void GetAllUIForms(List<UIForm> results)
    {
        if (results == null)
        {
            throw new Exception("Results is invalid.");
        }

        results.Clear();
        foreach (UIFormInfo uiFormInfo in uiFormInfos)
        {
            results.Add(uiFormInfo.UIForm);
        }
    }

    /// <summary>
    /// 往界面组添加界面
    /// </summary>
    /// <param name="uiForm">要添加的界面</param>
    public void AddUIForm(UIForm uiForm)
    {
        foreach (var uiFormInfo in uiFormInfos)
        {
            if (uiFormInfo.UIForm.Equals(uiForm))
            {
                uiFormInfo.Paused = false;
                uiFormInfo.Covered = true;
                UnityUtility.InvokeSafely(uiFormInfo.UIForm.OnResume);
                return;
            }
        }

        uiFormInfos.AddFirst(UIFormInfo.Create(uiForm));
    }

    /// <summary>
    /// 从界面组移除界面
    /// </summary>
    /// <param name="uiForm">要移除的界面</param>
    public bool RemoveUIForm(UIForm uiForm)
    {
        UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);

        if (uiFormInfo == null)
        {
            Log.Warning("Can not find UI form info {0}", uiForm.GetType().Name);
            return false;
        }

        if (!uiFormInfo.Covered)
        {
            uiFormInfo.Covered = true;
            UnityUtility.InvokeSafely(uiForm.OnCover);
        }

        if (!uiFormInfo.Paused)
        {
            uiFormInfo.Paused = true;
            UnityUtility.InvokeSafely(uiForm.OnPause);
        }

        if (cachedNode != null && cachedNode.Value.UIForm == uiForm) 
        {
            cachedNode = cachedNode.Next;
        }

        if (!uiFormInfos.Remove(uiFormInfo))
        {
            Log.Error("UI group '{0}' not exists specified UI form {1}", GroupName, uiFormInfo.UIForm.UIName);
        }

        GameFramework.ReferencePool.Release(uiFormInfo);
        return true;
    }

    /// <summary>
    /// 刷新界面组
    /// </summary>
    public void Refresh(bool isRefresh=true)
    {
        var current = uiFormInfos.First;
        bool pause = Pause;
        bool cover = false;
        int depth = UIFormCount;

        if (GroupType == UIGroupType.Static)
        {
            cover = Cover;

            while (current != null && current.Value != null)
            {
                LinkedListNode<UIFormInfo> next = current.Next;
                depth--;
                current.Value.UIForm.OnDepthChanged(UIFormCount, depth);
                if (current.Value == null)
                {
                    return;
                }

                if (cover)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnCover);
                    }
                }
                else
                {
                    if (current.Value.Covered)
                    {
                        current.Value.Covered = false;
                        if(isRefresh)UnityUtility.InvokeSafely(current.Value.UIForm.OnReveal);
                    }
                }

                if (pause)
                {
                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnPause);
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnResume);
                    }
                }

                current = next;
            }

            RefreshLayout();
        }
        else if (GroupType == UIGroupType.Dynamic)
        {
            while (current != null && current.Value != null)
            {
                LinkedListNode<UIFormInfo> next = current.Next;
                depth--;
                current.Value.UIForm.OnDepthChanged(UIFormCount, depth);
                if (current.Value == null || current.Value.UIForm == null)
                {
                    return;
                }

                if (pause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnCover);
                        if (current.Value == null || current.Value.UIForm == null)
                        {
                            return;
                        }
                    }

                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnPause);
                        if (current.Value == null || current.Value.UIForm == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        UnityUtility.InvokeSafely(current.Value.UIForm.OnResume);
                        if (current.Value == null || current.Value.UIForm == null)
                        {
                            return;
                        }
                    }

                    if (cover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            UnityUtility.InvokeSafely(current.Value.UIForm.OnCover);
                            if (current.Value == null || current.Value.UIForm == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            if(isRefresh)UnityUtility.InvokeSafely(current.Value.UIForm.OnReveal);
                            if (current.Value == null || current.Value.UIForm == null) 
                            {
                                return;
                            }
                        }
                    }

                    cover = true;
                }

                current = next;
            }
        }
    }
    
    /// <summary>
    /// 刷新界面布局
    /// </summary>
    public virtual void RefreshLayout()
    {
    }

    private UIFormInfo GetUIFormInfo(UIForm uiForm)
    {
        if (uiForm == null)
        {
            Log.Error("UI Form is valid");
            return null;
        }

        foreach (UIFormInfo uiFormInfo in uiFormInfos)
        {
            if (uiFormInfo.UIForm == uiForm)
            {
                return uiFormInfo;
            }
        }

        return null;
    }
}
