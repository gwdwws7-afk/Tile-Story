using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using GameFramework.Event;
using UnityEngine.UI;

public class DaliyWatchAdsPrefab : MonoBehaviour
{
    [SerializeField]
    private DelayButton DaliyDelay_Btn;
    [SerializeField]
    private GameObject FreeRoot;
    [SerializeField]
    private Graphic Finger;
    private bool isActiveAds = false;

    private bool isFree = false;
    private void Awake()
    {
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, RewardLoad);
    }

    private void OnDestroy()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, RewardLoad);
    }

    Sequence Sequence = null;
    public void PlayFingerAnim(bool isPlay = true, bool isOnce = true)
    {
        if (SystemInfoManager.IsSuperLowMemorySize) return;
        if (Finger == null) return;
        if (isPlay)
        {
            if (Sequence != null && Sequence.IsPlaying() && Finger.gameObject.activeInHierarchy) return;

            Finger.gameObject.SetActive(true);
            Finger.color = new Color(1, 1, 1, 0f);
            Sequence = DOTween.Sequence()
                .Append(Finger.DOFade(1, 0.5f))
                .AppendInterval(3f)
                .Append(Finger.DOFade(0, 0.5f))
                .AppendInterval(2f).OnKill(() => Sequence = null);
            if (!isOnce)
                Sequence.SetLoops(-1, LoopType.Restart);
        }
        else
        {
            Sequence.Kill(true);
            Finger.gameObject.SetActive(false);
        }
    }

    public void SetActive(bool isActive)
    {
        isActiveAds = isActive;
       
        if (isActive)
        {
            this.isFree = GameManager.PlayerData.NeedShowDaliyWatchAdsGuide;
            FreeRoot.gameObject.SetActive(this.isFree);

            if (this.isFree)
            {
                PlayFingerAnim(isOnce: true);
            }

            if (GameManager.Ads.CheckRewardedAdIsLoaded())
            {
                if (!gameObject.activeInHierarchy)
                {
                    transform.transform.localScale = Vector3.zero;
                    gameObject.SetActive(isActive);
                    transform.DOKill(true);
                    transform.DOScale(1,0.4f).SetEase(Ease.OutBack);
                }

                Init(this.isFree);
            }
        }
        else
        {
            gameObject.SetActive(isActive);
        }
    }

    private void Init(bool isFree)
    {
        DaliyDelay_Btn.SetBtnEvent(() =>
        {
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.PauseLevelTime));
            if (isFree)
            {
                var tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
                tileMatchPanel.RecordWatchAds();
                GameManager.PlayerData.RecordShowDaliyWatchAdsGuide();
                ShowDaliyWatchAdsPanel();
            }
            else
            {
                var tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
                if (tileMatchPanel == null || tileMatchPanel.IsGameWin || tileMatchPanel.IsGameLose) 
                    return;

                GameManager.PlayerData.TodayWatchPropsAdTime += 1;

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Ads_For_Props, new Firebase.Analytics.Parameter("Level", GameManager.PlayerData.NowLevel));
                GameManager.Ads.ShowRewardedAd("DaliyWatchAdsPrefab");
            }
        });
    }
    
    private void RewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne != null && ne.UserData != null && ne.UserData.Equals("DaliyWatchAdsPrefab")) 
        {
            var tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (tileMatchPanel == null)
                return;
            tileMatchPanel.RecordWatchAds();

            ShowDaliyWatchAdsPanel();

            GameManager.Ads.SetRewardAdColdingTime();
        }
    }

    private void ShowDaliyWatchAdsPanel()
    {
        SetActive(false);
        GameManager.UI.ShowUIForm("DaliyWatchAdsPanel");
    }

    private void RewardLoad(object sender, GameEventArgs e)
    {
        if (isActiveAds)
        {
            gameObject.SetActive(true);
        }
    }
}
