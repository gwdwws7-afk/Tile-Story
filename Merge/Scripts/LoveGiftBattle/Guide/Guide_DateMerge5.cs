using UnityEngine;

namespace Merge
{
    //Hehe, you can't hit me!
    public sealed class Guide_DateMerge5 : Guide
    {
        public Guide_DateMerge5(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DateMerge5;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
            {
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

            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                m_IsShowGuideFinished = true;
            });

            MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
            menu.ShowThiefDialog = true;
            m_GuideMenu.Show("DateMerge.GuideDialog5", Vector3.zero, 0);
            m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();

            base.OnGuideFinish();

            MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
            if (mainBoard != null)
                mainBoard.m_ThiefBoard.ShowThiefJokeAnim();
        }
    }
}
