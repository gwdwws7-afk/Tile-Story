using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFirstTryPanel : PopupMenuForm
{
    [SerializeField]
    private TextMeshProUGUILocalize TitleText;
    [SerializeField]
    private GameObject HardStar, SuperHardStar, HardBar, SuperHardBar;
    [SerializeField]
    private Button BgButton;

    private float waitTime;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        BgButton.SetBtnEvent(OnBgButtonClick);

        int level = GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel);
        int hardIndex = DTLevelUtil.GetLevelHard(level);
        HardStar.SetActive(hardIndex == 1);
        SuperHardStar.SetActive(hardIndex == 2);
        HardBar.SetActive(hardIndex == 1);
        SuperHardBar.SetActive(hardIndex == 2);
        waitTime = 0;

        if (hardIndex == 1)
        {
            TitleText.SetTerm("FirstTry.Hard Level");
            TitleText.SetMaterialPreset(MaterialPresetName.Shop_Purple);
        }
        else
        {
            TitleText.SetTerm("FirstTry.Super Hard Level");
            TitleText.SetMaterialPreset(MaterialPresetName.Shop_Red);
        }

        GameManager.Sound.PlayAudio(SoundType.SFX_Get_ingameprop.ToString());
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (waitTime < 0.5f)
            waitTime += elapseSeconds;
    }

    private void OnBgButtonClick()
    {
        if (waitTime < 0.5f)
            return;

        GameManager.UI.HideUIForm(this);
    }
}
