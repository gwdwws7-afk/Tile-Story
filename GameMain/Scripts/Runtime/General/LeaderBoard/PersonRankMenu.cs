using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PersonRankMenu : UIForm, ICustomOnEscapeBtnClicked
{
    public ScrollArea scrollArea;
    public DelayButton closeButton;
    public DelayButton playButton;
    public DelayButton infoButton;
    public PersonRankPanelManager myRankPanel;
    public ClockBar clock;
    public Vector3[] rankPanelPositions;

    public GameObject NoInternetPanel;
    // public TextMeshProUGUILocalize title;

    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage, curBg;
    [SerializeField] private ItemPromptBox promptBox;
    [SerializeField] private TextPromptBox textPromptBox;


    private float maxSpeed;
    private List<ScrollColumn> scrollColumns;
    private LeaderBoardRankScrollColumn myScrollColumn;

    private bool hasSetPanel = false;
    private bool hasShownAnim = false;
    private bool needToShowAnim = false;
    private bool _countdownOver = false;
    private bool _isAnimFinished = false;
    private bool _isEditName = false;
    private bool _isShowUpAnim = false;
    private float _rankPanelBottomY = 0;
    private RectTransform _myRankPanelRectTransform;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        // title.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()} League");
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventReceived);

        //初始化私有数据

        myRankPanel.gameObject.SetActive(false);
        _rankPanelBottomY = myRankPanel.transform.position.y;
        _myRankPanelRectTransform = myRankPanel.GetComponent<RectTransform>();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoInternetPanel.SetActive(true);
            playButton.gameObject.SetActive(false);
        }
        else
        {
            NoInternetPanel.SetActive(false);
            playButton.gameObject.SetActive(true);
            scrollArea.content.gameObject.SetActive(false);
            SetPanels();
        }

        var text = textPromptBox.GetComponentInChildren<TextMeshProUGUILocalize>();
        text.SetParameterValue("0", "<color=#217F02>");
        text.SetParameterValue("1", "</color>");

        SetTimer();
        promptBox.gameObject.SetActive(false);
        closeButton.SetBtnEvent(CloseEvent);
        playButton.SetBtnEvent(OnPlayButtonClick);
        infoButton.SetBtnEvent(OnInfoButtonClick);
        playButton.interactable = false;
        closeButton.interactable = false;
        infoButton.interactable = false;
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.PersonRankChanged));
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        OnResume();
        scrollArea.OnReset();
        needToShowAnim = false;
        _isAnimFinished = false;
        hasSetPanel = false;
        hasShownAnim = false;
        _countdownOver = false;
        _isEditName = false;
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventReceived);
        _myRankPanelRectTransform.anchorMax = new Vector2(0.5f, 0);
        _myRankPanelRectTransform.anchorMin = new Vector2(0.5f, 0);
        _myRankPanelRectTransform.anchoredPosition = rankPanelPositions[0];
        myRankPanel.OnReset();
        base.OnReset();
    }

    public override void OnRelease()
    {
        scrollArea.OnRelease();
        myRankPanel.OnReset();
        base.OnRelease();
    }

    private void OnEventReceived(object sender, GameEventArgs e)
    {
        var ne = e as CommonEventArgs;
        if (ne == null) return;
        if (ne.Type == CommonEventType.ShowPersonRankRewardPromptBox)
        {
            var rank = (int)ne.UserDatas[0];
            //var direction = (PromptBoxShowDirection)ne.UserDatas[1];
            var direction = PromptBoxShowDirection.Down;
            var pos = (Vector3)ne.UserDatas[2];
            ShowPromptBox(rank, direction, pos);
        }
        else if (ne.Type == CommonEventType.ShowPersonRankTextPromptBox)
        {
            //var direction = (PromptBoxShowDirection)ne.UserDatas[0];
            var direction = PromptBoxShowDirection.Down;
            var pos = (Vector3)ne.UserDatas[1];
            ShowTextPromptBox(direction, pos);
        }
    }

    private void ChangeName()
    {
        if (GameManager.PlayerData.RecordSetPlayerName && !string.IsNullOrEmpty(GameManager.PlayerData.PlayerName))
        {
            return;
        }

        //内存极低的手机打开取名输入法会崩溃
        if (SystemInfoManager.DeviceType > DeviceType.SurpLow) 
        {
            playButton.interactable = false;
            closeButton.interactable = false;
            infoButton.interactable = false;
            GameManager.Task.AddDelayTriggerTask(0.4f, () =>
            {
                GameManager.UI.ShowUIForm("PersonRankName", f =>
                {
                    playButton.interactable = true;
                    closeButton.interactable = true;
                    infoButton.interactable = true;
                }, () =>
                {
                    playButton.interactable = true;
                    closeButton.interactable = true;
                    infoButton.interactable = true;
                }, preName);
            });
        }
        else
        {
            GameManager.PlayerData.RecordSetPlayerName = true;
        }
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;
                    needToShowAnim = true;
                };
            };
        }

        if (bgImage != null)
        {
            bgImage.DOKill();
            bgImage.DOColor(new Color(1, 1, 1, 0.01f), 0).OnComplete(() => { bgImage.DOFade(1, 0.2f); });
        }

        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();

        if (cachedTransform) cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(false);

        if (UIGroup.UIFormCount == 0)
        {
            GameManager.Sound.PlayUIOpenSound();
        }

        base.OnHide(hideSuccessAction, userData);
    }

    public void CloseEvent()
    {
        GameManager.Process.EndProcess(ProcessType.ShowPersonRankChangeName);
        GameManager.UI.HideUIForm(this);
    }


    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);
        clock.OnUpdate(elapseSeconds, realElapseSeconds);

        if (scrollArea.CheckSpawnComplete() && !hasSetPanel)
        {
            hasSetPanel = true;
            playButton.interactable = true;
            closeButton.interactable = true;
            infoButton.interactable = true;
            // InitMyRankPanel();
            ChangeName();
        }

        if (hasSetPanel && !hasShownAnim && needToShowAnim)
        {
            hasShownAnim = true;
            scrollArea.content.gameObject.SetActive(true);
            if (_isShowUpAnim)
            {
                scrollArea.scrollRect.onValueChanged.RemoveAllListeners();
                var index = Mathf.Abs(GameManager.Task.PersonRankManager.Rank -
                                      GameManager.Task.PersonRankManager.LastUpRank);
                // if (index == 1)
                // {
                //     GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                // }
                // else if(index==2)
                // {
                //     GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                //     GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                // }
                // else if(index>2)
                // {
                //     GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                //     GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                //     GameManager.Task.AddDelayTriggerTask(0.9f, () =>
                //     {
                //         GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                //     });
                // }
                OnPause();
                scrollArea.DoPanelMove(GameManager.Task.PersonRankManager.LastUpRank - 1,
                    GameManager.Task.PersonRankManager.Rank - 1, () =>
                    {
                        OnResume();
                        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);
                        GameManager.Task.PersonRankManager.LastUpRank = GameManager.Task.PersonRankManager.Rank;
                    });
                //滚动动画前准备
                // maxSpeed = 0.01f;
                // myRankPanel.transform.position = lastScrollColumn.Instance.transform.position;
                // myRankPanel.gameObject.SetActive(true);
                // // lastScrollColumn.Instance.transform.localScale = Vector3.zero;
                // if (!ReferenceEquals(myScrollColumn.Instance, null))
                //     myScrollColumn.Instance.transform.localScale = Vector3.zero;
                // StartCoroutine(ShowScrollUpAnim(GameManager.Task.PersonRankManager.Rank - 1));
            }
            else
            {
                StartCoroutine(ShowRankPanelAnim());
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
#else
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
            HidePromptBox();
        }

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnClose()
    {
        GameManager.Process.EndProcess(ProcessType.ShowPersonRankChangeName);
        base.OnClose();
    }

    public override void OnPause()
    {
        closeButton.interactable = false;
        infoButton.interactable = false;
        playButton.interactable = false;
        base.OnPause();
    }

    public override void OnResume()
    {
        closeButton.interactable = true;
        infoButton.interactable = true;
        playButton.interactable = true;

        base.OnResume();
    }

    private string preName;

    private PersonRankData _rankData;

    public void OnNameSet()
    {
        SetPanels();
        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);
    }

    private void SetPanels()
    {
        scrollArea.OnReset();
        var datas = GameManager.Task.PersonRankManager.RankDatas;
        datas.Sort((x, y) => x.Rank.CompareTo(y.Rank));
        if (GameManager.Task.PersonRankManager.LastUpRank <= 0 ||
            GameManager.Task.PersonRankManager.LastUpRank > datas.Count)
        {
            GameManager.Task.PersonRankManager.LastUpRank = GameManager.Task.PersonRankManager.Rank;
        }

        _isShowUpAnim = GameManager.Task.PersonRankManager.LastUpRank != GameManager.Task.PersonRankManager.Rank &&
                        GameManager.PlayerData.RecordSetPlayerName;
        var i = 0;
        var myData = GameManager.Task.PersonRankManager.LocalData;
        myData.Rank = GameManager.Task.PersonRankManager.Rank;
        var hasAddMyself = false;
        foreach (var data in datas)
        {
            if (string.IsNullOrEmpty(data.Name) || data.Score <= 0)
            {
                continue;
            }

            if (data.IsSelf())
            {
                if (_isShowUpAnim)
                {
                    continue;
                }

                hasAddMyself = true;
                _rankData = data;
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, data, i));
                myScrollColumn = scrollArea.scrollColumnList.Last.Value as LeaderBoardRankScrollColumn;
                myRankPanel.InitPanel(data);
            }
            else if (_isShowUpAnim && i == GameManager.Task.PersonRankManager.LastUpRank - 1)
            {
                hasAddMyself = true;
                preName = myData.Name;
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, myData, i));
                myScrollColumn = scrollArea.scrollColumnList.Last.Value as LeaderBoardRankScrollColumn;
                i++;
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, data, i));
                myRankPanel.InitPanel(myData);
            }
            else
            {
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, data, i));
            }

            i++;
        }

        if (!hasAddMyself)
        {
            preName = myData.Name;
            scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, myData,
                GameManager.Task.PersonRankManager.LastUpRank - 1));
            myScrollColumn = scrollArea.scrollColumnList.Last.Value as LeaderBoardRankScrollColumn;
            myRankPanel.InitPanel(myData);
        }

        if (_isShowUpAnim)
        {
            scrollArea.currentIndex = GameManager.Task.PersonRankManager.LastUpRank - 1;
            if (!ReferenceEquals(myScrollColumn?.Instance, null))
            {
                myRankPanel.transform.position = myScrollColumn.Instance.transform.position;
            }
        }
        else
        {
            scrollArea.currentIndex = 0;
        }


        scrollArea.OnInit(GetComponent<RectTransform>());
        scrollColumns = scrollArea.scrollColumnList.ToList();
        scrollArea.scrollRect.vertical = hasShownAnim;
    }

    IEnumerator ShowRankPanelAnim()
    {
        scrollArea.isShowingAnim = true;
        LinkedList<ScrollColumn> list = new LinkedList<ScrollColumn>(scrollArea.scrollColumnList);

        foreach (ScrollColumn column in list)
        {
            if (column.Instance != null)
            {
                column.Instance.transform.localPosition = new Vector3(1100, column.Instance.transform.localPosition.y);
            }
        }

        foreach (ScrollColumn column in list)
        {
            if (column.Instance != null)
            {
                //音效在播放的0.1~0.2s时会出现，动画使用InCubic模式会存在回弹，所以音效的播放时间需要大致在0.3~0.4s左右
                if (column.index < 5)
                    GameManager.Task.AddDelayTriggerTask(0.2f, () => GameManager.Sound.PlayAudio("SFX_League_List_Insert"));
                column.Instance.transform.DOLocalMoveX(-20, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    column.Instance.transform.DOLocalMoveX(0, 0.2f).SetEase(Ease.InOutCubic);
                });

                yield return new WaitForSeconds(0.1f);
            }
        }

        scrollArea.isShowingAnim = false;
        ShowMyRankPanel();
        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);
        scrollArea.scrollRect.vertical = true;
    }

    IEnumerator ShowScrollUpAnim(int targetIndex)
    {
        var myRankTransform = myRankPanel.transform;

        yield return new WaitForSeconds(0.5f);
        //初始化数据
        var startPosition = scrollArea.scrollRect.verticalNormalizedPosition; //起始位置
        var targetPosition = 1f - targetIndex / (float)(scrollArea.Count - 1); //目标位置
        var curPosition = scrollArea.scrollRect.verticalNormalizedPosition; //当前位置
        var curT = (curPosition - startPosition) / (targetPosition - startPosition); //已经经过的时间

        //将自己的排名面板移到屏幕中间
        myRankTransform.DOScale(1.06f, 0.3f).SetEase(Ease.InCubic);

        yield return new WaitForSeconds(0.5f);

        float startColumnY = 0;
        float lastCurT = 0;
        var isUp = startPosition < targetPosition;
        var scrollViewY = scrollArea.scrollRect.viewport.position.y;

        if (!ReferenceEquals(myScrollColumn.Instance, null) &&
            myScrollColumn.Instance.transform.position.y <= scrollViewY - 0.1f &&
            myScrollColumn.Instance.transform.position.y >= _rankPanelBottomY)
        {
            curT = 1;
        }

        //开始滚动
        while (ReferenceEquals(myScrollColumn.Instance, null) ||
               ((myScrollColumn.Instance.transform.position.y > scrollViewY - 0.3f ||
                 myScrollColumn.Instance.transform.position.y < scrollViewY - 0.4f) &&
                curT < 1))
        {
            if (!ReferenceEquals(myScrollColumn.Instance, null) &&
                myScrollColumn.Instance.transform.localScale != Vector3.zero)
            {
                myScrollColumn.Instance.transform.localScale = Vector3.zero;
                startColumnY = myScrollColumn.Instance.transform.position.y;
                lastCurT = curT;
            }

            var curSpeed = GetSpeed(curT);
            if (isUp)
                curPosition = scrollArea.scrollRect.verticalNormalizedPosition + curSpeed;
            else
                curPosition = scrollArea.scrollRect.verticalNormalizedPosition - curSpeed;
            scrollArea.scrollRect.verticalNormalizedPosition = Mathf.Max(Mathf.Min(1, curPosition), 0);
            if (ReferenceEquals(myScrollColumn.Instance, null))
            {
                curT = (curPosition - startPosition) / (targetPosition - startPosition);
            }
            else
            {
                curT = lastCurT + (1 - lastCurT) * (myScrollColumn.Instance.transform.position.y - startColumnY) /
                    (-startColumnY + scrollArea.scrollRect.viewport.position.y - 0.1f);
                var pos = scrollArea.scrollRect.verticalNormalizedPosition;
                if (Mathf.Abs(pos) <= 0.001f ||
                    Mathf.Abs(1 - pos) <= 0.001f)
                {
                    curT = 1;
                }
            }

            var rank = Mathf.Lerp(GameManager.Task.PersonRankManager.LastUpRank,
                GameManager.Task.PersonRankManager.Rank, curT);
            myRankPanel.UpdatePanel(Mathf.CeilToInt(rank));
            yield return null;
        }

        //防止结束滚动时目标面板没有空出来
        if (myScrollColumn.Instance.transform.localScale != Vector3.zero)
        {
            myScrollColumn.Instance.transform.localScale = Vector3.zero;
        }

        yield return new WaitForSeconds(0.3f);

        //将我的排名面板放回滚动栏
        var currentPosition = myScrollColumn.Instance.transform.position;
        myRankPanel.transform.DOMove(currentPosition, 0.5f);
        myRankPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InCubic).onComplete = () =>
        {
            myScrollColumn.Instance.transform.localScale = Vector3.one;
            myRankPanel.gameObject.SetActive(false);
            myRankPanel.InitPanel(_rankData);
        };

        yield return new WaitForSeconds(0.5f);

        scrollArea.currentIndex = targetIndex;
        scrollArea.Refresh();
        scrollArea.RefreshScrollColumnList();
        // scrollArea.OnInit(GetComponent<RectTransform>());
        _isAnimFinished = true;
        GameManager.Task.PersonRankManager.LastUpRank = GameManager.Task.PersonRankManager.Rank;
        scrollArea.scrollRect.vertical = true;
        scrollArea.scrollRect.onValueChanged.AddListener(OnScrollViewChanged);
    }

    public void ExchangePanel()
    {
        scrollArea.scrollRect.onValueChanged.RemoveAllListeners();
        scrollArea.DoPanelMove(49, 0, null);
    }

    private void OnScrollViewChanged(Vector2 arg)
    {
        ShowMyRankPanel();
    }

    private void ShowMyRankPanel()
    {
        if (myScrollColumn != null && myScrollColumn.Instance != null)
        {
            float myPanelY = myScrollColumn.Instance.transform.position.y;

            float scrollViewY = scrollArea.scrollRect.viewport.position.y;

            if (myPanelY <= scrollViewY && myPanelY >= _rankPanelBottomY)
            {
                myRankPanel.gameObject.SetActive(false);
            }
            else if (myPanelY > scrollViewY)
            {
                _myRankPanelRectTransform.anchorMax = new Vector2(0.5f, 1);
                _myRankPanelRectTransform.anchorMin = new Vector2(0.5f, 1);
                _myRankPanelRectTransform.anchoredPosition = rankPanelPositions[1];
                myRankPanel.gameObject.SetActive(true);
            }
            else if (myPanelY < _rankPanelBottomY)
            {
                _myRankPanelRectTransform.anchorMax = new Vector2(0.5f, 0);
                _myRankPanelRectTransform.anchorMin = new Vector2(0.5f, 0);
                _myRankPanelRectTransform.anchoredPosition = rankPanelPositions[0];
                myRankPanel.gameObject.SetActive(true);
            }
        }
        else
        {
            myRankPanel.gameObject.SetActive(true);
        }
    }

    private void SetTimer()
    {
        var endTime = GameManager.Task.PersonRankManager.EndTime;
        if (endTime <= DateTime.Now)
        {
            clock.SetFinishState();
            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
        }
        else
        {
            clock.gameObject.SetActive(true);
            clock.StartCountdown(endTime);
            clock.CountdownOver += OnCountdownOver;
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        if (_countdownOver) return;
        _countdownOver = true;
        clock.SetFinishState();
        GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
    }


    private float GetSpeed(float t)
    {
        if (t < 0.1f)
        {
            // 使用二次插值
            var tNormalized = t / 0.1f;
            return Mathf.Lerp(0.001f, maxSpeed, tNormalized * tNormalized);
        }

        if (t > 0.3f)
        {
            // 使用二次插值
            var tNormalized = (1 - t) / 0.3f;
            return Mathf.Lerp(0.001f, maxSpeed, tNormalized * tNormalized);
        }

        // 在中间部分保持最大速度
        return maxSpeed;
    }

    private void OnPlayButtonClick()
    {
        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing)
        {
            //ProcedureUtil.ProcedureMapToGame();
            GameManager.Process.EndProcess(ProcessType.ShowPersonRankChangeName);
            GameManager.UI.HideUIForm(this);
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
        }

        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Finished)
        {
            Log.Warning("Need To Send PersonRank Rewards");
        }
    }

    public void ShowPromptBox(int rank, PromptBoxShowDirection direction, Vector3 pos)
    {
        var rewardDic =
            GameManager.Task.PersonRankManager.TaskData.GetRewardsByLevel(
                GameManager.Task.PersonRankManager.RankLevel, rank);
        promptBox.Init(rewardDic.Keys.ToList(), rewardDic.Values.ToList());
        promptBox.ShowPromptBox(direction, pos);
    }

    public void ShowTextPromptBox(PromptBoxShowDirection direction, Vector3 pos)
    {
        textPromptBox.ShowPromptBox(direction, pos);
    }

    private void HidePromptBox()
    {
        promptBox.HidePromptBox();
        textPromptBox.HidePromptBox();
    }

    private void OnInfoButtonClick()
    {
        GameManager.UI.ShowUIForm("PersonRankRules");
    }

    public void OnEscapeBtnClicked()
    {
        CloseEvent();
    }
}