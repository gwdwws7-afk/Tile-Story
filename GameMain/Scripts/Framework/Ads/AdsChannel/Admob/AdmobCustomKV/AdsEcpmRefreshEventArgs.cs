using GameFramework.Event;

public class AdsEcpmRefreshEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(AdsEcpmRefreshEventArgs).GetHashCode();

    public override int Id => EventId;

    public bool IsRV
    {
        get;
        private set;
    }
    
    public static AdsEcpmRefreshEventArgs Create(bool isRV)
    {
        AdsEcpmRefreshEventArgs eventArgs = GameFramework.ReferencePool.Acquire<AdsEcpmRefreshEventArgs>();
        eventArgs.IsRV = isRV;
        return eventArgs;
    }
    
    public override void Clear()
    {
    }
}
