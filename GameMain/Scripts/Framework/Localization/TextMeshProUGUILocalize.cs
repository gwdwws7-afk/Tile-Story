using ArabicSupport;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// TextMesh本地化组件
/// </summary>
public class TextMeshProUGUILocalize : MonoBehaviour
{
    [NonSerialized]
    public string FinalTerm;

    public string m_Term = string.Empty;
    public string m_MaterialName = string.Empty;
    public bool m_BestFitWidth = false;
    public bool m_BestFitHeight = false;
    public bool m_BestFitWidthAndHeight = false;
    public bool autoLineFeed = false;

    public bool isTextOutlineCover = true;

    private TextMeshProUGUI m_Target = null;
    private string MainTranslation = null;
    [HideInInspector]
    [SerializeField]
    private TextMeshProUGUI textOutlineCover = null;

    public string Term
    {
        get { return m_Term; }
        set { SetTerm(value); }
    }

    public TextMeshProUGUI Target
    {
        get
        {
            if (m_Target == null)
                m_Target = GetComponent<TextMeshProUGUI>();
            return m_Target;
        }
    }

    [Serializable]
    public struct ParamValue
    {
        public string Name, Value;

    }

    [SerializeField]
    public List<ParamValue> _Params = new List<ParamValue>();

    private void OnEnable()
    {
        GameManager.Event.Subscribe(LanguageChangeEventArgs.EventId, OnLanguageChange);

        OnLocalize();

        Refresh();

        if (m_BestFitWidth || m_BestFitHeight || m_BestFitWidthAndHeight)
        {
            FitFontSize(m_BestFitWidth, m_BestFitHeight);
        }
    }

    private void OnDisable()
    {
        GameManager.Event.Unsubscribe(LanguageChangeEventArgs.EventId, OnLanguageChange);
    }

