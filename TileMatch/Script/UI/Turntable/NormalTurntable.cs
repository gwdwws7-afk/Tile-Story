using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class NormalTurntable : UIForm, IItemFlyReceiver
{
    public CoinBarManager coinBar;
    public Transform turntable;
    public Transform body;
    public Transform pointer;
    public Transform sideBar;
    public SkeletonGraphic glowSpine;
    public GameObject targetGlow;
    public GameObject sparkEffect;

    public ParticleSystem LevelBtn_Effect;

    // public GameObject giftGetTick;
    public Image adsButtonImg;
    public GameObject adsIconImg;
    public DelayButton coinSpinButton;
    public Button adsSpinButton;
    public DelayButton closeButton;
    public DelayButton freeButton;

    [SerializeField] private DelayButton FreeByDaliy_Btn;

    // public Button giftButton;
    // public Button luxuryEntranceBtn;
    public TextMeshProUGUI freeButtonText;
    public TextMeshProUGUI adsText;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI coinCostText;
    public CountdownTimer countdownTimer;

    public Material greyMaterial;

    // public SimpleSlider slider;
    // public ItemPromptBox itemPromptBox;
    public ItemSlot[] rewardAreas;
    public SkeletonGraphic[] lights;

    private DTNormalTurntable turntableData;
    private bool isRotating;
    private bool isSpining;
    private bool isGetingReward;
    private int normalTurntableCoinSpinTime;
    private int normalTurntableAdsSpinTime;
    private int normalTurntableFreeSpinTime;
    private Action turntableNewTurnAction;
    private long adsSpinLimitTime;

    private int[] coinSpinCost = new int[] { 100, 200, 300, 400, 800 };

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        OnResume();
        base.OnInit(uiGroup, completeAction, userData);
        GameManager.Sound.PlayMusic("SFX_Wheel_Bgm");
        
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnRewardAdStateChanged);
        coinBar.OnInit(uiGroup, completeAction, userData);
        // coinBar.gameObject.SetActive(false);
        turntableData = GameManager.DataTable.GetDataTable<DTNormalTurntable>().Data;
        RewardManager.Instance.RegisterItemFlyReceiver(this);

        rewardAreas[0].transform.localScale = Vector3.one;
        for (int i = 0; i < rewardAreas.Length; i++)
        {
            List<TotalItemData> turntableRewards = turntableData.GetTurntableRewardByIndex(i).RewardTypeList;
            List<int> turntableRewardsNum = turntableData.GetTurntableRewardByIndex(i).RewardNumList;
            rewardAreas[i].OnInit(turntableRewards[0], turntableRewardsNum[0]);
        }

        coinSpinButton.OnInit(OnCoinSpinButtonClick);
        adsSpinButton.onClick.AddListener(OnAdsSpinButtonClick);
        freeButton.OnInit(OnFreeSpinButtonClick);
        closeButton.SetBtnEvent(OnCloseButtonClick);

        // giftButton.onClick.AddListener(OnGiftButtonClick);
        // luxuryEntranceBtn.onClick.AddListener(OnLuxuryEntranceBtnClick);

        isRotating = false;
        isGetingReward = false;
        body.localEulerAngles = Vector3.zero;
        sideBar.localEulerAngles = Vector3.zero;

        normalTurntableCoinSpinTime = GameManager.PlayerData.NormalTurntableCoinSpinTime;
        normalTurntableAdsSpinTime = GameManager.PlayerData.NormalTurntableAdsSpinTime;
        normalTurntableFreeSpinTime = GameManager.PlayerData.NormalTurntableFreeSpinTime;

        targetGlow.SetActive(false);

        adsSpinLimitTime = GameManager.Firebase.GetLong(Constant.RemoteConfig.SpinAdTimesLimit, Constant.GameConfig.AdsSpinLimitTime);
        if (normalTurntableAdsSpinTime < adsSpinLimitTime)
        {
            DateTime nextReadyTime = GameManager.PlayerData.TurntableNextAdsSpinReadyTime;

            if (DateTime.Now > nextReadyTime)
            {
                countdownTimer.timeText.gameObject.SetActive(false);
            }
            else
            {
                countdownTimer.OnReset();
                countdownTimer.CountdownOver += OnCountdownOver;
                countdownTimer.StartCountdown(nextReadyTime);
                countdownTimer.timeText.gameObject.SetActive(true);
                loadingText.gameObject.SetActive(false);

                adsText.gameObject.SetActive(false);

                adsButtonImg.material = greyMaterial;
                adsIconImg.SetActive(false);
            }
        }

        RefreshLayout();

        adsSpinButton.gameObject.SetActive(!GameManager.PlayerData.IsFirstFreeTurnByDaliy);
        FreeByDaliy_Btn.gameObject.SetActive(GameManager.PlayerData.IsFirstFreeTurnByDaliy);
        FreeByDaliy_Btn.SetBtnEvent(() =>
        {
            if (isRotating || isGetingReward)
                return;
            bool spinSuccess = ShowSpinAnim(false);
            if (spinSuccess)
            {
                adsSpinButton.gameObject.SetActive(true);
                FreeByDaliy_Btn.gameObject.SetActive(false);
                GameManager.PlayerData.IsFirstFreeTurnByDaliy = false;
            }
        });
    }

    private void OnRewardAdStateChanged(object sender, GameEventArgs e)
    {
        if (!(e is CommonEventArgs { Type: CommonEventType.RewardAdsShownEvent } ne))
        {
            return;
        }

        DateTime nextReadyTime = GameManager.PlayerData.TurntableNextAdsSpinReadyTime;

        var flag = GameManager.Ads.CheckRewardedAdIsLoaded();
        if (DateTime.Now > nextReadyTime)
        {
            adsText.gameObject.SetActive(flag);
            loadingText.gameObject.SetActive(!flag);

            adsButtonImg.material = flag ? null : greyMaterial;
            adsIconImg.SetActive(flag);
        }
    }


    private void OnEnable()
    {
        StartCoroutine(ShowLightAnim());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public override void OnReset()
    {
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnRewardAdStateChanged);

        RewardManager.Instance.UnregisterItemFlyReceiver(this);

        for (int i = 0; i < rewardAreas.Length; i++)
        {
            rewardAreas[i].OnReset();
        }

        coinSpinButton.onClick.RemoveAllListeners();
        adsSpinButton.onClick.RemoveAllListeners();
        freeButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        coinBar.OnRelease();
        //GameManager.DataNode.SetData("RevealFromLuxuryTurntable", false);

        base.OnReset();
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

        for (int i = 0; i < rewardAreas.Length; i++)
        {
            rewardAreas[i].OnRelease();
        }

        // slider.OnReset();
        // itemPromptBox.OnRelease();

        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);

        if (isSpining && (body.localEulerAngles.z + 15f) % 45 < 4f)
        {
            pointer.DOKill();
            pointer.DORotate(new Vector3(0, 0, 22), 0.1f).onComplete = () => { pointer.DORotate(Vector3.zero, 0.35f); };

            // GameManager.Sound.PlayAudio("SFX_WheelRoll");
        }

        coinBar.OnUpdate(elapseSeconds, realElapseSeconds);

        // if (itemPromptTimer > 0)
        // {
        //     itemPromptTimer -= elapseSeconds;
        //     if (itemPromptTimer <= 0)
        //     {
        //         itemPromptBox.HidePromptBox();
        //     }
        // }
    }

    public override void OnReveal()
    {
        // ShowLuxuryTurntableGuide();

        base.OnReveal();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        //if (gameObject.activeSelf)
        //{
        //    base.OnShow(showSuccessAction, userData);
        //    return;
        //}

        turntable.DOKill();
        turntable.localScale = new Vector3(0.4f, 0.4f);
        gameObject.SetActive(true);
        turntable.gameObject.SetActive(true);
        turntable.DOScale(1.05f, 0.15f).onComplete = () =>
        {
            turntable.DOScale(0.97f, 0.15f).onComplete = () =>
            {
                turntable.DOScale(1f, 0.15f).onComplete = () =>
                {
                    // if (!closeRewardPrompt && !itemPromptBox.gameObject.activeSelf) 
                    // {
                    //     OnGiftButtonClick();
                    // }
                };
            };
        };


        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        // itemPromptBox.HidePromptBox();
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnClose()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }

        GameManager.Process.EndProcess("AutoShowTurntable");
        GameManager.Process.EndProcess("CheckTurntableUnlock");

        GameManager.UI.HideUIForm(this);
        
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
    }

    IEnumerator ShowLightAnim()
    {
        while (true)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (i < 4)
                {
                    lights[i].AnimationState.SetAnimation(0, "glow", false);
                }
                else
                {
                    lights[i].AnimationState.ClearTrack(0);
                }
            }

            yield return new WaitForSeconds(0.633f);

            for (int i = 0; i < lights.Length; i++)
            {
                if (i < 4)
                {
                    lights[i].AnimationState.ClearTrack(0);
                }
                else
                {
                    lights[i].AnimationState.SetAnimation(0, "glow", false);
                }
            }

            yield return new WaitForSeconds(0.633f);
        }
    }

    private void RefreshLayout()
    {
        coinBar.RefreshCoinText();

        //coinButtonChanceText.SetText($"({Constant.GameConfig.CoinSpinLimitTime - normalTurntableCoinSpinTime}/{Constant.GameConfig.CoinSpinLimitTime})");

        if (normalTurntableCoinSpinTime < coinSpinCost.Length)
        {
            coinCostText.SetText(coinSpinCost[normalTurntableCoinSpinTime].ToString());
        }
        else
        {
            coinCostText.SetText(coinSpinCost[coinSpinCost.Length - 1].ToString());
        }

        if (normalTurntableFreeSpinTime < Constant.GameConfig.FreeSpinLimitTime)
        {
            freeButton.gameObject.SetActive(true);
            adsSpinButton.gameObject.SetActive(false);
            coinSpinButton.gameObject.SetActive(false);
        }
        else if (normalTurntableAdsSpinTime < adsSpinLimitTime)
        {
            freeButton.gameObject.SetActive(false);
            adsSpinButton.gameObject.SetActive(true);

            if (normalTurntableCoinSpinTime < Constant.GameConfig.CoinSpinLimitTime)
            {
                coinSpinButton.gameObject.SetActive(true);

                adsSpinButton.transform.localPosition = new Vector3(-265f, adsSpinButton.transform.localPosition.y);
                coinSpinButton.transform.localPosition = new Vector3(265f, coinSpinButton.transform.localPosition.y);
            }
            else
            {
                coinSpinButton.gameObject.SetActive(false);

                adsSpinButton.transform.localPosition = new Vector3(0f, adsSpinButton.transform.localPosition.y);
            }

            if (!countdownTimer.timeText.gameObject.activeSelf)
            {
                if (GameManager.Ads.CheckRewardedAdIsLoaded())
                {
                    adsText.gameObject.SetActive(true);
                    loadingText.gameObject.SetActive(false);

                    adsButtonImg.material = null;
                    adsIconImg.SetActive(true);
                }
                else
                {
                    adsText.gameObject.SetActive(false);
                    loadingText.gameObject.SetActive(true);

                    adsButtonImg.material = greyMaterial;
                    adsIconImg.SetActive(false);
                }
            }
        }
        else
        {
            freeButton.gameObject.SetActive(false);
            adsSpinButton.gameObject.SetActive(false);
            coinSpinButton.gameObject.SetActive(true);

            if(GameManager.PlayerData.IsFirstFreeTurnByDaliy)
                coinSpinButton.transform.localPosition = new Vector3(265f, coinSpinButton.transform.localPosition.y);
            else
                coinSpinButton.transform.localPosition = new Vector3(0, coinSpinButton.transform.localPosition.y);
        }
    }

    public void OnCloseButtonClick()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }

        GameManager.Process.EndProcess("AutoShowTurntable");
        GameManager.Process.EndProcess("CheckTurntableUnlock");
        GameManager.UI.HideUIForm(this);
        
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
    }

    public void OnCoinSpinButtonClick()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }

        if (normalTurntableCoinSpinTime >= Constant.GameConfig.CoinSpinLimitTime)
        {
            GameManager.UI.ShowWeakHint("Turntable.No more chances today");
            return;
        }

        int coinCost = normalTurntableCoinSpinTime < coinSpinCost.Length
            ? coinSpinCost[normalTurntableCoinSpinTime]
            : coinSpinCost[coinSpinCost.Length - 1];

        if (GameManager.PlayerData.CoinNum >= coinCost)
        {
            bool spinSuccess = ShowSpinAnim(true);
            if (spinSuccess)
            {
                GameManager.PlayerData.UseItem(TotalItemData.Coin, coinCost);
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Spin_Play_ByCoins_Normal,
                    new Parameter("CoinCost", coinCost));
                normalTurntableCoinSpinTime++;
                GameManager.PlayerData.NormalTurntableCoinSpinTime = normalTurntableCoinSpinTime;

                GameManager.Firebase.RecordCoinSpend("Spin", coinCost);
            }
        }
        else
        {
            coinSpinButton.interactable = false;
            GameManager.UI.ShowUIForm("ShopMenuManager",(obj) => { coinSpinButton.interactable = true; });
            GameManager.Firebase.RecordCoinNotEnough(5, GameManager.PlayerData.NowLevel);
        }
    }

    public void OnAdsSpinButtonClick()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }

        if (normalTurntableAdsSpinTime >= adsSpinLimitTime)
        {
            GameManager.UI.ShowWeakHint("Turntable.No more chances today");
            return;
        }

        DateTime nextReadyTime = GameManager.PlayerData.TurntableNextAdsSpinReadyTime;

        if (DateTime.Now > nextReadyTime)
        {
            if (GameManager.Ads.CheckRewardedAdIsLoaded())
            {
                OnPause();
                if (!GameManager.Ads.ShowRewardedAd("TurntableReward"))
                {
                    OnResume();
                }
            }
            else
            {
                GameManager.UI.ShowWeakHint("Common.Ad is still loading...",
                    new Vector3(0, adsSpinButton.transform.position.y + 0.2f));
            }
        }
        else
        {
            GameManager.UI.ShowWeakHint("Common.Ad is still loading...",
                new Vector3(0, adsSpinButton.transform.position.y + 0.2f));
        }
    }

    public void OnFreeSpinButtonClick()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }

        if (normalTurntableFreeSpinTime < Constant.GameConfig.FreeSpinLimitTime)
        {
            bool spinSuccess = ShowSpinAnim(false);
            if (spinSuccess)
            {
                normalTurntableFreeSpinTime = 1;
                GameManager.PlayerData.NormalTurntableFreeSpinTime = normalTurntableFreeSpinTime;
                GameManager.Event.Fire(this, TurntableAdsStateChangedEventArgs.Create());

                RefreshLayout();
            }
        }
    }

    public void OnGiftButtonClick()
    {
        // if (!itemPromptBox.gameObject.activeSelf)
        // {
        //     TotalItemType[] chestReward = new TotalItemType[] {TotalItemData.Coin };
        //     int[] nums = new int[] { 500 };
        //
        //     itemPromptBox.Init(chestReward, nums);
        //     itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, giftButton.transform.position);
        //     itemPromptTimer = 3f;
        // }
        // else
        // {
        //     closeRewardPrompt = true;
        //     itemPromptBox.HidePromptBox();
        // }
    }

    private void OnLuxuryEntranceBtnClick()
    {
        if (isRotating || isGetingReward)
        {
            return;
        }
    }

    private bool ShowSpinAnim(bool isCoinSpin)
    {
        if(coinBar.addButton)
            coinBar.addButton.interactable = false;
        // GameManager.Sound.StopMusic();

        TurntableReward randomReward = GetRandomTurntableReward(isCoinSpin);
        GameManager.DataNode.SetData("NormalTurntableRewardIsCoin", isCoinSpin);

        if (!isCoinSpin)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Spin_Play_ByAds);
        if (randomReward == null)
        {
            Log.Error("Spin Reward is null");
            return false;
        }

        Log.Info("Turntable Random Reward is {0}", randomReward.Index.ToString());

        isRotating = true;

        int cycleNum = 6;
        int rewardDegree = (randomReward.Index - 1) * -45;

        //float rotateDegree = cycleNum * 360 + rewardDegree + UnityEngine.Random.Range(-7f, 8f);
        float rotateDegree = cycleNum * 360 + rewardDegree;
        float duration = 7;

        isSpining = true;

        int? drumSoundId = GameManager.Sound.PlayAudio("SFX_WheelDrum", 10, true);

        body.DORotate(new Vector3(0, 0, -rotateDegree), duration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
            .onComplete = () =>
        {
            isSpining = false;
            OnTurntableSpinEnd(randomReward);
        };
        ;
        sideBar.DORotate(new Vector3(0, 0, -rotateDegree), duration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);

        pointer.DORotate(new Vector3(0, 0, 22), 0.1f).SetDelay(0.02f).onComplete = () =>
        {
            pointer.DORotate(new Vector3(0, 0, 20), 0.1f);
        };

        ParticleSystem[] sparks = null;
        if (!ReferenceEquals(sparkEffect, null))
        {
            sparks = sparkEffect.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i].Play();
            }
        }

        GameManager.Task.AddDelayTriggerTask(6f, () =>
        {
            if (drumSoundId.HasValue)
            {
                GameManager.Sound.StopSound(drumSoundId.Value, 1f);
            }
        });

        GameManager.Task.AddDelayTriggerTask(5.5f, () =>
        {
            if (sparks != null)
            {
                for (int i = 0; i < sparks.Length; i++)
                {
                    sparks[i].Stop();
                }
            }
        });

        int spinTime = GameManager.PlayerData.NormalTurntableDailySpinTime;
        GameManager.PlayerData.NormalTurntableDailySpinTime = spinTime + 1;
        if (spinTime + 1 == 5)
        {
            isGetingReward = true;
        }

        GameManager.Event.Fire(this, TurntableAdsStateChangedEventArgs.Create());

        if (GameManager.PlayerData.NowLevel >= Constant.Objective.UnlockLevel) 
            GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Turntable, 1);
        return true;
    }

    private void OnTurntableSpinEnd(TurntableReward randomReward)
    {
        GameManager.Sound.PlayAudio("SFX_wheelDing");

        //outlineShiny.transform.parent.localEulerAngles = new Vector3(0, 0, -45 * (randomReward.Index - 1));
        targetGlow.transform.localEulerAngles = new Vector3(0, 0, -45 * (randomReward.Index - 1));
        targetGlow.SetActive(true);
        
        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();

        GameManager.Task.AddDelayTriggerTask(0.4f, () =>
        {
            glowSpine.Initialize(false);
            glowSpine.AnimationState.SetAnimation(0, "01", false).Complete += (t) =>
            {
                ShowTurntableReward(randomReward);
            };

            // GameManager.Sound.PlayAudio("SFX_WheelGetGift");
        });
    }

    private void ShowTurntableReward(TurntableReward randomReward)
    {
        List<TotalItemData> turntableRewards = turntableData.GetTurntableRewardByIndex(randomReward.Index - 1).RewardTypeList;
        List<int> turntableRewardsNum = turntableData.GetTurntableRewardByIndex(randomReward.Index - 1).RewardNumList;

        for (int i = 0; i < turntableRewards.Count; i++)
        {
            RewardManager.Instance.AddNeedGetReward(turntableRewards[i], turntableRewardsNum[i]);
            if (turntableRewards[i] == TotalItemData.Coin)
            {
                GameManager.Firebase.RecordCoinGet("Spin", turntableRewardsNum[i]);
            }
            else
            {
                GameManager.Firebase.RecordLevelToolsGet("Spin", turntableRewards[i], turntableRewardsNum[i]);
            }
        }

        RewardPanelType rewardPanelType = RewardPanelType.TurntableRewardPanel;

        RefreshLayout();

        RewardManager.Instance.ShowNeedGetRewards(rewardPanelType, false, () =>
        {
            if (coinBar.addButton)
                coinBar.addButton.interactable = true;
            if (rewardPanelType == RewardPanelType.JackpotRewardPanel)
            {
                rewardAreas[0].transform.DOScale(1.05f, 0.2f).onComplete = () =>
                {
                    rewardAreas[0].transform.DOScale(1f, 0.2f);
                };
            }

            isGetingReward = false;
            // GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        }, () =>
        {
            isRotating = false;
            isGetingReward = true;

            targetGlow.SetActive(false);

            if (turntableNewTurnAction != null)
            {
                turntableNewTurnAction.Invoke();
                turntableNewTurnAction = null;
            }
        });

        if (randomReward.Index == 1)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Spin_Jackpot);
    }

    public override void OnPause()
    {
        coinSpinButton.interactable = false;
        adsSpinButton.interactable = false;
        closeButton.interactable = false;
        freeButton.interactable = false;
        if (coinBar.addButton)
            coinBar.addButton.interactable = false;

        // giftButton.interactable = false;
        // if (luxuryEntranceBtn != null)
        //     luxuryEntranceBtn.interactable = false;
        base.OnPause();
    }

    public override void OnResume()
    {
        coinSpinButton.interactable = true;
        adsSpinButton.interactable = true;
        closeButton.interactable = true;
        freeButton.interactable = true;
        if(coinBar.addButton)
            coinBar.addButton.interactable = true;
        // giftButton.interactable = true;
        // if (luxuryEntranceBtn != null) 
        //     luxuryEntranceBtn.interactable = true;
        base.OnResume();
    }

    private TurntableReward GetRandomTurntableReward(bool isCoinSpin)
    {
        if (turntableData == null)
        {
            turntableData = GameManager.DataTable.GetDataTable<DTNormalTurntable>().Data;
        }

        List<TurntableReward> turntableRewards = turntableData.turntableRewardData;
// #if UNITY_EDITOR
//         return turntableRewards[0];
// #endif
        //花金币转转盘，第一次必得JACKPOT! 第二次转五次金币转盘触发保底，第三次转10次金币转盘触发保底，六次及以后保底需要30次转转盘，提前触发保底则进入下次保底累计
        // if (isCoinSpin || true)
        if (true)
        {
            int cumulativeNumber = GameManager.PlayerData.TurntableCoinSpinCumulativeNumber;
            cumulativeNumber++;

            int spinGuaranteeLevel = GameManager.PlayerData.TurntableCoinSpinGuaranteeLevel;
            int spinGuarantee = 1;

            if (spinGuaranteeLevel == 1)
            {
                spinGuarantee = 1;
            }
            else if (spinGuaranteeLevel == 2)
            {
                spinGuarantee = 5;
            }
            else if (spinGuaranteeLevel == 3)
            {
                spinGuarantee = 10;
            }
            else if (spinGuaranteeLevel >= 6)
            {
                spinGuarantee = 30;
            }

            if (cumulativeNumber >= spinGuarantee)
            {
                GameManager.PlayerData.TurntableCoinSpinGuaranteeLevel = spinGuaranteeLevel + 1;
                GameManager.PlayerData.TurntableCoinSpinCumulativeNumber = 0;
                return turntableRewards[0];
            }
            else
            {
                GameManager.PlayerData.TurntableCoinSpinCumulativeNumber = cumulativeNumber;
            }
        }

        //没生命时必转到生命
        // if (GameManager.PlayerData.LifeNum <= 0 && GameManager.PlayerData.InfiniteLifeTime <= 0) 
        // {
        //     for (int i = 0; i < turntableRewards.Count; i++)
        //     {
        //         if (turntableRewards[i].Rewards == "3")
        //         {
        //             return turntableRewards[i];
        //         }
        //     }
        // }

        float totalProbability = 0;
        for (int i = 0; i < turntableRewards.Count; i++)
        {
            totalProbability += turntableRewards[i].Probability;
        }

        float randomNum = UnityEngine.Random.Range(0, totalProbability);
        for (int i = 0; i < turntableRewards.Count; i++)
        {
            if (turntableRewards[i].Probability >= randomNum)
            {
                return turntableRewards[i];
            }
            else
            {
                randomNum -= turntableRewards[i].Probability;
            }
        }

        Log.Error("Get Random Spin Reward Error");
        return turntableRewards[1];
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        countdownTimer.OnReset();
        countdownTimer.timeText.gameObject.SetActive(false);

        RefreshLayout();
    }

    public void OnRewardAdLoaded(object sender, GameEventArgs e)
    {
        if (adsText == null || loadingText == null || adsButtonImg == null)
        {
            return;
        }

        DateTime nextReadyTime = GameManager.PlayerData.TurntableNextAdsSpinReadyTime;

        if (DateTime.Now > nextReadyTime)
        {
            adsText.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(false);

            adsButtonImg.material = null;
            adsIconImg.SetActive(true);
        }
    }


    // public void ShowLuxuryTurntableGuide()
    // {
    //     if (!GameManager.PlayerData.GetBool(Constant.PlayerData.HasShownLuxuryTurntableGuide, false) && GameManager.DataNode.GetData("RevealFromLuxuryTurntable", false))
    //     {
    //         GameManager.PlayerData.SetBool(Constant.PlayerData.HasShownLuxuryTurntableGuide, true);
    //         OnPause();
    //         if (handler.IsValid())
    //         {
    //             UnityUtility.UnloadAssetAsync(handler);
    //         }
    //         handler = UnityUtility.InstantiateAsync("CommonGuideMenu",transform, (form) =>
    //          {
    //              OnResume();
    //              //Transform originalParent = luxuryEntranceBtn.transform.parent;
    //              //luxuryEntranceBtn.transform.SetParent(form.transform);
    //              luxuryEntranceBtn.transform.SetAsLastSibling();
    //
    //              CommonGuideMenu guideMenu = form.GetComponent<CommonGuideMenu>();
    //              guideMenu.tipBox.transform.position = new Vector3(0, luxuryEntranceBtn.transform.position.y - 0.7f);
    //              guideMenu.tipBox.SetOkButton(true);
    //              guideMenu.SetText("Turntable.Tap here to the Luxury Spin.");
    //              guideMenu.SetArrowScale(new Vector3(0.7f, 0.7f));
    //              guideMenu.ShowGuideArrow(luxuryEntranceBtn.transform.position + new Vector3(0.25f, 0f), luxuryEntranceBtn.transform.position + new Vector3(0.2f, 0), PromptBoxShowDirection.Left);
    //              guideMenu.guideImage.OnInit(new Vector3(0, 0), 0, 0);
    //              guideMenu.OnShow();
    //
    //              void targetBtnClickAction()
    //              {
    //                  //luxuryEntranceBtn.transform.SetParent(originalParent);
    //                  luxuryEntranceBtn.onClick.RemoveListener(targetBtnClickAction);
    //                  if (form != null)
    //                  {
    //                      guideMenu.OnHide();
    //                      guideMenu.OnReset();
    //                  }
    //              }
    //
    //              luxuryEntranceBtn.interactable = true;
    //              guideMenu.guideImage.onAreaClick += targetBtnClickAction;
    //              luxuryEntranceBtn.onClick.AddListener(targetBtnClickAction);
    //          });
    //     }
    // }


    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.ToString() != "TurntableReward")
        {
            return;
        }

        bool isUserEarnedReward = true; //ne.EarnedReward;
        if (isUserEarnedReward)
        {
            try
            {
                OnResume();
            }
            catch (Exception exception)
            {
                Log.Error("NormalTurntable OnRewardAdEarned Error {0}", exception.Message);
            }

            normalTurntableAdsSpinTime++;
            GameManager.PlayerData.NormalTurntableAdsSpinTime = normalTurntableAdsSpinTime;
            ShowSpinAnim(false);

            if (normalTurntableAdsSpinTime < adsSpinLimitTime)
            {
                GameManager.PlayerData.TurntableNextAdsSpinReadyTime = DateTime.Now.AddMinutes(5);
                if (countdownTimer != null)
                {
                    countdownTimer.OnReset();
                    countdownTimer.CountdownOver += OnCountdownOver;
                    countdownTimer.StartCountdown(DateTime.Now.AddMinutes(5));
                    countdownTimer.timeText.gameObject.SetActive(true);
                }

                loadingText.gameObject.SetActive(false);

                adsText.gameObject.SetActive(false);

                adsButtonImg.material = greyMaterial;
                adsIconImg.SetActive(false);
            }

            RefreshLayout();

            GameManager.PlayerData.TurntableShowWarningTime = DateTime.Now.AddMinutes(5);
            GameManager.Event.Fire(this, TurntableAdsStateChangedEventArgs.Create());
        }
        else
        {
            OnResume();
            RefreshLayout();
        }
    }

    public ReceiverType ReceiverType
    {
        get => ReceiverType.Common;
    }

    public GameObject GetReceiverGameObject()
    {
        if (GameManager.DataNode.GetData("NormalTurntableRewardIsCoin", false))
        {
            return coinSpinButton.gameObject;
        }
        else
        {
            return adsSpinButton.gameObject;
        }
    }

    public void OnFlyHit(TotalItemData type)
    {
        var isCoin = GameManager.DataNode.GetData("NormalTurntableRewardIsCoin", false);
        var target = isCoin ? coinSpinButton.transform : adsSpinButton.transform;
        target.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () => { target.localScale = Vector3.one; };
    }

    public void OnFlyEnd(TotalItemData type)
    {
        var isCoin = GameManager.DataNode.GetData("NormalTurntableRewardIsCoin", false);
        var target = isCoin ? coinSpinButton.transform : adsSpinButton.transform;
        target.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () => { target.localScale = Vector3.one; };
        LevelBtn_Effect.transform.position = target.position;
        if (LevelBtn_Effect != null)
        {
            LevelBtn_Effect.Play();
        }
    }

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        if (GameManager.DataNode.GetData("NormalTurntableRewardIsCoin", false))
        {
            return coinSpinButton.transform.position;
        }
        else
        {
            return adsSpinButton.transform.position;
        }
    }
}