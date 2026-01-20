using System;
using Firebase.Analytics;
using GameFramework.Event;
using TMPro;
using UnityEngine;

public class HarvestKitchenGameWinMenu : PopupMenuForm
{
    public TextMeshProUGUI praiseNumText, consumeChefHatNumText;
    public DelayButton okButton, adsButton, chefHatButton;
    public BasketBar basketBar;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        okButton.SetBtnEvent(OnOkBtnClick);
        adsButton.SetBtnEvent(OnAdsBtnClick);
        chefHatButton.SetBtnEvent(OnChefHatBtnClick);

        int targetNum = HarvestKitchenManager.Instance.GetCurrentTaskDatas().TargetPraise;
        int currentPraise = HarvestKitchenManager.Instance.PraiseNum;
        
        praiseNumText.text = $"x {HarvestKitchenManager.Instance.GetAddPraise()}";
        consumeChefHatNumText.text = $"-{HarvestKitchenManager.Instance.GetCurrentTaskConsumeBasketNum()}";

        if (currentPraise < targetNum)
        {
            // 判断是否有足够的厨师帽
            bool canOpen = HarvestKitchenManager.Instance.CanOpenGame();
            bool hasAds = GameManager.Ads.CheckRewardedAdIsLoaded() && !GameManager.DataNode.GetData("HarvestKitchenShowedRV", false);
             adsButton.gameObject.SetActive(!canOpen && hasAds);
            chefHatButton.gameObject.SetActive(canOpen);
            if (!(canOpen || hasAds))
                okButton.transform.localPosition = Vector3.up * -620f;
            else
                okButton.transform.localPosition = new Vector3(-266f, -620f, 0);
        }
        else
        {
            adsButton.gameObject.SetActive(false);
            chefHatButton.gameObject.SetActive(false);
            okButton.transform.localPosition = Vector3.up * -620f;
        }
        
        GameManager.DataNode.SetData<int>(HarvestKitchenManager.ENTER_MAIN_MENU_TYPE, 1);
        
        basketBar.Refresh();
        
        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
        
        base.OnInit(uiGroup, completeAction, userData);
    }
    
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        GameManager.Sound.StopMusic(0);
        GameManager.Sound.PlayUISound(SoundType.SFX_Kitchen_Match_Level_Success.ToString());
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        base.OnRelease();
    }

    public void OnOkBtnClick()
    {
        GameManager.Ads.ShowInterstitialAd(() =>
        {
            HarvestKitchenManager.Instance.ResetTemporaryData();
            
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
            GameManager.UI.ShowUIForm("HarvestKitchenMainMenu");
        });
    }

    public void OnAdsBtnClick()
    {
        GameManager.DataNode.SetData("HarvestKitchenShowedRV", true);
        GameManager.Ads.ShowRewardedAd("KitchenGameNextLevel");
    }

    public void OnChefHatBtnClick()
    {
        if (HarvestKitchenManager.Instance.UseBasketNumOpenLevel())
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Next_Level,
                new Parameter("type", 1));
            
            HarvestKitchenManager.Instance.ResetTemporaryData();

            GameManager.Ads.ShowInterstitialAd(() =>
            {
                GameManager.UI.HideUIForm(this);
                GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
                GameManager.UI.ShowUIForm("HarvestKitchenGameMenu");
            });
        }
    }
    
    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "KitchenGameNextLevel") 
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Next_Level,
                new Parameter("type", 2));
            
            HarvestKitchenManager.Instance.ResetTemporaryData();
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("HarvestKitchenGameMenu");
            GameManager.UI.ShowUIForm("HarvestKitchenGameMenu");
        }
    }
}
