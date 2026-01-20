using GameFramework.Event;

namespace HiddenTemple
{
    /// <summary>
    /// 宝箱奖励获取事件
    /// </summary>
    public sealed class ChestClaimEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ChestClaimEventArgs).GetHashCode();

        public ChestClaimEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static ChestClaimEventArgs Create()
        {
            ChestClaimEventArgs eventArgs = GameFramework.ReferencePool.Acquire<ChestClaimEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
