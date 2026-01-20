using GameFramework.Event;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝活动结束事件
    /// </summary>
    public sealed class HiddenTempleEndEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(HiddenTempleEndEventArgs).GetHashCode();

        public HiddenTempleEndEventArgs()
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
        /// 期数编号
        /// </summary>
        public int PeriodId
        {
            get;
            private set;
        }

        public static HiddenTempleEndEventArgs Create(int periodId)
        {
            HiddenTempleEndEventArgs eventArgs = GameFramework.ReferencePool.Acquire<HiddenTempleEndEventArgs>();
            eventArgs.PeriodId = periodId;
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
