using System;
using TMPro;
using UnityEngine;

public class SureDeleteAccountPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn,Ok_Btn;
    [SerializeField] private GameObject Ok_Gray_Obj;
    [SerializeField] private TMP_InputField Claim_InputField;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetInputField();
        BtnEvent();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void SetInputField()
    {
        Claim_InputField.text = null;
        Claim_InputField.onValueChanged.RemoveAllListeners();
        Claim_InputField.onValueChanged.AddListener((s) =>
        {
            if (s.ToLower() == "confirm")
            {
                Ok_Gray_Obj.gameObject.SetActive(false);
                Ok_Btn.gameObject.SetActive(true);
            }
            else
            {
                Ok_Gray_Obj.gameObject.SetActive(true);
                Ok_Btn.gameObject.SetActive(false);
            }
        });
    }

    private void BtnEvent()
    {
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        Ok_Btn.SetBtnEvent(() =>
        {
            //ok
            GameManager.UI.ShowUIForm("DeleteAccountContinuePanel",(u) =>
            {
                GameManager.UI.HideUIForm(this);
            });
        });
    }
}
