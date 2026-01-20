using UnityEngine;

namespace Merge
{
    //Great job! You've got the MAX level bouquet weapon!
    public sealed class Guide_MaxLevelBouquet : Guide
    {
        private PropLogic m_TargetProp;

        public Guide_MaxLevelBouquet(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_MaxLevelBouquet;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideTriggerType.Guide_MaxLevelBouquet && userData != null)
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

            GameManager.Task.AddDelayTriggerTask(0.5f, () =>
            {
                m_IsShowGuideFinished = true;
            });

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null && m_TargetProp != null && m_TargetProp.Square != null)
            {
                m_TargetProp.Prop.SetLayer("UI", 11);

                MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
                menu.ShowThiefDialog = false;
                m_GuideMenu.Show("DateMerge.GuideDialog6", m_TargetProp.Square.transform.position, 0.5f);
                m_GuideMenu.ShowArrowAnim(m_TargetProp.Square.transform.position + new Vector3(0, 0.2f, 0), Quaternion.Euler(0, 0, 0));
                m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide(false);
            if (m_TargetProp != null && m_TargetProp.Prop != null)
                m_TargetProp.Prop.SetLayer("UI", 7);

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                m_GuideMenu?.TriggerGuide(Merge.GuideTriggerType.Guide_TapMaxLevelBouquet, m_TargetProp);
            });
        }
    }
}
