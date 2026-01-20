using GameFramework.Event;
using System;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 事件组件
    /// </summary>
    public sealed class EventComponent : GameFrameworkComponent
    {
        private IEventManager m_EventManager = null;

        /// <summary>
        /// 获取订阅的事件类型数量
        /// </summary>
        public int EventHandlerCount
        {
            get
            {
                return m_EventManager.EventHandlerCount;
            }
        }

        /// <summary>
        /// 获取将要抛出的事件数量
        /// </summary>
        public int EventCount
        {
            get
            {
                return m_EventManager.EventCount;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_EventManager = GameFrameworkEntry.GetModule<EventManager>();
            if (m_EventManager == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 获取该事件类型订阅的事件处理函数数量
        /// </summary>
        /// <param name="id">事件类型编号</param>
        /// <returns>订阅的事件处理函数数量</returns>
        public int Count(int id)
        {
            return m_EventManager.Count(id);
        }

        /// <summary>
        /// 检查该事件类型是否订阅了该事件处理函数
        /// </summary>
        /// <param name="id">事件类型编号</param>
        /// <param name="handler">要检查的事件处理函数</param>
        /// <returns>是否订阅了该事件处理函数</returns>
        public bool Check(int id, EventHandler<GameEventArgs> handler)
        {
            return m_EventManager.Check(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数
        /// </summary>
        /// <param name="id">事件类型编号</param>
        /// <param name="handler">要订阅的事件处理函数</param>
        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            m_EventManager.Subscribe(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数
        /// </summary>
        /// <param name="id">事件类型编号</param>
        /// <param name="handler">要取消订阅的事件处理函数</param>
        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            m_EventManager.Unsubscribe(id, handler);
        }

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void Fire(object sender, GameEventArgs e)
        {
            m_EventManager.Fire(sender, e);
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void FireNow(object sender, GameEventArgs e)
        {
            m_EventManager.FireNow(sender, e);
        }
    }
}