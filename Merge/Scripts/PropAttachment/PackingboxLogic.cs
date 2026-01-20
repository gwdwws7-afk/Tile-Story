using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 打包箱逻辑类
    /// </summary>
    public class PackingboxLogic : PropAttachmentLogic
    {
        public virtual string BoxBreakEffectName => "FX_Boxbreak_StPatricks";

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

            //预加载破坏特效
            if (!GameManager.ObjectPool.HasObjectPool("BoxBreakPool"))
            {
                GameManager.ObjectPool.CreateObjectPool<EffectObject>("BoxBreakPool", int.MaxValue, 4, int.MaxValue);
                GameManager.ObjectPool.PreloadObjectPool<EffectObject>("BoxBreakPool", BoxBreakEffectName, GameManager.ObjectPool.transform, 2);
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

                //生成蜘蛛网
                SpawnWeb(propLogic, propLogic.Square.transform.position);

                //显示破坏特效
                ShowBoxBreakEffect(propLogic);
            }
        }

        public virtual void ShowBoxBreakEffect(PropLogic propLogic)
        {
            Vector3 pos = propLogic.Square.transform.position;
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            GameManager.ObjectPool.Spawn<EffectObject>(BoxBreakEffectName, "BoxBreakPool", pos, Quaternion.identity, mainBoard.m_EffectRoot, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                target.transform.localScale = Vector3.one;
                var anim = target.GetComponentInChildren<Spine.Unity.SkeletonAnimation>(true);
                anim.Initialize(true);
                anim.AnimationState.SetAnimation(0, "active", false);
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

        public override void OnOperationOccurAround(PropOperation operation)
        {
            if (operation == PropOperation.Merge)
                Release(false);
        }

        private void HideProp(Prop p)
        {
            p.gameObject.SetActive(false);
        }

        private void SpawnWeb(PropLogic propLogic, Vector3 position)
        {
            if (propLogic != null)
            {
                int attachmentId = 2;
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                propLogic.SpawnAttachment(attachmentId, position, mainBoard.m_MergeBoard.m_PropsRoot);
            }
        }
    }
}