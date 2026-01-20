using System;

public class TilePassStartMenu : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public DelayButton startButton;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        startButton.OnInit(OnButtonClick);
    }

    public override void OnReset()
    {
        base.OnReset();

        startButton.OnReset();
    }

    private void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("TilePassMainMenu");
        GameManager.UI.HideUIForm(this);
    }

    public void OnEscapeBtnClicked()
    {
        OnButtonClick();
    }
}
