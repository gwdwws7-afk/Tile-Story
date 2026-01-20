using GameFramework.Event;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// 本地化组件
/// </summary>
public sealed class LocalizationComponent : GameFrameworkComponent
{
    private ILocalizationManager m_LocalizationManager = null;


    /// <summary>
    /// 获取或设置本地化语言。
    /// </summary>
    public Language Language
    {
        get { return m_LocalizationManager.Language; }
        set { m_LocalizationManager.Language = value; }
    }

    /// <summary>
    /// 获取系统语言。
    /// </summary>
    public Language SystemLanguage
    {
        get { return m_LocalizationManager.SystemLanguage; }
    }

    /// <summary>
    /// 当前字体
    /// </summary>
    public TMP_FontAsset CurrentFont
    {
        get { return m_LocalizationManager.CurrentFont; }
    }

    public CultureInfo GetCultureInfoFromLanguage(Language language)
    {
        switch (language)
        {
            case Language.Unspecified:
                return CultureInfo.InvariantCulture;
            case Language.Afrikaans:
                return CultureInfo.GetCultureInfo("af-ZA");
            case Language.Albanian:
                return CultureInfo.GetCultureInfo("sq-AL");
            case Language.Arabic:
                return CultureInfo.GetCultureInfo("ar-SA");
            case Language.Basque:
                return CultureInfo.GetCultureInfo("eu-ES");
            case Language.Belarusian:
                return CultureInfo.GetCultureInfo("be-BY");
            case Language.Bulgarian:
                return CultureInfo.GetCultureInfo("bg-BG");
            case Language.Catalan:
                return CultureInfo.GetCultureInfo("ca-ES");
            case Language.ChineseSimplified:
                return CultureInfo.GetCultureInfo("zh-CN");
            case Language.ChineseTraditional:
                return CultureInfo.GetCultureInfo("zh-TW");
            case Language.Croatian:
                return CultureInfo.GetCultureInfo("hr-HR");
            case Language.Czech:
                return CultureInfo.GetCultureInfo("cs-CZ");
            case Language.Danish:
                return CultureInfo.GetCultureInfo("da-DK");
            case Language.Dutch:
                return CultureInfo.GetCultureInfo("nl-NL");
            case Language.English:
                return CultureInfo.GetCultureInfo("en-US");
            case Language.Estonian:
                return CultureInfo.GetCultureInfo("et-EE");
            case Language.Faroese:
                return CultureInfo.GetCultureInfo("fo-FO");
            case Language.Finnish:
                return CultureInfo.GetCultureInfo("fi-FI");
            case Language.French:
                return CultureInfo.GetCultureInfo("fr-FR");
            case Language.Georgian:
                return CultureInfo.GetCultureInfo("ka-GE");
            case Language.German:
                return CultureInfo.GetCultureInfo("de-DE");
            case Language.Greek:
                return CultureInfo.GetCultureInfo("el-GR");
            case Language.Hebrew:
                return CultureInfo.GetCultureInfo("he-IL");
            case Language.Hungarian:
                return CultureInfo.GetCultureInfo("hu-HU");
            case Language.Icelandic:
                return CultureInfo.GetCultureInfo("is-IS");
            case Language.Indonesian:
                return CultureInfo.GetCultureInfo("id-ID");
            case Language.Italian:
                return CultureInfo.GetCultureInfo("it-IT");
            case Language.Japanese:
                return CultureInfo.GetCultureInfo("ja-JP");
            case Language.Korean:
                return CultureInfo.GetCultureInfo("ko-KR");
            case Language.Latvian:
                return CultureInfo.GetCultureInfo("lv-LV");
            case Language.Lithuanian:
                return CultureInfo.GetCultureInfo("lt-LT");
            case Language.Macedonian:
                return CultureInfo.GetCultureInfo("mk-MK");
            case Language.Malayalam:
                return CultureInfo.GetCultureInfo("ml-IN");
            case Language.Norwegian:
                return CultureInfo.GetCultureInfo("nb-NO");
            case Language.Persian:
                return CultureInfo.GetCultureInfo("fa-IR");
            case Language.Polish:
                return CultureInfo.GetCultureInfo("pl-PL");
            case Language.PortugueseBrazil:
                return CultureInfo.GetCultureInfo("pt-BR");
            case Language.PortuguesePortugal:
                return CultureInfo.GetCultureInfo("pt-PT");
            case Language.Romanian:
                return CultureInfo.GetCultureInfo("ro-RO");
            case Language.Russian:
                return CultureInfo.GetCultureInfo("ru-RU");
            case Language.SerboCroatian:
                return CultureInfo.GetCultureInfo("sh-HR");
            case Language.SerbianCyrillic:
                return CultureInfo.GetCultureInfo("sr-Cyrl-RS");
            case Language.SerbianLatin:
                return CultureInfo.GetCultureInfo("sr-Latn-RS");
            case Language.Slovak:
                return CultureInfo.GetCultureInfo("sk-SK");
            case Language.Slovenian:
                return CultureInfo.GetCultureInfo("sl-SI");
            case Language.Spanish:
                return CultureInfo.GetCultureInfo("es-ES");
            case Language.Swedish:
                return CultureInfo.GetCultureInfo("sv-SE");
            case Language.Thai:
                return CultureInfo.GetCultureInfo("th-TH");
            case Language.Turkish:
                return CultureInfo.GetCultureInfo("tr-TR");
            case Language.Ukrainian:
                return CultureInfo.GetCultureInfo("uk-UA");
            case Language.Vietnamese:
                return CultureInfo.GetCultureInfo("vi-VN");
            case Language.Hindi:
                return CultureInfo.GetCultureInfo("hi-IN");
            default:
                return CultureInfo.InvariantCulture;
        }
    }


