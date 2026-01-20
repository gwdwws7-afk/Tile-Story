using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using Firebase.Analytics;
using GameFramework.Event;

public class ShopBuyItemPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn, WatchAds_Btn, Buy_Btn;
    [SerializeField] private TextMeshProUGUI NeedCoinNum_Text;
    //[SerializeField] private TextMeshProUGUI TargetNum_Text;
    [SerializeField] private TextMeshProUGUILocalize Title_Text, Content_Text;
    //[SerializeField] private Image[] TileImages;

    [SerializeField] private Transform PanelArea;
    [SerializeField] private TimeLimitLosePanel timeLimitLosePanel;
    
    [SerializeField] private Image BgImage;
    [SerializeField] private Sprite[] BgSprites;
    [SerializeField] private Image TitleLeft, TitleRight;
    [SerializeField] private Sprite[] TitleSprites;
    [SerializeField] private Button CheckBoard_Btn;
    [SerializeField] private GameObject Root, CheckBoardPanel;
    [SerializeField] private PurchaseBanner PurchaseBanner;

    private List<BaseGameFailPanel> AllGameFailPanels;
    BaseGameFailPanel CurGameFailPanel;
    
    private bool isOnlyOnceShowAdsContinueLevelBtn
        => GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Only_Once_Show_Ads_Continue_Btn, true);

    private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.LevelNormal,
        MaterialPresetName.LevelHard,
        MaterialPresetName.LevelSurpHard,
        MaterialPresetName.LevelNormal,
    };

    private int needCoin = 0;
    private bool isHaveAdsContinue = false;
    private bool isDirectQuit = false;
    private Action watchAdsAction;

    public void WatchAdsAction()
    {
    }

    private void SetBgImage()
    {
        if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            Title_Text.Target.enableVertexGradient = true;
            var gradient = new VertexGradient
            {
                topLeft = Color.white,
                topRight = Color.white,
                bottomLeft = new Color(193f/255f,1f,1f,1f),
                bottomRight = new Color(193f/255f,1f,1f,1f)
            };
            Title_Text.Target.colorGradient = gradient;
            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                Title_Text.SetMaterialPreset(quitTextMaterials[3]);
            });
            BgImage.sprite = BgSprites[3];
            TitleLeft.sprite = TitleSprites[3];
            TitleRight.sprite = TitleSprites[3];
            return;
        }
        Title_Text.Target.enableVertexGradient = false;
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            Title_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);
        });
        BgImage.sprite = BgSprites[hardIndex];
        TitleLeft.sprite = TitleSprites[hardIndex];
        TitleRight.sprite = TitleSprites[hardIndex];
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction, object userDatas)
    {
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, RefreshBtn);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnShopBuyGetRewardComplete);

        //准备失败场景数据
        PrepareFailPanelData();
        
        Buy_Btn.enabled = true;
        WatchAds_Btn.enabled = true;
        Close_Btn.gameObject.SetActive(true);
        Buy_Btn.interactable = true;
        WatchAds_Btn.interactable = true;
        Close_Btn.interactable = true;
        CheckBoard_Btn.interactable = true;

        Title_Text.SetTerm("Shop.No More Space!");
        if (userDatas != null)
        {
            var data = userDatas.ChangeType(new
            {
                needCoin = 0,
                isHaveAdsContinue = false,
                watchAdsAction = new Action(WatchAdsAction),
                isDirectQuit = false,
            });
            needCoin = data.needCoin;
            isHaveAdsContinue = data.isHaveAdsContinue;
            watchAdsAction = data.watchAdsAction;
            isDirectQuit = data.isDirectQuit;
        }

        GameManager.DataNode.SetData("isShowAdsBtn", CheckCanShowAdsBtn());
        
        SetBgImage();
        Init(needCoin, isHaveAdsContinue);
        
        ShowFailPanel();

        PurchaseBanner.Init(TotalItemType.None,() =>
        {
            GameManager.UI.HideUIForm(this);
        }, () =>
        {
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.BuyPackageBannerToContinueLevel));
        });
        base.OnInit(uiGroup, completeAction, userDatas);

        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            float fuuRate = panel.GetFuuRate();
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Fail_Fuu, new Parameter("Level", GameManager.PlayerData.NowLevel), new Parameter("Percentage", fuuRate));

            if (fuuRate >= 0.5f) GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Fail_Fuu_Half, new Parameter("Level", GameManager.PlayerData.NowLevel));
        }
    }

    public override void OnRelease()
    {
        GameManager.DataNode.SetData("OutOfMoveLeakCoin", false);
        GameManager.DataNode.SetData("KitchenFailLackOfCoin", false);
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, RefreshBtn);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnShopBuyGetRewardComplete);

        base.OnRelease();
    }

    private bool CheckCanShowAdsBtn()
    {
        int todayMaxTime = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.RV_Times_Continue, 99);
        if (GameManager.PlayerData.TodayWatchContinueAdTime >= todayMaxTime)
        {
            return false;
        }

        return (GameManager.Ads.CheckRewardedAdIsLoaded() && GameManager.Ads.CheckRewardAdCanShow() &&
                                                      ((isOnlyOnceShowAdsContinueLevelBtn && !isHaveAdsContinue) ||
                                                       !isOnlyOnceShowAdsContinueLevelBtn));
    }

    private void PrepareFailPanelData()
    {
        //获取所有的失败界面
        AllGameFailPanels = PanelArea.GetComponentsInChildren<BaseGameFailPanel>(true).ToList();
        //剔除不展示的
        AllGameFailPanels = AllGameFailPanels.Where(obj => obj.IsShowFailPanel).ToList();
        //判断是否有独立展示的fail界面【独立展示 意思是如果有 其他的fail界面就不展示了】
        var gameFailPanels = AllGameFailPanels.Where(obj => obj.IsSpecialPanel).ToList();
        if (gameFailPanels.Count > 0) AllGameFailPanels = gameFailPanels.ToList();
        // 然后按照展示优先级排序
        AllGameFailPanels = AllGameFailPanels.OrderBy(obj => obj.PriorityType).ToList();
    }

    private void Init(int needCoinNum, bool isHaveAdsContinue)
    {
        Content_Text.SetParameterValue("{0}", "<color=#217f02> 3 </color>");

        NeedCoinNum_Text.text = needCoinNum.ToString();

        bool isShowAdsBtn = GameManager.DataNode.GetData("isShowAdsBtn", false);
        
        WatchAds_Btn.gameObject.SetActive(isShowAdsBtn);
        if (isShowAdsBtn)
        {
            Buy_Btn.transform.localPosition = new Vector3(262.5f, 0f);
            Buy_Btn.transform.localScale = Vector3.one;
            Buy_Btn.gameObject.GetComponentInChildren<Image>().rectTransform.sizeDelta = new Vector2(416, 187);
        }
        else
        {
            Buy_Btn.transform.localPosition = Vector3.zero;
            Buy_Btn.transform.localScale = new Vector3(1.1f, 1.1f);
            Buy_Btn.gameObject.GetComponentInChildren<Image>().rectTransform.sizeDelta = new Vector2(580, 187);
        }

        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.DataNode.SetData<int>("ContinueLevelCount", 0);
            if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
            {
                GameManager.UI.HideUIForm(this);
                GameManager.Task.CalendarChallengeManager.CalendarChallengeFail();
                GameManager.Ads.ShowInterstitialAd(ProcedureUtil.ProcedureGameToMap);
                return;
            }

            CloseBtnEvent();
        });

        Buy_Btn.SetBtnEvent(() =>
        {
            RecordMessageByEvent();
            GameManager.Firebase.RecordMessageByEvent("Level_Buy_Continue_Click");

            if (GameManager.PlayerData.UseItem(TotalItemData.Coin, needCoinNum))
            {
                if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
                {
                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.DailyChallenge_Spend_Coin_More_Time,
                        new Parameter("LevelNum", GameManager.Task.CalendarChallengeManager.PlayingLevel),
                        new Parameter("CoinNum", needCoinNum));
                }
                else
                {
                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.Level_Buy_Continue,
                        new Parameter("Level", GameManager.PlayerData.NowLevel));
                }

                GameManager.UI.HideUIForm(this);
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.UseCoinToAddThreeBack));

                watchAdsAction?.Invoke();
            }
            else
            {
                if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
                {
                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.Coin_Not_Enough,
                        new Parameter("LevelID", 7),
                        new Parameter("Source", "BuyContinue"));
                }
                else
                {
                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.Coin_Not_Enough,
                        new Parameter("LevelID", 1),
                        new Parameter("Source", "BuyContinue"));
                }

                if (PlayerPrefs.GetInt("IsFirstCoinNotEnough", 0) == 0)
                {
                    PlayerPrefs.SetInt("IsFirstCoinNotEnough", 1);
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Coin_Not_Enough_FirstTime, new Parameter("Level", GameManager.PlayerData.NowLevel));
                }

                ShopMenuManager.RecordSourceIndex = 1;
                GameManager.DataNode.SetData("OutOfMoveLeakCoin", true);
                GameManager.UI.ShowUIForm("ShopMenuManager",userData: true);
            }
        });

        WatchAds_Btn.SetBtnEvent(() =>
        {
            if (!GameManager.Ads.CheckRewardedAdIsLoaded())
                return;

            RecordMessageByEvent();
            if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
            {
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.DailyChallenge_Watch_AD_More_Time,
                    new Parameter("LevelNum", GameManager.Task.CalendarChallengeManager.PlayingLevel));
            }
            else
            {
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.Level_AD_Continue,
                    new Parameter("Level", GameManager.PlayerData.NowLevel));
            }
            GameManager.Sound.StopSound(GameManager.DataNode.GetData<int>("LoseSoundId", 0));

            GameManager.PlayerData.TodayWatchContinueAdTime += 1;

            Buy_Btn.interactable = false;
            WatchAds_Btn.interactable = false;
            Close_Btn.interactable = false;
            CheckBoard_Btn.interactable = false;
            PurchaseBanner.PauseButton();
            watchAdsAction?.Invoke();

            GameManager.Task.AddDelayTriggerTask(0.4f, () =>
            {
                GameManager.Ads.ShowRewardedAd("ShopBuyItemPanel");
            });
        });

        CheckBoard_Btn.SetBtnEvent(() =>
        {
            SwitchCheckBoardPanel();
        });
    }

    private void SwitchCheckBoardPanel()
    {
        bool isShow = CheckBoardPanel.activeInHierarchy;
        if (isShow)
        {
            Root.SetActive(true);
            CheckBoardPanel.SetActive(false);
        }
        else
        {
            Root.SetActive(false);
            CheckBoardPanel.SetActive(true);
        }
    }

    private void RecordMessageByEvent()
    {
        if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            var ui = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (ui != null)
                GameManager.Firebase.RecordMessageByEvent(
                    "Level_Continue",
                    new Parameter("Level", GameManager.PlayerData.NowLevel),
                    new Parameter("Surplus", ui.SurplusNum.Item1));
        }
    }

    private void GameFail()
    {
        GameManager.DataNode.SetData<bool>("GameSettingQuit", false);
        GameManager.UI.HideUIForm(this);

        // 判断是否需要展示GlacierQuest的失败动画
        if (GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open && !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            //GameManager.UI.HideUIForm(this);
            GameManager.Task.GlacierQuestTaskManager.OnGameFail();
            GameManager.UI.ShowUIForm("GlacierQuestMenu",form =>
            {
                GlacierQuestMenu menu = form as GlacierQuestMenu;
                menu.SetCloseEvent(()=>
                {
                    TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
                    if (panel != null)
                    {
                        panel.StartGameLoseToMapProcess(() =>
                        {
                            GameManager.UI.HideUIForm("TileMatchPanel");
                        });
                    }
                });
            });
        }
        else
        {
            LevelPlayMenu.RecordSourceIndex = 2;
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Retry);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI,userData: new
            {
                needCoin = this.needCoin,
                isHaveAdsContinue = this.isHaveAdsContinue,
                watchAdsAction = this.watchAdsAction,
                isDirectQuit = this.isDirectQuit,
            });
        }
    }

    private void RefreshBtn(object sender, GameEventArgs e)
    {
        //WatchAds_Btn.gameObject.SetActive(!isHaveAdsContinue && GameManager.Ads.CheckRewardedAdIsLoaded());
    }

    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "ShopBuyItemPanel") 
        {
            GameManager.Task.AddDelayTriggerTask(0.4f, () =>
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_AD_Continue_Sucess, new Parameter("Level", GameManager.PlayerData.NowLevel));

                GameManager.UI.HideUIForm(this);
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.WatchAdsToAddThreeBack));

                GameManager.Ads.SetRewardAdColdingTime();
            });
        }
    }

    private void OnShopBuyGetRewardComplete(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;

        if (ne != null && ne.Type == CommonEventType.ShopBuyGetRewardComplete)
        {
            if (GameManager.DataNode.GetData("OutOfMoveLeakCoin", false))
            {
                GameManager.DataNode.SetData("OutOfMoveLeakCoin", false);
                if (GameManager.PlayerData.CoinNum >= needCoin)
                {
                    Buy_Btn.onClick?.Invoke();
                }
            }
        }
    }

    private void CloseBtnEvent()
    {
        Action<bool> action = b =>
        {
            Close_Btn.enabled = b;
            Buy_Btn.enabled = b;
            WatchAds_Btn.enabled = b;
        };
        if (CurGameFailPanel != null)
        {
            action(false);
            CurGameFailPanel.CloseFailPanel(() => 
            {
                action(true);
                CurGameFailPanel = null;
                CloseBtnEvent();
            });
            return;
        }

        //点击时发现没有剩余界面时关闭当前界面
        if (AllGameFailPanels.Count > 0)
        {
            //继续展示
            ShowFailPanel();
        }
        else 
        {
            GameFail();
        }
    }
    
    /// <summary>
    /// 展示失败界面
    /// </summary>
    private void ShowFailPanel()
    {
        //关闭当前的失败页面
        if (CurGameFailPanel != null) CurGameFailPanel.gameObject.SetActive(false);
        //设置当前需要展示的panel
        CurGameFailPanel = AllGameFailPanels[0];
        AllGameFailPanels.RemoveAt(0);
        //展示
        CurGameFailPanel.gameObject.SetActive(true);
        PanelArea.SetChildActive(false,CurGameFailPanel.gameObject);
        CurGameFailPanel.ShowFailPanel(null);
        //
        Title_Text.SetTerm("Settings.Are You Sure?");
    }
}