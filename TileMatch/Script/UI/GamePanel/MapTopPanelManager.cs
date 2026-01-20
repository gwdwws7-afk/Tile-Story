using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MySelf.Model;
using Spine.Unity;
using UnityEngine.AddressableAssets;

/// <summary>
/// 地图场景顶部栏管理器
/// </summary>
public sealed class MapTopPanelManager : CenterForm,IItemFlyReceiver
{
    public override bool IsAutoRelease => false;
    
    [SerializeField]
    private RectTransform TopTrans, LeftTrans, RightTrans,BottomTrans;
    [SerializeField]
    private DelayButton Setting_Btn, ChangeBG_Btn, RankingList_Btn, DaliyGift_Btn, frogJumpEntrance;

    [SerializeField]
    private GameObject[] Btn_ShinnyAnims;

    [SerializeField]
    private GameObject SettingRedPoint_Obj;

    [SerializeField]
    private DelayButton Help_Btn;
    [SerializeField]
    private Slider HelpBtnProgress_Slider;
    [SerializeField]
    private TextMeshProUGUI HelpBtnProgress_Text;
    [SerializeField]
    private GameObject redPointRoot, changeImgRedPoint;
    [SerializeField]
    private TextMeshProUGUI redPointNumText;
    [SerializeField]
    private DelayButton NewStory_Btn;
    [SerializeField]
    private GameObject effectOnNewStoryBtn;
    [SerializeField]
    private TextMeshProUGUILocalize NewStory_Btn_Localize, NewStory_BtnOutline_Localize;
    [SerializeField]
    private GameObject[] ImageOnHelpBtn;
    [SerializeField]
    public PersonRankEntrance personRankEntrance;
    [SerializeField]
    public TurntableEntrance turntableEntrance;
    [SerializeField] public ClimbBeanstalkEntrance climbBeanstalkEntrance;
    [SerializeField] public KitchenEntrance kitchenEntrance;
    [SerializeField]
    private MapLevelBtn MapLevelBtn;
    [SerializeField]
    public CalendarChallengeEntranceBtn calendarChallengeEntranceBtn;
    public GoldCollectionEntrance goldCollectionEntrance;
    public TilePassEntrance tilePassEntrance;
    public BalloonRiseEntrance balloonRiseEntrance;
    public CardSetEntrance cardSetEntrance;
    public HiddenTemple.HiddenTempleEntrance hiddenTempleEntrance;
    public Merge.MergeEntrance mergeEntrance;
    public PiggyBankEntrance piggyBankEntrance;
    public HarvestKitchenEntrance harvestKitchenEntrance;

    [SerializeField] private Transform Left_Root,Right_Root;

    public RectTransform HelpBtnPos;
    private bool showLevelFingerGuide;

    public PkGameEntrance PkGameEntrance => transform.GetComponentInChildren<PkGameEntrance>(true);

    public TextPromptBox unlockPromptBox;

    public Transform storyText;

    public float LeftRootScale => Left_Root.localScale.x;
    public float rightRootScale => Right_Root.localScale.x;

    #region

