using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WatchAdsCoinEntrance : EntranceUIForm
{
    public TextMeshProUGUI DaliyWatchCount_Text;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        gameObject.SetActive(false);
        return;

        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, RewardAdEarned);

        bool isShowDaliyWatchAdsBtn = GameManager.PlayerData.IsCanDaliyWatchAds && (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockDailyWatchAdsLevel);

        gameObject.SetActive(isShowDaliyWatchAdsBtn);

        DaliyWatchCount_Text.text = $"{GameManager.PlayerData.DaliyWatchAdsCountByToday}/5";
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, RewardAdEarned);

        base.OnRelease();
    }

    public override void OnButtonClick()
    {
        if (!GameManager.Ads.CheckRewardedAdIsLoaded())
        {
            GameManager.UI.ShowWeakHint("Common.Ad is still loading", Vector3.zero);
        }
        else
        {
            if (GameManager.PlayerData.IsShowWatchAdsPanel)
            {
                GameManager.UI.ShowUIForm("WatchDaliyCoinAdsPanel");
            }
            else
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Daliy_Ad_Coin_Click);
                GameManager.Ads.ShowRewardedAd("DaliyWatchAds_Btn");
            }
        }
    }

    private void RewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        try
        {
            DaliyWatchCount_Text.text = $"{GameManager.PlayerData.DaliyWatchAdsCountByToday}/5";
            if (GameManager.PlayerData.DaliyWatchAdsCountByToday <= 0)
            {
                gameObject.SetActive(false);
            }
        }
        catch { }

        if (ne.UserData.Equals("DaliyWatchAds_Btn"))
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Daliy_Ad_Coin_Sucess);
            GameManager.PlayerData.RecordDaliyWatchAdsByToday();

            RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin, 100);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null, null);
        }
    }
}
