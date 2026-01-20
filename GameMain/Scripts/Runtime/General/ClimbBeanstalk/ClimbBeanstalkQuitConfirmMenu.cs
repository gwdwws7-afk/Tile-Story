using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClimbBeanstalkQuitConfirmMenu : PopupMenuForm
{
    [SerializeField]
    private DelayButton closeBtn, quitBtn, continueBtn;
    private Action onQuitBtnClickedCallback;

    [SerializeField]
    public TextMeshProUGUI recentWinStreakText, lastWinStreakText;

    [SerializeField] private Image BgImage;
    [SerializeField] private Sprite[] BgSprites;
    [SerializeField]
    private TextMeshProUGUILocalize Title_Text;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        quitBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
            onQuitBtnClickedCallback?.Invoke();
            onQuitBtnClickedCallback = null;
        });
        continueBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });

        int currentWinStreak = ClimbBeanstalkManager.Instance.CurrentWinStreak;
        recentWinStreakText.text = currentWinStreak.ToString();
        lastWinStreakText.text = (currentWinStreak - 1).ToString();

        SetBgImage();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public void SetOnQuitBtnClcked(Action inputCallBack)
    {
        onQuitBtnClickedCallback = inputCallBack;
    }

    private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.LevelNormal,
        MaterialPresetName.LevelHard,
        MaterialPresetName.LevelSurpHard,
    };

    private void SetBgImage()
    {
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            Title_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);
        });
        BgImage.sprite = BgSprites[hardIndex];
    }
}
