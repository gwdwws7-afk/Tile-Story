using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class WeekendPicnicPackMenu : PopupMenuForm
{
    public ProductNameType[] productNameTypes;
    public CommonBuyButton buyButton;
    public DelayButton closeButton;
    public CountdownTimer timer;
    public TextMeshProUGUI coinText;
    public Image[] rewardImages;
    public TextMeshProUGUI[] rewardTexts;
    public TextMeshProUGUI offText;
    public string[] offTexts;

    private DateTime endDateTime;

    private int index = -1;
    private int Index
    {
        get
        {
            if (index == -1)
            {
                // 默认值为 1，代表内购玩家第一次购买周末野餐礼包
                if (GameManager.Ads.IsRemovePopupAds)
                    index = PlayerPrefs.GetInt("WeekendPicnic", 1);
                else
                    index = 0;
            }
            return index;
        }
        set
        {
            index = Mathf.Min(value, 2);
            PlayerPrefs.SetInt("WeekendPicnic", index);
        }
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        if (userData != null)
        {
            endDateTime = (DateTime)userData;
        }

        InitPackage(Index);
        BtnEvent(Index);

        timer.OnReset();
        timer.StartCountdown(endDateTime);
    }

    public override void OnReset()
    {
        timer.OnReset();

        for (int i = 0; i < rewardImages.Length; i++)
        {
            if (rewardImages[i].sprite != null)
            {
                Addressables.Release(rewardImages[i].sprite);
                rewardImages[i].sprite = null;
            }
        }

        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        timer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void InitPackage(int index)
    {
        var list = GameManager.DataTable.GetDataTable<DTShopPackageData>().Data.GetShopPackageData(productNameTypes[index]).GetItemDatas();
        int rewardInt = 0;
        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].type.ID)
            {
                case 1://金币
                    coinText.text = list[i].num.ToString();
                    break;
                default:
                    int temp = rewardInt;
                    rewardTexts[temp].SetItemText(list[i].num, list[i].type, true);
                    string spriteKey = UnityUtility.GetRewardSpriteKey(list[i].type, 0);

                    var image = rewardImages[temp];
                    image.color = new Color(1, 1, 1, 0);
                    UnityUtility.LoadSpriteAsync(spriteKey, "TotalItemAtlas", s =>
                     {
                         image.sprite = s;
                         image.SetNativeSize();
                         image.DOFade(1, 0.1f);
                     });
                    rewardInt++;
                    break;
            }
        }
        offText.text = offTexts[index];
    }

    private void BtnEvent(int index)
    {
        closeButton.OnInit(() => GameManager.UI.HideUIForm(this));

        buyButton.Init(productNameTypes[index], () =>
        {
            // 隐藏礼包
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
            // 增加礼包等级
            Index += 1;
            // 记录礼包期数
            DateTime signDateTime = new DateTime(2024, 5, 31);
            PlayerPrefs.SetInt("WeekendPicnicPeriod", (DateTime.Now - signDateTime).Days / 7 + 1);
        }, (fail) =>
        {
            Log.Error($"Buy Fail:{fail.ToString()}");
        });
    }
}
