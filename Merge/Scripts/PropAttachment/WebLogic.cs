using Spine.Unity;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// ÷©÷ÎÕ¯¬ﬂº≠¿‡
    /// </summary>
    public class WebLogic : PropAttachmentLogic
    {
        public override void OnInitialize(PropLogic propLogic)
        {
            propLogic.IsImmovable = true;
            propLogic.SetGray();
        }

        public override void OnRelease(PropLogic propLogic, bool isShutdown)
        {
            if (!isShutdown)
            {
                propLogic.IsImmovable = false;
                propLogic.SetNormal();
            }
        }

        protected override void UnspawnAttachment(bool isShutdown)
        {
            if (!isShutdown)
            {
                if (Attachment != null)
                {
                    var anim = Attachment.GetComponentInChildren<SkeletonAnimation>();
                    if (anim != null)
                    {
                        anim.AnimationState.SetAnimation(0, "active", false);

                        GameManager.Task.AddDelayTriggerTask(1.32f, () =>
                        {
                            base.UnspawnAttachment(isShutdown);
                        });
                    }
                    else
                    {
                        base.UnspawnAttachment(isShutdown);
                    }
                }
            }
            else
            {
                base.UnspawnAttachment(isShutdown);
            }
        }
    }
}