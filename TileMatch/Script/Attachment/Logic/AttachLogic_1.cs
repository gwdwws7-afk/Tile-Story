using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ±ù¿é
/// </summary>
public sealed class AttachLogic_1 : AttachLogic
{
    public override int AttachId => 1;

    public override string AttachAssetName => "Ice";

    protected override void OnInit(object userData)
    {
        TileItem.State |= TileItemState.DisableClick;
        TileItem.GetComponent<TileDelayButton>().DisableAnim = true;
    }

    protected override void OnRelease(bool showEffect)
    {
        TileItem.State &= ~TileItemState.DisableClick;
        TileItem.GetComponent<TileDelayButton>().DisableAnim = false;

        if (showEffect) 
        {
            Transform cachedTrans = TileItem.transform;
            GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
                    "Effect_IceBall_Broken",
                    "TileItemDestroyEffectPool",
                    1f,
                    cachedTrans.position,
                    cachedTrans.rotation,
                    cachedTrans.parent,
                    null);

            GameManager.Sound.PlayAudio("SFX_ice_cracking");
        }
    }

    public override void StopAllAnim()
    {
        if (shakeCoroutine != null)
        {
            TileItem.StopCoroutine(shakeCoroutine);
            TileItem.transform.localPosition = startPosition;
            shakeCoroutine = null;
        }
    }

    public override void OnClick()
    {
        base.OnClick();

        Vector3 shakePos = new Vector3(2, 2, 0);
        ShakeTile(shakePos);
    }

    public override void OnAroundTileGet(TotalItemData type)
    {
        if (TileItem != null && TileItem.IsBeCover)
            return;

        Release(true);
    }

    public override void OnAroundTileEmpty()
    {
        Release(TileItem != null ? !TileItem.IsBeCover : false);
    }

    private Vector3 positionShake = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private Coroutine shakeCoroutine = null;

    public void ShakeTile(Vector3 posShake)
    {
        positionShake = posShake;
        if (shakeCoroutine != null)
        {
            TileItem.StopCoroutine(shakeCoroutine);
            TileItem.transform.localPosition = startPosition;
        }
        shakeCoroutine = TileItem.StartCoroutine(ShakeBetweenTime());
    }

    IEnumerator ShakeBetweenTime()
    {
        Transform myTransform = TileItem.transform;
        float currentTime = 0f;
        startPosition = myTransform.localPosition;
        float cycleTime = 0.2f;
        int cycleCount = 2;
        int curCycle = 0;

        while (true)
        {
            float deltaTime = Time.deltaTime;
            currentTime += deltaTime;

            if (curCycle >= cycleCount)
            {
                yield break;
            }

            currentTime += Time.deltaTime;
            while (currentTime >= cycleTime)
            {
                currentTime -= cycleTime;
                curCycle++;
                if (curCycle >= cycleCount)
                {
                    myTransform.localPosition = startPosition;
                    break;
                }
            }

            float offsetScale = Mathf.Sin(2 * Mathf.PI * currentTime / cycleTime);
            if (positionShake != Vector3.zero)
                myTransform.localPosition = startPosition + positionShake * offsetScale;
            yield return null;

        }
    }
}
