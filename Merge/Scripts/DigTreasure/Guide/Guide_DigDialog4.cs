using UnityEngine;

namespace Merge
{
    //There's more treasure below! Dig deeper to explore further
    public sealed class Guide_DigDialog4 : Guide
    {
        public Guide_DigDialog4(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog4;

        private int delayTaskId;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
                return true;

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override GuideState GetGuideState()
        {
            if (m_IsShowGuideFinished)
            {
                if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        m_IsGuideCompleted = true;
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                    {
                        m_IsGuideCompleted = true;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        m_IsGuideCompleted = true;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        m_IsGuideCompleted = true;
                    }
                }
            }

            return base.GetGuideState();
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            delayTaskId = GameManager.Task.AddDelayTriggerTask(0.2f, () =>
              {
                  m_IsShowGuideFinished = true;
              });

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("DigMerge.GuideDialog4", MergeManager.Merge.GetSquare(2, 1).transform.position, 0.27f);
                m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
            m_GuideMenu.Hide();

            base.OnGuideFinish();
        }
    }
}
