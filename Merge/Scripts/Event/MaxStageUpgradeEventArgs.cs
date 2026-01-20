using GameFramework;
using GameFramework.Event;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 最大道具合成等级提升事件
    /// </summary>
    public sealed class MaxStageUpgradeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(MaxStageUpgradeEventArgs).GetHashCode();

        public MaxStageUpgradeEventArgs()
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

        /// <summary>
        /// 合成的道具编号
        /// </summary>
        public int PropId
        {
            get;
            private set;
        }

        /// <summary>
        /// 合成的道具
        /// </summary>
        public PropLogic PropLogic
        {
            get;
            private set;
        }

        /// <summary>
        /// 道具位置
        /// </summary>
        public Vector3 PropPos
        {
            get;
            private set;
        }

        public static MaxStageUpgradeEventArgs Create(int propId, PropLogic propLogic, Vector3 propPos)
        {
            MaxStageUpgradeEventArgs eventArgs = ReferencePool.Acquire<MaxStageUpgradeEventArgs>();
            eventArgs.PropId = propId;
            eventArgs.PropLogic = propLogic;
            eventArgs.PropPos = propPos;
            return eventArgs;
        }

        public override void Clear()
        {
        }
    }
}