using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeInfoMenu_DigTreasure : PopupMenuForm
    {
        public DelayButton m_CloseButton;
        public MergeInfoScrollView[] m_InfoViews;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);

            m_InfoViews[0].Init(new int[] { 70101, 70102, 70103, 70104 });
            m_InfoViews[1].Init(new int[] { 90101, 90102, 90103, 90104 });
            m_InfoViews[2].Init(new int[] { 80101, 80102, 80103, 80104 });
            m_InfoViews[3].Init(MergeManager.PlayerData.GetCurrentMaxMergeStage());
            m_InfoViews[4].Init(new int[] { 60101, 60102, 60103, 60104 });
            m_InfoViews[5].Init(new int[] { 100101, 100201, 100301, 100401, 100501, 100601 });
            m_InfoViews[6].Init(new int[] { 100701, 30101 });
        }

        public override void OnReset()
        {
            m_CloseButton.OnReset();

            base.OnReset();
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }
    }
}
