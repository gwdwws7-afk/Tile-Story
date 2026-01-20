using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeGuideMenu_DigTreasure : MergeGuideMenu
    {
        protected override void InitializeGuideList()
        {
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog1)) m_GuideList.Add(new Guide_DigDialog1(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog2)) m_GuideList.Add(new Guide_DigDialog2(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog3)) m_GuideList.Add(new Guide_DigDialog3(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog4)) m_GuideList.Add(new Guide_DigDialog4(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog5)) m_GuideList.Add(new Guide_DigDialog5(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog6)) m_GuideList.Add(new Guide_DigDialog6(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DigDialog7)) m_GuideList.Add(new Guide_DigDialog7(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward)) m_GuideList.Add(new Guide_TapMaxReward(this));
        }

        public override void Show(string content, Vector3 pos, float delta = 0.4f)
        {
            m_UncleTrans.localScale = new Vector3(1, 1, 1);
            m_UncleTrans.localPosition = new Vector3(-307f, m_UncleTrans.localPosition.y, 0);
            m_PromptBox.SetText(content);
            gameObject.SetActive(true);
            Vector3 diglogPos = new Vector3(m_DialogTrans.position.x, pos.y + delta, 0);
            m_PromptBox.ShowPromptBox(PromptBoxShowDirection.Right, diglogPos);

            float promptX = m_PromptBox.transform.localPosition.x;
            float promptY = m_PromptBox.transform.localPosition.y;

            m_PromptBox.transform.localPosition = new Vector3(promptX - 1100, promptY, 0);
            m_PromptBox.transform.DOLocalMoveX(promptX, 0.4f);

            m_UncleTrans.gameObject.SetActive(true);
            m_PromptBox.gameObject.SetActive(true);
            m_SkipButton.gameObject.SetActive(true);
        }
    }
}
