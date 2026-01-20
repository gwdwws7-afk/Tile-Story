using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Firebase.Analytics;
using JetBrains.Annotations;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class TotalGamePausePanel : PopupMenuForm
{
   [SerializeField] private DelayButton CloseBtn,QuitBtn,ContinuBtn;
   [SerializeField] private Image BgImage;
   [SerializeField]
   private TextMeshProUGUILocalize Title_Text;
   [SerializeField] private Sprite[] BgSprites;
   [SerializeField] private Transform PkSorceParent;
   
   [SerializeField] private Transform GameFailRoot;
   
   private List<BaseGameFailPanel> AllGameFailPanels;
   BaseGameFailPanel CurGameFailPanel;
   
   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      //准备失败场景数据
      PrepareFailPanelData();
      
      SetBtnEvent();
      SetBgImage();
      
      //展示失败页面内容
      ShowFailPanel();
      
      //pk
      // ShowPkSorce();
      base.OnInit(uiGroup, completeAction, userData);
   }

   public override void OnRelease()
   {
      AllGameFailPanels.Clear();
      CurGameFailPanel = null;
      base.OnRelease();
   }

   private void ShowPkSorce()
   {
      bool isShowPkSorce = PkGameModel.Instance.IsActivityOpen;
      
      if (isShowPkSorce)
      {
         Addressables.InstantiateAsync("PkSorce", PkSorceParent).Completed+= (u) =>
         {
            PkSorce pk=u.Result.GetComponent<PkSorce>();
            pk.gameObject.SetActive(true);
            pk.Init();
         };
      }
   }

   private void PrepareFailPanelData()
   {
      //获取所有的失败界面
      AllGameFailPanels = GameFailRoot.GetComponentsInChildren<BaseGameFailPanel>(true).ToList();
      //剔除不展示的
      AllGameFailPanels = AllGameFailPanels.Where(obj => obj.IsShowFailPanel).ToList();
      //判断是否有独立展示的fail界面【独立展示 意思是如果有其他的fail界面就不展示了】
      var gameFailPanels = AllGameFailPanels.Where(obj => obj.IsSpecialPanel).ToList();
      if (gameFailPanels.Count > 0) AllGameFailPanels = gameFailPanels.ToList();
      // 然后按照展示优先级排序
      AllGameFailPanels = AllGameFailPanels.OrderBy(obj => obj.PriorityType).ToList();
   }

   private void SetBtnEvent()
   {
      CloseBtn.SetBtnEvent(() =>
      {
         GameManager.UI.HideUIForm(this);
      });
      QuitBtn.SetBtnEvent(() =>
      {
         CloseBtnEvent();
         //GameManager.UI.HideUIForm(this);
      });
      ContinuBtn.SetBtnEvent(() =>
      {
         GameManager.UI.HideUIForm(this);
      });
   }
   
   private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
   {
      MaterialPresetName.LevelNormal,
      MaterialPresetName.LevelHard,
      MaterialPresetName.LevelSurpHard,
   };
    
   private void SetBgImage()
   {
      var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
      GameManager.Task.AddDelayTriggerTask(0.1f, () =>
      {
         Title_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);
      });
      BgImage.sprite = BgSprites[hardIndex];
   }
   
   /// <summary>
   /// 点击close事件
   /// </summary>
   void CloseBtnEvent()
   {
      Action<bool> action = b =>
      {
         CloseBtn.enabled = b;
         ContinuBtn.enabled = b;
         QuitBtn.enabled = b;
      };
      if (CurGameFailPanel != null)
      {
         action(false);
         CurGameFailPanel.CloseFailPanel(() => 
         {
            action(true);
            CurGameFailPanel = null;
            CloseBtnEvent();
         });
         return;
      }

      //点击时发现没有剩余界面时关闭当前界面
      if (AllGameFailPanels.Count > 0)
      {
         //继续展示
         ShowFailPanel();
      }
      else
      {
         ShowGameQuitPanel();
      }
   }
   
   
   /// <summary>
   /// 展示失败界面
   /// </summary>
   private void ShowFailPanel()
   {
      //关闭当前的失败页面
      if(CurGameFailPanel!=null)CurGameFailPanel.gameObject.SetActive(false);
      //设置当前需要展示的panel
      CurGameFailPanel = AllGameFailPanels[0];
      AllGameFailPanels.RemoveAt(0);
      //展示
      CurGameFailPanel.gameObject.SetActive(true);
      GameFailRoot.SetChildActive(false,CurGameFailPanel.gameObject);
      CurGameFailPanel.ShowFailPanel(null);
      //
      Title_Text.SetTerm("Settings.Are You Sure?");
   }
   
   private void ShowGameQuitPanel()
   {
      // if (GameManager.Task.LavaDungeonTaskManager.ActivityState == LavaDungeonState.Open)
      // {
      //    PkGameModel.Instance.Lose();
      //    PersonRankModel.Instance.GameLose();
      //    GameManager.Task.ClimbBeanstalkTaskManager.OnGameLose();
      //    GameManager.Task.LavaDungeonTaskManager.OnGameFail();
      //    
      //    GameManager.UI.HideUIForm(this);
      //    GameManager.UI.ShowUIForm<LavaDungeonMenu>(form =>
      //    {
      //       Log.Info("LavaDungeon：设置关闭界面的回调");
      //       LavaDungeonMenu menu = form as LavaDungeonMenu;
      //       menu.SetCloseEvent(() =>
      //       {
      //          GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Home, 
      //             new Parameter("Level", GameManager.PlayerData.NowLevel));
      //          GameManager.Ads.ShowInterstitialAd(ProcedureUtil.ProcedureGameToMap);
      //       });
      //    });
      //    return;
      // }
      //
      // GameFailPanel.RecordSourceIndex = 1;
      // GameManager.UI.ShowUIForm<GameFailPanel>(u =>
      // {
      //    GameManager.UI.HideUIForm(this);
      // });
   }
}
