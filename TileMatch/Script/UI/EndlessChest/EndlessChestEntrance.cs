using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Event;
using MySelf.Model;
using UnityEngine;

public enum EndlessChestType
{
   EndlessChestPanel,
}

public class EndlessChestEntrance : UIForm
{
   [Tooltip("加载宝箱界面的名称")]
   [SerializeField] private EndlessChestType Type;//无尽宝箱名称，用来实例化无尽宝箱界面
   [SerializeField] private DelayButton EnterBtn;
   [SerializeField] private CountdownTimer CountdownTimer;
   [SerializeField] private GameObject RedPointObj;

   private EndlessTreasureScheduleData data;
   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      Init();
      base.OnInit(uiGroup, completeAction, userData);
   }

   public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
   {
      CountdownTimer.OnUpdate(elapseSeconds,realElapseSeconds);
      base.OnUpdate(elapseSeconds, realElapseSeconds);
   }

   public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup){}

   public override void OnRelease()
   {
      GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEvent);
      data = null;
      base.OnRelease();
   }

   private void Init()
   {
      //是否解锁
      if (!EndlessChestModel.Instance.IsEndlessUnlock)
      {
         gameObject.SetActive(false);
         return;
      }

      //获取数据
      GetData();
      //
      if (data == null||EndlessChestModel.Instance.IsHaveOverById(data.ActivityID))
      {
         gameObject.SetActive(false);
         return;
      }
      
      gameObject.SetActive(true);
      //记录当期的activityid
      EndlessChestModel.Instance.RecordActivityId(data.ActivityID);

      GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEvent);

      //如果有活动数据刷新界面显示
      var freeRewars = GameManager.DataTable.GetDataTable<DTEndlessTreasureData>().Data.GetFreeRewardChest(EndlessChestModel.Instance.Data.CurChestId);
      
      RedPointObj.gameObject.SetActive(freeRewars.Count>0);
      SetCountdownTimer(data.EndDateTime);
      SetBtnEvent();
   }

   //先获取数据
   private void GetData()
   {
      //根据当前时间获取 当前可用的活动
      data = GameManager.DataTable.GetDataTable<DTEndlessTreasureScheduleData>().Data.GetEndlessDataByDateTimeNow();
   }

   private void SetCountdownTimer(DateTime endTime)
   {
      CountdownTimer.OnReset();
      CountdownTimer.StartCountdown(endTime);
      CountdownTimer.CountdownOver += (a, b) =>
      {
         var ui = GameManager.UI.GetUIForm("EndlessChestPanel");
          if (ui != null && ui.gameObject.activeSelf) 
         {
            //直接刷新
            Init();
         }
         else
         {
            //进行补发
            GameManager.Process.Unregister(ProcessType.EndlessChest);
            GameManager.Process.Register(ProcessType.EndlessChest.ToString(),5, () =>
            {
               int lastActivityId = EndlessChestModel.Instance.Data.LastActivityId > 0
                  ? EndlessChestModel.Instance.Data.LastActivityId
                  : EndlessChestModel.Instance.Data.ActivityId;
               var lastData= GameManager.DataTable.GetDataTable<DTEndlessTreasureScheduleData>().Data.GetEndlessDataByActivityId(lastActivityId);
               //有活动并且活动结束了，补发
               if (lastData != null)
               {
                    if (lastData.IsOver && !EndlessChestModel.Instance.IsHaveOverById(lastData.ActivityID))
                    {
                        //补发
                        if (EndlessChestModel.Instance.Data.RecordFreeRewardDict != null &&
                            EndlessChestModel.Instance.Data.RecordFreeRewardDict.Count > 0)
                        {
                            var rewards = EndlessChestModel.Instance.GetRcordFreeRewards();
                            if (rewards != null && rewards.Keys.Count > 0)  
                            {
                                //展示补发界面
                                RewardManager.Instance.AddNeedGetReward(rewards.Keys.ToList(), rewards.Values.ToList());
                                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.EndlessChestRewardPanel, false, () =>
                                {
                                    GameManager.Process.EndProcess(ProcessType.EndlessChest);

                                }, null, () =>
                                {
                                    GameManager.UI.HideUIForm("GlobalMaskPanel");
                                });
                            }
                            else
                            {
                                GameManager.Process.EndProcess(ProcessType.EndlessChest);
                            }
                        }
                        else
                        {
                            GameManager.Process.EndProcess(ProcessType.EndlessChest);
                        }

                        //结算前面endless活动数据
                        EndlessChestModel.Instance.Balance();
                    }
                    else
                        GameManager.Process.EndProcess(ProcessType.EndlessChest);
                }
                else
                  GameManager.Process.EndProcess(ProcessType.EndlessChest);
            });
            GameManager.Process.ExecuteProcess();
         }
      };
   }

   private void SetBtnEvent()
   {
      EnterBtn.SetBtnEvent(() =>
      {
         //打开endless
         GameManager.UI.ShowUIForm(Type.ToString(),userData:data.EndDateTime);
      });
   }
   
   private void CommonEvent(object sender, GameEventArgs e)
   {
      CommonEventArgs ne = (CommonEventArgs)e;
      if (ne.Type == CommonEventType.EndlessOver)
      {
         Init();
      }else if(ne.Type==CommonEventType.EndlssGetFreeReward)
      {
         //红点开关
         bool isHaveReward = EndlessChestModel.Instance.Data.RecordFreeRewardDict!=null&&
                             EndlessChestModel.Instance.Data.RecordFreeRewardDict.Count>0;
         RedPointObj.gameObject.SetActive(isHaveReward);
      }else if (ne.Type == CommonEventType.EndlessBalance)
      {
         Init();
      }
   }
}
