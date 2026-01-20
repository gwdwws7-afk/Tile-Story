using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StartCardPackRewardPanel : RewardPanel
{
    public Transform[] transList;

    public override void OnInit(bool autoGetReward)
    {
        autoGetReward = false;
        
        foreach (var trans in transList)
        {
            trans.localScale = Vector3.zero;
        }
        tipBg.transform.localScale = Vector3.zero;
        
        base.OnInit(autoGetReward);
    }

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(0.2f, false);

        float delayTime = -0.2f;
        for (int i = 0; i < transList.Length; i++)
        {
            var index = i;
            delayTime += 0.2f;
            transList[i].DOScale(1.1f, 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                transList[index].DOScale(1f, 0.2f).onComplete = () =>
                {
                    if (index == transList.Length - 1)
                    {
                        rewardArea.OnShow(() =>
                        {
                            tipBg.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                            {
                                tipBg.transform.DOScale(1f, 0.2f);
                                blackBg.clickButton.interactable = true;
                            };
                            
                            onShowComplete?.InvokeSafely();
                        });
                    }
                };
            };
        }
    }
    
    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        onHideComplete?.InvokeSafely();

        GameManager.Task.AddDelayTriggerTask(quickHide ? 0f : 0.2f, () =>
        {
            blackBg.OnHide(quickHide ? 0f : 0.2f);

            foreach (var trans in transList)
            {
                trans.DOScale(0f, quickHide ? 0f : 0.2f);
            }

            tipBg.transform.DOScale(0f, quickHide ? 0f : 0.2f); //.onComplete += () => onHideComplete?.InvokeSafely();
        });
    }
}
