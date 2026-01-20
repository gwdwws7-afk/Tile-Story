public class ClimbBeanstalkEntrance_Easter : ClimbBeanstalkEntrance
{
    public override void OnButtonClick()
    {
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel)
        {
            ShowUnlockPromptBox(Constant.GameConfig.UnlockClimbBeanstalkEventLevel);
            return;
        }

        if (GameManager.Process.Count > 0)
            return;
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel && ClimbBeanstalkManager.Instance.CheckActivityHasStarted())
            GameManager.UI.ShowUIForm("ClimbBeanstalkMenu1");
        // else if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkButtonLevel)
        //     GameManager.UI.ShowUIForm("ClimbBeanstalkWelcomeMenu1");
    }

    public override string GetFlyIconName()
    {
        return "ClimbBeanstalk_EasterCommon[up]";
    }
}
