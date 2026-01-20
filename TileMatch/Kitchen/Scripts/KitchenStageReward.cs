using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class KitchenStageReward : MonoBehaviour
{
    [HideInInspector]
    public int taskId;
    public Transform progressBg;
    public Transform rewardPanel;
    public GameObject rewardPrefab, claimed;

    public Image[] rewardBgArray;
    public Image[] progressBgArray;
    
    public List<KitchenStageRewardItem> rewardItemList;
    
    /// <summary>
    /// 初始化阶段奖励
    /// </summary>
    /// <param name="isCliamed">奖励是否已领取</param>
    public void Init(bool isCliamed, bool isFilp, int taskId, int reward, int progress, List<TotalItemData> rewards, List<int> rewardsNum)
    {
        this.taskId = taskId;
        
        for (int i = 0; i < rewardBgArray.Length; i++)
        {
            rewardBgArray[i].gameObject.SetActive(i == reward);
        }

        for (int i = 0; i < progressBgArray.Length; i++)
        {
            progressBgArray[i].gameObject.SetActive(i == progress);
        }
        
        if (isFilp) progressBg.transform.localScale = new Vector3(-1, 1, 1);
        else progressBg.transform.localScale = Vector3.one;

        rewardItemList = new List<KitchenStageRewardItem>();
        
        if (isCliamed)
        {
            // 已领取
            claimed.SetActive(true);
            rewardPanel.gameObject.SetActive(false);
        }
        else
        {
            // 还没领取奖励，0表示当前解锁的奖励，1表示还未解锁的奖励
            claimed.SetActive(false);
            rewardPanel.gameObject.SetActive(true);
            // 初始化阶段奖励
            if (rewards.Count != rewardsNum.Count)
            {
                Debug.LogError("KitchenMainMenu：奖励和奖励数量无法匹配");
            }
            else
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    var rewardItem = Instantiate(rewardPrefab, rewardPanel).GetComponent<KitchenStageRewardItem>();
                    rewardItem.OnInit(rewards[i], rewardsNum[i]);
                    rewardItem.transform.localScale = Vector3.one * (rewards.Count == 1 ? 1.5f : 1f);
                    rewardItem.gameObject.SetActive(true);
                    rewardItemList.Add(rewardItem);
                }
            }
        }
    }

    public void OnRelease()
    {
        if (rewardItemList != null)
        {
            foreach (var item in rewardItemList)
            {
                Destroy(item.gameObject);
            }
            rewardItemList.Clear();
        }
    }

    public void Refresh(bool isCliamed, int rewardImg, int progressImg)
    {
        if (isCliamed)
        {
            claimed.SetActive(true);
            rewardPanel.gameObject.SetActive(false);
        }
        for (int i = 0; i < rewardBgArray.Length; i++)
        {
            rewardBgArray[i].gameObject.SetActive(i == rewardImg);
        }

        for (int i = 0; i < progressBgArray.Length; i++)
        {
            progressBgArray[i].gameObject.SetActive(i == progressImg);
        }
    }

    public void RefreshToClaimed(bool isFlip, int rewardImg, int progressImg, Action callback = null)
    {
        //动画播放顺序
        // 1、完成的进度条播放从左到右的动画，动画播完后放大在缩小
        // 2、奖励放大做淡出
        // 3、打勾缩小做淡入
        // 4、在打勾缩小到0.8时，播放奖励框和大勾一起缩小再放大的动画，同时奖励框的颜色变为目标颜色
        // 5、滚动条滚动到目标位置，同时目标奖励框的颜色变为绿色
        Image claimedProgress = progressBgArray[progressImg];
        for (int i = 0; i < progressBgArray.Length; i++)
        {
            //fillOrigin：水平方向上 0为left  1为right
            if (isFlip) 
                claimedProgress.fillOrigin = 1;
            else 
                claimedProgress.fillOrigin = 0;
            // 将进度条的值至0
            claimedProgress.fillAmount = 0;
            // 同时显示未完成和完成的进度条
            progressBgArray[i].gameObject.SetActive(i % 2 == progressImg % 2);
        }

        claimedProgress.DOFillAmount(1, 0.5f).onComplete += () =>
        {
            claimedProgress.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0);

            rewardPanel.DOScale(1.3f, 0.3f);
            rewardPanel.GetComponent<CanvasGroup>().DOFade(0, 0.3f);

            claimed.transform.localScale = Vector3.one * 1.5f;
            claimed.SetActive(true);
            claimed.transform.DOScale(0.8f, 0.5f).SetEase(Ease.InCubic).onComplete += () =>
            {
                rewardBgArray[0].transform.parent.DOPunchScale(-Vector3.one * 0.1f, 0.2f, 1, 0).onComplete += () =>
                {
                    // 播放特效
                    // 切换奖励框
                    for (int i = 0; i < rewardBgArray.Length; i++)
                    {
                        rewardBgArray[i].gameObject.SetActive(i == rewardImg);
                    }
                    // 更新下一个奖励框的状态
                    callback?.Invoke();
                    callback = null;
                };
            };
            
        };
    }
}
