using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarSliderManager : MonoBehaviour
{
    [SerializeField] private DelayButton[] rewardBtns;

    [SerializeField] private TextMeshProUGUI finishedText;
    [SerializeField] private TextMeshProUGUI[] rewardTexts;
    [SerializeField] private GameObject[] ticks;
    [SerializeField] private Image[] rewardButtonCloseImgs1;
    [SerializeField] private Image[] rewardButtonCloseImgs2;
    [SerializeField] private Image[] rewardButtonOpenImgs1;
    [SerializeField] private Image[] rewardButtonOpenImgs2;
    [SerializeField] private Slider slider;
    [SerializeField] private Transform bg, root;


    private int _finishedDays;
    private int _totalDays;
    private int[] _rewardNeedDays;
    private DateTime _dateTime;

    public void Init(DateTime dateTime)
    {
        root.gameObject.SetActive(true);
        _dateTime = dateTime;
        _rewardNeedDays = GameManager.Task.CalendarChallengeManager.GetRewardNeedDays(dateTime);
        _finishedDays = GameManager.Task.CalendarChallengeManager.GetChallengeProgressByMonth(dateTime);
        if (GameManager.Task.CalendarChallengeManager.NeedToShowAnim)
        {
            _finishedDays -= 1;
        }
        Image[] openImgs = null;
        Image[] closeImgs = null;
        for (var i = 0; i < rewardBtns.Length; i++)
        {
            if (_dateTime.Month % 2 == 0)
            {
                rewardButtonCloseImgs2[i].gameObject.SetActive(false);
                rewardButtonOpenImgs2[i].gameObject.SetActive(false);
                openImgs = rewardButtonOpenImgs1;
                closeImgs = rewardButtonCloseImgs1;
            }
            else
            {

                rewardButtonCloseImgs1[i].gameObject.SetActive(false);
                rewardButtonOpenImgs1[i].gameObject.SetActive(false);
                openImgs = rewardButtonOpenImgs2;
                closeImgs = rewardButtonCloseImgs2;
            }
        }

        finishedText.text = _finishedDays.ToString();
        _totalDays = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        slider.value = _finishedDays == 0 ? 0 : 0.03f + 0.97f * _finishedDays / _totalDays;
        for (var i = 0; i < rewardBtns.Length; i++)
        {
            if (_finishedDays >= _rewardNeedDays[i])
            {
                closeImgs[i].gameObject.SetActive(false);
                openImgs[i].gameObject.SetActive(true);
                ticks[i].SetActive(true);
                rewardTexts[i].gameObject.SetActive(false);
            }
            else
            {
                closeImgs[i].gameObject.SetActive(true);
                openImgs[i].gameObject.SetActive(false);
                ticks[i].SetActive(false);
                rewardTexts[i].text = _rewardNeedDays[i].ToString();
                rewardTexts[i].gameObject.SetActive(true);
            }

            var btn = rewardBtns[i];

            var text = rewardTexts[i];
            var index = i;
            var needDays = _rewardNeedDays[i];
            var isFinished = _finishedDays >= _rewardNeedDays[index];
            if (isFinished)
                btn.interactable = false;
            else
            {
                btn.interactable = true;
                btn.SetBtnEvent(() =>
                {
                    OnRewardButtonClick(index, btn);
                });
            }

            text.text = needDays.ToString();
        }
    }

    private void OnRewardButtonClick(int index, DelayButton btn)
    {
        GameManager.Event.Fire(this,
            CommonEventArgs.Create(CommonEventType.ShowCalendarChallengeRewardPromptBox, index,
                PromptBoxShowDirection.Down, btn.transform.position));
    }

    public void ShowSliderAnim(Action onShowComplete = null)
    {
        StartCoroutine(ShowSliderAnimCoroutine(onShowComplete));
    }

    IEnumerator ShowSliderAnimCoroutine(Action onShowComplete = null)
    {
        _finishedDays += 1;
        var time = 0.4f;
        var anim1 = slider.DOValue(0.03f + 0.97f * _finishedDays / _totalDays, time);
        anim1.onComplete += () => { finishedText.text = _finishedDays.ToString(); };
        yield return anim1.WaitForCompletion();
        if (_rewardNeedDays.Contains(_finishedDays))
        {
            var index = Array.IndexOf(_rewardNeedDays, _finishedDays);
            ticks[index].SetActive(true);
            rewardTexts[index].gameObject.SetActive(false);
            var img = rewardButtonCloseImgs1[index];
            GameManager.Task.CalendarChallengeManager.SendRewards(index, () =>
            {

                var panel = RewardManager.Instance.RewardPanel as CalendarRewardPanel;
                panel.gameObject.SetActive(false);
                panel.Date = _dateTime;
                var pos = img.transform.position;
                var transform1 = panel.chestBefore.transform;
                var transform2 = panel.chestSingle.transform;
                transform1.position = pos;
                transform2.position = pos;
                transform1.localScale = new Vector3(0.32f, 0.32f, 1);
                transform2.localScale = new Vector3(0.32f, 0.32f, 1);
                GameManager.UI.HideUIForm("CalendarChallengeMenu");
            }, flag =>
             {
                 var dateTime = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
                 if (dateTime != DateTime.MinValue)
                 {
                     GameManager.UI.ShowUIForm("CalendarChallengeMenu",f =>
                     {
                         GameManager.Task.AddDelayTriggerTask(0.1f, onShowComplete);
                     });
                 }
                 else
                 {
                     GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeMenu);
                     GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeGuide);
                 }
             });
        }
        else
        {
            GameManager.Task.CalendarChallengeManager.LastWinDate = DateTime.MinValue;
            GameManager.Task.AddDelayTriggerTask(0.2f, onShowComplete);
        }
    }
}