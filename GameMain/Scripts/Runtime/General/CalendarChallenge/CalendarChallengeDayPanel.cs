using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarChallengeDayPanel : MonoBehaviour
{
    [SerializeField] private float leftPadding, topPadding;
    [SerializeField] private Vector2 btnSize, spacingSize;
    [SerializeField] private Material blueMat, purpleMat, lockMat;
    public Sprite completedImg;
    public Sprite currentImg;
    public Sprite lockImg;
    public Sprite normalImg;

    private int _selectedIndex;
    public DateTime SelectedDateTime;

    public Transform selectedBtn;
    private DelayButton[] Buttons;
    private DateTime _dateTime;


    private void Awake()
    {
        Buttons = GetComponentsInChildren<DelayButton>();
        SetButtonsPosition();
    }


    private void SetButtonsPosition()
    {
        Buttons = GetComponentsInChildren<DelayButton>();
        var width = btnSize.x;
        var height = btnSize.y;
        var spacingX = spacingSize.x;
        var spacingY = spacingSize.y;
        var startX = width * 1 / 2;
        var startY = -height * 1 / 2;
        for (var i = 0; i < Buttons.Length; i++)
        {
            var btn = Buttons[i];
            var x = startX + (i % 7) * (width + spacingX) + leftPadding;
            var y = startY - (i / 7) * (height + spacingY) - topPadding;
            btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }

    public void Init(DateTime time, DateTime selectedDate)
    {
        OnReset();
        _dateTime = time;
        var month = time.Month;
        var startDayOfMonth = time.AddDays(-time.Day + 1).DayOfWeek;
        var dayNum = DateTime.DaysInMonth(time.Year, time.Month);
        var dayIndex = 1;
        var startIdx = startDayOfMonth == DayOfWeek.Sunday ? 6 : (int)startDayOfMonth - 1;
        for (var i = 0; i < Buttons.Length; i++)
        {
            var index = i;
            var btn = Buttons[index];
            var img = btn.GetComponentInChildren<Image>(true);
            var text = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            var transform1 = img.transform;
            var isLock = false;
            transform1.localScale = new Vector3(1f, 1f);
            transform1.localRotation = Quaternion.Euler(0, 0, 0);
            //上个月,不显示
            if (index < startIdx)
            {
                btn.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
                btn.interactable = false;
            }
            //本月
            else if (index >= startIdx && index < startIdx + dayNum)
            {
                btn.gameObject.SetActive(true);
                text.gameObject.SetActive(true);
                text.text = dayIndex.ToString();
                //当天
                if (dayIndex == DateTime.Now.Day && month == DateTime.Now.Month && time.Year == DateTime.Now.Year)
                {
                    if (GameManager.Task.CalendarChallengeManager.CheckFinishDay(DateTime.Now))
                    {
                        img.sprite = completedImg;
                        text.gameObject.SetActive(false);
                        isLock = true;
                    }
                    else
                    {
                        img.sprite = currentImg;
                        btn.transform.SetAsLastSibling();
                        text.color = Color.white;
                        text.fontSharedMaterial = purpleMat;
                    }

                }
                //今天之前
                else if (time.Year < DateTime.Now.Year || (time.Month < DateTime.Now.Month && time.Year == DateTime.Now.Year) ||
                         (month == DateTime.Now.Month && time.Year == DateTime.Now.Year && dayIndex < DateTime.Now.Day))
                {
                    if (GameManager.Task.CalendarChallengeManager.CheckFinishDay(new DateTime(time.Year, month, dayIndex)))
                    {
                        img.sprite = completedImg;
                        text.gameObject.SetActive(false);
                        isLock = true;
                    }
                    else
                    {
                        img.sprite = normalImg;
                        text.color = Color.white;
                        text.fontSharedMaterial = blueMat;
                    }
                }
                //今天之后
                else
                {
                    transform1.localScale = new Vector3(1f, 1f);
                    transform1.localRotation = Quaternion.Euler(0, 0, 0);
                    img.sprite = lockImg;
                    text.color = new Color(201 / 255f, 119 / 255f, 79 / 255f);
                    text.fontSharedMaterial = lockMat;
                    isLock = true;
                }

                btn.interactable = true;
                dayIndex++;
            }
            else
            {
                btn.gameObject.SetActive(false);
                btn.interactable = false;
            }

            btn.SetBtnEvent(() =>
            {
                if (isLock)
                    return;
                if (_selectedIndex != index && !isLock)
                {
                    img.transform.DOScale(1.3f, 0.3f);
                    img.transform.DOLocalRotate(new Vector3(0, 0, -30), 0.3f);
                    btn.transform.SetAsLastSibling();
                }
                if (selectedBtn != null && _selectedIndex != index)
                {
                    selectedBtn.DOScale(1f, 0.3f);
                    selectedBtn.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
                }
                selectedBtn = img.transform;
                _selectedIndex = index;
                SelectedDateTime = new DateTime(time.Year, time.Month, _selectedIndex - startIdx + 1);
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.CalendarChallengeDaySelected, SelectedDateTime));
            });
        }
        if (selectedDate.Month == time.Month && selectedDate.Year == time.Year)
        {
            Select(selectedDate);
        }
    }

    public void Select(DateTime time)
    {
        if (time.Month != _dateTime.Month || time.Year != _dateTime.Year) return;
        var startDayOfMonth = _dateTime.AddDays(-_dateTime.Day + 1).DayOfWeek;
        var startIdx = startDayOfMonth == DayOfWeek.Sunday ? 6 : (int)startDayOfMonth - 1;
        var showBtn = Buttons[startIdx + time.Day - 1];
        showBtn.onClick.Invoke();
    }

    public void SelectForce(DateTime time)
    {
        if (time.Month != _dateTime.Month || time.Year != _dateTime.Year) return;
        var startDayOfMonth = _dateTime.AddDays(-_dateTime.Day + 1).DayOfWeek;
        var startIdx = startDayOfMonth == DayOfWeek.Sunday ? 6 : (int)startDayOfMonth - 1;
        var index = startIdx + time.Day - 1;
        var showBtn = Buttons[index];
        if (selectedBtn != null && _selectedIndex != index)
        {
            selectedBtn.DOScale(1f, 0.3f);
            selectedBtn.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
        }
        selectedBtn = showBtn.GetComponentInChildren<Image>(true).transform;
        _selectedIndex = index;
        SelectedDateTime = new DateTime(time.Year, time.Month, _selectedIndex - startIdx + 1);
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.CalendarChallengeDaySelected, SelectedDateTime));
    }

    public void OnReset()
    {
        selectedBtn = null;
        _selectedIndex = 0;
        SelectedDateTime = DateTime.MinValue;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetButtonsPosition();
    }
#endif
}