using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ClodLogic : PropAttachmentLogic
    {
        private string effectName = "Tile_Mining_Smkoe";

        public override void OnInitialize(PropLogic propLogic)
        {
            propLogic.IsPetrified = true;
            if (propLogic.Prop != null)
            {
                propLogic.Prop.gameObject.SetActive(false);
            }
            else
            {
                propLogic.SpawnPropComplete += p => HideProp(p);
            }

            //‘§º”‘ÿ∆∆ªµÃÿ–ß
            if (!GameManager.ObjectPool.HasObjectPool("BoxBreakPool"))
            {
                GameManager.ObjectPool.CreateObjectPool<EffectObject>("BoxBreakPool", int.MaxValue, 4, int.MaxValue);
                GameManager.ObjectPool.PreloadObjectPool<EffectObject>("BoxBreakPool", effectName, GameManager.ObjectPool.transform, 2);
            }
        }

        public override void OnRelease(PropLogic propLogic, bool isShutdown)
        {
            if (!isShutdown)
            {
                propLogic.IsPetrified = false;

                if (propLogic.Prop != null)
                {
                    propLogic.Prop.gameObject.SetActive(true);
                }
                else
                {
                    propLogic.SpawnPropComplete -= p => HideProp(p);
                }

                //…˙≥…÷©÷ÎÕ¯
                SpawnWeb(propLogic, propLogic.Square.transform.position);

                //œ‘ æ∆∆ªµÃÿ–ß
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                GameManager.ObjectPool.Spawn<EffectObject>(effectName, "BoxBreakPool", propLogic.Square.transform.position, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
                {
                    GameObject target = (GameObject)obj.Target;
                    target.transform.localScale = Vector3.one;
                    //var anim = target.GetComponentInChildren<Spine.Unity.SkeletonAnimation>(true);
                    //anim.Initialize(true);
                    //anim.AnimationState.SetAnimation(0, "breakStone", false);
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

                GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Dig.ToString());
                GameManager.Sound.ForbidSound(SoundType.SFX_DigTreasure_Dig.ToString(), true);

                GameManager.Task.AddDelayTriggerTask(0.1f, () =>
                {
                    GameManager.Sound.ForbidSound(SoundType.SFX_DigTreasure_Dig.ToString(), false);
                });

                if (propLogic.PropId / 10000 != 5 && !MergeManager.PlayerData.GetPropIsUnlock(propLogic.PropId)) 
                {
                    MergeManager.PlayerData.SetPropIsUnlock(propLogic.PropId);

                    ((MergeMainMenu_DigTreasure)mainBoard).ShowMergeFlyItemSlot(propLogic.PropId, propLogic.Square.transform.position);
                }
            }
        }

        public override void OnOperationOccurAround(PropOperation operation)
        {
            if (operation == PropOperation.Dig || operation == PropOperation.Merge) 
            {
                PropLogic propLogic = PropLogic;

                Release(false);

                if (UnRestrictedPropId(propLogic.PropId)) 
                {
                    List<Square> nearSquares = MergeManager.Merge.GetSquaresWithinCross(propLogic.Square);
                    foreach (Square nearSquare in nearSquares)
                    {
                        if (nearSquare.FilledProp != null)
                            nearSquare.FilledProp.OnOperationOccurAround(PropOperation.Dig);
                    }
                }
            }
        }

        private void HideProp(Prop p)
        {
            p.gameObject.SetActive(false);
        }

        private void SpawnWeb(PropLogic propLogic, Vector3 position)
        {
            if (propLogic != null && !ForbidSpawnWebPropId(propLogic.PropId))  
            {
                int attachmentId = 2;
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                propLogic.SpawnAttachment(attachmentId, position, mainBoard.m_MergeBoard.m_PropsRoot);
            }
        }

        private bool ForbidSpawnWebPropId(int id)
        {
            //Õ¡øÈ£¨±¶œ‰£¨±¶≤ÿ£¨‘ø≥◊£¨ •±≠
            int type = id / 10000;
            return type == 3 || type == 5 || type == 10 || type == 11 || type == 6;
        }

        private bool UnRestrictedPropId(int id)
        {
            //±¶œ‰£¨±¶≤ÿ£¨‘ø≥◊£¨ •±≠
            int type = id / 10000;
            return type == 3 || type == 10 || type == 11 || type == 6;
        }
    }
}