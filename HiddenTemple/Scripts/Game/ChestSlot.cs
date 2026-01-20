using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 进度条上宝箱插槽
    /// </summary>
    public sealed class ChestSlot : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_ChestCloseImg, m_ChestOpenImg, m_ChestLevel, m_TickImg;
        [SerializeField]
        private Button m_ClaimButton, m_TipButton;
        [SerializeField]
        private GameObject m_Effect;

        private int m_ChestStage;
        private int m_Stage;
        private ChestArea m_ChestArea;

        private bool m_IsUnlock;
        private bool m_IsClaimed;

        public void Initialize(int chestStage, int stage, ChestArea chestArea)
        {
            m_ChestStage = chestStage;
            m_Stage = stage;
            m_ChestArea = chestArea;

            m_ClaimButton.SetBtnEvent(OnClaimButtonClick);
            m_TipButton.SetBtnEvent(OnTipButtonClick);

            Refresh(false);
        }

        public void Release()
        {
            m_ClaimButton.DOKill();
            m_TickImg.transform.DOKill();
        }

        private void Refresh(bool showAnim)
        {
            int stage = Mathf.Clamp(m_Stage, 1, 6);
            bool isUnlock = m_ChestStage < stage;
            bool isClaimed = isUnlock && HiddenTempleManager.PlayerData.GetChestIsClaimed(m_ChestStage);

            if (showAnim)
            {
                m_ChestLevel.SetActive(!isUnlock);

                if (!m_IsUnlock && isUnlock)
                {
                    m_ClaimButton.transform.localScale = Vector3.zero;
                    m_ClaimButton.gameObject.SetActive(true);
                    m_ClaimButton.transform.DOScale(0.32f, 0.3f).SetEase(Ease.OutBack);
                    m_TickImg.gameObject.SetActive(false);
                    m_Effect.SetActive(true);
                }

                if (!m_IsClaimed && isClaimed)
                {
                    float scale = 0.42f;
                    m_ChestCloseImg.transform.DOScale(scale * 0.9f, 0.1f).onComplete = () =>
                      {
                          m_ChestCloseImg.SetActive(false);
                          m_ChestOpenImg.transform.localScale = new Vector3(scale * 0.9f, scale * 0.9f, scale * 0.9f);
                          m_ChestOpenImg.SetActive(true);

                          m_ChestOpenImg.transform.DOScale(scale * 1.05f, 0.1f).onComplete = () =>
                            {
                                m_ChestOpenImg.transform.DOScale(scale, 0.1f);
                            };
                      };
                    m_ClaimButton.gameObject.SetActive(false);
                    m_TickImg.transform.localScale = Vector3.zero;
                    m_TickImg.gameObject.SetActive(true);
                    m_TickImg.transform.DOScale(0.9f, 0.3f).SetEase(Ease.OutBack);
                    m_Effect.SetActive(false);
                }
            }
            else
            {
                m_ChestCloseImg.SetActive(!isClaimed);
                m_ChestOpenImg.SetActive(isClaimed);
                m_ChestLevel.SetActive(!isUnlock);
                m_ClaimButton.gameObject.SetActive(isUnlock && !isClaimed);
                m_TickImg.SetActive(isUnlock && isClaimed);
                m_Effect.SetActive(isUnlock && !isClaimed);
            }

            m_IsUnlock = isUnlock;
            m_IsClaimed = isClaimed;
        }

        public void ShowChestSlotUnlockAnim()
        {
            int stage = HiddenTempleManager.PlayerData.GetCurrentStage();
            if (m_Stage < stage)
            {
                m_Stage = stage;
                Refresh(true);
            }
        }

        private void OnClaimButtonClick()
        {
            IDataTable<DRChestData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRChestData>();
            if (dataTable != null)
            {
                DRChestData data = dataTable.GetDataRow(m_ChestStage);
                if (data != null)
                {
                    HiddenTempleManager.PlayerData.SetChestIsClaimed(m_ChestStage, true);
                    Refresh(true);
                    for (int i = 0; i < data.RewardsId.Count; i++)
                    {
                        RewardManager.Instance.SaveRewardData(data.RewardsId[i], data.RewardsNum[i], true);
                    }
                    GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(0, null));

                    Vector3 startPos = transform.position;
                    if (m_ChestStage == HiddenTempleManager.PlayerData.GetMaxStage())
                        startPos = transform.position + new Vector3(-0.02f, 0, 0);

                    int count = data.RewardsId.Count;
                    Vector3[] posList = null;
                    if (count <= 2)
                    {
                        posList = UnityUtility.GetAveragePosition(startPos + new Vector3(0, 0.05f, 0), new Vector3(0.12f, 0, 0), count);
                    }
                    else
                    {
                        int index = (count - 1) / 2 + 1;
                        List<Vector3> tempList = new List<Vector3>();
                        while (count > 0)
                        {
                            tempList.AddRange(UnityUtility.GetAveragePosition(startPos + new Vector3(0, 0.04f + index * 0.02f, 0), new Vector3(0.12f, 0, 0), 2));
                            count -= 2;
                            index--;
                        }
                        posList = tempList.ToArray();
                    }
                    bool showSound = false;
                    for (int i = 0; i < data.RewardsId.Count; i++)
                    {
                        TotalItemData itemData = data.RewardsId[i];
                        int itemNum = data.RewardsNum[i];
                        Vector3 pos = posList[i];
                        float delayTime = i * 0.3f;

                        UnityUtility.InstantiateAsync("HiddenTempleItemSlot", m_ChestArea.m_ChestFlyRoot, obj =>
                        {
                            var item = obj.GetComponent<HiddenTempleItemSlot>();
                            item.OnInit(itemData, itemNum);
                            item.gameObject.SetActive(false);
                            item.transform.position = pos;
                            item.transform.localScale = Vector3.zero;

                            GameManager.Task.AddDelayTriggerTask(0.03f + delayTime, () =>
                            {
                                item.Show();
                                item.transform.DOScale(0.85f, 0.15f).onComplete = () =>
                                {
                                    item.transform.DOScale(0.7f, 0.25f).SetEase(Ease.OutBack);

                                    item.transform.DOLocalMoveY(item.transform.localPosition.y + 200, 0.7f).SetEase(Ease.InCubic).SetDelay(0.08f);
                                    var canvasGroup = item.GetComponent<CanvasGroup>();
                                    canvasGroup.DOFade(0, 0.3f).SetDelay(0.6f);
                                };

                                GameManager.Task.AddDelayTriggerTask(1.3f, () =>
                                {
                                    UnityUtility.UnloadInstance(obj);
                                });

                                if (!showSound)
                                {
                                    showSound = true;
                                    GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());
                                }
                            });
                        });
                    }
                }

                GameManager.Event.Fire(this, ChestClaimEventArgs.Create());
            }
        }

        public void OnTipButtonClick()
        {
            m_ChestArea.ShowItemPromptBox(m_ChestStage, transform.position);
        }
    }
}
