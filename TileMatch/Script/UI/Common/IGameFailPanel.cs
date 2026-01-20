using System;

public interface IGameFailPanel
{
    GameFailPanelPriorityType PriorityType { get; }
    void ShowFailPanel(System.Action finishAction);
    void CloseFailPanel(Action finishAction);
}

public enum GameFailPanelPriorityType
{
    None,
    NormalGameFailPanel,
    GameMoreStarFailPanel,
    PkGameFailPanel,
    PersonRankLosePanel,
    GoldCollectionLosePanel,
    ClimbBeanstalkLosePanel,
    ClimbBeanstalkLevelLosePanel,
    CalendarChallengelFailPanel,
    MergeLevelLosePanel,
    StarAndPickaxeLosePanel,
    GlacierQuestLosePanel,
    TimeLimitLosePanel,
    MergeLosePanel,
    BalloonRiseLosePanel,
    KitchenLosePanel,
    HarvestKitchenLosePanel,
}

public abstract class BaseGameFailPanel:UnityEngine.MonoBehaviour,IGameFailPanel
{
    public virtual bool IsSpecialPanel => false;
    public virtual bool IsShowFailPanel => true;
    public virtual GameFailPanelPriorityType PriorityType=>GameFailPanelPriorityType.None;
    public abstract void ShowFailPanel(System.Action finishAction);
    
    public virtual void CloseFailPanel(Action finishAction)
    {
       gameObject.SetActive(false);
       finishAction?.Invoke();
    }
}
