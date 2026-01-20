using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// 本地化管理器
/// </summary>
public sealed class LocalizationManager : GameFrameworkModule, ILocalizationManager
{
    private readonly List<AsyncOperationHandle> loadAssetList;
    private readonly Dictionary<string, Material> presetMaterialDic;

    private Language language;
    private LanguageSourceData languageSourceData;
    private TMP_FontAsset currentFont;
    private string currentFontName;
    private bool initLanguageComplete;
    private bool initFontComplete;
    private bool loadingLanguageAsset;

    public LocalizationManager()
    {
        loadAssetList = new List<AsyncOperationHandle>();
        presetMaterialDic = new Dictionary<string, Material>();
        language = Language.Unspecified;
        languageSourceData = null;
        currentFont = null;
        currentFontName = null;
        initLanguageComplete = false;
        initFontComplete = false;
        loadingLanguageAsset = false;
    }

    /// <summary>
    /// 获取或设置本地化语言。
    /// </summary>
    public Language Language
    {
        get
        {
            return language;
        }
        set
        {
            if (value == Language.Unspecified)
            {
                Log.Error("Language is invalid.");
                return;
            }

            if (language == value)
            {
                return;
            }

            if (language == Language.Unspecified)
            {
                language = value;
            }
            else
            {
                language = value;

                Reset();
                Init();

                loadingLanguageAsset = true;
            }
        }
    }

    /// <summary>
    /// 获取系统语言。
    /// </summary>
    public Language SystemLanguage
    {
        get
        {
            return GetSystemLanguage();
        }
    }

    /// <summary>
    /// 当前字体资源
    /// </summary>
    public TMP_FontAsset CurrentFont
    {
        get
        {
            return currentFont;
        }
    }

    public void Init()
    {
        LoadLanguageData();

        HasLanguagePackage(Language, () =>
        {
            LoadFontVariant();
        }, () =>
        {
            Log.Info("Language Package {0} is not available", Language.ToString());
        });
    }

    public void Reset()
    {
        presetMaterialDic.Clear();

        for (int i = 0; i < loadAssetList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(loadAssetList[i]);
        }
        loadAssetList.Clear();

        currentFont = null;
        currentFontName = null;
        initLanguageComplete = false;
        initFontComplete = false;
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (loadingLanguageAsset)
        {
            if (CheckInitComplete())
            {
                loadingLanguageAsset = false;
                GameManager.UI.OnReset();
                GameManager.UI.OnInit();
            }
            return;
        }
    }

    public override void Shutdown()
    {
        Reset();
        language = Language.Unspecified;
    }

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    public string GetString(string key)
    {
        string value = GetRawString(key);
        if (value == null)
        {
            //return string.Format("<NoKey>{0}", key);
            return key;
        }

        return value;
    }

    /// <summary>
    /// 根据字典主键获取字典内容字符串。
    /// </summary>
    /// <param name="key">字典主键。</param>
    /// <param name="arg">字典参数。</param>
    /// <returns>要获取的字典内容字符串。</returns>
    public string GetString(string key, params string[] arg)
    {
        string value = GetRawString(key);
        if (value == null)
        {
            //return string.Format("<NoKey>{0}", key);
            return key;
        }

        if (arg.Length > 0 && int.TryParse(arg[0], out int number))
        {
            GetPluralTranslation(GetPluralType(number), ref value);
        }

        try
        {
            return string.Format(value, arg);
        }
        catch (Exception e)
        {
            return string.Format("<Error>{0},{1},{2}", key, value, e);
        }
    }

