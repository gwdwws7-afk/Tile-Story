using System;

/// <summary>
/// 界面辅助器接口
/// </summary>
public interface IUIFormHelper
{
    /// <summary>
    /// 创建界面
    /// </summary>
    void CreateUIForm(string uiName, UIGroup uiGroup, Action<UIForm> successAction = null, Action failAction = null);

    /// <summary>
    /// 释放界面
    /// </summary>
    void ReleaseUIForm(UIForm form);
}
