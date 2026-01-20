using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementDescribeMenu : PopupMenuForm
{
    public Button closeButton;
    public ElementDescribeArea[] describeObjects;
    private int[] levels = { 4, 14, 20, 31, 45, 61, 76, 101, 126, 151, 176, 201, 251, 301, 351, 401, 451, 501, 551, 601, 701, 801, 901, 1001, 1151 };
    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        closeButton.onClick.AddListener(OnClose);
        Log.Info("NowLevel" + GameManager.PlayerData.NowLevel);
        //CheckLevels();
        //var mapBottomPanelManager = GameManager.UI.GetUIGroup("BottomUI").GetUIForm("MapBottomPanel") as MapBottomPanelManager;
        
        //mapBottomPanelManager.levelButton.GetComponent<Canvas>().sortingOrder = 22;
        //mapBottomPanelManager.levelButton.onClick.AddListener(OnCloseButtonClick);
        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        StartCoroutine(RefreshTextMeshPro());
    }

    public override void OnReset()
    {
        closeButton.onClick.RemoveAllListeners();
        //var mapBottomPanelManager = GameManager.UI.GetUIGroup("BottomUI").GetUIForm("MapBottomPanel") as MapBottomPanelManager;
        //mapBottomPanelManager.levelButton.GetComponent<Canvas>().sortingOrder = 11;
        //mapBottomPanelManager.levelButton.onClick.RemoveListener(OnCloseButtonClick);
        base.OnReset();
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }

    public void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
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
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < levels.Length; i++)
        {
            describeObjects[i].SetTextPosition();
        }
    }

    public void ChangeTextFont()
    {
        StartCoroutine(RefreshTextMeshPro());
    }
}
