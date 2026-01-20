using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using GameFramework.Event;
using MySelf.Model;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapSettingMenuPanel : PopupMenuForm
{
    //public SwitchButton KidButton;
    [SerializeField]
    private SwitchButton Music_Btn, Audio_Btn, Shake_Btn, Tip_Btn, Notification_Btn;
    [SerializeField]
    private DelayButton Close_Btn, Service_Btn, Policy_Btn, PrivacyOptionsBtn, BG_Btn, Quit_Btn;
    [SerializeField]
    private DelayButton Language_Btn, ChangeMusic_Btn, ChangeHeadPortrait_Btn, ChangeName_Btn, Email_Btn, SaveData_Btn, FacebookSignOut_Btn, GoogleSignOut_Btn, Copy_Btn;
    [SerializeField]
    private InputField PlayerName_Input;
    [SerializeField]
    private Image Player_Image;

    [SerializeField]
    private GameObject HeadPortraitRedPoint_Obj;

    [SerializeField] private RectTransform[] AmazonMoveTransforms;

    public TextMeshProUGUI Version_Text;
    public TextMeshProUGUI UserID_Text;

    private const string email = "bubbleblast@linkdesks.com";
    
    private AsyncOperationHandle textureAssetHandle;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId,CommonEventCallBack);

        SetBtnEvent();
        SetVersion();
        HeadPortraitRedPoint_Obj.gameObject.SetActive(GameManager.PlayerData.IsShowHeadPortraitRedPoint);
#if AmazonStore || UNITY_IOS
        AmazonMoveTransforms[0].anchoredPosition = new Vector2(0, -50);
        AmazonMoveTransforms[0].sizeDelta = new Vector2(1000, 200);
        AmazonMoveTransforms[1].anchoredPosition = Vector2.zero;
        AmazonMoveTransforms[1].sizeDelta=new Vector2(1000, 209);
        AmazonMoveTransforms[2].anchoredPosition = new Vector2(190, 6);
