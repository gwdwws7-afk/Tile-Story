using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class GeneratorProp : MaxProp
    {
        protected List<int> m_CanGenerateProps;

        public override void Initialize(PropLogic propLogic)
        {
            base.Initialize(propLogic);

            if (m_CanGenerateProps == null)
            {
                InitializeCanGenerateProps();
            }
        }

        public override void OnClick()
        {
            base.OnClick();

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;

            if (PropLogic != null && m_CanGenerateProps != null && m_CanGenerateProps.Count > 0)
            {
                Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                if (randomSquare != null)
                {
                    int index = Random.Range(0, m_CanGenerateProps.Count);
                    int propId = m_CanGenerateProps[index];

                    if (propId != 0)
                    {
                        MergeManager.Merge.GenerateProp(propId, 0, transform.position, randomSquare, PropMovementState.Bouncing);

                        m_CanGenerateProps.RemoveAt(index);
                        if (m_CanGenerateProps.Count == 0)
                        {
                            OnCanGeneratePropsEmpty();
                        }
                        else
                        {
                            MergeManager.Merge.SavePropDistributedMap();
                        }

                        GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
                    }
                }
                else
                {
                    int index = Random.Range(0, m_CanGenerateProps.Count);
                    int propId = m_CanGenerateProps[index];

                    if (mainBoard != null && propId != 0) 
                    {
                        m_CanGenerateProps.RemoveAt(index);
                        if (m_CanGenerateProps.Count == 0)
                        {
                            OnCanGeneratePropsEmpty();
                        }
                        else
                        {
                            MergeManager.Merge.SavePropDistributedMap();
                        }

                        GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());

                        mainBoard.StoreProp(propId);

                        mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.GetStoragePropGeneratePos(), 0.3f, null, false);
                    }

                    //GameManager.UI.ShowUIForm("MergeWeakHintMenu", form =>
                    //{
                    //    MergeWeakHintMenu weakHintMenu = form.GetComponent<MergeWeakHintMenu>();
                    //    weakHintMenu.SetHintText("Merge.Board is full!", Camera.main.ViewportToScreenPoint(Vector3.zero));
                    //    weakHintMenu.OnShow();
                    //});
                }
            }
        }

        protected virtual void OnCanGeneratePropsEmpty()
        {
            Vector3 pos = transform.position;

            if (PropLogic != null && PropLogic.Square != null)
            {
                List<Square> nearSquares = MergeManager.Merge.GetSquaresWithinCross(PropLogic.Square);
                foreach (Square nearSquare in nearSquares)
                {
                    if (nearSquare.FilledProp != null)
                        nearSquare.FilledProp.OnOperationOccurAround(PropOperation.Dig);
                }
            }

            MergeManager.Merge.ReleaseProp(PropLogic);

            //œ‘ æ∆∆ªµÃÿ–ß
            string effectName = "FX_Boxbreak_DigTreasure";
            bool isSpineEffect = true;

            if (MergeManager.Instance.Theme == MergeTheme.DigTreasure)
            {
                effectName = "Tile_Mining_Smkoe";
                isSpineEffect = false;
            }

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>(effectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                target.transform.localScale = Vector3.one;
                if (isSpineEffect)
                {
                    var anim = target.GetComponentInChildren<Spine.Unity.SkeletonAnimation>(true);
                    anim.Initialize(true);
                    anim.AnimationState.SetAnimation(0, "breakStone", false);
                }
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

        protected virtual void InitializeCanGenerateProps()
        {
            int propId = PropLogic.PropId;

            IDataTable<DRCanGenerateProp> rewardDataTable = MergeManager.DataTable.GetDataTable<DRCanGenerateProp>(MergeManager.Instance.GetMergeDataTableName());
            var data = rewardDataTable.GetDataRow(propId);

            if (data != null)
            {
                m_CanGenerateProps = new List<int>();
                for (int i = 0; i < data.CanGeneratePropIds.Count; i++)
                {
                    int id = data.CanGeneratePropIds[i];
                    for (int j = 0; j < data.CanGeneratePropNum[i]; j++)
                    {
                        m_CanGenerateProps.Add(id);
                    }
                }
            }
        }

        #region Save Data

        public override void Save(PropSavedData savedData)
        {
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

            return base.Load(savedData);
        }

        #endregion
    }
}
