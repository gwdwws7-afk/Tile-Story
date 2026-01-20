using GameFramework;
using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 圣诞装修事件
    /// </summary>
    public sealed class ChristmasDecorateEndEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ChristmasDecorateEndEventArgs).GetHashCode();

        public ChristmasDecorateEndEventArgs()
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

        public static ChristmasDecorateEndEventArgs Create()
        {
            ChristmasDecorateEndEventArgs eventArgs = ReferencePool.Acquire<ChristmasDecorateEndEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}