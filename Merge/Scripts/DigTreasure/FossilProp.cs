using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public sealed class FossilProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public SpriteRenderer m_MaxSprite;

        //private List<int> m_CanGenerateProps;

        private int maxPropId = 10112;

        public override void Initialize(PropLogic propLogic)
        {
            base.Initialize(propLogic);

            //if (m_CanGenerateProps == null && propLogic.PropId == maxPropId) 
            //{
            //    InitializeCanGenerateProps();
            //}
        }

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

        public override void OnGeneratedByMerge()
        {
            base.OnGeneratedByMerge();

            int propId = PropLogic.PropId;
            int stage = propId % 100 - 1;
            bool isFirstGenerated = false;
            if (stage > MergeManager.PlayerData.GetCurrentMaxMergeStage())
            {
                isFirstGenerated = true;
                MergeManager.PlayerData.SetCurrentMaxMergeStage(stage);
                GameManager.Event.Fire(this, MaxStageUpgradeEventArgs.Create(propId, PropLogic, transform.position));
            }

            GenerateBubble();
            GenerateAdditionalOutput(stage + 1, isFirstGenerated);
        }

        public override void OnClick()
        {
            base.OnClick();

            if (PropLogic != null && PropLogic.PropId == maxPropId) 
            {
                IDataTable<DRMergeFinalChestReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeFinalChestReward>(MergeManager.Instance.GetMergeDataTableName());
                DRMergeFinalChestReward data = dataTable.MaxIdDataRow;

                if (data != null)
                {
                    for (int i = 0; i < data.RewardPropIds.Count; i++)
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]);
                    }
                }

                Vector3 propPos = Vector3.zero;
                if (PropLogic.Square != null) 
                    propPos = PropLogic.Square.transform.position;

                MergeManager.Merge.ClearProp(PropLogic);
                MergeManager.Merge.SavePropDistributedMap();

                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.MergeDigChestRewardPanel, false, () =>
                {
                }, null, () =>
                {
                    MergeDigChestRewardPanel panel = RewardManager.Instance.RewardPanel as MergeDigChestRewardPanel;
                    if (panel != null) 
                        panel.chestObj.transform.position = propPos;

                    PropLogic.Release(false);
                });

                MergeManager.Merge.HidePropSelectedBox();

                GameManager.Firebase.RecordMessageByEvent("Merge_Final_Box_Claim");

                //if (m_CanGenerateProps != null && m_CanGenerateProps.Count > 0)
                //{
                //    Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                //    if (randomSquare != null)
                //    {
                //        int index = Random.Range(0, m_CanGenerateProps.Count);
                //        int propId = m_CanGenerateProps[index];

                //        if (propId != 0)
                //        {
                //            MergeManager.Merge.GenerateProp(propId, 0, transform.position, randomSquare, PropMovementState.Bouncing);

                //            m_CanGenerateProps.RemoveAt(index);
                //            if (m_CanGenerateProps.Count == 0)
                //            {
                //                MergeManager.Merge.ReleaseProp(PropLogic);
                //            }
                //            else
                //            {
                //                MergeManager.Merge.SavePropDistributedMap();
                //            }

                //            GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
                //        }
                //    }
                //    else
                //    {
                //        GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeWeakHintMenu"), form =>
                //        {
                //            MergeWeakHintMenu weakHintMenu = form.GetComponent<MergeWeakHintMenu>();
                //            weakHintMenu.SetHintText("Merge.Board is full!", Camera.main.ViewportToScreenPoint(Vector3.zero));
                //            weakHintMenu.OnShow();
                //        });
                //    }
                //}
            }
        }

        private void GenerateBubble()
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
        }

        private void GenerateAdditionalOutput(int level, bool isFirstGenerated)
        {
            IDataTable<DRMergeAdditionalOutput> additionDataTable = MergeManager.DataTable.GetDataTable<DRMergeAdditionalOutput>(MergeManager.Instance.GetMergeDataTableName());
            var data = additionDataTable.GetDataRow(level);
            if (data != null)
            {
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
                            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                            if (mainBoard != null)
                            {
                                mainBoard.StoreProp(id);

                                mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.m_StorageButton.transform.position, () =>
                                {
                                    
                                }, false);
                            }
                        }
                    }
                }
            }
        }

        //private void InitializeCanGenerateProps()
        //{
        //    IDataTable<DRCanGenerateProp> dataTable = MergeManager.DataTable.GetDataTable<DRCanGenerateProp>(MergeManager.Instance.GetMergeDataTableName());
        //    var data = dataTable.GetDataRow(PropLogic.PropId);
        //    if (data != null)
        //    {
        //        m_CanGenerateProps = new List<int>();
        //        for (int i = 0; i < data.CanGeneratePropIds.Count; i++)
        //        {
        //            int propId = data.CanGeneratePropIds[i];
        //            for (int j = 0; j < data.CanGeneratePropNum[i]; j++)
        //            {
        //                m_CanGenerateProps.Add(propId);
        //            }
        //        }
        //    }
        //}


        #region Save Data

        //public override void Save(PropSavedData savedData)
        //{
        //    if (m_CanGenerateProps != null && m_CanGenerateProps.Count > 0)
        //    {
        //        string result = null;
        //        for (int i = 0; i < m_CanGenerateProps.Count; i++)
        //        {
        //            result += m_CanGenerateProps[i];

        //            if (i != m_CanGenerateProps.Count - 1)
        //                result += "+";
        //        }

        //        savedData.SetData("CanGenerateProps", result);
        //    }

        //    base.Save(savedData);
        //}

        //public override bool Load(PropSavedData savedData)
        //{
        //    if (PropLogic != null && PropLogic.PropId == maxPropId)
        //    {
        //        if (savedData.HasData("CanGenerateProps"))
        //        {
        //            if (m_CanGenerateProps == null)
        //                m_CanGenerateProps = new List<int>();
        //            else
        //                m_CanGenerateProps.Clear();

        //            string savedString = savedData.GetData("CanGenerateProps");
        //            string[] splitedStrings = savedString.Split("+");
        //            for (int i = 0; i < splitedStrings.Length; i++)
        //            {
        //                if (!string.IsNullOrEmpty(splitedStrings[i]) && int.TryParse(splitedStrings[i], out int result))
        //                {
        //                    m_CanGenerateProps.Add(result);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            InitializeCanGenerateProps();
        //        }
        //    }

        //    return base.Load(savedData);
        //}

        #endregion
    }
}
