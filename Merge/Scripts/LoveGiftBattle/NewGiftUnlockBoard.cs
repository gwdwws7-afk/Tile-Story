using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class NewGiftUnlockBoard : MonoBehaviour
    {
        public GameObject m_Main;
        public Image m_PropImage, m_BlackBg;
        public GameObject m_Prop;
        public GameObject m_Title;
        public GameObject m_Tip;
        public Button m_BgButton;

        public GameObject m_BgEffect;

        public Action<Square> m_HideAction;
        private AsyncOperationHandle spriteHandle;

        public void Init(string propName, Vector3 startPos, Vector3 dropPos, Action callback)
        {
            m_Main.SetActive(true);

            m_BgButton.onClick.RemoveAllListeners();
            m_BgButton.onClick.AddListener(OnBgButtonClick);
            m_BgButton.interactable = false;

            m_BlackBg.color = new Color(1, 1, 1, 0);

            string name = propName;
            spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(name, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            {
                m_PropImage.sprite = sp as Sprite;
                m_PropImage.SetNativeSize();

                callback?.Invoke();
            });

            m_BgEffect.transform.localPosition = new Vector3(0, 0, 0);
            m_BgEffect.gameObject.SetActive(false);

            m_Prop.transform.position = startPos;
            m_Prop.transform.localScale = Vector3.zero;
            m_Prop.transform.DOJump(dropPos, 0.2f, 1, 0.3f);
            m_Prop.transform.DOScale(1f, 0.3f).onComplete = () =>
            {
                m_BgEffect.gameObject.SetActive(true);
            };

            m_Title.SetActive(false);
            m_Tip.SetActive(false);

            gameObject.SetActive(true);
        }

        public void Release()
        {
            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = default;
        }

        public void Show()
        {
            Transform titleTrans = m_Title.transform;
            Transform tipTrans = m_Tip.transform;
            Transform trans = m_Prop.transform;

            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;
            gameObject.SetActive(true);

            m_BlackBg.gameObject.SetActive(true);
            m_BlackBg.DOFade(1, 0.2f);
            trans.DOLocalJump(Vector3.zero, 200f, 1, 0.4f).onComplete = () =>
            {
                ShowEffect();

                m_BgEffect.gameObject.SetActive(true);
                m_Title.gameObject.SetActive(true);
                titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    titleTrans.DOScale(1f, 0.2f);
                };

                m_Tip.SetActive(true);
                tipTrans.DOScale(1f, 0.35f).SetDelay(0.3f).SetEase(Ease.OutBack);

                GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
            };

            GameManager.Task.AddDelayTriggerTask(1f, () =>
            {
                m_BgButton.interactable = true;
            });
        }

        public void Hide()
        {
            m_Title.SetActive(false);
            m_Tip.SetActive(false);
            m_BlackBg.gameObject.SetActive(false);
            m_BgEffect.SetActive(false);

            MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
            var randomSquare = MergeManager.Merge.GetRandomEmptySquare();
            Vector3 targetJumpPos = randomSquare == null ? mainBoard.m_SupplyButton.transform.position : randomSquare.transform.position;

            m_Prop.transform.DOScale(0.5f, 0.4f);
            m_Prop.transform.DOJump(targetJumpPos, 0.3f, 1, 0.4f).onComplete = () =>
            {
                gameObject.SetActive(false);
            };

            GameManager.Task.AddDelayTriggerTask(0.3f, () =>
            {
                if (m_HideAction != null)
                {
                    m_HideAction.Invoke(randomSquare);
                    m_HideAction = null;
                }
            });
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
