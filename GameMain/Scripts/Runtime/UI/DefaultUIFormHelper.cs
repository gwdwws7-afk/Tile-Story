using System;
using UnityEngine;

public sealed class DefaultUIFormHelper : IUIFormHelper
{
    public void CreateUIForm(string uiName, UIGroup uiGroup, Action<UIForm> successAction = null, Action failAction = null)
    {
        Transform parent = uiGroup != null ? uiGroup.transform : null;
        UnityUtility.InstantiateAsync(uiName, parent, obj =>
        {
            if (obj != null)
            {
                UIForm form = obj.GetComponent<UIForm>();
                successAction?.Invoke(form);
            }
            else
            {
                Log.Error("Create UI form {0} fail", uiName);
                failAction?.Invoke();
            }
        });
    }

    private int taskId = -1;
    public void ReleaseUIForm(UIForm form)
    {
        form.Clear();
        UnityUtility.UnloadInstance(form.gameObject);

        //if(GameManager.Firebase.GetBool(Constant.RemoteConfig.Use_GC, false))
        //{
        //    if (taskId != -1) GameManager.Task.RemoveDelayTriggerTask(taskId);
        //    taskId = GameManager.Task.AddDelayTriggerTask(SystemInfoManager.IsLowPerformanceMachine() ? 5f : 10f, () =>
        //    {
        //        GameManager.UI.StartGarbageCollection();
        //        taskId = -1;
        //    });
        //}
    }
}
