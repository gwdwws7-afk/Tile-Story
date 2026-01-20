using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAccountDataPanel : PopupMenuForm
{
    [SerializeField] private UICommonController[] UICommonControllers;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
    }

    public void SetControlls(Dictionary<string,string> serviceData,Dictionary<string,string> localData,Action useServiceData,Action useLocalData)
    {
        //location data
        SetUICommonController(0,localData["Level"],localData["Coin"],localData["Star"],DateTime.Now.ToString(), useLocalData);
        //service data
        //SetUICommonController(1,serviceData["Level"],serviceData["Coin"],serviceData["Star"],GameManager.PlayerData.GetLastSaveDataTime,useServiceData);
        SetUICommonController(1, serviceData["Level"], serviceData["Coin"], serviceData["Star"], serviceData["LastUploadTime"], useServiceData);
    }

    private void SetUICommonController(int index,string levelNum,string coinNum,string starNum,string time,Action clickAction)
    {
        //btn
        UICommonControllers[index].Btns[0].SetBtnEvent(() =>
        {
            clickAction?.Invoke();
            GameManager.UI.HideUIForm(this);
        });
        //level num
        UICommonControllers[index].TextMeshProUGUIs[0].text = $"{levelNum}";
        //coin num
        UICommonControllers[index].TextMeshProUGUIs[1].text = $"{coinNum}";
        //star num
        UICommonControllers[index].TextMeshProUGUIs[2].text = $"{starNum}";
        //time
        UICommonControllers[index].TextMeshProUGUIs[3].text = $"{time}";
    }
}
