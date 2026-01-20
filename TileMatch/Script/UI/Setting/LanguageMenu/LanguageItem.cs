using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageItem : MonoBehaviour
{
    public Language language;
    public GameObject line;

    [SerializeField]
    private Toggle Toggle;

    public void OnInit()
    {
        bool isCurLanguage = GameManager.PlayerData.Language== language.ToString();
        Toggle.isOn = isCurLanguage;

        Toggle.SetBtnEvent((b)=> 
        {
            if (b) OnButtonClick();
        });
    }

    public void OnButtonClick()
    {
        if (GameManager.Localization.Language != language)
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
            GameManager.Localization.Language = language;
            GameManager.PlayerData.Language = language.ToString();
        }
    }
}
