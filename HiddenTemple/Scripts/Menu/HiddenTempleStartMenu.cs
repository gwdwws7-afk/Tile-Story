using DG.Tweening;
using Spine.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝开始界面
    /// </summary>
    public sealed class HiddenTempleStartMenu : PopupMenuForm
    {
        public GameObject m_MainRoot, m_GuideRoot;
        public Image m_Bg;
        public Button m_CloseButton, m_InfoButton, m_StartButton, m_PreviewButton;
        public GameObject m_StartText, m_PreviewText;
        public ClockBar m_ClockBar;
        public TextMeshProUGUILocalize m_UnlockText;
        public TextMeshProAdapterBox m_GuideTip;
        public Transform m_GuideArrow;
        public SkeletonGraphic m_LockSpine;
        public TextMeshProUGUI m_PickaxeNumText;

        private bool m_IsPreviewMenu;
        private Sequence guideArrowSequence;
        private int m_FirstTimeGivePickaxeNum = 10;
        private int m_NormalGivePickaxeNum = 3;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_IsPreviewMenu = (bool)userData;

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_InfoButton.SetBtnEvent(OnInfoButtonClick);
            m_StartButton.SetBtnEvent(OnStartButtonClick);
            m_PreviewButton.SetBtnEvent(OnPreviewButtonClick);

            m_StartButton.interactable = true;
            m_MainRoot.SetActive(true);
            m_GuideRoot.SetActive(false);
            m_Bg.color = Color.white;

            m_StartButton.gameObject.SetActive(!m_IsPreviewMenu);
            m_PreviewButton.gameObject.SetActive(m_IsPreviewMenu);
            m_StartText.SetActive(!m_IsPreviewMenu);
            m_PreviewText.SetActive(m_IsPreviewMenu);

            m_ClockBar.OnReset();
            if (HiddenTempleManager.Instance.CheckActivityHasStarted())
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(GameManager.Activity.GetCurActivityEndTime());
            }
            else
            {
                var scheduleData = GameManager.Activity.CheckIsInSchedule(HiddenTempleManager.Instance.ActivityID, 0);
                if (scheduleData != null)
                {
                    m_ClockBar.CountdownOver += OnCountdownOver;
                    m_ClockBar.StartCountdown(scheduleData.EndTimeDT);
                }
                else
                {
                    m_ClockBar.SetFinishState();
                }
            }

            if (m_IsPreviewMenu)
            {
                m_UnlockText.SetParameterValue("level", HiddenTempleManager.PlayerData.GetActivityUnlockLevel().ToString());
            }
            else
            {
                if (HiddenTempleManager.PlayerData.GetOpenActivityTime() == 1)
                    m_PickaxeNumText.text = m_FirstTimeGivePickaxeNum.ToString();
                else
                    m_PickaxeNumText.text = m_NormalGivePickaxeNum.ToString();
            }

            if(!m_IsPreviewMenu)
                GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Start_Show);
            else
                GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Unlock_Show);
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_InfoButton.onClick.RemoveAllListeners();
            m_StartButton.onClick.RemoveAllListeners();
            m_PreviewButton.onClick.RemoveAllListeners();

            if (guideArrowSequence != null)
                guideArrowSequence.Kill();

            m_ClockBar.OnReset();

            base.OnReset();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void OnCloseButtonClick()
        {
            if(!m_IsPreviewMenu)
                OnStartButtonClick();
            else
                GameManager.UI.HideUIForm(this);
        }

        private void OnInfoButtonClick()
        {
            GameManager.UI.ShowUIForm("HiddenTempleHowToPlayMenu");
        }

        private void OnStartButtonClick()
        {
            if (!m_StartButton.interactable)
                return;
            m_StartButton.interactable = false;
            m_MainRoot.SetActive(false);
            m_Bg.color = new Color(1, 1, 1, 0);

            if (HiddenTempleManager.PlayerData.GetOpenActivityTime() == 1) 
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Pickaxe, m_FirstTimeGivePickaxeNum);
            else
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Pickaxe, m_NormalGivePickaxeNum);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
            {
                GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                {
                    if (HiddenTempleManager.PlayerData.GetOpenActivityTime() == 1)
                    {
                        GameManager.DataNode.SetData<bool>("HiddenTempleFirstTimeOpen", true);
                        m_Bg.DOFade(0.7f, 0.2f);
                        MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                        var cloneBtn = Instantiate(mapTop.hiddenTempleEntrance.gameObject, mapTop.hiddenTempleEntrance.transform.position, Quaternion.identity, transform);
                        cloneBtn.transform.localScale = new Vector3(mapTop.LeftRootScale, mapTop.LeftRootScale, mapTop.LeftRootScale);
                        var btn = cloneBtn.GetComponent<HiddenTempleEntrance>().ClickButton;
                        btn.onClick.AddListener(() =>
                        {
                            GameManager.UI.HideUIForm(this);
                            GameManager.UI.ShowUIForm("HiddenTempleMainMenu");
                            Destroy(cloneBtn);
                        });
                        btn.interactable = true;

                        Vector3 startPos = cloneBtn.transform.position + new Vector3(0.2f, 0);
                        Vector3 endPos = cloneBtn.transform.position + new Vector3(0.3f, 0);
                        m_GuideArrow.transform.position = startPos;
                        guideArrowSequence = DOTween.Sequence();
                        guideArrowSequence.Append(m_GuideArrow.DOMove(endPos, 0.6f))
                            .Append(m_GuideArrow.DOMove(startPos, 0.6f))
                            .SetLoops(-1);
                        guideArrowSequence.OnComplete(() => guideArrowSequence = null).OnKill(() => guideArrowSequence = null);

                        m_GuideTip.Refresh();
                        if (mapTop.hiddenTempleEntrance.transform.position.y <= 0.28f) 
                            m_GuideTip.transform.position = new Vector3(m_GuideTip.transform.position.x, mapTop.hiddenTempleEntrance.transform.position.y + 0.28f, 0);
                        else
                            m_GuideTip.transform.position = new Vector3(m_GuideTip.transform.position.x, mapTop.hiddenTempleEntrance.transform.position.y - 0.3f, 0);
                        m_GuideRoot.SetActive(true);
                    }
                    else
                    {
                        GameManager.UI.HideUIForm(this);
                        GameManager.UI.ShowUIForm("HiddenTempleMainMenu");
                    }
                });
            });
        }

        private void OnPreviewButtonClick()
        {
            m_LockSpine.freeze = false;
            m_LockSpine.Initialize(false);
            m_LockSpine.AnimationState.SetAnimation(0, "shake_lock", false);
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_ClockBar.SetFinishState();
        }
    }
}
