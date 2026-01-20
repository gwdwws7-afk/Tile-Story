using GameFramework.Event;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝活动开启事件
    /// </summary>
    public sealed class HiddenTempleStartEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(HiddenTempleStartEventArgs).GetHashCode();

        public HiddenTempleStartEventArgs()
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

        public static HiddenTempleStartEventArgs Create(int periodId)
        {
            HiddenTempleStartEventArgs eventArgs = GameFramework.ReferencePool.Acquire<HiddenTempleStartEventArgs>();
            eventArgs.PeriodId = periodId;
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
