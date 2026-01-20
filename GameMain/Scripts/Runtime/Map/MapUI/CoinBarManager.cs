using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 金币栏管理器
/// </summary>
public class CoinBarManager : UIForm, ICoinFlyReceiver
{
    public DelayButton addButton;
    public Transform coinImage;
    public TextMeshProUGUI coinText;
    public ParticleSystem punchEffect;

    private Transform cachedTransform;

    private int shownNum;
    private int currentCoinNum;
    private int targetCoinNum;
    private int startCoinNum;
    private float timer;
    private bool needRefresh;

    public Vector3 CoinFlyTargetPos
    {
        get
        {
            return coinImage.position;
        }
    }

    public int ShownNum { get => shownNum; set => shownNum = value; }

    private void OnEnable()
    {
        RewardManager.Instance.RegisterCoinFlyReceiver(this);
    }

    private void OnDisable()
    {
        RewardManager.Instance.UnregisterCoinFlyReceiver(this);
    }

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        GameManager.Event.Subscribe(CoinNumChangeEventArgs.EventId, OnCoinNumChange);

        cachedTransform = transform;
        RefreshCoinText();
        if (addButton != null) addButton.OnInit(OnAddButtonClick);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CoinNumChangeEventArgs.EventId, OnCoinNumChange);

        if (addButton != null) addButton.OnReset();

        cachedTransform.DOKill();
        timer = 0;
        shownNum = 0;
        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (targetCoinNum != currentCoinNum)
        {
            timer += elapseSeconds;
            int delta = (int)Mathf.Lerp(startCoinNum, targetCoinNum, timer) - startCoinNum;
            if (delta <= 0) delta = 1;
            currentCoinNum += delta;
            if (currentCoinNum > targetCoinNum)
            {
                currentCoinNum = targetCoinNum;
                needRefresh = true;
            }
            SetNumText(currentCoinNum);
        }
        else if (needRefresh)
        {
            needRefresh = false;
            RefreshCoinText();
        }
    }

    public void RefreshCoinText()
    {
        currentCoinNum = shownNum == 0 ? GameManager.PlayerData.GetCurItemNum(TotalItemData.Coin) : shownNum;
        targetCoinNum = currentCoinNum;
        startCoinNum = currentCoinNum;
        SetNumText(currentCoinNum);
        timer = 0f;
    }

    public void AddCoinText(int addNum)
    {
        int realCoinNum = shownNum == 0 ? GameManager.PlayerData.CoinNum : shownNum;
        targetCoinNum = Math.Min(realCoinNum, targetCoinNum + addNum);
        startCoinNum = currentCoinNum;
        timer = 0f;
    }

    public void PunchCoinBar()
    {
        cachedTransform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
          {
              cachedTransform.localScale = Vector3.one;
          };

        if (punchEffect != null)
        {
            punchEffect.Play();
        }
    }

    private void OnAddButtonClick()
    {
        if (addButton) addButton.interactable = false;
        GameManager.UI.ShowUIForm("ShopMenuManager",UIFormType.PopupUI, obj =>
        {
            if (addButton) addButton.interactable = true;
        });
    }

    public void OnRemoveAddBtn()
    {
        if (addButton)
        {
            addButton.interactable = false;
            addButton.gameObject.SetActive(true);
        }
    }

    public void OnCoinNumChange(object sender, GameEventArgs e)
    {
        CoinNumChangeEventArgs ne = (CoinNumChangeEventArgs)e;

        try
        {
            if(gameObject.activeInHierarchy)
            {
                AddCoinText(ne.ChangeNum);
            }
            else
            {
                RefreshCoinText();
            }
        }
        catch (Exception except)
        {
            Log.Debug($"OnCoinNumChange:{except.Message}");
        }
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        
    }

    private void SetNumText(int num)
    {
        try
        {
            if (num >= 10000)
            {
                coinText.text = $"{(10*num/10000f).ToString("F1")}k";
            }
            else
            {
                coinText.text = num.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public virtual void Show()
    {
    }

    public virtual void OnCoinFlyHit()
    {
        PunchCoinBar();
    }

    public virtual void OnCoinFlyEnd()
    {
        RefreshCoinText();
        PunchCoinBar();
    }

    public GameObject GetReceiverGameObject()
    {
        return gameObject;
    }
}
