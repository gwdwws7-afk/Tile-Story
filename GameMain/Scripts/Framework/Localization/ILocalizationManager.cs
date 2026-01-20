using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 本地化管理器接口。
/// </summary>
public interface ILocalizationManager
{
    /// <summary>
    /// 获取系统语言。
    /// </summary>
    Language SystemLanguage { get; }

    /// <summary>
    /// 获取或设置本地化语言。
    /// </summary>
    Language Language { get; set; }

    /// <summary>
    /// 当前字体资源
    /// </summary>
    TMP_FontAsset CurrentFont { get; }

    /// <summary>
    /// 初始化
    /// </summary>
    void Init();

    /// <summary>
    /// 是否初始化完毕
    /// </summary>
    bool CheckInitComplete();

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    string GetString(string key);

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <param name="arg">字典参数。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    string GetString(string key, params string[] arg);

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="completeAction">成功事件</param>
    void GetPresetMaterialAsync(string name, Action<Material> completeAction);

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="fontName">字体名称</param>
    /// <param name="completeAction">成功事件</param>
    void GetPresetMaterialAsync(string name, string fontName, Action<Material> completeAction);
}
