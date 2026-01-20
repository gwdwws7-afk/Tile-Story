using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class MergeEndMenu : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_LastChanceButton, m_ClaimButton, m_TipButton;
        public Transform m_SlotRoot;
        public GameObject m_TopDescribeText, m_AreaDescribeText;

        private List<ItemData> m_Datas;
        private List<MergePromptItemSlot> m_ItemSlots = new List<MergePromptItemSlot>();
        private List<AsyncOperationHandle> m_AsyncHandlesList = new List<AsyncOperationHandle>();
        private int m_RewardCount;

        private bool m_IsReservedRewardMenu;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_IsReservedRewardMenu = userData != null;

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_LastChanceButton.OnInit(OnLastChanceButtonClick);
            m_ClaimButton.OnInit(OnClaimButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

            if (m_IsReservedRewardMenu)
            {
                m_Datas = (List<ItemData>)userData;

                m_RewardCount = m_Datas.Count;
                if (m_RewardCount > 0)
                {
                    InstantiateItemSlotAsync(m_Datas.Count, () =>
                    {
                        for (int i = 0; i < m_Datas.Count; i++)
                        {
                            m_ItemSlots[i].isShowSlotTight = false;
                            m_ItemSlots[i].OnInit(m_Datas[i].type, m_Datas[i].num);
                            m_ItemSlots[i].gameObject.SetActive(true);
                        }

                        for (int i = m_Datas.Count; i < m_ItemSlots.Count; i++)
                        {
                            m_ItemSlots[i].gameObject.SetActive(false);
                        }

                        Refresh();
                    });
                }

                m_TopDescribeText.SetActive(true);
                m_AreaDescribeText.SetActive(false);
                m_LastChanceButton.gameObject.SetActive(false);
                m_ClaimButton.gameObject.SetActive(true);
            }
            else
            {
                m_TopDescribeText.SetActive(false);
                m_AreaDescribeText.SetActive(true);
                m_LastChanceButton.gameObject.SetActive(true);
                m_ClaimButton.gameObject.SetActive(false);
            }
        }

        public override void OnReset()
        {
            for (int i = 0; i < m_ItemSlots.Count; i++)
            {
                m_ItemSlots[i].OnRelease();
            }
            m_ItemSlots.Clear();

            for (int i = 0; i < m_AsyncHandlesList.Count; i++)
            {
                UnityUtility.UnloadAssetAsync(m_AsyncHandlesList[i]);
            }
            m_AsyncHandlesList.Clear();

            base.OnReset();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        private void InstantiateItemSlotAsync(int targetCount, Action callback)
        {
            if (m_ItemSlots.Count < targetCount)
            {
                var asyncHandle = UnityUtility.InstantiateAsync("MergePromptItemSlot", m_SlotRoot, obj =>
                {
                    var slot = obj.GetComponent<MergePromptItemSlot>();
                    m_ItemSlots.Add(slot);
                    InstantiateItemSlotAsync(targetCount, callback);
                });

                m_AsyncHandlesList.Add(asyncHandle);
            }
            else
            {
                callback?.Invoke();
            }
        }

        private void Refresh()
        {
            if (m_ItemSlots.Count <= 0 || m_ItemSlots.Count < m_RewardCount)
            {
                return;
            }

            float slotHeight = 170;
            int maxColumnNum = 4;
            int columnNum = m_RewardCount > maxColumnNum ? maxColumnNum : m_RewardCount;
            int rowNum = Mathf.CeilToInt(m_RewardCount / (float)columnNum);

            Vector3 deltaPos = new Vector3(180, 0, 0);
            Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, deltaPos, columnNum);
            Vector3[] pos2List = posList;
            int count = 0;
            if (rowNum > 1)
            {
                count = m_RewardCount - maxColumnNum * (rowNum - 1);
                pos2List = UnityUtility.GetAveragePosition(Vector3.zero, deltaPos, count);
            }

            for (int i = 0; i < m_RewardCount; i++)
            {
                float height = 0;

                if (rowNum > 1)
                {
                    if (rowNum % 2 == 0)
                    {
                        height = slotHeight / 2f + slotHeight * ((rowNum - 2) / 2f);
                    }
                    else
                    {
                        height = slotHeight * ((rowNum - 1) / 2f);
                    }
                }

                for (int j = rowNum; j >= 1; j--)
                {
                    if (i >= m_RewardCount - count - maxColumnNum * (rowNum - j))
                    {
                        height -= (j - 1) * slotHeight;
                        break;
                    }
                }

                if (i < m_RewardCount - count)
                    m_ItemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(posList[i % columnNum].x, height);
                else
                    m_ItemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(pos2List[(i - m_RewardCount + count) % pos2List.Length].x, height);
                
                if(m_RewardCount<4)
                    m_ItemSlots[i].GetComponent<RectTransform>().localScale=new Vector3(1.3f,1.3f,1.3f);
            }
        }

        private void OnCloseButtonClick()
        {
            if (m_IsReservedRewardMenu)
            {
                OnClaimButtonClick();
            }
            else
            {
                GameManager.UI.HideUIForm(this);

                if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
                }
                else
                {
                    MergeManager.Instance.EndActivity();
                }
            }
        }

        private void OnLastChanceButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"), form =>
            {
                GameManager.UI.HideUIForm(this);
            });
        }

        private void OnClaimButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            if (m_Datas.Count > 0)
            {
                for (int i = 0; i < m_Datas.Count; i++)
                {
                    RewardManager.Instance.AddNeedGetReward(m_Datas[i].type, m_Datas[i].num);
                }
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowMergeEndProcess);
                });
            }
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowMergeEndProcess);
            }
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
