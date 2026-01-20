using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections.Generic;
using MySelf.Model;

/// <summary>
/// 预加载Map场景资源的流程
/// </summary>
public sealed class ProcedureMapPreload : ProcedureBase
{
    private bool preloadComplete;
    private int preloadCount;

    public override string ProcedureName => "ProcedureMapPreload";

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
         RecordFirebaseEvent();//打点内存
        
        GameManager.Scene.SetSceneType(SceneType.Map);
        GameManager.Process.UnregisterAll();

        preloadComplete = false;
        preloadCount = 0;
        
        GameManager.Ads.RequestAds();
        GameManager.Task.OnInit();

        if (GameManager.UI.GetUIForm("MapDecorationBGPanel") != null) 
        {
            preloadCount = 1;
            PreloadUIForm("MapTopPanelManager",UIFormType.CenterUI, false, () =>
            {
                preloadCount--;
            });
        }
        else
        {
            preloadCount = 2;
            PreloadUIForm("MapDecorationBGPanel",UIFormType.GeneralUI, true, () =>
            {
                preloadCount--;
                PreloadUIForm("MapTopPanelManager",UIFormType.CenterUI, false, () =>
                {
                    preloadCount--;
                });
            });
        }

        if (GameManager.PlayerData.NowLevel > 10 && (DateTime.Now < HarvestKitchenManager.Instance.EndTime ||
                                                     HarvestKitchenManager.Instance.CheckActivityHasStarted()))  
        {
            if(!HarvestKitchenManager.Instance.CheckHaveAsset())
                HarvestKitchenManager.Instance.DownloadAsset();
        }
        
        if (GameManager.PlayerData.NowLevel > 20)
        {
            //后台下载资源 
            GameManager.Download.DownloadAreaByBackGround();
            GameManager.Download.DownloadDecorationAreaThumbnailByBackGround();

            //后台下载卡牌活动AB包
            int cardActivityID = CardModel.Instance.CardActivityID;
            if (cardActivityID > 0 && !AddressableUtils.IsHaveAsset($"CardSetMainMenu{cardActivityID}")) 
            {
                GameManager.Download.AddDownload($"CardSetMainMenu{cardActivityID}");
            }
            
            //后台下载寻宝资源
            if (!HiddenTemple.HiddenTempleManager.Instance.CheckHaveAsset()) 
            {
                HiddenTemple.HiddenTempleManager.Instance.DownloadAsset();
            }

            //后台下载餐厅活动AB包
            if (!KitchenManager.Instance.CheckHaveAsset())
            {
                KitchenManager.Instance.DownloadAsset();
            }

            //后台下载火山副本AB包
            if (!AddressableUtils.IsHaveAssetSync("GlacierQuestStartMenu", out long GlacierSize) && !GameManager.Download.IsDownloading("GlacierQuestStartMenu"))
            {
                GameManager.Download.AddDownload("GlacierQuestStartMenu");
            }   
        }

        base.OnEnter(fsm);
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        if (!preloadComplete && preloadCount <= 0)
        {
            preloadComplete = true;

            ShowMapUI(() =>
            {
                ChangeState<ProcedureMap>(fsm);
            });
        }
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        base.OnLeave(fsm, isShutdown);
    }

    private void ShowMapUI(Action callback)
    {
        GameManager.UI.HideAllUIForm(new string[] { "MapDecorationBGPanel", "MapTopPanelManager", "MapMainBGPanel" });

        var bgPanel = GameManager.UI.GetUIForm("MapDecorationBGPanel");
        if (bgPanel != null) 
        {
            bgPanel.gameObject.SetActive(true);
        }
        else
        {
            GameManager.UI.ShowUIForm("MapDecorationBGPanel", UIFormType.GeneralUI);
        }

        GameManager.UI.ShowUIForm("MapTopPanelManager",UIFormType.CenterUI, panel =>
         {
             callback.Invoke();
         }, () =>
         {
             callback.Invoke();
         });
        GameManager.UI.ShowUIForm("MapMainBGPanel",UIFormType.BgUI, f =>
         {
             f.OnHide();
         });
    }

    /// <summary>
    /// 创建UI界面加入到Group中
    /// </summary>
    private void PreloadUIForm(string uiName,UIFormType uiFormType, bool isInit, Action callback)
    {
        GameManager.UI.CreateUIForm(uiName,uiFormType, uiForm =>
         {
             var group = GameManager.UI.GetUIGroup(uiForm.UIFormType);
             uiForm.SetGroup(group);
             group.Refresh();
             if (isInit)
             {
                 uiForm.OnInit(group, () =>
                 {
                     callback?.Invoke();
                 });
             }
             else
             {
                 uiForm.gameObject.SetActive(false);
                 callback?.Invoke();
             }
         }, () =>
          {
              callback?.Invoke();
          });
    }
    
     private void RecordFirebaseEvent()
        {
            int size =SystemInfoManager.GetSystemMemory();
            if(size>0)
                GameManager.Firebase.RecordMessageByEvent("DeviceSystemMemory",new Firebase.Analytics.Parameter("Size",size));
        }
}
