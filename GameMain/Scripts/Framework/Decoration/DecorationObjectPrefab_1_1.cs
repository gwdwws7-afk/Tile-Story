using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class DecorationObjectPrefab_1_1 : DecorationObjectPrefab
{
    public RawImage img;
    public SkeletonAnimation drawWaterAnim;
    public Image shady;

    public override void SetToNormalMaterial()
    {
    }

    public override void SetToRedShineMaterial()
    {
    }

    public override void AnimateOldDecorationObjectPrefab(int index)
    {
        shady.color = new Color(0, 0, 0, 0);
        shady.gameObject.SetActive(true);
        shady.DOFade(0.7f, 0.2f);

        drawWaterAnim.AnimationState.SetAnimation(0, "animation", false);
        drawWaterAnim.gameObject.SetActive(true);

        GameManager.Task.AddDelayTriggerTask(3.7f, () =>
        {
            shady.DOFade(0, 0.2f);

            img.DOFade(0, 1f).SetDelay(0.5f).onComplete = () =>
            {
                Destroy(gameObject);
            };
        });
    }

    public override float AnimateNewDecorationObjectPrefab(int index)
    {
        return 5.1f;
    }
}
