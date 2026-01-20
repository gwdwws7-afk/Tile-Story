using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class NewPropMergedBoard_Christmas : NewPropMergedBoard
    {
        public GameObject m_ChristmasBgEffect;
        public Image m_ChristmasPropImage;
        public GameObject m_ChristmasProp;
        
        private AsyncOperationHandle spriteHandle_Christmas;

        public override void Init(string propName, PropLogic propLogic, Action callback, MergeMainMenuBase mainMenu)
        {
            base.Init(propName, propLogic, callback, mainMenu);

            MergeMainMenu_Christmas christmasMenu = mainMenu as MergeMainMenu_Christmas;
            m_ChristmasBgEffect.SetActive(false);
            int rank = propLogic.Rank;
            if (christmasMenu.GetCanShowChristmasPropRank(rank) != 0) 
            {
                m_BgEffect.transform.localPosition = new Vector3(0, 250, 0);
                m_ChristmasProp.transform.localScale = Vector3.one;
                m_ChristmasProp.SetActive(true);
                m_ChristmasProp.transform.localPosition = new Vector3(0, -351, 0);
                m_Prop.transform.localPosition = new Vector3(0, 250, 0);
                m_Prop.transform.localScale = Vector3.one;

                string christmasName = "Christmas_" + christmasMenu.GetCanShowChristmasPropRank(rank).ToString();
                spriteHandle_Christmas = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(christmasName, "MergePropAtlas_Christmas"), sp =>
                {
                    m_ChristmasPropImage.sprite = sp as Sprite;
                    m_ChristmasPropImage.SetNativeSize();
                });
            }
            else
            {
                m_BgEffect.transform.localPosition = Vector3.zero;
                m_ChristmasProp.SetActive(false);
                m_Prop.transform.localPosition = Vector3.zero;
                m_Prop.transform.localScale = Vector3.one;
            }
        }

        public override void Release()
        {
            UnityUtility.UnloadAssetAsync(spriteHandle_Christmas);
            spriteHandle_Christmas = default;
            
            base.Release();
        }

        public override void Show()
        {
            if (m_ChristmasProp.activeSelf)
            {
                Transform christmasTrans = m_ChristmasProp.transform;
                christmasTrans.localScale = Vector3.zero;

                GameManager.Task.AddDelayTriggerTask(0.3f, () =>
                {
                    m_ChristmasBgEffect.SetActive(true);
                    christmasTrans.DOScale(1.1f, 0.15f).onComplete = () =>
                    {
                        christmasTrans.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
                        {
                            christmasTrans.DOScale(1, 0.15f);
                        };
                    };
                });
            }

            if (m_ChristmasProp.activeSelf)
            {
                m_Tip.transform.DOScale(1f, 0.35f).SetDelay(0.6f).SetEase(Ease.OutBack);

                GameManager.Task.AddDelayTriggerTask(0.9f, () =>
                {
                    m_BgButton.interactable = true;
                });
            }
            else
            {
                m_Tip.transform.DOScale(1f, 0.35f).SetDelay(0.3f).SetEase(Ease.OutBack);

                GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                {
                    m_BgButton.interactable = true;
                });
            }
            
            base.Show();
        }

        public override void Hide()
        {
            if (m_ChristmasProp.activeSelf)
            {
                m_Prop.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    m_Prop.transform.DOScale(0, 0.2f);
                };

                GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                {
                    m_Main.SetActive(false);

                    MergeMainMenu_Christmas christmasMenu = m_MainMenu as MergeMainMenu_Christmas;
                    Vector3 startPos = m_ChristmasProp.transform.position;
                    Vector3 targetPos = christmasMenu.m_ChristmasEntrance.transform.position;
                    Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.08f;

                    m_ChristmasProp.transform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
                    {
                        m_ChristmasProp.transform.DOMove(targetPos, 0.4f).SetEase(Ease.InCubic);
                        m_ChristmasProp.transform.DOScale(0.6f, 0.38f).SetEase(Ease.InCubic).onComplete = () =>
                        {
                            gameObject.SetActive(false);

                            christmasMenu.m_ChristmasEntrance.transform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                            {
                                christmasMenu.m_ChristmasEntrance.transform.DOScale(1f, 0.15f);
                            };

                            christmasMenu.m_ReachEffect.Play();

                            christmasMenu.RefreshChristmasEntrance();

                            if (m_PropLogic.Rank == 4)
                            {
                                if (christmasMenu.m_GuideMenu.CurGuide == null)
                                {
                                    christmasMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DecorateChristmasTree);
                                }
                                else
                                {
                                    christmasMenu.m_GuideMenu.OnGuideFinished = () =>
                                    {
                                        christmasMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DecorateChristmasTree);
                                    };
                                }
                            }
                        };
                    };
                });
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (m_HideAction != null)
            {
                m_HideAction.Invoke();
                m_HideAction = null;
            }

            if (m_PropLogic.PropId == MergeManager.Instance.MaxPropId)
            {
                GameManager.Task.AddDelayTriggerTask(1.2f, () =>
                {
                    m_MainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_TapMaxReward, m_PropLogic);
                });
            }
        }
    }
}
