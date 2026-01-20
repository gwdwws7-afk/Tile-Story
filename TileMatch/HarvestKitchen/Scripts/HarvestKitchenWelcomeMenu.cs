using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenWelcomeMenu : UIForm
{
    public DelayButton closeButton, greyButton, greenButton, explainButton;
    public TextMeshProUGUILocalize buttonText,describeText;
    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage;
    [SerializeField] private ClockBar clockBar;
    [SerializeField] private Transform lockImg;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButton.SetBtnEvent(OnCloseBtnClick);
        greyButton.SetBtnEvent(OnGreyButtonClick);
        greenButton.SetBtnEvent(OnGreenBtnClick);
        explainButton.SetBtnEvent(OnExplainBtnClick);
        
        greyButton.gameObject.SetActive(GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockHarvestKitchenLevel);
        greenButton.gameObject.SetActive(GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockHarvestKitchenLevel);
        
        buttonText.SetParameterValue("level", Constant.GameConfig.UnlockHarvestKitchenLevel.ToString());

        if (clockBar)
        {
            clockBar.StartCountdown(HarvestKitchenManager.Instance.EndTime);
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
                };
            };
        }

        if (bgImage != null)
        {
            bgImage.DOKill();
            bgImage.DOColor(new Color(1,1,1,0.01f),0).OnComplete(()=> 
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

    public void OnCloseBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.CheckHarvestKitchen);
        GameManager.Process.EndProcess(ProcessType.ShowHarvestKitchenLockMenu);
    }

    public void OnGreyButtonClick()
    {
        lockImg.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
    }
    
    public void OnGreenBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.UI.ShowUIForm("HarvestKitchenMainMenu");
    }

    public void OnExplainBtnClick()
    {
        GameManager.UI.ShowUIForm("HarvestKitchenExplainMenu", (form) =>
        {
            gameObject.SetActive(false);
            var uiform = form as HarvestKitchenExplainMenu;
            uiform.SetCloseAction(() => gameObject.SetActive(true));
        });
    }
    
    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        OnCloseBtnClick();
    }
}
