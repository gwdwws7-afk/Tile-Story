using System;
using Firebase.Analytics;
using GameFramework.Event;
using TMPro;
using UnityEngine;

public class KitchenGameLoseMenu : UIForm
{
    // public Image slider;
    // public TextMeshProUGUI processText;
    public GameObject tapToContinue;
    public DelayButton retryButton, bgButton, adsButton;
    public TextMeshProUGUI consumeChefHatNumText;

    private bool isClick = true;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        isClick = false;
        tapToContinue.SetActive(false);
        // processText.text = $"{KitchenManager.Instance.PraiseNum}/{KitchenManager.Instance.GetCurrentTaskDatas().TargetPraise}";
        // slider.fillAmount = KitchenManager.Instance.GetCurrentTaskProgress();
        consumeChefHatNumText.text = $"-{KitchenManager.Instance.GetCurrentTaskConsumeChefHatNum()}";

        retryButton.SetBtnEvent(OnClickRetryBtn);
        bgButton.SetBtnEvent(OnClickBgBtn);
        adsButton.SetBtnEvent(OnClickAdsButton);
        retryButton.interactable = true;
        bgButton.interactable = true;

        bool canOpen = KitchenManager.Instance.CanOpenGame();
        bool hasAds = GameManager.Ads.CheckRewardAdCanShow() && GameManager.Ads.CheckRewardedAdIsLoaded();
        retryButton.gameObject.SetActive(canOpen);
        adsButton.gameObject.SetActive(!canOpen && hasAds);
        
        GameManager.DataNode.SetData<int>(KitchenManager.ENTER_MAIN_MENU_TYPE, 2);
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
        if (KitchenManager.Instance.UseChefNumOpenLevel())
        {
            retryButton.interactable = false;
            GameManager.Ads.ShowInterstitialAd(() =>
            {
                KitchenManager.Instance.ResetTemporaryData();
                
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Retry,
                    new Parameter("stage", KitchenManager.Instance.TaskId));

                GameManager.UI.HideUIForm(this);
                GameManager.UI.HideUIForm("KitchenGameMenu");
                GameManager.UI.ShowUIForm("KitchenGameMenu");
            });
        }
    }

    public void OnClickBgBtn()
    {
        if (!isClick) return;
        bgButton.interactable = false;

        GameManager.Ads.ShowInterstitialAd(() =>
        {
            KitchenManager.Instance.ResetTemporaryData();
            
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("KitchenGameMenu");
            GameManager.UI.ShowUIForm("KitchenMainMenu");
        });
    }

    public void OnClickAdsButton()
    {
        GameManager.Ads.ShowRewardedAd("KitchenGameRetryLevel");
    }
    
    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "KitchenGameRetryLevel") 
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Retry_ad);
            KitchenManager.Instance.ResetTemporaryData();
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("KitchenGameMenu");
            GameManager.UI.ShowUIForm("KitchenGameMenu");
        }
    }
}
