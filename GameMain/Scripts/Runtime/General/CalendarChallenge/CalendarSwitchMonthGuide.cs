using System;
using UnityEngine;
using UnityEngine.UI;

public class CalendarSwitchMonthGuide : MonoBehaviour
{
    public TextPromptBox textPromptBox;

    public void OnInit(Button btn, Action callback)
    {
        GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide = true;
        btn.interactable = true;
        var cachedTrans = btn.transform;
        var originParent = cachedTrans.parent;
        int index = cachedTrans.GetSiblingIndex();
        gameObject.SetActive(true);
        cachedTrans.SetParent(transform);
        var pos = cachedTrans.position;
        textPromptBox.SetText("Calendar.Now you can keep playing the old months' levels");
        textPromptBox.ShowPromptBox(PromptBoxShowDirection.Down, pos);
        void ClickAction()
        {
            cachedTrans.SetParent(originParent);
            cachedTrans.SetSiblingIndex(index);
            textPromptBox.HidePromptBox();
            gameObject.SetActive(false);
            callback?.Invoke();
            btn.onClick.RemoveListener(ClickAction);
        }
        btn.onClick.AddListener(ClickAction);
    }
}
