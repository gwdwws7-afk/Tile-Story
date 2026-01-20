using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GlacierQuestMenu : UIForm
{
    [SerializeField] private DelayButton explainButton, closeButton, playButton;
    [SerializeField] private TextMeshProUGUI levelText, peopleText, failText;
    [SerializeField] private TextMeshProUGUILocalize normalText;
    [SerializeField] private Transform headIconArea, failHeadIconArea;
    // 玩法说明界面
    [SerializeField] private GameObject headIcon;
    [SerializeField] private GameObject guide;
    [SerializeField] private TextMeshProUGUI tapTip;
    [SerializeField] private TextMeshProUGUILocalize guideText;
    [SerializeField] private Transform guideTip;
    //引导的对象
    [SerializeField] private GameObject guideTarget1, guideTarget2, guideTarget3;
    [SerializeField] private ClockBar clockBar;
    public GlacierQuestPlatform[] targetPos;
    [SerializeField] private GameObject gameWin, gameFail;
    [SerializeField] private Transform InfoPanel;
    private GameObject[] icons = new GameObject[14];

    // 是否播放头像跳动动画
    private bool playHeadIconAnim = false;
    // 是否开始播放动画
    private bool startAnim = false;// TODO:添加对应的动画逻辑
    private bool clickEvent = false;
    private int iconNum = 11;
    private int speed = 0;
    private int oldPeople, curPeople;
    private int oldLevel = -1;

    private List<AsyncOperationHandle> recordHandles = new List<AsyncOperationHandle>();

    private Action closeEvent;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        if (GameManager.Task.GlacierQuestTaskManager.isInGame)
        {
            CloseButtonClick();
            GameManager.Task.AddDelayTriggerTask(0.5f, () => clickEvent = true);
        }
        
        GameManager.Sound.PlayMusic("Glacier_Bgm");

        explainButton.SetBtnEvent(OnClickExplainBtn);
        closeButton.SetBtnEvent(OnClickCloseBtn);
        playButton.SetBtnEvent(OnClickPlayBtn);
        tapTip.color = new Color(1, 1, 1, 0);
        tapTip.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(!GameManager.Task.GlacierQuestTaskManager.isInGame);
        explainButton.gameObject.SetActive(!GameManager.Task.GlacierQuestTaskManager.isInGame);

        // 提前隐藏地台
        int num = GameManager.Task.GlacierQuestTaskManager.OldLevel;
        if (num == 0)
            num = GameManager.Task.GlacierQuestTaskManager.CurLevel;
        for (int i = 0; i < targetPos.Length; i++)
        {
            targetPos[i].Init();
            targetPos[i].gameObject.SetActive(i == 0 || i >= num);
        }

        GameManager.Ads.HideBanner("GlacierQuest");

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        clockBar.OnReset();
        if (GameManager.Task.GlacierQuestTaskManager.CheckIsOpen())
        {
            DateTime endDate = GameManager.Task.GlacierQuestTaskManager.EndTime;
            clockBar.StartCountdown(endDate);
        }
        else
        {
            clockBar.SetFinishState();
        }
        switch (GameManager.Task.GlacierQuestTaskManager.ActivityState)
        {
            case GlacierQuestState.Open:
            case GlacierQuestState.Clear:
            case GlacierQuestState.ClearTime:
                ChangeTopPanel(true, GameManager.Task.GlacierQuestTaskManager.CurLevel);
                //显示倒计时
                clockBar.StartCountdown(GameManager.Task.GlacierQuestTaskManager.EndTime);
                //当值发生变化时
                if (GameManager.Task.GlacierQuestTaskManager.OldPeople != 0)
                {
                    //动画展示
                    oldLevel = GameManager.Task.GlacierQuestTaskManager.OldLevel;
                    levelText.text = $"{oldLevel}/7";
                    oldPeople = GameManager.Task.GlacierQuestTaskManager.OldPeople;
                    curPeople = GameManager.Task.GlacierQuestTaskManager.CurPeople;
                    peopleText.text = $"{oldPeople}/100";
                    //显示头像
                    InitHeadIcon(oldPeople, oldLevel, 1, oldPeople - curPeople);
                    //调用动画展示，延时等待场景跳转时的云散开
                    GameManager.Task.AddDelayTriggerTask(0.5f, PlayAnim);
                }
                else
                {
                    oldLevel = GameManager.Task.GlacierQuestTaskManager.CurLevel;
                    levelText.text = $"{oldLevel}/7";
                    oldPeople = GameManager.Task.GlacierQuestTaskManager.CurPeople;
                    peopleText.text = $"{oldPeople}/100";
                    //显示头像
                    InitHeadIcon(oldPeople, oldLevel, 0);
                    if (GameManager.Scene.SceneType != SceneType.Game)
                        playButton.gameObject.SetActive(true);
                }
                //隐藏地台
                // for (int i = 0; i < targetPos.Length; i++)
                // {
                //     targetPos[i].Init();
                //     targetPos[i].gameObject.SetActive(i >= oldLevel);
                // }
                break;
            case GlacierQuestState.Time://失败进入冷却
            case GlacierQuestState.Close:
                ChangeTopPanel(false, GameManager.Task.GlacierQuestTaskManager.CurLevel);
                oldPeople = GameManager.Task.GlacierQuestTaskManager.OldPeople;
                curPeople = GameManager.Task.GlacierQuestTaskManager.CurPeople;
                oldLevel = GameManager.Task.GlacierQuestTaskManager.OldLevel;
                //隐藏地台
                // for (int i = 0; i < targetPos.Length; i++)
                // {
                //     targetPos[i].Init();
                //     targetPos[i].gameObject.SetActive(i >= oldLevel);
                // }
                failHeadIconArea.SetSiblingIndex(9);
                InitHeadIcon(oldPeople, oldLevel, 2, oldPeople - curPeople);
                GameManager.Task.AddDelayTriggerTask(0.5f, PlayFailAnim);
                break;
        }

        if (GameManager.Task.GlacierQuestTaskManager.isInGame)
            GameManager.Task.GlacierQuestTaskManager.IsShowGuide = false;

        if (GameManager.Task.GlacierQuestTaskManager.IsShowGuide)
        {
            // 展示活动引导
            ShowGuide();
        }

        base.OnShow(showSuccessAction, userData);
    }

    public void CloseButtonClick()
    {
        explainButton.interactable = false;
        closeButton.interactable = false;
        playButton.interactable = false;
    }

    public void OpenButtonClick()
    {
        explainButton.interactable = true;
        closeButton.interactable = true;
        playButton.interactable = true;
    }

    float animTime = 0;
    List<Transform> headIcons = new List<Transform>();
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
        {
            ShowGuide();
            //playHeadIconAnim = true;
            //PlayHeadIconAnim(true, GameManager.Task.GlacierQuestTaskManager.CurLevel);
            //SkeletonGraphic ske = headIcon.GetComponentInChildren<SkeletonGraphic>();
            //ske.Initialize(false);
            //ske.timeScale = 1;
            //ske.AnimationState.SetAnimation(0, "02", false);
            //ske.DOPlayForward();
        }
