using System;
using Firebase.Analytics;

public static class ProcedureUtil
{
    public static void ProcedureMenuToMap()
    {
        
    }

    public static void ProcedureMapToGame()
    {
        GameManager.Process.UnregisterAll();

        if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge || GameManager.PlayerData.LifeNum > 0 || GameManager.PlayerData.GetInfiniteLifeTime() > 0) 
        {
            if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge && GameManager.PlayerData.GetInfiniteLifeTime() <= 0)
            {
                GameManager.PlayerData.UseItem(TotalItemData.Life, 1);
                GameManager.DataNode.SetData<bool>("UseLife", true);
            }

            GameManager.Firebase.RecordMessageByEvent(
                Constant.AnalyticsEvent.Level_Start,
                new Parameter("Level", GameManager.PlayerData.NowLevel));

            GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.MapToGame));
        }
        else
        {
            GameManager.UI.ShowUIForm("LifeShopPanel");
        }
    }
    
    public static void ProcedureGameToGame(Action<bool> action)
    {
        GameManager.Process.UnregisterAll();

        if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge || GameManager.PlayerData.LifeNum > 0 || GameManager.PlayerData.GetInfiniteLifeTime() > 0) 
        {
            action?.InvokeSafely(true);
        }
        else
        {
            action?.InvokeSafely(false);
            GameManager.UI.ShowUIForm("LifeShopPanel");
        }
    }

    public static void ProcedureGameToMap()
    {
        GameManager.Event.Fire(CommonEventArgs.EventId,CommonEventArgs.Create(CommonEventType.GameToMap));
    }
}
