using System;
using Firebase.Analytics;
using GameFramework.Event;
using TMPro;
using UnityEngine;

public class KitchenGameWinMenu : PopupMenuForm
{
    // public Image slider;
    public TextMeshProUGUI praiseNumText, consumeChefHatNumText;//, sliderProgressText
    // public Transform praiseTrans;
    public DelayButton okButton, adsButton, chefHatButton;

    // public List<CanvasGroup> addOneList;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        
        okButton.SetBtnEvent(OnOkBtnClick);
        adsButton.SetBtnEvent(OnAdsBtnClick);
        chefHatButton.SetBtnEvent(OnChefHatBtnClick);

        int targetNum = KitchenManager.Instance.GetCurrentTaskDatas().TargetPraise;
        int currentPraise = KitchenManager.Instance.PraiseNum;

        // slider.fillAmount = (float)currentPraise / targetNum;
        // sliderProgressText.text = $"{currentPraise} / {targetNum}";
        
        praiseNumText.text = $"x {KitchenManager.Instance.GetAddPraise()}";
        consumeChefHatNumText.text = $"-{KitchenManager.Instance.GetCurrentTaskConsumeChefHatNum()}";

        if (currentPraise < targetNum)
        {
            // 判断是否有足够的厨师帽
            bool canOpen = KitchenManager.Instance.CanOpenGame();
            bool hasAds = GameManager.Ads.CheckRewardAdCanShow() && GameManager.Ads.CheckRewardedAdIsLoaded();
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
        
        GameManager.DataNode.SetData<int>(KitchenManager.ENTER_MAIN_MENU_TYPE, 1);
        base.OnInit(uiGroup, completeAction, userData);
    }

    // private IEnumerator addOneAnim = null;
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        GameManager.Sound.StopMusic(0);
        GameManager.Sound.PlayUISound(SoundType.SFX_Kitchen_Match_Level_Success.ToString());
        
        // if (addOneAnim == null)
        // {
        //     addOneAnim = PlayAddOneAnim();
        // }
        // else
        // {
        //     StopAnim();
        //     addOneAnim = PlayAddOneAnim();
        // }
        // StartCoroutine(addOneAnim);
    }

    public override void OnRelease()
    {
        // StopAnim();
        
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        base.OnRelease();
    }

    public void OnOkBtnClick()
    {
        GameManager.Ads.ShowInterstitialAd(() =>
        {
            KitchenManager.Instance.ResetTemporaryData();
            
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("KitchenGameMenu");
            GameManager.UI.ShowUIForm("KitchenMainMenu");
        });
    }

    public void OnAdsBtnClick()
    {
        GameManager.Ads.ShowRewardedAd("KitchenGameNextLevel");
    }

    public void OnChefHatBtnClick()
    {
        if (KitchenManager.Instance.UseChefNumOpenLevel())
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Next_Level,
                new Parameter("type", 1));

            GameManager.Ads.ShowInterstitialAd(() =>
            {
                KitchenManager.Instance.ResetTemporaryData();
                
                GameManager.UI.HideUIForm(this);
                GameManager.UI.HideUIForm("KitchenGameMenu");
                GameManager.UI.ShowUIForm("KitchenGameMenu");
            });
        }
    }

    // void StopAnim()
    // {
    //     if(addOneAnim != null)
    //         StopCoroutine(addOneAnim);
    //     for (int i = 0; i < addOneList.Count; i++)
    //     {
    //         addOneList[i].gameObject.SetActive(false);
    //     }
    // }
    
    // IEnumerator PlayAddOneAnim()
    // {
    //     int i = 0;
    //     int target = KitchenManager.Instance.GetAddPraise();
    //     while (i < target)
    //     {
    //         CanvasGroup cg = addOneList[i % 4];
    //         cg.alpha = 1;
    //         cg.transform.localScale = Vector3.one;
    //         cg.transform.localPosition = Vector3.zero;
    //         cg.transform.SetAsLastSibling();
    //         cg.gameObject.SetActive(true);
    //         Sequence sq = DOTween.Sequence();
    //         sq.Append(cg.DOFade(0, 0.5f));
    //         sq.Join(cg.transform.DOScale(0.5f, 0.5f));
    //         sq.Join(cg.transform.DOLocalMoveY(60, 0.5f));
    //         praiseTrans.DOKill();
    //         praiseTrans.DOPunchScale(Vector3.one * 0.33f, 0.1f, 1, 0f);
    //         yield return new WaitForSeconds(0.15f);
    //         i++;
    //     }
    //     yield break;
    // }
    
    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
        if (ne != null && ne.UserData.ToString() == "KitchenGameNextLevel") 
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Next_Level,
                new Parameter("type", 2));
            
            KitchenManager.Instance.ResetTemporaryData();
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("KitchenGameMenu");
            GameManager.UI.ShowUIForm("KitchenGameMenu");
        }
    }
}