    public void OnLocalize(bool Force = false)
    {
        if (!Force && (!enabled || gameObject == null || !gameObject.activeInHierarchy))
            return;

        if (string.IsNullOrEmpty(FinalTerm))
        {
            GetFinalTerms(out FinalTerm);
        }

        if (string.IsNullOrEmpty(FinalTerm))
            return;

        if (_Params.Count == 0)
        {
            MainTranslation = GameManager.Localization.GetString(FinalTerm);
        }
        else
        {
            string[] argas = new string[_Params.Count];
            for (int i = 0; i < _Params.Count; i++)
            {
                argas[i] = _Params[i].Value;
            }
            MainTranslation = GameManager.Localization.GetString(FinalTerm, argas);
        }

        if (IsValid())
        {
            if (MainTranslation != null)
            {
                MainTranslation = FitTextLine(MainTranslation);
                m_Target.text = MainTranslation;
            }
            else
            {
                FinalTerm = FitTextLine(FinalTerm);
                m_Target.text = FinalTerm;
            }

            if (GameManager.Localization.Language == Language.Arabic ||
                GameManager.Localization.Language == Language.Hindi)
            {
                string fixedText;
                if (GameManager.Localization.Language == Language.Arabic)
                {
                    fixedText = m_Target.text;
                    string pattern = @"<[^>]*>";
                    MatchCollection matches = Regex.Matches(fixedText, pattern);
                    if (matches.Count > 0)
                    {
                        int index = 0;
                        foreach (Match match in matches)
                        {
                            fixedText = fixedText.Replace(match.Value, "=" + index.ToString() + "=");
                            index++;
                        }
                    }
                    m_Target.isRightToLeftText = true;
                    fixedText = ArabicFixer.Fix(fixedText, false, false);
                    fixedText = ReverseString(fixedText);
                    if (matches.Count > 0)
                    {
                        int index = 0;
                        foreach (Match match in matches)
                        {
                            fixedText = fixedText.Replace("=" + index.ToString() + "=", match.Value);
                            index++;
                        }
                    }

                    m_Target.text = fixedText;
                }
                else /*if(GameManager.Localization.Language == Language.Hindi)*/
                {
                    m_Target.isRightToLeftText = false;
                    fixedText = m_Target.text;
                }

                //修复描边重叠问题
                if (isTextOutlineCover&&!string.IsNullOrEmpty(m_MaterialName)) 
                {
                    if (textOutlineCover == null)
                    {
                        var newTextObj = new GameObject("TextOutlineCover");
                        textOutlineCover = newTextObj.AddComponent<TextMeshProUGUI>();

                        var rect = GetComponent<RectTransform>();
                        var rectTrans = newTextObj.GetComponent<RectTransform>();
                        rectTrans.SetParent(transform);
                        rectTrans.sizeDelta = rect.sizeDelta;
                        rectTrans.pivot = rect.pivot;
                        rectTrans.localScale = Vector3.one;
                        rectTrans.localPosition = Vector3.zero;
                        rectTrans.localEulerAngles = Vector3.zero;

                        textOutlineCover.enableWordWrapping = m_Target.enableWordWrapping;
                        textOutlineCover.font = GameManager.Localization.CurrentFont;
                        textOutlineCover.fontSize = m_Target.fontSize;
                        textOutlineCover.enableAutoSizing = m_Target.enableAutoSizing;
                        textOutlineCover.fontSizeMin = m_Target.fontSizeMin;
                        textOutlineCover.fontSizeMax = m_Target.fontSizeMax;
                        textOutlineCover.rectTransform.anchorMin = Vector2.zero;
                        textOutlineCover.rectTransform.anchorMax = Vector2.one;

                        textOutlineCover.rectTransform.anchoredPosition = Vector2.zero;
                        textOutlineCover.rectTransform.sizeDelta = Vector2.zero;

                        textOutlineCover.characterSpacing = m_Target.characterSpacing;
                        textOutlineCover.wordSpacing = m_Target.wordSpacing;
                        textOutlineCover.lineSpacing = m_Target.lineSpacing;
                        textOutlineCover.alignment = m_Target.alignment;
                        textOutlineCover.margin = m_Target.margin;
                        textOutlineCover.color = m_Target.color;
                        textOutlineCover.enableVertexGradient = m_Target.enableVertexGradient;
                        textOutlineCover.colorGradient = m_Target.colorGradient;
                        textOutlineCover.spriteAsset = m_Target.spriteAsset;

                        textOutlineCover.isRightToLeftText = GameManager.Localization.Language == Language.Arabic ? true : false;
                        textOutlineCover.text = fixedText;

                        var circleTextMesh = m_Target.GetComponent<CircleTextMesh>();
                        if (circleTextMesh != null)
                        {
                            var circleT = newTextObj.AddComponent<CircleTextMesh>();
                            circleT.radius = circleTextMesh.radius;
                            textOutlineCover.enabled = false;
                            textOutlineCover.enabled = true;
                        }
                    }
                    else
                    {
                        textOutlineCover.gameObject.SetActive(true);
                        textOutlineCover.font = GameManager.Localization.CurrentFont;
                        textOutlineCover.fontSize = m_Target.fontSize;
                        textOutlineCover.text = fixedText;
                        textOutlineCover.isRightToLeftText = GameManager.Localization.Language == Language.Arabic ? true : false;
                    }
                }
            }
            else
            {
                if (textOutlineCover != null)
                {
                    textOutlineCover.gameObject.SetActive(false);
                }

                m_Target.isRightToLeftText = false;
            }
        }
    }

    public void ForceMeshUpdate()
    {
        m_Target.ForceMeshUpdate();

        if (textOutlineCover != null)
        {
            textOutlineCover.ForceMeshUpdate();
        }
    }

