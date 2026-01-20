using DG.Tweening;
using GameFramework.Event;
using System;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenEntrance : EntranceUIForm
{
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private ParticleSystem ParticleSystem;
    [SerializeField] private GameObject redPoint;
    [SerializeField] private GameObject greenPoint;
    [SerializeField] private TextMeshProUGUI challengeCount;
    [SerializeField]
    private TextMeshProUGUILocalize m_UnlockText;
    [SerializeField]
    private GameObject m_Banner, m_PreviewBanner;

    [SerializeField] private SkeletonGraphic m_MainIcon;
    [SerializeField] private Image m_Bg;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventRecieved);
        
        UpdateBtnActiveSelfAndNumText();

        if (HarvestKitchenManager.Instance.CheckCanShowEntrance())
        {
            SetTimer();
        }
        else
        {
            gameObject.SetActive(false);
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);

        countdownTimer.OnReset();

        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnButtonClick()
    {
        if(HarvestKitchenManager.Instance.CheckActivityIsUnlock())
            GameManager.UI.ShowUIForm("HarvestKitchenMainMenu");
        else if(HarvestKitchenManager.Instance.CheckCanShowEntrance())
            GameManager.UI.ShowUIForm("HarvestKitchenWelcomeMenu");
    }

    public override void OnLocked()
    {
        m_MainIcon.freeze = true;
        m_MainIcon.color=Color.gray;
        m_Bg.color = Color.gray;
        m_UnlockText.SetParameterValue("level", Constant.GameConfig.UnlockHarvestKitchenLevel.ToString());
        m_Banner.SetActive(false);
        m_PreviewBanner.SetActive(true);

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        if (!SystemInfoManager.IsSuperLowMemorySize) 
            m_MainIcon.freeze = false;
        m_MainIcon.color = Color.white;
        m_Bg.color = Color.white;
        m_Banner.SetActive(true);
        m_PreviewBanner.SetActive(false);

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

    private void SetTimer()
    {
        DateTime endTime = HarvestKitchenManager.Instance.EndTime;
        if (endTime <= DateTime.Now)
        {
            countdownTimer.OnReset();
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
        if (HarvestKitchenManager.Instance.CheckCanShowEntrance())
        {
            gameObject.SetActive(true);
            // redPoint.SetActive(KitchenManager.Instance.CanOpenGame());

            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockHarvestKitchenLevel)
                OnLocked();
            else
                OnUnlocked();

            if (HarvestKitchenManager.Instance.CheckActivityIsUnlock())
            {
                greenPoint.SetActive(HarvestKitchenManager.Instance.CanOpenGame());
                challengeCount.text = HarvestKitchenManager.Instance.GetCurrentChallengeCount().ToString();
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
        int flyNum = GameManager.DataNode.GetData<int>(HarvestKitchenManager.LEVEL_WIN_CHEF_HAT_NUM, 0);
        if (flyNum <= 0) return;
        GameManager.DataNode.RemoveNode(HarvestKitchenManager.LEVEL_WIN_CHEF_HAT_NUM);
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent(GetFlyIconName(), flyNum, 21,
            new Vector3(250f, 0),
            Vector3.zero, gameObject,
            () =>
            {
            }, () =>
            {
                if(ParticleSystem)ParticleSystem.Play();
                m_MainIcon.transform.DOScale(0.8f, 0.15f).onComplete = () =>
                {
                    m_MainIcon.transform.DOScale(1f, 0.15f);
                };
                EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
            }, false);
    }

    public virtual string GetFlyIconName()
    {
        return "HarvestKitchenCommon[basket]";
    }
}
