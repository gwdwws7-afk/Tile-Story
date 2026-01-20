using System;
using Firebase.Analytics;
using GameFramework.Event;
using TMPro;
using UnityEngine;

public class HarvestKitchenGameLoseMenu : UIForm
{
    public GameObject tapToContinue;
    public DelayButton retryButton, bgButton, adsButton;
    public TextMeshProUGUI consumeChefHatNumText;
    public BasketBar basketBar;

    private bool isClick = true;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        isClick = false;
        tapToContinue.SetActive(false);
        consumeChefHatNumText.text = $"-{HarvestKitchenManager.Instance.GetCurrentTaskConsumeBasketNum()}";

        retryButton.SetBtnEvent(OnClickRetryBtn);
        bgButton.SetBtnEvent(OnClickBgBtn);
        adsButton.SetBtnEvent(OnClickAdsButton);
        retryButton.interactable = true;
        bgButton.interactable = true;

        bool canOpen = HarvestKitchenManager.Instance.CanOpenGame();
        bool hasAds = GameManager.Ads.CheckRewardedAdIsLoaded() && !GameManager.DataNode.GetData("HarvestKitchenShowedRV", false);
        retryButton.gameObject.SetActive(canOpen);
        adsButton.gameObject.SetActive(!canOpen && hasAds);
        
        GameManager.DataNode.SetData<int>(HarvestKitchenManager.ENTER_MAIN_MENU_TYPE, 2);
        
        basketBar.Refresh();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        
        GameManager.Sound.StopMusic(0);
        GameManager.Sound.PlayUISound(SoundType.SFX_Kitchen_Match_Level_Failed.ToString());
        
        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            tapToContinue.SetActive(true);
            isClick = true;
        });
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        base.OnRelease();
    }

    public void OnClickRetryBtn()
    {
        if (HarvestKitchenManager.Instance.UseBasketNumOpenLevel())
        {
            retryButton.interactable = false;
            GameManager.Ads.ShowInterstitialAd(() =>
            {
                HarvestKitchenManager.Instance.ResetTemporaryData();

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Retry,
                    new Parameter("stage", HarvestKitchenManager.Instance.TaskId));

                GameManager.UI.HideUIForm(this);
                GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
                GameManager.UI.ShowUIForm("HarvestKitchenGameMenu");
            });
        }
    }

    public void OnClickBgBtn()
    {
        if (!isClick) return;
        bgButton.interactable = false;

        GameManager.Ads.ShowInterstitialAd(() =>
        {
            HarvestKitchenManager.Instance.ResetTemporaryData();
            
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
            GameManager.UI.ShowUIForm("HarvestKitchenMainMenu");
        });
    }

    public void OnClickAdsButton()
    {
        GameManager.DataNode.SetData("HarvestKitchenShowedRV", true);
        GameManager.Ads.ShowRewardedAd("KitchenGameRetryLevel");
    }
    
    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "KitchenGameRetryLevel") 
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Retry_ad);
            HarvestKitchenManager.Instance.ResetTemporaryData();
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
            GameManager.UI.ShowUIForm("HarvestKitchenGameMenu");
        }
    }
}
