using DG.Tweening;
using System;
using UnityEngine;

public class GoldFlyObject : MonoBehaviour
{
    public Transform cachedTransform;
    public ParticleSystem collectEffect;
    public GameObject trailEffect;
    public GameObject image;
    public AnimationCurve rotateCurve;
    public AnimationCurve moveCurve;

    public void OnInit()
    {
        cachedTransform.localScale = Vector3.one;
        cachedTransform.localPosition = Vector3.zero;
        cachedTransform.localRotation = Quaternion.identity;

        trailEffect.SetActive(false);
        image.SetActive(true);
    }

    public void OnShow(Vector3 targetPos, Action completeAction)
    {
        collectEffect.Play();

        float offsetY = targetPos.y - cachedTransform.position.y;

        cachedTransform.DOScale(1.4f, 0.2f).onComplete = () =>
        {
            cachedTransform.DOScale(1.3f, 0.05f).onComplete = () =>
            {
                if(SystemInfoManager.DeviceType > DeviceType.Low)
                    trailEffect.SetActive(true);

                cachedTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.25f).SetEase(Ease.InQuad);
                cachedTransform.DOLocalRotate(new Vector3(0, 0, -100), 0.25f).SetEase(Ease.InQuad);
                cachedTransform.DOMoveY(cachedTransform.position.y + 0.07f, 0.15f).onComplete = () =>
                {
                    cachedTransform.DOMoveY(cachedTransform.position.y - 0.07f, 0.1f).onComplete = () =>
                    {
                        cachedTransform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.3f).SetEase(Ease.InOutQuad);
                        cachedTransform.DOLocalRotate(new Vector3(0, 0, -170), 0.3f).SetEase(Ease.InOutQuad);
                        cachedTransform.DOJump(targetPos, -0.5f * offsetY, 1, 0.3f).SetEase(Ease.InOutQuad).onComplete = () =>
                        {
                            image.SetActive(false);
                            completeAction?.Invoke();
                        };
                    };
                };
            };
        };
    }
}
