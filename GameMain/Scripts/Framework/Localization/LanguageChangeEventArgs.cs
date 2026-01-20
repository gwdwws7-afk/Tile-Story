using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 语言变化事件
/// </summary>
public sealed class LanguageChangeEventArgs : GameEventArgs
{
    /// <summary>
    /// 语言变化事件编号
    /// </summary>
    public static readonly int EventId = typeof(LanguageChangeEventArgs).GetHashCode();

    public LanguageChangeEventArgs()
    {
        NewLanguage = Language.Unspecified;
        UserData = null;
    }

    /// <summary>
    /// 获取语言变化事件编号
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
    /// 新的语言
    /// </summary>
    public Language NewLanguage
    {
        get;
        private set;
    }

    /// <summary>
    /// 获取用户自定义数据
    /// </summary>
    public object UserData
    {
        get;
        private set;
    }

    public static LanguageChangeEventArgs Create(Language language, object userData)
    {
        LanguageChangeEventArgs languageChangeEventArgs = GameFramework.ReferencePool.Acquire<LanguageChangeEventArgs>();
        languageChangeEventArgs.NewLanguage = language;
        languageChangeEventArgs.UserData = userData;
        return languageChangeEventArgs;
    }

    public override void Clear()
    {
        NewLanguage = Language.Unspecified;
        UserData = null;
    }
}
