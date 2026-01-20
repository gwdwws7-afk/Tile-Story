using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class PkGameDeucePanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private DelayButton CloseBtn,ContinueBtn;
    [SerializeField] private TextMeshProUGUILocalize TextMeshProUGUILocalize;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        int num = PkGameModel.Instance.GetRewardCoinNumByRank(PkGameOverStatus.Deuce);
        TextMeshProUGUILocalize.SetParameterValue("{0}",num.ToString());
        PkSorce.Init();
        SetBtnEvetn();
        PkGameModel.Instance.RemoveListen();
        GameManager.Sound.PlayAudio("SFX_PK_Lose");
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void SetBtnEvetn()
    {
        Action finishAction = () =>
        {
            GameManager.Firebase.RecordMessageByEvent("PKMatch_Draw");
            //结算活动
            PkGameModel.Instance.BalanceActivity();
            
            GameManager.Event.FireNow(CommonEventArgs.EventId,CommonEventArgs.Create(CommonEventType.PkGameOver));
            //给奖励
            RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin,PkGameModel.Instance.GetRewardCoinNumByRank(PkGameOverStatus.Deuce));
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                //如果没有展示奖励需要注册一个奖励展示进去插入
                if (!PkGameModel.Instance.IsActivityOpen&&PkGameModel.Instance.IsCanOpenByTime)
                {
                    GameManager.UI.ShowUIForm("PkGameStartPanel",u =>
                    {
                        u.SetHideAction(() =>
                        {
                            GameManager.Process.EndProcess(ProcessType.ShowPkGameOver);
                            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                        });
                    });
                }
                else
                {
                    GameManager.Process.EndProcess(ProcessType.ShowPkGameOver);
                    GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                }
            },null);
            //关闭界面
            GameManager.UI.HideUIForm(this);
        };
        CloseBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
        ContinueBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
    }
}
