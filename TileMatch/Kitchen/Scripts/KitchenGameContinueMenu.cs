using System;
using Firebase.Analytics;
using TMPro;
using UnityEngine;
using GameFramework.Event;

public class KitchenGameContinueMenu : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public TextMeshProUGUI curPraiseNum, totalPraiseNum, needCoinText;
    public TextMeshProUGUILocalize titleText;
    public DelayButton buyBtn, watchAdsBtn, giveUpBtn, closeBtn, continueBtn, bgBtn;
    public GameObject menuObj;
    public GameObject tipText1, tipText2;

    private int needCoin = 0;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        needCoin = KitchenManager.Instance.GetContinueNeedCoin();
        
        buyBtn.SetBtnEvent(OnClickBuyBtn);
        watchAdsBtn.SetBtnEvent(OnClickWatchAdsBtn);
        giveUpBtn.SetBtnEvent(OnClickGiveUpBtn);
        closeBtn.SetBtnEvent(OnClickGiveUpBtn);
        continueBtn.SetBtnEvent(OnClickContinueBtn);
        bgBtn.SetBtnEvent(OnClickBgBtn);

        tipText1.SetActive(true);
        tipText2.SetActive(false);

        switch (KitchenManager.Instance.toContinueMenuType)
        {
            case 0:
                GameManager.Sound.StopMusic(0.1f);
                delayId = GameManager.Task.AddDelayTriggerTask(2.5f, () =>
                {
                    //播放背景音乐
                    GameManager.Sound.PlayMusic(SoundType.SFX_Kitchen_Match_Level_BGM.ToString());
                    delayId = -1;
                });
                GameManager.Sound.PlayUISound(SoundType.SFX_Level_Fail_Lose_Lives.ToString());
                
                buyBtn.gameObject.SetActive(true);
                continueBtn.gameObject.SetActive(false);
                titleText.SetTerm("Settings.Are You Sure?");
                
                if (KitchenManager.Instance.canWatchAdsContinue && GameManager.Ads.CheckRewardedAdIsLoaded() &&
                    GameManager.Ads.CheckRewardAdCanShow())
                {
                    watchAdsBtn.gameObject.SetActive(true);
                    giveUpBtn.gameObject.SetActive(false);
                    closeBtn.gameObject.SetActive(true);
                }
                else
                {
                    watchAdsBtn.gameObject.SetActive(false);
                    giveUpBtn.gameObject.SetActive(true);
                    closeBtn.gameObject.SetActive(false);
                }
                break;
            case 1:
                GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
                
                buyBtn.gameObject.SetActive(false);
                continueBtn.gameObject.SetActive(true);
                watchAdsBtn.gameObject.SetActive(false);
                giveUpBtn.gameObject.SetActive(true);
                closeBtn.gameObject.SetActive(false);
                titleText.SetTerm("Settings.Are You Sure?");
                break;
            default:
                break;
        }

        needCoinText.text = needCoin.ToString();
        
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnShopBuyGetRewardComplete);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    private int delayId = -1;
    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnShopBuyGetRewardComplete);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

        if (delayId >= 0)
        {
            GameManager.Task.RemoveDelayTriggerTask(delayId);
            delayId = -1;
            GameManager.Sound.PlayMusic(SoundType.SFX_Kitchen_Match_Level_BGM.ToString());
        }
        base.OnRelease();
    }

    public void OnClickBuyBtn()
    {
        // 修改接关消耗
        if (GameManager.PlayerData.UseItem(TotalItemData.Coin, needCoin))
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Choose_Continue,
                new Parameter("type", 1));
            // 增加接关次数
            KitchenManager.Instance.continueOfCoinNum += 1;
            GameManager.UI.HideUIForm(this);
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenLoseContinue));
        }
        else
        {
            GameManager.DataNode.SetData("KitchenFailLackOfCoin", true);
            GameManager.UI.ShowUIForm("ShopMenuManager", userData: true);
        }
    }

    public void OnClickWatchAdsBtn()
    {
        KitchenManager.Instance.canWatchAdsContinue = false;
        GameManager.Ads.ShowRewardedAd("KitchenGameContinueMenu");
        watchAdsBtn.gameObject.SetActive(false);
        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Choose_Continue,
            new Parameter("type", 2));
    }
    
    public void OnClickGiveUpBtn()
    {
        // 记录关卡失败
        KitchenManager.Instance.ActivityLevelLose();
        
        GameManager.UI.HideUIForm(this);
        GameManager.UI.ShowUIForm("KitchenGameLoseMenu");
    }

    public void OnClickContinueBtn()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenContinue));
    }

    public void OnClickBgBtn()
    {
        menuObj.SetActive(!menuObj.activeSelf);
        tipText1.SetActive(menuObj.activeSelf);
        tipText2.SetActive(!menuObj.activeSelf);
    }
    
    public void SetLisks(int cur, int total)
    {
        curPraiseNum.text = cur.ToString();
        totalPraiseNum.text = total.ToString();

        if (needCoin == 900 && KitchenManager.Instance.canWatchAdsContinue)
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Show_Continue, new []
            {
                new Parameter("stage", KitchenManager.Instance.TaskId), 
                new Parameter("num", total - cur)
            });
        }
    }
    
    private void OnShopBuyGetRewardComplete(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;

        if (ne != null && ne.Type == CommonEventType.ShopBuyGetRewardComplete)
        {
            if (GameManager.DataNode.GetData("KitchenFailLackOfCoin", false))
            {
                GameManager.DataNode.SetData("KitchenFailLackOfCoin", false);
                if (GameManager.PlayerData.CoinNum >= needCoin)
                {
                    buyBtn.onClick?.Invoke();
                }
            }
        }
    }
    
    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "KitchenGameContinueMenu") 
        {
            GameManager.UI.HideUIForm(this);
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenLoseContinue));
            GameManager.Ads.SetRewardAdColdingTime();
            KitchenManager.Instance.canWatchAdsContinue = false;
        }
    }

    void ICustomOnEscapeBtnClicked.OnEscapeBtnClicked()
    {
        if (!closeBtn.isActiveAndEnabled || !closeBtn.interactable)
            return;

        OnClickGiveUpBtn();
    }
}
