using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using GameFramework.Event;

/// <summary>
/// 金币栏管理器
/// </summary>
public sealed class StarBarManager : UIForm, IItemFlyReceiver
{
    public DelayButton showEarnMoreStarBtn;
    public Transform Icon_Image;
    public TextMeshProUGUI Num_Text;
    public ParticleSystem PunchEffect;
    private Transform cachedTransform;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        GameManager.Event.Subscribe(StarNumRefreshEventArgs.EventId, OnStarNumChange);
        RewardManager.Instance.RegisterItemFlyReceiver(this);

        cachedTransform = transform;
        RefreshStarText();
        showEarnMoreStarBtn.OnInit(OnEarnMoreStarButtonClicked);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(StarNumRefreshEventArgs.EventId, OnStarNumChange);
        RewardManager.Instance.UnregisterItemFlyReceiver(this);

        showEarnMoreStarBtn.OnReset();
        cachedTransform.DOKill();
        base.OnRelease();
    }

    public void RefreshStarText()
    {
        Num_Text.text = GameManager.PlayerData.GetCurItemNum(TotalItemData.Star).ToString();
    }

    public void PunchCoinBar()
    {
        cachedTransform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
          {
              cachedTransform.localScale = Vector3.one;
          };

        if (PunchEffect != null) PunchEffect.Play();
    }

    public void OnStarNumChange(object sender, GameEventArgs e)
    {
        RefreshStarText();
    }

    public void OnEarnMoreStarButtonClicked()
    {
        GameManager.UI.ShowUIForm("EarnMoreStarPanel",UIFormType.PopupUI,null, null, -1);
    }

    #region
    public ReceiverType ReceiverType => ReceiverType.Star;
    
    public GameObject GetReceiverGameObject() => gameObject;

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        try
        {
            if (Icon_Image)
            {
                return Icon_Image.position;
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
        PunchCoinBar();
    }

    public void OnFlyEnd(TotalItemData type)
    {
        RefreshStarText();
        PunchCoinBar();
    }
	#endregion
}
