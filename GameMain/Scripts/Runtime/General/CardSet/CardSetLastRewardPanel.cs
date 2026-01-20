using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardSetLastRewardPanel : PopupMenuForm
{
    public Transform content;
    public DelayButton closeButton, claimButton;

    private Dictionary<TotalItemData, int> _unclaimedRewards = new Dictionary<TotalItemData, int>();
    private List<CardSetRewardSlot> _rewardSlotList = new List<CardSetRewardSlot>();
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GetUnclaimedRewards();
        SetContent();
        
        closeButton.OnInit(OnButtonClick);
        claimButton.OnInit(OnButtonClick);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        closeButton.OnReset();
        claimButton.OnReset();
        
        foreach (var slot in _rewardSlotList)
        {
            slot.Release();
        }
        _rewardSlotList.Clear();
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
    }

    private void GetUnclaimedRewards()
    {
        //套组奖励
        foreach (var kvp in CardModel.Instance.CollectCardDict)
        {
            if (kvp.Value.Count == 9 && !CardModel.Instance.CompletedCardSets.Contains(kvp.Key))
            {
                CardSet cardSet = CardModel.Instance.CardSetDict[kvp.Key];
                AddRewards(cardSet.RewardTypeList, cardSet.RewardNumList);
            }
        }
        //集满奖励
        if (CardModel.Instance.TotalCollectNum == CardModel.Instance.CardSetDict.Count * 9 && !CardModel.Instance.CompletedAll)
        {
            
            (List<TotalItemData> rewardTypeList, List<int> rewardNumList) = GameManager.DataTable.GetDataTable<DTCardSetData>().Data
                .GetCurrentFinalRewardByActivityID(CardModel.Instance.CardActivityID);
            AddRewards(rewardTypeList, rewardNumList);
        }
    }

    private void AddRewards(List<TotalItemData> rewardTypeList, List<int> rewardNumList)
    {
        for (int i = 0; i < rewardTypeList.Count; i++)
        {
            if (_unclaimedRewards.TryGetValue(rewardTypeList[i], out int num))
            {
                _unclaimedRewards[rewardTypeList[i]] = rewardNumList[i] + num;
            }
            else
            {
                _unclaimedRewards.Add(rewardTypeList[i], rewardNumList[i]);
            }
        }
    }

    private void SetContent()
    {
        foreach (var kvp in _unclaimedRewards)
        {
            AsyncOperationHandle assetHandle = UnityUtility.InstantiateAsync(
                "CardSetRewardSlot", content, asset =>
                {
                    CardSetRewardSlot slot = asset.GetComponent<CardSetRewardSlot>();
                    slot.Init(kvp.Key, kvp.Value);
                    _rewardSlotList.Add(slot);
                });
            _assetHandleList.Add(assetHandle);
        }

        //排序
        _rewardSlotList.Sort((x, y) =>
        {
            int xTypeIndex = x.ItemType.TotalItemTypeInt;
            int yTypeIndex = y.ItemType.TotalItemTypeInt;

            if (xTypeIndex < yTypeIndex)
            {
                return -1;
            }

            return 1;
        });
    }

    private void OnButtonClick()
    {
        foreach (var kvp in _unclaimedRewards)
        {
            RewardManager.Instance.AddNeedGetReward(kvp.Key, kvp.Value);
        }
        
        GameManager.UI.HideUIForm(this);
        
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
        });
    }
}
