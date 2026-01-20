using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ������ʵ��
/// </summary>
public abstract class AttachItem : MonoBehaviour
{
    protected AttachLogic m_Logic;

    public virtual void Init(AttachLogic logic)
    {
        m_Logic = logic;
    }

    public virtual void Release()
    {
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public abstract void SetColor(bool isBeCover);

    public virtual void SetSprite(int state) { }

    /// <summary>
    /// �������ʱ
    /// </summary>
    public virtual void OnClick() { }

    public virtual void OnAttachStateChange(int attachState) { }
    
    public virtual void SetDisappearAction(Action action)
    {
        
    }
}
