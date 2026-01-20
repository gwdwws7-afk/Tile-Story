using DG.Tweening;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 金币道具类
    /// </summary>
    public class CoinProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public int m_CoinNum;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;
        }

        public override void OnClick()
        {
            base.OnClick();

            if (m_CoinNum <= 0)
                return;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                RewardManager.Instance.SaveRewardData(TotalItemData.Coin, m_CoinNum, true);
                GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(m_CoinNum, null));
                Vector3 rewardStartPos = PropLogic.Square.transform.position;
                MergeManager.Merge.ReleaseProp(PropLogic);

                UnityUtility.InstantiateAsync("MergeRewardGetTip", mainBoard.transform, obj =>
                {
                    ItemSlot slot = obj.GetComponent<ItemSlot>();

                    slot.OnInit(TotalItemData.Coin, m_CoinNum);
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

                GameManager.Firebase.RecordMessageByEvent("Merge_Claim_Coin", new Firebase.Analytics.Parameter("num", m_CoinNum));

                if (MergeManager.Instance.Theme == MergeTheme.DigTreasure) 
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Get_Coins, new Firebase.Analytics.Parameter("Num", m_CoinNum));
            }
        }

        public override void OnDoubleClick()
        {
            base.OnDoubleClick();
        }

        public override void OnBounceEnd()
        {
            base.OnBounceEnd();

            //if (PropLogic != null && PropLogic.PropId == 20101 && PropLogic.Square != null && PropLogic.Prop != null)
            //{
            //    MergeMainMenu mainMenu = GameManager.UI.GetUIForm("MergeMainMenu") as MergeMainMenu;
            //    if (mainMenu.m_NewPropMergedBoard.gameObject.activeSelf)
            //    {
            //        mainMenu.m_NewPropMergedBoard.m_HideAction += () =>
            //        {
            //            GameManager.Task.AddDelayTriggerTask(0f, () =>
            //            {
            //                mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_4, PropLogic);
            //            });
            //        };
            //    }
            //    else
            //    {
            //        mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_4, PropLogic);
            //    }
            //}
        }

        public override void OnGeneratedByMerge()
        {
            if (PropLogic != null && PropLogic.PropId == 20105 && PropLogic.Square != null && PropLogic.Prop != null)
            {
                MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_MaxLevel, PropLogic);
            }

            base.OnGeneratedByMerge();
        }
    }
}
