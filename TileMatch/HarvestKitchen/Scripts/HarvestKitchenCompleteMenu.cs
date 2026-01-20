using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenCompleteMenu : PopupMenuForm
{
    public SkeletonGraphic anim;
    public GameObject[] effects;
    public Transform awesomeText, tapText;
    public Button closeButton;
    
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseButtonClick);
        GameManager.Task.AddDelayTriggerTask(1.3f,() =>
        {
            closeButton.interactable = true;
        });
        
        anim.AnimationState.SetAnimation(0, "chuxian", false).Complete += t =>
        {
            anim.AnimationState.SetAnimation(0, "loop", true);
        };

        foreach (var effect in effects)
        {
            effect.SetActive(false);
        }
        GameManager.Task.AddDelayTriggerTask(0.6f, () =>
        {
            foreach (var effect in effects)
            {
                effect.SetActive(true);
            }
        });

        awesomeText.localScale = Vector3.zero;
        tapText.localScale = Vector3.zero;
        awesomeText.DOScale(1.1f, 0.2f).SetDelay(1f).onComplete = () =>
        {
            awesomeText.DOScale(1f, 0.2f);
        };
        tapText.DOScale(1.1f, 0.2f).SetDelay(1.4f).onComplete = () =>
        {
            tapText.DOScale(1f, 0.2f);
        };
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
        
        UnityUtil.EVibatorType.Short.PlayerVibrator();
    }
}
