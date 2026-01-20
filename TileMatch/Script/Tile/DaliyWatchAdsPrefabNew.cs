using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using GameFramework.Event;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DaliyWatchAdsPrefabNew : MonoBehaviour
{
    [SerializeField] private DelayButton DaliyDelay_Btn;
    [SerializeField] private Image ProImage;
    [SerializeField] private Image Slider;
    
    private TotalItemType itemType;
    private Action<TotalItemType> itemAction =null;
    private TweenerCore<float, float, FloatOptions> tweenerCore;

    private AsyncOperationHandle _assetHandle;
    private void Awake()
    {
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, RewardLoad);
    }

    private void OnDestroy()
    {
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId,RewardAdEarned);
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, RewardLoad);
        tweenerCore.Kill();
        tweenerCore = null;
        UnityUtility.UnloadAssetAsync(_assetHandle);
        _assetHandle = default;
    }

    public void SetActive(bool isActive,TotalItemType itemType,Action<TotalItemType> itemAction)
    {
        this.itemType = itemType;
        this.itemAction = itemAction;
       
        if (isActive)
        {
            if (!gameObject.activeInHierarchy)
            {
                transform.DOKill(true);
                tweenerCore.Kill(true);
                tweenerCore = null;
                Slider.fillAmount = 1;
                transform.transform.localPosition = new Vector3(500,0,0);
                GameManager.Firebase.RecordMessageByEvent("Level_Time_Limited_Tool_ADs_Show");
                Init();
                gameObject.SetActive(true);
                transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.InOutSine).onComplete+= () =>
                {
                    tweenerCore= DOTween.To(()=>1f,t=>Slider.fillAmount=t,0f,10).SetEase(Ease.Linear);
                    tweenerCore.onComplete += () =>
                    {
                        transform.DOLocalMove(new Vector3(500,0,0), 0.4f).SetEase(Ease.InOutSine).onComplete+=() =>
                        {
                            gameObject.SetActive(false);
                        };
                    };
                };
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Init()
    {
        DaliyDelay_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.TodayWatchPropsAdTime += 1;
            GameManager.Ads.ShowRewardedAd("DaliyWatchAdsPrefabNew");
        });

        UnityUtility.UnloadAssetAsync(_assetHandle);
        _assetHandle = UnityUtility.LoadGeneralSpriteAsync(itemType.ToString(), sp =>
        {
            ProImage.sprite = sp;
        });
    }
    
    private void RewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.Equals("DaliyWatchAdsPrefabNew"))
        {
            GameManager.Firebase.RecordMessageByEvent("Level_Time_Limited_Tool_ADs_Watch");
            //触发回调
            itemAction?.InvokeSafely(itemType);
            SetActive(false,itemType,null);
        }
    }
    
    private void RewardLoad(object sender, GameEventArgs e)
    {
        if (!GameManager.Ads.CheckRewardedAdIsLoaded())
        {
            SetActive(false,TotalItemType.None,null);
        }
    }
}
