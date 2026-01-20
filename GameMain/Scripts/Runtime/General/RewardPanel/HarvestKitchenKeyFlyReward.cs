using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HarvestKitchenKeyFlyReward : FlyReward
{
    public override string Name => "HarvestKitchenKeyFlyReward";

    public override int SortPriority => 3;

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;
    public GameObject trailEffect;
    public GameObject trail;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        ShowRewardBgEffect();
    }

    public override void OnShow(Action callback = null)
    {
        Transform trans = body;
        float originalScale = trans.localScale.x;

        trans.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);

        trailEffect.SetActive(false);
        trail.SetActive(true);
        rewardImage.enabled = true;
        numText.gameObject.SetActive(true);
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

        var stageArea = HarvestKitchenManager.Instance.GetStageArea();
        var chest = stageArea.GetCurChest();
        if (chest != null)
        {
            Transform KeyTrans = cachedTransform;
            //Vector3 startPos = KeyTrans.position;
            targetPos = chest.key.transform.position;
            //Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
            
            GameManager.Task.AddDelayTriggerTask(0.8f, () =>
            {
                getRewardAnimFinish = true;
            });

            HideRewardBgEffect();
            
            trailEffect.SetActive(true);
            numText.gameObject.SetActive(false);
            KeyTrans.DOJump(targetPos, 0.2f,1,0.32f).SetEase(Ease.InQuad);
            KeyTrans.DOScale(0.5f, 0.31f).SetEase(Ease.InQuad).onComplete = () =>
            {
                GameManager.Sound.PlayAudio("SFX_itemget");
                rewardImage.enabled = false;
                //numText.gameObject.SetActive(false);
                var particles = trailEffect.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    particle.Stop();
                }
                // GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                // {
                //     OnHide();
                // });
                //cachedTransform.localScale = Vector3.one;
                stageArea.stageSliderKeyEffect.transform.position = targetPos;
                stageArea.stageSliderKeyEffect.AnimationState.SetAnimation(0, "Key1", false);
                stageArea.stageSliderKeyEffect.gameObject.SetActive(true);
                UnityUtil.EVibatorType.VeryShort.PlayerVibrator();

                trail.SetActive(false);
                
                GameManager.Task.AddDelayTriggerTask(0.3f, () =>
                {
                    chest.ShowIncreaseProgressAnim();
                    
                    OnHide();
                    
                    getRewardAnimFinish = true;
                });
            };
        }
        else
        {
            OnHide();
            getRewardAnimFinish = true;
        }
    }
}
