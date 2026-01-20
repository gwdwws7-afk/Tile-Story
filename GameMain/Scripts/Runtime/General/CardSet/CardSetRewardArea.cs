using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardSetRewardArea : RewardArea
{
    public override void OnShow(Action callback = null)
    {
        foreach (var reward in rewardFlyObjects)
        {
            reward.body.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        }

        base.OnShow(callback);
    }

    public override void ShowGetRewardAnim(Action onGetRewardComplete)
    {
        if (isRleased)
        {
            onGetRewardComplete?.InvokeSafely();
            return;
        }

        try
        {
            StartCoroutine(ShowGetRewardAnimCor(onGetRewardComplete));
        }
        catch (Exception e)
        {
            OnHide();
            Console.WriteLine(e);
        }
    }

    IEnumerator ShowGetRewardAnimCor(Action onGetRewardComplete)
    {
        foreach (var reward in rewardFlyObjects)
        {
            if (reward.RewardType == TotalItemData.Coin)
                GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(reward.RewardNum, null));

            // reward.transform.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 0.5f).SetEase(Ease.InSine);
            reward.transform.DOScale(1.15f, 0.2f).onComplete += () =>
            {
                reward.transform.DOScale(0f, 0.2f);
            };
            CanvasGroup canvasGroup = reward.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.DOFade(0, 0.2f).SetDelay(0.2f).onComplete += () =>
            {
                reward.gameObject.SetActive(false);
                Destroy(canvasGroup);
            };

            // yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.8f);
        onGetRewardComplete?.InvokeSafely();
        OnHide();
    }
}
