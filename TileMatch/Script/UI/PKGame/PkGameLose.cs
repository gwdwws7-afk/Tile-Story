using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using TMPro;
using UnityEngine;

public class PkGameLose : BaseGameFailPanel
{
    [SerializeField] private TextMeshProUGUI Num_Text;
    [SerializeField] private Transform propTrans;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.PkGameFailPanel;

    public override bool IsShowFailPanel=>
                                          PkGameModel.Instance.IsActivityOpen&& 
                                          PkGameModel.Instance.RecordEnterGameStatus!=PkGameStatus.Over;

    public override void ShowFailPanel(Action action)
    {
        int num = PkGameModel.Instance.PkRewardItemNum(GameManager.PlayerData.NowLevel);
        Num_Text.text = $"X {num}";

        var pkScore = GetComponentInChildren<PkSorce>();
        if(pkScore!=null)pkScore.Init();

        propTrans.localScale = Vector3.zero;
        propTrans.DOScale(1f, 0.2f).SetEase(Ease.OutBack,1.4f);
    }
}
