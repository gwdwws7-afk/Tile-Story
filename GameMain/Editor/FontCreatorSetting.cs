using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// ����ʹ��ǰ�᣺External Tools�ﹴѡRegistry packages��Regenerate project files
/// ȫ�Զ����ñ����л�
/// </summary>
public class FontCreatorSetting
{
#if UNITY_EDITOR
    const string Font_1 = "BANGOPRO SDF";
    const string Font_2 = "BlackHanSans-Regular SDF";
    //const string Font_3 = "DelaGothicOne-Regular SDF";
    const string Font_4 = "SOURCEHANSANSCN-HEAVY SDF";
    const string Font_5 = "SOURCEHANSANSCN-CN SDF";

    public string[] targetFonts = {
    Font_1,
    Font_2,
    //Font_3,
    Font_4,
    Font_5,
    };

    public Dictionary<string, string> fontShuffleDic = new Dictionary<string, string>()
    {
        {Font_1,$"{Application.dataPath}\\GameMain\\Fonts\\BANGOPRO SDF\\BANGOPRO Shuffle.txt" },
        {Font_2,$"{Application.dataPath}\\GameMain\\Fonts\\Black_Han_Sans\\Black_Han_Sans Shuffle.txt" },
        //{Font_3,$"{Application.dataPath}\\GameMain\\Fonts\\DelaGothicOne\\DelaGothicOne-Regular Shuffle.txt" },
        {Font_4,$"{Application.dataPath}\\GameMain\\Fonts\\SOURCEHANSANSCN-HEAVY\\SOURCEHANSANSCN-HEAVY Shuffle.txt" },
        {Font_5,$"{Application.dataPath}\\GameMain\\Fonts\\SOURCEHANSANSCN-CN\\SOURCEHANSANSCN-CN Shuffle.txt" },
    };
    public Dictionary<string, string> fontAssetDic = new Dictionary<string, string>()
    {
        {Font_1,$"{Application.dataPath}\\GameMain\\Fonts\\BANGOPRO SDF\\BANGOPRO SDF.asset" },
        {Font_2,$"{Application.dataPath}\\GameMain\\Fonts\\Black_Han_Sans\\BlackHanSans-Regular SDF.asset" },
        //{Font_3,$"{Application.dataPath}\\GameMain\\Fonts\\DelaGothicOne\\DelaGothicOne-Regular SDF.asset" },
        {Font_4,$"{Application.dataPath}\\GameMain\\Fonts\\SOURCEHANSANSCN-HEAVY\\SOURCEHANSANSCN-HEAVY SDF.asset" },
        {Font_5,$"{Application.dataPath}\\GameMain\\Fonts\\SOURCEHANSANSCN-CN\\SOURCEHANSANSCN-CN SDF.asset" },
    };

    public Dictionary<string, string> fontDic = new Dictionary<string, string>()
    {
        {Font_1,$"Assets/GameMain/Fonts/BANGOPRO SDF/BANGOPRO.OTF" },
        {Font_2,$"Assets/GameMain/Fonts/Black_Han_Sans/BlackHanSans-Regular.ttf" },
        //{Font_3,$"Assets/GameMain/Fonts/DelaGothicOne/DelaGothicOne-Regular.ttf" },
        {Font_4,$"Assets/GameMain/Fonts/SOURCEHANSANSCN-HEAVY/SOURCEHANSANSCN-HEAVY.OTF" },
        {Font_5,$"Assets/GameMain/Fonts/SOURCEHANSANSCN-CN/SOURCEHANSANSCN-CN.OTF" },
    };

    public Dictionary<string, string> fontShuffleTextAssetDic = new Dictionary<string, string>()
    {
        {Font_1,$"Assets/GameMain/Fonts/BANGOPRO SDF/BANGOPRO Shuffle.txt" },
        {Font_2,$"Assets/GameMain/Fonts/Black_Han_Sans/Black_Han_Sans Shuffle.txt" },
        //{Font_3,$"Assets/GameMain/Fonts/DelaGothicOne/DelaGothicOne-Regular Shuffle.txt" },
        {Font_4,$"Assets/GameMain/Fonts/SOURCEHANSANSCN-HEAVY/SOURCEHANSANSCN-HEAVY Shuffle.txt" },
        {Font_5,$"Assets/GameMain/Fonts/SOURCEHANSANSCN-CN/SOURCEHANSANSCN-CN Shuffle.txt" },
    };

