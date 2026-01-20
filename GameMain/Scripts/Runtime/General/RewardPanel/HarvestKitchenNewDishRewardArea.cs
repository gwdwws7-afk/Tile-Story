using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HarvestKitchenNewDishRewardArea : RewardArea
{
    public GameObject mainPanel;
    public GameObject dishImage;
    
    public override void OnShow(Action callback = null)
    {
        SortRewardFlyObjects(rewardFlyObjects);

        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            rewardFlyObjects[i].CachedTransform.localPosition = GetRewardLocalPosition(i);

            int index = i;
            float delayTime = i * 0.2f;

            if (i == rewardFlyObjects.Count - 1)
            {
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                {
                    rewardFlyObjects[index].OnShow();
                });

                GameManager.Task.AddDelayTriggerTask(delayTime + 0.27f, () =>
                {
                    callback?.Invoke();
                });
            }
            else
            {
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                {
                    rewardFlyObjects[index].OnShow();
                });
            }
        }

        onRewardAreaShow?.Invoke();
        onRewardAreaShow = null;
    }

    protected override string GetFlyRewardName(TotalItemData type)
    {
        if (type == TotalItemData.KitchenKey)
        {
            return "HarvestKitchenKeyFlyReward";
        }
        
        return base.GetFlyRewardName(type);
    }

    protected override Vector3 GetRewardLocalPosition(int index)
    {
        if (rewardFlyObjects.Count == 4)
        {
            return new Vector3(-390 + index * 260, 0);
        }
        else
        {
            return base.GetRewardLocalPosition(index);   
        }
    }

    public override void ShowGetRewardAnim(Action onGetRewardComplete)
    {
        if (isRleased)
        {
            onGetRewardComplete?.Invoke();
            return;
        }
        
        StartCoroutine(ShowKitchenGetRewardAnimCor(() =>
        {
            // var stageArea = HarvestKitchenManager.Instance.GetStageArea();
            // var chest = stageArea.GetCurChest();
            // if (chest != null)
            // {
            //     Transform cachedTransform = dishImage.transform;
            //     Vector3 startPos = cachedTransform.position;
            //     Vector3 targetPos = chest.key.transform.position;
            //     Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
            //     cachedTransform.DOMove(backPos, 0.15f).SetEase(Ease.OutSine).onComplete = () =>
            //     {
            //         cachedTransform.DOMove(targetPos, 0.33f).SetEase(Ease.InCubic);
            //         cachedTransform.DOScale(0.5f, 0.29f).SetEase(Ease.InCubic).onComplete = () =>
            //         {
            //             mainPanel.SetActive(false);
            //             onGetRewardComplete?.Invoke();
            //             stageArea.stageSliderKeyEffect.transform.position = targetPos;
            //             stageArea.stageSliderKeyEffect.AnimationState.SetAnimation(0, "Key1", false);
            //             stageArea.stageSliderKeyEffect.gameObject.SetActive(true);
            //             chest.ShowIncreaseProgressAnim();
            //         };
            //     };
            // }
            // else
            // {
            //     onGetRewardComplete?.Invoke();   
            // }
            mainPanel.SetActive(false);
            onGetRewardComplete?.Invoke();   
        }));
    }
    
    IEnumerator ShowKitchenGetRewardAnimCor(Action onGetRewardComplete)
    {
        Vector3 rewardFlyPosition = Vector3.down * 2;
        FlyReward specialFlyReward = null;

        float keyWaitTime = 0f;
        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            var flyer = rewardFlyObjects[i];
            TotalItemData rewardType = rewardFlyObjects[i].RewardType;
            if (rewardType == TotalItemData.Coin)
            {
                try
                {
                    if (RewardManager.Instance.CoinFlyReceiver != null)
                    {
                        RewardManager.Instance.CoinFlyReceiver.Show();
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, RewardManager.Instance.CoinFlyReceiver.CoinFlyTargetPos));
                    }
                    else
                    {
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, new Vector3(-1, 1, 0)));
                    }
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                
                keyWaitTime = keyWaitTime > 2f ? keyWaitTime : 2f;
                
                yield return new WaitForSeconds(0.1f);
            }
            else if (rewardType == TotalItemData.Life || rewardType == TotalItemData.InfiniteLifeTime)
            {
                try
                {
                    if (RewardManager.Instance.LifeFlyReceiver != null)
                    {
                        RewardManager.Instance.LifeFlyReceiver.Show();
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, RewardManager.Instance.LifeFlyReceiver.LifeFlyTargetPos));
                    }
                    else
                    {
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, new Vector3(-1, 1, 0)));
                    }
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                
                keyWaitTime = keyWaitTime > 0.85f ? keyWaitTime : 0.85f;
                
                yield return new WaitForSeconds(0.1f);
            }
            else if (rewardType.TotalItemType == TotalItemType.Item_BgID ||
                rewardType.TotalItemType == TotalItemType.Item_TileID)
            {
                specialFlyReward = flyer;

                try
                {
                    StartCoroutine(flyer.ShowGetRewardAnim(rewardType, Vector3.zero));
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                yield return new WaitForSeconds(0.1f);
            }
            else if (rewardType.TotalItemType == TotalItemType.KitchenKey)
            {
                yield return new WaitForSeconds(keyWaitTime);
                
                StartCoroutine(flyer.ShowGetRewardAnim(rewardType, Vector3.zero));
            }
            else
            {
                try
                {
                    var receiver = RewardManager.Instance.GetReceiverByItemType(rewardType);
                    Vector3 targetPos = rewardFlyPosition;
                    if (receiver != null)
                    {
                        targetPos = receiver.GetItemTargetPos(rewardType);
                    }
                    StartCoroutine(flyer.ShowGetRewardAnim(rewardType, targetPos));
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }

                keyWaitTime = keyWaitTime > 0.45f ? keyWaitTime : 0.45f;
                
                yield return new WaitForSeconds(0.1f);
            }
        }

        bool isAnimFinish = false;
        while (!isAnimFinish)
        {
            isAnimFinish = true;
            for (int i = 0; i < rewardFlyObjects.Count; i++)
            {
                if (!rewardFlyObjects[i].GetRewardAnimFinish)
                {
                    isAnimFinish = false;
                    break;
                }
            }
            yield return null;
        }

        //请留意! 暂时没有考虑奖励中包含多个“特殊的需要额外展示流程的道具”(比如背景/棋子/头像)的情况
        //只展示最后一个
        if (specialFlyReward != null)
        {
            if (specialFlyReward.RewardType.TotalItemType == TotalItemType.Item_BgID ||
                specialFlyReward.RewardType.TotalItemType == TotalItemType.Item_TileID)
                GameManager.UI.ShowUIForm("GetBGOrTileItemPanel",(u) =>
                {
                    GetBGOrTileItemPanel panel = u as GetBGOrTileItemPanel;

                    GameManager.Sound.PlayAudio(SoundType.SFX_ShowBGPanel.ToString());
                    u.m_OnHideCompleteAction = () =>
                    {
                        onGetRewardComplete?.Invoke();
                    };
                    //这个SetClearBg是OnGetRewardComplete的一环 但是我们需要提前触发 来避免GetBGOrTileItemPanel点不到
                    if (RewardManager.Instance.RewardPanel != null)
                        RewardManager.Instance.RewardPanel.SetClearBgActive(false);
                    OnHide();
                }, userData: specialFlyReward );
        }
        else
        {
            onGetRewardComplete?.Invoke();
            OnHide();
        }
    }
}
