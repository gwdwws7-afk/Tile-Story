using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public abstract class OnePlusTwoPackMenuBase : PopupMenuForm
{
    public TextMeshProUGUILocalize buyPrice;
    public DelayButton buyButton;
    public GameObject tick_1, tick_2, tick_3;
    public DelayButton freeButton_1, freeButton_2;
    public SkeletonGraphic lockSpine_1, lockSpine_2;
    public Transform freeText_1, freeText_2;
    public DelayButton closeButton;
    public ClockBar clockBar;
    public GameObject lowerProduct;
    public GameObject higherProduct;
    public CanvasGroup[] smallCanvasGroups;
    public CanvasGroup[] bigCanvasGroups;

    private bool isSmallPack;

    protected abstract string PackName { get; }
    protected abstract ProductNameType SmallProductType_Buy { get; }
    protected abstract ProductNameType SmallProductType_Free_1 { get; }
    protected abstract ProductNameType SmallProductType_Free_2 { get; }
    protected abstract ProductNameType BigProductType_Buy { get; }
    protected abstract ProductNameType BigProductType_Free_1 { get; }
    protected abstract ProductNameType BigProductType_Free_2 { get; }
    protected abstract DateTime StartTime { get; }
    protected abstract DateTime EndTime { get; }
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        //小礼包购买后是-1，领取左边免费后是-2，领取左边免费后是-3，大礼包反过来
        int getTime = GameManager.PlayerData.OnePlusTwoPackGetRewardTime;
        if (SmallPackExist() || (getTime < 0 && getTime > -4))  
        {
            isSmallPack = true;

            string price = GameManager.Purchase.GetPrice(SmallProductType_Buy);
            if (!string.IsNullOrEmpty(price))
                buyPrice.SetTerm(price);
            buyButton.OnInit(() => OnBuyButtonClicked(SmallProductType_Buy));
            freeButton_1.OnInit(() => OnFree1ButtonClicked(SmallProductType_Free_1));
            freeButton_2.OnInit(() => OnFree2ButtonClicked(SmallProductType_Free_2));
            lowerProduct.SetActive(true);
            higherProduct.SetActive(false);
        }
        else
        {
            isSmallPack = false;

            string price = GameManager.Purchase.GetPrice(BigProductType_Buy);
            if (!string.IsNullOrEmpty(price))
                buyPrice.SetTerm(price);
            buyButton.OnInit(() => OnBuyButtonClicked(BigProductType_Buy));
            freeButton_1.OnInit(() => OnFree1ButtonClicked(BigProductType_Free_1));
            freeButton_2.OnInit(() => OnFree2ButtonClicked(BigProductType_Free_2));
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

        buyButton.gameObject.SetActive(getTime == 0);
        freeButton_1.gameObject.SetActive(getTime != -2 && getTime != 2);
        freeButton_2.gameObject.SetActive(getTime != -3 && getTime != 3);
        tick_1.SetActive(getTime != 0);
        tick_2.SetActive(getTime == -2 || getTime == 2);
        tick_3.SetActive(getTime == -3 || getTime == 3);
        lockSpine_1.gameObject.SetActive(getTime == 0);
        lockSpine_2.gameObject.SetActive(getTime == 0);
        if (getTime == 0)
        {
            freeText_1.localPosition = new Vector3(-48f, 10.4f, 0);
            freeText_2.localPosition = new Vector3(-48f, 10.4f, 0);
        }
        else
        {
            freeText_1.localPosition = new Vector3(0, 10.4f, 0);
            freeText_2.localPosition = new Vector3(0, 10.4f, 0);
        }

        foreach (var group in smallCanvasGroups)
        {
            group.alpha = 0;
        }
        foreach (var group in bigCanvasGroups)
        {
            group.alpha = 0;
        }
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        foreach (var group in smallCanvasGroups)
        {
            group.DOKill();
        }
        foreach (var group in bigCanvasGroups)
        {
            group.DOKill();
        }

        base.OnReset();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        buyButton.OnReset();
        freeButton_1.OnReset();
        freeButton_2.OnReset();
        closeButton.OnReset();
        clockBar.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        
        gameObject.SetActive(true);

        if (isSmallPack)
        {
            for (int i = 0; i < smallCanvasGroups.Length; i++)
            {
                smallCanvasGroups[i].DOFade(1, 0.3f).SetDelay(i * 0.1f);
            }
        }
        else
        {
            for (int i = 0; i < bigCanvasGroups.Length; i++)
            {
                bigCanvasGroups[i].DOFade(1, 0.3f).SetDelay(i * 0.1f);
            }
        }
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
        
        gameObject.SetActive(false);
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
    }

    protected abstract bool SmallPackExist();

    private void OnBuyButtonClicked(ProductNameType productType)
    {
        GameManager.Purchase.BuyProduct(productType, () =>
        { 
            OnHide();
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, ()=>
            {
                OnShow();
                buyButton.gameObject.SetActive(false);
                tick_1.SetActive(true);
                lockSpine_1.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
                {
                    lockSpine_1.gameObject.SetActive(false);
                    lockSpine_1.Initialize(true);
                    freeText_1.localPosition = new Vector3(0, 10.4f, 0);
                };

                lockSpine_2.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
                {
                    lockSpine_2.gameObject.SetActive(false);
                    lockSpine_2.Initialize(true);
                    freeText_2.localPosition = new Vector3(0, 10.4f, 0);
                };
            });

            GameManager.PlayerData.OnePlusTwoPackGetRewardTime = productType == SmallProductType_Buy ? -1 : 1;
        });
    }

    private void OnFree1ButtonClicked(ProductNameType productType)
    {
        int getTime = GameManager.PlayerData.OnePlusTwoPackGetRewardTime;
        if (getTime != 0) 
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.AddNeedGetReward(productType);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                GameManager.UI.ShowUIForm(PackName);
            });

            if (getTime == -1 || getTime == 1)
                GameManager.PlayerData.OnePlusTwoPackGetRewardTime = productType == SmallProductType_Free_1 ? -2 : 2;
            else
                GameManager.PlayerData.OnePlusTwoPackGetRewardTime = 0;
        }
        else
        {
            buyButton.onClick?.Invoke();
        }
    }

    private void OnFree2ButtonClicked(ProductNameType productType)
    {
        int getTime = GameManager.PlayerData.OnePlusTwoPackGetRewardTime;
        if (getTime != 0) 
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.AddNeedGetReward(productType);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                GameManager.UI.ShowUIForm(PackName);
            });

            if (getTime == -1 || getTime == 1)
                GameManager.PlayerData.OnePlusTwoPackGetRewardTime = productType == SmallProductType_Free_2 ? -3 : 3;
            else
                GameManager.PlayerData.OnePlusTwoPackGetRewardTime = 0;
        }
        else
        {
            buyButton.onClick?.Invoke();
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
}
