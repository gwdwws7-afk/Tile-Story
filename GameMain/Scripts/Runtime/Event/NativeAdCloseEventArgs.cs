using GameFramework.Event;

/// <summary>
/// 奖励广告奖励获取事件
/// </summary>
public sealed class NativeAdCloseEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(NativeAdCloseEventArgs).GetHashCode();

    public NativeAdCloseEventArgs()
    {
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public object UserData
    {
        get;
        private set;
    }


    public static NativeAdCloseEventArgs Create(object userData)
    {
        NativeAdCloseEventArgs args = GameFramework.ReferencePool.Acquire<NativeAdCloseEventArgs>();
        args.UserData = userData;
        return args;
    }

    public override void Clear()
    {
    }
}