    private string GetPluralType(int number)
    {
        switch (GameManager.Localization.Language)
        {
            case Language.English:
            case Language.German:
            case Language.Spanish:
            case Language.PortuguesePortugal:
            case Language.Italian:
            case Language.French:
            case Language.Polish:
            case Language.Korean:
            case Language.Japanese:
            case Language.Vietnamese:
            case Language.Thai:
            case Language.ChineseSimplified:
            case Language.ChineseTraditional:
            case Language.Turkish:
                if (number == 0)
                    return "Zero";
                else if (number == 1)
                    return "One";
                else
                    return "Plural";
            case Language.Russian:
                if (number == 0)
                    return "Zero";
                //i % 10 = 1 and i % 100 != 11
                else if (number % 10 == 1 && number % 100 != 11)
                    return "One";
                //i % 10 = 2..4 and i % 100 != 12..14
                else if ((number % 10 >= 2 && number % 10 <= 4) && (number % 100 < 12 || number % 100 > 14))
                    return "Few";
                //i % 10 = 0 or i % 10 = 5..9 or i % 100 = 11..14
                //else if ((number % 10 == 0) || (number % 10 >= 5 && number % 10 <= 9) || (number % 100 >= 11 && number % 100 <= 14))
                //    return "Plural";
                else
                    return "Plural";
            case Language.Arabic:
                if (number == 0)
                    return "Zero";
                else if (number == 1)
                    return "One";
                else if (number == 2)
                    return "Two";
                else if (number >= 3 && number <= 10)
                    return "Few";
                else if (number >= 11 && number <= 100)
                    return "Many";
                else
                    return "Plural";
        }

        return "Plural";
    }

    private void GetPluralTranslation(string pluralType, ref string translation)
    {
        string tag = "[i2p_" + pluralType + "]";
        int idx0 = translation.IndexOf(tag, StringComparison.OrdinalIgnoreCase);

        if (idx0 < 0)
            idx0 = 0;
        else
            idx0 += tag.Length;

        int idx1 = translation.IndexOf("[i2p_", idx0, StringComparison.OrdinalIgnoreCase);
        if (idx1 < 0) idx1 = translation.Length;

        translation = translation.Substring(idx0, idx1 - idx0);

        int leftIndex = 0;
        int rightIndex = 0;
        int matchCount = 0;
        while (leftIndex >= 0 && rightIndex >= 0 && leftIndex < translation.Length && rightIndex < translation.Length)
        {
            leftIndex = translation.IndexOf("{[", rightIndex, StringComparison.OrdinalIgnoreCase);
            rightIndex = translation.IndexOf("]}", rightIndex, StringComparison.OrdinalIgnoreCase);
            if (leftIndex >= 0 && rightIndex >= 0)
            {
                translation = translation.Replace(translation.Substring(leftIndex, rightIndex - leftIndex + 2), "{" + matchCount.ToString() + "}");
                matchCount++;
            }
        }
    }

