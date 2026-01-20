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

public sealed class TaskFlyReward : FlyReward
{
    public override string Name => "TaskFlyReward";

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public TextMeshProUGUI fadeText;
    public UIParticle rewardBgEffect;
    public UIParticle rewardShowEffect;
    public GameObject bannerFront;
    public GameObject bannerBack;
    public GameObject rewardBody;
    public GameObject infiniteIcon;

    private List<TotalItemData> rewardTypeList;
    private List<int> rewardNumList;

    private List<RotateCoin> rotateCoins = new List<RotateCoin>();
    private AsyncOperationHandle asyncHandle;

    private bool isChest;

    public List<TotalItemData> RewardTypeList { get => rewardTypeList; }
    public List<int> RewardNumList { get => rewardNumList; }

    public bool IsChest { get => isChest; }

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        Log.Warning("this method is obsolete.Use Init instead.");
    }

    public void Init(List<TotalItemData> rewardTypeList, List<int> rewardNumList, int rewardTypeCount, bool autoGetReward)
    {
        this.rewardTypeList = rewardTypeList;
        this.rewardNumList = rewardNumList;
        this.autoGetReward = autoGetReward;

        if (rewardTypeCount == 1)
        {
            rewardType = rewardTypeList[0];
            rewardNum = rewardNumList[0];
        }

        cachedTransform = transform;

        RefreshAmountText();

        isChest = rewardTypeList.Count > 1;

        InitRewardImage();

        isLocked = true;
    }

    public override void OnReset()
    {
        base.OnReset();

        for (int i = 0; i < rotateCoins.Count; i++)
        {
            if (rotateCoins[i] != null)
            {
                rotateCoins[i].OnReset();
                GameManager.ObjectPool.Unspawn<RotateCoinObject>("RotateCoin", rotateCoins[i].gameObject);
            }
        }
        rotateCoins.Clear();

        UnityUtility.UnloadAssetAsync(asyncHandle);
        asyncHandle = default;

        isChest = false;
    }

    public override void OnRelease()
    {
        base.OnRelease();

        rewardTypeList = null;
        rewardNumList = null;

        for (int i = 0; i < rotateCoins.Count; i++)
        {
            if (rotateCoins[i] != null)
            {
                rotateCoins[i].OnReset();
                GameManager.ObjectPool.Unspawn<RotateCoinObject>("RotateCoin", rotateCoins[i].gameObject);
            }
        }
        rotateCoins.Clear();

        UnityUtility.UnloadAssetAsync(asyncHandle);
        asyncHandle = default;

        isChest = false;
    }

    public override void OnShow(Action callback = null)
    {
        base.OnShow();

        rewardBody.transform.localScale = Vector3.one;
        rewardBody.SetActive(true);

        callback?.Invoke();
    }

    public override void RefreshAmountText()
    {
        base.RefreshAmountText();

        if (!isChest)
        {
            if (rewardType == TotalItemData.Coin && fadeText != null) 
            {
                fadeText.SetText("+" + rewardNumList[0].ToString());
            }

            numText.transform.localPosition = new Vector3(0, -25, 0);
            infiniteIcon.SetActive(false);

            numText.SetItemText(rewardNumList[0], rewardTypeList[0], false);
        }
    }

    public override void ShowRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(true);
            rewardBgEffect.Play();
        }
    }

    public override void HideRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(false);
            rewardBgEffect.Stop();
        }
    }

    public override void ShowRewardShowEffect()
    {
        if (rewardShowEffect != null)
        {
            rewardShowEffect.gameObject.SetActive(true);
            rewardShowEffect.Play();
        }
    }

    public override void HideRewardShowEffect()
    {
        if (rewardShowEffect != null)
        {
            rewardShowEffect.gameObject.SetActive(false);
            rewardShowEffect.Stop();
        }
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        if (rewardType == TotalItemData.Coin)
        {
            int subCoinCount = GetNeedRotateCoinsCount(rewardNum);

            if (rotateCoins.Count < subCoinCount)
            {
                int delta = subCoinCount - rotateCoins.Count;
                for (int i = 0; i < delta; i++)
                {
                    GameManager.ObjectPool.Spawn<RotateCoinObject>("RotateCoin", "RotateCoin", Vector3.zero, Quaternion.identity, cachedTransform, (obj) =>
                    {
                        if (obj != null && obj.Target != null)
                        {
                            GameObject coinObject = (GameObject)obj.Target;
                            RotateCoin rotateCoin = coinObject.GetComponent<RotateCoin>();
                            if (rotateCoin != null)
                            {
                                rotateCoin.OnHide();
                                rotateCoins.Add(rotateCoin);
                            }
                            else
                            {
                                subCoinCount--;
                            }
                        }
                        else
                        {
                            subCoinCount--;
                        }
                    });
                }
            }

            rewardBody.transform.DOScale(new Vector3(1.1f, 1.1f), 0.15f).onComplete = () =>
            {
                rewardBody.transform.DOScale(Vector3.zero, 0.2f);
            };
            yield return new WaitForSeconds(0.35f);

            while (rotateCoins.Count < subCoinCount)
            {
                yield return null;
            }

            rewardBody.SetActive(false);
            HideRewardBgEffect();
            cachedTransform.localScale = Vector3.one;

            GameManager.Sound.PlayAudio(SoundType.SFX_GetCoin.ToString());

            Transform fadeTextTrans = fadeText.transform;
            fadeTextTrans.localScale = Vector3.zero;
            fadeTextTrans.localPosition = Vector3.zero;
            fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, 0);
            fadeTextTrans.SetAsLastSibling();

            fadeText.gameObject.SetActive(true);
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

                Vector3 bornPosition = cachedTransform.position + new Vector3(-0.06f + 0.04f * (i % 4), UnityEngine.Random.Range(0, 0.08f));
                Vector3 startPosition = bornPosition + new Vector3(0, -0.08f);
                Vector3 finalPosition = targetPos;
                rotateCoinTrans.position = bornPosition;

                var graphic = rotateCoin.skeletonGraphic;
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0);
                rotateCoin.OnShow();
                graphic.DOFade(1f, 0.5f);
                graphic.transform.DOScale(0.35f, 0.4f);

                yield return delay;

                rotateCoinTrans.DOMove(startPosition, 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    rotateCoinTrans.DOMove(finalPosition, 0.6f).SetEase(Ease.InCubic).OnComplete(() =>
                    {
                        rotateCoin.OnHide();
                        GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(addNum, RewardManager.Instance.CoinFlyReceiver));

                        if (RewardManager.Instance.CoinFlyReceiver != null)
                            RewardManager.Instance.CoinFlyReceiver.OnCoinFlyHit();
                    });
                });
            }

            yield return new WaitForSeconds(0.6f);

            fadeText.DOFade(0, 0.5f);

            yield return new WaitForSeconds(0.3f);

            if (RewardManager.Instance.CoinFlyReceiver != null)
            {
                RewardManager.Instance.CoinFlyReceiver.OnCoinFlyEnd();
            }

            yield return new WaitForSeconds(0.2f);

            fadeTextTrans.DOKill();
            fadeText.gameObject.SetActive(false);

            getRewardAnimFinish = true;
        }
        else if (rewardType == TotalItemData.CardPack1 ||
                 rewardType == TotalItemData.CardPack2 ||
                 rewardType == TotalItemData.CardPack3 ||
                 rewardType == TotalItemData.CardPack4 ||
                 rewardType == TotalItemData.CardPack5)
        {
            HideRewardBgEffect();
            rewardBody.transform.DOScale(new Vector3(1.1f, 1.1f), 0.15f).onComplete = () =>
            {
                rewardBody.transform.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    rewardBody.SetActive(false);
                };
            };
            yield return new WaitForSeconds(0.2f);
            
            //开卡包相关
            int packType = 1;
            switch (rewardType.TotalItemType)
            {
                case TotalItemType.CardPack2:
                    packType = 2;
                    break;
                case TotalItemType.CardPack3:
                    packType = 3;
                    break;
                case TotalItemType.CardPack4:
                    packType = 4;
                    break;
                case TotalItemType.CardPack5:
                    packType = 5;
                    break;
            }
            
            for (int i = 0; i < rewardNum; i++)
            {
                GetCardPackPanel panel = null;
                bool getCardComplete = false;
                GameManager.UI.ShowUIForm("GetCardPackPanel", userData: packType, showSuccessAction: form =>
                {
                    panel = form as GetCardPackPanel;
                    panel?.SetCompleteAction(() => { getCardComplete = true; });
                    panel?.ShowPackAnimForTaskFlyReward();
                });
            
                yield return new WaitUntil(() => getCardComplete);
            }
            getRewardAnimFinish = true;
        }
        else
        {
            yield return null;

            Vector3 startPos = new Vector3(cachedTransform.position.x, cachedTransform.position.y, 0);
            Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
            Vector3 startScale = cachedTransform.localScale;
            float delayTime = (type == TotalItemData.Life || type == TotalItemData.InfiniteLifeTime) ? 0.36f : 0.34f;
            cachedTransform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
            {
                cachedTransform.DOMove(targetPos, 0.36f).SetEase(Ease.InCubic);
                cachedTransform.DOScale(0.6f, delayTime).SetEase(Ease.InCubic).onComplete = () =>
                {
                    OnHide();
                    cachedTransform.localScale = Vector3.one;

                    getRewardAnimFinish = true;

                    if (type == TotalItemData.Life || type == TotalItemData.InfiniteLifeTime)
                    {
                        RewardManager.Instance.LifeFlyReceiver?.OnLifeFlyHit();
                        RewardManager.Instance.LifeFlyReceiver?.OnLifeFlyEnd();
                        GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(1, RewardManager.Instance.LifeFlyReceiver));
                    }
                    else
                    {
                        RewardManager.Instance.GetReceiverByItemType(type)?.OnFlyEnd(type);
                    }

                    GameManager.Sound.PlayAudio("SFX_itemget");
                };
            };
        }
    }

    private void InitRewardImage()
    {
        string key;

        if (isChest)
        {
            //int count = rewardTypeList.Count;
            //if (count >= 5)
            //{
            //    key = "RewardChest1";
            //}
            //else if (count >= 4)
            //{
            //    key = "RewardChest2";
            //}
            //else
            //{
            //    key = "RewardChest3";
            //}
            key = "Chest3";

            rewardBody.transform.localScale = Vector3.one;
            rewardImage.transform.localScale = new Vector3(1.25f, 1.25f);
            rewardImage.transform.localPosition = new Vector3(0, 5);
            bannerBack.SetActive(false);
            bannerFront.SetActive(false);
            numText.gameObject.SetActive(false);
        }
        else
        {
            key = UnityUtility.GetRewardSpriteKey(rewardTypeList[0], rewardNumList[0]);

            SetRewardImagePerfectSize(rewardType);

            bannerBack.SetActive(true);
            bannerFront.SetActive(true);
            numText.gameObject.SetActive(true);
        }

        Sprite sp = AddressableUtils.LoadAsset<Sprite>($"TotalItemAtlas[{key}]");
        rewardImage.sprite = sp;
        rewardImage.SetNativeSize();

        //asyncHandle = UnityUtility.LoadGeneralSpriteAsync(key, sp =>
        //{
        //    rewardImage.sprite = sp;
        //    rewardImage.SetNativeSize();
        //});
    }

    private int GetNeedRotateCoinsCount(int coinNum)
    {
        return 9;
    }

    public void SetRewardImagePerfectSize(TotalItemData type)
    {
        if (type == TotalItemData.None)
        {
        }
        else if (type == TotalItemData.Life || type == TotalItemData.InfiniteLifeTime) 
        {
            rewardImage.transform.localPosition = new Vector3(0f, 10f, 0f);
            rewardImage.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        }
        else if (type == TotalItemData.InfiniteFireworkBoost)
        {
            rewardImage.transform.localPosition = new Vector3(0f, 10f, 0f);
            rewardImage.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
        }
        else if (type == TotalItemData.Coin)
        {
            rewardImage.transform.localPosition = new Vector3(0f, 10f, 0f);
            rewardImage.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        }
        else
        {
            rewardImage.transform.localPosition = new Vector3(0f, 10f, 0f);
            rewardImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }
    }
}
