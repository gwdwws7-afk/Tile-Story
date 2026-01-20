using System;
using UnityEngine;

public class HarvestKitchenGetMoreMenu : PopupMenuForm
{
    private ProductNameType productType = ProductNameType.Harvest_Kitchen_Package; 
    public DelayButton closeButton, playButton, buyButton;
    public ClockBar clockBar;
    [SerializeField] private TextMeshProUGUILocalize priceText;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButton.SetBtnEvent(OnCloseBtnClick);
        playButton.SetBtnEvent(OnPlayBtnClick);
        buyButton.SetBtnEvent(OnBuyBtnClick);
        
        // 拉取价格
        string price = GameManager.Purchase.GetPrice(productType);
        if (string.IsNullOrEmpty(price))
        {
            priceText.gameObject.SetActive(false);
        }
        else
        {
            priceText.gameObject.SetActive(true);
            priceText.SetTerm(price);
        }
        
        clockBar.StartCountdown(HarvestKitchenManager.Instance.EndTime);
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        clockBar.OnReset();
        
        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public void OnCloseBtnClick()
    {
        GameManager.UI.HideUIForm(this);
    }
    
    public void OnPlayBtnClick()
    {
        GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
        GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI, form =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }
    
    public void OnBuyBtnClick()
    {
        GameManager.Purchase.BuyProduct(productType, () =>
        {
            // 礼包购买成功的回调打点
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Pack_Buy);
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                // 消耗道具，开始关卡
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenBuyPackageComplete));
            });
        }, (fail)=>
        {
            Log.Error($"Kitchen Buy Fail:{fail.ToString()}");
        });
    }
}
