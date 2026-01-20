using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class LifeProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public int m_InfiniteLifeTime;

        private void OnEnable()
        {
            //StartCoroutine(FloatAnim());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;
        }

        public override void OnClick()
        {
            base.OnClick();

            if (m_InfiniteLifeTime <= 0)
                return;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                RewardManager.Instance.SaveRewardData(TotalItemData.InfiniteLifeTime, m_InfiniteLifeTime, true);
                GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(1, null));
                Vector3 rewardStartPos = PropLogic.Square.transform.position;

                MergeManager.Merge.ReleaseProp(PropLogic);

                UnityUtility.InstantiateAsync("MergeRewardGetTip", mainBoard.transform, obj =>
                {
                    ItemSlot slot = obj.GetComponent<ItemSlot>();

                    slot.OnInit(TotalItemData.InfiniteLifeTime, m_InfiniteLifeTime);
                    obj.GetComponent<CanvasGroup>().alpha = 1;

                    Transform cachedTrans = obj.transform;
                    cachedTrans.localScale = Vector3.zero;
                    obj.SetActive(true);

                    cachedTrans.position = rewardStartPos;

                    cachedTrans.DOScale(0, 0).onComplete = () =>
                    {
                        cachedTrans.DOScale(1, 0.2f);
                        cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 1f).SetEase(Ease.InSine);
                        obj.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetDelay(0.6f).onComplete = () =>
                        {
                            slot.OnRelease();
                            UnityUtility.UnloadInstance(obj);
                        };
                    };
                });

                GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

                //GameManager.Firebase.RecordMessageByEvent("Merge_Claim_Coin", new Firebase.Analytics.Parameter("num", m_CoinNum));
            }
        }

        IEnumerator FloatAnim()
        {
            while (true)
            {
                m_Sprite.transform.DOLocalMoveY(3, 0.8f);

                yield return new WaitForSeconds(0.8f);

                m_Sprite.transform.DOLocalMoveY(-3, 0.8f);

                yield return new WaitForSeconds(0.8f);
            }
        }
    }
}
