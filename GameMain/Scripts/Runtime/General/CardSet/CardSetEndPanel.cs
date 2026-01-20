using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySelf.Model;
using TMPro;
using UnityEngine;

public class CardSetEndPanel : PopupMenuForm
{
    public DelayButton closeButton, greenButton;
    public TextMeshProUGUILocalize buttonText;
    
    public GameObject countdown;
    public CountdownTimer countdownTimer;
    public TextMeshProUGUI hour, minute, second;

    public GameObject end;
    public TextMeshProUGUILocalize cardText, cardSetText;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        closeButton.OnInit(OnButtonClick);
        greenButton.OnInit(OnButtonClick);
        
        if (DateTime.Now < CardModel.Instance.CardEndTime)
        {
            buttonText.SetTerm("Merge.Go");
            
            countdownTimer.OnReset();
            countdownTimer.CountdownOver += (sender, args) =>
            {
                GameManager.UI.HideUIForm(this);
            };
            countdownTimer.StartCountdown(CardModel.Instance.CardEndTime);
            // countdownTimer.HideZeroHour = false;
            
            countdown.SetActive(true);
            end.SetActive(false);
        }
        else
        {
            SetEnd();
        }
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        TimeSpan timeSpan = countdownTimer.LeftTime;
        hour.text = timeSpan.Days > 0 ? $"{timeSpan.Days * 24 + timeSpan.Hours}" : $"{timeSpan.Hours:D2}";
        minute.text = $"{timeSpan.Minutes:D2}";
        second.text = $"{timeSpan.Seconds:D2}";
    }

    private void OnButtonClick()
    {
        GameManager.UI.HideUIForm(this);

        //结束页面
        if (DateTime.Now >= CardModel.Instance.CardEndTime)
        {
            if (CompletedCardSetNum() != CardModel.Instance.CompletedCardSets.Count ||
                (CardModel.Instance.TotalCollectNum == CardModel.Instance.TotalCardNum &&
                 !CardModel.Instance.CompletedAll))
            {
                GameManager.UI.ShowUIForm($"CardSetLastRewardPanel{CardModel.Instance.CardActivityID}", form =>
                {
                    form.SetHideAction(() =>
                    {
                        // CardModel.Instance.ShowedEndPanel = true;
                        CardModel.Instance.ResetData();
                        //等领奖完成再结束进程
                    });
                });
            }
            else
            {
                // CardModel.Instance.ShowedEndPanel = true;
                CardModel.Instance.ResetData();
                GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
            }
        }
    }

    private void SetEnd()
    {
        buttonText.SetTerm("Card.Close");
        countdownTimer.OnReset();

        int totalCollectNum = CardModel.Instance.TotalCollectNum;
        int completedCardSetNum = CompletedCardSetNum();
        CardUtil.RecordFinishCards(totalCollectNum);
        CardUtil.RecordFinishSets(completedCardSetNum);
        cardText.SetParameterValue("0", totalCollectNum.ToString(), false);
        cardText.SetParameterValue("1", CardModel.Instance.TotalCardNum.ToString());
        cardSetText.SetParameterValue("0", completedCardSetNum.ToString(), false);
        cardSetText.SetParameterValue("1", CardModel.Instance.CardSetDict.Count.ToString());
        
        countdown.SetActive(false);
        end.SetActive(true);
    }

    private int CompletedCardSetNum()
    {
        int completedCount = 0;
        var collectDict = CardModel.Instance.CollectCardDict;
        var cardSetDict = CardModel.Instance.CardSetDict;

        foreach (var kvp in collectDict)
        {
            if (cardSetDict.TryGetValue(kvp.Key, out var cardSet) && 
                kvp.Value.Count == cardSet.CardDict.Count)
            {
                completedCount++;
            }
        }

        return completedCount;
    }
}
