using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TransparentRewardPanel : RewardPanel
{
    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        gameObject.SetActive(true);
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
