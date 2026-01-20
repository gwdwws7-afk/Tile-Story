using GameFramework.Event;

namespace Merge
{
    public class MergeStartEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(MergeStartEventArgs).GetHashCode();

        public MergeStartEventArgs()
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
        /// ÆÚÊý±àºÅ
        /// </summary>
        public int PeriodId
        {
            get;
            private set;
        }

        public static MergeStartEventArgs Create(int periodId)
        {
            MergeStartEventArgs eventArgs = GameFramework.ReferencePool.Acquire<MergeStartEventArgs>();
            eventArgs.PeriodId = periodId;
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
