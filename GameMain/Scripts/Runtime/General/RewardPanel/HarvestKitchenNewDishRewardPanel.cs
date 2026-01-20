using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;

public class HarvestKitchenNewDishRewardPanel : RewardPanel
{
    public Transform dishRoot;
    public UIParticle dishBgEffect;
    public Transform rewardText;
    public RewardArea rewardArea;
    public TextMeshProUGUILocalize dishNameText;
    
    public override RewardArea CustomRewardArea => rewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();

        blackBg.OnShow();
        
        Transform titleTrans = titleText.transform;
        Transform tipTrans = tipText.transform;
        
        titleTrans.localScale = Vector3.zero;
        dishNameText.transform.localScale = Vector3.zero;
        tipTrans.localScale = Vector3.zero;
        rewardText.localScale=Vector3.zero;
        dishRoot.transform.localScale = Vector3.one;
        dishNameText.SetTerm("HarvestKitchen.Dish" + (HarvestKitchenManager.Instance.TaskId - 1).ToString());

        gameObject.SetActive(true);
        
        titleText.gameObject.SetActive(true);
        titleTrans.DOScale(1.1f, 0.2f).SetDelay(0.1f).onComplete = () =>
        {
            titleTrans.DOScale(1f, 0.2f);
        };
        
        dishNameText.transform.DOScale(1.1f, 0.2f).SetDelay(0.1f).onComplete = () =>
        {
            dishNameText.transform.DOScale(1f, 0.2f);
        };

        rewardText.DOScale(1.1f, 0.2f).SetDelay(0.3f).onComplete = () =>
        {
            rewardText.DOScale(1f, 0.2f);
        };
                
        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            rewardArea.OnShow(() =>
            {
                if (!autoGetReward)
                {
                    tipText.gameObject.SetActive(true);
                    tipTrans.DOScale(1.1f, 0.2f).onComplete = () =>
                    {
                        tipTrans.DOScale(1f, 0.2f);
                    };
                }

                onShowComplete?.Invoke();
            });
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        dishNameText.transform.localScale = Vector3.zero;
        dishRoot.transform.localScale = Vector3.zero;
        dishBgEffect.gameObject.SetActive(false);
        rewardText.localScale = Vector3.zero;
        
        base.OnHide(quickHide, onHideComplete);
    }
}
