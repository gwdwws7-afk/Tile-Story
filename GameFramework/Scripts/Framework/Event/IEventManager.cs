using System;

namespace GameFramework.Event
{
    /// <summary>
    /// 事件管理器接口
    /// </summary>
    public interface IEventManager
    {
        int EventHandlerCount { get; }

        int EventCount { get; }

        int Count(int id);

        bool Check(int id, EventHandler<GameEventArgs> handler);

        void Subscribe(int id, EventHandler<GameEventArgs> handler);

        void Unsubscribe(int id, EventHandler<GameEventArgs> handler);

        void Fire(object sender, GameEventArgs e);

        void FireNow(object sender, GameEventArgs e);
    }
}