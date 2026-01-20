using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SplashLoadingMenuManager : UIForm
{
    //[SerializeField] private Texture[] LogoTextures;
    [SerializeField] private RawImage Logo_Image;
    public Transform logo;
    public TextMeshProUGUILocalize loadingText;
    public CanvasGroup loadingCanvas;
    [SerializeField] private GameObject[] Content_Objs;

    private bool logoStartShowAnim;
    private bool logoShowAnimComplete;
    private bool logoHideAnimComplete;
    private bool isInit;
    private float waitTime = 0.5f;
    private float targetValue;
    private float curValue;

    public bool LogoShowAnimComplete { get => logoShowAnimComplete; }

    public bool LogoHideAnimComplete { get => logoHideAnimComplete; }

    private bool isLanguageInit = false;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        if (isInit)
        {
            return;
        }
        isInit = true;

        curValue = 0;

        ShowContentTexts();
        SetLogoSize();
        //Logo_Image.texture = GetLogoTextByLanguage();
        //Logo_Image.GetComponent<RectTransform>().sizeDelta = GetLogoSize();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void ShowContentTexts()
    {
        if (!isLanguageInit)
        {
            bool isInit = GameManager.Localization.CheckInitComplete();

            if (isInit)
            {
                isLanguageInit = true;
            }

            foreach (var obj in Content_Objs)
            {
                obj.gameObject.SetActive(isLanguageInit);
            }
        }
    }
    
    private void SetLogoSize()
    {
        float ratio = 1f;
        //长屏幕
        if (Screen.height * 1080 > 1920 * Screen.width)
            ratio = (Screen.height * 1080f / Screen.width / 1920f);
        Logo_Image.transform.parent.localScale =Vector3.one*1f / ratio;
    }

    public override void OnRelease()
    {
        logo.DOKill();
        loadingCanvas.DOKill();

        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (!isLanguageInit)
        {
            if(Time.frameCount%5==0) ShowContentTexts();
            return;
        }

        if (logoStartShowAnim)
        {
            if (waitTime > 0)
            {
                waitTime -= elapseSeconds;
            }
            else
            {
                logoShowAnimComplete = true;

                if (targetValue > curValue)
                {
                    float orignalValue = curValue;
                    float lerpValue = Mathf.Lerp(curValue, targetValue, elapseSeconds * 5f);
                    float delta = lerpValue - orignalValue;

                    if (delta < 0.005f)
                        curValue = orignalValue + 0.005f;
                    else
                        curValue = lerpValue;

                    if (curValue > targetValue)
                        curValue = targetValue;

                    int value = (int)(curValue * 100);
                    SetLoadingText(value.ToString());
                }
            }
        }
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);
        logoStartShowAnim = true;
        logo.DOScale(1.15f, 0.28f).onComplete = () =>
        {
            logo.DOScale(0.92f, 0.18f).SetEase(Ease.InQuad).onComplete = () =>
            {
                logo.DOScale(1f, 0.14f);
            };
        };

        loadingCanvas.DOFade(1, 0.2f);
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        int value = 100;
        SetLoadingText(value.ToString());

        logo.DOMoveY(1.55f, 0.4f).onComplete = () =>
        {
            logoHideAnimComplete = true;
            gameObject.SetActive(false);
            base.OnHide(hideSuccessAction, userData);
        };

        loadingCanvas.DOFade(0, 0.2f);
        gameObject.SetActive(false);
    }

    public override bool CheckInitComplete()
    {
        return true;
    }

    public void SetLoadingText(string content)
    {
        loadingText.SetParameterValue("Process", content);
    }

    public void SetTargetValue(float value)
    {
        if (value > targetValue)
        {
            targetValue = value;
            if (targetValue > 1)
            {
                targetValue = 1;
            }
        }
        else
        {
            Log.Error("invalid target value");
        }
    }

    public bool CheckSliderAnimComplete()
    {
        return curValue == targetValue;
    }

    //private Texture GetLogoTextByLanguage()
    //{
    //    switch (GameManager.Localization.Language)
    //    {
    //        case Language.Japanese:
    //            return LogoTextures[2];
    //        case Language.German:
    //            return LogoTextures[1];
    //        case Language.Korean:
    //            return LogoTextures[3];
    //        default: return LogoTextures[0];
    //    }
    //}
    
    //private Vector2 GetLogoSize()
    //{
    //    switch (GameManager.Localization.Language)
    //    {
    //        case Language.Japanese:
    //            return new Vector2(1003,368);
    //        case Language.German:
    //            return new Vector2(876,420);
    //        case Language.Korean:
    //            return new Vector2(828,412);
    //        default:
    //            return new Vector2(842,411);
    //    }
    //}
}
