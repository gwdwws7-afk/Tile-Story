using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class Guide_Web_Halloween : Guide
    {
        public Guide_Web_Halloween(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_Web;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
                return true;

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
            Square webSquare = MergeManager.Merge.GetSquare(1, 4);

            if (webSquare == null || webSquare.FilledProp == null || webSquare.FilledProp.PropId != 10102)
            {
                m_IsGuideCompleted = true;
                return;
            }
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            //����û���ǰ�ϵ��������ý̳�
            Square webSquare = MergeManager.Merge.GetSquare(1, 4);
            List<PropLogic> targetPropList = MergeManager.Merge.GetPropListOnChessboard(10102);

            if (webSquare == null || webSquare.FilledProp == null || webSquare.FilledProp.PropId != 10102 || targetPropList.Count == 0 || targetPropList[0].Square == null)
            {
                m_IsGuideCompleted = true;
                return;
            }

            Square targetSquare = targetPropList[0].Square;
            int rowDelta = Mathf.Abs(webSquare.m_Row - targetSquare.m_Row) + 1;
            int colDelta = Mathf.Abs(webSquare.m_Col - targetSquare.m_Col) + 1;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("Merge.Guide9", webSquare.transform.position, 0.3f);
                m_GuideMenu.ShowFingerAnim(targetSquare.transform.position, webSquare.transform.position);
                m_GuideMenu.ShowRaycastMask((webSquare.transform.position + targetSquare.transform.position) / 2, colDelta * 85, rowDelta * 85, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();

            base.OnGuideFinish();
        }
    }   
}
