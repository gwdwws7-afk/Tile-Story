using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public abstract class OnePlusOnePackMenuBase : PopupMenuForm
{
    public TextMeshProUGUILocalize buyPrice;
    public DelayButton buyButton;
    public GameObject tick;
    public DelayButton freeButton;
    public SkeletonGraphic lockSpine;
    public DelayButton closeButton;
    public ClockBar clockBar;
    public GameObject lowerProduct;
    public GameObject higherProduct;
    public Transform[] AnimationTrans;
    
    protected abstract string PackName { get; }
    protected abstract ProductNameType LowerProductType { get; }
    protected abstract ProductNameType HigherProductType { get; }
    protected abstract DateTime StartTime { get; }
    protected abstract DateTime EndTime { get; }
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        if (!GameManager.Ads.IsRemovePopupAds || GameManager.PlayerData.OnePlusOnePackType == $"{PackName},{LowerProductType}")
        {
            string price = GameManager.Purchase.GetPrice(LowerProductType);
            if (!string.IsNullOrEmpty(price))
                buyPrice.SetTerm(price);
            buyButton.OnInit(()=>OnBuyButtonClicked(LowerProductType));
            freeButton.OnInit(()=>OnFreeButtonClicked(LowerProductType));
            lowerProduct.SetActive(true);
            higherProduct.SetActive(false);
        }
        else
        {
            string price = GameManager.Purchase.GetPrice(HigherProductType);
            if (!string.IsNullOrEmpty(price))
                buyPrice.SetTerm(price);
            buyButton.OnInit(()=>OnBuyButtonClicked(HigherProductType));
            freeButton.OnInit(()=>OnFreeButtonClicked(HigherProductType));
            lowerProduct.SetActive(false);
            higherProduct.SetActive(true);
        }
        closeButton.OnInit(OnClose);

        if (DateTime.Now > StartTime && DateTime.Now < EndTime)
        {
            clockBar.OnReset();
            clockBar.StartCountdown(EndTime);
            clockBar.CountdownOver += OnCountdownOver;
        }
        
        if (GameManager.PlayerData.OnePlusOnePackType == string.Empty)
        {
            buyButton.gameObject.SetActive(true);
            tick.SetActive(false);
            lockSpine.gameObject.SetActive(true);
        }
        else
        {
            buyButton.gameObject.SetActive(false);
            tick.SetActive(true);
            lockSpine.gameObject.SetActive(false);
        }
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        buyButton.OnReset();
        freeButton.OnReset();
        closeButton.OnReset();
        clockBar.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        
        gameObject.SetActive(true);
        StartCoroutine(IdleAnimation());
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
        
        gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
    }

    private void OnBuyButtonClicked(ProductNameType productType)
    {
        GameManager.Purchase.BuyProduct(productType, () =>
        { 
            OnHide();
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, ()=>
            {
                OnShow();
                buyButton.gameObject.SetActive(false);
                tick.SetActive(true);
                lockSpine.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
                {
                    lockSpine.gameObject.SetActive(false);
                };
            });
            GameManager.PlayerData.OnePlusOnePackType = $"{PackName},{productType}";
        });
    }

    private void OnFreeButtonClicked(ProductNameType productType)
    {
        if (GameManager.PlayerData.OnePlusOnePackType == $"{PackName},{productType}")
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.AddNeedGetReward(productType);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                GameManager.UI.ShowUIForm(PackName);
            });
            GameManager.PlayerData.OnePlusOnePackType = String.Empty;
        }
        else
        {
            // GameManager.UI.ShowWeakHint("Shop.OnePlusOneFreeTip", Vector3.zero);
            // lockSpine.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
            OnBuyButtonClicked(productType);
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.UI.HideUIForm(this);
        
        if (GameManager.Process.Count > 0)
            return;
        
        if (GameManager.PlayerData.OnePlusOnePackType != string.Empty)
        {
            string[] strArray = GameManager.PlayerData.OnePlusOnePackType.Split(',');
            ProductNameType productType = (ProductNameType)Enum.Parse(typeof(ProductNameType), strArray[1]);
            RewardManager.Instance.AddNeedGetReward(productType);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TilePassRewardPanel, false, null);
            GameManager.PlayerData.OnePlusOnePackType = String.Empty;
        }
    }
    
    IEnumerator IdleAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < AnimationTrans.Length; i++)
            {
                AnimationTrans[i].DOScale(1.2f, 0.15f).SetLoops(2, LoopType.Yoyo);
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(2f);
        }
    }
}
