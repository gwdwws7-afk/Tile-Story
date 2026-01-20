using DG.Tweening;
using GameFramework.Event;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CalendarChallengeMenu : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    enum ButtonState
    {
        Free,
        NeedAds,
        NeedCoins,
        Locked
    }

    public CalendarSliderManager calendarSliderManager;
    public CalendarChallengeDayPanel calendarChallengeDayPanel;

    [SerializeField] private DelayButton closeBtn, playBtn, prevBtn, nextBtn;
    [SerializeField] private GameObject leftGrayBtn, rightGrayBtn, coinObj, adsObj;
    [SerializeField] private GameObject normalTextObj, coinTextObj, adsTextObj;
    [SerializeField] private TextMeshProUGUILocalize monthText, normalPlayBtnText, coinPlayBtnText, adsPlayBtnText;
    [SerializeField] private Material grayImgMat;
    [SerializeField] private Image playBtnImg;
    [SerializeField] private Transform puzzle, largePuzzle;
    [SerializeField] private ItemPromptBox itemPromptBox;
    public ParticleSystem punchEffect;
    [SerializeField] private CalendarSwitchMonthGuide calendarSwitchMonthGuide;
    [SerializeField] private Transform[] titles;


    private DateTime _currentMenuDateTime;
    private DateTime _selectedDate;
    private ButtonState _buttonState;
    private DateTime _lastDay;

    private bool _showSwitchGuide;
    private bool _finishAll;

    private void OnDestroy()
    {
        grayImgMat = null;
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnCalendarChallengeDaySelected);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        var language = GameManager.Localization.Language;
        if (language is Language.ChineseSimplified or Language.ChineseTraditional or Language.Arabic)
        {
            titles[0].gameObject.SetActive(false);
            titles[1].gameObject.SetActive(true);
            titles[1].localPosition = new Vector3(0, -86);
        }
        else
        {
            titles[0].gameObject.SetActive(true);
            titles[1].gameObject.SetActive(true);
            titles[1].localPosition = new Vector3(0, -108);

        }
        _lastDay = GameManager.Task.CalendarChallengeManager.LastDay();
        playBtn.SetBtnEvent(OnPlayButtonClick);
        prevBtn.SetBtnEvent(OnPrevButtonClick);
        nextBtn.SetBtnEvent(OnNextButtonClick);
        closeBtn.SetBtnEvent(OnClose);
        puzzle.localScale = Vector3.zero;
        var nearestUncompletedLevel = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
        _finishAll = nearestUncompletedLevel == DateTime.MinValue;
        // GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide = false;
        _showSwitchGuide = (nearestUncompletedLevel.Month != DateTime.Today.Month ||
                            nearestUncompletedLevel.Year != DateTime.Today.Year) &&
                           !GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide;
        if (GameManager.Task.CalendarChallengeManager.NeedToShowAnim)
        {
            _currentMenuDateTime = GameManager.Task.CalendarChallengeManager.LastWinDate;
            _selectedDate = _currentMenuDateTime;
        }
        else if (GameManager.Task.CalendarChallengeManager.LastFailDate != DateTime.MinValue)
        {
            _currentMenuDateTime = GameManager.Task.CalendarChallengeManager.LastFailDate;
            _selectedDate = _currentMenuDateTime;
        }
        else if (_showSwitchGuide || _finishAll)
        {
            _currentMenuDateTime = DateTime.Today;
            _selectedDate = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
        }
        else
        {
            _currentMenuDateTime = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
            _selectedDate = _currentMenuDateTime;
        }


        SetMonthText();
        calendarChallengeDayPanel.Init(_currentMenuDateTime, _currentMenuDateTime);
        calendarSliderManager.Init(_currentMenuDateTime);
        SetChangeMonthButtonState();
        SetPlayButtonState();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (GameManager.Task.CalendarChallengeManager != null &&
            GameManager.Task.CalendarChallengeManager.NeedToShowAnim)
        {
            return;
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
#else
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
            HideRewardItemPromptBox();
        }
    }

    private void SetMonthText()
    {
        var text = GameManager.Localization.GetString(
            $"Calendar.{GameManager.Task.CalendarChallengeManager.GetMonthString(_currentMenuDateTime.Month, false)}");
        monthText.SetTerm($"{text} {_currentMenuDateTime.Year.ToString()}");
    }

    private void OnRewardAdEarnedReward(object sender, GameEventArgs e)
    {
        var ne = e as RewardAdEarnedRewardEventArgs;
        if (ne == null || ne.UserData.ToString() != "DailyChallengeStart")
        {
            return;
        }

        // _selectedDate = calendarChallengeDayPanel.SelectedDateTime;
        GameManager.Task.CalendarChallengeManager.StartCalendarChallenge(_selectedDate, 1);
    }

    private void OnCalendarChallengeDaySelected(object sender, GameEventArgs e)
    {
        var args = e as CommonEventArgs;
        if (args == null || args.Type != CommonEventType.CalendarChallengeDaySelected &&
            args.Type != CommonEventType.ShowCalendarChallengeRewardPromptBox)
        {
            return;
        }

        if (args.Type == CommonEventType.CalendarChallengeDaySelected)
        {
            var dateTime = args.UserDatas[0] as DateTime?;
            if (dateTime == null)
            {
                return;
            }

            _selectedDate = dateTime.Value;
            SetPlayButtonState();
        }

        if (args.Type == CommonEventType.ShowCalendarChallengeRewardPromptBox)
        {
            var index = (int)args.UserDatas[0];
            var direction = (PromptBoxShowDirection)args.UserDatas[1];
            var position = (Vector3)args.UserDatas[2];
            ShowRewardItemPromptBox(index, direction, position);
        }
    }

    private void ShowRewardItemPromptBox(int index, PromptBoxShowDirection direction, Vector3 position)
    {
        var rewards = GameManager.DataTable.GetDataTable<DTCalendarChallengeData>().Data.GetRewardsByLevel(index);
        itemPromptBox.Init(rewards.Keys.ToList(), rewards.Values.ToList());
        if (index == 3)
            itemPromptBox.triangelOffset = 100f;
        else if (index == 2)
            itemPromptBox.triangelOffset = 180f;
        else if (index == 1)
            itemPromptBox.triangelOffset = 250f;
        else if (index == 0)
            itemPromptBox.triangelOffset = 300f;

        itemPromptBox.ShowPromptBox(direction, position);
    }

    private void HideRewardItemPromptBox()
    {
        itemPromptBox.HidePromptBox();
    }

    private void SetPlayButtonState()
    {
        DateTime selectedData = _selectedDate;
        //����Specified time is not supported in this calendar.���npe
        if (selectedData == DateTime.MinValue)
            selectedData = Constant.GameConfig.DateTimeMin;

        var text1 = GameManager.Localization.GetString("Common.Play");
        var monthLong = GameManager.Localization.GetString(
            $"Calendar.{GameManager.Task.CalendarChallengeManager.GetMonthString(selectedData.Month, false)}");
        var monthShort = GameManager.Localization.GetString(
            $"Calendar.{GameManager.Task.CalendarChallengeManager.GetMonthString(selectedData.Month, true)}");
        var dateString = selectedData.ToString("M",
            CalendarChallengeManager.GetCultureInfoFromLanguage(GameManager.Localization.Language));
        dateString = dateString.Replace(monthLong, monthShort);
        
        normalPlayBtnText.SetTerm(_finishAll ? "Story.ComingSoon" : $"{text1} {dateString}");
        
        if (GameManager.Task.CalendarChallengeManager.CheckFinishDay(selectedData) ||
            selectedData > DateTime.Now.Date || _finishAll)
        {
            normalPlayBtnText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
            playBtnImg.material = grayImgMat;
            // playBtnText.transform.localPosition = new Vector3(-1, 4);
            // playBtnText.Target.fontSizeMax = 80;
            // adsObj.SetActive(false);
            // coinObj.SetActive(false);
            normalTextObj.SetActive(true);
            coinTextObj.SetActive(false);
            adsTextObj.SetActive(false);
            _buttonState = ButtonState.Locked;
            return;
        }

        
        playBtnImg.material = null;
        if (selectedData == DateTime.Now.Date && !GameManager.Task.CalendarChallengeManager.HasUsedTodayFreeChance)
        {
            // playBtnText.transform.localPosition = new Vector3(-1, 4);
            // playBtnText.Target.fontSizeMax = 60;
            // adsObj.SetActive(false);
            // coinObj.SetActive(false);
            normalPlayBtnText.SetMaterialPreset(MaterialPresetName.Btn_Green);
            normalTextObj.SetActive(true);
            coinTextObj.SetActive(false);
            adsTextObj.SetActive(false);
            _buttonState = ButtonState.Free;
        }
        else if (!GameManager.Ads.CheckRewardedAdIsLoaded() || GameManager.PlayerData.CoinNum >= 30)
        {
            // playBtnText.transform.localPosition = new Vector3(0, 34);
            // playBtnText.Target.fontSizeMax = 60;
            // adsObj.SetActive(false);
            // coinObj.SetActive(true);
            coinPlayBtnText.SetTerm(_finishAll ? "Story.ComingSoon" : $"{text1} {dateString}");
            coinPlayBtnText.SetMaterialPreset(MaterialPresetName.Btn_Green);
            normalTextObj.SetActive(false);
            coinTextObj.SetActive(true);
            adsTextObj.SetActive(false);
            _buttonState = ButtonState.NeedCoins;
        }
        else
        {
            // playBtnText.transform.localPosition = new Vector3(59, 4);
            // playBtnText.Target.fontSizeMax = 80;
            // adsObj.SetActive(true);
            // coinObj.SetActive(false);
            adsPlayBtnText.SetTerm(_finishAll ? "Story.ComingSoon" : $"{text1} {dateString}");
            adsPlayBtnText.SetMaterialPreset(MaterialPresetName.Btn_Green);
            normalTextObj.SetActive(false);
            coinTextObj.SetActive(false);
            adsTextObj.SetActive(true);
            _buttonState = ButtonState.NeedAds;
        }
    }

    public override void OnReset()
    {
        calendarChallengeDayPanel.OnReset();
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnCalendarChallengeDaySelected);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        base.OnReset();
    }

    public override void OnRelease()
    {
        itemPromptBox.OnRelease();
        base.OnRelease();
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        base.OnShowInit(showInitSuccessAction, userData);
        if (GameManager.Task.CalendarChallengeManager.NeedToShowAnim)
        {
            OnPause();
            calendarChallengeDayPanel.SelectForce(_currentMenuDateTime);
            puzzle.position = calendarChallengeDayPanel.selectedBtn.position;
            puzzle.gameObject.SetActive(true);
            puzzle.DOScale(Vector3.one, 0.3f).onComplete += () =>
            {
                puzzle.DOMove(largePuzzle.position, 0.8f).SetEase(Ease.InCubic).onComplete += () =>
                {
                    puzzle.gameObject.SetActive(false);
                    largePuzzle.DOScale(new Vector3(0.85f, 0.85f), 0.15f).onComplete += () =>
                    {
                        punchEffect.Play();
                        largePuzzle.DOScale(Vector3.one, 0.15f);
                        calendarSliderManager.ShowSliderAnim(() =>
                        {
                            OnResume();
                            var dateTime = GameManager.Task.CalendarChallengeManager.GetNearestUncompletedLevel();
                            if (_showSwitchGuide)
                            {
                                OnPause();
                                _selectedDate = dateTime;
                                ShowGuide();
                            }
                            else
                            {
                                if (dateTime.Month != _currentMenuDateTime.Month)
                                {
                                    _currentMenuDateTime = dateTime;
                                    SetMonthText();
                                    calendarSliderManager.Init(_currentMenuDateTime);
                                    calendarChallengeDayPanel.Init(_currentMenuDateTime, _selectedDate);
                                    SetChangeMonthButtonState();
                                }
                                calendarChallengeDayPanel.Select(dateTime);
                            }

                        });
                    };
                };
            };
        }
        else if (GameManager.Task.CalendarChallengeManager.LastFailDate != DateTime.MinValue)
        {
            OnResume();
            calendarChallengeDayPanel.Select(_currentMenuDateTime);
            GameManager.Task.CalendarChallengeManager.LastFailDate = DateTime.MinValue;
        }
        else
        {
            OnResume();
            if (_showSwitchGuide)
            {
                OnPause();
                GameManager.Task.AddDelayTriggerTask(0.1f, ShowGuide);
            }
            else
            {
                calendarChallengeDayPanel.Select(_currentMenuDateTime);
            }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeMenu);
        GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeGuide);
    }

    public void OnEscapeBtnClicked()
    {
        OnClose();
    }

    public override void OnPause()
    {
        closeBtn.interactable = false;
        playBtn.interactable = false;
        prevBtn.interactable = false;
        nextBtn.interactable = false;

        base.OnPause();
    }

    public override void OnResume()
    {
        closeBtn.interactable = true;
        playBtn.interactable = true;
        prevBtn.interactable = true;
        nextBtn.interactable = true;

        base.OnResume();
    }

    private void ShowGuide()
    {
        OnPause();
        if (_selectedDate.Month >= DateTime.Today.Month && _selectedDate.Year >= DateTime.Today.Year ||
            _finishAll || GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide)
        {
            OnResume();
            return;
        }

        calendarSwitchMonthGuide.OnInit(prevBtn, OnResume);
    }

    private void SetChangeMonthButtonState()
    {
        if (_currentMenuDateTime.Year == _lastDay.Year && _currentMenuDateTime.Month == _lastDay.Month)
        {
            prevBtn.gameObject.SetActive(false);
            leftGrayBtn.SetActive(true);
        }
        else
        {
            prevBtn.gameObject.SetActive(true);
            leftGrayBtn.SetActive(false);
        }

        if (_currentMenuDateTime.Year == DateTime.Today.Year && _currentMenuDateTime.Month == DateTime.Today.Month)
        {
            nextBtn.gameObject.SetActive(false);
            rightGrayBtn.SetActive(true);
        }
        else
        {
            nextBtn.gameObject.SetActive(true);
            rightGrayBtn.SetActive(false);
        }
    }

    private void OnNextButtonClick()
    {
        _currentMenuDateTime = _currentMenuDateTime.AddMonths(1);
        GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide = true;
        SetMonthText();
        calendarSliderManager.Init(_currentMenuDateTime);
        calendarChallengeDayPanel.Init(_currentMenuDateTime, _selectedDate);
        SetChangeMonthButtonState();
    }

    private void OnPrevButtonClick()
    {
        _currentMenuDateTime = _currentMenuDateTime.AddMonths(-1);
        GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide = true;
        SetMonthText();
        calendarSliderManager.Init(_currentMenuDateTime);
        calendarChallengeDayPanel.Init(_currentMenuDateTime, _selectedDate);
        SetChangeMonthButtonState();
    }

    private void OnPlayButtonClick()
    {
        // _selectedDate = calendarChallengeDayPanel.SelectedDateTime;
        switch (_buttonState)
        {
            case ButtonState.Free:
                GameManager.Task.CalendarChallengeManager.StartCalendarChallenge(_selectedDate);
                break;
            case ButtonState.NeedAds:
                GameManager.Ads.ShowRewardedAd("DailyChallengeStart");
                break;
            case ButtonState.NeedCoins:
                if (GameManager.PlayerData.CoinNum < 30)
                {
                    GameManager.UI.ShowUIForm("ShopMenuManager");
                    GameManager.Firebase.RecordCoinNotEnough(6, GameManager.PlayerData.NowLevel);
                    break;
                }
                GameManager.PlayerData.UseItem(TotalItemData.Coin, 30);
                GameManager.Task.CalendarChallengeManager.StartCalendarChallenge(_selectedDate, 2);
                break;
            case ButtonState.Locked:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}