using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Firebase.Analytics;
using GameFramework.Event;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FrogJumpMenu : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn, Ads_Btn, Purchase_Btn;
    [SerializeField] private ScrollArea scrollArea;
    [SerializeField] private ClockBar clockBar;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private GameObject loadingText, adsText,tapText,buyText;
    [SerializeField] private Image adsButtonImg, adsIcon;
    [SerializeField] private Transform Frog;
    [SerializeField] private Transform FrogParent;
    [SerializeField] private SkeletonGraphic FrogSpine;
    [SerializeField] private Material gray;
    [SerializeField] private Transform topAnchor, bottomAnchor;
    [SerializeField] private TextMeshProUGUI priceText;
    private Dictionary<int, FrogLeafManager> FrogLeafs;
    private int _curLevel = 0;
    private bool _isFirst = true;

    private readonly int[] _left = new int[] { 1, 3, 6, 9, 12, 15, 18, 21 };

    private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        // GameManager.PlayerData.CurFrogJumpLevel = 0;
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnCommonEvent);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        _curLevel = GameManager.PlayerData.CurFrogJumpLevel;
        FrogLeafs = new Dictionary<int, FrogLeafManager>();
        _isFirst = true;
        SetPanels();
        SetBtnEvent();
        SetFrogState(_curLevel);
        SetAdButtonState();
        tapText.SetActive(false);
        clockBar.StartCountdown(DateTime.Today.AddDays(1));
        clockBar.CountdownOver += OnCountDownOver;
        var price = GameManager.Purchase.GetPrice(ProductNameType.Frog_ad);
        if (string.IsNullOrEmpty(price))
        {
            priceText.gameObject.SetActive(false);
            buyText.SetActive(true);
        }
        else
        {
            priceText.gameObject.SetActive(true);
            buyText.SetActive(false);
            priceText.text = price;
        }
        OnResume();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private bool _isFirstShow;
    public override void OnShowInit(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        _isFirstShow = true;
    }

    public override void OnReset()
    {
        FrogLeafs.Clear();
        scrollArea.OnReset();
        clockBar.OnReset();
        countdownTimer.OnReset();
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnCommonEvent);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        StopAllCoroutines();
        base.OnReset();
    }
    
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        if (_isPurchase)
        {
#if UNITY_EDITOR||UNITY_STANDALONE
            if (Input.GetMouseButtonUp(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                StopAllCoroutines();
                OnClose();
                (GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager)?.SetFrogJumpEntrance();
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
            }
        }

        if (scrollArea.CheckSpawnComplete() && _isFirstShow && GameManager.PlayerData.IsFirstStartFrogJumpActivity && gameObject.activeSelf)
        {
            _isFirstShow = false;
            GameManager.PlayerData.IsFirstStartFrogJumpActivity = false;
            StartCoroutine(ScrollToBottom());
        }
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }

    private void SetPanels()
    {
        scrollArea.AddColumnFirst(new FrogJumpColumn("FrogJumpFirstPart", scrollArea, 0, 1024));
        for (int i = 1; i < 8; i++)
        {
            scrollArea.AddColumnFirst(new FrogJumpColumn("FrogJumpMidPart", scrollArea, i, 1024));
        }

        scrollArea.AddColumnFirst(new FrogJumpColumn("FrogJumpLastPart", scrollArea, 8, 1024));
        if (GameManager.PlayerData.IsFirstStartFrogJumpActivity)
        {
            scrollArea.currentIndex = 0;
            scrollArea.OnInit(GetComponent<RectTransform>());
        }
        else
        {
            scrollArea.OnInit(GetComponent<RectTransform>());
            scrollArea.CenterTheTargetColumn(8f - GetPanelIndexFloatByLevel(GameManager.PlayerData.CurFrogJumpLevel), 0f);
        }
        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }
    
    private int GetPanelIndexByLevel(int level)
    {
        if (level <= 1) return 0;
        return (level - 2) / 3 + 1;
    }
    
    private float GetPanelIndexFloatByLevel(int level)
    {
        if (level == 0) return 0.3f;
        if (level == 1) return 0.7f;
        return (level - 1) / 3f + 2 / 3f;
    }

    private void SetFrogState(int level)
    {
        Frog.localScale = _left.Contains(level)
            ? new Vector3(1, 1, 0)
            : new Vector3(-1, 1, 0);
    }
    
    private void SetAdButtonState()
    {
        if (DateTime.Now < GameManager.PlayerData.NextFrogAdReadyTime)
        {
            adsText.SetActive(false);
            adsIcon.gameObject.SetActive(true);
            loadingText.SetActive(false);
            adsButtonImg.material = gray;
            adsIcon.material = gray;
            countdownTimer.gameObject.SetActive(true);
            countdownTimer.StartCountdown(GameManager.PlayerData.NextFrogAdReadyTime);
            countdownTimer.CountdownOver += (sender, e) => { SetAdButtonState(); };
            return;
        }

        countdownTimer.gameObject.SetActive(false);
        if (!GameManager.Ads.CheckRewardedAdIsLoaded())
        {
            adsIcon.gameObject.SetActive(false);
            loadingText.SetActive(true);
            adsText.SetActive(false);
            adsButtonImg.material = gray;
        }
        else
        {
            loadingText.SetActive(false);
            adsIcon.gameObject.SetActive(true);
            adsText.SetActive(true);
            adsButtonImg.material = null;
            adsIcon.material = null;
        }
    }

    private void SetBtnEvent()
    {
        Ads_Btn.gameObject.SetActive(true);
        Purchase_Btn.gameObject.SetActive(true);
        Close_Btn.SetBtnEvent(OnClose);
        Ads_Btn.SetBtnEvent(OnAdsButtonClick);
        Purchase_Btn.SetBtnEvent(OnPurchaseButtonClick);
    }
    
    private bool _isPurchase = false;
    private void OnPurchaseButtonClick()
    {
        GameManager.Purchase.BuyProduct(ProductNameType.Frog_ad, () =>
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Frog_ad_Buy,new Parameter("level",_curLevel));
            GameManager.PlayerData.CurFrogJumpLevel = GameManager.PlayerData.MaxFrogJumpLevel;
            for (var i = _curLevel + 1; i <= GameManager.PlayerData.MaxFrogJumpLevel; i++)
            {
                var kv = GameManager.PlayerData.FrogJumpData.GetReward(i);
                RewardManager.Instance.AddNeedGetReward(kv.Key, kv.Value);
            }
            _isPurchase = true;
            StartCoroutine(FrogJumpToLeaf2(GameManager.PlayerData.MaxFrogJumpLevel));
            tapText.SetActive(true);
            Ads_Btn.gameObject.SetActive(false);
            Purchase_Btn.gameObject.SetActive(false);
        });
    }

    private void OnAdsButtonClick()
    {
        var nextReadyTime = GameManager.PlayerData.NextFrogAdReadyTime;
        if (DateTime.Now > nextReadyTime)
        {
            if (GameManager.Ads.CheckRewardedAdIsLoaded())
            {
                OnPause();
                if (!GameManager.Ads.ShowRewardedAd("FrogJump"))
                {
                    OnResume();
                }
                else
                {
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Frog_ad_Show);
                }
            }
            else
            {
                GameManager.UI.ShowWeakHint("Common.Ad is still loading...",
                    new Vector3(0, 0));
            }
        }
        else
        {
            GameManager.UI.ShowWeakHint("Common.Ad is still loading...",
                new Vector3(0, 0));
        }
    }

    private Vector3 GetLeafLocalPosition(FrogLeafManager leafManager)
    {
        var pos = leafManager.transform.localPosition;
        var col = GetPanelIndexByLevel(leafManager.Level);
        var posToScrollArea = scrollArea.GetColumnLocalPosition(8-col);
        return posToScrollArea + pos;
    }


    IEnumerator ScrollToBottom()
    {
        OnPause();
        yield return new WaitForSeconds(0.5f);
        scrollArea.CenterTheTargetColumn(8f-0.3f, 4f, Ease.InOutExpo);
        yield return new WaitForSeconds(2f);
        OnResume();
    }

    IEnumerator FrogJumpToLeaf2(int targetLevel, float timeScale = 1.5f)
    {
        OnPause();
        if (!FrogParent.gameObject.activeSelf)
        {
            scrollArea.scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            scrollArea.CenterTheTargetColumn(8f - GetPanelIndexFloatByLevel(_curLevel), 0.2f);
            yield return new WaitForSeconds(0.3f);
            scrollArea.scrollRect.onValueChanged.RemoveAllListeners();
        }

        GameManager.Sound.PlayAudio(Random.Range(0, 2) == 0
            ? SoundType.AnimalFrogCaliforniaTreeFrogChirp01.ToString()
            : SoundType.AnimalFrogCaliforniaTreeFrogChirp02.ToString());
        while (_curLevel < targetLevel)
        {
            if (FrogLeafs.TryGetValue(_curLevel + 1, out var tmpLeaf))
            {
                if (FrogSpine.AnimationState == null)
                {
                    FrogSpine.Initialize(true);
                }

                var anim1 = FrogSpine.AnimationState.SetAnimation(1, "active", false);
                anim1.TimeScale = timeScale;
                var anim2 = FrogParent.DOLocalMove(GetLeafLocalPosition(tmpLeaf), anim1.Animation.Duration / timeScale)
                    .SetEase(Ease.InOutSine);
                scrollArea.CenterTheTargetColumn(8f - GetPanelIndexFloatByLevel(_curLevel + 1),
                    anim1.Animation.Duration / timeScale);

                yield return new DOTweenCYInstruction.WaitForCompletion(anim2);
                tmpLeaf.ShowActiveAnim();
                FrogSpine.AnimationState.SetAnimation(1, "idle", true);
                _curLevel++;
                SetFrogState(_curLevel);
                RefreshLeafs();
            }
            else
            {
                break;
            }
        }

        yield return new WaitForSeconds(0.2f);
        _isPurchase = false;
        if (_curLevel == GameManager.PlayerData.MaxFrogJumpLevel)
        {
            OnResume();
            OnClose();
            (GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager)?.SetFrogJumpEntrance();
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
        }
        else
        {
            // RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, OnResume);
            var handle = UnityUtility.InstantiateAsync("ObjectiveRewardGetTip", FrogParent, obj =>
            {
                ItemSlot slot = obj.GetComponent<ItemSlot>();
                slot.OnInit(_rewardData.Key, _rewardData.Value);

                Transform cachedTrans = obj.transform;
                cachedTrans.localScale = Vector3.zero;
                cachedTrans.position = FrogParent.position;
                obj.GetComponent<CanvasGroup>().alpha = 1;
                obj.gameObject.SetActive(true);
                
                cachedTrans.DOScale(1, 0.2f);
                cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 1f).SetEase(Ease.InSine);
                obj.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetDelay(0.6f).onComplete = () =>
                {
                    slot.OnRelease();
                    UnityUtility.UnloadInstance(obj.gameObject);
                };
                if (_rewardData.Key == TotalItemData.CardPack1 ||
                    _rewardData.Key == TotalItemData.CardPack2 ||
                    _rewardData.Key == TotalItemData.CardPack3 ||
                    _rewardData.Key == TotalItemData.CardPack4 ||
                    _rewardData.Key == TotalItemData.CardPack5)
                {
                    GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                    {
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true,
                            () =>
                            {
                                RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                                OnResume();
                            });
                    });
                }
                else 
                    OnResume();
            });
            handles.Add(handle);
        }
    }

    public override void OnPause()
    {
        // scrollArea.scrollRect.onValueChanged.RemoveAllListeners();
        scrollArea.scrollRect.vertical = false;
        Close_Btn.interactable = false;
        Ads_Btn.interactable = false;
        Purchase_Btn.interactable = false;
        base.OnPause();

        GameManager.Task.AddDelayTriggerTask(3f, () =>
        {
            OnResume();
        });
    }

    public override void OnResume()
    {
        // scrollArea.scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        if(scrollArea)scrollArea.scrollRect.vertical = true;
        if(Close_Btn)Close_Btn.interactable = true;
        if(Ads_Btn)Ads_Btn.interactable = true;
        if(Purchase_Btn)Purchase_Btn.interactable = true;
        base.OnResume();
    }

    public override void OnRelease()
    {
        foreach (var handle in handles)
        {
            UnityUtility.UnloadAssetAsync(handle);
        }
        handles.Clear();
        scrollArea.OnRelease();
        GameManager.ObjectPool.DestroyObjectPool("FrogJumpColumnPool");
        base.OnRelease();
    }

    private void RefreshLeafs()
    {
        foreach (var leaf in FrogLeafs)
        {
            leaf.Value.Refresh(_curLevel);
        }
    }

    private void OnCommonEvent(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = e as CommonEventArgs;
        if (ReferenceEquals(ne, null)) return;
        var level = -1;
        switch (ne.Type)
        {
            case CommonEventType.FrogLeafCreated:
                var leaf = ne.UserDatas[0] as FrogLeafManager;
                level = (int)ne.UserDatas[1];
                FrogLeafs.Add(level, leaf);
                if (level == _curLevel && _isFirst)
                {
                    FrogParent.position = leaf.transform.position;
                    _isFirst = false;
                }
                break;
            case CommonEventType.FrogLeafDestroy:
                level = (int)ne.UserDatas[0];
                FrogLeafs.Remove(level);
                break;
        }
    }


    private void OnRewardAdLoaded(object sender, GameEventArgs e)
    {
        if (DateTime.Now > GameManager.PlayerData.NextFrogAdReadyTime) SetAdButtonState();
    }

    private KeyValuePair<TotalItemData,int> _rewardData;
    private void OnRewardAdEarnedReward(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ReferenceEquals(ne, null)) return;
        if (ne.UserData.ToString() != "FrogJump") return;
        
        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Frog_ad_AD,new Parameter("level",_curLevel));
        GameManager.PlayerData.NextFrogAdReadyTime = DateTime.Now.AddMinutes(2);
        // GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.FrogAdStateChange));
        SetAdButtonState();
        GameManager.PlayerData.CurFrogJumpLevel = _curLevel + 1;
        _rewardData = GameManager.PlayerData.FrogJumpData.GetReward(_curLevel + 1);
        if (_rewardData.Key == TotalItemData.CardPack1 ||
            _rewardData.Key == TotalItemData.CardPack2 ||
            _rewardData.Key == TotalItemData.CardPack3 ||
            _rewardData.Key == TotalItemData.CardPack4 ||
            _rewardData.Key == TotalItemData.CardPack5)
        {
            RewardManager.Instance.AddNeedGetReward(_rewardData.Key, _rewardData.Value);
        }
        else
        {
            RewardManager.Instance.SaveRewardData(_rewardData.Key, _rewardData.Value, true);
        }

        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            StartCoroutine(FrogJumpToLeaf2(_curLevel + 1,1f));
        });
    }
    
    private float _lastValue;

    private void OnScrollValueChanged(Vector2 arg0)
    {
        if (FrogParent.position.y > topAnchor.position.y|| FrogParent.position.y < bottomAnchor.position.y)
        {
            FrogParent.gameObject.SetActive(false);
        }
        else
        {
            FrogParent.gameObject.SetActive(true);
        }
    }

    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        OnClose();
    }
}