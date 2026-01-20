namespace Merge
{
    //You've got a swimfin!Drag to merge two of them!
    public sealed class Guide_DragMerge : Guide
    {
        public Guide_DragMerge(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DragMerge;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
                return true;

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
            Square square1 = MergeManager.Merge.GetSquare(3, 1);
            Square square2 = MergeManager.Merge.GetSquare(3, 2);
            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 10101 || square2.FilledProp.PropId != 10101)
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
            Square square1 = MergeManager.Merge.GetSquare(3, 1);
            Square square2 = MergeManager.Merge.GetSquare(3, 2);
            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 10101 || square2.FilledProp.PropId != 10101)
            {
                m_IsGuideCompleted = true;
                return;
            }

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                if (MergeManager.Instance.Theme == MergeTheme.Christmas) 
                    m_GuideMenu.Show("Merge.Guide2_2", square1.transform.position, 0.3f);
                else
                    m_GuideMenu.Show("Merge.Guide2_ST", square1.transform.position, 0.3f);
                m_GuideMenu.ShowFingerAnim(square1.transform.position, square2.transform.position);
                m_GuideMenu.ShowRaycastMask((square1.transform.position + square2.transform.position) / 2, 170, 85, 0.8f);
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
