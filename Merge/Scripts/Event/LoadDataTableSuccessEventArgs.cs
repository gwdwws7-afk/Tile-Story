using GameFramework;
using GameFramework.Event;

namespace Merge
{
    /// <summary>
    /// 加载数据表成功事件
    /// </summary>
    public sealed class LoadDataTableSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableSuccessEventArgs).GetHashCode();

        public LoadDataTableSuccessEventArgs()
        {
            DataTableAssetName = null;
            Duration = 0f;
            UserData = null;
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

        /// <summary>
        /// 数据表资源名称
        /// </summary>
        public string DataTableAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 加载持续时间
        /// </summary>
        public float Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        public static LoadDataTableSuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadDataTableSuccessEventArgs eventArgs = ReferencePool.Acquire<LoadDataTableSuccessEventArgs>();
            eventArgs.DataTableAssetName = e.DataAssetName;
            eventArgs.Duration = e.Duration;
            eventArgs.UserData = e.UserData;
            return eventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}