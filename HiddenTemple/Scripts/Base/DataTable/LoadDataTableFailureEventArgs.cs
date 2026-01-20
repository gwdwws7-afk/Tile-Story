using GameFramework;
using GameFramework.Event;

namespace HiddenTemple
{
    /// <summary>
    /// 加载数据表失败事件
    /// </summary>
    public sealed class LoadDataTableFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableFailureEventArgs).GetHashCode();

        public LoadDataTableFailureEventArgs()
        {
            DataTableAssetName = null;
            ErrorMessage = null;
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
        /// 错误信息
        /// </summary>
        public string ErrorMessage
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

        public static LoadDataTableFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadDataTableFailureEventArgs eventArgs = ReferencePool.Acquire<LoadDataTableFailureEventArgs>();
            eventArgs.DataTableAssetName = e.DataAssetName;
            eventArgs.ErrorMessage = e.ErrorMessage;
            eventArgs.UserData = e.UserData;
            return eventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}