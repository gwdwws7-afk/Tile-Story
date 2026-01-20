using GameFramework;
using System.Collections.Generic;

/// <summary>
/// 保存的数据
/// </summary>
public sealed class StoreData:IReference
{
    private readonly List<object> data;

    public StoreData()
    {
        data = new List<object>();
    }

    /// <summary>
    /// 数据
    /// </summary>
    public List<object> Data { get => data; }

    public static StoreData Create(params object[] arg)
    {
        StoreData storeData = new StoreData();

        if (arg != null)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                storeData.data.Add(arg[i]);
            }
        }
        return storeData;
    }

    public void Add(object arg)
    {
        data.Add(arg);
    }

    public void Clear()
    {
        data.Clear();
    }
}
