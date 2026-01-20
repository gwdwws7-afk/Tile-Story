using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KitchenEntrance : EntranceUIForm
{
    [SerializeField] private Image slider;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private ParticleSystem ParticleSystem;
    [SerializeField] private GameObject redPoint;
    [SerializeField] private GameObject greenPoint;
    [SerializeField] private TextMeshProUGUI challengeCount;
    [SerializeField]
    private TextMeshProUGUILocalize m_UnlockText;
    [SerializeField]
    private GameObject m_Banner, m_PreviewBanner;
    [SerializeField]
    private Image m_MainImg, m_HatImg;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        Log.Info("KitchenEntrance：初始化美食节活动入口");
        UpdateBtnActiveSelfAndNumText();

        if (KitchenManager.Instance != null)
            slider.fillAmount = 0.136f + 0.864f * KitchenManager.Instance.GetCurrentTaskProgress();
        else
            slider.fillAmount = 0;
        
        SetTimer();
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventRecieved);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventUpdateSlider);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventUpdateSlider);

        countdownTimer.OnReset();

        base.OnRelease();
    }

    public override void OnButtonClick()
    {
        if(KitchenManager.Instance.CheckActivityIsUnlock())
            GameManager.UI.ShowUIForm("KitchenMainMenu");
        else if(KitchenManager.Instance.CheckCanShowEntrance())
            GameManager.UI.ShowUIForm("KitchenWelcomeMenu");
    }

    public override void OnLocked()
    {
        m_UnlockText.SetParameterValue("level", Constant.GameConfig.UnlockKitchenLevel.ToString());
        m_Banner.SetActive(false);
        m_PreviewBanner.SetActive(true);

        m_MainImg.color = Color.gray;
        m_HatImg.color = Color.gray;

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        m_Banner.SetActive(true);
        m_PreviewBanner.SetActive(false);

        m_MainImg.color = Color.white;
        m_HatImg.color = Color.white;

        base.OnUnlocked();
    }

    private void OnEventRecieved(object sender, GameEventArgs e)
    {
        var ne = e as CommonEventArgs;
        if (ne == null) return;
        if (ne.Type == CommonEventType.KitchenInfoChanged)
        {
            UpdateBtnActiveSelfAndNumText();
            SetTimer();
        }
    }

    private void OnEventUpdateSlider(object sender, GameEventArgs e)
    {
        var ne = e as CommonEventArgs;
        if (ne == null) return;
        if (ne.Type == CommonEventType.KitchenEntranceUpdate)
        {
            if (KitchenManager.Instance != null)
                slider.fillAmount = 0.136f + 0.864f * KitchenManager.Instance.GetCurrentTaskProgress();
            else
                slider.fillAmount = 0;
        }
    }

    private void SetTimer()
    {
        DateTime endTime = KitchenManager.Instance.EndTime;
        if (endTime <= DateTime.Now)
        {
            countdownTimer.timeText.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        else
        {
            countdownTimer.timeText.gameObject.SetActive(true);
            countdownTimer.OnReset();
            countdownTimer.StartCountdown(endTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
    }

    public void UpdateBtnActiveSelfAndNumText()
    {
        if (KitchenManager.Instance.CheckCanShowEntrance())
        {
            gameObject.SetActive(true);
            // redPoint.SetActive(KitchenManager.Instance.CanOpenGame());

            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockKitchenLevel)
                OnLocked();
            else
                OnUnlocked();

            if (KitchenManager.Instance.CheckActivityIsUnlock())
            {
                greenPoint.SetActive(KitchenManager.Instance.CanOpenGame());
                challengeCount.text = KitchenManager.Instance.GetCurrentChallengeCount().ToString();
            }
            else
            {
                greenPoint.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        countdownTimer.timeText.gameObject.SetActive(false);
    }

    public void FlyReward()
    {
        int flyNum = GameManager.DataNode.GetData<int>(KitchenManager.LEVEL_WIN_CHEF_HAT_NUM, 0);
        if (flyNum <= 0) return;
        GameManager.DataNode.RemoveNode(KitchenManager.LEVEL_WIN_CHEF_HAT_NUM);
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent(GetFlyIconName(), flyNum, 21,
            new Vector3(250f, 0),
            Vector3.zero, gameObject,
            () =>
            {
            }, () =>
            {
                if(ParticleSystem)ParticleSystem.Play();
                m_HatImg.transform.DOScale(0.8f, 0.15f).onComplete = () =>
                {
                    m_HatImg.transform.DOScale(1f, 0.15f);
                };
                EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
            }, false);
    }

    public virtual string GetFlyIconName()
    {
        return "KitchenCommon[ChefHat]";
    }
}
