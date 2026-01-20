using System;
using UnityEngine;
using UnityEngine.UI;

public class CalendarChallengeEntranceBtn : EntranceUIForm
{
    public Image fillImage;
    public TextMeshProUGUILocalize text;
    public GameObject redPoint;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        var curFinishedDay = GameManager.Task.CalendarChallengeManager.GetChallengeProgressByMonth(DateTime.Today);
        var progress = (float)curFinishedDay / DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) * 0.7f +
                       0.15f;
        fillImage.fillAmount = progress;
        var nearestUncompletedLevel = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
        var finishAll = nearestUncompletedLevel == DateTime.MinValue;
        text.SetTerm(finishAll ? "Story.Finished" : "Common.Play");
        redPoint.SetActive(!finishAll && !GameManager.Task.CalendarChallengeManager.HasUsedTodayFreeChance);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public void ShowGuide()
    {
        entranceBtn.interactable = false;
        GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
        {
            entranceBtn.interactable = true;
            GameManager.Task.CalendarChallengeManager.HasShowedCalendarChallengeGuide = true;
            form.gameObject.SetActive(false);
            var guideMenu = form.GetComponent<CommonGuideMenu>();
            GameObject btnObj = Instantiate(gameObject, transform.position, Quaternion.identity, form.transform);
            btnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(125f, btnObj.GetComponent<RectTransform>().anchoredPosition.y);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                GameManager.UI.ShowUIForm("CalendarChallengeMenu");
            });
            guideMenu.SetText("Calendar.Tap to enter Puzzle Calendar and win prizes!");
            guideMenu.tipBox.SetOkButton(false);
            var position = btnObj.transform.position + new Vector3(0.3f, 0, 0);
            guideMenu.ShowGuideArrow(position, position + new Vector3(0.15f, 0, 0), PromptBoxShowDirection.Left);
            guideMenu.tipBox.transform.position =
                new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)).x, transform.position.y - 0.3f);
            guideMenu.OnShow(null, null);

            guideMenu.autoHideAction = () =>
            {
                if (btnObj != null)
                    Destroy(btnObj);
                GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeGuide);
            };

            void ClickAction()
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_First_Open);
                if (btnObj != null)
                    Destroy(btnObj);
                GameManager.UI.HideUIForm(form);
                guideMenu.guideImage.onTargetAreaClick = null;
            }

            btn.onClick.AddListener(ClickAction);
        }, () => { GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeGuide); });
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("CalendarChallengeMenu");
    }
}