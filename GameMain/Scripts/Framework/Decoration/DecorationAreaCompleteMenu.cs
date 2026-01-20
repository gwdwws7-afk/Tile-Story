using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MySelf.Model;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using Spine.Unity;
using TMPro;

public class DecorationAreaCompleteMenu : PopupMenuForm
{
    [SerializeField]
    private DelayButton bgButton;
    // [SerializeField]
    // private Transform welldoneRoot;
    [SerializeField]
    private GameObject TapToContinueRoot;
    [SerializeField]
    private Image areaCompleteRoundImage;

    [SerializeField]
    private SkeletonGraphic[] spineGraphics;
    [SerializeField]
    private TextMeshProUGUI wellDoneText;
    [SerializeField]
    private GameObject completeRoundImageBackEffect;
    [SerializeField]
    private GameObject fireCrackEffect;
    [SerializeField]
    private GameObject fireCrackEffect2;

    [SerializeField]
    private GameObject StarRoot;
    [SerializeField]
    private Button[] Star_Btns;
    [SerializeField]
    private GameObject[] Stars;
    [SerializeField]
    private TextMeshProUGUILocalize Star_Text;

    private int starNum = 0;
    private AsyncOperationHandle loadHandle;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        try
        {
            bgButton.SetBtnEvent(OnCloseBtnClicked);

            bgButton.enabled = true;
            TapToContinueRoot.gameObject.SetActive(false);
            StarRoot.SetActive(false);

            int recentAreadID = DecorationModel.Instance.GetDecorationOperatingAreaID();
            loadHandle = UnityUtility.LoadAssetAsync<Sprite>($"Area{recentAreadID}_Round", sp =>
            {
                if (sp != null)
                    areaCompleteRoundImage.sprite = sp;

            });

            for (int i = 0; i < Star_Btns.Length; i++)
            {
                int starIndex = i + 1;
                Star_Btns[i].SetBtnEvent(() =>
                {
                    OnStarButtonClick(starIndex);
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogError("DecorationAreaCompleteMenu Init error:" + e.Message);
        }
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void OnEnable()
    {
        GameManager.Sound.PlayAudio(SoundType.SFX_DecorationAreaFinished.ToString());

        for (int i = 0; i < spineGraphics.Length; ++i)
        {
            spineGraphics[i].SetToFirst();
            spineGraphics[i].AnimationState.SetAnimation(0, "active", false);
        }
        areaCompleteRoundImage.color = new Color(1, 1, 1, 0);
        areaCompleteRoundImage.DOFade(1, 0.3f);
        wellDoneText.color = new Color(1, 1, 1, 0);
        wellDoneText.DOFade(1, 0.3f);

        completeRoundImageBackEffect.SetActive(false);
        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            completeRoundImageBackEffect.SetActive(true);
            TapToContinueRoot.gameObject.SetActive(true);
            StarRoot.SetActive(true);
        });

        fireCrackEffect.SetActive(false);
        fireCrackEffect2.SetActive(false);
        GameManager.Task.AddDelayTriggerTask(0.3f, () =>
        {
            fireCrackEffect.SetActive(true);
            fireCrackEffect2.SetActive(true);
        });

        GameManager.Task.AddDelayTriggerTask(3.0f, () =>
        {
            fireCrackEffect.SetActive(false);
            fireCrackEffect2.SetActive(false);
        });

        GameManager.Task.AddDelayTriggerTask(1.5f, () =>
        {
            for (int i = 0; i < spineGraphics.Length; ++i)
            {
                if(spineGraphics[i].IsSpineAnimNameExist("active2"))
                {
                    //spineGraphics[i].SetToFirst();
                    spineGraphics[i].AnimationState.SetAnimation(0, "active2", false);
                }
            }
        });
    }

    public override void OnRelease()
    {
        bgButton.OnReset();
        UnityUtility.UnloadAssetAsync(loadHandle);
        loadHandle = default;

        base.OnRelease();
    }

    private void OnCloseBtnClicked()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.UI.HideUIForm("DecorationOperationPanel");
        DecorationModel.Instance.SetTargetAreaGetReward(DecorationModel.Instance.Data.DecorationAreaID, true);
        DecorationModel.Instance.NeedToShowDecorationViewAnim = true;
        GameManager.UI.ShowUIForm("MapTopPanelManager",(u1) =>
        {
            DecorationModel.Instance.CompleteNowArea(starNum);
        });
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
        ShowStarText(starNum);
    }

    private void ShowStarText(int startNum)
    {
        switch (startNum)
        {
            case 1:
                Star_Text.SetTerm("Settings.I hate it");
                break;
            case 2:
                Star_Text.SetTerm("Settings.I don t like it");
                break;
            case 3:
                Star_Text.SetTerm("Settings.It's ok");
                break;
            case 4:
                Star_Text.SetTerm("Settings.I like it!");
                break;
            case 5:
                Star_Text.SetTerm("Settings.I love it!");
                break;
            default:
                Star_Text.SetTerm("Rate.THE BEST WE CAN GET");
                break;
        }
    }
}
