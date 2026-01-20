using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using MySelf.Model;

public class EndlessChestPanel : UIForm, IItemFlyReceiver
{
	[SerializeField] private ClockBar CountdownTimer;
	[SerializeField] private DelayButton CloseBtn;
	[SerializeField] private Transform ItemRoot;
	[SerializeField] private List<EndlessChestItem> EndlessChestItems;
	[SerializeField] private GameObject RewardReceiver;
	[SerializeField] private ItemPromptBox ItemPromptBox;

	private List<EndlessTreasureDatas> list =null;
	private DateTime endDateTime;

	private int activityId=0;
	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		RewardManager.Instance.RegisterItemFlyReceiver(this);
	
		endDateTime = (DateTime)userData;
		//获取当前六个宝箱数据信息
		list = GameManager.DataTable.GetDataTable<DTEndlessTreasureData>().Data.GetEndlessTreasureDataListById(EndlessChestModel.Instance.Data.CurChestId);

		//记录活动id防止意外
		activityId = EndlessChestModel.Instance.Data.ActivityId;
		
		ShowEndlessChests();
		SetBtnEvent();
		SetCountdownTimer();
		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
	{
		CountdownTimer.OnUpdate(elapseSeconds,realElapseSeconds);
		base.OnUpdate(elapseSeconds, realElapseSeconds);
		
		if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				ItemPromptBox.HidePromptBox();
				ItemPromptBox.HidePromptBox();
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				ItemPromptBox.HidePromptBox();
				ItemPromptBox.HidePromptBox();
			}
		}
	}

	public override void OnRelease()
	{
		ItemPromptBox.OnRelease();
		list=null;
		RewardManager.Instance.UnregisterItemFlyReceiver(this);
		base.OnRelease();
	}

	private void SetBtnEvent()
	{
		CloseBtn.SetBtnEvent(() =>
		{
			int lastActivityId = EndlessChestModel.Instance.Data.LastActivityId > 0
				? EndlessChestModel.Instance.Data.LastActivityId
				: EndlessChestModel.Instance.Data.ActivityId;
			var lastData= GameManager.DataTable.GetDataTable<DTEndlessTreasureScheduleData>().Data.GetEndlessDataByActivityId(lastActivityId);
			if (lastData != null)
			{
				bool isOver = lastData.IsOver;
				bool isHaveFressReward = EndlessChestModel.Instance.Data.RecordFreeRewardDict != null &&
				                         EndlessChestModel.Instance.Data.RecordFreeRewardDict.Count > 0;
				if (isOver)
				{
					if (EndlessChestModel.Instance.Data.IsPurchase && isHaveFressReward)
					{
						var rewards = EndlessChestModel.Instance.GetRcordFreeRewards();
						//展示补发界面
						RewardManager.Instance.AddNeedGetReward(rewards.Keys.ToList(),rewards.Values.ToList());
						RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.EndlessChestRewardPanel,false, () =>
						{
							GameManager.Process.EndProcess(ProcessType.EndlessChest);
						});
					}
					//结算前面endless活动数据
					EndlessChestModel.Instance.Balance();
				}
			}
			
			GameManager.UI.HideUIForm(this);
		});
	}

	private void SetCountdownTimer()
	{
		CountdownTimer.gameObject.SetActive(true);
		CountdownTimer.OnReset();
		CountdownTimer.StartCountdown(endDateTime);
		CountdownTimer.CountdownOver += OnCountdownOver;
	}
	
	private void OnCountdownOver(object sender, CountdownOverEventArgs e)
	{
		CountdownTimer.gameObject.SetActive(false);
		CountdownTimer.OnReset();
	}

	//展示endless chest
	private void ShowEndlessChests()
	{
		for (int i = 0; i < list.Count; i++)
		{
			EndlessChestItems[i].Init(EndlessChestModel.Instance.Data.CurChestId+i,list[i],BuyEvent, StartGetRewardEvent,OverGetRewardEvent,ShowItemPromptBox);
		}
	}

	//展示领取奖励之后 第一个宝箱移走动画，后续宝箱前移动画
	private void ShowEndlessChestAnim()
	{
		List<Vector3> posList = new List<Vector3>();
		foreach (var item in EndlessChestItems)
		{
			posList.Add(item.transform.localPosition);
		}

		Sequence sequence = DOTween.Sequence();
		sequence.Append(EndlessChestItems[0].transform.DOScale(0,0.2f).SetDelay(0.1f).SetEase(Ease.InSine));
		sequence.Append(EndlessChestItems[1].transform.DOLocalMove(posList[0], 0.15f).SetEase(Ease.InOutSine));
		sequence.Join(EndlessChestItems[2].transform.DOLocalMove(posList[1], 0.15f).SetEase(Ease.InOutSine));
		sequence.Join(EndlessChestItems[3].transform.DOLocalMove(posList[2], 0.15f).SetEase(Ease.InOutSine));
		sequence.Join(EndlessChestItems[4].transform.DOLocalMove(posList[3], 0.15f).SetEase(Ease.InOutSine));
		sequence.Join(EndlessChestItems[5].transform.DOLocalMove(posList[4], 0.15f).SetEase(Ease.InOutSine));
		sequence.AppendCallback(() =>
		{
			EndlessChestItems[0].transform.localPosition = posList[5]+Vector3.right*300;
		});
		sequence.AppendCallback(() =>
		{
			//重新摆正位置
			EndlessChestItems = SetNewList();
			
			EndlessChestItems[0].Init(EndlessChestModel.Instance.Data.CurChestId,list.First(),BuyEvent,StartGetRewardEvent,OverGetRewardEvent,ShowItemPromptBox,true);
			EndlessChestItems[5].Init(EndlessChestModel.Instance.Data.CurChestId+5,list.Last(),BuyEvent,StartGetRewardEvent,OverGetRewardEvent,ShowItemPromptBox,true);
		});
		sequence.Join(EndlessChestItems[0].transform.DOLocalMove(posList[5], 0.15f).SetDelay(0.02f).SetEase(Ease.InOutSine));
		sequence.Join(EndlessChestItems[0].transform.DOScale(Vector3.one, 0));
		sequence.AppendCallback(() =>
		{
			SetClickStatus(true);
		});
	}

	private List<EndlessChestItem> SetNewList()
	{
		List<EndlessChestItem> list = new List<EndlessChestItem>();
		var item1 = EndlessChestItems[0];
		foreach (var item in EndlessChestItems)
		{
			if(item!=item1)list.Add(item);
		}
		list.Add(item1);
		return list;
	}

	private void SetClickStatus(bool isCanClick)
	{
		CloseBtn.enabled = isCanClick;
		EndlessChestItems.ForEach(obj=>obj.SetClickStatus(isCanClick));
	}

	private void BuyEvent(int id,bool isSuccessBuy)
	{
		//购买行为处理
		if(isSuccessBuy)EndlessChestModel.Instance.RecordBuyChestId(id);
		//更新可领取奖励情况【用于补发】
		EndlessChestModel.Instance.RecordBuyChestId(id);
	}

	private void StartGetRewardEvent(int id)
	{
		SetClickStatus(false);
		//先更新数据
		list = GameManager.DataTable.GetDataTable<DTEndlessTreasureData>().Data.GetEndlessTreasureDataListById(EndlessChestModel.Instance.Data.CurChestId);
		//更新可领取奖励情况【用于补发】
		var normalFreeRewards = GameManager.DataTable.GetDataTable<DTEndlessTreasureData>().Data.GetNormalFreeRewardChest(EndlessChestModel.Instance.Data.CurChestId);
		var cardFreeRewards = GameManager.DataTable.GetDataTable<DTEndlessTreasureData>().Data.GetCardFreeRewardChest(EndlessChestModel.Instance.Data.CurChestId);
		EndlessChestModel.Instance.RecordCanGetRewards(normalFreeRewards, cardFreeRewards);
		
		//将下一个按钮变为黄色
		EndlessChestItems[1].PlayBgAnim();
	}

	private void OverGetRewardEvent(int id)
	{
		//领取奖励之后播放动画
		ShowEndlessChestAnim();
	}

	private void ShowItemPromptBox(List<TotalItemData> typeList,List<int> countList,Vector3 pos)
	{
		ItemPromptBox.Init(typeList, countList);
		ItemPromptBox.ShowPromptBox(pos.x>0?PromptBoxShowDirection.Left:PromptBoxShowDirection.Right, pos);
	}

	#region FlyItem
	public ReceiverType ReceiverType => ReceiverType.Common;
	public GameObject GetReceiverGameObject() => RewardReceiver;
	public Vector3 GetItemTargetPos(TotalItemData type)
	{
		return RewardReceiver.transform.position;
	}
	private bool isRefresh = true;
	public void OnFlyEnd(TotalItemData type){}
	public void OnFlyHit(TotalItemData type) { }
	#endregion
}
