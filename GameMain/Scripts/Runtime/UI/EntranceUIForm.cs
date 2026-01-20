using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntranceUIForm : UIForm
{
    [SerializeField] protected DelayButton entranceBtn;
    [SerializeField] private GameObject offlineBanner;
    [SerializeField] private bool offlineAffected;

    private float refreshTimer;
    private bool isInitialize = false;
    private bool isOffline;

    public bool IsLocked
    {
        get;
        private set;
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        RefreshButtonStatus();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (offlineAffected)
        {
            if (refreshTimer >= 1f)
            {
                refreshTimer = 0;
                RefreshButtonStatus();
            }
            else
            {
                refreshTimer += elapseSeconds;
            }
        }
    }

    private void RefreshButtonStatus()
    {
        if (offlineAffected && GameManager.Firebase.GetBool(Constant.RemoteConfig.Enable_Offline_Restrictions, true)) 
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (isInitialize && isOffline) 
                    return;
                isOffline = true;
                isInitialize = true;

                entranceBtn.OnInit(OnOfflineButtonClick);
                offlineBanner.SetActive(true);
                OnOffline();
            }
            else
            {
                if (isInitialize && !isOffline)
                    return;
                isOffline = false;
                isInitialize = true;

                entranceBtn.OnInit(OnNormalButtonClick);
                offlineBanner.SetActive(false);
                OnOnline();
            }
        }
        else
        {
            if (isInitialize && isOffline)
                return;
            isOffline = true;
            isInitialize = true;

            entranceBtn.OnInit(OnButtonClick);
            offlineBanner.SetActive(false);
        }
    }

    public void OnOfflineButtonClick()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.UI.ShowWeakHint("PersonRank.Please check your internet connection", Vector3.zero);
        }
        else
        {
            RefreshButtonStatus();

            OnButtonClick();
        }
    }

    public void OnNormalButtonClick()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            RefreshButtonStatus();

            GameManager.UI.ShowWeakHint("PersonRank.Please check your internet connection", Vector3.zero);
        }
        else
        {
            OnButtonClick();
        }
    }

    public override void OnPause()
    {
        base.OnPause();

        entranceBtn.interactable = false;
    }

    public override void OnResume()
    {
        entranceBtn.interactable = true;

        base.OnResume();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public abstract void OnButtonClick();

    public virtual void OnOffline()
    {
    }

    public virtual void OnOnline()
    {
    }

    public virtual void OnLocked()
    {
        IsLocked = true;
    }

    public virtual void OnUnlocked()
    {
        IsLocked = false;
    }

    public void ShowUnlockPromptBox(int unlockLevel)
    {
        MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        if (mapTop != null && mapTop.gameObject.activeInHierarchy)
        {
            mapTop.ShowUnlockPromptBox(unlockLevel, UIFormType, transform.position);
        }
    }

    public void HideUnlockPromptBox()
    {
        MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        if (mapTop != null && mapTop.gameObject.activeInHierarchy)
        {
            mapTop.HideUnlockPromptBox();
        }
    }
}
