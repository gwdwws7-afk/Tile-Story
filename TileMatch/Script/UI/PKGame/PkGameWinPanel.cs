using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;

public class PkGameWinPanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private DelayButton CloseBtn,HelpBtn,CotinueBtn;
    [SerializeField] private TextMeshProUGUILocalize TextMeshProUGUILocalize;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        int num = PkGameModel.Instance.GetRewardCoinNumByRank(PkGameOverStatus.Win);
        TextMeshProUGUILocalize.SetParameterValue("{0}",num.ToString());
        PkSorce.Init();
        BtnEvent();
        ShowEffect();
        PkGameModel.Instance.RemoveListen();
        GameManager.Sound.PlayAudio("SFX_PK_Success");
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void BtnEvent()
    {
        Action finishAction = () =>
        {
            GameManager.Firebase.RecordMessageByEvent("PKMatch_Win");
            
            if(PkGameModel.Instance.IsNoNetwork())GameManager.Firebase.RecordMessageByEvent("PKMatch_Win_NoNetwork");
            //数据清理
            PkGameModel.Instance.BalanceActivity();
            
            GameManager.Event.FireNow(CommonEventArgs.EventId,CommonEventArgs.Create(CommonEventType.PkGameOver));
            SetHideAction(null);
            //给奖励
            RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin,PkGameModel.Instance.GetRewardCoinNumByRank(PkGameOverStatus.Win));
            //自动弹出奖励
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
          
            //关闭当前界面
            GameManager.UI.HideUIForm(this);
        };
        CloseBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
        HelpBtn.SetBtnEvent(() =>
        {
           //help
           GameManager.UI.ShowUIForm("PkRulesPanel");
        });
        CotinueBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
    }
    
    private void ShowEffect()
    {
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "TileWellDone",
            "TileItemDestroyEffectPool",
            2.2f,
            transform.position,
            transform.rotation,
            transform, (t) =>
            {
                var effect = t?.Target as GameObject;
                if (effect != null)
                {
                    var skeleton = effect.transform.GetComponent<SkeletonGraphic>();
                    if (skeleton != null)
                    {
                        skeleton.AnimationState.ClearTracks();
                        skeleton.Skeleton.SetToSetupPose();
                        skeleton.AnimationState.SetAnimation(0, "active", false);
                    }
                }
            });
    }

}
