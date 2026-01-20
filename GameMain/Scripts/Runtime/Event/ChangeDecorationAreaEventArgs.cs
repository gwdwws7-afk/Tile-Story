using GameFramework.Event;

/// <summary>
/// 变更DecorationArea事件
/// </summary>
public sealed class ChangeDecorationAreaEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(ChangeDecorationAreaEventArgs).GetHashCode();

    public ChangeDecorationAreaEventArgs()
    {
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public int NewAreaID
    {
        get;
        private set;
    }


    public static ChangeDecorationAreaEventArgs Create(int inputNewAreaID)
    {
        ChangeDecorationAreaEventArgs args = GameFramework.ReferencePool.Acquire<ChangeDecorationAreaEventArgs>();
        args.NewAreaID = inputNewAreaID;
        return args;
    }

    public override void Clear()
    {
    }
}