    public ReceiverType ReceiverType => ReceiverType.Common;
    public GameObject GetReceiverGameObject() => gameObject;
    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        switch (type)
        {
            default:
                return MapLevelBtn.transform.position;
        }
    }

    public void OnFlyEnd(TotalItemData type)
    {
        MapLevelBtn.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            MapLevelBtn.transform.localScale = Vector3.one;
        };
        MapLevelBtn.PlayLevelBtnEffect();
    }

    public void OnFlyHit(TotalItemData type)
    {
        var cachedTransform = MapLevelBtn.transform;
        cachedTransform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            cachedTransform.localScale = Vector3.one;
        };
    }
    #endregion

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        GameManager.Event.Subscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
        RewardManager.Instance.RegisterItemFlyReceiver(this);

        OnResume();
        if (userData != null)
        {
            (userData as Action)?.Invoke();
        }

        TopTrans.DOKill();
        TopTrans.anchoredPosition = new Vector2(0, 400f);
        RightTrans.DOKill();
        RightTrans.anchoredPosition = new Vector2(300, 0);
        LeftTrans.DOKill();
        LeftTrans.anchoredPosition = new Vector2(-300, 0);

        BottomTrans.DOKill();
        MapLevelBtn.transform.DOKill();
        Help_Btn.transform.DOKill();

        MapLevelBtn.transform.localScale = Vector3.one * 0.4f;
        Help_Btn.transform.localScale = Vector3.one * 0.4f;
        BottomTrans.anchoredPosition = new Vector2(0, -800f);                        

        MapLevelBtn.Init();
        BtnEvent();
        UpdateHelpBtn();

        changeImgRedPoint.SetActive((GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockChangeBgLevel && GameManager.PlayerData.IsShowChangeImageRedPoint)||
                                    GameManager.PlayerData.IsShowBGRedPoint);
        turntableEntrance.gameObject.SetActive(GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockNormalTurntableLevel);
        calendarChallengeEntranceBtn.gameObject.SetActive(GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockCalendarChallengeLevel);
        SetLeaderBoardBtnState();

        showLevelFingerGuide = GameManager.PlayerData.NowLevel <= 5;
        
        SetFrogJumpEntrance();

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        SetRootSize();

        if(showLevelFingerGuide)
            CheckIdleTimeForGuideFinger();

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        try
        {
            GameManager.Event.Unsubscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
            RewardManager.Instance.UnregisterItemFlyReceiver(this);
            
            ShowBtnShinnyAnim(false);

            if (loopSequence != null && loopSequence.IsActive())
            {
                loopSequence.Kill();
            }

            if (guideFingerObject != null)
            {
                Addressables.ReleaseInstance(guideFingerObject);
                guideFingerObject = null;
            }
            lastInputTime = 0;
        }
        catch (Exception e)
        {
            Log.Debug($"MapTopPanelManager:{e.Message}");
        }
        base.OnRelease();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        SetRootSize(true);
        
        //top
        TopTrans.DOAnchorPos(Vector3.zero, 0.4f).SetEase(Ease.OutBack);
        //right
        RightTrans.DOAnchorPos(Vector3.zero, 0.3f).SetDelay(0.15f).SetEase(Ease.OutBack);
        //Left
        LeftTrans.DOAnchorPos(Vector3.zero, 0.3f).SetDelay(0.15f).SetEase(Ease.OutBack);

        BottomTrans.DOAnchorPos(Vector3.zero, 0.4f).SetEase(Ease.OutBack);
        MapLevelBtn.transform.DOScale(Vector3.one,0.5f).SetEase(Ease.OutBack);
        Help_Btn.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (GameManager.PlayerData.NowLevel == 1 && !PlayerBehaviorModel.Instance.HasShownHelpFirstGuide()) 
            {
                ShowHelpGuide();
            }

            ShowBtnShinnyAnim(true);
        });
        
        if(!SystemInfoManager.IsSuperLowMemorySize)
            turntableEntrance.StartRotate();

        BtnEvent();
        base.OnShow(showSuccessAction, userData);
    }
    
    private bool hasShownGuide = false;
    private void ShowHelpGuide()
    {
        if(hasShownGuide)
            return;
        hasShownGuide = true;
        GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
        {
            form.gameObject.SetActive(false);
            var guideMenu = form.GetComponent<CommonGuideMenu>();
            //var originParent = Help_Btn.transform.parent;
            //Help_Btn.transform.SetParent(form.transform);
            GameObject btnObj = Instantiate(Help_Btn.gameObject, HelpBtnPos.position, Quaternion.identity, form.transform);
            btnObj.transform.localScale = Vector3.one;
            Button btn = btnObj.GetComponent<Button>();
            btn.interactable = true;

            guideMenu.SetText("Story.ThePoorGirlNeedsHelp");
            guideMenu.tipBox.SetOkButton(false);
            var position = btnObj.transform.position+new Vector3(0,0.3f,0);
            guideMenu.ShowGuideArrow(position,position+new Vector3(0,0.15f,0));

            guideMenu.tipBox.transform.position = new Vector3(0, position.y + 0.45f, 0);
            guideMenu.OnShow(null, null);
            
            void ClickAction()
            {
                GameManager.UI.ShowUIForm("DecorationOperationPanel");

                if (btnObj != null)
                    Destroy(btnObj);
                PlayerBehaviorModel.Instance.RecordHelpFirstGuide();
                //Help_Btn.transform.SetParent(originParent);
                GameManager.UI.HideUIForm(form);
                guideMenu.guideImage.onTargetAreaClick = null;
            }
            btn.onClick.AddListener(ClickAction);
        });
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        TopTrans.DOKill();
        RightTrans.DOKill();
        LeftTrans.DOKill();
        BottomTrans.DOKill();
        MapLevelBtn.transform.DOKill();
        Help_Btn.transform.DOKill();

        HideUnlockPromptBox();

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnPause()
    {
        //Setting_Btn.interactable = false;
        ChangeBG_Btn.interactable = false;
        DaliyGift_Btn.interactable = false;
        Help_Btn.interactable = false;
        NewStory_Btn.interactable = false;
        frogJumpEntrance.interactable = false;
        turntableEntrance.OnPause();
        base.OnPause();
    }

    public override void OnResume()
    {
        //Setting_Btn.interactable = true;
        ChangeBG_Btn.interactable = true;
        DaliyGift_Btn.interactable = true;
        Help_Btn.interactable = true;
        NewStory_Btn.interactable = true;
        frogJumpEntrance.interactable = true;
        turntableEntrance.OnResume();
        base.OnResume();
    }
    
    public void SetFrogJumpEntrance()
    {
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockFrogJumpLevel &&
            GameManager.PlayerData.CurFrogJumpLevel < GameManager.PlayerData.MaxFrogJumpLevel)
        {
            frogJumpEntrance.gameObject.SetActive(true);
        }
        else
        {
            frogJumpEntrance.gameObject.SetActive(false);
            bool isShowDaliyWatchAdsBtn = GameManager.PlayerData.IsCanDaliyWatchAds &&
                                          (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockDailyWatchAdsLevel);

        }
    }

    private bool m_waitFrame = false;
    public override bool CheckInitComplete()
    {
        if (!personRankEntrance.CheckInitComplete())
            return false;

        if (!m_waitFrame)
        {
            m_waitFrame = true;
            return false;
        }

        return true;
    }

    private bool isForceSetSize = true;
    private void SetRootSize(bool isForce = false)
    {
        int interval = SystemInfoManager.IsSuperLowMemorySize ? 180 : 60;
        if (isForce||isForceSetSize||Time.frameCount % interval == 0)
        {
            isForceSetSize = false;
            GetScreenSize();
            // int childCount = 0;
            // foreach (Transform item in Right_Root)
            // {
            //     if (item.gameObject.activeInHierarchy) childCount++;
            // }
            // if (childCount == 6)
            // {
            //     var ratio = Screen.safeArea.height* 1080 / Screen.safeArea.width / 1920f;
            //     ratio = Math.Max(1f, ratio);
            //     if (ratio < 1f / t)
            //     {
            //         Right_Root.localScale = Vector3.one * (ratio * t);
            //     }
            //     else
            //         Right_Root.localScale = Vector3.one;
            // }
        }
    }
    
    private  float ratio=0;
    private float Ratio
    {
        get
        {
            // if (ratio == 0)
            {
                float curYSize = Left_Root.GetComponent<RectTransform>().rect.height;
                ratio = curYSize / 1100;
            }
            return ratio;
        }
    }
    
    /// <summary>
    /// 计算出屏幕可用空间size
    /// </summary>
    private void GetScreenSize()
    {
        float changeSize = 1f;
        
        int childCount = 0;
        for (int i = 0; i < Left_Root.childCount; i++)
        {
            if (Left_Root.GetChild(i).gameObject.activeInHierarchy) childCount++;
        }

        if (childCount > 6)
        {
            changeSize = Math.Min(changeSize, Ratio * (5.8f / childCount));
        }
        
        childCount = 0;
        for (int i = 0; i < Right_Root.childCount; i++)
        {
            if (Right_Root.GetChild(i).gameObject.activeInHierarchy) childCount++;
        }
        
        if (childCount > 6)
        {
            changeSize = Math.Min(changeSize, Ratio * (5.8f / childCount));
        }

        if (Left_Root.localScale.x != changeSize || Right_Root.localScale.x != changeSize) 
        {
            Left_Root.localScale = Vector3.one * changeSize;
            Right_Root.localScale = Vector3.one * changeSize;
        }
    }

    public void ShowLevelBtnAnim(Action finishAction)
    {
        MapLevelBtn.PlayLevelAnim(finishAction);
    }

    private void BtnEvent()
    {
        MapLevelBtn.SetLevelBtnEvent(() =>
        {
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockAddOneStepBoostLevel)
            {
                ProcedureUtil.ProcedureMapToGame();
            }
            else
            {
                GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
            }
        });
        Help_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.ShowUIForm("DecorationOperationPanel");
        });
        NewStory_Btn.SetBtnEvent(() =>
        {
            if (GameManager.Process.Count <= 0)
            {
                if (DecorationModel.Instance.GetDecorationOperatingAreaID() == Constant.GameConfig.MaxDecorationArea)
                    GameManager.UI.ShowUIForm("DecorationViewPanel",null, null, true);//true 显示右上角CloseBtn 
                else
                    GameManager.UI.ShowUIForm("DecorationViewPanel",null, null, false);//false 不显示右上角CloseBtn 
            }
        });
        Setting_Btn.SetBtnEvent(() =>
        {
            GameManager.PlayerData.IsShowSettingRedPoint = false;
            //SettingRedPoint_Obj.gameObject.SetActive(GameManager.PlayerData.IsShowSettingRedPoint);
            GameManager.UI.ShowUIForm("MapSettingMenuPanel");
        });

        ChangeBG_Btn.SetBtnEvent(()=> 
        {
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockChangeBgLevel) 
            {
                GameManager.UI.ShowWeakHint("Theme.Unlock at Level {0}", new Vector3(0, -0.2f), Constant.GameConfig.UnlockChangeBgLevel.ToString());
                return;
            }

            //打开换背景界面
            GameManager.UI.ShowUIForm("ChangeImagePanel",f =>
            {
                GameManager.PlayerData.RemoveShowBGRedPoint();
                if (changeImgRedPoint.activeSelf)
                {
                    changeImgRedPoint.SetActive(false);
                    GameManager.PlayerData.IsShowChangeImageRedPoint = false;
                }
            });
        });

        RankingList_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.ShowWeakHint("Story.ComingSoon");
        });
    }

    public void UpdateHelpBtn()
    {
        int nowAreaID = DecorationModel.Instance.GetDecorationOperatingAreaID();
        int maxCount = DecorationModel.Instance.GetTargetAreaMaxDecorationCount(nowAreaID);
        int currentCount = DecorationModel.Instance.GetTargetAreaFinishedDecorationCount(nowAreaID);

        bool isCurAreaHaveAsset = GameManager.Download.IsCurAreaHaveAsset();

        int imageIndex = nowAreaID;
        if (imageIndex > 3)
            imageIndex = 1;
        for (int i = 1; i <= ImageOnHelpBtn.Length; i++)
        {
            ImageOnHelpBtn[i - 1].gameObject.SetActive(imageIndex == i);
        }

        if (currentCount < maxCount)
        {
            NewStory_Btn.gameObject.SetActive(false);
            Help_Btn.gameObject.SetActive(true);

            HelpBtnProgress_Slider.value = currentCount / (float)maxCount;
            HelpBtnProgress_Slider.fillRect.gameObject.SetActive(currentCount != 0);
            HelpBtnProgress_Text.text = $"{currentCount}/{maxCount}";
        }
        else
        {
            NewStory_Btn.gameObject.SetActive(true);
            if (nowAreaID == Constant.GameConfig.MaxDecorationArea)
            {
                NewStory_Btn_Localize.SetTerm("Story.ComingSoon");
                NewStory_BtnOutline_Localize.SetTerm("Story.ComingSoon");
                effectOnNewStoryBtn.SetActive(false);
            }
            else
            {
                NewStory_Btn_Localize.SetTerm("Help.NewStory");
                NewStory_BtnOutline_Localize.SetTerm("Help.NewStory");
                effectOnNewStoryBtn.SetActive(true);
            }
            Help_Btn.gameObject.SetActive(false);
        }

        if (!isCurAreaHaveAsset)
        {
            NewStory_Btn.gameObject.SetActive(true);
            Help_Btn.gameObject.SetActive(false);
        }

        int canDecorateCount = DecorationModel.Instance.GetOperatingAreaCanDecorateObjectNum();
        bool hasShowHelpFirstGuide = PlayerBehaviorModel.Instance.HasShownHelpFirstGuide();
        redPointRoot.gameObject.SetActive(canDecorateCount > 0 && hasShowHelpFirstGuide);//第一个引导没播放前 也不显示红点
        redPointNumText.text = canDecorateCount.ToString();

        if (canDecorateCount > 0)
            ShowStoryTextAnim();
        else
            HideStoryTextAnim();
    }

    public void SetLeaderBoardBtnState()
    {
        GameManager.Task.PersonRankManager.CheckIsOpen();
        var flag1 = GameManager.PlayerData.NowLevel>=Constant.GameConfig.UnlockPersonRankButtonLevel;
        var flag2 = GameManager.Task.PersonRankManager.TaskState!=PersonRankState.End;
        personRankEntrance.gameObject.SetActive(flag1&&flag2);
        if(flag1&&flag2)
        {
            personRankEntrance.InitEntrance();
            if (!GameManager.Task.PersonRankManager.HasShownPersonRankEntranceBtn&&GameManager.PlayerData.NowLevel<Constant.GameConfig.UnlockPersonRankGameLevel)
            {
                GameManager.Task.PersonRankManager.HasShownPersonRankEntranceBtn = true;
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_IconPreview);
            }
        }
    }

    private void OnDecorationAreaChange(object sender, GameEventArgs e)
    {
        //ChangeDecorationAreaEventArgs changeEvent = (ChangeDecorationAreaEventArgs)e;
        UpdateHelpBtn();
    }

    Sequence sequence = null;
    public void ShowBtnShinnyAnim(bool isShow)
    {
        if (isShow)
        {
            if (SystemInfoManager.DeviceType <= DeviceType.Normal)
                return;

            if (sequence != null) return;
            foreach (var shiny in Btn_ShinnyAnims)
            {
                shiny.gameObject.SetActive(false);
            }
            sequence = DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() => Btn_ShinnyAnims[0].SetActive(true))
                .AppendInterval(3.5f)
                .AppendCallback(() => Btn_ShinnyAnims[0].SetActive(false))
                .AppendInterval(2f)
                .SetLoops(-1)
                .OnKill(()=>sequence=null);
        }
        else
		{
            if(sequence!=null) sequence.Kill();
            foreach (var shiny in Btn_ShinnyAnims)
            {
                shiny.gameObject.SetActive(false);
            }
        }
    }

    #region GuideFinger
    private float lastInputTime;
    private const float IDLE_TIME_THRESHOLD = 5.0f;
    private GameObject guideFingerObject;
    private void CheckIdleTimeForGuideFinger()
    {
        //如果有其他PopupUI 也不显示
        // UIGroup popUpGroup = GameManager.UI.GetUIGroup(UIFormType.PopupUI);
        UIGroup popUpGroup = GameManager.UI.GetUIGroup("PopupUI");
        if (popUpGroup.CurrentUIForm != null && popUpGroup.CurrentUIForm.gameObject.activeInHierarchy)
        {
            lastInputTime = 0;
            return;
        }

        //如果已经在引导 也不显示
        // var uiForm = GameManager.UI.GetUIForm(popUpGroup,"CommonGuideMenu");
        var uiForm = GameManager.UI.GetUIForm("CommonGuideMenu");
        if (uiForm != null && uiForm.gameObject.activeInHierarchy)
        {
            lastInputTime = 0;
            return;
        }

        if (lastInputTime == 0) lastInputTime = Time.time;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                lastInputTime = Time.time;
                HideGuideFinger();
            }
            else if (Time.time - lastInputTime > IDLE_TIME_THRESHOLD)
            {
                lastInputTime = Time.time;
                CreateOrShowGuideFinger();
            }
        }
        else
        {
            if (Input.touchCount > 0)
            {
                lastInputTime = Time.time;
                HideGuideFinger();
            }
            else if (Time.time - lastInputTime > IDLE_TIME_THRESHOLD)
            {
                lastInputTime = Time.time;
                CreateOrShowGuideFinger();
            }
        }
    }

    private void CreateOrShowGuideFinger()
    {
        if (guideFingerObject == null)
        {
            UnityUtility.InstantiateAsync("SimpleGuideFinger", MapLevelBtn.transform, obj =>
            {
                guideFingerObject = obj;
                obj.transform.localPosition = new Vector3(25.0f, -50.0f, 0);
                //一个点三下(约两秒)隐藏两秒的动画
                SkeletonGraphic sg = guideFingerObject.GetComponentInChildren<SkeletonGraphic>();
                sg.AnimationState.SetAnimation(0, "03", true);
            });
        }
        else
        {
            if (!guideFingerObject.activeSelf)
            {
                guideFingerObject.SetActive(true);
                SkeletonGraphic sg = guideFingerObject.GetComponentInChildren<SkeletonGraphic>();
                sg.AnimationState.GetCurrent(0).TrackTime = 0;
            }
        }
    }

    private void HideGuideFinger()
    {
        if (guideFingerObject != null && guideFingerObject.activeSelf) 
        {
            guideFingerObject.SetActive(false);
        }
    }
    #endregion

    public Transform GetChangeBGBtnTrans()
    {
        return ChangeBG_Btn.transform;
    }

    public Transform GetLevelBtnTrans()
    {
        return MapLevelBtn.transform;
    }

    public DelayButton SettingBtn=>Setting_Btn;

    public void ShowUnlockPromptBox(int unlockLevel, UIFormType type, Vector3 pos)
    {
        unlockPromptBox.promptText.GetComponent<TextMeshProUGUILocalize>().SetParameterValue("level", unlockLevel.ToString());
        unlockPromptBox.ShowPromptBox(type == UIFormType.LeftUI ? PromptBoxShowDirection.Right : PromptBoxShowDirection.Left, pos, 2f);
    }

    public void HideUnlockPromptBox()
    {
        unlockPromptBox.HidePromptBox();
    }

    private Sequence loopSequence;

    private void ShowStoryTextAnim()
    {
        // 如果已有动画运行，先杀死它
        if (loopSequence != null && loopSequence.IsActive())
        {
            loopSequence.Kill();
        }

        // 重置为原始尺寸
        storyText.localScale = Vector3.one;

        // 创建动画序列
        loopSequence = DOTween.Sequence();

        loopSequence.Append(storyText.DOScale(1.15f, 0.2f)
            .SetEase(Ease.OutCubic));

        loopSequence.Append(storyText.DOScale(0.95f, 0.22f)
            .SetEase(Ease.InQuad));

        loopSequence.Append(storyText.DOScale(1.05f, 0.15f)
            .SetEase(Ease.OutQuad));

        loopSequence.Append(storyText.DOScale(1f, 0.15f)
            .SetEase(Ease.InQuad));

        // 第三步：添加间隔时间
        loopSequence.AppendInterval(1f);

        // 设置循环播放（-1表示无限循环）
        loopSequence.SetLoops(-1, LoopType.Restart);
    }

    private void HideStoryTextAnim()
    {
        // 如果已有动画运行，先杀死它
        if (loopSequence != null && loopSequence.IsActive())
        {
            loopSequence.Kill();
        }

        // 重置为原始尺寸
        storyText.localScale = Vector3.one;
    }
}