    /// <summary>
    /// 是否存在字典。
    /// </summary>
    /// <param name="term">字典主键。</param>
    /// <returns>是否存在字典。</returns>
    public bool HasRawString(string term)
    {
        if (string.IsNullOrEmpty(term))
        {
            Log.Error("Key is invalid.");
            return false;
        }

        if (languageSourceData.TryGetTranslation(term, out string translation))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 根据字典主键获取字典值。
    /// </summary>
    /// <param name="term">字典主键。</param>
    /// <returns>字典值。</returns>
    public string GetRawString(string term)
    {
        if (string.IsNullOrEmpty(term))
        {
            Log.Error("Key is invalid.");
            return null;
        }

        string translation = null;
        if (languageSourceData.TryGetTranslation(term, out translation))
        {
            return translation;
        }

        return translation;
    }

    /// <summary>
    /// 是否初始化完毕
    /// </summary>
    public bool CheckInitComplete()
    {
        return initLanguageComplete && initFontComplete;
    }

    /// <summary>
    /// 检测语言包是否可用
    /// </summary>
    public void HasLanguagePackage(Language language, Action successAction, Action failAction)
    {
        if (language == Language.Unspecified)
        {
            throw new Exception("language is Unspecified");
        }

        string languagePackageName = GetLanguagePackageName(language);

        AsyncOperationHandle<IList<IResourceLocation>> asynchandle = Addressables.LoadResourceLocationsAsync(languagePackageName);
        asynchandle.Completed += (location) =>
        {
            if (location.Status == AsyncOperationStatus.Succeeded)
            {
                successAction?.Invoke();
            }
            else
            {
                failAction?.Invoke();
            }

            Addressables.Release(asynchandle);
        };
    }

    private void LoadLanguageData()
    {
        if (languageSourceData == null)
        {
            LanguageSourceAsset languageAsset = Resources.Load<LanguageSourceAsset>("DefaultLanguageData");

            if (!languageAsset.mSource.mIsGlobalSource)
                languageAsset.mSource.mIsGlobalSource = true;
            languageAsset.mSource.owner = languageAsset;
            languageSourceData = languageAsset.mSource;

            if (languageAsset.mSource.mDictionary.Count == 0)
                languageAsset.mSource.UpdateDictionary(true);
        }

        initLanguageComplete = true;
    }

    private void LoadFontVariant()
    {
        currentFontName = GetFontName(Language);

        loadAssetList.Add(UnityUtility.LoadAssetAsync<TMP_FontAsset>(currentFontName, font =>
        {
            if (font != null)
            {
                Log.Info("Load Font {0} success", currentFontName);
                currentFont = font;
                initFontComplete = true;

                GameManager.Event.Fire(this, LanguageChangeEventArgs.Create(Language, this));
            }
            else
            {
                Log.Info("Load Font {0} fail", currentFontName);
                initFontComplete = true;
            }
        }));
    }

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="completeAction">成功事件</param>
    public void GetPresetMaterialAsync(string name, Action<Material> completeAction)
    {
        if (string.IsNullOrEmpty(name))
        {
            Log.Error("PresetMaterial name is invalid");
            return;
        }

        GetPresetMaterialAsync(name, GetFontName(Language), completeAction);
    }

    /// <summary>
    /// 获取字体自定义材质
    /// </summary>
    /// <param name="name">材质后缀名称</param>
    /// <param name="fontName">字体名称</param>
    /// <param name="completeAction">成功事件</param>
    public void GetPresetMaterialAsync(string name, string fontName, Action<Material> completeAction)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fontName)) 
        {
            Log.Error("PresetMaterial name is invalid");
            return;
        }

        string materialName = fontName + " - " + name;
        if (presetMaterialDic.TryGetValue(materialName, out Material result))
        {
            completeAction?.Invoke(result);
        }
        else
        {
            loadAssetList.Add(UnityUtility.LoadAssetAsync<Material>(materialName, mat =>
            {
                if (mat != null)
                {
                    presetMaterialDic[materialName] = mat;
                    completeAction?.Invoke(mat);
                }
                else
                {
                    Log.Error("Load material {0} fail", materialName);
                }
            }));
        }
    }

    /// <summary>
    /// 获取资源包的名称
    /// </summary>
    private string GetLanguagePackageName(Language language)
    {
        switch (language)
        {
            case Language.English:
            case Language.German:
            case Language.Spanish:
            case Language.PortuguesePortugal:
            case Language.Italian:
            case Language.French:
            case Language.Polish:
            case Language.Turkish:
            case Language.Indonesian:
                return "DefaultLanguage";
            case Language.Korean:
                return "KoreanLanguage";
            case Language.Japanese:
                return "JapaneseLanguage";
            case Language.ChineseSimplified:
            case Language.ChineseTraditional:
                return "CNLanguage";
            case Language.Russian:
            case Language.Vietnamese:
                return "ChineseLanguage";
            case Language.Arabic:
                return "ArabicLanguage";
            case Language.Thai:
                return "ThaiLanguage";
            case Language.Hindi:
                return "HindiLanguage";
        }
        Log.Warning("{0} Language Package not match",language.ToString());
        return null;
    }

    /// <summary>
    /// 获取字体名称
    /// </summary>
    private string GetFontName(Language language)
    {
        switch (language)
        {
            case Language.English:
            case Language.German:
            case Language.Spanish:
            case Language.PortuguesePortugal:
            case Language.Italian:
            case Language.French:
            case Language.Polish:
            case Language.Turkish:
            case Language.Indonesian:
                return "BANGOPRO SDF";
            case Language.Korean:
                return "BlackHanSans-Regular SDF";
            case Language.Japanese:
                return "SOURCEHANSANSCN-HEAVY SDF";
            case Language.ChineseSimplified:
            case Language.ChineseTraditional:
                return "SOURCEHANSANSCN-CN SDF";
            case Language.Russian:
            case Language.Vietnamese:
				return "SOURCEHANSANSCN-HEAVY SDF";
			case Language.Arabic:
                return "Lalezar-Regular SDF";
            case Language.Thai:
                return "Mitr-SemiBold SDF";
            case Language.Hindi:
                return "NotoSerifDevanagari-Bold SDF";
        }
        return null;
    }

    private Language GetSystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case UnityEngine.SystemLanguage.Afrikaans: return Language.Afrikaans;
            case UnityEngine.SystemLanguage.Arabic: return Language.Arabic;
            case UnityEngine.SystemLanguage.Basque: return Language.Basque;
            case UnityEngine.SystemLanguage.Belarusian: return Language.Belarusian;
            case UnityEngine.SystemLanguage.Bulgarian: return Language.Bulgarian;
            case UnityEngine.SystemLanguage.Catalan: return Language.Catalan;
            case UnityEngine.SystemLanguage.Chinese: return Language.ChineseSimplified;
            case UnityEngine.SystemLanguage.ChineseSimplified: return Language.ChineseSimplified;
            case UnityEngine.SystemLanguage.ChineseTraditional: return Language.ChineseTraditional;
            case UnityEngine.SystemLanguage.Czech: return Language.Czech;
            case UnityEngine.SystemLanguage.Danish: return Language.Danish;
            case UnityEngine.SystemLanguage.Dutch: return Language.Dutch;
            case UnityEngine.SystemLanguage.English: return Language.English;
            case UnityEngine.SystemLanguage.Estonian: return Language.Estonian;
            case UnityEngine.SystemLanguage.Faroese: return Language.Faroese;
            case UnityEngine.SystemLanguage.Finnish: return Language.Finnish;
            case UnityEngine.SystemLanguage.French: return Language.French;
            case UnityEngine.SystemLanguage.German: return Language.German;
            case UnityEngine.SystemLanguage.Greek: return Language.Greek;
            case UnityEngine.SystemLanguage.Hebrew: return Language.Hebrew;
            case UnityEngine.SystemLanguage.Hungarian: return Language.Hungarian;
            case UnityEngine.SystemLanguage.Icelandic: return Language.Icelandic;
            case UnityEngine.SystemLanguage.Indonesian: return Language.Indonesian;
            case UnityEngine.SystemLanguage.Italian: return Language.Italian;
            case UnityEngine.SystemLanguage.Japanese: return Language.Japanese;
            case UnityEngine.SystemLanguage.Korean: return Language.Korean;
            case UnityEngine.SystemLanguage.Latvian: return Language.Latvian;
            case UnityEngine.SystemLanguage.Lithuanian: return Language.Lithuanian;
            case UnityEngine.SystemLanguage.Norwegian: return Language.Norwegian;
            case UnityEngine.SystemLanguage.Polish: return Language.Polish;
            case UnityEngine.SystemLanguage.Portuguese: return Language.PortuguesePortugal;
            case UnityEngine.SystemLanguage.Romanian: return Language.Romanian;
            case UnityEngine.SystemLanguage.Russian: return Language.Russian;
            case UnityEngine.SystemLanguage.SerboCroatian: return Language.SerboCroatian;
            case UnityEngine.SystemLanguage.Slovak: return Language.Slovak;
            case UnityEngine.SystemLanguage.Slovenian: return Language.Slovenian;
            case UnityEngine.SystemLanguage.Spanish: return Language.Spanish;
            case UnityEngine.SystemLanguage.Swedish: return Language.Swedish;
            case UnityEngine.SystemLanguage.Thai: return Language.Thai;
            case UnityEngine.SystemLanguage.Turkish: return Language.Turkish;
            case UnityEngine.SystemLanguage.Ukrainian: return Language.Ukrainian;
            case UnityEngine.SystemLanguage.Unknown: return Language.Unspecified;
            case UnityEngine.SystemLanguage.Vietnamese: return Language.Vietnamese;
            //Unity.systemLanguange not define Hindi:return Language.Hindi;
            default: return Language.Unspecified;
        }
    }
}
