using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorationObjectPrefab_15_11 : DecorationObjectPrefab
{
    public Transform particleRendererRoot;

    public GameObject fireExtinguisher;
    public GameObject water;
    public Transform[] roadPoints;

    public override void AnimateOldDecorationObjectPrefab(int index)
    {
        MaskableGraphic[] particleRenderers = particleRendererRoot.GetComponentsInChildren<MaskableGraphic>();
        for (int i = 0; i < particleRenderers.Length; ++i)
        {
            Material newInstance = new Material(particleRenderers[i].material);
            particleRenderers[i].material = newInstance;
            newInstance.DOColor(new Color(1, 1, 1, 0), 3.0f);
        }

        GameManager.Task.AddDelayTriggerTask(3.0f, () =>
        {
            AutoPlaySound script = GetComponent<AutoPlaySound>();
            if (script != null)
                script.MuteSound();
        });

        GameManager.Task.AddDelayTriggerTask(10.0f, () =>
        {
            DestroyImmediate(gameObject);
        });
    }

    public override float AnimateNewDecorationObjectPrefab(int index)
    {
        if (fireExtinguisher != null)
        {
            GameManager.Sound.PlayAudio("SFX_Decoration_Extinguisher");
            fireExtinguisher.SetActive(true);
            water.gameObject.SetActive(true);
            fireExtinguisher.transform.position = roadPoints[0].position;
            float path1Length = (roadPoints[1].position - roadPoints[0].position).magnitude;
            float path2Length = (roadPoints[2].position - roadPoints[1].position).magnitude;
            float path3Length = (roadPoints[3].position - roadPoints[2].position).magnitude;
            float time1 = 0.3f;
            float time2 = time1 / path1Length * path2Length;
            float time3 = time1 / path1Length * path3Length;
            fireExtinguisher.transform.DOMove(roadPoints[1].position, time1).SetEase(Ease.InOutSine).onComplete = () =>
            {
                fireExtinguisher.transform.DOMove(roadPoints[2].position, time2).SetEase(Ease.InOutSine).onComplete = () =>
                {
                    fireExtinguisher.transform.DOMove(roadPoints[3].position, time3).SetEase(Ease.InOutSine).onComplete = () =>
                    {
                        //water.gameObject.SetActive(false);
                        GameManager.Task.AddDelayTriggerTask(0.8f, () =>
                        {
                            fireExtinguisher.SetActive(false);
                        });
                    };
                };
            };
            return time1 + time2 + time3 + 0.8f;//0.3+0.46+0.57
            //我得承认 这里一通算 实际上现在决定后续动画开始的是下面的return 2.5f，因为那里是Index=0的动画
            //但反正效果也挺好的(虽然没完全播完但后续动画早点开始) 保持吧
        }

        return 2.0f;
    }
}
