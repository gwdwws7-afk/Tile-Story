using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public sealed class EvaluationMenu : PopupMenuForm
{
    private int starNum = 0;

    [SerializeField]
    private DelayButton Close_Btn,Rate_Btn,GrayRate_Btn;
    [SerializeField]
    private DelayButton[] Star_Btns;
    [SerializeField]
    private GameObject[] Stars;
    [SerializeField]
    private GameObject Hander_Obj;
    [SerializeField]
    private TextMeshProUGUILocalize Star_Text;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rate_Menu_Show);

        SetBtnEvent();
        ShowRateBtn(starNum);
    }
    
    private void SetBtnEvent()
    {
        for (int i = 0; i < Star_Btns.Length; i++)
        {
            int starIndex = i + 1;
            Star_Btns[i].SetBtnEvent(() =>
            {
                OnStarButtonClick(starIndex);
            });
        }
        Close_Btn.SetBtnEvent(()=> 
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rate_Menu_Close);
            GameManager.UI.HideUIForm(this);
            OnClose();
        });
        Rate_Btn.SetBtnEvent(()=> 
        {
            //GOTO
            GameManager.Firebase.RecordMessageByEvent(
                Constant.AnalyticsEvent.Rate_Stars,
                new Parameter( "Star", starNum));
            
            OnRateButtonClick();
        });
        GrayRate_Btn.interactable = false;
    }

    private void ShowRateBtn(int rateStarNum)
    {
        bool isCanClickRateBtn = rateStarNum > 0;
        GrayRate_Btn.gameObject.SetActive(!isCanClickRateBtn);
        Rate_Btn.gameObject.SetActive(isCanClickRateBtn);

        ShowStarText(rateStarNum);
    }

    public void OnRateButtonClick()
    {
        GameManager.UI.HideUIForm(this);
        OnClose();

        bool isForceOpenURL = GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Force_Open_Rate_URL, true);
        if (!isForceOpenURL)
        {
            if (starNum < 5) return;
        }
        if (starNum < 5) return;

        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rate_FiveStar);
        GameManager.PlayerData.RecordEffectiveRate();

        int clickedRateButton = PlayerPrefs.GetInt(Constant.PlayerData.ClickedRateButton, 0);
#if UNITY_ANDROID && !UNITY_EDITOR

#if AmazonStore
        string appStoreLink = "amzn://apps/android?p=" + Application.identifier;
        Application.OpenURL(appStoreLink);
#else
        if (clickedRateButton == 0) 
        {
            PlayerPrefs.SetInt(Constant.PlayerData.ClickedRateButton, 1);
            OnPause();
            GoogleViewMananger.Mananger.RequestReview(b =>
            {
                if (GoogleViewMananger.Mananger.IsCanLaunch())
                {
                    GoogleViewMananger.Mananger.LaunchReview(result =>
                    {
                        OnResume();

                        if (!result)
                        {
                            Application.OpenURL("market://details?id=" + Application.identifier);
                        }
                    });
                }
                else
                {
                    OnResume();

                    Application.OpenURL("market://details?id=" + Application.identifier);
                }
            });
        }
        else
        {
            Application.OpenURL("market://details?id=" + Application.identifier);
        }
#endif
        
#elif UNITY_IPHONE || UNITY_IOS
        bool result = false;
        if (PlayerPrefs.GetInt("IsHaveOpenStoreReviewByIOS", 0) == 0) 
        {
            PlayerPrefs.SetInt("IsHaveOpenStoreReviewByIOS", 1);
            result = UnityEngine.iOS.Device.RequestStoreReview();
            //Debug.LogError("RequestStoreReview: " + result.ToString());
        }
        if (!result)
            Application.OpenURL("itms-apps://itunes.apple.com/app/id=6504067069?mt=8&action=write-review");
#else
        Application.OpenURL("market://details?id=" + Application.identifier);
#endif
    }


    private void OnStarButtonClick(int index)
    {
        starNum = index;

        for (int i = 0; i < Stars.Length; i++)
        {
            if (index > i)
            {
                Stars[i].SetActive(true);
            }
            else
            {
                Stars[i].SetActive(false);
            }
        }
        Hander_Obj.gameObject.SetActive(false);
        ShowRateBtn(starNum);
    }

    private void ShowStarText(int startNum)
    {
        switch (startNum)
        {
            case 1:
                Star_Text.SetTerm("Settings.I hate it");
                Star_Text.Target.color = Color.red;
                break;
            case 2:
                Star_Text.SetTerm("Settings.I don t like it"); 
                Star_Text.Target.color = Color.red;
                break;
            case 3:
                Star_Text.SetTerm("Settings.It's ok");
                Star_Text.Target.color = new Color(13 / 255f, 129 / 255f, 27 / 255f, 255 / 255f);
                break;
            case 4:
                Star_Text.SetTerm("Settings.I like it!"); 
                Star_Text.Target.color = new Color(13 / 255f, 129 / 255f, 27 / 255f, 255 / 255f);
                break;
            case 5:
                Star_Text.SetTerm("Settings.I love it!"); 
                Star_Text.Target.color = new Color(13 / 255f, 129 / 255f, 27 / 255f, 255 / 255f);
                break;
            default:
                Star_Text.SetTerm("Rate.THE BEST WE CAN GET");  
                Star_Text.Target.color = new Color(13 / 255f, 129 / 255f, 27 / 255f, 255 / 255f);
                break;
        }
    }
}
