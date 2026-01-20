using System;

/// <summary>
/// 数据表基类
/// </summary>
public abstract class DataTableBase
{
    private readonly string name;

    public DataTableBase()
        : this(null)
    {
    }

    public DataTableBase(string name)
    {
        this.name = name ?? string.Empty;
    }

    public string Name { get => name; }

    public string FullName { get => new TypeNamePair(Type, name).ToString(); }

    public abstract Type Type { get; }

    public abstract void ReadData(string dataTableAssetName, Action<GameEventMessage> readDataSuccess, Action<GameEventMessage> readDataFailure);

    public abstract void Shutdown();
}
