using GameFramework.Event;

namespace Merge
{
    public class MergeOfferCompleteEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(MergeOfferCompleteEventArgs).GetHashCode();

        public MergeOfferCompleteEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static MergeOfferCompleteEventArgs Create()
        {
            MergeOfferCompleteEventArgs eventArgs = GameFramework.ReferencePool.Acquire<MergeOfferCompleteEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
