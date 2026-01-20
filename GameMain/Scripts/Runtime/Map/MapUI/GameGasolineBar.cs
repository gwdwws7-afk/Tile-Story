using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameGasolineBar : UIForm
{
    public Transform gasolineImage;
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
        RefreshGasolineText();

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        //RewardManager.Instance.UnregisterItemFlyReceiver(this);
        cachedTransform.DOKill();
        GameManager.DataNode.RemoveNode("OilDrumCollectNum");

        base.OnRelease();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public void RefreshGasolineText()
    {
        currentText.text = GameManager.DataNode.GetData("OilDrumCollectNum", 0).ToString();
    }

    public void PunchGasolineBar()
    {
        cachedTransform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            cachedTransform.localScale = Vector3.one;
        };

        if (punchEffect != null) punchEffect.Play();
    }

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
