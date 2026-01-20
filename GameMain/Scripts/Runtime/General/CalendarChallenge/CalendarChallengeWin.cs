using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarChallengeWin : PopupMenuForm
{
    public Image progressImg;
    public TextMeshProUGUI progressText, dateText1, dateText2, dayText;
    public TextMeshProUGUILocalize monthText;
    public Transform dateImageTransform, progressTransform;
    public DelayButton claimBtn;
    public GameObject coinObj, dateObj, tapObj, effectObj1, effectObj2;
    public CoinBarManager coinBar;

    private DateTime _dateTime;
    private bool _canClose;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        _dateTime = GameManager.Task.CalendarChallengeManager.LastWinDate;
        progressImg.fillAmount = 0;
        progressText.text = "0/1";
        var text = GameManager.Localization.GetString(
            $"Calendar.{GameManager.Task.CalendarChallengeManager.GetMonthString(_dateTime.Month, false)}");
        var dateString = _dateTime.ToString("M", CalendarChallengeManager.GetCultureInfoFromLanguage(GameManager.Localization.Language));
        monthText.SetTerm(dateString);
        dayText.text = dateText1.text = dateText2.text = _dateTime.Day.ToString();
        effectObj1.SetActive(true);
        effectObj2.SetActive(false);
        effectObj1.transform.localPosition = Vector3.zero;
        coinObj.SetActive(true);
        dateObj.SetActive(false);
        tapObj.SetActive(false);
        claimBtn.gameObject.SetActive(true);
        claimBtn.SetBtnEvent(OnClaimButtonClick);
        _canClose = false;
        gameObject.SetActive(false);
        coinBar.OnInit(null, null, null);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        coinBar.OnRelease();
        claimBtn.transform.localScale = Vector3.one;
        progressImg.fillAmount = 0;
        progressText.text = "0/1";
        coinObj.transform.localScale = Vector3.one;
        progressTransform.localScale = Vector3.one;
        base.OnReset();
    }

    public override void OnClose()
    {
        GameManager.Ads.ShowInterstitialAd(ProcedureUtil.ProcedureGameToMap);
    }

    private void OnClaimButtonClick()
    {
        claimBtn.transform.DOScale(Vector3.zero, 0.2f).onComplete += () =>
        {
            claimBtn.gameObject.SetActive(false);
        };
        progressImg.DOFillAmount(1, 0.2f).onComplete += () =>
        {
            progressText.text = "1/1";
            effectObj1.transform.DOScale(0, 0.05f).onComplete += () =>
             {
                 effectObj1.SetActive(false);
                 effectObj1.transform.localScale = new Vector3(2, 2, 1);
             };
            coinObj.transform.DOScale(0, 0.3f);
            RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin, 30);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
            {
                coinObj.SetActive(false);
                progressTransform.DOScale(Vector3.zero, 0.1f);
                dateImageTransform.DOScale(new Vector3(2, 2, 1), 0.4f);
                dateImageTransform.DOMove(dateObj.transform.position, 0.4f).onComplete += () =>
                {
                    effectObj2.SetActive(true);
                    dateObj.SetActive(true);
                    tapObj.SetActive(true);
                    dateImageTransform.gameObject.SetActive(false);
                    _canClose = true;
                };
            });
        };
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        coinBar.OnUpdate(elapseSeconds, realElapseSeconds);
        if (!_canClose)
        {
            return;
        }
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
#else
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
            OnClose();
        }

    }
}
