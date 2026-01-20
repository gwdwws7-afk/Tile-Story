using GameFramework.Event;

namespace HiddenTemple
{
    /// <summary>
    /// 稿子数量变化事件
    /// </summary>
    public sealed class PickaxeNumChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PickaxeNumChangeEventArgs).GetHashCode();

        public PickaxeNumChangeEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static PickaxeNumChangeEventArgs Create()
        {
            PickaxeNumChangeEventArgs eventArgs = GameFramework.ReferencePool.Acquire<PickaxeNumChangeEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
