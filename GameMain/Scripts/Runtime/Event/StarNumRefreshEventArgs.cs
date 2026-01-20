using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 星星数量刷新事件
/// </summary>
public sealed class StarNumRefreshEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(StarNumRefreshEventArgs).GetHashCode();

    public StarNumRefreshEventArgs()
    {
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public static StarNumRefreshEventArgs Create()
    {
        StarNumRefreshEventArgs starNumRefreshEventArgs = GameFramework.ReferencePool.Acquire<StarNumRefreshEventArgs>();
        return starNumRefreshEventArgs;
    }

    public override void Clear()
    {
    }
}