    protected override void Awake()
    {
        base.Awake();

        m_LocalizationManager = GameFrameworkEntry.GetModule<LocalizationManager>();
        if (m_LocalizationManager == null)
        {
            Log.Fatal("localization Manager is invalid");
            return;
        }
    }

    public void Init()
    {
        //中文包检测是否需要下载
        if (Language == Language.ChineseSimplified || Language == Language.ChineseTraditional)
        {
            string fontName = "SOURCEHANSANSCN-CN SDF";
            if (!AddressableUtils.IsHaveAssetSync<TMP_FontAsset>(fontName))
            {
                GameManager.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
                GameManager.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

                GameManager.Download.AddDownload(fontName);

                return;
            }
        }

        m_LocalizationManager.Init();
    }

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    public string GetString(string key)
    {
        return m_LocalizationManager.GetString(key);
    }

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <param name="arg">字典参数。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    public string GetString(string key, params string[] arg)
    {
        return m_LocalizationManager.GetString(key, arg);
    }

    /// <summary>
    /// 检测是否初始化完毕
    /// </summary>
    public bool CheckInitComplete()
    {
        return m_LocalizationManager.CheckInitComplete();
    }

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="completeAction">成功事件</param>
    public void GetPresetMaterialAsync(string name, Action<Material> completeAction)
    {
        m_LocalizationManager.GetPresetMaterialAsync(name, completeAction);
    }

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="fontName">字体名称</param>
    /// <param name="completeAction">成功事件</param>
    public void GetPresetMaterialAsync(string name, string fontName, Action<Material> completeAction)
    {
        m_LocalizationManager.GetPresetMaterialAsync(name, fontName, completeAction);
    }

    /// <summary>
    /// 确认语言是否支持
    /// </summary>
    /// <returns></returns>
    public bool CheckLanguageSupport(Language language)
    {
        return language == Language.English || language == Language.German || language == Language.Spanish || language == Language.PortuguesePortugal || language == Language.Italian ||
            language == Language.French || language == Language.Russian || language == Language.Polish || language == Language.Korean || language == Language.Japanese ||
            language == Language.Arabic || language == Language.Vietnamese || language == Language.Turkish || language == Language.Thai || language == Language.Indonesian || language == Language.Hindi ||
            language == Language.ChineseTraditional || language == Language.ChineseSimplified;
    }

    /// <summary>
    /// 是否是东亚方块字
    /// </summary>
    /// <returns>日/韩/简中/繁中 为 true</returns>
    public bool RecentLanguageIsEastAsianLanguage()
    {
        return Language == Language.Japanese || Language == Language.Korean || Language == Language.ChineseSimplified ||
               Language == Language.ChineseTraditional;
    }

    private void OnDownloadSuccess(object sender, GameEventArgs e)
    {
        string fontName = "SOURCEHANSANSCN-CN SDF";

        DownloadSuccessEventArgs ne = e as DownloadSuccessEventArgs;
        if (ne != null && ne.DownloadKey == fontName)
        {
            m_LocalizationManager.Init();
        }
    }

    private void OnDownloadFailure(object sender, GameEventArgs e)
    {
        string fontName = "SOURCEHANSANSCN-CN SDF";

        DownloadFailureEventArgs ne = e as DownloadFailureEventArgs;
        if (ne != null && ne.DownloadKey == fontName)
        {
            Language = Language.English;

            GameManager.PlayerData.Language = Language.ToString();
        }
    }
}