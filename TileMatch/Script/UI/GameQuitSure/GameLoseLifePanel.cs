using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLoseLifePanel : PopupMenuForm
{
    [SerializeField]
    private DelayButton CloseBtn, ContinueBtn, QuitBtn;

    [SerializeField]
    private GameObject LoseLifeImg, InfiniteLifeImg, LoseLifeText, LoseLevelText, LosePickaxeWinText, LosePickaxeWinStreakText;

    [SerializeField] private Image BgImage;
    [SerializeField] private Sprite[] BgSprites;
    [SerializeField] private TextMeshProUGUILocalize Tittle_Text;
    [SerializeField] private Image TitleImageLeft, TitleImageRight;
    [SerializeField] private Sprite[] TitleSprites;
    [SerializeField] private GameObject TargetParent, LifeRoot, StarRoot, PickaxeRoot, PickaxeWinStreakRoot;
    [SerializeField] private GameObject StarTwoLose, StarThreeLose;
    [SerializeField] private TextMeshProUGUI PickaxeNumText, PickaxeWinStreakText;

    [SerializeField] private Transform PanelArea;
    
    private List<BaseGameFailPanel> AllGameFailPanels;
    BaseGameFailPanel CurGameFailPanel;

    private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
    {
        MaterialPresetName.LevelNormal,
        MaterialPresetName.LevelHard,
        MaterialPresetName.LevelSurpHard,
        MaterialPresetName.Btn_Blue,
    };

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetBtnEvent();
        base.OnInit(uiGroup, completeAction, userData);

        //准备失败场景数据
        PrepareFailPanelData();

        ShowFailPanel();
        
        SetBgImage();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);
        m_IsAvailable = true;
        
        try
        {
            showSuccessAction?.Invoke(this);
        }
        catch (Exception e)
        {
            Debug.LogError($"form {UIName} showSuccessAction error - {e.Message}");
        }
    }

    private void PrepareFailPanelData()
    {
        //获取所有的失败界面
        AllGameFailPanels = PanelArea.GetComponentsInChildren<BaseGameFailPanel>(true).ToList();
        //剔除不展示的
        AllGameFailPanels = AllGameFailPanels.Where(obj => obj.IsShowFailPanel).ToList();
        //判断是否有独立展示的fail界面【独立展示 意思是如果有 其他的fail界面就不展示了】
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
        ContinueBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        QuitBtn.SetBtnEvent(() =>
        {
            QuitBtnEvent();
        });
    }
    
    private void QuitBtnEvent()
    {
        Action<bool> action = b =>
        {
            CloseBtn.enabled = b;
            ContinueBtn.enabled = b;
            QuitBtn.enabled = b;
        };
        if (CurGameFailPanel != null)
        {
            action(false);
            CurGameFailPanel.CloseFailPanel(() => 
            {
                action(true);
                CurGameFailPanel = null;
                QuitBtnEvent();
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
            GameFail();
        }
    }

    private void GameFail()
    {
        // 特殊展示，展示GlacierQuest的失败动画
        if (GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open && !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            GameManager.UI.HideUIForm(this);
            GameManager.Task.GlacierQuestTaskManager.OnGameFail();
            GameManager.UI.ShowUIForm("GlacierQuestMenu",form =>
            {
                Log.Info("GlacierQuest：设置关闭界面的回调");
                GlacierQuestMenu menu = form as GlacierQuestMenu;
                menu.SetCloseEvent(()=>
                {
                    TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
                    if (panel != null)
                    {
                        panel.StartGameLoseToMapProcess(() =>
                        {
                            GameManager.UI.HideUIForm("TileMatchPanel");
                            GameManager.UI.HideUIForm(this);
                        });
                    }
                });
            });
        }
        else
        {
            LevelPlayMenu.RecordSourceIndex = 2;
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Retry);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI,u =>
            {
                GameManager.UI.HideUIForm(this);
            });
        }
    }

    private void SetBgImage()
    {
        if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
        {
            BgImage.sprite = BgSprites[3];
            TitleImageLeft.sprite = TitleSprites[3];
            TitleImageRight.sprite = TitleSprites[3];
            return;
        }
        var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
        BgImage.sprite = BgSprites[hardIndex];
        TitleImageLeft.sprite = TitleSprites[hardIndex];
        TitleImageRight.sprite = TitleSprites[hardIndex];

        Tittle_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);
    }

    private void OnDestroy()
    {
        BgImage.sprite = null;
        for (int i = 0; i < BgSprites.Length; i++)
        {
            BgSprites[i] = null;
        }
    }
    
    /// <summary>
    /// 展示失败界面
    /// </summary>
    private void ShowFailPanel()
    {
        //关闭当前的失败页面
        if (CurGameFailPanel != null) CurGameFailPanel.gameObject.SetActive(false);
        //设置当前需要展示的panel
        CurGameFailPanel = AllGameFailPanels[0];
        AllGameFailPanels.RemoveAt(0);
        //展示
        CurGameFailPanel.gameObject.SetActive(true);
        PanelArea.SetChildActive(false,CurGameFailPanel.gameObject);
        CurGameFailPanel.ShowFailPanel(null);
        //
        Tittle_Text.SetTerm("Settings.Are You Sure?");
    }
}
