using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using UnityEngine;
using TMPro;

public class WatchDaliyCoinAdsPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Ads_Btn,Choose_Btn,Close_Btn;
    [SerializeField] private GameObject Choose_Obj;
    [SerializeField] private TextMeshProUGUI WatchAdsCount_Text;

    private bool isChoose = false;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);

        SetEvent();
        Choose_Obj.SetActive(isChoose);
        WatchAdsCount_Text.text = $"{GameManager.PlayerData.DaliyWatchAdsCountByToday}/5";
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);
        
        base.OnRelease();
    }

    private void SetEvent()
    {
        Ads_Btn.SetBtnEvent(() =>
        {
            if(isChoose)GameManager.PlayerData.RecordIsShowWatchAdsPanel();
            
            GameManager.PlayerData.RecordDaliyWatchAdsByToday();
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Daliy_Ad_Coin_Click);
            GameManager.Ads.ShowRewardedAd("WatchDaliyCoinAdsPanel");
        });
        Choose_Btn.SetBtnEvent(() =>
        {
            isChoose = !isChoose;
            Choose_Obj.gameObject.SetActive(isChoose);
        });
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }

    private void RewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.Equals("WatchDaliyCoinAdsPanel"))
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Daliy_Ad_Coin_Sucess);
            GameManager.UI.HideUIForm(this);
            
            RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin,100);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false,null,null);
        }
    }
}