#endif
        //是否播放人数减少动画
        if (startAnim)
        {
            animTime += Time.deltaTime;
            float show = oldPeople - speed * animTime;
            if (show <= curPeople)
            {
                show = curPeople;
                startAnim = false;
                peopleText.transform.DOScale(1, 0.2f).onComplete += () =>
                {
                    //播放特效
                    PunchAnimPlayInTarget(peopleText.transform, 100, Vector3.zero);
                    animTime = 0;
                };
            }
            peopleText.text = $"{(int)show}/100";
        }
        //播放头像跳动动画时刷新 HeaaIconArea 中头像位置
        if (playHeadIconAnim)
        {
            if (headIcons.Count == 0)
            {
                for (int i = 0; i < headIconArea.childCount; i++)
                {
                    headIcons.Add(headIconArea.GetChild(i));
                }
            }
            if (headIcons.Count > 0)
                headIcons.Sort((Transform t1, Transform t2) => { return t1.position.z.CompareTo(t2.position.z); });
            for (int i = 0; i < headIcons.Count; i++)
            {
                headIcons[i].SetSiblingIndex(headIcons.Count - 1 - i);
            }
        }
        else
        {
            headIcons.Clear();
        }

        if (GameManager.Task.GlacierQuestTaskManager.isInGame)//!isPause && 
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                if (clickEvent && GameManager.Task.GlacierQuestTaskManager.ActivityState != GlacierQuestState.Clear &&
                    GameManager.Task.GlacierQuestTaskManager.ActivityState != GlacierQuestState.ClearTime)
                {
                    clickEvent = false;
                    OnClickCloseBtn();
                }
            }
        }
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        GameManager.Sound.PlayMusic(GameManager.PlayerData.BGMusicName);
        GameManager.Ads.ShowBanner("GlacierQuest");
        // 回收头像
        for (int i = 0; i < icons.Length; i++)
            Destroy(icons[i]);
        icons = new GameObject[14];
        // 重置地台
        for (int i = 0; i < targetPos.Length; i++)
        {
            targetPos[i].Init();
            targetPos[i].gameObject.SetActive(true);
        }

        // 还原失败头像层级
        failHeadIconArea.SetSiblingIndex(8);

        startAnim = false;
        playHeadIconAnim = false;
        clickEvent = false;

        headSprites.Clear();
        foreach (var handle in recordHandles)
        {
            UnityUtility.UnloadAssetAsync(handle);
        }
        recordHandles.Clear();

        GameManager.Process.EndProcess(ProcessType.GlacierQuest);
        base.OnRelease();
    }

    public void OnClickExplainBtn()
    {
        GameManager.UI.ShowUIForm("GlacierQuestPlayGuideMenu");
    }

    public void OnClickCloseBtn()
    {
        OpenButtonClick();
        if (closeEvent == null) // 判断是否在局内，局内等待场景切换隐藏gameObject
        {
            GameManager.Sound.PlayUICloseSound();
            GameManager.UI.HideUIForm(this);
        }
        if (GameManager.Task.GlacierQuestTaskManager.ActivityState != GlacierQuestState.Clear && GameManager.Task.GlacierQuestTaskManager.ActivityState != GlacierQuestState.ClearTime)
        {
            closeEvent?.Invoke();
            closeEvent = null;
        }
    }

    public void OnClickPlayBtn()
    {
        GameManager.Sound.PlayMusic(GameManager.PlayerData.BGMusicName);

        GameManager.Process.UnregisterAll();
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockAddOneStepBoostLevel)
        {
            ProcedureUtil.ProcedureMapToGame();
        }
        else
        {
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
        }
    }

    [HideInInspector]
    public bool showAnim = false;
    //修改顶板
    void ChangeTopPanel(bool show = true, int level = 0)
    {
        gameWin.SetActive(false);
        gameFail.SetActive(false);

        // 使得通过的关卡数大于7时也可以结束
        if (level > 0)
        {
            normalText.SetTerm("GlacierQuest.MapTips2");
        }
        if (level >= 7)
        {
            normalText.SetTerm("GlacierQuest.MapTips3");
            clockBar.gameObject.SetActive(false);
        }
        else
        {
            clockBar.gameObject.SetActive(true);
        }

        if (!showAnim)
        {
            gameWin.SetActive(show);
            gameFail.SetActive(!show);
        }
        else
        {
            InfoPanel.localScale = Vector3.zero;
            if (show)
            {
                gameWin.SetActive(true);
            }
            else
            {
                gameFail.SetActive(true);
            }
            InfoPanel.DOScale(Vector3.one, 0.3f);
        }
        showAnim = false;
    }

    /// <summary>
    /// 显示地台上的头像
    /// </summary>
    /// <param name="peopleNum">当前剩余数量</param>
    /// <param name="index">当前所处的编号</param>
    /// <param name="isNeedFail">是否需要创建挑战失败头像  0 无反应  1 胜利  2 失败</param>
    /// <param name="failPeopleNum">失败的人数</param>
    private void InitHeadIcon(int peopleNum, int index = 0, int isNeedFail = 0, int failPeopleNum = 0)
    {
        iconNum = Mathf.Min(11, peopleNum);
        // 最后一层只显示六个头像
        if (index == 6)
        {
            iconNum = isNeedFail == 0 ? iconNum : 6;
        }
        int[] headIds = GameManager.Task.GlacierQuestTaskManager.HeadIds;
        GlacierQuestPlatform platform = targetPos[index];
        Transform success = platform.success;
        if (success != null)
        {
            //创建挑战失败的头像
            if (isNeedFail != 0)
            {
                if (platform.fail != null)
                {
                    failPeopleNum = Mathf.Min(3, failPeopleNum);
                    int num = isNeedFail == 1 ? failPeopleNum : failPeopleNum - 1;
                    for (int i = 1; i <= num; i++)
                    {
                        GlacierQuestHeadIcon head = null;
                        GameObject go = Instantiate(headIcon, failHeadIconArea);
                        go.name = $"FailHeadIcon_{i}";
                        icons[icons.Length - i] = go;
                        go.SetActive(true);
                        if (iconNum + 3 > 11)
                        {
                            go.transform.position = success.GetChild((isNeedFail - 2 + i) * 4).position;//失败头像位置 0，4，8
                        }
                        else
                        {
                            go.transform.position = success.GetChild(iconNum - i + 1).position;
                        }
                        int range = UnityEngine.Random.Range(0, Mathf.Min(peopleNum, headIds.Length));
                        if (!head)
                            head = go.GetComponent<GlacierQuestHeadIcon>();
                        ShowHeadIcon(head.headIcon, headIds[range], false);
                        //开启遮罩
                        head.mask.enabled = true;
                    }
                }
                else
                {
                    Log.Info($"GlacierQuest : Not found fail target position in {index}");
                }
            }
            //创建胜利的头像
            for (int i = 0; i < iconNum; i++)
            {
                //生成对应数量的头像,并对头像进行实例化
                GameObject go = Instantiate(headIcon, headIconArea);
                go.name = $"HeadIcon_{i}";
                icons[iconNum - 1 - i] = go;
                go.SetActive(true);
                var head = go.transform.GetComponent<GlacierQuestHeadIcon>();
                if (iconNum - i - 1 == 0)
                {
                    ShowHeadIcon(head.headIcon, headIds[0], true);
                    // 更换自己的头像框
                    head.skele.AnimationState.SetAnimation(0, "04", false);
                    head.skele.AnimationState.TimeScale = 0;
                }
                else
                {
                    ShowHeadIcon(head.headIcon, headIds[UnityEngine.Random.Range(0, Mathf.Min(peopleNum, headIds.Length))], false);
                }
                Vector3 pos = success.transform.GetChild(i).position;
                pos.z = (iconNum - i - 1) * 5;
                go.transform.position = pos;
                if (iconNum - 1 - i == 0 && isNeedFail == 2)
                {
                    go.transform.SetParent(failHeadIconArea);
                    go.transform.GetChild(0).GetComponent<Mask>().enabled = true;
                }
            }
        }
    }

    void PlayAnim()
    {
        Log.Info("GlacierQuest：播放胜利动画");
        //记录人数差值，用于人数跳动动画
        speed = (oldPeople - curPeople) / 1;//除数为消耗的时间 单位 s
        //播放头像跳动动画
        if (oldLevel == 2 || oldLevel == 5)
        {
            playHeadIconAnim = true;
        }
        PlayHeadIconAnim(true, oldLevel, () => peopleText.transform.DOScale(1.5f, 0.2f).onComplete += () => startAnim = true, () =>
          {
              //提高失败头像层级
              // if(oldLevel != 0)
              //     failHeadIconArea.SetSiblingIndex(9);
              if (oldLevel == 0)
                  failHeadIconArea.SetSiblingIndex(8);
              //播放失败头像和地台消失动画
              for (int i = 1; i <= 3; i++)
              {
                  if (icons[icons.Length - i] != null)
                  {
                      //播放头像下沉动画
                      icons[icons.Length - i].GetComponent<GlacierQuestHeadIcon>().Sink(-130, 2.2f);
                  }
              }

              //判断是否需要领取奖励
              if (GameManager.Task.GlacierQuestTaskManager.IsCanClaimedReward)//(GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Clear)
              {
                  // 等待动画播放
                  GameManager.Task.AddDelayTriggerTask(3f, () =>
                    {
                        //发放奖励，活动进入冷却
                        Log.Info("活动挑战成功");
                        GameManager.UI.ShowUIForm("GlacierQuestRewardSet",form =>
                        {
                            (form as GlacierQuestRewardSet).SetClaimEvent(closeEvent);
                            closeEvent = null;
                        });
                        OnClickCloseBtn();
                        //延迟清除旧数据，防止奖励未领取游戏崩溃
                        GameManager.Task.GlacierQuestTaskManager.ClearOldData();
                    });
              }
              else
              {
                  //清除老数据，方便下次进入
                  GameManager.Task.GlacierQuestTaskManager.ClearOldData();

                  if (GameManager.Scene.SceneType != SceneType.Game)
                  {
                      playButton.gameObject.SetActive(true);
                  }
                  else
                  {
                      tapTip.DOFade(1, 0.4f).SetDelay(0.2f);
                      tapTip.gameObject.SetActive(true);
                  }
              }
          });
        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            //放大并修改进度
            levelText.transform.DOScale(1.5f, 0.3f).onComplete += () =>
            {
                levelText.text = $"{GameManager.Task.GlacierQuestTaskManager.CurLevel}/7";
                levelText.transform.DOScale(1, 0.2f).onComplete += () =>
                {
                    //播放特效
                    // EffectManager.Instance.PunchAnimPlayInTarget(levelText.transform, 100, Vector3.zero);
                };
            };
        });
    }

    void PlayFailAnim()
    {
        Log.Info("GlacierQuest：播放失败头像动画");
        oldLevel = GameManager.Task.GlacierQuestTaskManager.OldLevel;
        if (oldLevel == 2 || oldLevel == 5)
        {
            playHeadIconAnim = true;
        }
        //播放头像跳动动画
        PlayHeadIconAnim(false, oldLevel, null, () =>
        {
            if (oldLevel == 0)
                failHeadIconArea.SetSiblingIndex(8);
            //播放失败头像和地台消失动画
            //自己的头像,需要在自己的头像下沉动画播放结束后，关闭屏幕锁定逻辑
            icons[0].GetComponent<GlacierQuestHeadIcon>().Sink(-130, 2.2f);
            for (int i = 1; i < 3; i++)
            {
                //播放头像下沉动画
                icons[icons.Length - i].GetComponent<GlacierQuestHeadIcon>().Sink(-130, 2.2f);
            }
        });
        tapTip.DOFade(1, 0.4f).SetDelay(0.2f);
        tapTip.gameObject.SetActive(true);
        //活动失败后清除当期活动数据
        GameManager.Task.GlacierQuestTaskManager.ClearOldData();
    }

    /// <summary>
    /// 播放头像跳动动画
    /// </summary>
    /// <param name="isSuccess">是否胜利</param>
    /// <param name="level">当前的通关数</param>
    /// <param name="callback1">失败头像开始播放动画的回调</param>
    /// <param name="callback2">失败头像消失后的回调</param>
    private void PlayHeadIconAnim(bool isSuccess, int level, Action callback1 = null, Action callback2 = null)
    {
        //延迟显示失败头像
        GameManager.Task.AddDelayTriggerTask(0.4f, () =>
        {
            Log.Info("GlacierQuest：显示失败头像");
            for (int i = 1; i <= 3; i++)
            {
                if (icons[icons.Length - i] == null)
                {
                    Log.Info($"GlacierQuest：头像{i}为空");
                    break;
                }
                icons[icons.Length - i].SetActive(true);
            }
        });
        float jumpH = 100;
        if (level == 2 || level == 5)
            jumpH = 170;

        Transform success = targetPos[level + 1].success;
        if (success != null)
        {
            Log.Info($"GlacierQuest：播放胜利头像动画");
            GameManager.Sound.PlayAudio("Glacier_Stage_Change");
            int headCount = headIconArea.childCount, successCount = success.childCount;
            for (int i = 0; i < headCount; i++)
            {
                float delayTime = 0;
                if (i > 0)
                {
                    delayTime = (isSuccess ? 0.5f : 0) + i * 0.01f + (int)(i / 3) * 0.1f;
                }
                RectTransform headTrans = headIconArea.GetChild(headCount - 1 - i).GetComponent<RectTransform>();
                Vector2 target = success.GetChild(successCount - 1 - i).GetComponent<RectTransform>().anchoredPosition;
                float targetZ = 350 + i * 35;
                string animName = "02";
                if (isSuccess && i == 0)
                    animName = "04";
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                {
                    if (playHeadIconAnim)
                    {
                        // 跳动的头像存在层级变化,用z轴的值控制头像的层级
                        Sequence seq = DOTween.Sequence();
                        seq.Append(headTrans.DOJumpAnchorPos(target, jumpH, 1, 0.5f));
                        seq.Join(headTrans.DOMoveZ(targetZ, 0.5f));
                        seq.onComplete += () =>
                        {
                            SkeletonGraphic ske = headTrans.GetComponentInChildren<SkeletonGraphic>();
                            ske.Initialize(false);
                            ske.timeScale = 1;
                            ske.AnimationState.SetAnimation(0, animName, false);
                            ske.DOPlayForward();
                        };
                        seq.PlayForward();
                    }
                    else
                    {
                        // 跳动的头像不存在层级变化
                        headTrans.DOJumpAnchorPos(target, jumpH, 1, 0.5f).onComplete += () =>
                        {
                            SkeletonGraphic ske = headTrans.GetComponentInChildren<SkeletonGraphic>();
                            ske.Initialize(false);
                            ske.timeScale = 1;
                            ske.AnimationState.SetAnimation(0, animName, false);
                            ske.DOPlayForward();
                        };
                    }
                });
            }

            Transform fail = targetPos[level].fail;
            if (fail != null)
            {
                Log.Info($"GlacierQuest：播放失败头像动画");
                float delayTimeFail = (isSuccess ? 0.5f : 0) + (iconNum - 2) * 0.01f + (int)(Mathf.Max(0, (iconNum - 2)) / 3) * 0.1f;
                //播放失败动画
                GameManager.Task.AddDelayTriggerTask(delayTimeFail + 0.3f, () =>
                {
                    failHeadIconArea.SetSiblingIndex(9);
                    if (level > 0)
                    {
                        targetPos[level].PlayAnim();
                    }
                    GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                    {
                        callback1?.Invoke();
                        callback1 = null;
                        Log.Info($"GlacierQuest：开始播放失败头像动画  {failHeadIconArea.childCount}");
                        for (int i = 0; i < failHeadIconArea.childCount; i++)
                        {
                            RectTransform headIcon = failHeadIconArea.GetChild(i).GetComponent<RectTransform>();
                            // 获取失败头像的目标位置
                            Vector2 target = fail.GetChild(i).GetComponent<RectTransform>().anchoredPosition;
                            headIcon.DOJumpAnchorPos(target, jumpH, 1, 0.5f);
                            // float rotz = UnityEngine.Random.Range(-5, 5);
                            // if (rotz < 0) rotz -= 15;
                            // else rotz += 15;
                            // headIcon.GetChild(0).GetChild(0).DOLocalRotate(new Vector3(0, 0, rotz), 0.5f);
                        }

                        // 等待失败头像跳跃动画播放完毕
                        GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                        {
                            callback2?.Invoke();
                            callback2 = null;
                        });
                    });
                });
            }
            else
            {
                Log.Error($"GlacierQuest : 目标台阶 {level} 没有节点 fail ");
            }
        }
        else
        {
            Log.Error($"GlacierQuest : 目标台阶 {level + 1} 没有节点 success ");
        }
    }

    private Dictionary<int, Sprite> headSprites = new Dictionary<int, Sprite>();
    // 异步加载头像
    public void ShowHeadIcon(Image image, int headId, bool isChoose)
    {
        if (!headSprites.TryGetValue(headId, out Sprite sprite))
        {
            string headName = $"HeadPortrait_{headId}{(isChoose ? $"_{headId}" : "")}";
            var handle= UnityUtility.LoadAssetAsync<Sprite>(headName, sprite =>
            {
                if (!headSprites.Keys.Contains(headId))
                    headSprites.Add(headId, sprite);
                image.sprite = headSprites[headId];
            });
            recordHandles.Add(handle);
        }
        else
        {
            image.sprite = sprite;
        }
    }

    private void PunchAnimPlayInTarget(Transform parent, float size, Vector3 delta = default)
    {
        // var g = Instantiate(punchEffect, parent);
        // g.gameObject.SetActive(true);
        // g.transform.localPosition = delta;
        // g.GetComponent<Coffee.UIExtensions.UIParticle>().scale = size;
        // g.GetComponentInChildren<ParticleSystem>().Play(true);
        // GameManager.Task.AddDelayTriggerTask(10f, () =>
        // {
        //     if (g != null)
        //     {
        //         Destroy(g);
        //     }
        // });
    }

    public void SetCloseEvent(Action action)
    {
        closeEvent = action;
    }

    private void ShowGuide()
    {
        guide.GetComponent<Image>().DOFade(0, 0);
        playButton.gameObject.SetActive(false);
        guide.SetActive(true);
        guideTarget1.SetActive(false);
        guideTarget2.SetActive(false);
        guideTarget3.SetActive(false);
        guide.GetComponent<Image>().DOFade(0.8f, 0.5f).onComplete += () =>
        {
            //第一个引导
            guideTarget1.SetActive(true);
            //提高头像的层级
            for (int i = icons.Length - 1; i >= 0; i--)
            {
                if (icons[i] == null) continue;
                icons[i].transform.SetParent(guideTarget1.transform);
            }
            guideTip.gameObject.SetActive(true);
            Vector3 pos = guideTarget1.transform.localPosition;
            pos.x += 100;
            pos.y += guideTarget1.GetComponent<RectTransform>().rect.height / 2 + 45;
            guideTip.localPosition = pos;
            guideText.SetTerm("GlacierQuest.Guide1");
            DelayButton btn = guide.GetComponentInChildren<DelayButton>();
            btn.enabled = false;
            btn.SetBtnEvent(() =>
            {
                //恢复头像层级
                for (int i = icons.Length - 4; i >= 0; i--)
                {
                    if (icons[i] == null) continue;
                    icons[i].transform.SetParent(headIconArea);
                }
                guideTarget1.SetActive(false);
                //第二个引导
                btn.enabled = false;
                GameManager.Task.AddDelayTriggerTask(0.2f, () => btn.enabled = true);
                guideText.SetTerm("GlacierQuest.Guide2");
                pos = guideTarget2.transform.localPosition;
                pos.y += guideTarget2.GetComponent<RectTransform>().rect.height / 2 + 20;
                guideTip.localPosition = pos;
                guideTarget2.SetActive(true);

                btn.SetBtnEvent(() =>
                {
                    guideTarget2.SetActive(false);
                    //第三个引导
                    btn.enabled = false;
                    GameManager.Task.AddDelayTriggerTask(0.2f, () => btn.enabled = true);
                    guideText.SetTerm("GlacierQuest.Guide3");
                    pos = guideTarget3.transform.localPosition;
                    pos.y += guideTarget3.GetComponent<RectTransform>().rect.height / 2 + 20;
                    guideTip.localPosition = pos;
                    guideTarget3.SetActive(true);

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        guideTarget3.SetActive(false);
                        guide.SetActive(false);
                        guideTip.gameObject.SetActive(false);
                        GameManager.Task.GlacierQuestTaskManager.IsShowGuide = false;
                        playButton.gameObject.SetActive(true);
                    });
                });
            });
            GameManager.Task.AddDelayTriggerTask(0.155f, () => btn.enabled = true);
        };
    }
}
