using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 奖励界面
/// </summary>
public class SkillUnlockRewardPanel : RewardPanel
{
    [SerializeField] private TextMeshProUGUILocalize Content_Text;
    [SerializeField] private Graphic Content_Graphic;
    [SerializeField] private DelayButton Claim_Btn;

    private const string skill2 = "Skill.Undo the last tile in the rack";
    private const string skill3 = "Skill.Shuffle all tiles on the board";
    private const string skill4 = "Skill.Make an auto-match";
    private const string skill5 = "Common.Move 3 surface tiles away";

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        base.OnShow(rewardArea,onShowComplete);

        SetContentText();
        Content_Graphic.gameObject.SetActive(true);
        Content_Graphic.color = new Color(1, 1, 1, 0);
        Content_Graphic.DOFade(1, 0.4f).SetDelay(0.2f);
        var graphic = Claim_Btn.GetComponent<Graphic>();
        graphic.color = new Color(1, 1, 1, 0);
        graphic.DOFade(1, 0.2f).SetDelay(5f);
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        base.OnHide(quickHide, onHideComplete);
        Content_Graphic.DOFade(0, 0.2f);
    }
    
    public override void SetOnClickEvent(UnityAction onClick)
    {
        Claim_Btn.SetBtnEvent(() =>
        {
            onClick?.Invoke();
        });
    }

    public override void ClearOnClickEvent()
    {
        Claim_Btn.onClick.RemoveAllListeners();
    }

    public void SetContentText()
    {
        TotalItemData type = TotalItemData.None;

        switch (GameManager.PlayerData.NowLevel)
        {
            case 5:
                type = TotalItemData.Prop_Back;
                break;
            case 9:
                type = TotalItemData.Prop_ChangePos;
                break;
            case 13:
                type = TotalItemData.Prop_Grab;
                break;
            case 20:
                type = TotalItemData.Prop_Absorb;
                break;
        }

        if (type == TotalItemData.Prop_Back)
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
                Content_Text.SetTerm("Skill.Undo the last placed tile on the board");
            else
                Content_Text.SetTerm(skill2);
        }
        else if (type == TotalItemData.Prop_ChangePos)
        {
            Content_Text.SetTerm(skill3);
        }
        else if (type == TotalItemData.Prop_Grab)
        {
            Content_Text.SetTerm(skill4);
        }
        else if (type == TotalItemData.Prop_Absorb)
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
                Content_Text.SetTerm("Common.Remove up to 2 types of tiles from the rack");
            else
                Content_Text.SetTerm(skill5);
        }
    }
}