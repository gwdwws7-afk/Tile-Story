using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class HarvestKitchenStageArea : MonoBehaviour
{
    public HarvestKitchenStageChest[] chests;
    public Transform curStageBanner;
    public SkeletonGraphic stageSliderFullEffect;
    public SkeletonGraphic stageSliderKeyEffect;
    
    private DTHarvestKitchenChestDatas curChestData;
    
    public void Init()
    {
        curChestData = HarvestKitchenManager.Instance.GetCurrentChestData();
        
        List<DTHarvestKitchenChestDatas> chestDatas = HarvestKitchenManager.Instance.ChestData.KitchenChestDatas;
        for (int i = 0; i < chests.Length; i++)
        {
            chests[i].Init(chestDatas[i], curChestData.ID);
        }

        UpdateLayoutAnimated(false);
        RefreshChest();
    }

    public void RefreshChest(Action callback = null)
    {
        if (curChestData != null) 
        {
            int finishedDishNum = HarvestKitchenManager.Instance.TaskId - 1;
            int neededDishNum = 0;
            List<DTHarvestKitchenChestDatas> chestDatas = HarvestKitchenManager.Instance.ChestData.KitchenChestDatas;
            for (int i = 0; i < curChestData.ID; i++)
            {
                neededDishNum += chestDatas[i].TargetDishNum;
            }

            //当前阶段宝箱达成数量
            if (finishedDishNum >= neededDishNum)
            {
                int curChestID = curChestData.ID;
                for (int i = 0; i < curChestData.RewardsList.Count; i++)
                {
                    RewardManager.Instance.AddNeedGetReward(curChestData.RewardsList[i], curChestData.RewardsNumList[i]);
                }
                
                HarvestKitchenManager.Instance.ChestId += 1;

                var targetChest = chests[curChestID - 1];
                GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                {
                    if (targetChest == null)
                        return;
                    stageSliderFullEffect.transform.SetParent(targetChest.sliderBorder.transform);
                    stageSliderFullEffect.transform.position = targetChest.slider.transform.position;
                    stageSliderFullEffect.gameObject.SetActive(true);
                    stageSliderFullEffect.AnimationState.SetAnimation(0, "Progressbar_effects1", false);
                });

                GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                {
                    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.HarvestKitchenChestRewardPanel, false, () =>
                    {
                        if (targetChest == null)
                            return;
                        
                        targetChest.ShowOpenChestAnim();
                        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                        {
                            curChestData = HarvestKitchenManager.Instance.GetCurrentChestData();
                            UpdateLayoutAnimated(true);

                            GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                            {
                                callback?.Invoke();
                            });

                            if (curChestData == null)
                            {
                                GameManager.UI.ShowUIForm("HarvestKitchenCompleteMenu", UIFormType.PopupUI, form =>
                                {
                                    SetFinishedLayout();
                                    
                                    UnityUtil.EVibatorType.Medium.PlayerVibrator();
                                });
                            }
                            else
                            {
                                UnityUtil.EVibatorType.Medium.PlayerVibrator();
                            }
                        });
                    }, () =>
                    {
                        targetChest.chestCloseImg.gameObject.SetActive(false);
                        targetChest.chestOpenImg.gameObject.SetActive(true);
                    });
                });
                
                UnityUtil.EVibatorType.Medium.PlayerVibrator();
            }
            else
            {
                callback?.Invoke();
            }
        }
        else
        {
            callback?.Invoke();
        }
    }
    
    public HarvestKitchenStageChest GetCurChest()
    {
        if (curChestData != null)
        {
            return chests[curChestData.ID - 1];
        }

        return null;
    }

    public void SetFinishedLayout()
    {
        if (curStageBanner != null)
        {
            float posX = -320;
            for (int i = 0; i < chests.Length; i++)
            {
                chests[i].transform.localPosition = new Vector2(posX, 0);
                chests[i].chest.transform.localScale = Vector3.one * 0.8f;
                chests[i].slider.transform.localScale = Vector3.one * 0.8f;
                chests[i].tick.transform.localScale = Vector3.one * 0.8f;
                
                posX += 160;
            }   
        }
    }
    
    public void UpdateLayoutAnimated(bool showAnim)
    {
        if (curChestData == null) 
        {
            curStageBanner.gameObject.SetActive(false);
            return;
        }
        
        float areaW = 800;
        float baseWidth = 135;
        float selectedWidth = 260;
        
        float[] targetWidths = new float[chests.Length];
        for (int i = 0; i < chests.Length; i++)
        {
            bool sel = (i == curChestData.ID - 1);
            targetWidths[i] = sel ? selectedWidth : baseWidth;
        }

        float startX = -areaW / 2f + targetWidths[0] / 2f;
        for (int i = 0; i < chests.Length; i++)
        {
            var item = chests[i].transform;
            if (item == null) continue;

            float x = startX;
            Vector2 targetPos = new Vector2(x, 0);
            float scale = (i == curChestData.ID - 1) ? 1.1f : 0.8f;
            
            if (showAnim)
            {
                item.DOLocalMove(targetPos, 0.4f).SetEase(Ease.Linear);
                chests[i].chest.transform.DOScale(scale, 0.4f).SetEase(Ease.Linear);  
                chests[i].slider.transform.DOScale(scale, 0.4f).SetEase(Ease.Linear);  
                chests[i].tick.transform.DOScale(scale, 0.4f).SetEase(Ease.Linear);

                if (i == curChestData.ID - 1)
                {
                    curStageBanner.DOLocalMoveX(targetPos.x + 5f, 0.4f).onComplete = () =>
                    {
                        curStageBanner.DOLocalMoveX(targetPos.x, 0.2f);
                    };   
                }
            }
            else
            {
                item.localPosition = targetPos;
                chests[i].chest.transform.localScale = Vector3.one * scale;
                chests[i].slider.transform.localScale = Vector3.one * scale;
                chests[i].tick.transform.localScale = Vector3.one * scale;

                if (i == curChestData.ID - 1)
                {
                    curStageBanner.localPosition = new Vector3(targetPos.x, curStageBanner.position.y, 0);   
                }
            }

            if (i + 1 < chests.Length) 
                startX += targetWidths[i] / 2f + targetWidths[i + 1] / 2f;
        }
        curStageBanner.gameObject.SetActive(true);
    }
}
