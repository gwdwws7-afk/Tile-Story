using DG.Tweening;
using System;
using UnityEngine;

public class GoldCollectionStartPanel : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public TextMeshProUGUILocalize text;
    public DelayButton tipButton;
    public DelayButton closeButton;
    public DelayButton lockButton;
    public DelayButton okButton;
    public ClockBar timer;
    public Transform lockImg;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipButton.OnInit(OnTipButtonClick);
        closeButton.OnInit(OnClose);

        //预告
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockGoldCollectionLevel)
        {
            text.SetTerm("Gold.UnlockPanelDes");
            lockButton.gameObject.SetActive(true);
            lockButton.OnInit(OnLockButtonClick);
            okButton.gameObject.SetActive(false);
        }
        //开始
        else
        {
            text.SetTerm("Gold.StartPanelDes");
            text.SetParameterValue("0", "<color=#0EEE0A>");
            text.SetParameterValue("1", "</color>");
            lockButton.gameObject.SetActive(false);
            okButton.gameObject.SetActive(true);
            okButton.OnInit(() =>
            {
                GameManager.UI.HideUIForm(this);

                GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI,form =>
                {
                    form.m_OnHideCompleteAction = () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
                    };
                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
                });
            });
        }

        timer.OnReset();
        timer.StartCountdown(GameManager.Task.GoldCollectionTaskManager.EndTime);
        timer.CountdownOver += OnCountdownOver;
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        timer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        base.OnReset();

        tipButton.OnReset();
        closeButton.OnReset();
        lockButton.OnReset();
        okButton.OnReset();
        timer.OnReset();
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
    }

    public void OnEscapeBtnClicked()
    {
        OnClose();
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("GoldCollectionRules");
    }

    private void OnLockButtonClick()
    {
        lockImg.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        OnClose();
    }
}
