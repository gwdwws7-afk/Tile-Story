using DG.Tweening;
using UnityEngine;

namespace Merge
{
    //This bag's not sturdy! If the HP hits zero, I can't hold on!
    public sealed class Guide_DateMerge3 : Guide
    {
        public Guide_DateMerge3(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DateMerge3;

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

            MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
            if (mainBoard != null)
            {
                MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
                menu.ShowThiefDialog = true;
                m_GuideMenu.Show("DateMerge.GuideDialog3", mainBoard.m_ThiefBoard.m_ThiefHitPos.position, 0);
                m_GuideMenu.ShowRaycastMask(mainBoard.m_ThiefBoard.m_ThiefGuidePos.position, 300, 200, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
            m_GuideMenu.Hide(false);

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                m_GuideMenu?.TriggerGuide(Merge.GuideTriggerType.Guide_TapBox);
            });
        }
    }
}
