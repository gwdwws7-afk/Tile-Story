

/// <summary>
/// 广告单元接口
/// </summary>
public interface IAdUnit
{
    bool IsLoaded();
    void LoadAd(string[] ids);
    bool Show(object userData);
    void Dispose();
    long GetPrice();
}
