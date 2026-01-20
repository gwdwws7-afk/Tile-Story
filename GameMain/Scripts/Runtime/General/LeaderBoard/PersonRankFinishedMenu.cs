using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PersonRankFinishedMenu : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public DelayButton okButton;
    public GameObject upRankObj, keepRankObj;
    public Image preRankImg, nowRankImg, rankUpImg, keepRankImg, curBg;
    public TextMeshProUGUILocalize preRankText, nowRankText, describeText, keepRankText;
    public ScrollArea scrollArea;
    public PersonRankPanelManager myRankPanel;
    public Vector3[] rankPanelPositions;
    [SerializeField] private ItemPromptBox promptBox;
    [SerializeField] private TextPromptBox textPromptBox;

    public AsyncOperationHandle _AssetHandle1, _AssetHandle2;
    private LeaderBoardRankScrollColumn myScrollColumn;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        // title.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()} League");
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventRecieved);
        var text = textPromptBox.GetComponentInChildren<TextMeshProUGUILocalize>();
        myRankPanel.gameObject.SetActive(false);
        _rankPanelBottomY = myRankPanel.transform.position.y;
        _myRankPanelRectTransform = myRankPanel.GetComponent<RectTransform>();

        text.SetParameterValue("0", "<color=#217F02>");
        text.SetParameterValue("1", "</color>");
        SetPanels();
        SetImages();
        okButton.SetBtnEvent(OnClose);
        promptBox.gameObject.SetActive(false);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        //段位上升动画
        if (upRankObj.activeInHierarchy)
        {
            preRankImg.transform.localPosition = Vector3.zero;
            preRankImg.gameObject.SetActive(true);
            nowRankImg.gameObject.SetActive(false);
            rankUpImg.gameObject.SetActive(false);

            preRankImg.transform.DOLocalMoveX(-171, 0.4f).SetEase(Ease.InBack).onComplete = () =>
            {
                rankUpImg.transform.localRotation = Quaternion.Euler(0, 0, -20);
                rankUpImg.transform.localPosition = new Vector3(-120, -30, 0);
                rankUpImg.color = new Color(rankUpImg.color.r, rankUpImg.color.g, rankUpImg.color.b, 0);
                rankUpImg.gameObject.SetActive(true);
                rankUpImg.transform.DOLocalMove(new Vector3(-3, -16, 0), 0.3f);
                rankUpImg.transform.DOLocalRotate(Vector3.zero, 0.3f);
                rankUpImg.DOFade(1, 0.3f);

                nowRankImg.transform.localScale = Vector3.zero;
                nowRankImg.color = new Color(nowRankImg.color.r, nowRankImg.color.g, nowRankImg.color.b, 0);
                nowRankImg.gameObject.SetActive(true);
                nowRankImg.DOFade(1, 0.2f).SetDelay(0.25f);
                nowRankImg.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.25f);
            };
        }

        //隐藏入口
        var mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        mapTop?.personRankEntrance.gameObject.SetActive(false);
    }

    public override bool CheckInitComplete()
    {
        if (!_AssetHandle1.IsDone || !_AssetHandle2.IsDone)
            return false;

        return true;
    }

    private void OnEventRecieved(object sender, GameEventArgs e)
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

    private void SetImages()
    {
        var willLevelUp = GameManager.Task.PersonRankManager.WillLevelUp();
        upRankObj.SetActive(willLevelUp);
        keepRankObj.SetActive(!willLevelUp);
        if (willLevelUp)
        {
            describeText.SetTerm("PersonRank.You moved to the higher league and won rewards!");
            preRankText.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()}");
            nowRankText.SetTerm($"PersonRank.{(GameManager.Task.PersonRankManager.RankLevel + 1).ToString()}");
            if (_AssetHandle1.IsValid())
            {
                Addressables.Release(_AssetHandle1);
            }

            _AssetHandle1 = UnityUtility.LoadSpriteAsync(
                $"Medal{((int)GameManager.Task.PersonRankManager.RankLevel).ToString()}", "LeaderBoard",
                sp => { preRankImg.sprite = sp; });
            if (_AssetHandle2.IsValid())
            {
                Addressables.Release(_AssetHandle2);
            }

            _AssetHandle2 = UnityUtility.LoadSpriteAsync(
                $"Medal{((int)GameManager.Task.PersonRankManager.RankLevel + 1).ToString()}", "LeaderBoard",
                sp => { nowRankImg.sprite = sp; });
        }
        else if (GameManager.Task.PersonRankManager.RankLevel == PersonRankLevel.Supreme)
        {
            describeText.SetTerm("PersonRank.You're staying in the highest league. Claim your rewards!");
            keepRankText.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()}");
            if (_AssetHandle1.IsValid())
            {
                Addressables.Release(_AssetHandle1);
            }

            _AssetHandle1 = UnityUtility.LoadSpriteAsync(
                $"Medal{((int)GameManager.Task.PersonRankManager.RankLevel).ToString()}", "LeaderBoard",
                sp => { keepRankImg.sprite = sp; });
        }
        else
        {
            describeText.SetTerm("PersonRank.You stayed in the same league.Claim your rewards!");
            keepRankText.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()}");
            if (_AssetHandle1.IsValid())
            {
                Addressables.Release(_AssetHandle1);
            }

            _AssetHandle1 = UnityUtility.LoadSpriteAsync(
                $"Medal{((int)GameManager.Task.PersonRankManager.RankLevel).ToString()}", "LeaderBoard",
                sp => { keepRankImg.sprite = sp; });

        }
    }

    private void SetPanels()
    {
        scrollArea.OnReset();
        var datas = GameManager.Task.PersonRankManager.RankDatas;
        datas.Sort((x, y) => x.Rank.CompareTo(y.Rank));
        var index = 0;
        var i = 0;
        foreach (var data in datas)
        {
            if (string.IsNullOrEmpty(data.Name) || data.Score <= 0)
            {
                continue;
            }

            if (data.IsSelf())
            {
                index = i;
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, data, i));
                myScrollColumn = scrollArea.scrollColumnList.Last.Value as LeaderBoardRankScrollColumn;
                myRankPanel.InitPanel(data);
            }
            else
            {
                scrollArea.AddColumnLast(new LeaderBoardRankScrollColumn(scrollArea, data, i));
            }

            i++;
        }

        scrollArea.OnInit(GetComponent<RectTransform>());
    }

    private void OnDestroy()
    {
        curBg.sprite = null;
    }

    public override void OnClose()
    {
        GameManager.Task.PersonRankManager.SendReward(() =>
        {
            var mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop?.SetLeaderBoardBtnState();
            GameManager.UI.ShowUIForm("PersonRankWelcomeMenu",showFailAction: () =>
            {
                GameManager.Process.EndProcess(ProcessType.PersonRankFinish);
            });
        });
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }

    public void OnEscapeBtnClicked()
    {
        OnClose();
    }

    private bool hasSetPanel = false;
    private bool hasShownAnim = false;
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);

        if (scrollArea.CheckSpawnComplete() && !hasShownAnim && gameObject.activeSelf)
        {
            hasShownAnim = true;
            scrollArea.content.gameObject.SetActive(true);
            StartCoroutine(ShowRankPanelAnim());

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

    private void OnScrollViewChanged(Vector2 arg)
    {
        ShowMyRankPanel();
    }

    private float _rankPanelBottomY = 0;
    private RectTransform _myRankPanelRectTransform;
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

    public override void OnReset()
    {
        if (_AssetHandle1.IsValid())
        {
            Addressables.Release(_AssetHandle1);
        }

        if (_AssetHandle2.IsValid())
        {
            Addressables.Release(_AssetHandle2);
        }
        scrollArea.OnReset();
        hasSetPanel = false;
        hasShownAnim = false;
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);
        _myRankPanelRectTransform.anchorMax = new Vector2(0.5f, 0);
        _myRankPanelRectTransform.anchorMin = new Vector2(0.5f, 0);
        _myRankPanelRectTransform.anchoredPosition = rankPanelPositions[0];
        base.OnReset();
    }

    public override void OnRelease()
    {
        scrollArea.OnRelease();
        base.OnRelease();
    }

    public void ShowPromptBox(int rank, PromptBoxShowDirection direction, Vector3 pos)
    {
        var rewardDic =
            GameManager.Task.PersonRankManager.TaskData.GetRewardsByLevel(GameManager.Task.PersonRankManager.RankLevel,
                rank);
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
}