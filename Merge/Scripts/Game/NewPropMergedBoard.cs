using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class NewPropMergedBoard : MonoBehaviour
    {
        public GameObject m_Main;
        public Image m_PropImage;
        public GameObject m_Prop;
        public GameObject m_Title;
        public GameObject m_Tip;
        public Button m_BgButton;

        public GameObject m_BgEffect;

        private string m_PropName;
        protected PropLogic m_PropLogic;
        protected MergeMainMenuBase m_MainMenu;
        public Action m_HideAction;
        private AsyncOperationHandle spriteHandle;

        public virtual void Init(string propName, PropLogic propLogic, Action callback, MergeMainMenuBase mainMenu)
        {
            m_PropName = propName;
            m_PropLogic = propLogic;
            m_MainMenu = mainMenu;

            m_Main.SetActive(true);

            m_BgButton.onClick.RemoveAllListeners();
            m_BgButton.onClick.AddListener(OnBgButtonClick);

            m_BgButton.interactable = false;
            string name;
            if (m_PropLogic.PropId == MergeManager.Instance.MaxPropId)
                name = propName + "_2";
            else
                name = propName;

            if(spriteHandle.IsValid())
                UnityUtility.UnloadAssetAsync(spriteHandle);
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

        public virtual void Release()
        {
            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = default;
        }

        public virtual void Show()
        {
            Transform titleTrans = m_Title.transform;
            Transform tipTrans = m_Tip.transform;
            Transform trans = m_Prop.transform;

            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;
            trans.localScale = Vector3.zero;
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

            GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);

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
