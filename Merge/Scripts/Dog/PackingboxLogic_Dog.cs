using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class PackingboxLogic_Dog : PackingboxLogic
    {
        public override string BoxBreakEffectName => "FX_Boxbreak_Dog";

        public override void ShowBoxBreakEffect(PropLogic propLogic)
        {
            Vector3 pos = propLogic.Square.transform.position;
            //��ʾ�ƻ���Ч
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>(BoxBreakEffectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
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
    }   
}
