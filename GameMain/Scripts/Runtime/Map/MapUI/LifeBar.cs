using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 生命栏管理器
/// </summary>
public class LifeBar : UIForm, ILifeFlyReceiver
{
    public Transform life;
    public DelayButton addButton;
    public GameObject infiniteImage;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI fullText;
    public CountdownTimer countdownTimer;
    public GameObject addImage;
    public ParticleSystem punchEffect;
    public Transform lifeBottom;

    public Vector3 LifeFlyTargetPos
    {
        get
        {
            return lifeBottom.position;
        }
    }

    private void OnEnable()
    {
        RewardManager.Instance.RegisterLifeFlyReceiver(this);
    }

    private void OnDisable()
    {
        RewardManager.Instance.UnregisterLifeFlyReceiver(this);
    }

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        if (addButton != null)
        {
            addButton.OnInit(OnAddButtonClick);
        }
        Refresh();

        GameManager.Event.Subscribe(LifeNumChangeEventArgs.EventId, OnLifeNumChange);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(LifeNumChangeEventArgs.EventId, OnLifeNumChange);
        countdownTimer.OnReset();
        base.OnRelease();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnPause()
    {
        base.OnPause();
        if (addButton != null)
            addButton.interactable = false;
    }

    public override void OnResume()
    {
        base.OnResume();
        if (addButton != null)
            addButton.interactable = true;
    }

    public void Refresh()
    {
        float infiniteLifeTime = GameManager.PlayerData.GetInfiniteLifeTime();
        if (infiniteLifeTime > 0)
        {
            infiniteImage.SetActive(true);
            lifeText.gameObject.SetActive(false);
            fullText.gameObject.SetActive(false);
            if (addImage != null)
                addImage.SetActive(false);

            countdownTimer.OnReset();
            countdownTimer.CountDownTextUseDay = false;
            countdownTimer.CountdownOver += OnInfiniteTimeCountdownOver;
            countdownTimer.StartCountdown(DateTime.Now.AddMinutes(infiniteLifeTime));

            counterText.gameObject.SetActive(true);
        }
        else
        {
            infiniteImage.SetActive(false);

            int lifeNum = GameManager.PlayerData.LifeNum;
            if (lifeNum >= GameManager.PlayerData.FullLifeNum)
            {
                counterText.gameObject.SetActive(false);
                fullText.gameObject.SetActive(true);
                if (addImage != null)
                    addImage.SetActive(false);
                countdownTimer.OnReset();
            }
            else
            {
                float recoverLifeTime = GameManager.PlayerData.GetRecoverLifeTime();
                if (recoverLifeTime > 0)
                {
                    counterText.gameObject.SetActive(true);
                    fullText.gameObject.SetActive(false);

                    countdownTimer.OnReset();
                    countdownTimer.CountdownOver += OnLifeRecoverCountdownOver;
                    countdownTimer.StartCountdown(DateTime.Now.AddMinutes(recoverLifeTime));
                    lifeNum = GameManager.PlayerData.GetItemNum(TotalItemData.Life);
                }
                else
                {
                    counterText.gameObject.SetActive(false);
                    fullText.gameObject.SetActive(true);
                    countdownTimer.OnReset();
                    lifeNum = GameManager.PlayerData.GetItemNum(TotalItemData.Life);
                }
                if (addImage != null)
                    addImage.SetActive(true);
            }
            lifeText.SetText(lifeNum.ToString());
            if (lifeNum < 10)
            {
                lifeText.fontSize = 60;
            }
            else
            {
                lifeText.fontSize = 52;
            }

            lifeText.gameObject.SetActive(true);
        }
    }

    private bool CanShowLifeShop()
    {
        if (addImage != null)
            return addImage.gameObject.activeSelf;
        else
            return false;
    }

    private void OnLifeRecoverCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.PlayerData.GetRecoverLifeTime();
        Refresh();
    }

    private void OnInfiniteTimeCountdownOver(object sender, CountdownOverEventArgs e)
    {
        Refresh();
    }

    private void OnAddButtonClick()
    {
        if (!CanShowLifeShop())
            return;

        if (addButton != null)
            addButton.interactable = false;
        GameManager.UI.ShowUIForm("LifeShopPanel",UIFormType.PopupUI,f =>
        {
            if (addButton != null)
                addButton.interactable = true;
        });
    }

    public virtual void Show()
    {
    }

    public void OnLifeNumChange(object sender, GameEventArgs e)
    {
        Refresh();
    }

    public virtual void OnLifeFlyHit()
    {
        if (life != null)
        {
            life.DOPunchScale(new Vector3(-0.3f, -0.3f), 0.2f, 1);
        }
    }

    public virtual void OnLifeFlyEnd()
    {
        punchEffect.Play();
    }
}
