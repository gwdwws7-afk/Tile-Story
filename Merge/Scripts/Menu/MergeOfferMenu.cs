using System;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 合成无尽宝箱界面
    /// </summary>
    public class MergeOfferMenu : PopupMenuForm
    {
        public ScrollArea m_ScrollArea;
        public Transform m_EffectsRoot;
        public DelayButton m_CloseButton;
        public ClockBar m_ClockBar;

        private int m_NeedShowMoveInColumn = 0;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);

            //MergeManager.PlayerData.SetCurMergeOfferLevel(1);

            IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
            DRMergeOffer[] allData = dataTable.GetAllDataRows();

            m_NeedShowMoveInColumn = 3;
            int startIndex = MergeManager.PlayerData.GetCurMergeOfferLevel() - 1;
            int endIndex = startIndex + 3 < allData.Length ? startIndex + 3 : allData.Length;
            for (int i = startIndex; i < endIndex; i++)
            {
                m_ScrollArea.AddColumnLast(new MergeOfferScrollColumn(MergeManager.Instance.GetMergeOfferColumnName("MergeOfferColumn"), allData[i], m_ScrollArea.recycleWidth, this));
            }
            m_ScrollArea.OnSpawnAction -= OnScrollColumnSpawn;
            m_ScrollArea.OnSpawnAction += OnScrollColumnSpawn;
            m_ScrollArea.OnInit(GetComponent<RectTransform>());

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
            m_ScrollArea.OnReset();

            base.OnReset();
        }

        public override void OnRelease()
        {
            m_ScrollArea.OnRelease();

            base.OnRelease();
        }

        private void Update()
        {
            m_ClockBar.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnScrollColumnSpawn(ScrollColumn column)
        {
            if (m_NeedShowMoveInColumn > 0 && column != null && column.Instance != null)
            {
                column.Instance.transform.localScale = Vector3.one;
                column.Instance.GetComponent<MergeOfferColumn>().ShowMoveInAnim(m_NeedShowMoveInColumn);
            }
            m_NeedShowMoveInColumn--;
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.RefreshSale();
            }
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
        }
    }
}
