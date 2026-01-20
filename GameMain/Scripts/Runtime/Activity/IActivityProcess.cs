
/// <summary>
/// 活动流程接口，触发顺序为 ActivityPreEndProcess -> ActivityEndProcess -> ActivityStartProcess -> ActivityAfterStartProcess
/// </summary>
public interface IActivityProcess
{
    void ActivityStartProcess();
    void ActivityEndProcess();
    void ActivityPreEndProcess();
    void ActivityAfterStartProcess();
}
