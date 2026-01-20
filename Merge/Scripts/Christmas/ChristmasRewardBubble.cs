using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using GameFramework.Event;

namespace Merge
{
    public class ChristmasRewardBubble : MonoBehaviour
    {
        public Transform m_Body;
        public SkeletonGraphic m_BubbleAnim;
        public Image m_RewardImg;
        public TextMeshProUGUI m_RewardNumText;
        public Button m_Button;
        public SkeletonGraphic m_BreakEffect;
        public CountdownTimer m_Timer;
        public GameObject m_ClockIcon;
        public GameObject m_AdsIcon;
        public CanvasGroup m_RewardCanvasGroup;

        private int m_Index;
        private MergeChristmasMenu m_Menu;
        private bool m_IsShowing;
        private DRChristmasBubbleReward m_RewardData;
        private string m_RewardSpriteKey;
        private AsyncOperationHandle m_AsyncHandle;

        public bool IsShowing => m_IsShowing;

        public void Initialize(int index, MergeChristmasMenu menu)
        {
            m_Index = index;
            m_Menu = menu;
            m_IsShowing = false;

            GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
            GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

            m_Body.DOKill();

            if (MergeManager.PlayerData.GetChristmasBubbleRewardId(m_Index) > 0) 
                Refresh(false);
        }

        public void Release()
        {
            GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
            GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
        }

