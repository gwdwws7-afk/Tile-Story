using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class GemProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public SpriteRenderer m_MaxSprite;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;

            if (m_MaxSprite != null)
            {
                m_MaxSprite.sortingLayerName = layerName;
                m_MaxSprite.sortingOrder = sortOrder + 1;
            }
        }

        public override void OnClick()
        {
            base.OnClick();

            if (PropLogic == null)
                return;

            if (PropLogic.PropId % 10 < 4)
                return;

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard == null) 
                return;

            IDataTable<DRMergeStageReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeStageReward>(MergeManager.Instance.GetMergeDataTableName());
            var data = dataTable.GetDataRow(MergeManager.PlayerData.GetDigTreasureRewardStage());
            if (data == null)
                return;

            int curProgress = MergeManager.PlayerData.GetDigTreasureRewardProgress();
            if (curProgress >= data.TargetNum)
                return;

            int pointNum = GetGemPointNum(PropLogic.PropId);
            MergeManager.PlayerData.SetDigTreasureRewardProgress(curProgress + pointNum);

            Vector3 startPos = transform.position;
            UnityUtility.InstantiateAsync("MergeAddNumTip", mainBoard.transform, obj =>
            {
                AddItemSlot slot = obj.GetComponent<AddItemSlot>();

                slot.itemNumText.text = pointNum.ToString();
                obj.GetComponent<CanvasGroup>().alpha = 1;

                Transform cachedTrans = obj.transform;
                cachedTrans.localScale = Vector3.zero;
                obj.SetActive(true);

                cachedTrans.position = startPos;

                cachedTrans.DOScale(0, 0).onComplete = () =>
                {
                    cachedTrans.DOScale(1.2f, 0.25f).SetEase(Ease.InOutCubic).onComplete = () =>
                    {
                        cachedTrans.DOScale(1, 0.2f).SetEase(Ease.OutCubic);
                    };
                    cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.1f, 0), 0.6f).SetEase(Ease.InSine).SetDelay(0.4f);
                    obj.GetComponent<CanvasGroup>().DOFade(0, 0.2f).SetDelay(0.8f);
                };

                GameManager.Task.AddDelayTriggerTask(1f, () =>
                {
                    UnityUtility.UnloadInstance(obj);
                });
            });

            string itemSlotName = GetFlyItemName(PropLogic.PropId);
            for (int i = 0; i < GetFlyItemGenerateNum(PropLogic.PropId); i++)
            {
                int index = i;
                UnityUtility.InstantiateAsync(itemSlotName, mainBoard.transform, obj =>
                {
                    Transform cachedTrans = obj.transform;
                    cachedTrans.position = startPos;
                    obj.SetActive(true);

                    float randomRadius = Random.Range(0.16f, 0.2f);
                    Vector3 randomPos = GetRandomPositionInLowerHalf(startPos, randomRadius);
                    cachedTrans.DOMove(randomPos, 0.2f);

                    cachedTrans.DOMove(mainBoard.m_FlyGemHitPos.position, 0.4f).SetEase(Ease.InOutQuart).SetDelay(0.2f+ index*0.03f).onComplete = () =>
                    {
                        UnityUtility.UnloadInstance(obj);
                    };

                    GameManager.Task.AddDelayTriggerTask(0.5f + index * 0.03f, () =>
                    {
                        if (mainBoard != null)
                        {
                            mainBoard.ShowStageHitAnim();
                        }

                        obj.SetActive(false);
                    });
                });
            }

            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                mainBoard.RefreshProgressBar(true);
            });

            GameManager.ObjectPool.Spawn<EffectObject>("TargetCollectEffect", "PropEffectPool", transform.position, Quaternion.identity, mainBoard.m_EffectRoot, res =>
            {
                GameObject effect = (GameObject)res.Target;
                effect.SetActive(true);
                effect.GetComponentInChildren<SkeletonAnimation>().AnimationState.SetAnimation(0, "idle", false);

                GameManager.Task.AddDelayTriggerTask(1.3f, () =>
                {
                    if (effect != null)
                    {
                        GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", effect);
                    }
                });
            });

            MergeManager.Merge.ReleaseProp(PropLogic);

            mainBoard.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_DigDialog5);

            GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Recycle_Gem.ToString());
        }

        public override void OnGeneratedByMerge()
        {
            if (PropLogic != null && PropLogic.PropId % 10 == 4 && PropLogic.Square != null && PropLogic.Prop != null) 
            {
                MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DigDialog5, PropLogic);
            }

            if (!MergeManager.PlayerData.GetPropIsUnlock(PropLogic.PropId))
            {
                MergeManager.PlayerData.SetPropIsUnlock(PropLogic.PropId);

                MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                mainBoard?.ShowMergeFlyItemSlot(PropLogic.PropId, transform.position);
            }

            base.OnGeneratedByMerge();
        }

        private Vector3 GetRandomPositionInLowerHalf(Vector3 startPos, float radius = 1.0f)
        {
            float r = Mathf.Sqrt(Random.value) * radius;
            float theta = Random.Range(1.2f * Mathf.PI, 1.8f * Mathf.PI);
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);
            return startPos + new Vector3(x, y, 0f);
        }

        private int GetGemPointNum(int propId)
        {
            if (propId == 70104)//ÂÌ
                return 30;
            else if (propId == 80104)//ºì
                return 80;
            else if (propId == 90104)//À¶
                return 50;

            return 50;
        }

        private string GetFlyItemName(int propId)
        {
            int type = propId / 10000;
            switch (type)
            {
                case 7:
                    return "MergeGameItemSlot_Green";
                case 8:
                    return "MergeGameItemSlot_Red";
                case 9:
                    return "MergeGameItemSlot_Blue";
            }

            return "MergeGameItemSlot_Green";
        }

        private int GetFlyItemGenerateNum(int propId)
        {
            if (propId == 70104)//ÂÌ
                return 3;
            else if (propId == 80104)//ºì
                return 8;
            else if (propId == 90104)//À¶
                return 5;

            return 7;
        }
    }
}
