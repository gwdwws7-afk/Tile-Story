using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public enum LevelPlayType
{
    Play,
    Retry,
}

public sealed class LevelPlayMenu : PopupMenuForm
{
    public static int RecordSourceIndex = 0;

    [SerializeField] private GameObject DifficultyIcon, LevelFailIcon;
    [SerializeField] private RectTransform BgTrans, AreaTrans;
    [SerializeField] private TextMeshProUGUILocalize Tittle_Text, PlayBtn_Text;
    [SerializeField] private DelayButton Play_Btn, Ad_Play_Btn, Close_Btn;
    [SerializeField] private RectTransform Btn_Group;
    [SerializeField] private Image BgImage;
    [SerializeField] private GameObject[] DifficultyTexts;
    [SerializeField] private LevelPlayBooster[] Boosters;
    [SerializeField] private CommonGuideMenu GuideMenu;
    [SerializeField] private GameObject Mask;
    [SerializeField] private LevelPlayMenu_NormalArea NormalArea;
    [SerializeField] private LevelPlayMenu_HiddenTempleArea HiddenTempleArea;
    [SerializeField] private LevelBannerManager LevelBannerManager;
    [SerializeField] private LevelTagManager LevelTagManager;

    private LevelPlayType type;
    private int levelDifficulty;
    private List<AsyncOperationHandle> handleList = new List<AsyncOperationHandle>();