        private void Update()
        {
            if (m_IsShowing)
                m_Timer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        public void Show(Vector3 startPos, bool showAnim, float delayTime = 0f)
        {
            if (m_IsShowing)
            {
                return;
            }
            m_IsShowing = true;

            m_Button.SetBtnEvent(OnButtonClick);
            m_Button.interactable = true;

            m_Body.DOKill();
            if (showAnim)
            {
                void delayAction()
                {
                    gameObject.SetActive(true);

                    m_Body.transform.position = startPos;
                    m_Body.transform.DOLocalJump(Vector3.zero, -2f, 1, 0.6f);

                    m_RewardImg.gameObject.SetActive(false);
                    GameManager.Task.AddDelayTriggerTask(0, () =>
                    {
                        m_RewardImg.gameObject.SetActive(true);
                    });

                    if (m_Timer.gameObject.activeSelf)
                    {
                        m_Timer.gameObject.SetActive(false);
                        GameManager.Task.AddDelayTriggerTask(0, () =>
                        {
                            m_Timer.gameObject.SetActive(true);
                        });
                    }

                    m_BubbleAnim.AnimationState.SetAnimation(0, "appear", false).Complete += t =>
                    {
                        m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", true);
                    };

                    GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
                }

                if (delayTime > 0)
                    GameManager.Task.AddDelayTriggerTask(delayTime, delayAction);
                else
                    delayAction();
            }
            else
            {
                m_Body.transform.localPosition = Vector3.zero;
                m_RewardImg.gameObject.SetActive(true);
                gameObject.SetActive(true);

                if (delayTime > 0)
                {
                    var track = m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", false);
                    track.AnimationStart = delayTime;
                    track.Complete += t =>
                    {
                        m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", true);
                    };
                }
                else
                {
                    m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", true);
                }
            }
        }

        public void Hide()
        {
            m_Button.onClick.RemoveAllListeners();

            gameObject.SetActive(false);

            m_IsShowing = false;
        }

        public void Refresh(bool allCanGet)
        {
            RefreshBubbleReward(allCanGet);
            RefreshBubbleTimer();
        }

        private void RefreshBubbleReward(bool allCanGet)
        {
            int rewardId = MergeManager.PlayerData.GetChristmasBubbleRewardId(m_Index);
            if (rewardId <= 0)
            {
                rewardId = MergeManager.Instance.GetRandomBubbleRewardId();
                MergeManager.PlayerData.SetChristmasBubbleRewardId(m_Index, rewardId);
                DRChristmasBubbleReward rewardData = MergeManager.Instance.GetBubbleRewardData(rewardId);

                if (!allCanGet)
                    MergeManager.PlayerData.SetChristmasBubbleGetRewardTime(m_Index, DateTime.Now.AddMinutes(rewardData.CD));
            }

            m_RewardData = MergeManager.Instance.GetBubbleRewardData(rewardId);

            if (allCanGet)
                MergeManager.PlayerData.ClearChristmasBubbleGetRewardTime(m_Index);

            TotalItemData itemData = TotalItemData.FromInt(m_RewardData.RewardId);
            string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(itemData, m_RewardData.RewardNum);
            if (m_RewardSpriteKey != rewardSpriteKey)
            {
                m_RewardImg.color = new Color(1, 1, 1, 0);
                m_RewardSpriteKey = rewardSpriteKey;
                UnityUtility.UnloadAssetAsync(m_AsyncHandle);
                m_AsyncHandle = UnityUtility.LoadGeneralSpriteAsync(rewardSpriteKey, sp =>
                {
                    m_RewardImg.sprite = sp;
                    m_RewardImg.DOFade(1, 0.1f);
                });
            }
            m_RewardNumText.SetItemText(m_RewardData.RewardNum, itemData, true);

            if (itemData.TotalItemType == TotalItemType.InfiniteFireworkBoost || itemData.TotalItemType == TotalItemType.InfiniteAddOneStepBoost || itemData.TotalItemType == TotalItemType.InfiniteMagnifierBoost) 
            {
                m_RewardNumText.transform.localPosition = new Vector3(0, -190f, 0);
            }
            else
            {
                m_RewardNumText.transform.localPosition = new Vector3(0, -150f, 0);
            }
        }

        private void RefreshBubbleTimer()
        {
            m_Timer.OnReset();
            DateTime getRewardTime = MergeManager.PlayerData.GetChristmasBubbleGetRewardTime(m_Index);

            if (DateTime.Now < getRewardTime) 
            {
                m_Timer.StartCountdown(getRewardTime);
                m_Timer.CountdownOver += OnCountdownOver;
                m_Timer.gameObject.SetActive(true);
                m_RewardNumText.gameObject.SetActive(false);
                m_RewardCanvasGroup.alpha = 0.5f;

                RefreshTimerBarIcon();
            }
            else
            {
                m_Timer.gameObject.SetActive(false);
                m_RewardNumText.gameObject.SetActive(true);

                if (m_RewardCanvasGroup.alpha == 0.5f)
                {
                    m_RewardCanvasGroup.DOFade(1, 0.1f);
                    m_Body.DOScale(1.1f, 0.2f).onComplete = () =>
                    {
                        m_Body.DOScale(1f, 0.2f);
                    };
                }
            }
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_Timer.gameObject.SetActive(false);
            m_RewardNumText.gameObject.SetActive(true);
            m_Timer.OnReset();

            if (m_RewardCanvasGroup.alpha == 0.5f)
            {
                m_RewardCanvasGroup.DOFade(1, 0.1f);
                m_Body.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    m_Body.DOScale(1f, 0.2f);
                };
            }
        }

        public void GetBubbleReward()
        {
            if (m_RewardData == null)
            {
                RefreshBubbleReward(false);

                if (m_RewardData == null)
                    return;
            }

            int rewardId = m_RewardData.RewardId;
            int rewardNum = m_RewardData.RewardNum;
            TotalItemData rewardData = TotalItemData.FromInt(rewardId);
            GameManager.PlayerData.AddItemNum(rewardData, rewardNum);

            if(rewardData.TotalItemType==TotalItemType.Life||rewardData.TotalItemType==TotalItemType.InfiniteLife)
                GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(rewardNum, null));
            
            MergeManager.PlayerData.ClearChristmasBubbleGetRewardTime(m_Index);
            MergeManager.PlayerData.SetChristmasBubbleRewardId(m_Index, 0);

