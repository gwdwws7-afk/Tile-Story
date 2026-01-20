using DG.Tweening;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// –«–«µ¿æﬂ¿‡
    /// </summary>
    public class StarProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public int m_StarNum;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;
        }

        public override void OnClick()
        {
            base.OnClick();

            if (m_StarNum <= 0)
                return;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                RewardManager.Instance.SaveRewardData(TotalItemData.Star, m_StarNum, true);
                GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(m_StarNum, null));
                Vector3 rewardStartPos = PropLogic.Square.transform.position;
                MergeManager.Merge.ReleaseProp(PropLogic);

                UnityUtility.InstantiateAsync("MergeRewardGetTip", mainBoard.transform, obj =>
                {
                    ItemSlot slot = obj.GetComponent<ItemSlot>();

                    slot.OnInit(TotalItemData.Star, m_StarNum);
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

                GameManager.Event.Fire(this, StarNumRefreshEventArgs.Create());
                GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

                if (MergeManager.Instance.Theme == MergeTheme.DigTreasure)
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Get_Stars, new Firebase.Analytics.Parameter("Num", m_StarNum));
            }
        }

        public override void OnDoubleClick()
        {
            base.OnDoubleClick();
        }
    }
}
