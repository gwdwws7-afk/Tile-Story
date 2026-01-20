using System;
using System.Runtime.InteropServices;

/// <summary>
/// 类型和名称的组合值
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct TypeNamePair : IEquatable<TypeNamePair>
{
    private readonly Type type;
    private readonly string name;

    public TypeNamePair(Type type)
        : this(type, string.Empty)
    {
    }

    public TypeNamePair(Type type, string name)
    {
        this.type = type ?? throw new Exception("Type is invalid.");
        this.name = name ?? string.Empty;
    }

    /// <summary>
    /// 获取类型
    /// </summary>
    public Type Type { get => type; }

    /// <summary>
    /// 获取名称
    /// </summary>
    public string Name { get => name; }

    /// <summary>
    /// 获取类型和名称的组合值字符串
    /// </summary>
    /// <returns>类型和名称的组合值字符串</returns>
    public override string ToString()
    {
        if (type == null)
        {
            throw new Exception("Type is invalid.");
        }

        string typeName = type.FullName;
        return string.IsNullOrEmpty(name) ? typeName : string.Format("{0}.{1}", typeName, name);
    }

    /// <summary>
    /// 获取对象的哈希值。
    /// </summary>
    /// <returns>对象的哈希值。</returns>
    public override int GetHashCode()
    {
        return type.GetHashCode() ^ name.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is TypeNamePair && Equals((TypeNamePair)obj);
    }

    public bool Equals(TypeNamePair value)
    {
        return type == value.type && name == value.name;
    }

    public static bool operator ==(TypeNamePair a,TypeNamePair b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(TypeNamePair a,TypeNamePair b)
    {
        return !(a == b);
    }
}
