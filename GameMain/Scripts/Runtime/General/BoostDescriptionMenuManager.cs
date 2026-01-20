using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boost道具描述界面
/// </summary>
public sealed class BoostDescriptionMenuManager : PopupMenuForm
{
    public Button closeButton;
    public ElementDescribeArea[] describeObjects;


    private int[] levels = { 7, 7, 11 };
    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        closeButton.onClick.AddListener(OnClose);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnReset()
    {
        closeButton.onClick.RemoveAllListeners();

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        StartCoroutine(RefreshTextMeshPro());
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
    }

    public override bool CheckInitComplete()
    {
        return true;
    }

    IEnumerator RefreshTextMeshPro()
    {
        yield return new WaitForEndOfFrame();
        int nowLevel = GameManager.PlayerData.NowLevel;
        //Debug.Log("nowLevel: "+nowLevel);
        for (int i = 0; i < levels.Length; i++)
        {
            int unlockLevel = levels[i];
            if (nowLevel >= unlockLevel)
            {
                describeObjects[i].SetLockState(false);
            }
            else
            {
                describeObjects[i].SetLockState(true);
            }
            describeObjects[i].SetTextPosition();
            describeObjects[i].SetLevelText(unlockLevel);
        }
    }
}
