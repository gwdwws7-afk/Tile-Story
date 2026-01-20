using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenEndMenu : UIForm
{
    public DelayButton closeButton;
    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButton.SetBtnEvent(OnCloseBtnClick);
        
        base.OnInit(uiGroup, completeAction, userData);
    }
    
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;
                };
            };
        }

        if (bgImage != null)
        {
            bgImage.DOKill();
            bgImage.DOColor(new Color(1,1,1,0.01f),0).OnComplete(()=> 
            {
                bgImage.DOFade(1, 0.2f);
            });
        }
        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();

        if (cachedTransform) cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(false);

        base.OnHide(hideSuccessAction, userData);
    }

    public void OnCloseBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        // 是否存在未领取的奖励，尝试补发奖励
        Dictionary<TotalItemData, int> rewardDict = HarvestKitchenManager.Instance.GetCanClaimmReward();
        if (rewardDict != null && rewardDict.Count > 0)
        {
            foreach (var reward in rewardDict)
            {
                RewardManager.Instance.AddNeedGetReward(reward.Key, reward.Value);
            }

            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                GameManager.Process.EndProcess(ProcessType.CheckHarvestKitchen);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.CheckHarvestKitchen);
        }
    }
}
