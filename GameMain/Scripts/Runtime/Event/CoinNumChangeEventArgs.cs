using GameFramework.Event;

/// <summary>
/// 金币数量变化事件
/// </summary>
public sealed class CoinNumChangeEventArgs : GameEventArgs
{
    /// <summary>
    /// 金币数量变化事件编号
    /// </summary>
    public static readonly int EventId = typeof(CoinNumChangeEventArgs).GetHashCode();

    public CoinNumChangeEventArgs()
    {
        ChangeNum = 0;
        CoinFlyReceiver = null;
    }

    /// <summary>
    /// 金币数量变化事件编号
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
    /// 金币改变数量
    /// </summary>
    public int ChangeNum
    {
        get;
        private set;
    }

    /// <summary>
    /// 目标金币接收对象
    /// </summary>
    public ICoinFlyReceiver CoinFlyReceiver
    {
        get;
        private set;
    }

    public static CoinNumChangeEventArgs Create(int changeNum, ICoinFlyReceiver flyReceiver)
    {
        CoinNumChangeEventArgs coinNumChangeEventArgs = GameFramework.ReferencePool.Acquire<CoinNumChangeEventArgs>();
        coinNumChangeEventArgs.ChangeNum = changeNum;
        coinNumChangeEventArgs.CoinFlyReceiver = flyReceiver;
        return coinNumChangeEventArgs;
    }

    public override void Clear()
    {
        ChangeNum = 0;
        CoinFlyReceiver = null;
    }
}
