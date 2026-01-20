using GameFramework;
using System;

public sealed class AdsEvent : IReference
{
    public Action action;
    public float delayTime;

    public static AdsEvent Create(Action action, float delayTime=0)
    {
        AdsEvent adsEvent = ReferencePool.Acquire<AdsEvent>();
        adsEvent.action = action;
        adsEvent.delayTime = delayTime;

        return adsEvent;
    }

    public void Clear()
    {
        action = null;
        delayTime = 0;
    }
}
