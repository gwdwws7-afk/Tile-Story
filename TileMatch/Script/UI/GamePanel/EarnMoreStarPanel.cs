using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EarnMoreStarPanel : PopupMenuForm
{
    [SerializeField]
    private DelayButton Close_Btn;
    [SerializeField]
    private DelayButton Continue_Btn;
    [SerializeField]
    private TextMeshProUGUILocalize earnMoreStarInfoText;

    [SerializeField]
    private GameObject[] earnMoreStarVerGOs;
    [SerializeField]
    private GameObject[] notEnoughStarVerGOs;
    [SerializeField]
    private Image[] needToReleaseImages;

    private GameObject guideFingerObject;
    private int lackNumCache;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetBtnEvent();

        lackNumCache = (int)userData;
        if (lackNumCache > 0)
        {
            SetToNotEnoughStarVer();
            earnMoreStarInfoText.SetParameterValue("value", lackNumCache.ToString());
        }
        else
        {
            SetToEarnMoreStarVer();
        }


        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        base.OnShowInit(showInitSuccessAction, userData);

        if (lackNumCache > 0)
        {
            if (!PlayerBehaviorModel.Instance.HasShownEarnMoreStarGuide())
            {
                if (guideFingerObject == null)
                {
                    UnityUtility.InstantiateAsync("SimpleGuideFinger", Continue_Btn.transform, obj =>
                    {
                        guideFingerObject = obj;
                        obj.transform.localPosition = new Vector3(25.0f, -50.0f, 0);
                        SkeletonGraphic sg = guideFingerObject.GetComponentInChildren<SkeletonGraphic>();
                        sg.AnimationState.SetAnimation(0, "03", true);
                    });
                }
            }
        }
    }

    public override void OnRelease()
    {
        Close_Btn.OnReset();
        Continue_Btn.OnReset();

        if (guideFingerObject != null)
        {
            Destroy(guideFingerObject);
            guideFingerObject = null;
        }
    }

    private void OnDestroy()
    {
        foreach (var image in needToReleaseImages)
        {
            image.sprite = null;
        }
    }

    private void SetBtnEvent()
    {
        Close_Btn.SetBtnEvent(OnCloseBtnClicked);
        Continue_Btn.SetBtnEvent(OnContinueBtnClicked);
    }

    private void SetToEarnMoreStarVer()
    {
        for(int i = 0; i < earnMoreStarVerGOs.Length; ++i)
        {
            earnMoreStarVerGOs[i].SetActive(true);
        }
        for (int i = 0; i < notEnoughStarVerGOs.Length; ++i)
        {
            notEnoughStarVerGOs[i].SetActive(false);
        }
    }

    private void SetToNotEnoughStarVer()
    {
        for (int i = 0; i < earnMoreStarVerGOs.Length; ++i)
        {
            earnMoreStarVerGOs[i].SetActive(false);
        }
        for (int i = 0; i < notEnoughStarVerGOs.Length; ++i)
        {
            notEnoughStarVerGOs[i].SetActive(true);
        }
    }


    private void OnCloseBtnClicked()
    {
        GameManager.UI.HideUIForm(this);
    }

    private void OnContinueBtnClicked()
    {
        if (!PlayerBehaviorModel.Instance.HasShownEarnMoreStarGuide())
            PlayerBehaviorModel.Instance.RecordEarnMoreStarGuide();

        GameManager.UI.HideUIForm(this);

        //ProcedureUtil.ProcedureMapToGame();
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockAddOneStepBoostLevel)
        {
            ProcedureUtil.ProcedureMapToGame();
        }
        else
        {
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
        }
    }
}
