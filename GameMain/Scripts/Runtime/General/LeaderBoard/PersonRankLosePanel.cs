using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersonRankLosePanel : BaseGameFailPanel
{
    public Image[] CurrentMultObjects;
    public TextMeshProUGUI[] CurrentMultTexts;
    public TextMeshProUGUILocalize describeText;
    public Material btnGreenMat, btnRedMat;
    public Sprite[] greenBtnSprites, redBtnSprites;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.PersonRankLosePanel;

    public override bool IsShowFailPanel
    {
        get
        {
            return GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing &&
                   GameManager.Task.PersonRankManager.ContinuousWinTime >= 1;
        }
    }

    private bool m_IsStatus = false;

    private void OnDisable()
    {
        m_IsStatus = false;
    }

    public void ShowStartFailPanel()
    {
        m_IsStatus = false;
        ShowFailPanel(null);
    }

    public override void CloseFailPanel(Action finishAction)
    {
        m_IsStatus = true;
        ShowFailPanel(finishAction);
    }

    public override void ShowFailPanel(Action callback)
    {
        if (!m_IsStatus)
        {
            describeText.SetParameterValue("0", "<color=#217F02>");
            describeText.SetParameterValue("1", "</color>");

            var index = GameManager.Task.PersonRankManager.ContinuousWinTime;
            for (var i = 0; i < CurrentMultObjects.Length; i++)
            {
                int spriteIdx;
                if (i == 0)
                    spriteIdx = 0;
                else if (i >= 1 && i <= 3)
                {
                    spriteIdx = 1;
                }
                else
                {
                    spriteIdx = 2;
                }
                CurrentMultTexts[i].fontSharedMaterial = i == index ? btnGreenMat : btnRedMat;
                CurrentMultObjects[i].sprite = i == index ? greenBtnSprites[spriteIdx] : redBtnSprites[spriteIdx];

                CurrentMultObjects[i].gameObject.SetActive(i == index);
            }
        }
        else
        {
            var index = GameManager.Task.PersonRankManager.ContinuousWinTime;
            var time = 0.2f;
            for (var i = 0; i < index; i++)
            {
                var idx = i;
                GameManager.Task.AddDelayTriggerTask(0.2f * idx,
                    () =>
                    {
                        GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                        CurrentMultTexts[index - idx].DOFade(0f, time);
                        CurrentMultObjects[index - idx].DOFade(0, time).onComplete += () =>
                        {
                            CurrentMultObjects[index - idx].gameObject.SetActive(false);
                            CurrentMultObjects[index - idx].DOFade(1f, 0f);
                            CurrentMultTexts[index - idx].DOFade(1f, 0f);
                        };
                    });
                GameManager.Task.AddDelayTriggerTask(0.1f + 0.2f * idx,
                    () =>
                    {
                        CurrentMultObjects[index - 1 - idx].DOFade(0f, 0f);
                        CurrentMultTexts[index - 1 - idx].DOFade(0f, 0f);
                        CurrentMultObjects[index - 1 - idx].gameObject.SetActive(true);
                        CurrentMultTexts[index - 1 - idx].DOFade(1f, 0.1f);
                        CurrentMultObjects[index - 1 - idx].DOFade(1f, 0.1f);
                    });
            }

            GameManager.Task.AddDelayTriggerTask(0.5f + index * 0.2f, () => { callback?.Invoke(); });
        }
    }
}