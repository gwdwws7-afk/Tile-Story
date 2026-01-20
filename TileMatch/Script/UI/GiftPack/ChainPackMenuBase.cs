using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ChainPackMenuBase : PopupMenuForm
{
    public TextMeshProUGUILocalize buyPrice_1, buyPrice_2, buyPrice_3;
    public DelayButton buyButton_1, buyButton_2, buyButton_3, closeButton;
    public GameObject tick_1, tick_2, tick_3;
    public SkeletonGraphic lockSpine_1, lockSpine_2;
    public ClockBar clockBar;
    public Material greyMaterial;

    protected abstract string PackName { get; }
    protected abstract ProductNameType ProductType_1 { get; }
    protected abstract ProductNameType ProductType_2 { get; }
    protected abstract ProductNameType ProductType_3 { get; }
    protected abstract DateTime StartTime { get; }
    protected abstract DateTime EndTime { get; }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        buyButton_1.OnInit(() => OnBuyButtonClicked(ProductType_1));
        buyButton_2.OnInit(() => OnBuyButtonClicked(ProductType_2));
        buyButton_3.OnInit(() => OnBuyButtonClicked(ProductType_3));
        closeButton.OnInit(OnCloseButtonClick);

        string price = GameManager.Purchase.GetPrice(ProductType_1);
        if (!string.IsNullOrEmpty(price))
            buyPrice_1.SetTerm(price);

        string price2 = GameManager.Purchase.GetPrice(ProductType_2);
        if (!string.IsNullOrEmpty(price2))
            buyPrice_2.SetTerm(price2);

        string price3 = GameManager.Purchase.GetPrice(ProductType_3);
        if (!string.IsNullOrEmpty(price3))
            buyPrice_3.SetTerm(price3);

        Refresh(false);

        if (DateTime.Now > StartTime && DateTime.Now < EndTime)
        {
            clockBar.OnReset();
            clockBar.StartCountdown(EndTime);
            clockBar.CountdownOver += OnCountdownOver;
        }
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    public override void OnRelease()
    {
        buyButton_1.OnReset();
        buyButton_2.OnReset();
        buyButton_3.OnReset();
        closeButton.OnReset();
        clockBar.OnReset();

        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void Refresh(bool isShowAnim)
    {
        int stage = GameManager.PlayerData.ChainPackStage;

        buyButton_1.gameObject.SetActive(stage == 0);
        buyButton_2.gameObject.SetActive(stage <= 1);
        buyButton_3.gameObject.SetActive(stage <= 2);
        tick_1.SetActive(stage > 0);
        tick_2.SetActive(stage > 1);
        tick_3.SetActive(stage > 2);
        if (stage == 0)
        {
            buyButton_2.body.GetComponent<Image>().material = greyMaterial;
            buyButton_3.body.GetComponent<Image>().material = greyMaterial;
            buyPrice_2.SetMaterialPreset(MaterialPresetName.Btn_Grey);
            buyPrice_3.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        }
        else if (stage == 1)
        {
            buyButton_2.body.GetComponent<Image>().material = null;
            buyButton_3.body.GetComponent<Image>().material = greyMaterial;
            buyPrice_2.SetMaterialPreset(MaterialPresetName.Btn_Green);
            buyPrice_3.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        }
        else
        {
            buyButton_2.body.GetComponent<Image>().material = null;
            buyButton_3.body.GetComponent<Image>().material = null;
            buyPrice_2.SetMaterialPreset(MaterialPresetName.Btn_Green);
            buyPrice_3.SetMaterialPreset(MaterialPresetName.Btn_Green);
        }

        if (isShowAnim)
        {
            if (stage == 1)
            {
                lockSpine_1.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
                  {
                      lockSpine_1.gameObject.SetActive(false);
                      lockSpine_1.Initialize(true);
                  };

                GameManager.Sound.PlayAudio(SoundType.SFX_Help_Chapter_Unlock.ToString());
            }
            else if (stage == 2)
            {
                lockSpine_2.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
                {
                    lockSpine_2.gameObject.SetActive(false);
                    lockSpine_2.Initialize(true);
                };

                GameManager.Sound.PlayAudio(SoundType.SFX_Help_Chapter_Unlock.ToString());
            }
            else
            {
                lockSpine_1.gameObject.SetActive(stage == 0);
                lockSpine_2.gameObject.SetActive(stage <= 1);
            }
        }
        else
        {
            lockSpine_1.gameObject.SetActive(stage == 0);
            lockSpine_2.gameObject.SetActive(stage <= 1);
        }
    }

    private void OnBuyButtonClicked(ProductNameType productType)
    {
        int stage = GameManager.PlayerData.ChainPackStage;
        if (productType == ProductType_2 && stage < 1) 
        {
            return;
        }

        if (productType == ProductType_3 && stage < 2)
        {
            return;
        }

        GameManager.Purchase.BuyProduct(productType, () =>
        {
            if (stage >= 2)
                GameManager.PlayerData.ChainPackStage = 0;
            else
                GameManager.PlayerData.ChainPackStage = stage + 1;

            OnHide();
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                OnShow();
                Refresh(true);
            });
        });
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.UI.HideUIForm(this);
    }
}