    private List<MaterialPresetName> fontMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.Title_Blue,
        MaterialPresetName.Title_Purple,
        MaterialPresetName.Title_Red,
    };

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

        type = (LevelPlayType)GameManager.DataNode.GetData<int>("CurLevelPlayType", 0);
        SetButtonEvent();

        if (type == LevelPlayType.Retry) 
            InitRetryMenu();

        if (Mask.activeSelf) Mask.SetActive(false);

        //Area initialize
        int nowLevel = GameManager.PlayerData.RealLevel();
        var hardIndex = DTLevelUtil.GetLevelHard(nowLevel);

        if (levelDifficulty != hardIndex)
        {
            levelDifficulty = hardIndex;
            Tittle_Text.SetMaterialPreset(fontMaterials[hardIndex]);

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

        bool levelCanGetPickaxe = HiddenTemple.HiddenTempleManager.Instance.CheckLevelWinCanGetPickaxe();

        if (type == LevelPlayType.Play)
        {
            SetSmallSizePanel();

            if (!DifficultyIcon.activeSelf) DifficultyIcon.SetActive(true);
            if (LevelFailIcon.activeSelf) LevelFailIcon.SetActive(false);
            for (int i = 0; i < DifficultyTexts.Length; i++)
            {
                DifficultyTexts[i].SetActive(i == hardIndex);
            }

            Tittle_Text.SetParameterValue("level", GameManager.PlayerData.NowLevel.ToString());
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_Level_Start_FirstTry, false) &&
                PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + GameManager.PlayerData.NowLevel, 0) == 0) 
                PlayBtn_Text.SetTerm("Common.FirstTry");
            else
                PlayBtn_Text.SetTerm("Common.Play");
            //Ad_Play_Btn.GetComponentInChildren<TextMeshProUGUILocalize>().SetTerm("Level.Free");
        }
        else if (type == LevelPlayType.Retry)
        {
            SetBigSizePanel();

            if (DifficultyIcon.activeSelf) DifficultyIcon.SetActive(false);
            if (!LevelFailIcon.activeSelf) LevelFailIcon.SetActive(true);

            Tittle_Text.SetParameterValue("level", GameManager.PlayerData.NowLevel.ToString());
            PlayBtn_Text.SetTerm("Common.Retry");
            //Ad_Play_Btn.GetComponentInChildren<TextMeshProUGUILocalize>().SetTerm("Level.Free");
        }

        if (levelCanGetPickaxe) 
        {
            HiddenTempleArea.Initialize(type);
            NormalArea.Release();
        }
        else
        {
            NormalArea.Initialize(type);
            HiddenTempleArea.Release();
        }

        //Boost initialize
        //Boosters[0].Initialize(TotalItemType.MagnifierBoost);
        Boosters[1].Initialize(TotalItemType.Prop_AddOneStep);
        Boosters[2].Initialize(TotalItemType.FireworkBoost);

        if(type == LevelPlayType.Play)
        {
            if (nowLevel == Constant.GameConfig.UnlockAddOneStepBoostLevel)
                ShowBoostGuideAnim(TotalItemType.Prop_AddOneStep);
            else if (nowLevel == Constant.GameConfig.UnlockFireworkBoost)
                ShowBoostGuideAnim(TotalItemType.FireworkBoost);
        }
        else if (type == LevelPlayType.Retry)
        {
            GameManager.Firebase.RecordMessageByEvent("Level_Retry", new Parameter("FailSource", RecordSourceIndex == 1 ? "Setting" : "Fail"));
        }

        LevelTagManager.Initialize(type);

        if (type != LevelPlayType.Retry && !SystemInfoManager.IsSuperLowMemorySize) 
        {
            LevelBannerManager.Initialize(type);
            LevelBannerManager.CreateCanShowBanner(LevelBannerManager.transform);
        }
    }

    public override void OnReset()
    {
        //Boosters[0].Recycle();
        Boosters[1].Recycle();
        Boosters[2].Recycle();

        LevelTagManager.Release();

        LevelBannerManager.Release();

        levelDifficulty = 0;
        for (int i = 0; i < handleList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(handleList[i]);
        }
        handleList.Clear();

        base.OnReset();
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        RecordSourceIndex = 0;
        base.OnRelease();
    }

    public override bool CheckInitComplete()
    {
        if (!NormalArea.CheckInitComplete())
            return false;

        for (int i = 0; i < handleList.Count; i++)
        {
            if (handleList[i].IsValid() && !handleList[i].IsDone) 
                return false;
        }

        return true;
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        for (int i = 0; i < Boosters.Length; i++)
        {
            Boosters[i].Refresh();
        }

        LevelBannerManager.ShowCanShowBanner();

        bool canShowAdBtn = GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockAdBoostLevel &&
            GameManager.Ads.CheckRewardedAdIsLoaded();

        if (canShowAdBtn)
        {
            if(!Ad_Play_Btn.gameObject.activeSelf) Ad_Play_Btn.gameObject.SetActive(true);
            Play_Btn.transform.localPosition = new Vector3(-239, 0);
            Play_Btn.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 209);
        }
        else
        {
            if (Ad_Play_Btn.gameObject.activeSelf) Ad_Play_Btn.gameObject.SetActive(false);
            Play_Btn.transform.localPosition = Vector3.zero;
            Play_Btn.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 209);
        }
    }

    private void SetButtonEvent()
    {
        Play_Btn.SetBtnEvent(OnPlayButtonClick);
        Ad_Play_Btn.SetBtnEvent(OnAdBoostButtonClick);
        Close_Btn.SetBtnEvent(OnCloseButtonClick);
        Play_Btn.interactable = true;
        Ad_Play_Btn.interactable = true;
    }

    private void SetSmallSizePanel()
    {
        BgTrans.sizeDelta = new Vector2(1032, 1304);
        AreaTrans.sizeDelta = new Vector2(794, 300);
        Btn_Group.anchoredPosition = new Vector2(0f, -418f);
        LevelTagManager.GetComponent<RectTransform>().anchoredPosition = new Vector2(-384.2f, 469.1f);
    }

    private void SetBigSizePanel()
    {
        BgTrans.sizeDelta = new Vector2(1032, 1354);
        AreaTrans.sizeDelta = new Vector2(794, 350);
        Btn_Group.anchoredPosition = new Vector2(0f, -441f);
        LevelTagManager.GetComponent<RectTransform>().anchoredPosition = new Vector2(-384.2f, 494.6f);
    }

    private void InitRetryMenu()
    {
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
            panel.RecordGameLoseData();
        
        int level = GameManager.PlayerData.NowLevel;
        PlayerPrefs.SetInt(Constant.PlayerData.LevelFailTime + level, PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + level, 0) + 1);
        GameManager.DataNode.SetData<int>("ContinueLevelCount", 0);

        if (!GameManager.Sound.IsSoundForbidden(SoundType.LOSE.ToString()))
        {
            GameManager.Sound.StopMusic(0.1f);
            GameManager.Sound.PlayAudio(SoundType.LOSE.ToString());
        }

        if (GameManager.PlayerData.LifeNum == 0)
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Lives_None,
new Firebase.Analytics.Parameter("LevelNum", GameManager.PlayerData.NowLevel), new Firebase.Analytics.Parameter("CoinNum", GameManager.PlayerData.CoinNum));
        }
    }

    public void SelectTargetBooster(TotalItemType boostType)
    {
        for (int i = 0; i < Boosters.Length; i++)
        {
            Boosters[i].Refresh();
        }

        if (boostType == TotalItemType.MagnifierBoost)
            Boosters[0].OnButtonClick();
        else if(boostType == TotalItemType.Prop_AddOneStep)
            Boosters[1].OnButtonClick();
        else if (boostType == TotalItemType.FireworkBoost)
            Boosters[2].OnButtonClick();
    }

    private void ShowBoostGuideAnim(TotalItemType boostType)
    {
        LevelPlayBooster targetBooster = null;
        string textKey = string.Empty;
        switch (boostType)
        {
            case TotalItemType.MagnifierBoost:
                targetBooster = Boosters[0];
                textKey = "Game.MagnifierGuide";
                break;
            case TotalItemType.Prop_AddOneStep:
                targetBooster = Boosters[1];
                textKey = "Game.ExtraSlotGuide";
                break;
            case TotalItemType.FireworkBoost:
                targetBooster = Boosters[2];
                textKey = "Game.MegaFireworkGuide";
                break;
        }

        if (PlayerPrefs.GetInt(targetBooster.BoostUnlockSavedKey, 0) != 0)
            return;

        Mask.SetActive(true);
        GameManager.Sound.PlayAudio(SoundType.SFX_Help_Chapter_Unlock.ToString());
        targetBooster.m_LockAnim.AnimationState.SetAnimation(0, "active_lock", false).Complete += t =>
        {
            Mask.SetActive(false);
            PlayerPrefs.SetInt(targetBooster.BoostUnlockSavedKey, 1);
            GameManager.PlayerData.AddInfiniteBoostTime(boostType, 15);
            targetBooster.Refresh();

            GameObject boosterInstance = Instantiate(targetBooster.gameObject, targetBooster.transform.position, Quaternion.identity, GuideMenu.transform);
            var images = boosterInstance.GetComponentsInChildren<Image>();
            images[0].gameObject.SetActive(false);
            foreach (var image in images)
            {
                image.raycastTarget = false;
            }
            LevelPlayBooster booster = boosterInstance.GetComponent<LevelPlayBooster>();
            float infiniteTime = GameManager.PlayerData.GetInfiniteBoostTime(boostType);
            if (infiniteTime > 0)
            {
                booster.m_InfiniteTimer.OnReset();
                booster.m_InfiniteTimer.CountDownTextUseDay = false;
                booster.m_InfiniteTimer.StartCountdown(DateTime.Now.AddMinutes(infiniteTime));
            }
            booster.m_ClickButton.onClick.AddListener(() =>
            {
                if (boosterInstance != null)
                    Destroy(boosterInstance);
                GuideMenu.OnHide();
            });

            Vector3 position = targetBooster.transform.position + new Vector3(0, 0.26f, 0);
            GuideMenu.ShowGuideArrow(position, position + new Vector3(0, 0.12f, 0), PromptBoxShowDirection.Down);
            GuideMenu.tipBox.SetOkButton(false);
            GuideMenu.SetText(textKey);
            GuideMenu.guideImage.GetComponent<Button>().SetBtnEvent(() =>
            {
                if (boosterInstance != null)
                    Destroy(boosterInstance);
                GuideMenu.OnHide();
            });
            GuideMenu.OnShow(null);
        };
    }

    private void StartGame()
    {
        ProcedureUtil.ProcedureMapToGame();
    }

    private void RetryGame()
    {
        int nowLevel = GameManager.PlayerData.NowLevel;

        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Restart,
            new Parameter("Level", nowLevel));

        bool isGameSettingQuit = GameManager.DataNode.GetData<bool>("GameSettingQuit", false);
        if (isGameSettingQuit)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Setting_Restart, new Parameter("Level", nowLevel));

        GameManager.UI.HideUIForm("TileMatchPanel");
        GameManager.DataNode.RemoveNode("GoldTileCurrentCount");

        ProcedureUtil.ProcedureGameToGame(b =>
        {
            if (b) 
            {
                GameManager.UI.HideUIForm(this);

                void retryAction()
                {
                    if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge && GameManager.PlayerData.GetInfiniteLifeTime() <= 0)
                    {
                        GameManager.PlayerData.UseItem(TotalItemData.Life, 1);
                        GameManager.DataNode.SetData<bool>("UseLife", true);
                    }

                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.Level_Start,
                        new Parameter("Level", GameManager.PlayerData.NowLevel));

                    GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.GameToGame));
                }
                    
                if (!GameManager.DataNode.GetData<bool>("UsedAdsBoost", false))
                {
                    GameManager.Ads.ShowInterstitialAd(retryAction);
                }
                else
                {
                    retryAction();
                }
            }
        });
    }

    public void CommonHandle(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.BoostNumChange:
                for (int i = 0; i < Boosters.Length; i++)
                {
                    Boosters[i].Refresh();
                }
                break;
        }
    }

    private void OnPlayButtonClick()
    {
        Play_Btn.interactable = false;
        if (type == LevelPlayType.Play)
        {
            StartGame();
        }
        else if (type == LevelPlayType.Retry)
        {
            RetryGame();
        }

        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            if (Play_Btn != null) 
                Play_Btn.interactable = true;
        });
    }

    private void OnCloseButtonClick()
    {
        if (type == LevelPlayType.Play)
        {
            GameManager.UI.HideUIForm(this);
        }
        else if (type == LevelPlayType.Retry)
        {
            TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (panel != null)
            {
                GameManager.UI.HideUIForm("TileMatchPanel");
                GameManager.UI.HideUIForm(this);

                panel.StartGameLoseToMapProcess(() =>
                {

                });
            }
        }
    }

    private void OnAdBoostButtonClick()
    {
        Ad_Play_Btn.interactable = false;
        if (GameManager.PlayerData.LifeNum > 0 || GameManager.PlayerData.GetInfiniteLifeTime() > 0) 
        {
            GameManager.Ads.ShowRewardedAd("LevelReward");
            GameManager.Firebase.RecordMessageByEvent("PreLevelReward_Ad");
        }
        else
        {
            GameManager.UI.ShowUIForm("LifeShopPanel");
        }
        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            if (Ad_Play_Btn != null) 
                Ad_Play_Btn.interactable = true;
        });
    }

    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.ToString() != "LevelReward")
        {
            return;
        }

        //bool isUserEarnedReward = ne.EarnedReward;
        //if (isUserEarnedReward)
        {
            GameManager.DataNode.SetData("UsedAdsBoost", true);
        }

        if (type == LevelPlayType.Play)
        {
            StartGame();
        }
        else if (type == LevelPlayType.Retry)
        {
            RetryGame();
        }
    }
}
