using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OnePlusOnePackEntranceBase : UIForm
{
    public DelayButton button;
    public CountdownTimer countdownTimer;

    protected abstract string PackName { get; }
    protected abstract DateTime StartTime { get; }
    protected abstract DateTime EndTime { get; }
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        UpdateBtnActiveAndTimer();

        button.OnInit(OnButtonClick);
        
        base.OnInit(uiGroup, completeAction, userData);

        if (GameManager.PlayerData.OnePlusOnePackType != string.Empty && DateTime.Now >= EndTime)
        {
            string[] strArray = GameManager.PlayerData.OnePlusOnePackType.Split(',');
            if (strArray[0] == PackName)
            {
                GameManager.Process.Register("OnePlusOneReissue", 0, () =>
                {
                    if (GameManager.PlayerData.OnePlusOnePackType == string.Empty)
                    {
                        GameManager.Process.EndProcess("OnePlusOneReissue");
                        return;
                    }
                    ProductNameType productType = (ProductNameType)Enum.Parse(typeof(ProductNameType), strArray[1]);
                    RewardManager.Instance.AddNeedGetReward(productType);
                    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TilePassRewardPanel, false, () =>
                    {
                        GameManager.Process.EndProcess("OnePlusOneReissue");
                        GameManager.PlayerData.OnePlusOnePackType = String.Empty;
                    });
                });
            }
        }
    }
    
    public override void OnRelease()
    {
        button.OnReset();

        base.OnRelease();
    }
    
    private void Update()
    {
        countdownTimer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }
    
    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }
    
    private void UpdateBtnActiveAndTimer()
    {
        if (DateTime.Now > StartTime && DateTime.Now < EndTime &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPackLevel)
        {
            gameObject.SetActive(true);

            countdownTimer.OnReset();
            countdownTimer.StartCountdown(EndTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        gameObject.SetActive(false);
        
        if (GameManager.Process.Count > 0)
            return;
        
        if (GameManager.PlayerData.OnePlusOnePackType != string.Empty)
        {
            string[] strArray = GameManager.PlayerData.OnePlusOnePackType.Split(',');
            ProductNameType productType = (ProductNameType)Enum.Parse(typeof(ProductNameType), strArray[1]);
            RewardManager.Instance.AddNeedGetReward(productType);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TilePassRewardPanel, false, null);
            GameManager.PlayerData.OnePlusOnePackType = String.Empty;
        }
    }
    
    private void OnButtonClick()
    {
        if (GameManager.Process.Count > 0)
            return;

        GameManager.UI.ShowUIForm(PackName);
    }
    
    public override void OnPause()
    {
        button.interactable = false;
        base.OnPause();
    }

    public override void OnResume()
    {
        button.interactable = true;
        base.OnResume();
    }
}
