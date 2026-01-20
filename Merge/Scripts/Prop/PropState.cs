namespace Merge
{
    /// <summary>
    /// 道具移动状态
    /// </summary>
    public enum PropMovementState : byte
    {
        /// <summary>
        /// 静止
        /// </summary>
        Static,
        /// <summary>
        /// 拖动中
        /// </summary>
        Draging,
        /// <summary>
        /// 自主移动中
        /// </summary>
        Moveing,
        /// <summary>
        /// 弹跳中
        /// </summary>
        Bouncing,
        /// <summary>
        /// 飞行中
        /// </summary>
        Flying,
        /// <summary>
        /// 隐藏
        /// </summary>
        Hide,
    }
}
