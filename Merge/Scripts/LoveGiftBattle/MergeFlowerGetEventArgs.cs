using GameFramework.Event;

namespace Merge
{
    public class MergeFlowerGetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(MergeFlowerGetEventArgs).GetHashCode();

        public MergeFlowerGetEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static MergeFlowerGetEventArgs Create()
        {
            MergeFlowerGetEventArgs eventArgs = GameFramework.ReferencePool.Acquire<MergeFlowerGetEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
