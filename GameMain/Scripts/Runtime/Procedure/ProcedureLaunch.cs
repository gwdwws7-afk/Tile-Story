using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 起始流程
/// </summary>
public sealed class ProcedureLaunch : ProcedureBase
{
    public override string ProcedureName => "ProcedureLaunch";

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        base.OnEnter(fsm);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.targetFrameRate = 60;

        Input.multiTouchEnabled = false;
        
        DOTween.SetTweensCapacity(512,256);

        // 语言配置：设置当前使用的语言，如果不设置，则默认使用操作系统语言
        InitLanguageSettings();
        // 声音配置：根据用户配置数据，设置即将使用的声音选项
        InitSoundSettings();

        GameManager.Localization.Init();
        
        GameManager.UI.OnInit();
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        ChangeState<ProcedureMenu>(fsm);
    }

    private void InitLanguageSettings()
    {
        Language language;
        string languageString = GameManager.PlayerData.Language;
        if (string.IsNullOrEmpty(languageString))
        {
            language = GameManager.Localization.SystemLanguage;
        }
        else
        {
            language = (Language)Enum.Parse(typeof(Language), languageString);
        }

        if (!GameManager.Localization.CheckLanguageSupport(language))
        {
            // 若是暂不支持的语言，则使用英语
            language = Language.English;

            GameManager.PlayerData.Language= language.ToString();
        }

        GameManager.Localization.Language = language;
        GameManager.PlayerData.Language = language.ToString();
    }

    private void InitSoundSettings()
    {
        GameManager.Sound.MuteMusic(GameManager.PlayerData.MusicMuted);
        GameManager.Sound.MuteAudio(GameManager.PlayerData.AudioMuted);
    }
}
