using System;
using UnityEngine;
using UnityEngine.UI;

public class PersonRankName : PopupMenuForm
{
    public DelayButton okButton;
    public Text userName;
    public Text preholder;
    [SerializeField]
    private InputField PlayerName_Input;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        var preName = userData as string;
        // SetNameInputText(preName);
        var text = GameManager.Localization.GetString("Setting.Enter your name");
        GameManager.PlayerData.RecordSetPlayerName = true;
        preholder.text = text;
        okButton.onClick.AddListener(OnOkButtonClick);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        okButton.OnReset();
        base.OnReset();
    }

    private void OnOkButtonClick()
    {
        if (userName.text != null && userName.text.Length > 2)
        {
            var personRankName = userName.text;
            GameManager.Task.PersonRankManager.LocalData.Name = personRankName;
            GameManager.PlayerData.PlayerName = personRankName;
            GameManager.PlayerData.RecordSetPlayerName = true;
            GameManager.UI.GetUIForm("PersonRankMenu").GetComponent<PersonRankMenu>().OnNameSet();
            GameManager.UI.ShowWeakHint("PersonRank.Edited successfully");
            GameManager.UI.HideUIForm(this);
        }
        else if (string.IsNullOrEmpty(userName.text))
        {
            GameManager.UI.ShowWeakHint("PersonRank.The name field is required");
        }else if (userName.text.Length > 0)
        {
            GameManager.UI.ShowWeakHint("PersonRank.Type at least three characters.");
        }
    }

    private void SetNameInputText(string preName)
    {
        string playerName = string.IsNullOrEmpty(preName) ? string.Empty : preName;
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerName_Input.text = playerName;
        }
        else
        {
            var nameText = GameManager.Task.PersonRankManager.LocalData.Name;
            PlayerName_Input.SetTextWithoutNotify(nameText);
        }

        if (GameManager.PlayerData.RecordSetPlayerName)
        {
            PlayerName_Input.characterLimit = 15;
        }
        else
        {
            PlayerName_Input.characterLimit = 0;
        }
    }

    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
