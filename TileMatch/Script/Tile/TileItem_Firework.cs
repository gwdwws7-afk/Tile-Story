using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ÑÌ»¨Æå×Ó
/// </summary>
public class TileItem_Firework : TileItem
{
    public Image m_Image;
    public GameObject m_BorderEffect;

    private const float m_ShakeInternal = 3f;
    private float m_Timer = 0f;
    private Vector3 positionShake = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private Coroutine shakeCoroutine = null;
    private int delayTaskId = 0;

    private void OnEnable()
    {
        if (!SystemInfoManager.IsSuperLowMemorySize)
        {
            delayTaskId = GameManager.Task.AddDelayTriggerTask(2f, () =>
            {
                m_BorderEffect.SetActive(true);
            });
        }
    }

    private void OnDisable()
    {
        GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
        m_BorderEffect.SetActive(false);
    }

    public override void SetImage()
    {
    }

    public override void SetCoverState()
    {
        m_Image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    public override void SetUncoverState()
    {
        m_Image.DOColor(Color.white, 0.2f);
    }

    public override void StopAllAnim()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            m_Image.transform.localPosition = startPosition;
            shakeCoroutine = null;
        }
        m_Timer = -5f;

        m_BorderEffect.SetActive(false);

        base.StopAllAnim();
    }

    public override void StartAllAnim()
    {
        base.StartAllAnim();

        if (!SystemInfoManager.IsSuperLowMemorySize)
            m_BorderEffect.SetActive(true);
    }

    public override void OnDestroyTile()
    {
        base.OnDestroyTile();

        m_BorderEffect.SetActive(false);
    }

    private void Update()
    {
        if (!IsBeCover && !IsDestroyed && !State.HasFlag(TileItemState.DisableClick)) 
        {
            if (m_Timer >= m_ShakeInternal)
                ShowShakeAnim();
            else
                m_Timer += Time.deltaTime;
        }
    }

    private void ShowShakeAnim()
    {
        m_Timer = 0f;
        positionShake = new Vector3(2, 2, 0);
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            m_Image.transform.localPosition = startPosition;
        }
        shakeCoroutine = StartCoroutine(ShakeBetweenTime());
    }

    IEnumerator ShakeBetweenTime()
    {
        Transform myTransform = m_Image.transform;
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
