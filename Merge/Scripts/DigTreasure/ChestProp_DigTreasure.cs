using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Merge
{
    public class ChestProp_DigTreasure : MaxProp
    {
        public TextMeshProUGUI m_ProgressText;
        public Canvas m_ProgressBar;

        private bool m_IsUnLock;
        private List<int> m_CanGenerateProps;

        public bool IsUnLock => m_IsUnLock;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_ProgressBar.sortingOrder = sortOrder + 1;
        }

        public override void Initialize(PropLogic propLogic)
        {
            base.Initialize(propLogic);

            if (m_CanGenerateProps == null)
            {
                InitializeCanGenerateProps();
            }
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override Square OnPutOn(PropLogic prop)
        {
            //key put on
            if (!m_IsUnLock && MovementState == PropMovementState.Static && !PropLogic.IsPetrified) 
            {
                if (prop.PropId == 110101 && !PropLogic.IsSilenced && !prop.IsSilenced) 
                {
                    MergeManager.Merge.ReleaseProp(prop);
                    UnlockChest(true);

                    return PropLogic.Square;
                }
            }

            return base.OnPutOn(prop);
        }

        public void UnlockChest(bool showAnim)
        {
            if (m_IsUnLock)
                return;

            m_IsUnLock = true;
            Refresh();
            MergeManager.Merge.SavePropDistributedMap();

            if (showAnim)
            {
                ShowPunchAnim();
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Unlock_Box_Element.ToString());
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (!m_IsUnLock)
            {
                if (PropLogic != null && !PropLogic.IsPetrified)
                {
                    MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                    if (mainBoard != null)
                    {
                        mainBoard.ShowLockPromptBox(transform.position);
                    }
                }
            }
        }

        public override void OnClick()
        {
            base.OnClick();

            if (m_IsUnLock && PropLogic != null && m_CanGenerateProps != null && m_CanGenerateProps.Count > 0) 
            {
                int index = Random.Range(0, m_CanGenerateProps.Count);
                int propId = m_CanGenerateProps[index];
                if (propId != 0)
                {
                    Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                    if (randomSquare != null)
                    {
                        MergeManager.Merge.GenerateProp(propId, 0, transform.position, randomSquare, PropMovementState.Bouncing);

                        m_CanGenerateProps.RemoveAt(index);
                        if (m_CanGenerateProps.Count == 0)
                        {
                            MergeManager.Merge.ReleaseProp(PropLogic);
                        }
                        else
                        {
                            MergeManager.Merge.SavePropDistributedMap();
                        }

                        GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
                    }
                    else
                    {
                        //GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeWeakHintMenu"), form =>
                        //{
                        //    MergeWeakHintMenu weakHintMenu = form.GetComponent<MergeWeakHintMenu>();
                        //    weakHintMenu.SetHintText("Merge.Board is full!", Camera.main.ViewportToScreenPoint(Vector3.zero));
                        //    weakHintMenu.OnShow();
                        //});

                        MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                        if (mainBoard != null)
                        {
                            mainBoard.StoreProp(propId);

                            m_CanGenerateProps.RemoveAt(index);
                            if (m_CanGenerateProps.Count == 0)
                            {
                                Vector3 pos = transform.position;

                                MergeManager.Merge.ReleaseProp(PropLogic);

                                //œ‘ æ∆∆ªµÃÿ–ß
                                string effectName = "Tile_Mining_Smkoe";
                                GameManager.ObjectPool.Spawn<EffectObject>(effectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
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
                            else
                            {
                                MergeManager.Merge.SavePropDistributedMap();
                            }

                            mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.m_StorageButton.transform.position, () =>
                            {

                            }, false);
                        }
                    }
                }
            }
        }

        private void Refresh()
        {
            m_ProgressBar.gameObject.SetActive(!m_IsUnLock);
            m_MaxSprite.gameObject.SetActive(m_IsUnLock);
        }

        private void InitializeCanGenerateProps()
        {
            IDataTable<DRCanGenerateProp> dataTable = MergeManager.DataTable.GetDataTable<DRCanGenerateProp>(MergeManager.Instance.GetMergeDataTableName());
            var data = dataTable.GetDataRow(PropLogic.PropId);
            if (data != null)
            {
                m_CanGenerateProps = new List<int>();
                for (int i = 0; i < data.CanGeneratePropIds.Count; i++)
                {
                    int propId = data.CanGeneratePropIds[i];
                    for (int j = 0; j < data.CanGeneratePropNum[i]; j++)
                    {
                        m_CanGenerateProps.Add(propId);
                    }
                }
            }
        }

        #region Save Data

        private bool m_IsLoaded = false;

        public override void Save(PropSavedData savedData)
        {
            if (!m_IsLoaded)
                return;

            savedData.SetData("IsLock", m_IsUnLock ? "1" : "0");

            if (m_CanGenerateProps != null && m_CanGenerateProps.Count > 0)
            {
                string result = null;
                for (int i = 0; i < m_CanGenerateProps.Count; i++)
                {
                    result += m_CanGenerateProps[i];

                    if (i != m_CanGenerateProps.Count - 1)
                        result += "+";
                }

                savedData.SetData("CanGenerateProps", result);
            }

            base.Save(savedData);
        }

        public override bool Load(PropSavedData savedData)
        {
            if (savedData.HasData("IsLock"))
            {
                string savedString = savedData.GetData("IsLock");
                if (!string.IsNullOrEmpty(savedString) && int.TryParse(savedString, out int result))
                {
                    m_IsUnLock = result == 1 ? true : false;

                    Refresh();
                }
            }

            if (savedData.HasData("CanGenerateProps"))
            {
                if (m_CanGenerateProps == null)
                    m_CanGenerateProps = new List<int>();
                else
                    m_CanGenerateProps.Clear();

                string savedString = savedData.GetData("CanGenerateProps");
                string[] splitedStrings = savedString.Split("+");
                for (int i = 0; i < splitedStrings.Length; i++)
                {
                    if (!string.IsNullOrEmpty(splitedStrings[i]) && int.TryParse(splitedStrings[i], out int result))
                    {
                        m_CanGenerateProps.Add(result);
                    }
                }
            }
            else
            {
                InitializeCanGenerateProps();
            }

            m_IsLoaded = true;

            return base.Load(savedData);
        }

        #endregion
    }
}
