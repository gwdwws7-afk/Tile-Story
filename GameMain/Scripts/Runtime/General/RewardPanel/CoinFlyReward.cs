using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// 金币飞行奖励
/// </summary>
public sealed class CoinFlyReward : FlyReward
{
    public override string Name => "CoinFlyReward";

    public override int SortPriority => 10;

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public TextMeshProUGUI fadeText;
    public UIParticle rewardBgEffect;
    public Transform coinBody;

    private List<RotateCoin> rotateCoins = new List<RotateCoin>();

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        //InitRewardImage();

        transform.localScale = Vector3.one;
        coinBody.localScale = Vector3.one;

        if (RewardManager.Instance.RewardPanel != null)
            coinBody.gameObject.SetActive(true);
        else
            coinBody.gameObject.SetActive(false);

        if (RewardManager.Instance.RewardPanel != null)
            ShowRewardBgEffect();
    }

    public override void OnShow(Action callback = null)
    {
        gameObject.SetActive(true);
        if (RewardManager.Instance.RewardPanel != null)
        {
            coinBody.gameObject.SetActive(true);
            Transform trans = body;
            float originalScale = trans.localScale.x;

            trans.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);
            //numCanvas.alpha = 0;

            gameObject.SetActive(true);

            trans.DOScale(originalScale * 1.1f, 0.16f).onComplete = () =>
              {
                  trans.DOScale(originalScale * 0.95f, 0.2f).SetEase(Ease.InQuad).onComplete = () =>
                    {
                        trans.DOScale(originalScale, 0.2f).onComplete = () =>
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

            //numCanvas.DOFade(1, 0.3f);
        }
        else
        {
            HideRewardBgEffect();

            coinBody.gameObject.SetActive(false);
        }
    }

    public override void OnReset()
    {
        base.OnReset();

        StopAllCoroutines();

        fadeText.gameObject.SetActive(false);
        fadeText.DOKill();
        fadeText.transform.DOKill();

        for (int i = 0; i < rotateCoins.Count; i++)
        {
            if (rotateCoins[i] != null)
            {
                rotateCoins[i].OnReset();
                //GameManager.ObjectPool.Unspawn<RotateCoinObject>("RotateCoin", rotateCoins[i].gameObject);
                Addressables.ReleaseInstance(rotateCoins[i].gameObject);
            }
        }
        rotateCoins.Clear();
    }

    public override void OnRelease()
    {
        base.OnRelease();

        StopAllCoroutines();

        fadeText.DOKill();
        fadeText.transform.DOKill();

        rotateCoins.Clear();
    }

    public override void RefreshAmountText()
    {
        numText.SetText(rewardNum.ToString());
        fadeText.SetText("+" + rewardNum.ToString());
        fadeText.gameObject.SetActive(true);
    }

    public override void DoubleRefreshAmountText()
    {
        fadeText.gameObject.SetActive(true);
        numText.transform.DOScale(1.2f,0.2f).onComplete = () =>
        {
            numText.SetText(rewardNum.ToString());
            fadeText.SetText("+" + rewardNum.ToString());
            numText.transform.DOScale(1f, 0.2f);
        };
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        HideRewardBgEffect();

        int subCoinCount = GetNeedRotateCoinsCount(rewardNum);
        if (rotateCoins.Count < subCoinCount)
        {
            int delta = subCoinCount - rotateCoins.Count;
            for (int i = 0; i < delta; i++)
            {
                //GameManager.ObjectPool.Spawn<RotateCoinObject>("RotateCoin", "RotateCoin", Vector3.zero, Quaternion.identity, cachedTransform, (obj) =>
                //{
                //    GameObject coinObject = (GameObject)obj.Target;
                //    RotateCoin rotateCoin = coinObject.GetComponent<RotateCoin>();
                //    rotateCoin.OnHide();
                //    rotateCoins.Add(rotateCoin);
                //});
                Addressables.InstantiateAsync("RotateCoin", Vector3.zero, Quaternion.identity, cachedTransform).Completed += res =>
                {
                    if (res.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject coinObject = res.Result;
                        RotateCoin rotateCoin = coinObject.GetComponent<RotateCoin>();
                        rotateCoin.OnHide();
                        rotateCoins.Add(rotateCoin);
                    }
                };
            }
        }

        if (coinBody.gameObject.activeSelf)
        {
            Vector3 originalScale = coinBody.localScale;
            coinBody.DOScale(originalScale * 1.1f, 0.15f).onComplete = () =>
            {
                coinBody.DOScale(Vector3.zero, 0.2f);
            };
            yield return new WaitForSeconds(0.35f);
        }

        while (rotateCoins.Count < subCoinCount)
        {
            yield return null;
        }

        coinBody.gameObject.SetActive(false);

        GameManager.Sound.PlayAudio(SoundType.SFX_GetCoin.ToString());

        Transform fadeTextTrans = fadeText.transform;
        fadeTextTrans.localScale = Vector3.zero;
        fadeTextTrans.localPosition = Vector3.zero;
        fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, 0);
        fadeTextTrans.SetAsLastSibling();

        fadeText.DOFade(1, 1f);
        fadeTextTrans.DOScale(1f, 0.34f).onComplete = () =>
        {
            fadeTextTrans.DOLocalMoveY(250f, 2.5f).SetEase(Ease.Linear);
        };

        int addNum = (rewardNum + 1) / subCoinCount;
        WaitForSeconds delay = new WaitForSeconds(0.12f);
        for (int i = 0; i < rotateCoins.Count; i++)
        {
            RotateCoin rotateCoin = rotateCoins[i].GetComponent<RotateCoin>();
            Transform rotateCoinTrans = rotateCoins[i].transform;

            Vector3 bornPosition = cachedTransform.position + new Vector3((-0.06f + 0.04f * (i % 4)) * Mathf.Max(cachedTransform.position.x, 1), UnityEngine.Random.Range(0, 0.08f) * Mathf.Max(cachedTransform.position.y, 1));
            Vector3 startPosition = bornPosition + new Vector3(0, -0.08f* Mathf.Max(bornPosition.y, 1));
            Vector3 finalPosition = targetPos;
            rotateCoinTrans.position = bornPosition;

            var graphic = rotateCoin.skeletonGraphic;
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
            rotateCoin.OnShow();
            graphic.DOFade(1f, 0.5f);
            graphic.transform.DOScale(0.4f, 0.4f);

            yield return delay;

            rotateCoinTrans.DOMove(startPosition, 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                rotateCoinTrans.DOMove(finalPosition, 0.6f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    try
                    {
                        rotateCoin.OnHide();
                        GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(addNum, RewardManager.Instance.CoinFlyReceiver));

                        if (RewardManager.Instance.CoinFlyReceiver != null)
                            RewardManager.Instance.CoinFlyReceiver.OnCoinFlyHit();
                        
                        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                    }
                    catch (Exception e)
                    {
                        OnHide();
                        Debug.LogError(e.Message);
                    }
                });
            });
        }

        yield return new WaitForSeconds(0.6f);

        fadeText.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.3f);

        try
        {
            if (RewardManager.Instance.CoinFlyReceiver != null)
            {
                RewardManager.Instance.CoinFlyReceiver.OnCoinFlyEnd();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
 
        yield return new WaitForSeconds(0.2f);

        getRewardAnimFinish = true;
    }

    public override void ShowAmountText()
    {
        //numText.SetText("0");
        //numText.gameObject.SetActive(true);
        //DOTween.To(x => numText.SetText(((int)x).ToString()), 0, rewardNum, 0.3f);

        base.ShowAmountText();
    }

    public override void HideAmountText()
    {
        //numText.gameObject.SetActive(false);

        base.HideAmountText();
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

    //private void InitRewardImage()
    //{
    //    asyncHandle = UnityUtility.LoadGeneralSpriteAsync(UnityUtility.GetCoinSpriteKey(rewardNum), sp =>
    //    {
    //        rewardImage.sprite = sp;
    //        rewardImage.SetNativeSize();

    //        if (rewardNum >= 50000)
    //        {
    //            rewardImage.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
    //        }
    //        else if (rewardNum >= 10000)
    //        {
    //            rewardImage.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
    //        }
    //        else
    //        {
    //            rewardImage.transform.localScale = Vector3.one;
    //        }
    //    });
    //}

    private int GetNeedRotateCoinsCount(int coinNum)
    {
        return 5;
    }
}
