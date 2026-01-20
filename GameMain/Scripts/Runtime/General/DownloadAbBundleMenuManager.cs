using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class DownloadAbBundleMenuManager : UIForm
{
    [SerializeField] private DelayButton DownloadAllABBtn,CloseBtn;
    [SerializeField] private Transform LoadAllABParent;
    
    private List<string> abList = new List<string>();
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GetAllABBundleData();
        ShowAllAbBtn();
        
        CloseBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        DownloadAllABBtn.SetBtnEvent(() =>
        {
            foreach (var ab in abList)
            {
                DownloadAb(ab);
            }
        });
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        abList.Clear();
        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (Time.frameCount % 100 == 0)
        {
            ShowAllAbBtn();
        }

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }
    
    private void GetAllABBundleData()
    {
        abList = LoadAllAbNames();
    }
    
    private void ShowAllAbBtn()
    {
        UnityUtility.FillGameObjectWithFirstChild<DelayButton>(LoadAllABParent.gameObject,abList.Count, (index, comp) =>
        {
            string abName = abList[index];

            var canvasGroup = comp.GetComponent<CanvasGroup>();
            var text = comp.GetComponentInChildren<TextMeshProUGUI>();
            comp.SetBtnEvent(() =>
            {
                //点击事件
                DownloadAb(abList[index]);
            });
            comp.name = abName;
            
            IsHaveAb(abName, (b) =>
            {
                canvasGroup.alpha = b ? 0.5f : (IsDownloadIng(abName) ? 0.7f : 1f);
                comp.enabled = !(b || IsDownloadIng(abName));
                text.text = abName;
            });
        });
    }

    private List<string> LoadAllAbNames()
    {
        try
        {
            return JsonConvert.DeserializeObject<HashSet<string>>(Resources.Load<TextAsset>("addressableBundles").text).ToList();
        }
        catch (Exception e)
        {
            return new List<string>();
        }
    }

    private void DownloadAb(string abName)
    {
        if(!IsDownloadIng(abName))
            GameManager.Download.AddDownload(abName);
    }

    private bool IsDownloadIng(string abName)
    {
        return GameManager.Download.IsDownloading(abName);
    }

    private void IsHaveAb(string abName,Action<bool> action)
    {
        AddressableUtils.IsHaveAssetAsync(abName,action);
        // Addressables.LoadResourceLocationsAsync(abName).Completed += (a) =>
        // {
        //     action?.Invoke(a.Result!=null && a.Result.Count>0);
        // };
    }
}
