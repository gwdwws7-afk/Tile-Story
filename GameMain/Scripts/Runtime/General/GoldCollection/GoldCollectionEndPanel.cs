using System;

public class GoldCollectionEndPanel : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public TextMeshProUGUILocalize text;
    public DelayButton tipButton;
    public DelayButton closeButton;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipButton.OnInit(OnTipButtonClick);
        closeButton.OnInit(OnClose);

        if (GameManager.Task.GoldCollectionTaskManager.CheckAllTaskComplete())
        {
            text.SetTerm("Gold.EndPanelDes1");
        }
        else
        {
            text.SetTerm("Gold.EndPanelDes2");
        }
    }

    public override void OnReset()
    {
        base.OnReset();
        tipButton.OnReset();
        closeButton.OnReset();
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionEndPanel);
    }

    public void OnEscapeBtnClicked()
    {
        OnClose();
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("GoldCollectionRules");
    }
}
