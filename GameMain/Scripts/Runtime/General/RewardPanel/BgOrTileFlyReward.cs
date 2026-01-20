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
/// 背景或棋子类型飞行奖励
/// </summary>
public sealed class BgOrTileFlyReward : FlyReward
{
    public override string Name => "BgOrTileFlyReward";

    public override int SortPriority => 1;

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;
    public GameObject infiniteIcon;
    public GameObject bgMask;


    private AsyncOperationHandle asyncHandle;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        InitRewardImage();

        ShowRewardBgEffect();
    }

    private void InitRewardImage()
    {
        SetRewardImagePerfectSize();

        if (rewardType.TotalItemType == TotalItemType.Item_BgID && rewardType.RefID == 0) 
        {
            rewardImage.sprite = BGSmallUtil.GetSprite(rewardType.ID);
            rewardImage.SetNativeSize();
            bgMask.SetActive(true);
        }
        else
        {
            string spriteKey = UnityUtility.GetRewardSpriteKey(rewardType, rewardNum);
            asyncHandle = UnityUtility.LoadGeneralSpriteAsync(spriteKey, sp =>
            {
                rewardImage.sprite = sp;
            });
            bgMask.SetActive(false);
        }
    }

    public override void OnReset()
    {
        base.OnReset();
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

        numText.gameObject.SetActive(true);
        numText.SetItemText(rewardNum, rewardType, false);

        //可以不显示
        //numText.gameObject.SetActive(false);
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

    //飞到固定位置
    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        yield return null;

        GameManager.Task.AddDelayTriggerTask(0.54f, () =>
        {
            getRewardAnimFinish = true;
        });

        numText.gameObject.SetActive(false);

        targetPos = new Vector3(0, 139.0f, 0);

        cachedTransform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutQuart).onComplete = () =>
        {
            try
            {
                getRewardAnimFinish = true;
                GameManager.Sound.PlayAudio("SFX_itemget");
            }
            catch (Exception e)
            {
                OnHide();
                Debug.LogError(e.Message);
            }
        };
    }

    private void SetRewardImagePerfectSize()
    {
        if (rewardType.TotalItemType == TotalItemType.Item_BgID ||
            rewardType.TotalItemType == TotalItemType.Item_TileID ||
            rewardType.TotalItemType == TotalItemType.Item_PortraitID)
        {
            rewardImage.transform.localPosition = new Vector3(0f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(310, 310);
            rewardImage.transform.localScale = Vector3.one;
        }
        else
        {
            Log.Error($"Unexpected BgOrTileFlyReward Receive a rewardType.TotalItemType == {rewardType.TotalItemType}");
            rewardImage.transform.localPosition = new Vector3(20f, 42f, 0f);
            rewardImage.rectTransform.sizeDelta = new Vector2(310, 310);
            rewardImage.transform.localScale = Vector3.one;
        }
    }
}
