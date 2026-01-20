using MySelf.Model;
using System;

public class TilePassEnsureMenu : PopupMenuForm
{
    public DelayButton quitButton;
    public DelayButton continueButton;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        continueButton.OnInit(OnContinueButtonClicked);
        quitButton.OnInit(OnClose);
    }

    public override void OnReset()
    {
        continueButton.OnReset();
        quitButton.OnReset();

        base.OnReset();
    }

    public override void OnClose()
    {
        base.OnClose();

        GameManager.UI.HideUIForm("TilePassLastChanceMenu");
        GameManager.UI.HideUIForm(this);

        //如果时间结束，接通行证结束页面
        if (DateTime.Now > TilePassModel.Instance.EndTime && TilePassModel.Instance.EndTime != DateTime.MinValue)
        {
            GameManager.UI.ShowUIForm("TilePassEndMenu",showSuccessAction =>
            {

            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
        }
    }

    private void OnContinueButtonClicked()
    {
        gameObject.SetActive(false);
        GameManager.UI.ShowUIForm("TilePassLastChanceMenu");
    }
}
