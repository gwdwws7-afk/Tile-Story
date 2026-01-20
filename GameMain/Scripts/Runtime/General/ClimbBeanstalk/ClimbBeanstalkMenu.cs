using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ClimbBeanstalkMenu : CenterForm, ICustomOnEscapeBtnClicked
{
    public SoundType bgMusic = SoundType.None;
    [SerializeField]
    private DelayButton closeBtn;
    [SerializeField]
    protected ItemPromptBox itemPromptBox;
    [SerializeField]
    public ScrollArea scrollArea;
    [SerializeField]
    private SafeArea safeArea;
    [SerializeField]
    protected Transform backScene, treeTop, grassLand;
    [SerializeField]
    protected Transform myPortraitTransform;
    [SerializeField]
    private Image myPortraitImage;

    [SerializeField]
    private GameObject stageTextEffect;
    [SerializeField]
    private DelayButton playButton;

    [SerializeField]
    private TextMeshProUGUILocalize phaseText;

    [SerializeField]
    Image[] imagesNeedToRelease;

    private bool isShowGuide;

    // 在预设中修改对应的名称
    public string NORMAL_COLUMN_PREFAB_NAME = "ClimbBeanstalkGO";
    public string LAST_COLUMN_PREFAB_NAME = "ClimbBeanstalkDestinationGO";

    private float backSceneRatio = 0.7f;
    private float backSceneOriginY;
    private AsyncOperationHandle handle;

    protected float topBoundary, bottomBoundary;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        topBoundary = 1.04f;
        bottomBoundary = -0.03f;

        if (bgMusic != SoundType.None)
            GameManager.Sound.PlayMusic(bgMusic.ToString());

        closeBtn.SetBtnEvent(OnCloseBtnClicked);
        playButton.SetBtnEvent(OnPlayBtnClicked);

        List<ClimbBeanstalkTaskDatas> dataList = ClimbBeanstalkManager.Instance.DataTable.GetRecentClimbBeanstalkTaskDatas();

        int currentStage = ClimbBeanstalkManager.Instance.LastWinStreakNum;
        int currentIndex = dataList.Count - currentStage;

        for (int i = dataList.Count - 1; i >= 0; i--)
        {
            if (i == dataList.Count - 1)
                scrollArea.AddColumnLast(new ClimbBeanstalkScrollColumn(LAST_COLUMN_PREFAB_NAME, dataList[i], scrollArea.recycleWidth + 95, this));
            else
                scrollArea.AddColumnLast(new ClimbBeanstalkScrollColumn(NORMAL_COLUMN_PREFAB_NAME, dataList[i], scrollArea.recycleWidth, this));
        }
        safeArea.Refresh();
        scrollArea.currentIndex = currentIndex;// + 1;
        scrollArea.OnSpawnAction += column =>
        {
            RefreshLayout();
        };
        scrollArea.SetCenterDelta(0.1f / dataList.Count * 2);
        scrollArea.OnInit(GetComponent<RectTransform>());


        backScene.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        backSceneOriginY = backScene.transform.position.y;
        // backScene.position = new Vector3(0, backSceneOriginY - scrollArea.scrollRect.verticalNormalizedPosition * backSceneRatio, 0);
        // 设置 position 的描点为中心点，所以设置的位置需要下移 Screen.height/2
        backScene.localPosition = new Vector3(0, -scrollArea.scrollRect.verticalNormalizedPosition * (4320f - Screen.height - 800f) - Screen.height / 2, 0);

        scrollArea.scrollRect.onValueChanged.RemoveAllListeners();
        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);

        //因为起始点有偏移，所以滚动栏Y轴直接置为0
        if (currentIndex == dataList.Count - 1)
            scrollArea.scrollRect.verticalNormalizedPosition = 0;

        //调整草地与树顶的位置
        if (treeTop)
        {
            grassLand.localPosition = new Vector3(0, scrollArea.GetColumnLocalPosition(dataList.Count - 1).y - 445, 0);
            treeTop.localPosition = new Vector3(0, scrollArea.GetColumnLocalPosition(0).y + 440, 0);
        }

        myPortraitTransform.localPosition = new Vector3(0, GetSliderY(currentStage), 0);


        //设置我的头像
        UnityUtility.UnloadAssetAsync(handle);
        string headPortraitName = $"HeadPortrait_{GameManager.PlayerData.HeadPortrait}_{GameManager.PlayerData.HeadPortrait}";
        handle = UnityUtility.LoadAssetAsync<Sprite>(headPortraitName, (s) =>
          {
              myPortraitImage.sprite = s;
          });

        phaseText.SetParameterValue("0", ClimbBeanstalkManager.Instance.CurrentPhaseID.ToString());


        //展示教程
        if (!ClimbBeanstalkManager.Instance.EverShowedIntro && !isShowGuide && ClimbBeanstalkManager.Instance.CurrentWinStreak == 0)
        {
            isShowGuide = true;
            ShowClimbBeanstalkGuide();
        }
        else
        {
            isShowGuide = false;
        }

        //主动刷新一次 确保玩家看到的时候已经是正确的背景高度
        OnScrollViewChanged(Vector2.zero);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        ShowRiseOrDropAnim();
        //主动刷新一次 确保玩家看到的时候已经是正确的背景高度
        OnScrollViewChanged(Vector2.zero);

        base.OnShowInit(showInitSuccessAction, userData);
    }

    private void ShowRiseOrDropAnim()
    {
        if (isShowGuide)
            return;

        int currentWinStreak = ClimbBeanstalkManager.Instance.CurrentWinStreak;
        int lastWinStreak = ClimbBeanstalkManager.Instance.LastWinStreakNum;

        if (currentWinStreak > lastWinStreak)
        {
            GameManager.Task.AddDelayTriggerTask(0f, () =>
            {
                StartCoroutine(ShowPlayerRiseAnimCor(currentWinStreak, lastWinStreak));
            });
        }
        else if (currentWinStreak < lastWinStreak)
        {
            //对于刚打完一个Phase的情况 也会落入这个分支(current<last) 用EverShowedIntro来区分一下
            if (ClimbBeanstalkManager.Instance.EverShowedIntro)
                ShowPlayerDropAnim(currentWinStreak);
        }

    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);

        if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                itemPromptBox.HidePromptBox();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                itemPromptBox.HidePromptBox();
            }
        }

        //防止滑动越界穿帮
        if (scrollArea.scrollRect.verticalNormalizedPosition > topBoundary)
            scrollArea.scrollRect.verticalNormalizedPosition = topBoundary;
        else if (scrollArea.scrollRect.verticalNormalizedPosition < bottomBoundary)
            scrollArea.scrollRect.verticalNormalizedPosition = bottomBoundary;

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        scrollArea.OnReset();
        base.OnReset();
    }

    public override void OnRelease()
    {
        scrollArea.OnRelease();
        UnityUtility.UnloadAssetAsync(handle);

        base.OnRelease();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < imagesNeedToRelease.Length; ++i)
        {
            imagesNeedToRelease[i].sprite = null;
        }
    }

    private void EnterAnim()
    {
        itemPromptBox.HidePromptBox();
        itemPromptBox.forbidShow = true;

        scrollArea.scrollRect.vertical = false;
        closeBtn.interactable = false;
    }

    private void ExitAnim()
    {
        itemPromptBox.forbidShow = false;

        scrollArea.scrollRect.vertical = true;
        closeBtn.interactable = true;
    }

    public override void OnClose()
    {
        //播放背景音乐
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        base.OnClose();
    }

    /// <summary>
    /// 显示奖励内容弹框
    /// </summary>
    public void ShowRewardTip(int stage, List<TotalItemData> rewardTypes, List<int> nums, Vector3 showPosition)
    {
        itemPromptBox.Init(rewardTypes, nums);

        List<ClimbBeanstalkTaskDatas> dataList = ClimbBeanstalkManager.Instance.DataTable.GetRecentClimbBeanstalkTaskDatas();
        int centerIndex = dataList.Count - scrollArea.GetViewportCenterIndex();

        if (nums.Count >= 3)
            itemPromptBox.triangelOffset = 50;
        else
            itemPromptBox.triangelOffset = 0;

        if (stage <= centerIndex)
            itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, showPosition);
        else
            itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Down, showPosition);
    }

    public override bool CheckInitComplete()
    {
        if (!scrollArea.CheckSpawnComplete())
            return false;

        ScrollColumn[] stageColumns = scrollArea.GetColumns(NORMAL_COLUMN_PREFAB_NAME);
        for (int i = 0; i < stageColumns.Length; i++)
        {
            if (stageColumns[i].Instance != null && !stageColumns[i].Instance.GetComponent<ClimbBeanstalkGO>().CheckInitComplete())
                return false;
        }

        stageColumns = scrollArea.GetColumns(LAST_COLUMN_PREFAB_NAME);
        for (int i = 0; i < stageColumns.Length; i++)
        {
            if (stageColumns[i].Instance != null && !stageColumns[i].Instance.GetComponent<ClimbBeanstalkGO>().CheckInitComplete())
                return false;
        }

        return true;
    }

    private void RefreshLayout()
    {
        ScrollColumn[] stageColumns = scrollArea.GetColumns(NORMAL_COLUMN_PREFAB_NAME);
        List<ClimbBeanstalkScrollColumn> stageColumnList = new List<ClimbBeanstalkScrollColumn>();
        for (int i = 0; i < stageColumns.Length; i++)
        {
            ClimbBeanstalkScrollColumn balloonColumn = (ClimbBeanstalkScrollColumn)stageColumns[i];
            stageColumnList.Add(balloonColumn);
        }
        stageColumnList.Sort((a, b) =>
        {
            if (a.Stage < b.Stage)
                return -1;
            else if (a.Stage > b.Stage)
                return 1;
            else
                return 0;
        });

        for (int i = 0; i < stageColumnList.Count; i++)
        {
            if (stageColumnList[i].Instance != null)
            {
                stageColumnList[i].Instance.transform.SetAsFirstSibling();
            }
        }

        //TreeTop -> LastColumn -> Normal -> grassLand -> MyPortrait
        ScrollColumn[] lastColumns = scrollArea.GetColumns(LAST_COLUMN_PREFAB_NAME);
        if (lastColumns.Length > 0 && lastColumns[0].Instance != null)
            lastColumns[0].Instance.transform.SetAsFirstSibling();
        if (treeTop)
            treeTop.SetAsFirstSibling();

        grassLand.transform.SetAsLastSibling();
        myPortraitTransform.transform.SetAsLastSibling();
    }

    protected float GetSliderY(int stage)
    {
        List<ClimbBeanstalkTaskDatas> dataList = ClimbBeanstalkManager.Instance.DataTable.GetRecentClimbBeanstalkTaskDatas();


        if (stage > dataList.Count)
            return scrollArea.GetColumnLocalPosition(dataList.Count - stage).y;

        int lastStageIndex = dataList.Count - stage;

        float result;
        if (stage == 0)
            result = scrollArea.GetColumnLocalPosition(lastStageIndex).y - 320;        //让玩家头像碰到地面
        else if (stage == dataList.Count)
            result = scrollArea.GetColumnLocalPosition(lastStageIndex).y + scrollArea.recycleWidth / 2f;
        else
            result = scrollArea.GetColumnLocalPosition(lastStageIndex).y/* + scrollArea.recycleWidth / 2f*/;

        return result;
    }


    IEnumerator ShowPlayerRiseAnimCor(int currentWinStreak, int lastWinStreak)
    {
        EnterAnim();
        playButton.gameObject.SetActive(false);

        yield return null;

        List<ClimbBeanstalkTaskDatas> climbBeanstalkDataTable = ClimbBeanstalkManager.Instance.DataTable.GetRecentClimbBeanstalkTaskDatas();


        //float animDuration = 0f;
        //float speedRatio = 0.7f;

        //animDuration = speedRatio * (currentWinStreak - lastWinStreak);

        Vector3 flyTargetPos = new Vector3(0, GetSliderY(currentWinStreak), 0);

        float playerPortraitRotateDuration = 0.2f;
        float flyDuration = 1f;
        myPortraitImage.transform.DOLocalRotate(Vector3.forward * 20.0f, playerPortraitRotateDuration).onComplete = () =>
        {
            myPortraitTransform.DOLocalMove(flyTargetPos, flyDuration).SetEase(Ease.OutQuad).onComplete = () =>
            {
                myPortraitImage.transform.DOLocalRotate(Vector3.zero, playerPortraitRotateDuration);
            };
        };

        yield return new WaitForSeconds(playerPortraitRotateDuration);

        float singleInterval = flyDuration / (float)(currentWinStreak - lastWinStreak);

        int passNodeCount = 0;

        //实际上升动画前 检查是否有奖励领取
        //如果有的话 在动画最后不要显示Play按钮
        ClimbBeanstalkTaskDatas firstUnclaimedTask = null;
        bool hasRewardToCliam = ClimbBeanstalkManager.Instance.HasRewardToClaim(out firstUnclaimedTask);

        //逐次刷新颜色(完成状态)
        LinkedListNode<ScrollColumn> current = scrollArea.scrollColumnList.Last;
        while (current != null)
        {
            float duration = 0;
            if (current.Value.Instance != null)
            {
                duration = current.Value.Instance.GetComponent<ClimbBeanstalkGO>().RefreshLayout(false, singleInterval);
            }

            current = current.Previous;
            if (current == null && duration > 0)
            {
                //意味着到了终点 希望滚动到最高
                scrollArea.CenterTheTargetColumn(0, 1.0f);
            }
            if (current != null && duration > 0)
            {
                passNodeCount++;
                if (passNodeCount > 1)
                    scrollArea.CenterTheTargetColumn(climbBeanstalkDataTable.Count - currentWinStreak + 1, 1.0f);
                yield return new WaitForSeconds(singleInterval);
            }
        }

        if (!hasRewardToCliam)
            playButton.gameObject.SetActive(true);

        ExitAnim();
        ClimbBeanstalkManager.Instance.LastWinStreakNum = ClimbBeanstalkManager.Instance.CurrentWinStreak;
    }

    private void ShowPlayerDropAnim(int currentWinStreak)
    {
        EnterAnim();

        playButton.gameObject.SetActive(false);

        float playerPortraitRotateDuration = 0.2f;
        float duration = 1f;

        //所有节点颜色等 更新
        //暂时无法做到随玩家掉落过程逐步刷新
        scrollArea.Refresh();

        //当播放掉落动画时 起始的在屏幕中的高度更高一些
        scrollArea.CenterTheTargetColumn(scrollArea.currentIndex + 1, 0);

        //玩家掉落
        float y = GetSliderY(currentWinStreak) + ((currentWinStreak == 0) ? 150 : 0);
        Vector3 flyTargetPos = new Vector3(0, y, 0);
        myPortraitImage.transform.DOLocalRotate(Vector3.back * 20.0f, playerPortraitRotateDuration).onComplete = () =>
        {
            myPortraitTransform.DOLocalMove(flyTargetPos, duration).SetEase(Ease.InCubic).onComplete = () =>
            {
                GameManager.UI.HideUIForm("GlobalMaskPanel");
                playButton.gameObject.SetActive(true);
                ExitAnim();

                myPortraitImage.transform.DOLocalRotate(Vector3.zero, playerPortraitRotateDuration).SetEase(Ease.OutElastic);
            };
        };

        //镜头掉落
        List<ClimbBeanstalkTaskDatas> balloonRiseDataTable = ClimbBeanstalkManager.Instance.DataTable.GetRecentClimbBeanstalkTaskDatas();
        GameManager.Task.AddDelayTriggerTask(playerPortraitRotateDuration, () =>
        {
            scrollArea.CenterTheTargetColumn(balloonRiseDataTable.Count - currentWinStreak + 1, duration, Ease.InCubic);
        });


        ClimbBeanstalkManager.Instance.LastWinStreakNum = ClimbBeanstalkManager.Instance.CurrentWinStreak;
    }

    private void ShowClimbBeanstalkGuide()
    {
        EnterAnim();

        playButton.gameObject.SetActive(false);
        stageTextEffect.gameObject.SetActive(true);
        float stageTextEffectTime = 1.0f;
        GameManager.Task.AddDelayTriggerTask(stageTextEffectTime, () =>
        {
            stageTextEffect.gameObject.SetActive(false);
        });

        scrollArea.scrollRect.verticalNormalizedPosition = 1;
        scrollArea.scrollRect.DOVerticalNormalizedPos(0, 3f).SetEase(Ease.InOutCubic).SetDelay(stageTextEffectTime);

        GameManager.Task.AddDelayTriggerTask(3.0f + stageTextEffectTime, () =>
        {
            ClimbBeanstalkManager.Instance.EverShowedIntro = true;

            GameManager.UI.HideUIForm("GlobalMaskPanel");
            playButton.gameObject.SetActive(true);

            ExitAnim();
        });
    }
    public virtual void OnScrollViewChanged(Vector2 value)
    {
        backScene.position = new Vector3(0, backSceneOriginY - scrollArea.scrollRect.verticalNormalizedPosition * backSceneRatio, 0);
    }

    private void OnCloseBtnClicked()
    {
        //玩家主动点击退出按钮时 EndProcess
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.CheckClimbBeanstalk);
    }

    private void OnPlayBtnClicked()
    {
        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);

        GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
        GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
    }

    void ICustomOnEscapeBtnClicked.OnEscapeBtnClicked()
    {
        if (!closeBtn.isActiveAndEnabled && !closeBtn.interactable)
            return;

        OnCloseBtnClicked();
    }
}
