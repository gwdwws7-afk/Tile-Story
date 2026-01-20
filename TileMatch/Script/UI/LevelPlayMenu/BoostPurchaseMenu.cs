using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostPurchaseMenu : PopupMenuForm
{
    [SerializeField]
    private DelayButton Close_Btn, Buy_Btn;
    [SerializeField]
    private TextMeshProUGUILocalize Title_Text;
    [SerializeField]
    private TextMeshProUGUI Price_Text;
    [SerializeField]
    private Image BgImage;
    [SerializeField] private Sprite[] BgSprites;
    [SerializeField] private PurchaseBanner PurchaseBanner;

    private List<MaterialPresetName> textMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.LevelNormal,
        MaterialPresetName.LevelHard,
        MaterialPresetName.LevelSurpHard,
        MaterialPresetName.Btn_Blue,
    };

    public GameObject[] BoostImages;
    private TotalItemData type;
    private int price = 1800;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        type = (TotalItemData)userData;

        int index = 0;
        switch (type.TotalItemType)
        {
            case TotalItemType.MagnifierBoost:
                index = 0;
                price = 1900;
                Title_Text.SetTerm("Game.Magnifier");
                break;
            case TotalItemType.Prop_AddOneStep:
                index = 1;
                price = 3900;
                Title_Text.SetTerm("Game.Extra Slot");
                break;
            case TotalItemType.FireworkBoost:
                index = 2;
                price = 2900;
                Title_Text.SetTerm("Game.Mega Firework");
                break;
        }

        for (int i = 0; i < BoostImages.Length; i++)
        {
            BoostImages[i].SetActive(i == index);
        }
        Price_Text.text = price.ToString();

        SetButtonEvent();

        PurchaseBanner.Init(type.TotalItemType, () =>
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.AddNeedGetReward(type, 3);
        }, () =>
        {
            //自动勾选
            LevelPlayMenu levelMenu = GameManager.UI.GetUIForm("LevelPlayMenu") as LevelPlayMenu;
            if (levelMenu != null)
            {
                levelMenu.SelectTargetBooster(type.TotalItemType);
            }
            GameManager.UI.HideUIForm("GlobalMaskPanel");
        });
        Title_Text.Target.enableVertexGradient = false;
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
        Title_Text.SetMaterialPreset(textMaterials[hardIndex]);
        BgImage.sprite = BgSprites[hardIndex];
    }

    private void SetButtonEvent()
    {
        Buy_Btn.SetBtnEvent(() =>
        {
            if (!GameManager.PlayerData.UseItem(TotalItemData.Coin, price, false))
            {
                //GameManager.Firebase.RecordMessageByEvent(
                //    Constant.AnalyticsEvent.Level_Prop_Purchase_Fail,
                //    new Parameter("Level", GameManager.PlayerData.NowLevel));
                GameManager.UI.HideUIForm(this);
                GameManager.UI.ShowUIForm("ShopMenuManager",userData: true);
                //GameManager.Firebase.RecordCoinNotEnough(2, GameManager.PlayerData.NowLevel);

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Booster_Purchase_Fail, new Parameter("Level", GameManager.PlayerData.NowLevel));
            }
            else
            {
                GameManager.UI.HideUIForm(this);
                RewardManager.Instance.AddNeedGetReward(type, 3);
                GameManager.UI.ShowUIForm("GlobalMaskPanel");
                if (RewardManager.Instance.RewardArea)
                {
                    RewardManager.Instance.RewardArea.GetRewardWorldPositionFunc = (a) => { return new Vector3(0, 0.23f, 0); };
                }
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                {
                    LevelPlayMenu levelMenu = GameManager.UI.GetUIForm("LevelPlayMenu") as LevelPlayMenu;
                    if (levelMenu != null)
                    {
                        levelMenu.SelectTargetBooster(type.TotalItemType);
                    }

                    GameManager.UI.HideUIForm("GlobalMaskPanel");
                });
                //GameManager.PlayerData.AddItemNum(type, 3);
                //LevelPlayMenu levelMenu = GameManager.UI.GetUIForm<LevelPlayMenu>() as LevelPlayMenu;
                //if (levelMenu != null)
                //{
                //    levelMenu.SelectTargetBooster(type.TotalItemType);
                //}

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Booster_Purchase, new Parameter("Level", GameManager.PlayerData.NowLevel),
    new Parameter("boosterID", (long)type.TotalItemType));
            }
        });

        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }
}
