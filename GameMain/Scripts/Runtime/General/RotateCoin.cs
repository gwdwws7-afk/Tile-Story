using Spine.Unity;
using UnityEngine;

/// <summary>
/// 旋转金币类
/// </summary>
public sealed class RotateCoin : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
    //public UIParticle effect;

    public void OnInit()
    {
        transform.localScale = Vector3.one;
    }

    public void OnReset()
    {
        //effect.Stop();
        //skeletonGraphic.timeScale = 1.7f;
        if (skeletonGraphic != null)
        {
            skeletonGraphic.transform.localScale = new Vector3(0.3f, 0.3f);
        }
    }

    public void OnShow()
    {
        OnInit();
        gameObject.SetActive(true);
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
        OnReset();
    }
}
