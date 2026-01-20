using GameFramework.Event;
using System;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 转盘入口按钮
/// </summary>
public sealed class TurntableEntrance : EntranceUIForm
{
    public CountdownTimer timer;

    // public SimpleSlider slider;
    // public TextMeshProUGUILocalize lockText;
    public Image bodyImg;

    // public Image bannerImg;
    public GameObject warningSign;

    public GameObject loadingBanner, spinBanner;

    private long adsSpinLimitTime;
    private int showWarningSignTaskId;
    private Sequence sequence;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GameManager.Event.Subscribe(TurntableAdsStateChangedEventArgs.EventId, OnTurntableStateChanged);
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnRewardAdStateChanged);

        adsSpinLimitTime = GameManager.Firebase.GetLong(Constant.RemoteConfig.SpinAdTimesLimit, Constant.GameConfig.AdsSpinLimitTime);

        SetTimer();
    }

    public override void OnRelease()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
        bodyImg.transform.DOKill();
        GameManager.Event.Unsubscribe(TurntableAdsStateChangedEventArgs.EventId, OnTurntableStateChanged);
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnRewardAdStateChanged);

        GameManager.Task.RemoveDelayTriggerTask(showWarningSignTaskId);
        showWarningSignTaskId = 0;

        base.OnRelease();
    }

    private void OnRewardAdStateChanged(object sender, GameEventArgs e)
    {
        if (!(e is CommonEventArgs { Type: CommonEventType.RewardAdsShownEvent } ne))
        {
            return;
        }
        SetTimer();
    }

    private void OnRewardAdLoadComplete(object sender, GameEventArgs e)
    {
        SetTimer();
    }

    private void OnTurntableStateChanged(object sender, GameEventArgs e)
    {
        SetTimer();
    }

    public void StartRotate()
    {
        if (sequence == null)
        {
            bodyImg.transform.localRotation = Quaternion.identity;
            sequence = DOTween.Sequence();
            sequence.AppendInterval(2f);
            sequence.Append(bodyImg.transform.DOLocalRotate(new Vector3(0, 0, -1080f), 3f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            sequence.SetLoops(-1, LoopType.Restart);
        }

        //bodyImg.transform.DOLocalRotate(new Vector3(0, 0, -1080f), 3f, RotateMode.FastBeyond360)
        //    .SetLoops(-1, LoopType.Restart).SetEase(Ease.OutCubic).SetDelay(5f, true);
    }

    private void SetTimer()
    {
        timer.OnReset();

        if (GameManager.PlayerData.IsFirstFreeTurnByDaliy)
        {
            timer.gameObject.SetActive(false);
            spinBanner.gameObject.SetActive(true);
            loadingBanner.gameObject.SetActive(false);
            SetWarningSign(false);
            return;
        }

        if (GameManager.PlayerData.NormalTurntableAdsSpinTime < adsSpinLimitTime)
        {
            if(DateTime.Now < GameManager.PlayerData.TurntableNextAdsSpinReadyTime)
            {
                _isCountDownOver = false;
                timer.StartCountdown(GameManager.PlayerData.TurntableNextAdsSpinReadyTime);
                timer.CountdownOver += OnCountdownOver;
                timer.gameObject.SetActive(true);
                spinBanner.gameObject.SetActive(false);
                loadingBanner.gameObject.SetActive(false);
                SetWarningSign(false);
            }
            else
            {
                timer.gameObject.SetActive(false);
                bool adsLoaded = GameManager.Ads.CheckRewardedAdIsLoaded();
                spinBanner.gameObject.SetActive(adsLoaded);
                loadingBanner.gameObject.SetActive(!adsLoaded);
                SetWarningSign(adsLoaded);
            }
        }
        else
        {
            timer.gameObject.SetActive(false);
            spinBanner.gameObject.SetActive(true);
            loadingBanner.gameObject.SetActive(false);
            SetWarningSign(false);
        }
    }

    private bool _isCountDownOver;
    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        if (_isCountDownOver) return;
        _isCountDownOver = true;
        timer.gameObject.SetActive(false);
        bool adsLoaded = GameManager.Ads.CheckRewardedAdIsLoaded();
        spinBanner.gameObject.SetActive(adsLoaded);
        loadingBanner.gameObject.SetActive(!adsLoaded);
        SetWarningSign(adsLoaded);
    }

    private void SetWarningSign(bool isShow)
    {
        isShow = isShow || GameManager.PlayerData.IsFirstFreeTurnByDaliy;
        //将不同按钮警示动画错开
        if (isShow && !warningSign.gameObject.activeSelf && showWarningSignTaskId == 0) 
        {
            showWarningSignTaskId = GameManager.Task.AddDelayTriggerTask(UnityEngine.Random.Range(0.5f, 1f), () =>
                {
                    showWarningSignTaskId = 0;
                    warningSign.gameObject.SetActive(true);
                });
        }

        if (!isShow && (warningSign.gameObject.activeSelf || showWarningSignTaskId != 0)) 
        {
            GameManager.Task.RemoveDelayTriggerTask(showWarningSignTaskId);
            showWarningSignTaskId = 0;
            warningSign.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        timer.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("NormalTurntable");
    }
}