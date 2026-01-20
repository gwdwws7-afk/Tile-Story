using GameFramework;
using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 选中道具变化事件
    /// </summary>
    public sealed class SelectedPropChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(SelectedPropChangeEventArgs).GetHashCode();

        public SelectedPropChangeEventArgs()
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

        public static SelectedPropChangeEventArgs Create()
        {
            SelectedPropChangeEventArgs eventArgs = ReferencePool.Acquire<SelectedPropChangeEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}