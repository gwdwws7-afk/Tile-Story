using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeleteAccountMenu : PopupMenuForm
{
    private const string Confirm_String = "confirm";

    public DelayButton closeButton;
    public DelayButton okButton;
    public DelayButton continueButton;
    public DelayButton cancelButton;
    public TMP_InputField inputField;
    public TextMeshProUGUILocalize okText;
    public Material greyMaterial;
    public GameObject[] areas;

    private int currentArea;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        closeButton.OnInit(OnClose);
        okButton.OnInit(OnOkButtonClick);
        continueButton.OnInit(OnOkButtonClick);
        cancelButton.OnInit(OnClose);
        inputField.onValueChanged.AddListener(OnEditInputField);

        for (int i = 0; i < areas.Length; i++)
        {
            areas[i].SetActive(i == currentArea);
        }

        if (currentArea == 0)
        {
            continueButton.transform.parent.gameObject.SetActive(true);
            okButton.gameObject.SetActive(false);
        }
        else
        {
            continueButton.transform.parent.gameObject.SetActive(false);
            okButton.gameObject.SetActive(true);
            RefreshOkButtonState();
        }
    }

    public override void OnReset()
    {
        closeButton.OnReset();
        okButton.OnReset();
        continueButton.OnReset();
        cancelButton.OnReset();
        inputField.onValueChanged.RemoveAllListeners();

        inputField.text = null;
        currentArea = 0;

        base.OnReset();
    }

    public override void OnClose()
    {
        if (currentArea >= areas.Length - 1)
        {
            Application.Quit();
        }

        GameManager.UI.HideUIForm(this);
        // GameManager.UI.HideBlackBg();
    }

    private void RefreshOkButtonState()
    {
        if (currentArea == 1 && (inputField.text == null || inputField.text.Trim() != Confirm_String))  
        {
            okButton.interactable = false;
            okButton.body.GetComponent<Image>().material = greyMaterial;
            okText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        }
        else
        {
            okButton.interactable = true;
            okButton.body.GetComponent<Image>().material = null;
            okText.SetMaterialPreset(MaterialPresetName.Btn_Green);
        }
    }

    private void OnOkButtonClick()
    {
        if (currentArea == 1)
        {
            DeleteAccount();
        }

        if (currentArea >= areas.Length - 1) 
        {
            Application.Quit();
            return;
        }

        areas[currentArea].SetActive(false);
        currentArea++;
        areas[currentArea].transform.localScale = Vector3.zero;
        areas[currentArea].SetActive(true);
        areas[currentArea].transform.DOScale(1.03f, 0.2f).onComplete = () =>
        {
            areas[currentArea].transform.DOScale(1f, 0.2f);
        };

        if (currentArea == 1)
        {
            continueButton.transform.parent.gameObject.SetActive(false);
            okButton.gameObject.SetActive(true);
        }

        RefreshOkButtonState();
    }

    private void OnEditInputField(string value)
    {
        RefreshOkButtonState();
    }

    private void DeleteAccount()
    {
        //本地数据清空
        PlayerPrefs.DeleteAll();
    }
}
