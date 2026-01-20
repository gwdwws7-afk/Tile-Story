

namespace Merge
{
    public enum GuideState : byte
    {
        None,

        //准备展示
        PrepareShow,

        //展示中
        Showing,

        //完成
        Completed,

        //延后
        Delay,
    }

    /// <summary>
    /// 教程基类
    /// </summary>
    public abstract class Guide
    {
        protected MergeGuideMenu m_GuideMenu;
        protected bool m_IsShowGuideFinished;
        protected bool m_IsGuideCompleted;

        public Guide(MergeGuideMenu guideMenu)
        {
            m_GuideMenu = guideMenu;
            m_IsShowGuideFinished = false;
            m_IsGuideCompleted = false;
        }

        public abstract GuideTriggerType GuideType { get; }

        public int GuideId { get => (int)GuideType; }

        public virtual int GuidePriority { get => 1; }

        public virtual PropLogic TargetProp { get => null; }

        public abstract bool CheckCanTrigger(GuideTriggerType triggerType, object userData);

        public abstract void CheckCanComplete(GuideTriggerType triggerType, object userData);

        public virtual void OnUpdate()
        {
        }

        public GuideState RefreshGuideState()
        {
            if (!m_IsShowGuideFinished)
                return GuideState.PrepareShow;

            if (m_IsGuideCompleted)
                return GuideState.Completed;

            return GetGuideState();
        }

        public virtual GuideState GetGuideState()
        {
            return GuideState.None;
        }

        public virtual void OnTriggerGuide()
        {
        }

        public virtual void OnShowFinish()
        {
            m_IsShowGuideFinished = true;
        }

        public virtual void OnGuideFinish()
        {
            Clear();
        }

        public virtual void Clear()
        {
            m_IsShowGuideFinished = false;
            m_IsGuideCompleted = false;
        }
    }
}
