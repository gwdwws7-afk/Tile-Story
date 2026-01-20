using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardTransparentRewardPanel : RewardPanel
{
    public RewardArea rewardArea;

    public override RewardArea CustomRewardArea => rewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        
        rewardArea.OnShow(() =>
        {
            onShowComplete?.Invoke();
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        onHideComplete?.Invoke();
    }
    
    public override void SetOnClickEvent(UnityAction onClick)
    {

    }

    public override void ClearOnClickEvent()
    {
        
    }
}
