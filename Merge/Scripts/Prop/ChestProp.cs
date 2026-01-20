using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// ±¶œ‰µ¿æﬂ¿‡
    /// </summary>
    public class ChestProp : Prop
    {
        public SpriteRenderer m_Sprite;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;
        }

        public override void OnReset()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.HideChestRewardTipBox();
            }

            base.OnReset();
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (PropLogic.IsPetrified)
                return;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.ShowChestRewardTipBox(GetChestRewardItemData(), PropLogic);
            }
        }

        public override void OnClick()
        {
            base.OnClick();

            if (PropLogic.IsPetrified)
                return;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard.m_SelectedChestProp != PropLogic)
                return;

            var rewardData = GetChestRewardItemData();
            for (int i = 0; i < rewardData.Count; i++)
            {
                RewardManager.Instance.SaveRewardData(rewardData[i].type, rewardData[i].num, true);
            }

            int maxAverageCount = 3;
            if (PropLogic != null && PropLogic.Square != null) 
            {
                if (PropLogic.Square.m_Col <= 0 || PropLogic.Square.m_Col >= MergeManager.Instance.BoardCol - 1) 
                {
                    maxAverageCount = 2;
                }
            }

            int averageCount = rewardData.Count > maxAverageCount ? maxAverageCount : rewardData.Count;
            Vector3 rewardStartPos = PropLogic.Square.transform.position;
            var posList = UnityUtility.GetAveragePosition(rewardStartPos, new Vector3(0.1f, 0, 0), averageCount);

            int count = 0;
            foreach (ItemData data in rewardData)
            {
                TotalItemData type = data.type;
                int num = data.num;
                int index = count++;

                GameManager.Task.AddDelayTriggerTask(0.2f * index, () =>
                {
                    UnityUtility.InstantiateAsync("MergeRewardGetTip", mainBoard.transform, obj =>
                    {
                        ItemSlot slot = obj.GetComponent<ItemSlot>();

                        slot.OnInit(type, num);
                        obj.GetComponent<CanvasGroup>().alpha = 1;

                        Transform cachedTrans = obj.transform;
                        cachedTrans.localScale = Vector3.zero;
                        obj.SetActive(true);

                        cachedTrans.position = posList[index % posList.Length];

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
                });
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

            //var rewardData = GetChestRewardItemData();
            //for (int i = 0; i < rewardData.Count; i++)
            //{
            //    RewardManager.Instance.AddNeedGetReward(rewardData[i].type, rewardData[i].num);
            //}
            //RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            //{
            //});

            MergeManager.Merge.ReleaseProp(PropLogic);

            if (mainBoard != null)
            {
                mainBoard.HideChestRewardTipBox();
            }
        }

        public override void OnDoubleClick()
        {
            base.OnDoubleClick();
        }

        public override void OnDragStart()
        {
            base.OnDragStart();

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.HideChestRewardTipBox();
            }
        }

        private List<ItemData> GetChestRewardItemData()
        {
            IDataTable<DRChestPropReward> dataTable = MergeManager.DataTable.GetDataTable<DRChestPropReward>(MergeManager.Instance.GetMergeDataTableName());
            DRChestPropReward data = dataTable.GetDataRow(PropLogic.PropId);

            List<ItemData> rewardDatas = new List<ItemData>();
            if (data != null)
            {
                for (int i = 0; i < data.RewardPropIds.Count; i++)
                {
                    rewardDatas.Add(new ItemData(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]));
                }
            }

            return rewardDatas;
        }
    }
}
