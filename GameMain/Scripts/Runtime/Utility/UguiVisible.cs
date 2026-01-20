using System;
using UnityEngine;

/// <summary>
/// 确认ugui是否在任何摄像机上可见
/// </summary>
public class UguiVisible : MonoBehaviour
{
    private bool m_IsVisible;

    public bool IsVisible { get { return m_IsVisible; } }

    public Action<bool> OnChangeVisibleCallback;

    private void OnBecameVisible()
    {
        if (!m_IsVisible)
        {
            m_IsVisible = true;
            OnChangeVisibleCallback?.Invoke(true);
        }
    }

    private void OnBecameInvisible()
    {
        if (m_IsVisible)
        {
            m_IsVisible = false;
            OnChangeVisibleCallback?.Invoke(false);
        }
    }
}
