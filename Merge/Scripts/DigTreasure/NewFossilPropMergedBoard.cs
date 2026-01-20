using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class NewFossilPropMergedBoard : MonoBehaviour
    {
        public GameObject m_Main;
        public Image m_PropImage;
        public GameObject m_Prop;
        public GameObject m_Title;
        public GameObject m_Tip;
        public Button m_BgButton;

        public GameObject m_BgEffect;

        private string m_PropName;
        private PropLogic m_PropLogic;
        private MergeMainMenu_DigTreasure m_MainMenu;
        public Action m_HideAction;
        private AsyncOperationHandle spriteHandle;
        private AsyncOperationHandle spriteHandle_Christmas;

        public void Init(string propName, PropLogic propLogic, Action callback, MergeMainMenu_DigTreasure mainMenu)
        {
            m_PropName = propName;
            m_PropLogic = propLogic;
            m_MainMenu = mainMenu;

            m_Main.SetActive(true);

            m_BgButton.onClick.RemoveAllListeners();
            m_BgButton.onClick.AddListener(OnBgButtonClick);
            m_BgButton.gameObject.SetActive(true);

            m_BgButton.interactable = false;
            string name = propName;
            spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(name, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            {
                m_PropImage.sprite = sp as Sprite;
                m_PropImage.SetNativeSize();

                callback?.Invoke();
            });

            m_BgEffect.transform.localPosition = new Vector3(0, 0, 0);
            m_Prop.transform.localPosition = new Vector3(0, 0, 0);
            m_Prop.transform.localScale = Vector3.one;
        }

        public void Release()
        {
            UnityUtility.UnloadAssetAsync(spriteHandle);
            UnityUtility.UnloadAssetAsync(spriteHandle_Christmas);
        }

        public void Show()
        {
            Transform titleTrans = m_Title.transform;
            Transform tipTrans = m_Tip.transform;
            Transform trans = m_Prop.transform;

            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;
            trans.localScale = Vector3.zero;
            m_BgEffect.SetActive(true);
            gameObject.SetActive(true);

            trans.DOScale(1.1f, 0.15f).SetDelay(0.1f).onComplete = () =>
            {
                trans.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
                {
                    trans.DOScale(1, 0.15f);
                    m_BgButton.interactable = true;
                };

                ShowEffect();
            };

            m_Title.gameObject.SetActive(true);
            titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
            {
                titleTrans.DOScale(1f, 0.2f);
            };

            tipTrans.DOScale(1f, 0.35f).SetDelay(0.3f).SetEase(Ease.OutBack);

            GameManager.Task.AddDelayTriggerTask(0.6f, () =>
            {
                m_BgButton.interactable = true;
            });

            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
            });
        }

        public void Hide()
        {
            //gameObject.SetActive(false);
            m_BgButton.gameObject.SetActive(false);
            m_BgEffect.SetActive(false);
            m_Title.transform.localScale = Vector3.zero;
            m_Tip.transform.localScale = Vector3.zero;

            if (m_HideAction != null)
            {
                m_HideAction.Invoke();
                m_HideAction = null;
            }

            Vector3 startPos = new Vector3(m_Prop.transform.position.x, m_Prop.transform.position.y, 0);
            Vector3 backPos = startPos + (startPos - m_MainMenu.m_SignImg.transform.position).normalized * 0.2f;

            m_Prop.transform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
            {
                m_Prop.transform.DOMove(m_MainMenu.m_SignImg.transform.position, 0.36f).SetEase(Ease.InCubic);
                m_Prop.transform.DOScale(0.4f, 0.34f).SetEase(Ease.InCubic).onComplete = () =>
                {
                    m_MainMenu.m_SignUpgradeHitEffect.Play();
                    gameObject.SetActive(false);
                    m_MainMenu.RefreshSign();
                    m_MainMenu.m_SignImg.transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.2f);
                };
            };

            GameManager.Sound.PlayAudio(SoundType.SFX_Merge_Unlock_New_Item.ToString());

            if (m_PropLogic.PropId == 10112)
            {
                GameManager.Task.AddDelayTriggerTask(1.2f, () =>
                {
                    m_MainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_TapMaxReward, m_PropLogic);
                });
            }
        }

        private void ShowEffect()
        {
            GameManager.ObjectPool.SpawnWithRecycle<EffectObject>("TileWellDone", "TileItemDestroyEffectPool", 2.2f, transform.position, transform.rotation, transform, (t) =>
            {
                var effect = t?.Target as GameObject;
                if (effect != null)
                {
                    var skeleton = effect.transform.GetComponent<SkeletonGraphic>();
                    if (skeleton != null)
                    {
                        skeleton.Initialize(false);
                        skeleton.AnimationState.SetAnimation(0, "active", false);
                    }
                }
            });
        }

        private void OnBgButtonClick()
        {
            m_BgButton.interactable = false;

            Hide();
        }
    }
}
