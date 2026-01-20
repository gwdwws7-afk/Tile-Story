using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 默认道具类
    /// </summary>
    public sealed class DefaultProp : Prop
    {
        public SpriteRenderer m_Sprite;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;
        }

        public override void OnGeneratedByMerge()
        {
            base.OnGeneratedByMerge();

            int propId = PropLogic.PropId;
            int stage = propId % 10100 - 1;
            bool isFirstGenerated = false;
            if (stage > MergeManager.PlayerData.GetCurrentMaxMergeStage())
            {
                isFirstGenerated = true;
                MergeManager.PlayerData.SetCurrentMaxMergeStage(stage);
                GameManager.Event.Fire(this, MaxStageUpgradeEventArgs.Create(propId, PropLogic, transform.position));
            }

            GenerateAdditionalOutput(stage + 1, isFirstGenerated);
        }

        public override void OnClick()
        {
            base.OnClick();

            int propId = PropLogic.PropId;
            if (propId == MergeManager.Instance.MaxPropId)
            {
                IDataTable<DRMergeFinalChestReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeFinalChestReward>(MergeManager.Instance.GetMergeDataTableName());
                int time = MergeManager.PlayerData.GetFinalRewardTime();
                if (time <= 0 && GameManager.PlayerData.IsOwnTileID(MergeManager.Instance.TileId))
                    time = 1;

                DRMergeFinalChestReward data = dataTable.GetDataRow(time + 1);
                if (data == null)
                    data = dataTable.MaxIdDataRow;

                if (data != null)
                {
                    for (int i = 0; i < data.RewardPropIds.Count; i++)
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]);
                    }
                }
                MergeManager.PlayerData.AddGetFinalRewardTime();

                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                });

                MergeManager.Merge.ReleaseProp(PropLogic);
                MergeManager.Merge.HidePropSelectedBox();

                GameManager.Firebase.RecordMessageByEvent("Merge_Final_Box_Claim");
            }
        }

        private void GenerateAdditionalOutput(int level, bool isFirstGenerated)
        {
            IDataTable<DRMergeAdditionalOutput> additionDataTable = MergeManager.DataTable.GetDataTable<DRMergeAdditionalOutput>(MergeManager.Instance.GetMergeDataTableName());
            var data = additionDataTable.GetDataRow(level);
            if (data != null)
            {
                IDataTable<DRMergeGenerateBubble> dataTable = MergeManager.DataTable.GetDataTable<DRMergeGenerateBubble>(MergeManager.Instance.GetMergeDataTableName());
                var bubbleData = dataTable.GetDataRow(PropLogic.PropId);
                if (bubbleData != null)
                {
                    int randomNum = Random.Range(1, 101);
                    if (randomNum <= bubbleData.GenerateBubbleProbability)
                    {
                        Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                        if (randomSquare != null)
                        {
                            MergeManager.Merge.GenerateProp(bubbleData.GenerateBubble, 1, transform.position, randomSquare, PropMovementState.Bouncing);
                        }
                    }
                }

                if (data.AdditionalPropIds.Count > 0)
                {
                    foreach (int id in data.AdditionalPropIds)
                    {
                        //星星道具只会在首次合成更高级的普通元素时，才会生成一次
                        if (id == 40101 && !isFirstGenerated)
                            continue;

                        Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                        if (randomSquare != null)
                        {
                            MergeManager.Merge.GenerateProp(id, 0, transform.position, randomSquare, PropMovementState.Bouncing);
                        }
                        else
                        {
                            MergeMainMenu mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
                            if (mainBoard != null)
                            {
                                mainBoard.StoreProp(id);

                                mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.m_StorageButton.transform.position,0.5f, () =>
                                {
                                    MergeMainMenu mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
                                    if (mainMenu.m_NewPropMergedBoard.gameObject.activeSelf)
                                    {
                                        mainMenu.m_NewPropMergedBoard.m_HideAction += () =>
                                        {
                                            GameManager.Task.AddDelayTriggerTask(1.2f, () =>
                                            {
                                                mainBoard?.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_BoardFull);
                                            });
                                        };
                                    }
                                    else
                                    {
                                        mainBoard?.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_BoardFull);
                                    }
                                }, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
