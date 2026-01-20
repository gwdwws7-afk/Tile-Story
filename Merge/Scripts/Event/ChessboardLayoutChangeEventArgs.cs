using GameFramework;
using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 棋盘布局发生变动事件
    /// </summary>
    public sealed class ChessboardLayoutChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ChessboardLayoutChangeEventArgs).GetHashCode();

        public ChessboardLayoutChangeEventArgs()
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

        public static ChessboardLayoutChangeEventArgs Create()
        {
            ChessboardLayoutChangeEventArgs eventArgs = ReferencePool.Acquire<ChessboardLayoutChangeEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}