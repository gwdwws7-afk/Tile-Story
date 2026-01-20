using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

public class EndlessChestRewardPanel : RewardPanel
{
    [SerializeField] private DelayButton CloseBtn;
    [SerializeField] private RewardArea RewardArea;
    [SerializeField] private CanvasGroup[] CanvasGroups;

    public override RewardArea CustomRewardArea => RewardArea;
    
    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow();

        foreach (var group in CanvasGroups)
        {
            group.gameObject.SetActive(true);
            group.DOFade(1, 0.2f);
        }
        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;
            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;

            titleText.gameObject.SetActive(true);
            titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
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

                    onShowComplete?.InvokeSafely();
                });

                titleTrans.DOScale(1f, 0.2f);
            };
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        blackBg.OnHide(quickHide ? 0 : 0.2f);

        if (!quickHide)
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;

            titleTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                titleTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    titleText.gameObject.SetActive(false);
                    titleTrans.localScale = Vector3.one;
                };
            };

            tipTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                tipTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    tipText.gameObject.SetActive(false);
                    tipTrans.localScale = Vector3.one;

                    onHideComplete?.InvokeSafely();
                };
            };
        }
        else
        {
            titleText.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);

            onHideComplete?.InvokeSafely();
        }

        foreach (var group in CanvasGroups)
        {
            group.gameObject.SetActive(false);
        }
    }
    
    public override void SetOnClickEvent(UnityAction onClick)
    {
        blackBg.clickButton.onClick.RemoveAllListeners();
        blackBg.clickButton.onClick.AddListener(onClick);
        
        CloseBtn.SetBtnEvent(() =>
        {
            onClick?.Invoke();
        });
    }
}
