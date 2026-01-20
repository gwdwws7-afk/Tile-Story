using UnityEngine;

namespace Merge
{
    public class PackingboxLogic_LoveGiftBattle : PackingboxLogic
    {
        public override string BoxBreakEffectName => "FX_Box_Broken_BB";

        public override void ShowBoxBreakEffect(PropLogic propLogic)
        {
            Vector3 pos = propLogic.Square.transform.position;
            //œ‘ æ∆∆ªµÃÿ–ß
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>(BoxBreakEffectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
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
        }
    }
}