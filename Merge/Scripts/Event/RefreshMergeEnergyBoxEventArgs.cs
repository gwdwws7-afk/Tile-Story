using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 需要刷新合成体力宝箱显示事件
    /// </summary>
    public sealed class RefreshMergeEnergyBoxEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(RefreshMergeEnergyBoxEventArgs).GetHashCode();

        public RefreshMergeEnergyBoxEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static RefreshMergeEnergyBoxEventArgs Create()
        {
            RefreshMergeEnergyBoxEventArgs eventArgs = GameFramework.ReferencePool.Acquire<RefreshMergeEnergyBoxEventArgs>();
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}
