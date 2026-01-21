using GameFramework;
using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 圣诞装修事件
    /// </summary>
    public sealed class DogDecorateEndEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DogDecorateEndEventArgs).GetHashCode();

        public DogDecorateEndEventArgs()
        {
        }

        /// <summary>
        /// 事件编号
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static DogDecorateEndEventArgs Create()
        {
            DogDecorateEndEventArgs eventArgs = ReferencePool.Acquire<DogDecorateEndEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}