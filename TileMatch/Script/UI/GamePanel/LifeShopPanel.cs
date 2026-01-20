using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeShopPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Buy_Btn, Close_Btn;
    [SerializeField] private TextMeshProUGUI Price_Text;
    [SerializeField] private GameObject m_FreeText, m_CoinBuyText;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetEvent();

        if(IsFirstBuyLife())
        {
            m_FreeText.SetActive(true);
            m_CoinBuyText.SetActive(false);
        }
        else
        {
            m_FreeText.SetActive(false);
            m_CoinBuyText.SetActive(true);
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    private bool IsFirstBuyLife()
    {
        return PlayerPrefs.GetInt("IsFirstBuyLife", 0) == 0;
    }

    private void SetFirstBuyLife()
    {
        PlayerPrefs.SetInt("IsFirstBuyLife", 1);
    }

    private void SetEvent()
    {
        Buy_Btn.SetBtnEvent(() =>
        {
            if (IsFirstBuyLife())
            {
                GameManager.UI.HideUIForm(this);
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Life, 5);
                RewardManager.Instance.AutoGetRewardDelayTime = 0.4f;
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                {
                    RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                });
                SetFirstBuyLife();
            }
            else
            {
                int coinNum = 900;
                if (GameManager.PlayerData.UseItem(TotalItemData.Coin, coinNum))
                {
                    GameManager.UI.HideUIForm(this);
                    RewardManager.Instance.AddNeedGetReward(TotalItemData.Life, 5);
                    RewardManager.Instance.AutoGetRewardDelayTime = 0.4f;
                    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                    {
                        RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                    });

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Lives_Buy);
                }
                else
                {
                    UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
                    if (group != null && group.UIFormCount == 1)
                        GameManager.UI.HideUIForm(this);
                    GameManager.UI.ShowUIForm("ShopMenuManager");
                    GameManager.Firebase.RecordCoinNotEnough(8, GameManager.PlayerData.NowLevel);
                }
            }
        });

        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }
}
