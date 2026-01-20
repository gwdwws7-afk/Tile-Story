using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //You've got a swimfin!Drag to merge two of them!
    public sealed class Guide_MergeBalloon : Guide
    {
        private PropLogic m_TargetProp;

        public Guide_MergeBalloon(MergeGuideMenu guideMenu) :base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_MergeBalloon;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType && userData != null)
            {
                m_TargetProp = (PropLogic)userData;

                return true;
            }

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override GuideState GetGuideState()
        {
            if (m_IsShowGuideFinished)
            {
                if (m_TargetProp == null || m_TargetProp.Prop == null || m_TargetProp.PropId != 50101 || m_TargetProp.AttachmentId != 0)  
                    return GuideState.Completed;
            }

            return base.GetGuideState();
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            Square square1 = m_TargetProp.Square;

            //获取最近的气球道具
            PropLogic prop = null;
            float minDis = 1000;
            List<PropLogic> propList = MergeManager.Merge.GetPropListOnChessboard(50101, false);
            foreach (var item in propList)
            {
                if (item != m_TargetProp && item.Square != null && !item.IsPetrified && !item.IsSilenced && item.AttachmentId == 0)   
                {
                    float dis = Mathf.Pow(square1.m_Row - item.Square.m_Row, 2) + Mathf.Pow(square1.m_Col - item.Square.m_Col, 2);
                    if (dis < minDis) 
                    {
                        minDis = dis;
                        prop = item;
                    }
                }
            }

            if (prop == null || prop.Square == null || prop.Prop == null) 
            {
                m_IsGuideCompleted = true;
                return;
            }
            Square square2 = prop.Square;

            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 50101 || square2.FilledProp.PropId != 50101) 
            {
                m_IsGuideCompleted = true;
                return;
            }

            int rowDelta = Mathf.Abs(square1.m_Row - square2.m_Row) + 1;
            int colDelta = Mathf.Abs(square1.m_Col - square2.m_Col) + 1;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                string guideContent = "DateMerge.GuideMerge";

                MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
                menu.ShowThiefDialog = false;
                m_GuideMenu.Show(guideContent, (square1.transform.position + square2.transform.position) / 2);
                m_GuideMenu.ShowFingerAnim(square1.transform.position, square2.transform.position);
                m_GuideMenu.ShowRaycastMask((square1.transform.position + square2.transform.position) / 2, colDelta * 75, rowDelta * 75, 0.8f);
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