    public Dictionary<string, Vector2> fontAutoSizeDic = new Dictionary<string, Vector2>()
    {
        {Font_1,new Vector2(512,512) },
        {Font_2,new Vector2(1024,1024) },
        //{Font_3,new Vector2(1024,2048) },
        {Font_4,new Vector2(1024,2048) },
        {Font_5,new Vector2(2048,2048) },
    };

    [MenuItem("Window/TextMeshPro/FontAssetCreator(Auto Refresh\\Switch\\Save)", false, 1025)]
    public static void Test()
    {
        var window = EditorWindow.GetWindow<TMPro.EditorUtilities.TMPro_FontAssetCreatorWindow>();
        window.titleContent = new GUIContent("Font Asset Creator");
        window.Focus();
        FontCreatorSetting f = new FontCreatorSetting();
        f.ProcessControlCreatorWindow(window);
        window.Repaint();
    }

    async void ProcessControlCreatorWindow(TMPro.EditorUtilities.TMPro_FontAssetCreatorWindow window)
    {
        UpdateTargetLanguageShuffle();
        Type t = window.GetType();
        var fontFile = t.GetField("m_SourceFont", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        var shuffleFile = t.GetField("m_CharactersFromFile", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        var atlasWidth = t.GetField("m_AtlasWidth", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        var atlasHeight = t.GetField("m_AtlasHeight", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        //�Ƿ��ڶ�������
        var isProcessField = t.GetField("m_IsProcessing", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
        bool isProcess = (bool)isProcessField.GetValue(window);
        for (int i = 0; i < targetFonts.Length; i++)
        {
            string path = fontDic[targetFonts[i]];
            Font newFont = AssetDatabase.LoadAssetAtPath<Font>(path);
            fontFile.SetValue(window, newFont);
            path = fontShuffleTextAssetDic[targetFonts[i]];
            TextAsset shuffleText = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            shuffleFile.SetValue(window, shuffleText);
            atlasWidth.SetValue(window, (int)fontAutoSizeDic[targetFonts[i]].x);
            atlasHeight.SetValue(window, (int)fontAutoSizeDic[targetFonts[i]].y);
            Debug.Log($"Load Font_{i+1} Complete,WaitProcess");
            while (!isProcess)//WaitActive
            {
                isProcess = (bool)isProcessField.GetValue(window);
                await Task.Yield();
            }
            Debug.Log("Start Process");
            while (isProcess)//WaitActiveComplete
            {
                isProcess = (bool)isProcessField.GetValue(window);
                await Task.Yield();
            }
            Debug.Log("Process Complete");
            string filePath = Path.GetFullPath(fontAssetDic[targetFonts[i]]).Replace('\\', '/');
            var method = t.GetMethod("Save_SDF_FontAsset", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
            method.Invoke(window, new object[] { filePath });
            Debug.Log("Save Complete");
        }
    }

    /// <summary>
    /// ˢ������shuffle����
    /// </summary>
    void UpdateTargetLanguageShuffle()
    {
        Dictionary<string, StringBuilder> builderDic = new Dictionary<string, StringBuilder>();
        foreach (var font in targetFonts)
        {
            builderDic[font] = new StringBuilder();
        }
        foreach (var pair in builderDic)
        {
            foreach (var term in I2.Loc.LocalizationManager.GlobalSources)
            {
                LanguageSourceAsset GO = Resources.Load<LanguageSourceAsset>(term);
                GO.SourceData.AppendLanguages(pair.Key, pair.Value);
            }
            string result = pair.Value.ToString();
            if (pair.Key != Font_1)
            {
                result = System.Text.RegularExpressions.Regex.Replace(result, @"\d", "");
            }
            File.WriteAllText(fontShuffleDic[pair.Key], result);
        }
    }

#endif
}
