using DG.Tweening;
using System;
using MySelf.Model;
using UnityEngine;

/// <summary>
/// 奖励界面
/// </summary>
public class PkGameRewardPanel : RewardPanel
{
    [SerializeField] private TextMeshProUGUILocalize contentText2;
    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        contentText2.Target.color = new Color(1, 1, 1, 0);
        contentText2.Target.DOFade(1, 0.2f);
        SetContentText();
        base.OnShow(rewardArea, onShowComplete);
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        contentText2.Target.DOFade(0,0.2f);
        base.OnHide(quickHide, onHideComplete);
    }

    private void SetContentText()
    {
        bool isFirstWin = PkGameModel.Instance.IsFirstWin;
        string contentTerm = isFirstWin ? "Pk.You beat the level on you1" : "Pk.Win at first try, earn the most flags!";
        contentText2.SetTerm(contentTerm);
        if (isFirstWin)
        {
            contentText2.SetParameterValue("{0}","<color=#FCE400>");
            contentText2.SetParameterValue("{1}","</color>");
        }
    }
}