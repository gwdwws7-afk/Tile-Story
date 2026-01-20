using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeadPortrait : MonoBehaviour, IItemFlyReceiver
{
    public Image m_PortraitImage;
    public ParticleSystem m_PunchEffect;
    private AsyncOperationHandle m_Handle;

    public ReceiverType ReceiverType => ReceiverType.Common;

    public void Initialize()
    {
        RewardManager.Instance.RegisterItemFlyReceiver(this);

        UnityUtility.UnloadAssetAsync(m_Handle);
        string headPortraitName = $"HeadPortrait_{GameManager.PlayerData.HeadPortrait}_{GameManager.PlayerData.HeadPortrait}";
        m_Handle = UnityUtility.LoadAssetAsync<Sprite>(headPortraitName, (s) =>
          {
              m_PortraitImage.sprite = null;
              m_PortraitImage.sprite = s as Sprite;
              m_PortraitImage.gameObject.SetActive(true);
          });

        gameObject.SetActive(true);
    }

    public void Release()
    {
        RewardManager.Instance.UnregisterItemFlyReceiver(this);

        if (m_PortraitImage.sprite != null)
        {
            m_PortraitImage.sprite = null;
        }
        m_PortraitImage.gameObject.SetActive(false);
        UnityUtility.UnloadAssetAsync(m_Handle);
        m_Handle = default;

        gameObject.SetActive(false);
    }

    public GameObject GetReceiverGameObject()
    {
        return gameObject;
    }

    public void OnFlyHit(TotalItemData type)
    {
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            transform.localScale = Vector3.one;
        };

        if (m_PunchEffect != null)
        {
            m_PunchEffect.Play();
        }
    }

    public void OnFlyEnd(TotalItemData type)
    {
        OnFlyHit(type);
    }

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        return transform.position;
    }
}
