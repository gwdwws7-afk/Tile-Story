using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class MergeLosePanel : BaseGameFailPanel
{
    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.MergeLosePanel;

    public override bool IsShowFailPanel => Merge.MergeManager.Instance != null && Merge.MergeManager.Instance.CheckLevelWinCanGetTarget();

    public Image propImage, crossImage;
    public TextMeshProUGUILocalize loseText;

    private AsyncOperationHandle spriteHandle;

    public override void ShowFailPanel(Action finishAction)
    {
        propImage.transform.localScale = Vector3.zero;
        crossImage.transform.localScale = Vector3.zero;

        UnityUtility.UnloadAssetAsync(spriteHandle);
        spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(Merge.MergeManager.Instance.GetMergeEnergyBoxName(), "TotalItemAtlas"), sp =>
        {
            propImage.sprite = sp;
            propImage.SetNativeSize();

            propImage.transform.DOScale(1.7f, 0.2f).SetEase(Ease.OutBack);
            crossImage.transform.DOScale(0.75f, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f);
        });

        loseText.SetTerm(Merge.MergeManager.Instance.GetMergeLoseTextName());
    }

    public override void CloseFailPanel(Action finishAction)
    {
        base.CloseFailPanel(finishAction);

        UnityUtility.UnloadAssetAsync(spriteHandle);
        spriteHandle = default;
    }
}