            m_Menu.ShowGetRewardAnim(m_Index, rewardData, rewardNum, transform.position, () =>
             {
                 m_RewardImg.gameObject.SetActive(false);
             });

            GameManager.Task.AddDelayTriggerTask(0.15f, () =>
            {
                m_BreakEffect.AnimationState.SetAnimation(0, "gem1", false);
                m_BreakEffect.gameObject.SetActive(true);
            });

            m_BubbleAnim.AnimationState.SetAnimation(0, "click", false).Complete += t =>
            {
                gameObject.SetActive(false);
            };

            GameManager.DataNode.SetData<bool>("ChristmasTreeCanAutoPop", true);

            GameManager.Firebase.RecordMessageByEvent("Merge_Xmas_Tree_Pop_Claim", new Firebase.Analytics.Parameter("Type", m_RewardData.Id));
        }

        private void OnButtonClick()
        {
            DateTime getRewardTime = MergeManager.PlayerData.GetChristmasBubbleGetRewardTime(m_Index);
            DateTime getRewardRVTime = MergeManager.PlayerData.GetChristmasBubbleGetRewardRVTime();

            m_Menu?.HideFinalChestRewardTipBox();
            
            if (DateTime.Now >= getRewardTime)
            {
                m_Button.interactable = false;

                GetBubbleReward();

                GameManager.Sound.PlayAudio("SFX_Xmas_Tree_Pop_Break");
            }
            else if (m_RewardData.EnableAd && GameManager.Ads.CheckRewardedAdIsLoaded() && DateTime.Now >= getRewardRVTime)
            {
                GameManager.Firebase.RecordMessageByEvent("Merge_Xmas_Tree_Pop_Claim_AD");
                GameManager.Ads.ShowRewardedAd("ChristmasBubble_" + m_Index.ToString());
                MergeManager.PlayerData.SetChristmasBubbleGetRewardRVTime(DateTime.Now.AddMinutes(10));
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChristmasBubbleGetRewardRVTime));
            }
            else if(m_Button.interactable)
            {
                m_Body.DOKill();
                m_Body.DOScale(0.9f, 0.15f).onComplete = () =>
                {
                    m_Body.DOScale(1.05f, 0.15f).onComplete = () =>
                    {
                        m_Body.DOScale(1f, 0.15f);
                    };
                };
            }
        }

        public void RefreshTimerBarIcon()
        {
            if (m_RewardData == null) return;
            DateTime getRewardRVTime = MergeManager.PlayerData.GetChristmasBubbleGetRewardRVTime();
            bool showAdsIcon = m_RewardData.EnableAd && GameManager.Ads.CheckRewardedAdIsLoaded();
            showAdsIcon = showAdsIcon && DateTime.Now >= getRewardRVTime;
            m_ClockIcon.SetActive(!showAdsIcon);
            m_AdsIcon.SetActive(showAdsIcon);
        }

        public void OnRewardAdLoaded(object sender, GameEventArgs e)
        {
            RefreshTimerBarIcon();
        }

        public void OnRewardAdEarned(object sender, GameEventArgs e)
        {
            RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
            if (ne.UserData.ToString() != "ChristmasBubble_" + m_Index.ToString())
            {
                RefreshTimerBarIcon();
                return;
            }

            Debug.Log("..........ChristmasBubble OnRewardAdEarned " + ne.EarnedReward.ToString());

            bool isUserEarnedReward = true;
#if UNITY_ANDROID && !UNITY_EDITOR && !AmazonStore
            isUserEarnedReward = ne.EarnedReward;
#endif
            if (isUserEarnedReward)
            {
                m_Timer.gameObject.SetActive(false);
                m_RewardNumText.gameObject.SetActive(true);
                m_RewardCanvasGroup.DOFade(1, 0.1f);

                m_Button.interactable = false;

                GetBubbleReward();

                GameManager.Sound.PlayAudio("SFX_Xmas_Tree_Pop_Break");
            }
        }
    }
}
