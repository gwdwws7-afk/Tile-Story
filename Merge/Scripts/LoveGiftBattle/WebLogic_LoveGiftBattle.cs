using Spine.Unity;
using UnityEngine;

namespace Merge
{
    public class WebLogic_LoveGiftBattle : WebLogic
    {
        private Vector3 squarePos;
        private string BoxBreakEffectName => "FX_Box_Broken_BB";

        public override void OnInitialize(PropLogic propLogic)
        {
            base.OnInitialize(propLogic);

            if (propLogic.Square != null)
                squarePos = propLogic.Square.transform.position;
            else
                squarePos = Vector3.one;
        }

        public override void OnRelease(PropLogic propLogic, bool isShutdown)
        {
            if (!isShutdown)
            {
                propLogic.IsImmovable = false;
                propLogic.SetNormal();

                //œ‘ æ∆∆ªµÃÿ–ß
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                GameManager.ObjectPool.Spawn<EffectObject>(BoxBreakEffectName, "BoxBreakPool", squarePos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
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
        }

        protected override void UnspawnAttachment(bool isShutdown)
        {
            base.UnspawnAttachment(true);
        }
    }
}