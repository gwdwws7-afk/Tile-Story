using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GasolineFlyReward : FlyReward
{
    public override string Name => "GasolineFlyReward";

    public TextMeshProUGUI NumText;
    public Transform ItemRoot;
    private Transform[] Items = null;

    //根据数量初始化多个道具 散落在四周 
    private IEnumerator InitObjNum(int num)
    {
        Items = new Transform[num];

        var sample = ItemRoot.transform.GetChild(0).GetComponent<Transform>();

        for (int i = 0; i < num; i++)
        {
            var child = Instantiate(sample);
            child.transform.SetParent(ItemRoot.transform);
            child.transform.localScale = Vector3.one;
            child.transform.localPosition = sample.transform.localPosition;

            Vector3 vet = child.transform.localPosition + new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20), 0);
            child.transform.DOLocalMove(vet, 0.2f).SetEase(Ease.InOutSine);
            Items[i] = child;
            //NumText.text = (num - i - 1).ToString();
            if (i == num - 1) NumText.gameObject.SetActive(false);
            yield return null;
        }
        sample.gameObject.SetActive(false);
    }

    public override void OnShow(Action callback = null)
    {
        //Transform trans = body;
        //float originalScale = trans.localScale.x;

        //trans.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);

        gameObject.SetActive(true);

        //trans.DOScale(originalScale * 1.1f, 0.15f).onComplete = () =>
        //{
        //    trans.DOScale(originalScale * 0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
        //    {
        //        trans.DOScale(originalScale, 0.15f).onComplete = () =>
        //        {
        try
        {
            callback?.Invoke();
        }
        catch (Exception e)
        {
            OnHide();
            Log.Error("StarFlyReward OnShow error - {0}", e.Message);
        }
        //        };
        //    };
        //};
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type, Vector3 targetPos)
    {
        yield return InitObjNum(rewardNum);

        float delayTime = 0f;
        for (int i = 0; i < rewardNum; i++)
        {
            int index = i;
            Transform cachedTrans = Items[index];
            cachedTrans.gameObject.SetActive(true);
            delayTime = i * 0.1f;
            cachedTrans.DOMove(targetPos, 1f).SetDelay(delayTime).SetEase(Ease.InBack).onComplete += () =>
            {
                cachedTrans.gameObject.SetActive(false);
                var receiver = RewardManager.Instance.GetReceiverByItemType(type);
                if (index < rewardNum - 1)
                    receiver?.OnFlyHit(type);
                else
                    receiver?.OnFlyEnd(type);
            };
        }

        GameManager.Task.AddDelayTriggerTask(delayTime + 1.4f, () =>
        {
            getRewardAnimFinish = true;
        });
    }
}
