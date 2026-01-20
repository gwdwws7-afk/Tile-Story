using GameFramework.Event;

/// <summary>
/// 生命数量变化事件
/// </summary>
public sealed class LifeNumChangeEventArgs : GameEventArgs
{
    /// <summary>
    /// 生命数量变化事件编号
    /// </summary>
    public static readonly int EventId = typeof(LifeNumChangeEventArgs).GetHashCode();

    public LifeNumChangeEventArgs()
    {
        ChangeNum = 0;
        LifeFlyReceiver = null;
    }

    /// <summary>
    /// 生命数量变化事件编号
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
    /// 生命改变数量
    /// </summary>
    public int ChangeNum
    {
        get;
        private set;
    }

    /// <summary>
    /// 目标生命栏
    /// </summary>
    public ILifeFlyReceiver LifeFlyReceiver
    {
        get;
        private set;
    }

    public static LifeNumChangeEventArgs Create(int changeNum, ILifeFlyReceiver flyReceiver)
    {
        LifeNumChangeEventArgs lifeNumChangeEventArgs = GameFramework.ReferencePool.Acquire<LifeNumChangeEventArgs>();
        lifeNumChangeEventArgs.ChangeNum = changeNum;
        lifeNumChangeEventArgs.LifeFlyReceiver = flyReceiver;
        return lifeNumChangeEventArgs;
    }

    public override void Clear()
    {
        ChangeNum = 0;
        LifeFlyReceiver = null;
    }
}
