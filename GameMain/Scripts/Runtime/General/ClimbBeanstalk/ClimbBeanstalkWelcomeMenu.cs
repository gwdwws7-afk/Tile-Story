using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClimbBeanstalkWelcomeMenu : UIForm
{
    public DelayButton closeButton, greyButton, greenButton;
    public TextMeshProUGUILocalize buttonText, describeText;
    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage;
    [SerializeField] private ClockBar clockBar;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButton.SetBtnEvent(OnCloseBtnClick);
        greyButton.SetBtnEvent(OnGreyButtonClick);
        greenButton.SetBtnEvent(OnGreenBtnClick);

        greyButton.gameObject.SetActive(GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel);
        greenButton.gameObject.SetActive(GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel);

        buttonText.SetParameterValue("level", Constant.GameConfig.UnlockClimbBeanstalkEventLevel.ToString());

        if (clockBar)
        {
            clockBar.StartCountdown(GameManager.Activity.GetCurActivityEndTime());//GameManager.DataTable.GetDataTable<DTClimbBeanstalkScheduleData>().Data.GetNowActiveActivityEndTime());
            clockBar.CountdownOver += OnCountDownOver;
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;

                    // StartCoroutine(ShowDelayGuide());
                    // ShowPersonRankGuide();
                };
            };
        }

        if (bgImage != null)
        {
            bgImage.DOKill();
            bgImage.DOColor(new Color(1, 1, 1, 0.01f), 0).OnComplete(() =>
                {
                    bgImage.DOFade(1, 0.2f);
                });
        }
        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (clockBar) clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();

        if (cachedTransform) cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(false);

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnReset()
    {
        if (clockBar) clockBar.OnReset();
        StopAllCoroutines();
        base.OnReset();
    }

    private void OnGreyButtonClick()
    {
    }

    private void OnGreenBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        ClimbBeanstalkManager.Instance.ShowClimbBeanstalkMenu("ClimbBeanstalkMenu");
    }

    public void OnCloseBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.CheckClimbBeanstalk);
    }

    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        OnCloseBtnClick();
    }
}