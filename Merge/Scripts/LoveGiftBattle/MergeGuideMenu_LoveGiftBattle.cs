using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeGuideMenu_LoveGiftBattle : MergeGuideMenu
    {
        public GameObject m_Police, m_Thief;

        public bool ShowThiefDialog
        {
            get;
            set;
        }

        protected override void InitializeGuideList()
        {
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DateMerge1)) m_GuideList.Add(new Guide_DateMerge1(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DateMerge2)) m_GuideList.Add(new Guide_DateMerge2(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DateMerge3)) m_GuideList.Add(new Guide_DateMerge3(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapBox)) m_GuideList.Add(new Guide_DateMerge_TapBox(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DateMerge5)) m_GuideList.Add(new Guide_DateMerge5(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_MaxLevel)) m_GuideList.Add(new Guide_DateMerge_MaxLevel(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward)) m_GuideList.Add(new Guide_DateMerge_TapMaxReward(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DragMerge)) m_GuideList.Add(new Guide_DateMerge_DragMerge(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_MergeBalloon)) m_GuideList.Add(new Guide_MergeBalloon(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_MaxLevelBouquet)) m_GuideList.Add(new Guide_MaxLevelBouquet(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxLevelBouquet)) m_GuideList.Add(new Guide_TapMaxLevelBouquet(this));
        }

        public override void Show(string content, Vector3 pos, float delta = 0.4f)
        {
            PromptBoxShowDirection direction = PromptBoxShowDirection.Right;
            //if (pos.x >= 0)
            //{
            //    m_UncleTrans.localScale = new Vector3(1, 1, 1);
            //    m_UncleTrans.localPosition = new Vector3(-307f, m_UncleTrans.localPosition.y, 0);
            //    direction = PromptBoxShowDirection.Right;
            //}
            //else
            //{
            //    m_UncleTrans.localScale = new Vector3(-1, 1, 1);
            //    m_UncleTrans.localPosition = new Vector3(307f, m_UncleTrans.localPosition.y, 0);
            //    direction = PromptBoxShowDirection.Left;
            //}
            m_UncleTrans.localScale = new Vector3(1, 1, 1);
            m_UncleTrans.localPosition = new Vector3(-307f, m_UncleTrans.localPosition.y, 0);
            direction = PromptBoxShowDirection.Right;
            m_PromptBox.SetText(content);
            gameObject.SetActive(true);
            float deltaY = Mathf.Abs(m_DialogTrans.position.y - pos.y);
            Vector3 diglogPos;
            if (deltaY < delta)
            {
                if (m_DialogTrans.position.y > pos.y)
                    diglogPos = m_DialogTrans.position + new Vector3(0, delta - deltaY, 0);
                else
                    diglogPos = m_DialogTrans.position - new Vector3(0, delta - deltaY, 0);
            }
            else
            {
                diglogPos = m_DialogTrans.position;
            }
            m_PromptBox.ShowPromptBox(direction, diglogPos);

            float promptX = m_PromptBox.transform.localPosition.x;
            float promptY = m_PromptBox.transform.localPosition.y;
            if (direction == PromptBoxShowDirection.Right)
            {
                //m_UncleTrans.localPosition = new Vector3(-907f, m_UncleTrans.localPosition.y, 0);
                //m_UncleTrans.DOLocalMoveX(-307f, 0.4f);

                m_PromptBox.transform.localPosition = new Vector3(promptX - 1100, promptY, 0);
                m_PromptBox.transform.DOLocalMoveX(promptX, 0.4f);
            }
            else
            {
                //m_UncleTrans.localPosition = new Vector3(907f, -513f, 0);
                //m_UncleTrans.DOLocalMoveX(307f, 0.4f);

                m_PromptBox.transform.localPosition = new Vector3(promptX + 1100, promptY, 0);
                m_PromptBox.transform.DOLocalMoveX(promptX, 0.4f);
            }
            m_UncleTrans.gameObject.SetActive(true);
            m_PromptBox.gameObject.SetActive(true);
            m_SkipButton.gameObject.SetActive(true);

            m_Police.SetActive(!ShowThiefDialog);
            m_Thief.SetActive(ShowThiefDialog);
        }
    }
}
