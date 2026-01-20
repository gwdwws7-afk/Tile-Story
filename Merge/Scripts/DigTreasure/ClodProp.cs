using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ClodProp : Prop
    {
        public GameObject m_ProgressBarCanvas, m_Progress1, m_Progress2, m_Progress3;
        public GameObject m_Bar1, m_Bar2_1, m_Bar2_2, m_Bar3_1, m_Bar3_2, m_Bar3_3;
        public Transform m_EffectRoot;

        private bool m_IsInitialized;
        private int m_TotalDigNum;
        private int m_LastDigNum;

        public override void Initialize(PropLogic propLogic)
        {
            base.Initialize(propLogic);

            int propId = propLogic.PropId;
            m_TotalDigNum = propId % 100;
            m_LastDigNum = m_TotalDigNum;

            Refresh();

            propLogic.IsImmovable = true;
            m_IsInitialized = true;
        }

        public override void OnReset()
        {
            if (PropLogic != null) 
                PropLogic.IsImmovable = false;
            m_IsInitialized = false;

            base.OnReset();
        }

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);
        }

        private void Refresh()
        {
            m_Progress1.SetActive(m_TotalDigNum == 1);
            m_Progress2.SetActive(m_TotalDigNum == 2);
            m_Progress3.SetActive(m_TotalDigNum == 3);

            int digTime = m_TotalDigNum - m_LastDigNum;
            if (m_TotalDigNum == 1)
            {
                m_Bar1.SetActive(digTime <= 0);
            }
            else if(m_TotalDigNum == 2)
            {
                m_Bar2_1.SetActive(digTime <= 1);
                m_Bar2_2.SetActive(digTime <= 0);
            }
            else if (m_TotalDigNum == 3)
            {
                m_Bar3_1.SetActive(digTime <= 2);
                m_Bar3_2.SetActive(digTime <= 1);
                m_Bar3_3.SetActive(digTime <= 0);
            }

            m_ProgressBarCanvas.SetActive(m_TotalDigNum > m_LastDigNum);
        }

        public override void OnSelected()
        {
            if (!m_IsInitialized || m_LastDigNum <= 0 || PropLogic == null || PropLogic.Square == null)  
                return;

            if (MergeGuideMenu.s_CurGuideId == GuideTriggerType.Guide_DigDialog7)
                return;

            if (PropLogic.IsPetrified)
            {
                if (MergeManager.Instance.Theme == MergeTheme.DigTreasure && PropLogic.AttachmentId == 5)
                {
                    MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                    if (mainBoard != null)
                        mainBoard.ShowWeakHint("Merge.Locked");
                }

                return;
            }

            if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
            {
                MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                if (mainBoard != null)
                {
                    MergeManager.PlayerData.SubtractMergeEnergyBoxNum(1);
                    GameManager.Event.Fire(this, RefreshMergeEnergyBoxEventArgs.Create());

                    Vector3 pos = PropLogic.Square.transform.position;
                    mainBoard.ShowPickaxeAnim(pos);

                    int curDigNum = m_LastDigNum--;
                    GameManager.Task.AddDelayTriggerTask(0.267f, () =>
                    {
                        OnDigProp(pos, curDigNum);
                    });

                    mainBoard.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_DigDialog2);
                    mainBoard.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_DigDialog3);
                }
            }
            else
            {
                if (DateTime.Now > MergeManager.Instance.StartTime && DateTime.Now < MergeManager.Instance.EndTime)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGetBoxMenu"));
                }
            }
        }

        private void OnDigProp(Vector3 pos, int curDigNum)
        {
            IDataTable<DRDigOutput> dataTable = MergeManager.DataTable.GetDataTable<DRDigOutput>(MergeManager.Instance.GetMergeDataTableName());
            var data = dataTable.GetDataRow(PropLogic.PropId);
            if (data != null)
            {
                Vector3 startPos = transform.position;
                if (m_TotalDigNum - curDigNum == 0)
                    GenerateProp(data.Output1PropIds, startPos);
                else if (m_TotalDigNum - curDigNum == 1)
                    GenerateProp(data.Output2PropIds, startPos);
                else if (m_TotalDigNum - curDigNum == 2)
                    GenerateProp(data.Output3PropIds, startPos);

                if (curDigNum - 1 <= 0) 
                {
                    List<Square> nearSquares = MergeManager.Merge.GetSquaresWithinCross(PropLogic.Square);
                    foreach (Square nearSquare in nearSquares)
                    {
                        if (nearSquare.FilledProp != null)
                            nearSquare.FilledProp.OnOperationOccurAround(PropOperation.Dig);
                    }

                    ShowDestroyEffect();

                    MergeManager.Merge.ReleaseProp(PropLogic);
                }
                else
                {
                    Refresh();
                    MergeManager.Merge.SavePropDistributedMap();
                }

                ShowDigEffect(pos);

                GameManager.Sound.PlayAudio("SFX_Temple_Shovel_Dig");
            }

            int maxLayer = 4;
            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null) 
            {
                mainBoard.IsUseSupply = true;

                int topLayer = mainBoard.GetTopClodLayer();
                if (topLayer < 3)
                {
                    int curDepth = MergeManager.PlayerData.GetDigTreasureCurDepth();
                    int moveLayer = maxLayer - topLayer;

                    //IDataTable<DRPropDistributedMap> map = MergeManager.DataTable.GetDataTable<DRPropDistributedMap>(MergeManager.Instance.GetMergeDataTableName());
                    //int deepestLayer = map.MaxIdDataRow.Id;
                    //if (curDepth + moveLayer + mainBoard.BoardRow > deepestLayer)
                    //    moveLayer = deepestLayer - mainBoard.BoardRow - curDepth;

                    if (moveLayer > 0)
                    {
                        mainBoard.MoveMergeBoard(moveLayer, 0.6f);
                    }

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Layer_Unlock, new Firebase.Analytics.Parameter("Stage", 8 + curDepth + moveLayer));
                }
            }
        }

        private void ShowDestroyEffect()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>("Tile_Mining_Smkoe", "BoxBreakPool", PropLogic.Square.transform.position, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                target.transform.localScale = Vector3.one;
                target.SetActive(true);
                GameManager.Task.AddDelayTriggerTask(1.1f, () =>
                {
                    if (target != null)
                    {
                        target.SetActive(false);
                        GameManager.ObjectPool.Unspawn<EffectObject>("BoxBreakPool", target);
                    }
                });
            });
        }

        private void GenerateProp(List<int> propIds, Vector3 startPos)
        {
            if (propIds == null)
                return;

            List<PropLogic> props = new List<PropLogic>();
            foreach (int id in propIds)
            {
                Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                if (randomSquare != null)
                {
                    var prop = MergeManager.Merge.GenerateProp(id, 0, startPos, randomSquare, PropMovementState.Hide);
                    props.Add(prop);
                }
                else
                {
                    MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                    if (mainBoard != null)
                    {
                        mainBoard.StoreProp(id, null, false);

                        mainBoard.ShowShiningFlyEffect(startPos, mainBoard.m_StorageButton.transform.position, () =>
                        {
                            mainBoard.RefreshStorage();
                        }, false);

                        if (!MergeManager.PlayerData.GetPropIsUnlock(id))
                        {
                            MergeManager.PlayerData.SetPropIsUnlock(id);
                        }
                    }
                }
            }

            if (props.Count > 0) 
                MergeManager.Instance.StartCoroutine(ShowPropsBouncingAnim(props));
            else
                GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Release_Item.ToString());
        }

        IEnumerator ShowPropsBouncingAnim(List<PropLogic> props)
        {
            while (props.Count > 0)
            {
                if (!MergeManager.Merge.IsInitialized)
                    yield break;

                bool isReady = true;
                foreach (var prop in props)
                {
                    if (prop.Prop == null || prop.Prop.PropLogic == null)  
                    {
                        isReady = false;
                        break;
                    }
                }

                if (isReady)
                {
                    MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                    foreach (var prop in props)
                    {
                        if (prop.Prop != null)
                        {
                            prop.Prop.gameObject.SetActive(true);
                            prop.Prop.SetMovementState(PropMovementState.Bouncing);

                            if (!MergeManager.PlayerData.GetPropIsUnlock(prop.PropId))
                            {
                                MergeManager.PlayerData.SetPropIsUnlock(prop.PropId);

                                GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                                {
                                    mainBoard?.ShowMergeFlyItemSlot(prop.PropId, prop.Square.transform.position);
                                });
                            }
                        }
                    }
                    props.Clear();

                    GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Release_Item.ToString());

                    yield break;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void ShowDigEffect(Vector3 pos)
        {
            //œ‘ æ∆∆ªµÃÿ–ß
            string effectName = "FX_Boxbreak_DigTreasure";
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>(effectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                target.transform.localScale = Vector3.one;
                var anim = target.GetComponentInChildren<Spine.Unity.SkeletonAnimation>(true);
                anim.Initialize(true);
                anim.AnimationState.SetAnimation(0, "breakStone", false);
                target.SetActive(true);
                GameManager.Task.AddDelayTriggerTask(1.1f, () =>
                {
                    if (target != null)
                    {
                        target.SetActive(false);
                        GameManager.ObjectPool.Unspawn<EffectObject>("BoxBreakPool", target);
                    }
                });
            });
        }

        #region Save Data

        private bool m_IsLoaded = false;

        public override void Save(PropSavedData savedData)
        {
            if (!m_IsLoaded)
                return;

            savedData.SetData("LastDigNum", m_LastDigNum.ToString());

            base.Save(savedData);
        }

        public override bool Load(PropSavedData savedData)
        {
            if (savedData.HasData("LastDigNum"))
            {
                string savedString = savedData.GetData("LastDigNum");
                if (!string.IsNullOrEmpty(savedString) && int.TryParse(savedString, out int result))
                {
                    m_LastDigNum = result;

                    Refresh();
                }
            }

            m_IsLoaded = true;

            return base.Load(savedData);
        }

        #endregion
    }
}
