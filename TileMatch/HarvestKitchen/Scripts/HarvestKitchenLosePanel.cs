using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenLosePanel : BaseGameFailPanel
{
    public Image propImage, crossImage;
    
    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.HarvestKitchenLosePanel;

    public override bool IsShowFailPanel => HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget();

    public override void ShowFailPanel(Action finishAction)
    {
        propImage.transform.localScale = Vector3.zero;
        crossImage.transform.localScale = Vector3.zero;
        
        propImage.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        crossImage.transform.DOScale(0.75f, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f);
    }
}
