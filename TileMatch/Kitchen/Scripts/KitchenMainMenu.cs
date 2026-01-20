using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class KitchenMainMenu : PopupMenuForm
{
    private DTKitchenTaskDatas currentTask = null;
    
    public DelayButton playButton, closeButton, explainButton, okButton, greatButton;
    public Image slider;
    public TextMeshProUGUI process;
    public SkeletonGraphic girlSpine;
    public ClockBar clockBar;
    public Transform phoneTrans, sliderTrans, stageRewardParent;
    public GameObject stageRewardPrefab;

    public Transform sliderCentreTrans, rewardCentreTrans;
    public Image sliderCentre;
    public TextMeshProUGUI processCentre;
    public KitchenStageRewardItem[] rewardCentre;

    public TextMeshProUGUI chefHatNumText, consumeChefHatNumText;

    // 飞获得的点赞
    public GameObject flyReward;
    public Transform praiseReward, flyTarget;
    public TextMeshProUGUI praiseNumText;

    public Transform firstEnterDialogBox, normalEnterDialogBox, startGuideDialogBox;
    public TextMeshProUGUILocalize firstEnterText, normalEnterText;

    public GameObject guide, sliderAnimOverSpine;
    
    private List<KitchenStageReward> stageRewardList;

    private List<int> delayTaskIdList = new List<int>();
    
    public List<CanvasGroup> addOneList;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Sound.PlayMusic( SoundType.SFX_Kitchen_Match_Level_BGM.ToString());
        
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnBuyPackageComplete);
        
        playButton.SetBtnEvent(OnPlayBtnClick);
        closeButton.SetBtnEvent(OnCloseBtnClick);
        explainButton.SetBtnEvent(OnExplainBtnClick);
        greatButton.SetBtnEvent(OnCloseBtnClick_Complete);

        // Load data
        currentTask = KitchenManager.Instance.GetCurrentTaskDatas();

        int praiseNum = KitchenManager.Instance.PraiseNum;
        // 是否存在旧数据
        if (KitchenManager.Instance.OldPraiseNum != -1) // 不存在
        {
            praiseNum = KitchenManager.Instance.OldPraiseNum;
        }
        slider.fillAmount = (float)praiseNum/currentTask.TargetPraise;
        process.text = $"{praiseNum}/{currentTask.TargetPraise}";
        sliderCentre.fillAmount = slider.fillAmount;
        processCentre.text = process.text;

        consumeChefHatNumText.text = $"-{KitchenManager.Instance.GetCurrentTaskConsumeChefHatNum()}";
        
        InitStageReward();

        sliderCentreTrans.gameObject.SetActive(false);

        if (clockBar)
        {
            clockBar.StartCountdown(KitchenManager.Instance.EndTime);
            clockBar.CountdownOver += OnCountDownOver;
        }

        // 加载阶段奖励
        stageRewardList = new List<KitchenStageReward>();
        DTKitchenTaskData taskData = KitchenManager.Instance.TaskData;
        float index = 0;
        for (int i = 0; i < taskData.KitchenTaskDatas.Count; i++)
        {
            DTKitchenTaskDatas data = taskData.KitchenTaskDatas[i];
            KitchenStageReward stageReward = Instantiate(stageRewardPrefab, stageRewardParent).GetComponent<KitchenStageReward>();
            stageReward.gameObject.SetActive(true);
            stageReward.Init(data.ID < currentTask.ID, i == 9, data.ID, GetRewardBg(data.ID),
                GetProgress(data.ID), data.RewardsList, data.RewardsNumList);
            if (data.ID == currentTask.ID)
                index = i;
            stageRewardList.Add(stageReward);
        }
        // 最大值为显示最后一个奖励时，列表左边第一个奖励的编号
        index = Mathf.Min(Mathf.Max(index - 2, 0), 4.5f);
        // 刷新列表的显示位置
        stageRewardParent.localPosition = new Vector3(-155f * index, 0, 0);

        RefreshChefHatNum();
        
        base.OnInit(uiGroup, completeAction, userData);
    }
    
    public int GetRewardBg(int taskId)
    {
        if (currentTask == null || taskId < currentTask.ID)
            return 0;
        if (taskId == currentTask.ID)
            return 1;
        return 2;
    }

    public int GetProgress(int taskId)
    {
        if (currentTask == null || taskId < currentTask.ID)
        {
            if (taskId == 1 || taskId == 10)
            {
                return 2;
            }

            return 3;
        }
        if (taskId == 1 || taskId == 10)
        {
            return 0;
        }
        return 1;
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        if (longTimeInMain != null)
        {
            StopCoroutine(longTimeInMain);
            normalEnterDialogBox.gameObject.SetActive(false);
        }
        
        // KitchenManager.Instance.IsFirstOpenMainMenu = true;
        if (KitchenManager.Instance.IsFirstOpenMainMenu)
        {
            KitchenManager.Instance.IsFirstOpenMainMenu = false;
            SetAllButtonActive(false);
            StartCoroutine(PlayFirstEnterAnimCor());
        }
        else
        {
            sliderCentreTrans.gameObject.SetActive(true);
            string animName = "idle";
            string textTerm = "";
            string audioName = SoundType.SFX_Story_Child_Sigh.ToString();
            float delayTime = 0f;
            switch (GameManager.DataNode.GetData(KitchenManager.ENTER_MAIN_MENU_TYPE, 0))
            {
                case 0:
                    int textIndex = Random.Range(1, 3);// 文案的序号
                    if (Random.Range(0, 2) == 0)
                    {
                        animName = "idle";
                        textTerm = $"Kitchen.NormalOpen{textIndex}";// 两个文案随机
                        audioName = "SFX_Story_Child_fighter_laugh_1";// 没有写入枚举中
                        longTimeIndex = 0;
                    }
                    else
                    {
                        animName = "help";
                        textTerm = $"Kitchen.NormalOpen{2 + textIndex}";// 两个文案随机
                        longTimeIndex = 2;
                    }
                    delayTime = 0;
                    break;
                case 1:// 关卡胜利后回来
                    animName = "good";
                    if (KitchenManager.Instance.PraiseNum >= currentTask.TargetPraise)
                    {
                        // 存在可领取的奖励
                        int index = Random.Range(1, 3);// 文案的序号
                        textTerm = $"Kitchen.NewTask{index}";
                    }
                    else
                    {
                        textTerm = "Kitchen.WinOpen";
                    }
                    audioName = "SFX_Story_Child_fighter_laugh_1";
                    delayTime = 1.3f;
                    longTimeIndex = 0;
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    break;
                case 2:// 关卡失败后回来
                    animName = "worry";
                    textTerm = "Kitchen.FailOpen";
                    delayTime = 1.3f;
                    longTimeIndex = 2;
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    break;
            }

            GameManager.DataNode.RemoveNode(KitchenManager.ENTER_MAIN_MENU_TYPE);
            if(animName.Equals("idle"))
                girlSpine.AnimationState.SetAnimation(0, animName, true);
            else
                girlSpine.AnimationState.SetAnimation(0, animName, false).Complete += entry =>
                {
                    girlSpine.AnimationState.SetAnimation(0, $"{animName}_idle", true);
                };
            // 显示文案
            delayTaskIdList.Add(GameManager.Task.AddDelayTriggerTask(delayTime, () =>
            {
                GameManager.Sound.PlayAudio(audioName);
                normalEnterText.SetTerm(textTerm);
                normalEnterDialogBox.localScale = Vector3.zero;
                normalEnterDialogBox.gameObject.SetActive(true);
                normalEnterDialogBox.DOScale(1, 0.3f);
            }));
            delayTaskIdList.Add(GameManager.Task.AddDelayTriggerTask(1.7f + delayTime, () =>
            {
                normalEnterDialogBox.gameObject.SetActive(false);
                longTimeInMain = LongTimeInMain();
                StartCoroutine(longTimeInMain);
            }));
        
            if (KitchenManager.Instance.OldPraiseNum != -1)
            {
                int getPraiseNum = KitchenManager.Instance.GetAddPraise();
                praiseNumText.text = $"x {getPraiseNum}";
                KitchenManager.Instance.OldPraiseNum = -1;
                SetAllButtonState(false);
                StartCoroutine(PlaySliderAnimCor(getPraiseNum));
            }
        }
    }

    public override void OnReveal()
    {
        gameObject.SetActive(true);
        //base.OnReveal();
    }

    // 第一次进入MainMenu的动画流程
    IEnumerator PlayFirstEnterAnimCor()
    {
        girlSpine.AnimationState.SetAnimation(0, "idle", false);
        firstEnterText.SetTerm("Kitchen.FirstEnter1");
        firstEnterDialogBox.localScale = Vector3.zero;
        firstEnterDialogBox.gameObject.SetActive(true);
        firstEnterDialogBox.DOScale(1, 0.3f);
        yield return new WaitForSeconds(1.9f);// 等待文案出现并展示
        girlSpine.AnimationState.SetAnimation(0, "help", false).Complete += entry =>
        {
            girlSpine.AnimationState.SetAnimation(0, "help_idle", true);
        };
        yield return new WaitForSeconds(0.5f);// 对话框展示中，等待切换的动画播放到目标帧数开始播放手机出现动画
        phoneTrans.gameObject.SetActive(true);
        phoneTrans.DOLocalMoveY(220f, 0.6f);
        yield return new WaitForSeconds(0.6f);// 等待文案展示完成
        firstEnterDialogBox.DOScale(0, 0.3f);
        yield return new WaitForSeconds(0.3f);// 等待文案消失
        
        firstEnterText.SetTerm("Kitchen.FirstEnter2");
        firstEnterDialogBox.DOScale(1, 0.3f);
        yield return new WaitForSeconds(2.8f);// 等待新文案弹出和展示完成
        firstEnterDialogBox.DOScale(0, 0.3f);
        yield return new WaitForSeconds(0.3f);
        
        // 上移文案的位置，显示确认按钮
        bool isClick = false;
        firstEnterText.transform.localPosition = Vector3.up * 197;
        okButton.SetBtnEvent(() =>
        {
            isClick = true;
        });
        okButton.gameObject.SetActive(true);
        firstEnterText.SetTerm("Kitchen.FirstEnter3");
        firstEnterDialogBox.DOScale(1, 0.3f);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => isClick);
        firstEnterText.transform.localPosition = Vector3.up * 175;
        okButton.gameObject.SetActive(false);
        // SetAllButtonActive(true);
        firstEnterDialogBox.DOScale(0, 0.3f);
        
        sliderTrans.SetParent(sliderCentreTrans.parent);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(sliderTrans.DOLocalJump(Vector3.zero, 200, 1, 0.5f));
        sequence.Join(sliderTrans.DOScale(1f, 0.5f));
        yield return new WaitForSeconds(0.5f);// 等待进度条移动动画完成
        rewardCentreTrans.localScale = Vector3.zero;
        rewardCentreTrans.DOScale(Vector3.one * 1.5f, 0.2f).onComplete += () =>
        {
            rewardCentreTrans.DOScale(Vector3.one, 0.1f);
        };
        sliderTrans.gameObject.SetActive(false);
        sliderAnimOverSpine.SetActive(true);
        sliderCentreTrans.gameObject.SetActive(true);
        sequence.Kill();

        yield return new WaitForSeconds(0.3f);// 等待进度条动画播放完毕
        SetAllButtonActive(true);
        ShowGuide();
    }

    private Action playGuideCallback = null;
    public void ShowGuide()
    {
        startGuideDialogBox.gameObject.SetActive(false);
        guide.SetActive(true);
        Transform oldParent = playButton.transform.parent;
        Vector3 oldPos = startGuideDialogBox.localPosition;
        Transform oldParent1 = sliderCentreTrans.parent;
        
        playButton.transform.SetParent(guide.transform);
        chefHatNumText.transform.parent.SetParent(guide.transform);
        sliderCentreTrans.SetParent(guide.transform);
        startGuideDialogBox.localPosition = playButton.transform.localPosition + Vector3.up * 100;
        startGuideDialogBox.localScale = Vector3.zero;
        startGuideDialogBox.gameObject.SetActive(true);
        startGuideDialogBox.DOScale(1, 0.3f);
        playGuideCallback = () =>
        {
            playButton.transform.SetParent(oldParent);
            chefHatNumText.transform.parent.SetParent(oldParent);
            sliderCentreTrans.SetParent(oldParent1);
            startGuideDialogBox.localPosition = oldPos;
            startGuideDialogBox.localScale = Vector3.one;
            startGuideDialogBox.gameObject.SetActive(false);
            guide.SetActive(false);
        };
    }
    
    IEnumerator PlaySliderAnimCor(int getPraiseNum)
    {
        yield return new WaitForSeconds(0.1f);
        
        // 获得的点赞飞到进度条
        flyReward.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        praiseReward.SetParent(sliderCentreTrans);
        flyReward.SetActive(false);
        praiseNumText.gameObject.SetActive(false);
        GameManager.Sound.PlayAudio(SoundType.SFX_Collection_OilDrum_Appear.ToString());
        Sequence sequence = DOTween.Sequence();
        sequence.Append(praiseReward.DOLocalMove(flyTarget.localPosition, 0.5f).SetEase(Ease.InQuart));
        sequence.Join(praiseReward.DOScale(0.55f, 0.5f));
        yield return new WaitForSeconds(0.5f);
        // 还原设置
        praiseReward.SetParent(flyReward.transform);
        praiseReward.localScale = Vector3.one;
        praiseReward.localPosition = Vector3.zero;
        praiseNumText.gameObject.SetActive(true);
        
        StartCoroutine(PlayAddOneAnim(getPraiseNum));
        
        // 播放进度条的动画
        while (true)
        {
            bool cliamedReward = false;
            float fillAmount = Mathf.Min((float)KitchenManager.Instance.PraiseNum / currentTask.TargetPraise, 1);
            float duration = 0.5f;
            sliderCentre.DOFillAmount(fillAmount, duration);
            yield return new WaitForSeconds(duration);
            // 刷新进度的文案显示, 可尝试改为与上方的 DOFillAmount 动画一同改变
            processCentre.text = $"{KitchenManager.Instance.PraiseNum}/{currentTask.TargetPraise}";
            // 当前任务进度已满，领取奖励
            if (KitchenManager.Instance.PraiseNum >= currentTask.TargetPraise)
            {
                GameManager.Sound.PlayAudio("SFX_goldenpass_missioncomplete");
                // 领取奖励后，削减点赞数量
                KitchenManager.Instance.PraiseNum -= currentTask.TargetPraise;
                for (int i = 0; i < currentTask.RewardsList.Count; i++)
                {
                    RewardManager.Instance.AddNeedGetReward(currentTask.RewardsList[i], currentTask.RewardsNumList[i]);
                }
                rewardCentreTrans.gameObject.SetActive(false);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    // 获取下一个任务
                    currentTask = KitchenManager.Instance.AccpetNextTaskDatas();

                    if (currentTask != null)
                    {
                        KitchenManager.Instance.RefreshChallengeNum(currentTask.ChallengeToolNumber);
                        // 清空进度条
                        sliderCentre.fillAmount = 0;
                        processCentre.text = $"0/{currentTask.TargetPraise}";
                        
                        InitStageReward();
                        
                        // 切换奖励图标
                        rewardCentreTrans.localScale = Vector3.zero;
                        rewardCentreTrans.gameObject.SetActive(true);
                        rewardCentreTrans.DOScale(1.1f, 0.15f).onComplete = () =>
                        {
                            rewardCentreTrans.DOScale(Vector3.one, 0.1f).onComplete += () =>
                            {
                                cliamedReward = true;
                            };
                        };
                        
                        // 刷新道具消耗显示
                        consumeChefHatNumText.text = $"-{currentTask.ChallengeToolNumber.ToString()}";
                        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    }
                    else
                    {
                        cliamedReward = true;
                    }
                });
                yield return new WaitUntil(() => cliamedReward);
                if (currentTask == null)
                {
                    RefreshStageReward(-1);
                    // 提前关闭活动
                    KitchenManager.Instance.EndActivity();
                    // 刷新按钮逻辑
                    closeButton.SetBtnEvent(OnCloseBtnClick_Complete);
                    greatButton.gameObject.SetActive(true);
                    playButton.gameObject.SetActive(false);
                    SetAllButtonState(true);

                    sliderCentreTrans.gameObject.SetActive(false);
                    normalEnterText.SetTerm("Kitchen.Complete");
                    normalEnterDialogBox.localScale = Vector3.zero;
                    normalEnterDialogBox.gameObject.SetActive(true);
                    normalEnterDialogBox.DOScale(1, 0.3f);

                    yield break;
                }
                // 更新下方的阶段奖励显示
                RefreshStageReward(currentTask.ID);
            }
            else
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenEntranceUpdate));
                SetAllButtonState(true);
                yield break;
            }
        }
    }

    public void RefreshStageReward(int taskId)
    {
        if(taskId == -1)// 已完成所有任务，只刷新最后一个奖励
        {
            stageRewardList.Last().RefreshToClaimed(true, GetRewardBg(stageRewardList.Last().taskId), GetProgress(stageRewardList.Last().taskId));
            return;
        }
        
        for (int i = 0; i < stageRewardList.Count; i++)
        {
            if (stageRewardList[i].taskId == taskId - 1)
            {
                stageRewardList[i].RefreshToClaimed(false, GetRewardBg(stageRewardList[i].taskId), GetProgress(stageRewardList[i].taskId), 
                    i + 1 == stageRewardList.Count
                        ? null
                        : () =>
                        {
                            stageRewardList[i + 1].Refresh(false,
                                GetRewardBg(stageRewardList[i + 1].taskId), GetProgress(stageRewardList[i + 1].taskId));
                            float index = Mathf.Min(Mathf.Max(i - 1, 0f), 4.5f);
                            stageRewardParent.DOLocalMoveX(-155f * index, 0.5f);
                        });
                break;
            }
            if (stageRewardList[i].taskId > taskId) break;
        }
    }

    // 设置所有按钮的点击状态
    public void SetAllButtonState(bool state)
    {
        closeButton.interactable = state;
        explainButton.interactable = state;
        playButton.interactable = state;
    }

    public void SetAllButtonActive(bool active)
    {
        closeButton.gameObject.SetActive(active);
        explainButton.gameObject.SetActive(active);
        playButton.gameObject.SetActive(active);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        // 开启倒计时
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
        {
            KitchenManager.Instance.AddChefHat(10);
            // RefreshChefHatNum();
            
            // KitchenManager.Instance.ActivityLevelWin(100);
            
            // GameManager.UI.ShowUIForm<KitchenGameWinMenu>();
            // RefreshStageReward(3);
        }
        #endif
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnBuyPackageComplete);
        
        phoneTrans.gameObject.SetActive(false);

        foreach (var item in stageRewardList)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        stageRewardList.Clear();

        foreach (var id in delayTaskIdList)
        {
            GameManager.Task.RemoveDelayTriggerTask(id);
        }
        delayTaskIdList.Clear();

        StopAllCoroutines();
        
        base.OnRelease();
    }

    public void OnPlayBtnClick()
    {
        // 判断是否有足够的道具，并进行消耗
        if (KitchenManager.Instance.UseChefNumOpenLevel())
        {
            RefreshChefHatNum();
            // 通知入口刷新红点显示
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Start);
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm("KitchenGameMenu");
        }
        else
        {
            // 开启获得更多厨师帽界面
            GameManager.UI.ShowUIForm("KitchenGetMoreMenu");
        }

        if (playGuideCallback != null)
        {
            playGuideCallback?.InvokeSafely();
        }
    }

    private void OnBuyPackageComplete(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;

        if (ne != null && ne.Type == CommonEventType.KitchenBuyPackageComplete)
        {
            // playButton.onClick?.Invoke();
            RefreshChefHatNum();
        }
    }
    
    /// <summary>
    /// 通常的关闭逻辑
    /// </summary>
    public void OnCloseBtnClick()
    {
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.CheckKitchen);
    }

    /// <summary>
    /// 完成所有任务后的界面关闭逻辑
    /// </summary>
    public void OnCloseBtnClick_Complete()
    {
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        // 刷新入口
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
        GameManager.UI.HideUIForm(this);
        GameManager.UI.ShowUIForm("KitchenEndMenu");
    }
    
    public void OnExplainBtnClick()
    {
        GameManager.UI.ShowUIForm("KitchenExplainMenu");
    }
    
    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        OnCloseBtnClick();
    }

    public void InitStageReward()
    {
        // 设置进度条和阶段奖励的位置
        sliderCentreTrans.localPosition = Vector3.zero;
        rewardCentre[0].transform.localPosition = Vector3.zero;
        rewardCentreTrans.localPosition = new Vector3(229.3f, -49.5f, 0);
        if (currentTask.RewardsList.Count > 1)
        {
            sliderCentreTrans.localPosition = Vector3.left * 74f;
            rewardCentre[0].transform.localPosition = Vector3.left * 74f;
            rewardCentreTrans.localPosition += Vector3.right * 74;
        }
        
        for (int i = 0; i < rewardCentre.Length; i++)
        {
            if (currentTask.RewardsList.Count > i)
            {
                rewardCentre[i].gameObject.SetActive(true);
                rewardCentre[i].OnInit(currentTask.RewardsList[i], currentTask.RewardsNumList[i]);
            }
            else
            {
                rewardCentre[i].gameObject.SetActive(false);
            }
        }
    }

    public void RefreshChefHatNum()
    {
        chefHatNumText.text = KitchenManager.Instance.ChefHatNum.ToString();
    }

    private IEnumerator longTimeInMain = null;
    private int longTimeIndex = 0;
    IEnumerator LongTimeInMain()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            int index = Random.Range(1, 3) + longTimeIndex;
            normalEnterText.SetTerm($"Kitchen.NormalOpen{index.ToString()}");
            normalEnterDialogBox.localScale = Vector3.zero;
            normalEnterDialogBox.gameObject.SetActive(true);
            normalEnterDialogBox.DOScale(1, 0.3f);
            yield return new WaitForSeconds(3f);
            normalEnterDialogBox.DOScale(0, 0.3f);
        }
        yield break;
    }
    
    IEnumerator PlayAddOneAnim(int getPraiseNum)
    {
        int i = 0;
        while (i < 5)
        {
            CanvasGroup cg = addOneList[i % 4];
            cg.alpha = 1;
            Transform cgTrans = cg.transform;
            cgTrans.localScale = Vector3.one;
            cgTrans.localPosition = Vector3.zero;
            cgTrans.SetAsLastSibling();
            cg.gameObject.SetActive(true);
            Sequence sq = DOTween.Sequence();
            sq.Append(cg.DOFade(0, 0.35f));
            sq.Join(cgTrans.DOScale(0.5f, 0.35f));
            sq.Join(cgTrans.DOLocalMoveY(60, 0.35f));
            flyTarget.DOKill();
            flyTarget.DOPunchScale(Vector3.one * 0.33f, 0.1f, 1, 0f);
            yield return new WaitForSeconds(0.1f);
            i++;
        }
        yield break;
    }
}
