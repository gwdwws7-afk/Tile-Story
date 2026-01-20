using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// 默认飞行奖励
/// </summary>
public sealed class DefaultFlyReward : FlyReward
{
    public override string Name => "DefaultFlyReward";

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;
    public GameObject infiniteIcon;
    public GameObject numberChangeEffect;


    private AsyncOperationHandle asyncHandle;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        InitRewardImage();

        ShowRewardBgEffect();

        if (autoGetReward && (rewardType == TotalItemData.MagnifierBoost || rewardType == TotalItemData.Prop_AddOneStep || rewardType == TotalItemData.FireworkBoost)) 
        {
            HideRewardBgEffect();
        }
    }

    private void InitRewardImage()
    {
        SetRewardImagePerfectSize();

        string spriteKey = UnityUtility.GetRewardSpriteKey(rewardType, rewardNum);
        asyncHandle = UnityUtility.LoadGeneralSpriteAsync(spriteKey, sp =>
        {
            rewardImage.sprite = sp;
        });
        //asyncHandle.WaitForCompletion();
        //rewardImage.sprite = (Sprite)asyncHandle.Result;
    }

    public override void OnReset()
    {
        base.OnReset();

        numberChangeEffect.SetActive(false);
        UnityUtility.UnloadAssetAsync(asyncHandle);
        asyncHandle = default;
    }

    public override void OnRelease()
    {
        base.OnRelease();

        UnityUtility.UnloadAssetAsync(asyncHandle);
        asyncHandle = default;
    }

    public override void OnShow(Action callback = null)
    {
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
                        try
                        {
                            callback?.Invoke();
                        }
                        catch (Exception e)
                        {
                            OnHide();
                            Debug.LogError(e.Message);
                        }
                    };
                };
          };
    }

    public override void RefreshAmountText()
    {
        numText.transform.localPosition = new Vector3(0f, -150.0f, 0);
        infiniteIcon.SetActive(false);

        numText.SetItemText(rewardNum, rewardType, false);

        if (rewardType == TotalItemData.RemoveAds)
            numText.gameObject.SetActive(false);
        else
            numText.gameObject.SetActive(true);
    }

    public override void DoubleRefreshAmountText()
    {
        numText.transform.localPosition = new Vector3(0f, -150.0f, 0);
        infiniteIcon.SetActive(false);

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

    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        yield return null;

        if (cachedTransform == null)
        {
            OnHide();
            yield break;
        }

        Vector3 startPos = new Vector3(cachedTransform.position.x, cachedTransform.position.y, 0);
        Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
        Vector3 startScale = cachedTransform.localScale;

        GameManager.Task.AddDelayTriggerTask(0.54f, () =>
        {
            getRewardAnimFinish = true;
        });

        cachedTransform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
        {
            cachedTransform.DOMove(targetPos, 0.36f).SetEase(Ease.InCubic);
            cachedTransform.DOScale(0.5f, 0.34f).SetEase(Ease.InCubic).onComplete = () =>
            {
                try
                {
                    OnHide();
                    cachedTransform.localScale = Vector3.one;
                    getRewardAnimFinish = true;
                    var receiver = RewardManager.Instance.GetReceiverByItemType(type);
                    receiver?.OnFlyEnd(type);
                    GameManager.Sound.PlayAudio("SFX_itemget");
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
            };
        };
    }

    private void SetRewardImagePerfectSize()
    {
        if (rewardType == TotalItemData.Coin)
        {
            rewardImage.transform.localPosition = new Vector3(-1.4f, 47f, 0f);
            rewardImage.transform.localScale = Vector3.one;
        }
        else if (rewardType == TotalItemData.Prop_AddOneStep)
        {
            rewardImage.transform.localPosition = new Vector3(4f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(260, 260);
            rewardImage.transform.localScale = Vector3.one;
        }
        else if (rewardType == TotalItemData.Prop_Grab)
        {
            rewardImage.transform.localPosition = new Vector3(4f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(260, 260);
            rewardImage.transform.localScale = Vector3.one;
        }
        else if (rewardType == TotalItemData.Star || rewardType == TotalItemData.RemoveAds) 
        {
            rewardImage.transform.localPosition = new Vector3(0f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(310, 310);
            rewardImage.transform.localScale = Vector3.one;
        }
        else
        {
            rewardImage.transform.localPosition = new Vector3(0f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(260, 260);
            rewardImage.transform.localScale = Vector3.one;
        }
    }
}
