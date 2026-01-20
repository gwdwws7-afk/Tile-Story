using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardPackFlyReward : FlyReward
{
    public override string Name => "CardPackFlyReward";

    public Image rewardImage;
    // public SkeletonGraphic pack;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;
    public GameObject numberChangeEffect;
    
    private AsyncOperationHandle _asyncHandle;
    private int _packType;
    private Vector3 _pos;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);
        
        SetImage();
        numText.gameObject.SetActive(true);
        ShowRewardBgEffect();
    }
    
    private void SetImage()
    {
        switch (rewardType.TotalItemType)
        {
            case TotalItemType.CardPack1:
                _packType = 1;
                break;
            case TotalItemType.CardPack2:
                _packType = 2;
                break;
            case TotalItemType.CardPack3:
                _packType = 3;
                break;
            case TotalItemType.CardPack4:
                _packType = 4;
                break;
            case TotalItemType.CardPack5:
                _packType = 5;
                break;
        }
        _asyncHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetSpriteKey($"CardPack{_packType}",Constant.ResourceConfig.TotalItemAtlas), sp =>
        {
            rewardImage.sprite = sp as Sprite;
        });
        
        // pack.Skeleton.SetSkin($"cardbag{_packType}");
        // pack.Skeleton.SetSlotsToSetupPose();;
        // pack.AnimationState.TimeScale = 0;
        // pack.AnimationState.ClearTracks();
        // pack.AnimationState.SetAnimation(0, "appear", false);
        // pack.Update(0);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        if (numberChangeEffect != null)
        {
            numberChangeEffect.SetActive(false);   
        }
        UnityUtility.UnloadAssetAsync(_asyncHandle);
        _asyncHandle = default;
    }

    public override void OnShow(Action callback = null)
    {
        if (RewardManager.Instance.RewardPanel.RewardPanelType == RewardPanelType.CardTransparentRewardPanel)
        {
            gameObject.SetActive(true);
            callback?.InvokeSafely();
            return;
        }
        
        Transform trans = body;
        float originalScale = trans.localScale.x;

        trans.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);

        gameObject.SetActive(true);

        trans.DOScale(originalScale * 1.1f, 0.15f).onComplete = () =>
        {
            trans.DOScale(originalScale * 0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
            {
                trans.DOScale(originalScale, 0.15f).onComplete = () =>
                {
                    callback?.InvokeSafely();
                };
            };
        };
    }
    
    public override void RefreshAmountText()
    {
        numText.transform.localPosition = new Vector3(0f, -150.0f, 0);
        numText.SetItemText(rewardNum, rewardType, false);
    }
    
    public override void DoubleRefreshAmountText()
    {
        numText.transform.localPosition = new Vector3(0f, -150.0f, 0);

        numText.transform.DOScale(1.6f, 0.2f).SetDelay(0.2f).onComplete = () =>
        {
            numText.SetItemText(rewardNum, rewardType, false);
            numText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        };

        numberChangeEffect.SetActive(true);
    }
    
    public override void ShowRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(true);
        }
    }

    public override void HideRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(false);
        }
    }

    private void ResetToInitialState()
    {
        gameObject.SetActive(false);
        
        // pack.AnimationState.TimeScale = 0;
        // pack.AnimationState.ClearTracks();
        // pack.AnimationState.SetAnimation(0, "appear", false);
        // pack.Update(0);

        if (cachedTransform != null)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.position = _pos;   
        }
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type, Vector3 targetPos)
    {
        numText.gameObject.SetActive(false);
        numberChangeEffect.SetActive(false);
        HideRewardBgEffect();
        _pos = cachedTransform.position;
        
        for (int i = 0; i < rewardNum; i++)
        {
            GetCardPackPanel panel = null;
            bool getCardComplete = false;
            GameManager.UI.ShowUIForm("GetCardPackPanel", userData: _packType, showSuccessAction: form =>
            {
                panel = form as GetCardPackPanel;
                panel?.SetCompleteAction(() => { getCardComplete = true; });
            });

            GameManager.Sound.PlayAudio("Card_Collection_Show_Card_Pack");
            gameObject.SetActive(true);
            //pack.AnimationState.TimeScale = 1;
            cachedTransform.DOScale(1.5f / body.localScale.x, 0.3f);
            cachedTransform.DOMove(Vector3.zero, 0.3f);
            yield return new WaitForSeconds(0.3f);
            
            yield return new WaitUntil(() => panel != null);
            ResetToInitialState();
            panel.ShowPackAnim();
            
            yield return new WaitUntil(() => getCardComplete);
        }
        OnHide();
        getRewardAnimFinish = true;
    }
}
