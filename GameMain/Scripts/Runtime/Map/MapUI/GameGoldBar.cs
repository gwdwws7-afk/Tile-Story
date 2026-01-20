using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameGoldBar : UIForm, IItemFlyReceiver
{
    public Transform iconImage;
    public TextMeshProUGUI currentText;
    public TextMeshProUGUI totalText;
    public ParticleSystem punchEffect;
    private Transform cachedTransform;

    public GameObject body;
    public GameCoinBar coinBar;
    public GameLifeBar lifeBar;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        //RewardManager.Instance.RegisterItemFlyReceiver(this);
        cachedTransform = transform;
        RefreshGoldText();

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        //RewardManager.Instance.UnregisterItemFlyReceiver(this);
        cachedTransform.DOKill();
        GameManager.DataNode.RemoveNode("GoldTileCurrentCount");

        base.OnRelease();
    }
    
    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public void RefreshGoldText()
    {
        currentText.text = GameManager.DataNode.GetData("GoldTileCurrentCount", 0).ToString();
    }

    public void PunchGoldBar()
    {
        cachedTransform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            cachedTransform.localScale = Vector3.one;
        };

        if (punchEffect != null) punchEffect.Play();
    }

    #region
    public ReceiverType ReceiverType => ReceiverType.Gold;

    public GameObject GetReceiverGameObject() => gameObject;

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        try
        {
            if (iconImage)
            {
                return iconImage.position;
            }
            else
            {
                return transform.position;
            }
        }
        catch
        {
            return Vector3.zero;
        }
    }

    public void OnFlyHit(TotalItemData type)
    {
        PunchGoldBar();
    }

    public void OnFlyEnd(TotalItemData type)
    {
        RefreshGoldText();
        PunchGoldBar();
    }
    #endregion

    public void Show()
    {
        if ((coinBar == null || !coinBar.body.activeSelf) &&
            (lifeBar == null || !lifeBar.body.activeSelf))
        {
            body.gameObject.SetActive(true);
        }
        else
        {
            body.gameObject.SetActive(false);
        }
    }
}
