using GameFramework.Event;

/// <summary>
/// 插屏关闭事件参数
/// </summary>
public class InterstitialAdCloseEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(InterstitialAdCloseEventArgs).GetHashCode();

    public InterstitialAdCloseEventArgs()
    {
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
    /// 是否展示成功
    /// </summary>
    public bool ShowSuccess
    {
        get;
        private set;
    }

    public static InterstitialAdCloseEventArgs Create(bool showSuccess)
    {
        InterstitialAdCloseEventArgs eventArgs = GameFramework.ReferencePool.Acquire<InterstitialAdCloseEventArgs>();
        eventArgs.ShowSuccess = showSuccess;
        return eventArgs;
    }

    public override void Clear()
    {
        ShowSuccess = false;
    }
}
