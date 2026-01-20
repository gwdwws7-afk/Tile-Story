using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeStageRewardMenu_DigTreasure : PopupMenuForm
    {
        public DelayButton m_CloseButton;
        public ScrollArea m_ScrollArea;
        public MergeMaxStageRewardColumn m_MaxStageColumn;
        public Transform m_DetectPoint;
        public ClockBar m_ClockBar;

        private GameObject[] columnContainer;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);

            IDataTable<DRMergeStageReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeStageReward>(MergeManager.Instance.GetMergeDataTableName());
            var datas = dataTable.GetAllDataRows();

            columnContainer = new GameObject[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                columnContainer[i] = new GameObject($"columnContainer{i}");
                columnContainer[i].transform.SetParent(m_ScrollArea.content);
                columnContainer[i].transform.localPosition = Vector3.zero;
                columnContainer[i].transform.localScale = Vector3.one;
            }

            for (int i = datas.Length - 1; i >= 0; i--)
            {
                m_ScrollArea.AddColumnLast(new MergeStageRewardScrollColumn("MergeStageRewardColumn", datas[i], columnContainer[datas.Length - 1 - i].transform, m_ScrollArea, 200f));
            }
            m_ScrollArea.currentIndex = datas.Length - MergeManager.PlayerData.GetDigTreasureRewardStage();
            m_ScrollArea.OnInit(GetComponent<RectTransform>());

            m_MaxStageColumn.Initialize(datas[datas.Length - 1]);
            m_MaxStageColumn.gameObject.SetActive(m_DetectPoint.position.y < m_ScrollArea.scrollRect.content.position.y);

            m_ScrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);

            if (DateTime.Now < MergeManager.Instance.GetActivityEndTime() && GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel())
            {
                m_ClockBar.OnReset();
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(MergeManager.Instance.GetActivityEndTime());
            }
            else
            {
                m_ClockBar.SetFinishState();
            }
        }

        public override void OnReset()
        {
            m_ScrollArea.scrollRect.onValueChanged.RemoveAllListeners();

            m_CloseButton.OnReset();

            m_ScrollArea.OnRelease();
            m_MaxStageColumn.Release();

            m_ClockBar.OnReset();

            base.OnReset();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ScrollArea.OnUpdate(elapseSeconds, realElapseSeconds);
            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnScrollViewChanged(Vector2 arg)
        {
            m_MaxStageColumn.gameObject.SetActive(m_DetectPoint.position.y < m_ScrollArea.scrollRect.content.position.y);
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_ClockBar.SetFinishState();
        }
    }
}
