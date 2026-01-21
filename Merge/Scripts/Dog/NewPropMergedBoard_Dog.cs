using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class NewPropMergedBoard_Dog : NewPropMergedBoard
    {
        public GameObject m_DogBgEffect;
        public Image m_DogPropImage;
        public GameObject m_DogProp;
        
        private AsyncOperationHandle spriteHandle_Dog;

        public override void Init(string propName, PropLogic propLogic, Action callback, MergeMainMenuBase mainMenu)
        {
            base.Init(propName, propLogic, callback, mainMenu);

            MergeMainMenu_Dog dogMenu = mainMenu as MergeMainMenu_Dog;
            m_DogBgEffect.SetActive(false);
            int rank = propLogic.Rank;
            if (dogMenu.GetCanShowDogPropRank(rank) != 0) 
            {
                m_BgEffect.transform.localPosition = new Vector3(0, 250, 0);
                m_DogProp.transform.localScale = Vector3.one;
                m_DogProp.SetActive(true);
                m_DogProp.transform.localPosition = new Vector3(0, -351, 0);
                m_Prop.transform.localPosition = new Vector3(0, 250, 0);
                m_Prop.transform.localScale = Vector3.one;

                string dogName = "Dog_" + dogMenu.GetCanShowDogPropRank(rank).ToString();
                spriteHandle_Dog = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(dogName, "MergePropAtlas_Dog"), sp =>
                {
                    m_DogPropImage.sprite = sp as Sprite;
                    m_DogPropImage.SetNativeSize();
                });
            }
            else
            {
                m_BgEffect.transform.localPosition = Vector3.zero;
                m_DogProp.SetActive(false);
                m_Prop.transform.localPosition = Vector3.zero;
                m_Prop.transform.localScale = Vector3.one;
            }
        }

        public override void Release()
        {
            UnityUtility.UnloadAssetAsync(spriteHandle_Dog);
            spriteHandle_Dog = default;
            
            base.Release();
        }

        public override void Show()
        {
            if (m_DogProp.activeSelf)
            {
                Transform dogTrans = m_DogProp.transform;
                dogTrans.localScale = Vector3.zero;

                GameManager.Task.AddDelayTriggerTask(0.3f, () =>
                {
                    m_DogBgEffect.SetActive(true);
                    dogTrans.DOScale(1.1f, 0.15f).onComplete = () =>
                    {
                        dogTrans.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
                        {
                            dogTrans.DOScale(1, 0.15f);
                        };
                    };
                });
            }

            if (m_DogProp.activeSelf)
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
            if (m_DogProp.activeSelf)
            {
                m_Prop.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    m_Prop.transform.DOScale(0, 0.2f);
                };

                GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                {
                    m_Main.SetActive(false);

                    MergeMainMenu_Dog dogMenu = m_MainMenu as MergeMainMenu_Dog;
                    Vector3 startPos = m_DogProp.transform.position;
                    Vector3 targetPos = dogMenu.m_DogEntrance.transform.position;
                    Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.08f;

                    m_DogProp.transform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
                    {
                        m_DogProp.transform.DOMove(targetPos, 0.4f).SetEase(Ease.InCubic);
                        m_DogProp.transform.DOScale(0.6f, 0.38f).SetEase(Ease.InCubic).onComplete = () =>
                        {
                            gameObject.SetActive(false);

                            dogMenu.m_DogEntrance.transform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                            {
                                dogMenu.m_DogEntrance.transform.DOScale(1f, 0.15f);
                            };

                            dogMenu.m_ReachEffect.Play();

                            dogMenu.RefreshDogEntrance();

                            if (m_PropLogic.Rank == 4)
                            {
                                if (dogMenu.m_GuideMenu.CurGuide == null)
                                {
                                    dogMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DecorateDogTree);
                                }
                                else
                                {
                                    dogMenu.m_GuideMenu.OnGuideFinished = () =>
                                    {
                                        dogMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DecorateDogTree);
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