#endif
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        SetBtnStatus();
        SetPlayer();
        RefreshLogin();

        ShowGuide();
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Process.EndProcess(ProcessType.ShowLoginGuide);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEventCallBack);

        UnityUtility.UnloadAssetAsync(textureAssetHandle); 
        Player_Image.sprite = null;

        base.OnRelease();
	}

    public override void OnReveal()
    {
        RefreshLogin();
        base.OnReveal();
    }

    private void SetBtnEvent()
    {
        //KidButton.SetBtnEvent(OnKidButtonClick);
        Music_Btn.SetBtnEvent(OnMusicButtonClick);
        Audio_Btn.SetBtnEvent(OnAudioButtonClick);
        Shake_Btn.SetBtnEvent(OnShakeButtonClick);
        Tip_Btn.SetBtnEvent(OnTipButtonClick);
        Notification_Btn.SetBtnEvent(OnNotificationButtonClick);
        Close_Btn.SetBtnEvent(() => GameManager.UI.HideUIForm(this));
        Service_Btn.SetBtnEvent(OnServiceButtonClick);
        Policy_Btn.SetBtnEvent(OnPolicyButtonClick);

#if AmazonStore || UNITY_IOS
        // Debug.LogError("UserGeography: ", MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography);
        PrivacyOptionsBtn.gameObject.SetActive(MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography == MaxSdkBase.ConsentFlowUserGeography.Gdpr);
#else
        PrivacyOptionsBtn.gameObject.SetActive(GoogleMobileAds.Ump.Api.ConsentInformation.PrivacyOptionsRequirementStatus
                                               == GoogleMobileAds.Ump.Api.PrivacyOptionsRequirementStatus.Required);
#endif
        PrivacyOptionsBtn.SetBtnEvent(OnPrivacyOptionsButtonClick);
        BG_Btn.SetBtnEvent(() => GameManager.UI.HideUIForm(this));

        Quit_Btn.SetBtnEvent(OnQuitButtonClick);

        Language_Btn.SetBtnEvent(LanguageEvent);
        ChangeMusic_Btn.SetBtnEvent(MusicEvent);
        ChangeHeadPortrait_Btn.SetBtnEvent(HeadPortraitEvent);
        ChangeName_Btn.SetBtnEvent(ChangeNameEvent);
        Email_Btn.SetBtnEvent(EmailEvent);
        Copy_Btn.SetBtnEvent(CopyEvent);
                      
        SaveData_Btn.SetBtnEvent(SaveDataEvent);
        FacebookSignOut_Btn.SetBtnEvent(FacebookSignOutOrSyncDataEvent);
        GoogleSignOut_Btn.SetBtnEvent(GoogleSignOutOrSyncDataEvent);
    }

    private void RefreshLogin()
    {
#if AmazonStore || UNITY_IOS
        SaveData_Btn.gameObject.SetActive(false);
        FacebookSignOut_Btn.gameObject.SetActive(false);
        GoogleSignOut_Btn.gameObject.SetActive(false);
#else
        bool isSignIn = !string.IsNullOrEmpty(GameManager.PlayerData.UserID);
        bool isGoogle = GameManager.PlayerData.LoginSdkName == LoginType.Google;
        SaveData_Btn.gameObject.SetActive(!isSignIn);
        FacebookSignOut_Btn.gameObject.SetActive(isSignIn && !isGoogle);
        GoogleSignOut_Btn.gameObject.SetActive(isSignIn && isGoogle);
#endif
    }

    private void SetBtnStatus()
	{
        Music_Btn.SetStatus(!GameManager.PlayerData.MusicMuted);
        Audio_Btn.SetStatus(!GameManager.PlayerData.AudioMuted);
        Shake_Btn.SetStatus(!GameManager.PlayerData.ShakeMuted);
        Tip_Btn.SetStatus(!GameManager.PlayerData.TurnOffTips);
        //KidButton.SetStatus(GameManager.PlayerData.GetBool(Constant.PlayerData.KidMode));
        Notification_Btn.SetStatus(!GameManager.PlayerData.NotificationForbidden);
    }

    private void ShowGuide()
    {
        if (GameManager.PlayerData.NowLevel < 22)return;
        if(GameManager.PlayerData.LoginSdkName!=LoginType.None)return;
        if(!GameManager.PlayerData.IsShowLoginGuide)return;
        if(GameManager.PlayerData.IsHaveShowSaveDataGuide)return;
        
        GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
        {
            GameManager.PlayerData.IsHaveShowSaveDataGuide = true;
            //记录展示guide
            form.gameObject.SetActive(false);
            var guideMenu = form.GetComponent<CommonGuideMenu>();
            var originParent = SaveData_Btn.transform.parent;
            SaveData_Btn.interactable = true;
            SaveData_Btn.transform.SetParent(form.transform);
            guideMenu.SetText("Setting.Tap here to save your data!");
            guideMenu.tipBox.SetOkButton(false);
            var position = SaveData_Btn.transform.position+new Vector3(0,0.22f,0);
            guideMenu.ShowGuideArrow(position,position+new Vector3(0,0.12f,0),PromptBoxShowDirection.Down);

            guideMenu.tipBox.transform.position = new Vector3(0, position.y + 0.4f, 0);
            guideMenu.OnShow(null, null);
            
            void ClickAction()
            {
                PlayerBehaviorModel.Instance.RecordHelpFirstGuide();
                SaveData_Btn.transform.SetParent(originParent);
                GameManager.UI.HideUIForm(form);
                guideMenu.guideImage.onTargetAreaClick = null;
                SaveData_Btn.onClick.RemoveListener(ClickAction);
            }
            SaveData_Btn.onClick.AddListener(ClickAction);
        });
    }

    private void LanguageEvent()
    {
        GameManager.UI.ShowUIForm("LanguageMenu");
    }

    private void MusicEvent()
    {
        //GameManager.UI.ShowUIForm<ChangeMusicPanel>();
    }

    private void HeadPortraitEvent()
    {
        GameManager.UI.ShowUIForm("HeadPortraitPanel");
        HeadPortraitRedPoint_Obj.gameObject.SetActive(false);
        GameManager.PlayerData.IsShowHeadPortraitRedPoint = false;
    }

    private void ChangeNameEvent()
    {
        PlayerName_Input.ActivateInputField();
    }


    private void EmailEvent()
    {
        string subject = MyEscapeURL("Tile Story! Support Request");
        string appName = "Tile Story!";
        string appVersion = "Game Version:" + Application.version + "\n";
        string devices = "Device info:" + SystemInfo.deviceModel + SystemInfo.deviceType + "\n";
        string OS = "OS info:" + SystemInfo.operatingSystem + "\n";
        string CPU = "CPU info:" + SystemInfo.processorType + "\n";
        string GPU = "GPU info:" + SystemInfo.graphicsDeviceType + "\n";
        string language = "Language info:" + GameManager.Localization.Language + "\n";
        string body = "\n\n\n\nThe following information is valuable for us to analyze and solve your issues.Please keep it.\n ";
        body = MyEscapeURL(body + appName + appVersion + devices + OS + CPU + GPU + language);
        SendEmail(email, subject, body);
    }

    private void CopyEvent()
    {

    }

    private void SaveDataEvent()
    {
        //Login
        GameManager.PlayerData.IsHaveShowSaveDataGuide = true;
        GameManager.UI.ShowUIForm("SignInPanel");
    }

    private void FacebookSignOutOrSyncDataEvent()
    {
        //facebook sign out
        GameManager.UI.ShowUIForm("ConnectAccountPanel");
    }

    private void GoogleSignOutOrSyncDataEvent()
    {
        //google sign out
        GameManager.UI.ShowUIForm("ConnectAccountPanel");
    }

    private string MyEscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    private void SendEmail(string mailAddress, string subject, string content)
    {
        Application.OpenURL("mailto:" + mailAddress + "?subject=" + subject + "&body=" + content);
    }

    private void SetPlayer()
    {
        SetHeadPortrait();

        SetNameInputText();
        
        PlayerName_Input.characterLimit = 12;
        PlayerName_Input.onEndEdit.RemoveAllListeners();
        PlayerName_Input.onEndEdit.AddListener((s)=> 
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (!GameManager.PlayerData.RecordSetPlayerName)
                {
                    GameManager.PlayerData.RecordSetPlayerName = true;
                }
                GameManager.PlayerData.PlayerName = s;
                GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Set_Name, 1);
            }
            else
            {
                PlayerName_Input.text = GameManager.PlayerData.PlayerName;
            }
        });
    }
    
    public static string Reverse( string s )
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    private void SetNameInputText()
    {
        string playerName =GameManager.PlayerData.PlayerName;
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerName_Input.text = playerName;
        }
    }

    private void SetHeadPortrait()
    {
        string headPortraitName = $"HeadPortrait_{GameManager.PlayerData.HeadPortrait}_{GameManager.PlayerData.HeadPortrait}";
        if (Player_Image.sprite != null && Player_Image.sprite.name == headPortraitName) return;
        UnityUtility.UnloadAssetAsync(textureAssetHandle); 
        textureAssetHandle= UnityUtility.LoadAssetAsync<Sprite>(headPortraitName, s =>
        {
            Player_Image.sprite = s;
        });
    }

    private void SetVersion()
    {
        Version_Text.SetText("v " + Application.version);
    }

    private void OnQuitButtonClick()
    {
        GameManager.UI.ShowUIForm("SureQuitPanel");
    }

    private void OnServiceButtonClick()
    {
        Application.OpenURL("http://www.linkdesks.net/TermsOfUse.html");
    }

    private void OnPolicyButtonClick()
    {
        Application.OpenURL("http://www.linkdesks.net/PrivacyPolicy.html");
    }

    private void OnPrivacyOptionsButtonClick()
    {
#if AmazonStore || UNITY_IOS
        var cmpService = MaxSdk.CmpService;
        cmpService.ShowCmpForExistingUser(error =>
        {
            //isGDPRLoading = false;
            if (null == error)
            {
                // The CMP alert was shown successfully.
            }
            else
            {
                //UIManager.instance.ShowTip(LanguageManager.Instance.GetText(LanguageKey.TIP_NET_ERR));
                Debug.LogError("ConsentForm Showing Failed: " + error.Message);
            }
        });
#else
        GoogleMobileAds.Ump.Api.ConsentInformation.Reset();
        Debug.Log("Showing privacy options form.");

        Debug.Log("ConsentStatus: " + GoogleMobileAds.Ump.Api.ConsentInformation.ConsentStatus);
        GoogleMobileAds.Ump.Api.ConsentForm.LoadAndShowConsentFormIfRequired((GoogleMobileAds.Ump.Api.FormError showError) =>
        {
#if AmazonStore || UNITY_IOS

            PrivacyOptionsBtn.gameObject.SetActive(MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography == MaxSdkBase.ConsentFlowUserGeography.Gdpr);
#endif

            PrivacyOptionsBtn.gameObject.SetActive(GoogleMobileAds.Ump.Api.ConsentInformation.PrivacyOptionsRequirementStatus
                                                   == GoogleMobileAds.Ump.Api.PrivacyOptionsRequirementStatus.Required);

            if (showError != null)
            {
                Debug.LogError("ConsentForm Showing Failed: " + showError.Message);
            }
            Debug.Log("ConsentStatus: " + GoogleMobileAds.Ump.Api.ConsentInformation.ConsentStatus);
        });
#endif
    }

    /// <summary>
    /// ���ְ�ť�����
    /// </summary>
    private void OnMusicButtonClick()
    {
        GameManager.PlayerData.MusicMuted = !GameManager.PlayerData.MusicMuted;
        GameManager.Sound.MuteMusic(GameManager.PlayerData.MusicMuted);

        if (!GameManager.PlayerData.MusicMuted)
        {
            GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        }
        else
        {
            GameManager.Sound.StopMusic(0);
        }

        Music_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.MusicMuted, false);
    }

    /// <summary>
    /// ��Ч��ť�����
    /// </summary>
    private void OnAudioButtonClick()
    {
        GameManager.PlayerData.AudioMuted = !GameManager.PlayerData.AudioMuted;
        GameManager.Sound.MuteAudio(GameManager.PlayerData.AudioMuted);
        Audio_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.AudioMuted, false);
    }

    /// <summary>
    /// ��ͯģʽ��ť�����
    /// </summary>
    private void OnKidButtonClick()
    {
        GameManager.PlayerData.KidMode = !GameManager.PlayerData.KidMode;

        GameManager.Ads.SetChildDirectedTreatment(GameManager.PlayerData.KidMode);
    }

    private void OnShakeButtonClick()
    {
        GameManager.PlayerData.ShakeMuted =! GameManager.PlayerData.ShakeMuted;
        Shake_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.ShakeMuted, false);
    }

    private void OnTipButtonClick()
    {
        GameManager.PlayerData.TurnOffTips = !GameManager.PlayerData.TurnOffTips;
        Tip_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.TurnOffTips, false);
    }

    private void OnNotificationButtonClick()
    {
        GameManager.PlayerData.NotificationForbidden = !GameManager.PlayerData.NotificationForbidden;
        if (GameManager.PlayerData.NotificationForbidden)
        {
            GameManager.Notification.Shutdown();
        }
        Notification_Btn.ShowStatusChangeAnim(!GameManager.PlayerData.NotificationForbidden, false);
    }

    private void CommonEventCallBack(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.ChangeHeadPortrait:
                SetHeadPortrait();
                break;
        }
    }
}
