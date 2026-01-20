using GameFramework.Event;

namespace Merge
{
    public class MergeEndEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(MergeEndEventArgs).GetHashCode();

        public MergeEndEventArgs()
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

        public static MergeEndEventArgs Create(int periodId)
        {
            MergeEndEventArgs eventArgs = GameFramework.ReferencePool.Acquire<MergeEndEventArgs>();
            eventArgs.PeriodId = periodId;
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
