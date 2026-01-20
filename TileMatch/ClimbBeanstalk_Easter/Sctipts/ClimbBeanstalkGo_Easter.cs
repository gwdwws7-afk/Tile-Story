public class ClimbBeanstalkGo_Easter : ClimbBeanstalkGO
{
    public override void GetAnimName(int chestType, bool isGetReward)
    {
        string animName = null;
        switch (chestType)
        {
            case 1:
                chestOpenAnimName = animName = "idle1";
                eggEffectIndex = 0;
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            case 2:
                chestOpenAnimName = animName = "idle2";
                eggEffectIndex = 1;
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            case 3:
            case 4:
                chestOpenAnimName = animName = "idle5";
                eggEffectIndex = 2;
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            default:
                break;
        }
    }

    public override void ShowNeedGetReward()
    {
        //控制台测试加分导致一次性领取多个奖励，会使流程触发出错，暂时通过这个方式解决
        if (RewardManager.Instance.GettingRewardCount > 0 || RewardManager.Instance.NeedGetRewardCount <= 0)
        {
            return;
        }

        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.ClimbBeanstalkLevelChestRewardPanel, false, () =>
        {
            //if (ClimbBeanstalkManager.Instance.CheckActivityState())
            //    return;
            GameManager.Process.EndProcess(ProcessType.CheckClimbBeanstalk);
        }, () =>
        {
        }, () =>
        {
            GameManager.UI.HideUIForm("GlobalMaskPanel");
            // 复活节元素爬藤
            ClimbBeanstalkLevelChestRewardPanel panel = RewardManager.Instance.RewardPanel as ClimbBeanstalkLevelChestRewardPanel;
            if (panel != null && chestSpine != null) 
            {
                panel.SetChestTypeAndPosition(eggEffectIndex, chestOpenAnimName, chestSpine.transform.position);
                panel.SetOnShowCallback(() =>
                {
                    if (chestSpine != null)
                    {
                        chestSpine.gameObject.SetActive(false);
                        if (chestShadow != null)
                            chestShadow.gameObject.SetActive(false);
                    }
                });
            }

            GameManager.Task.AddDelayTriggerTask(1.0f, () =>
            {
                if (climbBeanstalkMenu != null)
                    GameManager.UI.HideUIForm(climbBeanstalkMenu);
            });
        });
    }
}
