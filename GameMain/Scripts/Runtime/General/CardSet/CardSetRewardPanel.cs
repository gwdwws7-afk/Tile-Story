using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardSetRewardPanel : RewardPanel
{
    public Transform[] transList;
    public Transform cardSetItem;
    public Image cardSetCover, cardSetBanner;
    public TextMeshProUGUILocalize cardSetName;
    public UIParticle bgEffect, ribbon1, ribbon2;
    public CardSetRewardArea rewardArea;
    
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    
    public override RewardArea CustomRewardArea => rewardArea;

    public override void OnInit(bool autoGetReward)
    {
        autoGetReward = false;
        base.OnInit(autoGetReward);
        
        string coverStr = $"Card.{CardModel.Instance.CardActivityID}_{CardModel.Instance.CurrentCardSet}";
        cardSetCover.sprite = AddressableUtils.LoadAsset<Sprite>(coverStr);
        
        string bannerStr = $"CommonAtlas{CardModel.Instance.CardActivityID}[条幅{CardModel.Instance.CurrentCardSet % 5}]";
        cardSetBanner.sprite = AddressableUtils.LoadAsset<Sprite>(bannerStr);
        
        cardSetName.SetTerm($"Card.{CardModel.Instance.CardActivityID}_{CardModel.Instance.CurrentCardSet}");
        
        foreach (var trans in transList)
        {
            trans.localScale = Vector3.zero;
        }
        tipBg.transform.localScale = Vector3.zero;
        
        ribbon1.Stop();
        ribbon2.Stop();
    }

    public override void OnRelease()
    {
        AddressableUtils.ReleaseAsset(cardSetCover.sprite);
        AddressableUtils.ReleaseAsset(cardSetBanner.sprite);
        _assetHandleList.Clear();
        
        base.OnRelease();
    }

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        
        cardSetItem.localPosition = new Vector3(-254, 706);
        cardSetItem.localScale = Vector3.one;
        cardSetItem.DOLocalJump(new Vector3(0, 235), 300f, 1, 0.3f).SetEase(Ease.InCubic);
        cardSetItem.DOScale(1.6f, 0.3f).SetEase(Ease.InCubic).onComplete += () =>
        {
            bgEffect.gameObject.SetActive(true);
            bgEffect.Play();
            UnityUtil.EVibatorType.Medium.PlayerVibrator();
            
            blackBg.OnShow(0.2f, false);
        };

        float delayTime = 0.3f;
        for (int i = 0; i < transList.Length; i++)
        {
            var index = i;
            delayTime += 0.2f;
            transList[i].DOScale(1.1f, 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                transList[index].DOScale(1f, 0.2f).onComplete = () =>
                {
                    if (index == transList.Length - 1)
                    {
                        ribbon1.Play();
                        ribbon2.Play();
                        
                        rewardArea.OnShow(() =>
                        {
                            tipBg.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                            {
                                tipBg.transform.DOScale(1f, 0.2f);
                                blackBg.clickButton.interactable = true;
                            };
                            
                            onShowComplete?.InvokeSafely();
                        });
                    }
                };
            };
        }
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        onHideComplete?.InvokeSafely();

        GameManager.Task.AddDelayTriggerTask(quickHide ? 0f : 0.6f, () =>
        {
            blackBg.OnHide(quickHide ? 0f : 0.2f);
            
            cardSetItem.DOScale(0f, quickHide ? 0f : 0.2f).onComplete += () =>
            {
                bgEffect.gameObject.SetActive(false);
            };

            foreach (var trans in transList)
            {
                trans.DOScale(0f, quickHide ? 0f : 0.2f);
            }

            tipBg.transform.DOScale(0f, quickHide ? 0f : 0.2f); //.onComplete += () => onHideComplete?.InvokeSafely();
        });
    }
    
    
    
    // public override void SetOnClickEvent(UnityAction onClick)
    // {
    //     blackBg.clickButton.onClick.RemoveAllListeners();
    //     blackBg.clickButton.onClick.AddListener(GetReward);
    // }

    private void GetReward()
    {
        Debug.LogError("GetReward");
        blackBg.clickButton.onClick.RemoveAllListeners();
        OnHide(false, OnHideComplete);
        return;

        void OnHideComplete()
        {
            GameManager.PlayerData.SyncAllItemData();
            
            float delayTime = -0.2f;
            foreach (var reward in CustomRewardArea.GetComponentsInChildren<FlyReward>())
            {
                if (reward.RewardType == TotalItemData.Coin)
                    GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(reward.RewardNum, null));
                
                delayTime += 0.2f;
                reward.transform.DOLocalMoveY(50, 0.2f).SetDelay(delayTime);
                CanvasGroup canvasGroup = reward.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1;
                canvasGroup.DOFade(0, 0.2f).SetDelay(delayTime).onComplete += () =>
                {
                    reward.gameObject.SetActive(false);
                    Destroy(canvasGroup);
                };
            }

            GameManager.Task.AddDelayTriggerTask(delayTime + 0.5f, () =>
            {
                RewardManager.Instance.OnReset();
                SetClearBgActive(false);
            });
        }
    }
}
