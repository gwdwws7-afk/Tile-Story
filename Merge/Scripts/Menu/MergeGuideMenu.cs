using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeGuideMenu : MonoBehaviour
    {
        public const string GuideCompletePrefix = "Merge.GuideIsComplete_";

        public Transform m_UncleTrans;
        public Transform m_DialogTrans;
        public Button m_SkipButton;
        public TextPromptBox m_PromptBox;
        public GameObject m_Arrow;
        public Transform m_Finger;
        public SkeletonAnimation m_FingerSpine;
        public RaycastMask m_RaycastMask;

        public static GuideTriggerType s_CurGuideId = GuideTriggerType.None;
        protected List<Guide> m_GuideList = new List<Guide>();
        private Guide m_CurGuide;
        private bool m_IsShowingFinger;

        public Action OnGuideFinished;

        public Guide CurGuide => m_CurGuide;

        public void Initialize()
        {
            s_CurGuideId = GuideTriggerType.None;
            m_CurGuide = null;
            OnGuideFinished = null;

            m_GuideList.Clear();
            InitializeGuideList();

            m_SkipButton.SetBtnEvent(OnSkipButtonClick);
        }

        protected virtual void InitializeGuideList()
        {
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapBox)) m_GuideList.Add(new Guide_TapBox(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DragMerge)) m_GuideList.Add(new Guide_DragMerge(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_KeepMerging)) m_GuideList.Add(new Guide_KeepMerging(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_SpecialItems)) m_GuideList.Add(new Guide_SpecialItems(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapSpecialItem)) m_GuideList.Add(new Guide_TapSpecialItem(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_MaxLevel)) m_GuideList.Add(new Guide_MaxLevel(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_BoardFull)) m_GuideList.Add(new Guide_BoardFull(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward)) m_GuideList.Add(new Guide_TapMaxReward(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_Web)) m_GuideList.Add(new Guide_Web(this));
        }

        private void Update()
        {
            if (m_CurGuide != null)
            {
                m_CurGuide.OnUpdate();

                GuideState state = m_CurGuide.RefreshGuideState();
                if (state == GuideState.Delay)
                {
                    m_CurGuide.OnGuideFinish();
                    m_CurGuide = null;
                    s_CurGuideId = GuideTriggerType.None;
                }
                else if (state == GuideState.Completed)
                {
                    int guideId = m_CurGuide.GuideId;
                    m_GuideList.Remove(m_CurGuide);
                    m_CurGuide.OnGuideFinish();
                    m_CurGuide = null;
                    s_CurGuideId = GuideTriggerType.None;

                    PlayerPrefs.SetInt(MergeManager.Instance.GetMergeGuideName(GuideCompletePrefix) + guideId.ToString(), 1);

                    if (OnGuideFinished != null)
                    {
                        OnGuideFinished.Invoke();
                        OnGuideFinished = null;
                    }
                }
            }
        }

        public void TriggerGuide(GuideTriggerType triggerType, object userData = null)
        {
            if (m_CurGuide != null)
            {
                if (m_CurGuide.GuideType == triggerType)
                    return;

                m_CurGuide.CheckCanComplete(triggerType, userData);
            }

            try
            {
                foreach (Guide guide in m_GuideList)
                {
                    if (guide != m_CurGuide && guide.CheckCanTrigger(triggerType, userData))
                    {
                        if (m_CurGuide == null || guide.GuidePriority > m_CurGuide.GuidePriority)
                        {
                            if (m_CurGuide != null)
                                m_CurGuide.OnGuideFinish();

                            m_CurGuide = guide;
                            s_CurGuideId = m_CurGuide.GuideType;
                            m_CurGuide.OnTriggerGuide();
                            Log.Info("Start guide {0}", m_CurGuide.GuideId);
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Guide Trigger error:" + exception.Message);
            }
        }

        public void FinishGuide(GuideTriggerType type)
        {
            if (m_CurGuide != null && m_CurGuide.GuideType == type)
            {
                int guideId = m_CurGuide.GuideId;
                m_GuideList.Remove(m_CurGuide);
                m_CurGuide.OnGuideFinish();
                m_CurGuide = null;
                s_CurGuideId = GuideTriggerType.None;

                PlayerPrefs.SetInt(MergeManager.Instance.GetMergeGuideName(GuideCompletePrefix) + guideId.ToString(), 1);

                if (OnGuideFinished != null)
                {
                    OnGuideFinished.Invoke();
                    OnGuideFinished = null;
                }
            }
        }

        public virtual void Show(string content, Vector3 pos, float delta = 0.4f)
        {
            PromptBoxShowDirection direction = PromptBoxShowDirection.Right;
            if (pos.x >= 0)
            {
                m_UncleTrans.localScale = new Vector3(1, 1, 1);
                m_UncleTrans.localPosition = new Vector3(-307f, m_UncleTrans.localPosition.y, 0);
                direction = PromptBoxShowDirection.Right;
            }
            else
            {
                m_UncleTrans.localScale = new Vector3(-1, 1, 1);
                m_UncleTrans.localPosition = new Vector3(307f, m_UncleTrans.localPosition.y, 0);
                direction = PromptBoxShowDirection.Left;
            }
            m_PromptBox.SetText(content);
            m_RaycastMask.DOKill();
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
                m_UncleTrans.localPosition = new Vector3(-907f, m_UncleTrans.localPosition.y, 0);
                m_UncleTrans.DOLocalMoveX(-307f, 0.4f);

                m_PromptBox.transform.localPosition = new Vector3(promptX + 600, promptY, 0);
                m_PromptBox.transform.DOLocalMoveX(promptX, 0.4f);
            }
            else
            {
                m_UncleTrans.localPosition = new Vector3(907f, -513f, 0);
                m_UncleTrans.DOLocalMoveX(307f, 0.4f);

                m_PromptBox.transform.localPosition = new Vector3(promptX - 600, promptY, 0);
                m_PromptBox.transform.DOLocalMoveX(promptX, 0.4f);
            }
            m_UncleTrans.gameObject.SetActive(true);
            m_PromptBox.gameObject.SetActive(true);
            m_SkipButton.gameObject.SetActive(true);
        }

        public void Hide(bool shutdown = true)
        {
            m_UncleTrans.DOKill();
            m_PromptBox.transform.DOKill();
            m_UncleTrans.gameObject.SetActive(false);
            m_PromptBox.gameObject.SetActive(false);
            m_Arrow.SetActive(false);
            if (m_IsShowingFinger)
            {
                m_IsShowingFinger = false;
                StopAllCoroutines();
                m_Finger.DOKill();
                m_Finger.gameObject.SetActive(false);
            }
            m_RaycastMask.SetFocusTarget(Vector3.zero, 0, 0, 0);

            if (shutdown)
            {
                m_SkipButton.gameObject.SetActive(false);

                m_RaycastMask.DOFade(0, 0.2f).onComplete = () =>
                {
                    gameObject.SetActive(false);
                };
            }
        }

        public void ShowFullMask()
        {
            Hide();
            gameObject.SetActive(true);
        }

        public void ShowArrowAnim(Vector3 arrowPos, Quaternion quaternion)
        {
            m_Arrow.transform.SetPositionAndRotation(arrowPos, quaternion);
            m_Arrow.SetActive(true);
        }

        public void ShowFingerAnim(Vector3 startPos, Vector3 endPos)
        {
            if (m_IsShowingFinger)
            {
                m_IsShowingFinger = false;
                StopAllCoroutines();
                m_Finger.DOKill();
                m_Finger.gameObject.SetActive(false);
            }

            m_IsShowingFinger = true;
            StartCoroutine(ShowFingerAnimCor(startPos, endPos));
        }

        IEnumerator ShowFingerAnimCor(Vector3 startPos, Vector3 endPos)
        {
            m_FingerSpine.Initialize(false);

            m_Finger.DOKill();
            m_Finger.transform.position = startPos;
            m_Finger.gameObject.SetActive(true);

            MeshRenderer meshRender = m_FingerSpine.GetComponent<MeshRenderer>();

            bool isClickAnim = startPos == endPos;

            WaitForSeconds waitForSecond1 = new WaitForSeconds(1f);
            WaitForSeconds waitForSecond2 = new WaitForSeconds(0.35f);

            while (true)
            {
                m_Finger.transform.position = startPos;
                m_FingerSpine.AnimationState.SetAnimation(0, "02", false);

                yield return null;

                meshRender.material.DOKill();
                meshRender.material.SetColor("_Color", Color.white);

                if (!isClickAnim)
                {
                    yield return new WaitForSeconds(0.18f);

                    m_Finger.DOMove(endPos, 1f);
                }

                yield return waitForSecond1;

                meshRender.material.DOFade(0, 0.3f);

                yield return waitForSecond2;
            }
        }

        public bool ShowRaycastMask(Vector3 focusPos, float sliderX, float sliderY, float alpha = 0f, float edgeWidth = 0f)
        {
            //m_RaycastMask.color = new Color(m_RaycastMask.color.r, m_RaycastMask.color.g, m_RaycastMask.color.b, alpha);
            m_RaycastMask.DOKill();
            m_RaycastMask.DOFade(alpha, 0.2f);

            var group = GameManager.UI.GetUIGroup(UIFormType.CenterUI);
            if (group != null)
            {
                if (m_RaycastMask.SetFocusTarget(group.transform.parent.GetComponent<RectTransform>(), focusPos, sliderX, sliderY, edgeWidth))
                {
                    m_RaycastMask.gameObject.SetActive(true);
                    return true;
                }
            }

            return false;
        }

        public void OnSkipButtonClick()
        {
            if (m_CurGuide != null)
            {
                int guideId = m_CurGuide.GuideId;
                m_GuideList.Remove(m_CurGuide);
                m_CurGuide.OnGuideFinish();
                m_CurGuide = null;
                s_CurGuideId = GuideTriggerType.None;

                PlayerPrefs.SetInt(MergeManager.Instance.GetMergeGuideName(GuideCompletePrefix) + guideId.ToString(), 1);
            }
            else
            {
                Hide();
            }
        }

        public static bool CheckGuideIsComplete(GuideTriggerType type)
        {
            return PlayerPrefs.GetInt(MergeManager.Instance.GetMergeGuideName(GuideCompletePrefix) + ((int)type).ToString(), 0) == 1;
        }
    }
}
