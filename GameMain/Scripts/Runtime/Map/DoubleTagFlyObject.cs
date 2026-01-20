using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleTagFlyObject : EntranceFlyObject
{
    public Transform img;

    //传入的scale倍数
    private float scale;

    public void OnInit(float scale)
    {
        gameObject.SetActive(false);
        transform.localScale = Vector3.one;
        img.localScale = new Vector3 (scale, scale);
        this.scale = scale;
    }

    public override void OnReset()
    {
        gameObject.SetActive(false);
        base.OnReset();
    }

    public override void OnRelease()
    {
        gameObject.SetActive(false);
        base.OnRelease();
    }

    public void FlyDoubleTag(Vector3 startPos, Vector3 targetPos, GameObject target, float delayTime=0)
    {
        FadeFlyObject(0, 0);
        body.localPosition = startPos;
        body.gameObject.SetActive(true);
        FadeFlyObject(1, 0.1f);

        //body.DOScale(new Vector3(1.07f, 0.93f), 0.04f).SetDelay(delayTime).onComplete = () =>
        //{
        //    body.DOScale(new Vector3(0.93f, 1.07f), 0.04f).onComplete = () =>
        //    {
        //        body.DOScale(Vector3.one, 0.07f).onComplete = () =>
        //        {
        //            body.DOScale(new Vector3(1.1f, 0.9f), 0.04f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
        //            {
        //                body.DOScale(new Vector3(0.9f, 1.1f), 0.04f).SetEase(Ease.OutQuad);

        //                body.DOLocalJump(targetPos, 110f * scale, 1, 0.2f).SetEase(Ease.InOutQuad).onComplete = () =>
        //                {
        //                    body.gameObject.SetActive(false);

        //                    ShowFlyObjectReachAnim(target);
        //                };
        //            };
        //        };
        //    };
        //};
        body.DOScale(new Vector3(1.1f, 0.9f), 0.05f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
        {
            body.DOScale(new Vector3(0.9f, 1.1f), 0.05f).SetEase(Ease.OutQuad);

            body.DOLocalJump(targetPos, 110f * scale, 1, 0.2f).SetEase(Ease.InOutQuad).onComplete = () =>
            {
                body.gameObject.SetActive(false);

                ShowFlyObjectReachAnim(target);
            };
        };
    }

    public void FlyBallEnimation(Vector3 startPos, Vector3 targetPos, GameObject target, float delayTime = 0)
    {
        FadeFlyObject(0, 0);
        body.localPosition = startPos;
        body.gameObject.SetActive(true);
        FadeFlyObject(1, 0.1f);

        body.DOScale(new Vector3(1.07f, 0.93f), 0.1f).SetDelay(delayTime).onComplete = () =>
        {
            body.DOScale(new Vector3(0.93f, 1.07f), 0.08f).onComplete = () =>
            {
                body.DOScale(Vector3.one, 0.1f).onComplete = () =>
                {
                    body.DOScale(new Vector3(1.1f, 0.9f), 0.1f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
                    {
                        body.DOScale(new Vector3(0.9f, 1.1f), 0.08f).SetEase(Ease.OutQuad);

                        body.DOLocalJump(targetPos, 110f * scale, 1, 0.2f).SetEase(Ease.InOutQuad).onComplete = () =>
                        {
                            body.gameObject.SetActive(false);

                            ShowFlyObjectReachAnim(target);
                        };
                    };
                };
            };
        };
    }
}