    private string ReverseString(string s)
    {
        if (s.Contains(Environment.NewLine))
        {
            string[] stringSeparators = new string[] { Environment.NewLine };
            string[] strSplit = s.Split(stringSeparators, StringSplitOptions.None);

            if (strSplit.Length == 0 || strSplit.Length == 1) 
            {
                char[] charArray = s.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }
            else
            {
                char[] charArray = strSplit[0].ToCharArray();
                Array.Reverse(charArray);
                string outputString = new string(charArray);
                int iteration = 1;
                if (strSplit.Length > 1)
                {
                    while (iteration < strSplit.Length)
                    {
                        charArray = strSplit[iteration].ToCharArray();
                        Array.Reverse(charArray);
                        outputString += Environment.NewLine + new string(charArray);
                        iteration++;
                    }
                }
                return outputString;
            }
        }
        else
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    public void Refresh()
    {
        if (IsValid())
        {
            if (GameManager.Localization.CurrentFont != null)
            {
                m_Target.font = GameManager.Localization.CurrentFont;
            }

            if (!string.IsNullOrEmpty(m_MaterialName))
            {
                string newMaterialName = m_MaterialName;
                GameManager.Localization.GetPresetMaterialAsync(m_MaterialName, mat =>
                {
                    if (newMaterialName == m_MaterialName) 
                        m_Target.fontSharedMaterial = mat;
                });
            }
        }
    }
    
    /// <summary>
    /// 适配字体大小
    /// </summary>
    /// <param name="bestFitWidth">宽度适配</param>
    /// <param name="bestFitHeight">高度适配</param>
    /// <param name="refresh">刷新</param>
    public void FitFontSize(bool bestFitWidth, bool bestFitHeight, bool refresh = false)
    {
        if (IsValid() && !string.IsNullOrEmpty(m_Target.text))  
        {
            RectTransform rectTrans = m_Target.GetComponent<RectTransform>();
            Vector2 sizeDelta = rectTrans.rect.size;

            float preferredHeight = m_Target.preferredHeight;
            float preferredWidth = m_Target.preferredWidth;

            if (refresh || preferredWidth <= 0) 
            {
                m_Target.GetPreferredValues();
            }

            preferredHeight = m_Target.preferredHeight;
            preferredWidth = m_Target.preferredWidth;

            if (sizeDelta.x < preferredWidth && bestFitWidth) 
            {
                float fontSize = m_Target.fontSize;

                m_Target.fontSize = fontSize / (preferredWidth / sizeDelta.x);
            }

            if (sizeDelta.y < preferredHeight && bestFitHeight)
            {
                float fontSize = m_Target.fontSize;

                m_Target.fontSize = fontSize / (preferredHeight / sizeDelta.y);
            }

            if (textOutlineCover != null)
            {
                textOutlineCover.fontSize = m_Target.fontSize;
            }
        }
    }

    public string FitTextLine(string text)
    {
        if (autoLineFeed)
        {
            try
            {
                TMP_TextInfo info = m_Target.GetTextInfo(text);
                if (info.lineCount > 1)
                {
                    if (GameManager.Localization.Language == Language.ChineseSimplified || GameManager.Localization.Language == Language.ChineseTraditional)
                    {
                        text = text.Replace("，", "，\n");
                    }
                    else if (GameManager.Localization.Language == Language.Arabic)
                    {

                    }
                    else
                    {
                        text = text.Replace(",", ",\n");
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("TextMeshProUGUI FitTextLine Error:{0}", e.Message);
            }
        }

        return text;
    }

    public void SetTerm(string primary)
    {
        if (!string.IsNullOrEmpty(primary))
            FinalTerm = m_Term = primary;

        OnLocalize(true);
    }

    public void SetMaterialPreset(MaterialPresetName materialPresetName)
    {
        if (materialPresetName != MaterialPresetName.None) 
        {
            m_MaterialName = materialPresetName.ToString();

            Refresh();
        }
    }

    // Returns the term that will actually be translated
    public void GetFinalTerms(out string primaryTerm)
    {
        primaryTerm = string.Empty;

        if (!string.IsNullOrEmpty(m_Term))
        {
            primaryTerm = m_Term;
        }
        else if (IsValid())
        {
            primaryTerm = m_Target.text;
        }

        if (primaryTerm != null)
            primaryTerm = primaryTerm.Trim();
    }

    public void SetParameterValue(string ParamName, string ParamValue, bool localize = true)
    {
        bool setted = false;
        for (int i = 0, imax = _Params.Count; i < imax; ++i)
            if (_Params[i].Name == ParamName)
            {
                var temp = _Params[i];
                temp.Value = ParamValue;
                _Params[i] = temp;
                setted = true;
                break;
            }
        if (!setted)
            _Params.Add(new ParamValue { Name = ParamName, Value = ParamValue });

        if (localize)
            OnLocalize();
    }

    public bool IsValid()
    {
        if (m_Target == null)
        {
            m_Target = GetComponent<TextMeshProUGUI>();
        }

        return m_Target != null;
    }

    public void OnLanguageChange(object sender, GameEventArgs e)
    {
        OnLocalize();

        Refresh();

        if (m_BestFitWidth || m_BestFitHeight || m_BestFitWidthAndHeight)
        {
            FitFontSize(m_BestFitWidth, m_BestFitHeight);
        }
    }
}
