using DG.Tweening;
using GameFramework.Event;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeThiefBoard : MonoBehaviour
    {
        public ScrollRect m_StageScrollRect;
        public Slider m_StageSlider;
        public SkeletonGraphic m_ThiefSpine;
        public Image[] m_ThiefSliderFills;
        public Button m_ThiefButton;
        public Transform m_ThiefHitPos, m_ThiefGuidePos, m_ThiefDialogPos, m_ThiefFlyDialogPos, m_ThiefLeftGiftPos;
        public GameObject m_DamageTextRoot, m_CriticalDamageTextRoot, m_PatsTextRoot;
        public TextMeshProUGUI m_DamageText1, m_DamageText2, m_CriticalDamageText1, m_CriticalDamageText2;
        public TextMeshProUGUILocalize m_PatsText1, m_PatsText2, m_BottomText;
        public TextPromptBox m_PromptBox;
        public GameObject[] m_StagesOn;
        public GameObject[] m_StagesOff;
        public GameObject[] m_StagesTick;
        public Transform[] m_HitTextShowPos;

        private int m_CurStage;
        private int m_CurProgress;
        private int m_UnitIndex;
        private EventData m_HitEventData;
        private EventData m_InEventData;
        private int m_HitIndex;

        private DRMerchantInventory m_CurMerchantInventory;
        public DRMerchantInventory CurMerchantInventory
        {
            get
            {
                if (m_CurMerchantInventory == null) 
                {
                    IDataTable<DRMerchantInventory> dataTable = MergeManager.DataTable.GetDataTable<DRMerchantInventory>(MergeManager.Instance.GetMergeDataTableName());
                    m_CurMerchantInventory = dataTable.GetDataRow(m_CurStage);
                }

                return m_CurMerchantInventory;
            }
        }

        private void Start()
        {
            m_ThiefSpine.Initialize(false);
            m_HitEventData = m_ThiefSpine.Skeleton.Data.FindEvent("hit");
            m_InEventData = m_ThiefSpine.Skeleton.Data.FindEvent("heihei");
            m_ThiefSpine.AnimationState.Event += HandleHitStateEvent;
            m_ThiefSpine.AnimationState.Event += HandleInStateEvent;
        }

        public void Init()
        {
            GameManager.Event.Subscribe(MergeFlowerGetEventArgs.EventId, OnThiefHit);

            m_ThiefButton.onClick.RemoveAllListeners();
            m_ThiefButton.onClick.AddListener(OnThiefButtonClick);

            m_CurStage = MergeManager.PlayerData.GetLoveGiftRewardStage();
            m_CurProgress = MergeManager.PlayerData.GetCurLoveGiftRewardProgress();
            m_UnitIndex = 0;
            m_HitIndex = 0;
            if (CurMerchantInventory != null)
            {
                m_UnitIndex = CurMerchantInventory.UnitValueArray.Length;
                for (int i = 0; i < CurMerchantInventory.UnitValueArray.Length; i++)
                {
                    if (m_CurProgress < CurMerchantInventory.UnitValueArray[i])
                    {
                        m_UnitIndex = i;
                        break;
                    }
                }

                m_ThiefSpine.gameObject.SetActive(true);
                m_ThiefSpine.AnimationState.SetAnimation(0, "idle", true);
            }
            else
            {
                m_ThiefSpine.gameObject.SetActive(false);
                m_BottomText.SetTerm("DateMerge.CompleteTip");
                m_ThiefSliderFills[0].transform.parent.gameObject.SetActive(false);
            }

            Refresh();
        }

        public void Recycle()
        {
            GameManager.Event.Unsubscribe(MergeFlowerGetEventArgs.EventId, OnThiefHit);

            m_PromptBox.HidePromptBox();
        }

        public void Release()
        {
            Recycle();
        }

        private void Refresh()
        {
            for (int i = 0; i < m_StagesOn.Length; i++)
            {
                m_StagesOn[i].SetActive(i < m_CurStage);

                if (i == m_CurStage - 1)
                {
                    m_StagesOn[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                }
                else
                {
                    m_StagesOn[i].transform.localScale = Vector3.one;
                }
            }

            for (int i = 0; i < m_StagesOff.Length; i++)
            {
                m_StagesOff[i].SetActive(i >= m_CurStage);
            }

            for (int i = 0; i < m_StagesTick.Length; i++)
            {
                m_StagesTick[i].SetActive(i < m_CurStage - 1);
            }

            m_StageSlider.value = (m_CurStage - 1) / (float)(m_StagesOn.Length - 1);
            m_StageScrollRect.horizontalNormalizedPosition = GetTargetNormalizedPosition(m_CurStage);

            int curStageProgressTotalValue = int.MaxValue;

            if (CurMerchantInventory != null)
            {
                curStageProgressTotalValue = CurMerchantInventory.TotalUnitValue;

                float targetValue = 1 - m_CurProgress / (float)curStageProgressTotalValue;
                for (int i = 0; i < m_ThiefSliderFills.Length; i++)
                {
                    m_ThiefSliderFills[i].fillAmount = targetValue;
                }

                if (m_UnitIndex >= 2) m_ThiefSliderFills[2].gameObject.SetActive(false);
                if (m_UnitIndex >= 4) m_ThiefSliderFills[1].gameObject.SetActive(false);
            }

            if (m_CurProgress >= curStageProgressTotalValue)
            {
                m_CurMerchantInventory = null;

                m_CurStage++;
                MergeManager.PlayerData.SetLoveGiftRewardStage(m_CurStage);
                IDataTable<DRMerchantInventory> dataTable = MergeManager.DataTable.GetDataTable<DRMerchantInventory>(MergeManager.Instance.GetMergeDataTableName());
                var newInventoryData = dataTable.GetDataRow(m_CurStage);
                if (newInventoryData != null)
                {
                    m_CurMerchantInventory = newInventoryData;

                    m_CurProgress = 0;
                    m_UnitIndex = 0;
                    MergeManager.PlayerData.SetCurLoveGiftRewardProgress(0);

                    m_StagesOn[m_CurStage - 2].transform.DOScale(1, 0.2f);
                    m_StageSlider.DOValue((m_CurStage - 1) / (float)(m_StagesOn.Length - 1), 0.5f).onComplete = () =>
                    {
                        m_StagesOn[m_CurStage - 1].transform.DOScale(1.15f, 0.2f);
                        m_StagesOff[m_CurStage - 1].SetActive(false);
                        m_StagesOn[m_CurStage - 1].SetActive(true);
                        m_StagesTick[m_CurStage - 2].SetActive(true);

                        foreach (var fill in m_ThiefSliderFills)
                        {
                            fill.gameObject.SetActive(true);
                            fill.fillAmount = 1;
                        }
                    };

                    m_StageScrollRect.DOHorizontalNormalizedPos(GetTargetNormalizedPosition(m_CurStage), 0.5f);
                }

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Merge_Stage_Task_Complete, new Firebase.Analytics.Parameter("stage", m_CurStage - 2));
            }
        }

        private Queue<int> damageQueue = new Queue<int>();
        private Queue<bool> criticalQueue = new Queue<bool>();
        private Queue<int> curDamageQueue = new Queue<int>(3);
        private Queue<bool> curCriticalQueue = new Queue<bool>(3);

        public bool IsThiefDefeated => CurMerchantInventory == null ? true : MergeManager.PlayerData.GetCurLoveGiftRewardProgress() >= CurMerchantInventory.TotalUnitValue;

        public void OnThiefHit(object sender, GameEventArgs e)
        {
            MergeFlowerGetEventArgs ne = e as MergeFlowerGetEventArgs;

            if (ne != null && CurMerchantInventory != null) 
            {
                m_HitIndex = 0;
                bool isTriggerCriticalHit = false;
                int totalDamage = 0;

                for (int i = 0; i < 3; i++)
                {
                    int damage = Random.Range(CurMerchantInventory.ReduceRange[0], CurMerchantInventory.ReduceRange[1]);
                    bool isCritical = false;
                    if (!isTriggerCriticalHit)
                    {
                        isTriggerCriticalHit = Random.Range(0, 100f) < CurMerchantInventory.CriticalHitProbability;
                        isCritical = isTriggerCriticalHit;
                    }

                    int finalDamage = isCritical ? damage * 2 : damage;
                    damageQueue.Enqueue(finalDamage);
                    criticalQueue.Enqueue(isCritical);
                    totalDamage += finalDamage;
                }

                int realProgress = MergeManager.PlayerData.GetCurLoveGiftRewardProgress();
                MergeManager.PlayerData.SetCurLoveGiftRewardProgress(realProgress + totalDamage);

                GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                {
                    if (CurMerchantInventory != null)
                    {
                        //计算动画播放过快被吞掉的伤害
                        while (curDamageQueue.Count > 0) 
                        {
                            m_CurProgress += curDamageQueue.Dequeue();
                        }
                        curCriticalQueue.Clear();

                        for (int i = 0; i < 3; i++)
                        {
                            if (damageQueue.Count > 0)
                            {
                                curDamageQueue.Enqueue(damageQueue.Dequeue());
                                curCriticalQueue.Enqueue(criticalQueue.Dequeue());
                            }
                        }

                        if (realProgress + totalDamage >= CurMerchantInventory.TotalUnitValue)
                        {
                            ShowThiefFlyAnim();
                        }
                        else
                        {
                            ShowThiefHitAnim();
                        }

                        //小偷被打说话
                        string hitTerm = "DateMerge.HitDialog" + Random.Range(1, 6);
                        m_PatsText1.SetTerm(hitTerm);
                        m_PatsText2.SetTerm(hitTerm);
                        Vector3 randomPos = m_HitTextShowPos[3].position;
                        var target = Instantiate(m_PatsTextRoot, randomPos, Quaternion.identity, m_PatsTextRoot.transform.parent);
                        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
                        target.transform.localScale = Vector3.zero;
                        canvasGroup.alpha = 1;
                        target.SetActive(true);
                        target.transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutExpo).onComplete = () =>
                        {
                            target.transform.DOScale(1f, 0.3f).SetEase(Ease.OutExpo);
                        };
                        canvasGroup.DOFade(0, 0.3f).SetDelay(0.7f).onComplete = () =>
                        {
                            Destroy(target);
                        };
                    }
                });
            }
        }

        private void ShowThiefFlyAnim()
        {
            m_ThiefSpine.AnimationState.SetAnimation(0, "out", false);

            //等待动画播放完毕
            float delayTime = 1.2f;
            GameManager.Task.AddDelayTriggerTask(delayTime + 0.2f, () =>
            {
                ShowThiefDialog("DateMerge.ArrogantFly", m_ThiefFlyDialogPos.position);
            });

            GameManager.Task.AddDelayTriggerTask(delayTime, () =>
            {
                //生成礼包道具
                int id = 90100 + m_CurStage;
                MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
                mainBoard.ShowNewGiftUnlockBoard(id, () =>
                {
                    m_PromptBox.HidePromptBox();

                    Refresh();

                    if (CurMerchantInventory != null)
                    {
                        m_ThiefSpine.AnimationState.SetAnimation(0, "in", false).Complete += t =>
                        {
                            m_ThiefSpine.AnimationState.SetAnimation(0, "idle", true);
                        };
                    }
                    else
                    {
                        mainBoard?.ReleaseAllFlowerProp();
                        m_ThiefSpine.gameObject.SetActive(false);
                        m_BottomText.SetTerm("DateMerge.CompleteTip");
                        m_ThiefSliderFills[0].transform.parent.gameObject.SetActive(false);
                        GameManager.UI.ShowUIForm("MergeEndMenu_Success", form =>
                        {
                            GameManager.UI.HideUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
                        });
                    }
                });
            });
        }

        private void ShowThiefHitAnim()
        {
            m_ThiefSpine.AnimationState.SetAnimation(0, "hit", false).Complete += t =>
            {
                m_ThiefSpine.AnimationState.SetAnimation(0, "idle", true);
            };

            //ShowThiefDialog("DateMerge.HitDialog" + Random.Range(1, 6).ToString());
        }

        private void HandleHitStateEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (m_HitEventData != e.Data || curDamageQueue.Count == 0 || curCriticalQueue.Count == 0)   
                return;

            int index = m_HitIndex++;
            int damage = curDamageQueue.Dequeue();
            bool isCritical = curCriticalQueue.Dequeue();
            m_CurProgress += damage;

            GameObject targetTextRoot = null;
            if (isCritical)
            {
                m_CriticalDamageText1.text = $"-{damage}";
                m_CriticalDamageText2.text = $"-{damage}";
                targetTextRoot = m_CriticalDamageTextRoot;
            }
            else
            {
                m_DamageText1.text = $"-{damage}";
                m_DamageText2.text = $"-{damage}";
                targetTextRoot = m_DamageTextRoot;
            }

            Vector3 randomPos = m_HitTextShowPos[index % 3].position;
            var target = Instantiate(targetTextRoot, randomPos, Quaternion.identity, targetTextRoot.transform.parent);

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            target.transform.localScale = Vector3.zero;
            canvasGroup.alpha = 1;
            target.SetActive(true);
            target.transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutExpo).onComplete = () =>
            {
                target.transform.DOScale(1f, 0.3f).SetEase(Ease.OutExpo);
            };
            canvasGroup.DOFade(0, 0.3f).SetDelay(0.7f).onComplete = () =>
            {
                Destroy(target);
            };

            float targetValue = 1 - m_CurProgress / (float)CurMerchantInventory.TotalUnitValue;
            for (int i = 0; i < m_ThiefSliderFills.Length; i++)
            {
                m_ThiefSliderFills[i].DOKill();
                m_ThiefSliderFills[i].DOFillAmount(targetValue, 0.2f).SetEase(Ease.InOutQuart);
            }

            //Generate reward prop
            if (CurMerchantInventory != null)
            {
                while (m_UnitIndex < CurMerchantInventory.UnitValueArray.Length && m_CurProgress >= CurMerchantInventory.UnitValueArray[m_UnitIndex])
                {
                    m_UnitIndex++;

                    if (m_UnitIndex >= 2) m_ThiefSliderFills[2].gameObject.SetActive(false);
                    if (m_UnitIndex >= 4) m_ThiefSliderFills[1].gameObject.SetActive(false);

                    IDataTable<DRMerchantReward> rewardDataTable = MergeManager.DataTable.GetDataTable<DRMerchantReward>(MergeManager.Instance.GetMergeDataTableName());
                    var rewardData = rewardDataTable.GetDataRow(m_CurStage);
                    if (rewardData != null)
                    {
                        MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
                        if (mainBoard != null)
                        {
                            for (int i = 0; i < rewardData.UnitReward.Length; i++)
                            {
                                int unitRewardId = rewardData.UnitReward[i];
                                float delayTime = i * 0.1f;

                                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                                {
                                    var randomSquare = MergeManager.Merge.GetRandomEmptySquare();
                                    if (randomSquare == null)
                                    {
                                        mainBoard.StoreProp(unitRewardId);

                                        mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.m_SupplyButton.transform.position, 0.3f, null, false);
                                    }
                                    else
                                    {
                                        MergeManager.Merge.GenerateProp(unitRewardId, 0, m_ThiefHitPos.position, randomSquare, PropMovementState.Flying);

                                        mainBoard.ShowShiningFlyEffect(transform.position, randomSquare.transform.position, 0.3f, null, false);
                                    }

                                    GameManager.Sound.PlayAudio("SFX_Pinata_Show");
                                });
                            }
                        }
                    }
                }
            }
        }

        private void HandleInStateEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (m_InEventData != e.Data)
                return;

            if (isFirstEnter)
            {
                isFirstEnter = false;
                ShowThiefDialog("DateMerge.EnterDialog1");
            }
            else
            {
                ShowThiefDialog("DateMerge.EnterDialog2");
            }
        }

        private float GetTargetNormalizedPosition(int stage)
        {
            if (stage < 4)
                return 0;
            else if (stage >= 4 && stage < 5)
                return 0.5f;
            else
                return 1;
        }

        private int dialogIndex = 0;

        private void OnThiefButtonClick()
        {
            dialogIndex = (dialogIndex + 1) % 3;
            ShowThiefDialog("DateMerge.ArrogantDialog" + (dialogIndex + 1).ToString());
        }

        private void ShowThiefDialog(string content)
        {
            ShowThiefDialog(content, m_ThiefDialogPos.position);
        }

        private void ShowThiefDialog(string content, Vector3 pos)
        {
            m_PromptBox.SetText(content);
            m_PromptBox.ShowPromptBox(PromptBoxShowDirection.Left, pos, 3f);
        }

        private bool isFirstEnter = false;
        public void ShowThiefFirstEnterAnim()
        {
            isFirstEnter = true;
            m_ThiefSpine.gameObject.SetActive(true);
            m_ThiefSpine.AnimationState.SetAnimation(0, "in", false).Complete += t =>
            {
                m_ThiefSpine.AnimationState.SetAnimation(0, "idle", true);
            };

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.OnPause();

                GameManager.Task.AddDelayTriggerTask(2.2f, () =>
                {
                    mainBoard.OnResume();

                    mainBoard.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_DateMerge1);
                });
            }
        }

        public void ShowThiefJokeAnim()
        {
            m_ThiefSpine.AnimationState.SetAnimation(0, "joke", false).Complete += t =>
            {
                m_ThiefSpine.AnimationState.SetAnimation(0, "idle", true);
            };
        }
    }
}
