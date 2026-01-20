using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class KitchenLosePanel : BaseGameFailPanel
{
    public Image propImage, crossImage;
    
    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.KitchenLosePanel;

    public override bool IsShowFailPanel => KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget();

    public override void ShowFailPanel(Action finishAction)
    {
        propImage.transform.localScale = Vector3.zero;
        crossImage.transform.localScale = Vector3.zero;
        
        propImage.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        crossImage.transform.DOScale(0.75f, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f);
    }
}
