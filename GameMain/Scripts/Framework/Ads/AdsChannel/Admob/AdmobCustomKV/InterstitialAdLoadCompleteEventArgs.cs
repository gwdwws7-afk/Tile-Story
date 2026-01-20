using GameFramework.Event;

public class InterstitialAdLoadCompleteEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(InterstitialAdLoadCompleteEventArgs).GetHashCode();

    public override int Id => EventId;
    
    public static InterstitialAdLoadCompleteEventArgs Create()
    {
        InterstitialAdLoadCompleteEventArgs eventArgs = GameFramework.ReferencePool.Acquire<InterstitialAdLoadCompleteEventArgs>();
        return eventArgs;
    }
    
    public override void Clear()
    {
    }
}
