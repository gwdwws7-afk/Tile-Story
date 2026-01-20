using GameFramework.Event;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameSettingPanel : PopupMenuForm
{
    [SerializeField] private DelayButton BG_Btn, Close_Btn, Continue_Btn, Quit_Btn;
    [SerializeField] private SwitchButton Music_Btn, Audio_Btn, Shake_Btn, Tip_Btn;
    [SerializeField] private Image BgImage;

    [SerializeField] private TextMeshProUGUILocalize Tittle_Text;

    private int levelDifficulty;
    private List<AsyncOperationHandle> handleList = new List<AsyncOperationHandle>();

    private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.Title_Blue,
        MaterialPresetName.Title_Purple,
        MaterialPresetName.Title_Red,
        MaterialPresetName.Title_Blue,
    };

    private void SetBgImage()
    {
        if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            // Tittle_Text.Target.enableVertexGradient = true;
            // var gradient = new VertexGradient
            // {
            //     topLeft = Color.white,
            //     topRight = Color.white,
            //     bottomLeft = new Color(193f/255f,1f,1f,1f),
            //     bottomRight = new Color(193f/255f,1f,1f,1f)
            // };
            // Tittle_Text.Target.colorGradient = gradient;
            // GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            // {
            //     Tittle_Text.SetMaterialPreset(quitTextMaterials[3]);
            // });
            //BgImage.sprite = BgSprites[3];
            //Tittle_Text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -123f);
            //Close_Btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-78, -64);
            //BgImage.sprite = BgSprites[0];
            return;
        }
        //Tittle_Text.Target.enableVertexGradient = false;
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());

        if (levelDifficulty != hardIndex && !SystemInfoManager.IsSuperLowMemorySize) 
        {
            levelDifficulty = hardIndex;
            Tittle_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);

            string bgKey = "panel_normal";
            if (hardIndex == 1)
                bgKey = "panel_hard";
            else if (hardIndex == 2)
                bgKey = "panel_superhard";
            handleList.Add(UnityUtility.LoadAssetAsync<Sprite>(bgKey, sp =>
            {
                BgImage.sprite = sp;
            }));
        }
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        isShowGameSurePanel = true;
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        SetCurStatus();
        
        BtnEvent();
        SetBgImage();

        string way = userData as string;
        if (way == "ShowByFocus")
        {
            BgImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1032, 900);
            Continue_Btn.transform.localPosition = new Vector3(0, -20);
            Continue_Btn.gameObject.SetActive(true);
            Quit_Btn.gameObject.SetActive(false); 
        }
        else
        {
            BgImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1032, 1157);
            Continue_Btn.transform.localPosition = new Vector3(0, 215);
            Continue_Btn.gameObject.SetActive(true);
            Quit_Btn.gameObject.SetActive(true);   
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        levelDifficulty = 0;
        for (int i = 0; i < handleList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(handleList[i]);
        }
        handleList.Clear();

        base.OnReset();
    }

    public override bool CheckInitComplete()
    {
        for (int i = 0; i < handleList.Count; i++)
        {
            if (handleList[i].IsValid() && !handleList[i].IsDone)
                return false;
        }

        return base.CheckInitComplete();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.PauseLevelTime));
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        BG_Btn.OnReset();
        Close_Btn.OnReset();

        Continue_Btn.OnReset();
        Quit_Btn.OnReset();
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnRelease()
    {
        isShowGameSurePanel = true;
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        base.OnRelease();
    }

    private void SetCurStatus()
    {
        Music_Btn.SetStatus(!GameManager.PlayerData.MusicMuted);
        Audio_Btn.SetStatus(!GameManager.PlayerData.AudioMuted);
        Shake_Btn.SetStatus(!GameManager.PlayerData.ShakeMuted);
        Tip_Btn.SetStatus(!GameManager.PlayerData.TurnOffTips);
    }

    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.ToString() == "RestartGame")
        {
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm("TileMatchPanel");
        }
    }

    private bool isShowGameSurePanel = true;
    private void BtnEvent()
    {
        BG_Btn.SetBtnEvent(() =>
        {
            // GameManager.UI.HideUIForm(this);
        });
        Close_Btn.SetBtnEvent(() => { GameManager.UI.HideUIForm(this); });

        Quit_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
            if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
            {
                isShowGameSurePanel = false;
                GameManager.Task.CalendarChallengeManager.CalendarChallengeFail();
                GameManager.Ads.ShowInterstitialAd(ProcedureUtil.ProcedureGameToMap);
                return;
            }
            if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing &&
                GameManager.Task.PersonRankManager.ContinuousWinTime >= 1)
            {
                //有了生命丢失界面就不要有连胜丢失界面
                //isShowGameSurePanel = false;
                //GameManager.UI.ShowUIForm<PersonRankLoseComboMenu>(form =>
                //{
                //    (form as PersonRankLoseComboMenu).SetOnQuitBtnClcked(CheckClimbBeanstalkAndShowForm);

                //});
                CheckClimbBeanstalkAndShowForm();
            }
            else
            {
                CheckClimbBeanstalkAndShowForm();
            }
        });

        Music_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.MusicMuted = !GameManager.PlayerData.MusicMuted;
            GameManager.Sound.MuteMusic(GameManager.PlayerData.MusicMuted);

            if (!GameManager.PlayerData.MusicMuted)
            {
                GameManager.Sound.PlayMusic(GameManager.PlayerData.HappyBgMusicName);
            }
            else
            {
                GameManager.Sound.StopMusic(0);
            }

            Music_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.MusicMuted, false);
        });
        Audio_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.AudioMuted = !GameManager.PlayerData.AudioMuted;
            GameManager.Sound.MuteAudio(GameManager.PlayerData.AudioMuted);
            Audio_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.AudioMuted, false);
        });
        Shake_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.ShakeMuted = !GameManager.PlayerData.ShakeMuted;
            Shake_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.ShakeMuted, false);
        });
        Tip_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.TurnOffTips = !GameManager.PlayerData.TurnOffTips;
            Tip_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.TurnOffTips, false);
        });
        Continue_Btn.SetBtnEvent(() =>
        {
            // GameManager.Ads.ShowInterstitialAd();
            // GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Setting_Home, 
            // 	new Parameter("Level", GameManager.PlayerData.NowLevel));
            //
            // ProcedureUtil.ProcedureGameToMap();
            GameManager.UI.HideUIForm(this);
        });
    }


    private void CheckClimbBeanstalkAndShowForm()
    {
        //if (ClimbBeanstalkManager.Instance.ActivityIsOpen() && ClimbBeanstalkManager.Instance.CurrentWinStreak > 0)
        //{
        //    isShowGameSurePanel = false;
        //    GameManager.UI.ShowUIForm<ClimbBeanstalkQuitConfirmMenu>(form =>
        //    {
        //        (form as ClimbBeanstalkQuitConfirmMenu).SetOnQuitBtnClcked(ShowGameQuitPanel);
        //    });
        //}
        //else
        //{
        //    ShowGameQuitPanel();
        //}

        ShowGameQuitPanel();
    }

    private void ShowGameQuitPanel()
    {
        GameManager.DataNode.SetData<bool>("GameSettingQuit", true);
        if (isShowGameSurePanel)
        {
            GameManager.UI.ShowUIForm("GameLoseLifePanel",UIFormType.PopupUI);
        }
        else
        {
            LevelPlayMenu.RecordSourceIndex = 1;
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Retry);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
            //GameManager.UI.ShowUIForm<LevelFailPanel>();
        }
    }
}