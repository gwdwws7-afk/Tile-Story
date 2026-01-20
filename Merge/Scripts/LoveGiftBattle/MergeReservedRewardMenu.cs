using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class MergeReservedRewardMenu : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_OkButton, m_TipButton;
        public Transform m_SlotRoot;

        private List<ItemData> m_Datas = null;
        private List<MergePromptItemSlot> m_ItemSlots = new List<MergePromptItemSlot>();
        private List<AsyncOperationHandle> m_AsyncHandlesList = new List<AsyncOperationHandle>();
        private int m_RewardCount;

        protected virtual string SlotName => "MergePromptItemSlot";

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_OkButton.OnInit(OnOkButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

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
                UnityUtility.UnloadInstance(m_AsyncHandlesList[i]);
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
                var asyncHandle = UnityUtility.InstantiateAsync(SlotName, m_SlotRoot, obj =>
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
            }
        }

        private void OnCloseButtonClick()
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

        private void OnOkButtonClick()
        {
            OnCloseButtonClick();
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
