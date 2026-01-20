using MySelf.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationViewPanel : CenterForm, ICustomOnEscapeBtnClicked
{
    public ScrollArea scrollArea;

    [SerializeField]
    private DelayButton closeBtn;

    public static string RecordCurDownloadName;
    public static Action RecordCurDownloadAction;

    private void OnDisable()
    {
        RecordCurDownloadName = string.Empty;
        RecordCurDownloadAction = null;
    }

    public bool InAnim
    {
        get { return inAnim; }
        set { inAnim = value; }
    }
    private bool inAnim = false;
    private bool showUnlockNewAreaAnim = false;
    public bool ShowUnlockNewAreaAnim
    {
        get { return showUnlockNewAreaAnim; }
    }
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        int needAddPanel = Constant.GameConfig.MaxDecorationArea;

        ////更改成反向填充
        //if (decorationPanels.Count == 0)
        //{
        //    for (int i = 0; i < needAddPanel; i++)
        //    {
        //        decorationPanels.Add(Instantiate(originPanel, originPanel.transform.parent).GetComponentInChildren<DecorationPanel>());
        //    }
        //}

        bool recentAreaIsComplete = DecorationModel.Instance.CheckTargetAreaIsComplete(DecorationModel.Instance.Data.DecorationAreaID);
        bool existNextCanDecorateArea = DecorationModel.Instance.Data.DecorationAreaID < Constant.GameConfig.MaxDecorationArea;
        showUnlockNewAreaAnim = false;
        if (recentAreaIsComplete && existNextCanDecorateArea)
        {
            showUnlockNewAreaAnim = true;
            DecorationModel.Instance.SetTryToOverrideDecorationAreaID(DecorationModel.Instance.Data.DecorationAreaID + 1);
        }


        if (scrollArea.Count == 0)
        {
            CheckDecorationAreaThumbnail();

            //添加ComingSoon
            scrollArea.AddColumnFirst(new DecorationScrollPanel("DecorationPanelComingSoon", needAddPanel + 1, scrollArea, 800.0f));

            for (int i = needAddPanel; i > 0; i--)
            {
                scrollArea.AddColumnLast(new DecorationScrollPanel("DecorationPanel", i, scrollArea, 800.0f));
            }

            scrollArea.SetColumnCountDelta(2);
            scrollArea.currentIndex = needAddPanel - DecorationModel.Instance.Data.DecorationAreaID + 1;
            //特殊处理一下 基本上是当前所有区域都装修完时 定位到ComingSoon
            if (DecorationModel.Instance.CheckTargetAreaIsComplete(DecorationModel.Instance.Data.DecorationAreaID) &&
                (DecorationModel.Instance.Data.DecorationAreaID == Constant.GameConfig.MaxDecorationArea))
            {
                if (!DecorationModel.Instance.NeedToShowDecorationViewAnim)
                    scrollArea.currentIndex -= 1;
            }

            if (DecorationModel.Instance.NeedToShowDecorationViewAnim)
            {
                DecorationModel.Instance.NeedToShowDecorationViewAnim = false;
                showUnlockNewAreaAnim = true;
            }

            scrollArea.OnInit(GetComponent<RectTransform>());
        }
        else
        {
            scrollArea.Refresh();
        }

        //int flag = 0;
        //for (int i = needAddPanel; i > 0; i--)
        //{
        //    decorationPanels[flag].InitializePanel(i);
        //    decorationPanels[flag].gameObject.SetActive(true);
        //    flag++;
        //}

        SetScrollRectInteract(true);
        //bool showCloseBtn = (bool)userData;
        bool showCloseBtn = !showUnlockNewAreaAnim;
        closeBtn.gameObject.SetActive(showCloseBtn);
        closeBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });



        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        if (showUnlockNewAreaAnim)
        {
            StartUnlockNewAreaAnim();
        }
        base.OnShowInit(showInitSuccessAction, userData);
    }

    public override void OnRelease()
    {
        scrollArea.OnRelease();

        base.OnRelease();

        //GameManager.ObjectPool.DestroyObjectPool("DecorationPanelPool");
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
        OnReset();
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }

    public override bool CheckInitComplete()
    {
        bool allIsComplete = true;
        bool anyInstanceIsLoaded = false;
        foreach (var tmp in scrollArea.scrollColumnList)
        {
            if (tmp.Instance == null)
            {
                continue;
            }
            anyInstanceIsLoaded = true;
            DecorationPanel script = tmp.Instance.GetComponent<DecorationPanel>();
            if (script != null)
            {
                if (!script.CheckInitComplete())
                {
                    allIsComplete = false;
                }
            }
        }
        return anyInstanceIsLoaded && allIsComplete;
    }

    private void Update()
    {
        scrollArea.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
    }

    private void CheckDecorationAreaThumbnail()
    {
        //UnityUtility.HasAsset("Area4_Empty", typeof(Sprite), res =>
        //{
        //    if (res == HasAssetResult.NeedDownload)
        //    {
        //        GameManager.Download.AddDownload("Area4_Empty");
        //    }
        //});

        //for (int i = 5; i <= Constant.GameConfig.MaxDecorationArea; i++)
        //{
        //    string assetName = $"Area{i}_Empty";
        //    UnityUtility.HasAsset(assetName, typeof(Sprite), res =>
        //    {
        //        if (res == HasAssetResult.NeedDownload)
        //        {
        //            GameManager.Download.AddDownload(assetName);
        //        }
        //    });
        //}
    }

    private void SetScrollRectInteract(bool interactable)
    {
        //scrollArea.scrollRect.vertical = interactable;
        scrollArea.scrollRect.enabled = interactable;
    }

    //public void OnAreaStateChange(object sender, GameEventArgs e)
    //{
    //    MapAreaStateChangeEventArgs ne = (MapAreaStateChangeEventArgs)e;

    //    SetScrollRectInteract(ne.State == MapAreaState.Static);
    //}

    //private void OnDownloadSuccess(object sender, GameEventArgs e)
    //{
    //    DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;

    //    if (ne.DownloadKey.StartsWith("Area"))
    //    {
    //        scrollArea.Refresh();
    //    }
    //}

    private void StartUnlockNewAreaAnim()
    {
        if (inAnim)
        {
            Log.Error("StartUnlockNewAreaAnim called twice -> return");
            return;
        }

        inAnim = true;
        SetScrollRectInteract(false);

        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            StartCoroutine(PlayLastChapterFinishAnimCoroutine());
        });
    }

    IEnumerator PlayLastChapterFinishAnimCoroutine()
    {
        int recentAreaID = DecorationModel.Instance.Data.DecorationAreaID;
        //先更新已完成的章节 (ID = recentAreaID)
        //onAnimFinished 回调中再移动滑动Panel 再更新新章节(ID = recentAreaID + 1)
        int tryTimes = 0;
        int maxTryTimes = 30;
        while (tryTimes <= maxTryTimes)
        {
            bool everFind = false;
            foreach (var tmp in scrollArea.scrollColumnList)
            {
                if (tmp.Instance == null)
                {
                    continue;
                }
                DecorationPanel script = tmp.Instance.GetComponent<DecorationPanel>();
                if (script != null)
                {
                    //找到ID和recentAreaID 一样的才算真找到，找到一个就break掉
                    if (script.PlayLastChapterFinishAnim(recentAreaID, OnLastChapterAnimFinished))
                    {
                        everFind = true;
                        break;
                    }
                }
            }
            if (!everFind)
            {
                tryTimes++;
                yield return new WaitForSeconds(0.1f);
            }
            else
                break;
        }
        if (tryTimes >= maxTryTimes)
        {
            OnLastChapterAnimFinished();
        }
    }


    private void OnLastChapterAnimFinished()
    {
        StartCoroutine(ScrollAndPlayRecentChapterUnlockAnimCoroutine());
    }

    IEnumerator ScrollAndPlayRecentChapterUnlockAnimCoroutine()
    {
        //向上滚动一个
        scrollArea.CenterTheTargetColumn(scrollArea.currentIndex - 1, 0.5f);
        yield return new WaitForSeconds(0.5f);

        //真正数据推进到下一个Area的地方
        int recentAreaID = DecorationModel.Instance.Data.DecorationAreaID;
        DecorationModel.Instance.SetDecorationAreaID(recentAreaID + 1);
        DecorationModel.Instance.SetTryToOverrideDecorationAreaID(-1);

        //更新新解锁的章节
        foreach (var tmp in scrollArea.scrollColumnList)
        {
            if (tmp.Instance == null)
            {
                continue;
            }
            DecorationPanel script = tmp.Instance.GetComponent<DecorationPanel>();
            if (script != null)
            {
                script.PlayRecentChapterUnlockAnim(recentAreaID + 1);
            }
        }
        yield return new WaitForSeconds(1.0f);

        //if (recentAreaID == DecorationModel.Instance.Data.DecorationAreaID)
        //{
        //    //这应该是意味着 SetDecorationAreaID(recentAreaID + 1); 失败了
        //    //进而应该是因为已经达到了当前装修的最大区域ID
        //    //这种情况我们把返回按钮打开
        //    closeBtn.gameObject.SetActive(true);
        //}
        //1.11.3开始这里并不强制玩家进入下一Area 即此时全都打开closeBtn
        closeBtn.gameObject.SetActive(true);


        //等待一段时间 确保解锁动画播放完毕 才允许玩家拖动
        //因为解锁动画上有预期只会横向移动的TrailRenderer，如果上下拖动ScrollRect的话 会非常恐怖
        yield return new WaitForSeconds(0.3f);
        inAnim = false;
        SetScrollRectInteract(true);
    }

    public void OnEscapeBtnClicked()
    {
        if (inAnim)
            return;
        if (!closeBtn.isActiveAndEnabled)
            return;

        closeBtn?.onClick?.Invoke();
    }
}