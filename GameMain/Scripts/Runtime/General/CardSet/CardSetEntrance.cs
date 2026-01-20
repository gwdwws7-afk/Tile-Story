using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Event;
using MySelf.Model;
using UnityEngine;

public class CardSetEntrance : EntranceUIForm
{
    public CountdownTimer countdownTimer;
    public GameObject claimBanner;
    public GameObject warningSign;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        if (!CardModel.Instance.IsHaveCardAsset)
        {
            gameObject.SetActive(false);
            return;
        }

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockCardSetLevel ||
            !CardModel.Instance.ShowedPreviewPanel ||
            CardModel.Instance.ShowedEndPanel)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        entranceBtn.OnInit(OnButtonClick);
        SetTimer();
        SetClaim();

        GameManager.Event.Subscribe(CardChangeEventArgs.EventId, OnCardChange);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        entranceBtn.OnReset();
        countdownTimer.OnReset();
        GameManager.Event.Unsubscribe(CardChangeEventArgs.EventId, OnCardChange);
        base.OnRelease();
    }

    public override void OnButtonClick()
    {
        if (!CardModel.Instance.ShowedStartPanel)
            GameManager.UI.ShowUIForm($"CardSetStartPanel{CardModel.Instance.CardActivityID}");
        else
            GameManager.UI.ShowUIForm($"CardSetMainMenu{CardModel.Instance.CardActivityID}");
    }

    private void SetTimer()
    {
        if (DateTime.Now < CardModel.Instance.CardStartTime)
        {
            countdownTimer.OnReset();
            countdownTimer.CountdownOver += (sender, args) => { OnInit(null); };
            countdownTimer.StartCountdown(CardModel.Instance.CardStartTime);
        }
        else
        {
            countdownTimer.OnReset();
            countdownTimer.CountdownOver += (sender, args) => { gameObject.SetActive(false); };
            countdownTimer.StartCountdown(CardModel.Instance.CardEndTime);
        }
    }

    private void OnCardChange(object sender, GameEventArgs e)
    {
        SetClaim();
    }

    public void SetClaim()
    {
        claimBanner.SetActive(false);
        foreach (var kvp in CardModel.Instance.CollectCardDict)
        {
            if (kvp.Value.Count == 9 && !CardModel.Instance.CompletedCardSets.Contains(kvp.Key))
            {
                claimBanner.SetActive(true);
                break;
            }
        }
        countdownTimer.gameObject.SetActive(!claimBanner.activeSelf);
        
        //能兑换或有新卡
        int leastNeedStarNum = GameManager.DataTable.GetDataTable<DTCardStarRewardData>().Data.GetRewardByID(1).StarNum;
        warningSign.SetActive(CardModel.Instance.ExtraStarNum >= leastNeedStarNum || 
                              CardModel.Instance.NewCardDict.Sum(kvp => kvp.Value.Count) > 0);
    }

    public void ShowGuide()
    {
        if (!gameObject.activeSelf)
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel);
            return;
        }

        //试图修复很偶发的同时弹出别的弹框导致教程被挡住
        if (GameManager.UI.GetUIGroup(UIFormType.PopupUI).CurrentUIForm != null)
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel);
            return;
        }

        entranceBtn.interactable = false;
        GameManager.UI.ShowUIForm("CardSetGuideMenu", form =>
        {
            CardSetGuideMenu guideMenu = form as CardSetGuideMenu;
            
            Transform target = entranceBtn.transform;
            Transform originalParent = target.parent;
            target.SetParent(form.transform);
            target.position = transform.position;
            
            guideMenu?.ShowArrow(target.position + new Vector3(0.2f, 0));
            entranceBtn.OnInit(() =>
            {
                target.SetParent(originalParent);
                target.localPosition = Vector3.zero;
                
                GameManager.UI.HideUIForm(form);
                
                GameManager.UI.ShowUIForm($"CardSetMainMenu{CardModel.Instance.CardActivityID}", menu =>
                {
                    menu.SetHideAction(() => 
                        GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel));
                });

                entranceBtn.OnInit(() =>
                    GameManager.UI.ShowUIForm($"CardSetMainMenu{CardModel.Instance.CardActivityID}"));
            });
            entranceBtn.interactable = true;
        });
    }
}
