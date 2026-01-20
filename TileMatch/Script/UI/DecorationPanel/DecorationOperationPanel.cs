using GameFramework.Event;
using MySelf.Model;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class DecorationOperationPanel : CenterForm, ICustomOnEscapeBtnClicked
{
    [SerializeField]
    private DelayButton closeBtn;
	[SerializeField]
	private DelayButton chapterBtn;
	[SerializeField]
	private UnlockDecorationItemBtn unlockDecorationItemBtnTemplate;
    [SerializeField]
    private Transform unlockBtnRoot;
    private List<UnlockDecorationItemBtn> unlockBtnList = new List<UnlockDecorationItemBtn>();

    [SerializeField]
    private DecorationOperationSlider operationSlider;
    [SerializeField]
    private DelayButton chestButton;

    [SerializeField]
    private DelayButton chooseChapterBtn;

    [SerializeField]
    private GameObject[] starFlyObjects;//上限三个 不做池了 固定先放好三个
    [SerializeField]
    private Transform starFlyStartTrans;
    [SerializeField]
    private GameObject shinningFlyObject;

    [SerializeField]
    private ItemPromptBox itemPromptBox;
    [SerializeField]
    private Transform showBoxPos;

    [SerializeField]
    private SkeletonGraphic broomSpine;

    [SerializeField]
    private DecorationBGDialogBubble dialogBubble;

    private int inAnimValue = 0x00;
    private bool showedUnlockBtn = false;

    public enum InAnimReason
    {
        StarFlyTillShinningFly = 0x01,
        StoryPanel = 0x02,
        Guide = 0x04,
        AreaComplete = 0x08,
        OpenDecorationViewPanel = 0x10,
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
        GameManager.Event.Subscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnChangeSize);

        ClearInAnimValue();

        BtnEvent();
        HideDialogBubble();

        showedUnlockBtn = false;
        int chapterId = DecorationModel.Instance.GetDecorationOperatingAreaID();
        int buildSchedule = GameManager.PlayerData.OperatingAreaFinishDecorationCount;
        if (!GameManager.PlayerData.IsHaveShowStory(chapterId, buildSchedule)
            && GameManager.DataTable.GetDataTable<DTHelp>().Data.IsShowStory(chapterId, buildSchedule))
        {
            unlockBtnRoot.gameObject.SetActive(false);
            UpdateProgressBar();
        }
        else
        {
            RefreshDisplay();
        }

        GameManager.Task.AddDelayTriggerTask(0, () =>
        {
            GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
            ChangeHelpSE(false);
        });

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        base.OnShowInit(showInitSuccessAction, userData);
    }

    public void Register()
    {
        int chapterId = DecorationModel.Instance.GetDecorationOperatingAreaID();
        int buildSchedule = GameManager.PlayerData.OperatingAreaFinishDecorationCount;
        if (!GameManager.PlayerData.IsHaveShowStory(chapterId,buildSchedule)
            &&
            GameManager.DataTable.GetDataTable<DTHelp>().Data.IsShowStory(chapterId,buildSchedule))
        {
            GameManager.Process.Register(ProcessType.ShowStoryPanel,80, () =>
            {
                RegisterInAnimReason(InAnimReason.StoryPanel);
                GameManager.UI.ShowUIForm("StoryPanel",(u) =>
                {
                    u.SetHideAction(() =>
                    {
                        UnregisterInAnimReason(InAnimReason.StoryPanel);
                        GameManager.Process.EndProcess(ProcessType.ShowStoryPanel);
                        HideDialogBubble();
                        if (!showedUnlockBtn)
                            ShowUnlockBtn();
                        else
                            RefreshUnlockBtnPos();
                    });
                }, userData: new
                {
                    chapterID = DecorationModel.Instance.GetDecorationOperatingAreaID(),
                    buildSchedule = GameManager.PlayerData.OperatingAreaFinishDecorationCount,
                });
            });
        }
        
        if (!PlayerBehaviorModel.Instance.HasShownHelpSecondGuide())
        {
            GameManager.Process.Register(ProcessType.ShowGuide, 60, () =>
            {
                ShowHelpGuide();
            });
        }
        GameManager.Process.ExecuteProcess();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnReveal()
    {
        RefreshUnlockBtnPos();
        base.OnReveal();
    }

    private int taskId;
    private void ShowHelpGuide()
    {
        if (taskId != 0)
        {
            GameManager.Task.RemoveDelayTriggerTask(taskId);
        }
        RegisterInAnimReason(InAnimReason.Guide);
        taskId = GameManager.Task.AddDelayTriggerTask(0.3f, () =>
        {
            UnlockDecorationItemBtn originObj = unlockBtnList.Find(x => x.gameObject.activeSelf);
            if (ReferenceEquals(originObj, null))
            {
                GameManager.Process.EndProcess(ProcessType.ShowGuide);
                return;
            }

            GameManager.UI.ShowUIForm("FingerGuideMenu",form =>
            {                
                form.gameObject.SetActive(false);
                var guideMenu = form.GetComponent<FingerGuideMenu>();
                var position = originObj.transform.position-new Vector3(0,0.05f,0);
                guideMenu.finger.transform.position = position;
                guideMenu.SetText("Story.Guide.Tap here to restore her home");
                guideMenu.tipBox.SetOkButton(false);
                guideMenu.tipBox.transform.position = originObj.transform.position + new Vector3(0, 0.4f, 0);
                guideMenu.AutoHide = false;
                guideMenu.SetGuideImageVisibility(true);

                Transform originParent = originObj.transform.parent;
                //originObj.transform.SetParent(form.transform);
                Canvas canvas = originObj.gameObject.GetOrAddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "UI";
                canvas.sortingOrder = 9;
                originObj.gameObject.GetOrAddComponent<GraphicRaycaster>();

                guideMenu.OnShow((f) =>
                {
                    GameManager.Task.AddDelayTriggerTask(0.1f, () =>
                    {
                        UnregisterInAnimReason(InAnimReason.Guide);
                    });
                });

                guideMenu.onSkipAction = () =>
                {
                    if (canvas != null)
                        canvas.sortingOrder = 4;
                };

                void ClickAction()
                {
                    PlayerBehaviorModel.Instance.RecordHelpSecondGuide();
                    //originObj.transform.SetParent(originParent);
                    GameManager.UI.HideUIForm(form);
                    guideMenu.guideImage.onTargetAreaClick = null;
                    originObj.GetComponent<DelayButton>().onClick.RemoveListener(ClickAction);

                    GameManager.Process.EndProcess(ProcessType.ShowGuide);
                }
                originObj.GetComponent<DelayButton>().onClick.AddListener(ClickAction);
            });
        });
    }

    public override void OnRelease()
    {
        //GameManager.Sound.PlayMusic(GameManager.PlayerData.BGMusicName);
        ChangeHelpSE(true);
        GameManager.Event.Unsubscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnChangeSize);
        
        closeBtn.OnReset();
        chooseChapterBtn.OnReset();
        for (int i = 0; i < unlockBtnList.Count; ++i)
        {
            unlockBtnList[i].OnRelease();
        }
        itemPromptBox.OnRelease();
        GameManager.Task.RemoveDelayTriggerTask(taskId);
        taskId = 0;
        if (guideFingerObject != null)
        {
            Destroy(guideFingerObject);
            guideFingerObject = null;
        }
        lastInputTime = 0;
        base.OnRelease();
	}


    private void Update()
    {
#if UNITY_EDITOR
        //if (Input.GetKeyUp(KeyCode.C))
        //{
        //    GameManager.UI.ShowUIForm<DecorationAreaCompleteMenu>();
        //}
#endif

        try
        {
            CheckIdleTimeForGuideFinger();

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
#else
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
            {
                itemPromptBox.HidePromptBox();
                HideDialogBubble();
            }
        }
        catch (Exception e)
        {
            Log.Debug($"{e.Message} {e.StackTrace}");
        }
    }


    private void BtnEvent()
	{
		closeBtn.SetBtnEvent(() =>
		{
            if (InAnim())
                return;

			GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm("MapTopPanelManager");

            //重置家具描边效果
            MapDecorationBGPanel decorationBGPanel = (MapDecorationBGPanel)GameManager.UI.GetUIForm("MapDecorationBGPanel");
            decorationBGPanel.ChangeAllDecorationObjectPrefabToNormalMaterial();
        });

        chooseChapterBtn.SetBtnEvent(() =>
        {
            if (InAnim())
                return;

            RegisterInAnimReason(InAnimReason.OpenDecorationViewPanel);
            GameManager.UI.ShowUIForm("DecorationViewPanel",form => { UnregisterInAnimReason(InAnimReason.OpenDecorationViewPanel); },
                () => { UnregisterInAnimReason(InAnimReason.OpenDecorationViewPanel); }, true);//true 显示右上角CloseBtn
        });

        chestButton.SetBtnEvent(() =>
        {
            if (InAnim())
                return;

            ShowPrompt(PromptBoxShowDirection.Down, showBoxPos.position);
        });
    }

    public void RefreshDisplay()
    {
        try
        {
            ShowUnlockBtn();
            UpdateProgressBar();
        }
        catch (Exception e)
        {
           Log.Error($"RefreshDisplay:{e.Message} {e.StackTrace}");
        }
    }

	private void ShowUnlockBtn()
    {
        showedUnlockBtn = true;

        int nowAreaID = DecorationModel.Instance.GetDecorationOperatingAreaID();

        unlockDecorationItemBtnTemplate.gameObject.SetActive(false);

        MapDecorationBGPanel decorationBGPanel = GameManager.UI.GetUIForm("MapDecorationBGPanel") as MapDecorationBGPanel;
        decorationBGPanel.ChangeAllDecorationObjectPrefabMaterial();//可解锁的设置为红色描边 否则变为普通
        decorationBGPanel.HideDialogBubble();
        Vector3[] allUnlockBtnPositionArray = decorationBGPanel.GetAllUnlockBtnPosition();

        int createNum = allUnlockBtnPositionArray.Length - unlockBtnList.Count;
        for (int i = 0; i < createNum; ++i)
        {
            UnlockDecorationItemBtn newBtn = Instantiate(unlockDecorationItemBtnTemplate, unlockBtnRoot);
            unlockBtnList.Add(newBtn);
        }

        for (int i = 0; i < unlockBtnList.Count; ++i)
        {
            if (i < allUnlockBtnPositionArray.Length)
            {
                int currentProgress = DecorationModel.Instance.GetTargetDecorationType(nowAreaID, i);
                if (currentProgress < 0)
                {
                    //这种情况会发生在 切换区域时 当unlockBtnList已经被其他区域撑大之后 尝试去寻找了当前区域不存在的装修组件
                    unlockBtnList[i].gameObject.SetActive(false);
                }
                else
                {
                    if (currentProgress == 0)//未装修
                    {
                        DecorateItem decorateItem = DecorationModel.Instance.GetTargetDecorationItem(nowAreaID, i);
                        if (DecorationModel.Instance.GetTargetDecorationItemIsUnlocked(decorateItem))
                        {
                            if (!unlockBtnList[i].gameObject.activeSelf)
                            {
                                int index = i;
                                Transform cachedTrans = unlockBtnList[index].GetComponent<DelayButton>().body;
                                cachedTrans.localScale = Vector3.zero;
                                unlockBtnList[index].gameObject.SetActive(true);
                                cachedTrans.DOScale(1.05f, 0.2f).onComplete = () =>
                                {
                                    cachedTrans.DOScale(1f, 0.2f);
                                };
                            }
                            else
                            {
                                unlockBtnList[i].gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            unlockBtnList[i].gameObject.SetActive(false);
                        }
                    }
                    else//已装修
                    {
                        unlockBtnList[i].gameObject.SetActive(false);
                    }

                    unlockBtnList[i].Init(nowAreaID, i, this);
                    unlockBtnList[i].transform.position = allUnlockBtnPositionArray[i];
                }
            }
            else
            {
                unlockBtnList[i].gameObject.SetActive(false);
            }
        }
    }

    private void RefreshUnlockBtnPos()
    {
        try
        {
            MapDecorationBGPanel decorationBGPanel = (MapDecorationBGPanel)GameManager.UI.GetUIForm("MapDecorationBGPanel");
            Vector3[] allUnlockBtnPositionArray = decorationBGPanel.GetAllUnlockBtnPosition();
            for (int i = 0; i < unlockBtnList.Count; ++i)
            {
                if (i < allUnlockBtnPositionArray.Length)
                {
                    unlockBtnList[i].transform.position = allUnlockBtnPositionArray[i];
                }
            }
        }
        catch (Exception e)
        {
           Log.Warning($"RefreshUnlockBtnPos:{e.Message} {e.StackTrace}");
        }
    }

    private void UpdateProgressBar()
    {
        int nowAreaID = DecorationModel.Instance.GetDecorationOperatingAreaID();
        int maxCount = DecorationModel.Instance.GetTargetAreaMaxDecorationCount(nowAreaID);
        int currentCount = DecorationModel.Instance.GetTargetAreaFinishedDecorationCount(nowAreaID);

        operationSlider.SetSliderValue(currentCount, maxCount);

        CheckAndShowDecorationAreaComplete();
    }

    private void CheckAndShowDecorationAreaComplete()
    {
        Log.Info("CheckAndShowDecorationAreaComplete...");

        Register();
        int recentAreaID = GameManager.PlayerData.DecorationAreaID;
        if (DecorationModel.Instance.CheckTargetAreaIsComplete(recentAreaID) &&
            !DecorationModel.Instance.GetTargetAreaGetReward(recentAreaID))
        {
            if (!DecorationModel.Instance.inAreaCompleteAnim)
            {
                DecorationModel.Instance.inAreaCompleteAnim = true;
                
                GameManager.Process.Register(ProcessType.DecorationUpdateProgressBar,20, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.DecorationUpdateProgressBar);
                    RegisterInAnimReason(InAnimReason.AreaComplete);
                    GameManager.UI.ShowUIForm("DecorationAreaCompleteMenu");
                });
                GameManager.Process.ExecuteProcess();

                GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
            }
        }
    }

    private void ChangeHelpSE(bool toMute)
    {
        try
        {
            MapDecorationBGPanel decorationBGPanel = GameManager.UI.GetUIForm("MapDecorationBGPanel") as MapDecorationBGPanel;
            AutoPlaySound[] scripts = decorationBGPanel.GetComponentsInChildren<AutoPlaySound>();
            for(int i = 0; i < scripts.Length; ++i)
            {
                if (toMute)
                    scripts[i].MuteSound();
                else
                    scripts[i].ResumeSound();
            }
        }
        catch (Exception e)
        {
            Log.Error($"ChangeHelpSE:{e.Message}");
        }
    }

    public void PlayStarFlyingAnim(int starNum, Vector3 targetPostion, Action onFlyFinished, Action onFlyFinished_NotLastOne)
    {
        int animStarNum = Mathf.Min(starNum, starFlyObjects.Length);//再多也只飞这么些个 (现在是三个)

        for (int i = 0; i < animStarNum; ++i)
        {
            float delay = 0.1f * i;

            GameObject starFlyObject = starFlyObjects[i];
            starFlyObject.transform.position = starFlyStartTrans.position;
            starFlyObject.SetActive(true);
            int index = i;
            starFlyObject.transform.DOJump(targetPostion, -0.01f, 1, 0.5f).SetDelay(delay).OnComplete(() =>
            {
                GameManager.Sound.PlayAudio("SFX_itemget");//"bo"
                starFlyObject.SetActive(false);
                //最后一个飞完 回调
                if (index == animStarNum - 1)
                {
                    if (onFlyFinished != null)
                        onFlyFinished();
                }
                else
                {
                    if (onFlyFinished_NotLastOne != null)
                        onFlyFinished_NotLastOne();
                }
            });
        }
    }

    public void PlayShinningFlyAnim(Vector3 startPostion, Action onFlyFinished)
    {
        shinningFlyObject.transform.position = startPostion;
        shinningFlyObject.SetActive(true);
        //Vector3 shinningFlyEndTransPosition = shinningFlyEndTrans.position;
        Vector3 shinningFlyEndTransPosition = operationSlider.GetShinningFlyDestinationPosition().position;
        shinningFlyObject.transform.DOMove(shinningFlyEndTransPosition, 0.5f);

        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            if (onFlyFinished != null)
                onFlyFinished();
            //飞到终点后等一会儿再关掉 确保拖尾不会凭空消失
            GameManager.Task.AddDelayTriggerTask(0.3f, () =>
            {
                shinningFlyObject.SetActive(false);
            });
        });
    }

    public void ShowPrompt(PromptBoxShowDirection direction, Vector3 pos)
    {
        List<TotalItemData> rewardTypeList = new List<TotalItemData>();
        List<int> rewardNumList = new List<int>();

        int operatingAreaID = DecorationModel.Instance.GetDecorationOperatingAreaID();
        DecorateArea data = GameManager.DataTable.GetDataTable<DTDecorateArea>().Data.GetDecorateArea((area) => area.ID == operatingAreaID);
        foreach (var rewardData in data.Reward)
        {
            rewardTypeList.Add(TotalItemData.FromInt(rewardData.itemId));
            rewardNumList.Add(rewardData.number);
        }

        itemPromptBox.Init(rewardTypeList, rewardNumList);
        itemPromptBox.ShowPromptBox(direction, pos);
    }

    public void PlayBroomAnim(Action onAnimFinished)
    {
        GameManager.Sound.PlayAudio(SoundType.SFX_sweeping_broom.ToString());

        int operatingAreaID = DecorationModel.Instance.GetDecorationOperatingAreaID();
        //if (operatingAreaID == 19)
        //    broomSpine.transform.localPosition = new Vector3(0, 102.0f, 0);
        //else
        //    broomSpine.transform.localPosition = new Vector3(0, -598.0f, 0);
        broomSpine.transform.localPosition = new Vector3(0, -598.0f, 0);

        broomSpine.gameObject.SetActive(true);
        broomSpine.AnimationState.SetAnimation(0, "idle", false).Complete += (t) =>
        {
            if (onAnimFinished != null)
            {
                onAnimFinished();
                GameManager.Sound.PlayAudio(SoundType.SFX_DecorateFinished.ToString());
            }

        };
    }

    /// <summary>
    /// 装修背景升级时播放的特效
    /// </summary>
    public void PlayBgUpgradeEffect()
    {
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "decorate_chapter_glow",
            "TileItemDestroyEffectPool",
            2.0f,
            transform.position,
            Quaternion.identity,
            transform);
    }

    public void PlayDialogBubble(IdleDialogueData dialogueData, Transform dialogBubblePosRef, bool dialogFaceLeft, bool useTriangleEdge, bool skipClickSound)
    {
        if (!skipClickSound)
            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        dialogBubble.ShowTalk(dialogueData, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge);
    }

    public void PlayDialogBubble(string inputTerm, Transform dialogBubblePosRef, bool dialogFaceLeft, bool useTriangleEdge, bool skipClickSound)
    {
        if (!skipClickSound)
            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        dialogBubble.ShowTalk(inputTerm, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge);
    }

    public void RegisterInAnimReason(InAnimReason inputReason)
    {
        //Log.Info($"RegisterInAnimReason {inputReason}({(int)inputReason}) inAnimValue = {inAnimValue}");
        inAnimValue = inAnimValue | (int)inputReason;
        //Log.Info($"RegisterInAnimReason after inAnimValue = {inAnimValue}");

    }

    public void UnregisterInAnimReason(InAnimReason inputReason)
    {
        //Log.Info($"UnregisterInAnimReason {inputReason}({(int)inputReason}) inAnimValue = {inAnimValue}");
        inAnimValue = inAnimValue & ~(int)inputReason;
        //Log.Info($"UnregisterInAnimReason after inAnimValue = {inAnimValue}");
    }

    public void ClearInAnimValue()
    {
        //Log.Info($"ClearInAnimValue inAnimValue = {inAnimValue}");
        inAnimValue = 0;
        //Log.Info($"ClearInAnimValue after inAnimValue = {inAnimValue}");
    }

    public bool InAnim()
    {
        return inAnimValue != 0;
    }

    public void OnEscapeBtnClicked()
    {
        if (InAnim())
            return;
        if (!closeBtn.isActiveAndEnabled)
            return;

        itemPromptBox.HidePromptBox();
        HideDialogBubble();

        closeBtn?.onClick?.Invoke();
    }

    private void HideDialogBubble()
    {
        dialogBubble.Hide();
    }

    private void OnDecorationAreaChange(object sender, GameEventArgs e)
    {
        //ChangeDecorationAreaEventArgs changeEvent = (ChangeDecorationAreaEventArgs)e;
        RefreshDisplay();
    }
    
    private void OnChangeSize(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.DecorationScaleUp:
                unlockBtnRoot.gameObject.SetActive(false);
                break;
            case CommonEventType.DecorationScaleDown:
                unlockBtnRoot.gameObject.SetActive(true);
                break;
        }
    }

    #region GuideFinger
    private float lastInputTime;
    private const float IDLE_TIME_THRESHOLD = 5.0f;
    private GameObject guideFingerObject;
    private void CheckIdleTimeForGuideFinger()
    {
        int chapterId = DecorationModel.Instance.GetDecorationOperatingAreaID();
        //int currentCount = DecorationModel.Instance.GetTargetAreaFinishedDecorationCount(chapterId);

        if (chapterId > 1)//只在第一章显示该引导
            return;

        //没有显示过引导建造第一个按钮的引导
        if (!PlayerBehaviorModel.Instance.HasShownHelpSecondGuide())
            return;

        //如果已经在引导 也不显示
        var uiForm = GameManager.UI.GetUIForm("FingerGuideMenu");
        if (uiForm != null && uiForm.gameObject.activeInHierarchy) return;

        //如果有弹窗 不显示
        UIGroup popUpGroup = GameManager.UI.GetUIGroup(UIFormType.PopupUI);
        if (popUpGroup.CurrentUIForm != null && popUpGroup.CurrentUIForm.gameObject.activeInHierarchy)
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
        int lowestCost = int.MaxValue;
        UnlockDecorationItemBtn targetBtn = null;
        for(int i = 0; i < unlockBtnList.Count; ++i)
        {
            if(unlockBtnList[i].isActiveAndEnabled && unlockBtnList[i].StarCost > 0 && unlockBtnList[i].StarCost < lowestCost)
            {
                lowestCost = unlockBtnList[i].StarCost;
                targetBtn = unlockBtnList[i];
            }
        }
        if (targetBtn == null)
            return;

        if (guideFingerObject == null)
        {
            UnityUtility.InstantiateAsync("SimpleGuideFinger", targetBtn.transform, obj =>
            {
                guideFingerObject = obj;
                obj.transform.localPosition = new Vector3(0f, 0f, 0);
                guideFingerObject.transform.SetParent(targetBtn.transform.parent);
                SkeletonGraphic sg = guideFingerObject.GetComponentInChildren<SkeletonGraphic>();
                sg.AnimationState.SetAnimation(0, "03", true);
            });
        }
        else
        {
            if (!guideFingerObject.activeSelf)
            {
                guideFingerObject.gameObject.SetActive(true);

                guideFingerObject.transform.SetParent(targetBtn.transform);
                guideFingerObject.transform.localPosition = new Vector3(0f, 0f, 0);
                guideFingerObject.transform.SetParent(targetBtn.transform.parent);
            }
        }
    }

    private void HideGuideFinger()
    {
        if (guideFingerObject != null)
        {
            guideFingerObject.gameObject.SetActive(false);
        }
    }
    #endregion
}
