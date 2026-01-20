using GameFramework;
using System.Collections.Generic;

public sealed class GameEventMessage : IReference
{
    private readonly List<object> args;

    public GameEventMessage()
    {
        args = new List<object>();
    }

    public List<object> Items
    {
        get
        {
            return args;
        }
    }

    public static GameEventMessage Create(params object[] arg)
    {
        GameEventMessage message = ReferencePool.Acquire<GameEventMessage>();

        if (arg != null)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                message.AddArgs(arg[i]);
            }
        }

        return message;
    }

    public void AddArgs(object arg)
    {
        args.Add(arg);
    }

    public void Clear()
    {
        args.Clear();
    }
}
