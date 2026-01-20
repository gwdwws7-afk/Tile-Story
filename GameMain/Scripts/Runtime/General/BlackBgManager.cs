using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class BlackBgManager : MonoBehaviour
{
    public Button clickButton;

    private bool m_IsShowing;
    private bool m_IsHiding;
    public bool isLocked;

    public bool IsShowing
    {
        get
        {
            return m_IsShowing;
        }
    }

    public void OnShow(float fadeTime = 0f, bool interactable = true, float bgAlpha = 0.9f)
    {
        if (isLocked)
        {
            return;
        }

        clickButton.interactable = interactable;
        Image image = GetComponent<Image>();

        if (m_IsHiding)
        {
            image.DOKill();
            m_IsHiding = false;
            image.color = new Color(0, 0, 0, bgAlpha);
        }

        if (gameObject.activeSelf)
        {
            if (m_IsShowing)
            {
                image.DOKill();
                m_IsShowing = false;
            }
            image.color = new Color(0, 0, 0, bgAlpha);
            return;
        }

        if (fadeTime <= 0) 
        {
            image.color = new Color(0, 0, 0, bgAlpha);
            gameObject.SetActive(true);
        }
        else
        {
            m_IsShowing = true;
            image.color = new Color(0, 0, 0, 0.7f);
            gameObject.SetActive(true);
            image.DOFade(bgAlpha, fadeTime).onComplete = () =>
            {
                m_IsShowing = false;
            };
        }
    }

    public void OnHide(float fadeTime= 0)
    {
        if (isLocked)
        {
            return;
        }

        clickButton.interactable = false;

        if (m_IsShowing)
        {
            GetComponent<Image>().DOKill();
            m_IsShowing = false;
        }

        if (fadeTime <= 0) 
        {
            gameObject.SetActive(false);
        }
        else
        {
            m_IsHiding = true;
            var image = GetComponent<Image>();
            image.DOKill();
            image.DOFade(0f, fadeTime).onComplete = () =>
            {
                gameObject.SetActive(false);
                image.color = new Color(0, 0, 0, 0.9f);
                m_IsHiding = false;
            };
        }
    }
}
