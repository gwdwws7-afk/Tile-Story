using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginGiftPanel : PopupMenuForm
{
    [SerializeField]
    private Button m_DoubleBtn, m_ClaimBtn, m_CloseBtn;
    [SerializeField]
    private ClockBar m_ClockBar;
    [SerializeField]
    private Material m_GreyMaterial;
    [SerializeField]
    private GameObject m_LoadingText, m_DoubleText;
    [SerializeField]
    private Image m_DoubleBtnImg;
    [SerializeField]
    private RectTransform m_BgTrans, m_AreaTrans, m_TitleTrans;

    public List<DaySlot> m_DaySlots = new List<DaySlot>();
    private List<ItemData> m_Rewards;
    private int m_Today;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

        m_DoubleBtn.SetBtnEvent(OnDoubleButtonClick);
        m_ClaimBtn.SetBtnEvent(() => { OnClaimButtonClick(); });
        m_CloseBtn.SetBtnEvent(OnCloseButtonClick);
        m_DoubleBtn.interactable = true;
        m_ClaimBtn.interactable = true;
        m_CloseBtn.interactable = true;

        m_ClockBar.OnReset();
        m_ClockBar.CountdownOver += OnCountdownOver;
        m_ClockBar.StartCountdown(DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay);

        m_Rewards = null;
        bool isGet = !GameManager.PlayerData.NeedShowLoginGiftByToday();
        int loginDay = GameManager.PlayerData.AccumulatedLoginDays;
        DTLoginGift dataTable = GameManager.DataTable.GetDataTable<DTLoginGift>().Data;
        if (isGet)
        {
            loginDay -= 1;
        }
        else
        {
            if (loginDay >= 7) 
            {
                GameManager.PlayerData.AccumulatedLoginDays = 0;
                loginDay = 0;

                for (int i = 1; i <= 7; i++)
                {
                    PlayerPrefs.SetInt("LoginGiftIsGetDouble_" + i.ToString(), 0);
                }
            }
            m_Rewards = dataTable.GetLoginGift(loginDay + 1).GetRewardDatas();
        }
        m_Today = loginDay + 1;

        for (int i = 0; i < m_DaySlots.Count; i++)
        {
            m_DaySlots[i].Initialize(dataTable.GetLoginGift(i + 1), loginDay + 1, isGet);
        }

        m_DoubleBtn.gameObject.SetActive(!isGet);
        m_ClaimBtn.gameObject.SetActive(!isGet);
        if (isGet)
        {
            m_BgTrans.sizeDelta = new Vector2(1032, 1300);
            m_CloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-293, -893);
            m_AreaTrans.anchoredPosition = new Vector2(0, -71f);
            m_TitleTrans.anchoredPosition = new Vector2(0, 626f);
        }
        else
        {
            m_BgTrans.sizeDelta = new Vector2(1032, 1554);
            m_CloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-293, -762);
            m_AreaTrans.anchoredPosition = new Vector2(0, 59.6f);
            m_TitleTrans.anchoredPosition = new Vector2(0, 756.7f);
        }

        RefreshBtnState();
    }

    public override void OnReset()
    {
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

        m_ClockBar.OnReset();
        for (int i = 0; i < m_DaySlots.Count; i++)
        {
            m_DaySlots[i].Release();
        }

        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void RefreshBtnState()
    {
        bool isLoaded = GameManager.Ads.CheckRewardedAdIsLoaded();
        m_LoadingText.SetActive(!isLoaded);
        m_DoubleText.SetActive(isLoaded);
        m_DoubleBtnImg.material = isLoaded ? null : m_GreyMaterial;
    }

    private void OnDoubleButtonClick()
    {
        if (m_Rewards == null)
            return;

        if (GameManager.Ads.CheckRewardedAdIsLoaded())
        {
            m_DoubleBtn.interactable = false;
            m_ClaimBtn.interactable = false;
            m_CloseBtn.interactable = false;

            GameManager.Ads.ShowRewardedAd("LoginGift");
        }
        else
        {
            GameManager.UI.ShowWeakHint("Common.Ad is still loading", Vector3.zero);
        }
    }

    public void OnClaimButtonClick(bool isDouble = false)
    {
        if (m_Rewards == null)
            return;

        m_DoubleBtn.interactable = false;
        m_ClaimBtn.interactable = false;
        m_CloseBtn.interactable = false;

        int day = GameManager.PlayerData.AccumulatedLoginDays;
        for (int i = 0; i < m_Rewards.Count; i++)
        {
            RewardManager.Instance.AddNeedGetReward(m_Rewards[i].type, m_Rewards[i].num);
        }
        GameManager.PlayerData.RecordShowLoginGiftByToday();
        if (GameManager.PlayerData.AccumulatedLoginDays < 7 && GameManager.PlayerData.AccumulatedLoginDays >= 0) 
            GameManager.PlayerData.AccumulatedLoginDays = GameManager.PlayerData.AccumulatedLoginDays + 1;

        this.m_ProcessFinishAction = null;
        GameManager.UI.HideUIForm(this);
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, ()=> {
            GameManager.Process.EndProcess(ProcessType.ShowDaliyGift);
        }, null, () =>
        {
            if (isDouble)
            {
                if (RewardManager.Instance.RewardArea != null)
                {
                    RewardManager.Instance.RewardArea.onRewardAreaShow = () =>
                    {
                        RewardManager.Instance.RewardArea.DoubleNumberText(true);
                        GameManager.Task.AddDelayTriggerTask(0.3f, () =>
                        {
                            RewardManager.Instance.RewardArea.DoubleNumberText();
                        });
                    };
                }
            }
        });

        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.GetLoginGift));

        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Sign_In_Day_GetReward, new Firebase.Analytics.Parameter("SignNum", day + 1));
    }

    private void OnCloseButtonClick()
    {
        if (m_Rewards == null)
            GameManager.UI.HideUIForm(this);
        else
            OnClaimButtonClick();
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.UI.HideUIForm(this);
    }

    public void OnRewardAdLoaded(object sender, GameEventArgs e)
    {
        RefreshBtnState();
    }

    public void OnRewardAdEarned(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.ToString() != "LoginGift")
        {
            return;
        }

        bool isUserEarnedReward = true;//ne.EarnedReward;
        if (isUserEarnedReward)
        {
            for (int i = 0; i < m_Rewards.Count; i++)
            {
                m_Rewards[i].num *= 2;
            }

            OnClaimButtonClick(true);

            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Sign_In_WatchAd_DoubleReward);
            PlayerPrefs.SetInt("LoginGiftIsGetDouble_" + m_Today, 1);
        }
    }
}
