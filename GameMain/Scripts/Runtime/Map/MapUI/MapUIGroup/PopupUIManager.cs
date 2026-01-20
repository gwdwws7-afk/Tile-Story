using System;
using UnityEngine;

public sealed class PopupUIManager : UIGroup
{
    public override string GroupName => "PopupUI";

    public override UIGroupType GroupType => UIGroupType.Dynamic;

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClick();
        }
    }

    public void OnCancelClick()
    {
        if (!Init || GameManager.UI.IsAsyncLoading) 
        {
            return;
        }

        UIForm firstUIForm = CurrentUIForm;
        if (firstUIForm != null)
        {
            if (firstUIForm.IsAvailable && !IsForbidCancelForm(firstUIForm.GetType().Name))
            {
                firstUIForm.OnClose();
            }
        }
    }

    private bool IsForbidCancelForm(string name)
    {
        return false;
    }

    private DateTime m_StartUnFocusTime;
}
