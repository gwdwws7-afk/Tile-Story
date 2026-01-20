using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPropPurchaseMenu : PopupMenuForm
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

    public GameObject[] PropImages;

    private TotalItemData type;
    private int price = 1800;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        type = (TotalItemData)userData;

        int index = 0;
        switch(type.TotalItemType)
        {
            case TotalItemType.Prop_Back:
                index = 0;
                price = 1900;
                Title_Text.SetTerm("Shop.Glove");
                break;
            case TotalItemType.Prop_ChangePos:
                index = 1;
                price = 3900;
                Title_Text.SetTerm("Shop.Fan");
                break;
            case TotalItemType.Prop_Absorb:
                index = 2;
                price = 2900;
                Title_Text.SetTerm("Shop.Magnet");
                break;
            case TotalItemType.Prop_AddOneStep:
                index = 3;
                price = 3900;
                Title_Text.SetTerm("Shop.Extra Slot");
                break;
            case TotalItemType.Prop_Grab:
                index = 4;
                price = 3900;
                Title_Text.SetTerm("Shop.Crane");
                break;
        }

        for (int i = 0; i < PropImages.Length; i++)
        {
            PropImages[i].SetActive(i == index);
        }
        Price_Text.text = price.ToString();

        SetButtonEvent();
        PurchaseBanner.Init(type.TotalItemType,() =>
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.AddNeedGetReward(type, 3);
        }, () =>
        {
            GameManager.UI.HideUIForm("GlobalMaskPanel");
            //DoPropSkill();
            DoPropSkill();
        });

        Title_Text.Target.enableVertexGradient = false;
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
        Title_Text.SetMaterialPreset(textMaterials[hardIndex]);
        if (hardIndex < BgSprites.Length) 
            BgImage.sprite = BgSprites[hardIndex];
    }

    private void SetButtonEvent()
    {
        Buy_Btn.SetBtnEvent(() =>
        {
            if (!GameManager.PlayerData.UseItem(TotalItemData.Coin, price, false))
            {
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.Level_Prop_Purchase_Fail,
                    new Parameter("Level", GameManager.PlayerData.NowLevel));
                GameManager.UI.HideUIForm(this);
                ShopMenuManager.RecordSourceIndex = 2;
                GameManager.UI.ShowUIForm("ShopMenuManager",userData: true);
                GameManager.Firebase.RecordCoinNotEnough(2, GameManager.PlayerData.NowLevel);
            }
            else
            {
                GameManager.UI.HideUIForm(this);
                RewardManager.Instance.AddNeedGetReward(type, 3);
                GameManager.UI.ShowUIForm("GlobalMaskPanel");
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                {
                    GameManager.UI.HideUIForm("GlobalMaskPanel");
                    DoPropSkill();
                });

                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.Level_Prop_Purchase,
                    new Parameter("Level", GameManager.PlayerData.NowLevel),
                    new Parameter("PropID", (type.ID)));

                if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Level_Prop_Purchase, new Parameter("DailyLevel", GameManager.PlayerData.NowLevel));
            }
        });

        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
        });
    }

    private void DoPropSkill()
    {
        //GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.AutoUsePropSkill, type));
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            if (type != TotalItemData.Prop_AddOneStep) 
            {
                for (int i = 0; i < panel.skillList.Count; i++)
                {
                    if (panel.skillList[i].GetTotalItemType() == type)
                    {
                        panel.skillList[i].GuideButton.onClick?.Invoke();
                        break;
                    }
                }
            }
        }
    }
}
