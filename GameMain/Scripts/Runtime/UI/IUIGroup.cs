
/// <summary>
/// 界面组接口
/// </summary>
public interface IUIGroup
{
    /// <summary>
    /// 界面组名称
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// 界面组类型
    /// </summary>
    UIGroupType GroupType { get; }

    /// <summary>
    /// 获取或设置界面组是否暂停
    /// </summary>
    bool Pause { get; set; }

    /// <summary>
    /// 获取界面组中界面数量
    /// </summary>
    int UIFormCount { get; }

    /// <summary>
    /// 获取当前界面
    /// </summary>
    UIForm CurrentUIForm { get; }

    /// <summary>
    /// 界面组中是否存在界面
    /// </summary>
    /// <param name="uiFormName">界面名称</param>
    bool HasUIForm(string uiName);

    /// <summary>
    /// 界面组中获取目标界面
    /// </summary>
    /// <param name="uiFormName">界面名称</param>
    UIForm GetUIForm(string uiFormName);

    /// <summary>
    /// 往界面组添加界面
    /// </summary>
    /// <param name="uiForm">要添加的界面</param>
    void AddUIForm(UIForm uiForm);

    /// <summary>
    /// 从界面组移除界面
    /// </summary>
    /// <param name="uiForm">要移除的界面</param>
    bool RemoveUIForm(UIForm uiForm);

    /// <summary>
    /// 刷新界面组
    /// </summary>
    void Refresh(bool isRefresh);
}
