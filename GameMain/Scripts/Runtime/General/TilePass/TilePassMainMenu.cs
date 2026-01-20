using DG.Tweening;
using MySelf.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TilePassMainMenu : UIForm, IItemFlyReceiver, ILifeFlyReceiver
{
    public DelayButton tipButton;
    public DelayButton closeButton;
    public DelayButton activateButton;
    public DelayButton upgradeButton;
    public GameObject VIP;
    public GameObject superVIP;
    public Transform sliderTarget;
    public ParticleSystem sliderEffect;
    public DelayButton sliderButton;
    public SimpleSlider slider;
    public TextMeshProUGUI sliderIndex;
    public Transform sliderStage;
    public SkeletonGraphic sliderEffect1;
    public SkeletonGraphic sliderEffect2;
    public ScrollArea scrollArea;
    public TextPromptBox textPromptBox;
    public ItemPromptBox itemPromptBox;

    public Transform currentLines;
    public Transform currentLineLeft;
    public Transform currentLineRight;
    public Transform airplane;

    public DelayButton claimAllButton;
    public ParticleSystem claimEffect;
    public GameObject coinBar;

    private List<ItemSlot> m_RewardGetTips = new List<ItemSlot>();
    public int lastRecordIndex;
    public GameObject clickObj = null;

    private bool m_LevelButtonClick = false;

    /// <summary>
    /// 获取数据表
    /// </summary>
    public DTTilePassData TilePassData
    {
        get => GameManager.DataTable.GetDataTable<DTTilePassData>().Data;
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        TilePassModel.Instance.RecordTilePassOpen();//记录open界面次数

        RewardManager.Instance.RegisterItemFlyReceiver(this);
        RewardManager.Instance.RegisterLifeFlyReceiver(this);
        base.OnInit(uiGroup, completeAction, userData);

        tipButton.OnInit(OnTipButtonClick);
        closeButton.OnInit(OnClose);
        upgradeButton.OnInit(OnActivateButtonClick);
        activateButton.OnInit(OnActivateButtonClick);
        sliderButton.OnInit(OnTipButtonClick);

        RefreshVIPState();

        //scroll area
        for (int i = 0; i < TilePassData.CurrentTilePassDatas.Count; i++)
        {
            scrollArea.AddColumnLast(new TilePassScrollColumn(270, TilePassData.CurrentTilePassDatas[i], this));
        }
        lastRecordIndex = TilePassModel.Instance.CurrentIndex - 1 >= 0 ? TilePassModel.Instance.CurrentIndex - 1 : 0;
        scrollArea.currentIndex = lastRecordIndex;
        scrollArea.OnSpawnAction += column =>
        {
            RefreshScrollArea();
        };
        scrollArea.OnInit(GetComponent<RectTransform>());

        //current line
        currentLines.transform.localPosition = scrollArea.GetColumnLocalPosition(lastRecordIndex);
        airplane.localPosition = scrollArea.GetColumnLocalPosition(lastRecordIndex);

        sliderEffect.gameObject.SetActive(false);
        sliderEffect1.AnimationState.ClearTracks();
        sliderEffect2.AnimationState.ClearTracks();
        claimEffect.gameObject.SetActive(false);
        TilePassUtil.RecordTilePassShow(); //打点
        m_LevelButtonClick = false;

        GameManager.Ads.HideBanner("TilePass");
    }

    public override void OnReset()
    {
        base.OnReset();

        tipButton.OnReset();
        closeButton.OnReset();
        upgradeButton.OnReset();
        activateButton.OnReset();
        claimAllButton.OnReset();
        sliderButton.OnReset();

        scrollArea.OnReset();
        OnResume();

        clickObj = null;
    }

    public override void OnRelease()
    {
        base.OnRelease();

        TilePassModel.Instance.RecordLastUnClaimedRewardNum();//记录可领取数

        OnReset();

        for (int i = 0; i < m_RewardGetTips.Count; i++)
        {
            m_RewardGetTips[i].transform.DOKill();
            m_RewardGetTips[i].GetComponent<CanvasGroup>().DOKill();
            m_RewardGetTips[i].OnRelease();
            UnityUtility.UnloadInstance(m_RewardGetTips[i].gameObject);
        }
        m_RewardGetTips.Clear();

        if (guideMenu != null)
        {
            UnityUtility.UnloadInstance(guideMenu.gameObject);
            guideMenu = null;
        }

        scrollArea.OnRelease();
        itemPromptBox.OnRelease();

        RewardManager.Instance.UnregisterItemFlyReceiver(this);
        RewardManager.Instance.UnregisterLifeFlyReceiver(this);

        GameManager.Ads.ShowBanner("TilePass");
    }

    public override void OnPause()
    {
        base.OnPause();

        tipButton.interactable = false;
        closeButton.interactable = false;
        upgradeButton.interactable = false;
        activateButton.interactable = false;
        claimAllButton.interactable = false;
        sliderButton.interactable = false;

        scrollArea.scrollRect.vertical = false;

        textPromptBox.HidePromptBox();
        textPromptBox.forbidShow = true;
        itemPromptBox.HidePromptBox();
        itemPromptBox.forbidShow = true;
    }

    public override void OnResume()
    {
        base.OnResume();

        tipButton.interactable = true;
        closeButton.interactable = true;
        upgradeButton.interactable = true;
        activateButton.interactable = true;
        claimAllButton.interactable = true;
        sliderButton.interactable = true;

        scrollArea.scrollRect.vertical = true;

        textPromptBox.forbidShow = false;
        itemPromptBox.forbidShow = false;
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);
        if (scrollArea.scrollRect.verticalNormalizedPosition > 1)
            scrollArea.scrollRect.verticalNormalizedPosition = 1;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (DetectUIObject()) return;
                textPromptBox.HidePromptBox();
                itemPromptBox.HidePromptBox();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (DetectUIObject()) return;
                textPromptBox.HidePromptBox();
                itemPromptBox.HidePromptBox();
            }
        }
    }

    private bool DetectUIObject()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // 获取点击位置的所有UI对象
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == clickObj) return true;
        }
        return false;
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        coinBar.SetActive(false);

        Transform cachedTransform = transform;
        cachedTransform.DOKill();
        cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(true);

        UpdateData();

        //如果当前阶段完成，播动画
        // if (lastRecordIndex != TilePassModel.Instance.CurrentIndex - 1)
        // {
        //     StartCoroutine(ShowRoyalPassProcessAnim(lastRecordIndex, TilePassModel.Instance.CurrentIndex - 1));
        // }
        // else
        // {
        //     RefreshClaimAll();
        //     scrollArea.Refresh();
        // }
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);

        gameObject.SetActive(false);
        OnReset();

        itemPromptBox.HidePromptBox();
        textPromptBox.HidePromptBox();

        if (!m_LevelButtonClick)
            GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
        GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
    }

    public override void OnClose()
    {
        base.OnClose();

        MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        mapTop.tilePassEntrance.RefreshRewardGetWarning();

        if (!IsAvailable) return;

        GameManager.UI.HideUIForm(this);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        if (depthInUIGroup == uiGroupDepth)
        {
            transform.SetAsLastSibling();
        }

        base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
    }

    public override bool CheckInitComplete()
    {
        return scrollArea.CheckSpawnComplete();
    }

    public void RefreshVIPState()
    {
        if (TilePassModel.Instance.IsSuperVIP)
        {
            superVIP.SetActive(true);
            VIP.SetActive(false);
        }
        else
        {
            superVIP.SetActive(false);
            VIP.SetActive(true);
            if (TilePassModel.Instance.IsVIP)
            {
                upgradeButton.gameObject.SetActive(true);
                activateButton.gameObject.SetActive(false);
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
                activateButton.gameObject.SetActive(true);
            }
        }
    }

    public void RefreshClaimAll()
    {
        return;
        int unclaimedCount = 0;
        int canClaimIndex = TilePassModel.Instance.CurrentIndex >= TilePassData.CurrentTilePassDatas.Count ?
            TilePassData.CurrentTilePassDatas.Count - 1 : TilePassModel.Instance.CurrentIndex - 1;
        for (int i = 0; i <= canClaimIndex; i++)
        {
            //free
            if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + i.ToString()))
            {
                unclaimedCount++;
            }
            //vip
            if (TilePassModel.Instance.IsVIP)
            {
                if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + i.ToString()))
                {
                    unclaimedCount++;
                }
            }
        }
        if (unclaimedCount > 0)
        {
            claimAllButton.GetComponentInChildren<TextMeshProUGUILocalize>().SetTerm("TilePass.ClaimAll");
            claimAllButton.OnReset();
            claimAllButton.OnInit(OnClaimAllButtonClick);
        }
        else
        {
            claimAllButton.GetComponentInChildren<TextMeshProUGUILocalize>().SetTerm("Common.Play");
            claimAllButton.OnReset();
            claimAllButton.SetBtnEvent(() =>
            {
                // ProcedureUtil.ProcedureMapToGame();
                m_LevelButtonClick = true;
                OnClose();

                GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                GameManager.UI.ShowUIForm("LevelPlayMenu",form =>
                {
                    form.m_OnHideCompleteAction = () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
                    };
                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
                });
            });
        }
    }

    private void RefreshScrollArea()
    {
        ScrollColumn[] columns = scrollArea.GetColumns("TilePassColumn");
        List<TilePassScrollColumn> columnList = new List<TilePassScrollColumn>();
        for (int i = 0; i < columns.Length; i++)
        {
            TilePassScrollColumn tilePassColumn = (TilePassScrollColumn)columns[i];
            columnList.Add(tilePassColumn);
        }
        columnList.Sort((a, b) =>
        {
            if (a.index < b.index)
                return -1;
            else if (a.index > b.index)
                return 1;
            else
                return 0;
        });

        for (int i = 0; i < columnList.Count; i++)
        {
            if (columnList[i].Instance != null)
            {
                columnList[i].Instance.transform.SetAsFirstSibling();
            }
        }
    }

    /// <summary>
    /// 更新CurrentIndex
    /// </summary>
    private void UpdateData()
    {
        if (TilePassModel.Instance.LastRecordTargetNum == 0 && TilePassModel.Instance.CurrentIndex > 1)
        {
            TilePassModel.Instance.LastRecordTargetNum = TilePassModel.Instance.TotalTargetNum;
        }
        
        int totalTilePassTargetNum = TilePassModel.Instance.TotalTargetNum;
        int lastIndexNeedTargetNum = TilePassData.GetTotalTargetNum(TilePassModel.Instance.CurrentIndex - 1);

        //如果当前阶段完成，更新CurrentIndex，最大值是表长
        //如果已到达最大阶段，跳过
        if (TilePassModel.Instance.CurrentIndex < TilePassData.CurrentTilePassDatas.Count)
        {
            if (totalTilePassTargetNum - lastIndexNeedTargetNum >= TilePassData.CurrentTilePassDatas[TilePassModel.Instance.CurrentIndex].TargetNum)
            {
                int realCurrentIndex = TilePassData.CurrentTilePassDatas.Count;
                int currentIndexNeedTargetNum = lastIndexNeedTargetNum;
                for (int i = TilePassModel.Instance.CurrentIndex; i < TilePassData.CurrentTilePassDatas.Count; i++)
                {
                    currentIndexNeedTargetNum += TilePassData.CurrentTilePassDatas[i].TargetNum;
                    if (currentIndexNeedTargetNum > totalTilePassTargetNum)
                    {
                        realCurrentIndex = i;
                        break;
                    }
                }
                TilePassModel.Instance.CurrentIndex = realCurrentIndex;
            }
        }

        //slider
        //index最大值是30
        if (TilePassModel.Instance.LastRecordTargetNum == TilePassModel.Instance.TotalTargetNum)
        {
            int index = Mathf.Min(TilePassModel.Instance.CurrentIndex, TilePassData.CurrentTilePassDatas.Count - 1);
            sliderIndex.SetText(index.ToString());
            
            lastIndexNeedTargetNum = TilePassData.GetTotalTargetNum(index - 1);
            slider.TotalNum = TilePassData.CurrentTilePassDatas[index].TargetNum;
            slider.CurrentNum = totalTilePassTargetNum - lastIndexNeedTargetNum;
        }
        else
        {
            ShowSliderAnim(lastRecordIndex);
        }
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("TilePassRules");
    }

    private void OnActivateButtonClick()
    {
        upgradeButton.gameObject.SetActive(false);
        activateButton.gameObject.SetActive(false);
        GameManager.UI.ShowUIForm("TilePassActivateMenu");
    }

    private void OnClaimAllButtonClick()
    {
        //foreach (ScrollColumn scrollColumn in scrollArea.scrollColumnList)
        //{
        //    scrollColumn.ClaimAll();
        //}

        bool hasCoinReward = false;
        int canClaimIndex = TilePassModel.Instance.CurrentIndex >= TilePassData.CurrentTilePassDatas.Count ?
            TilePassData.CurrentTilePassDatas.Count - 1 : TilePassModel.Instance.CurrentIndex - 1;
        for (int index = 0; index <= canClaimIndex; index++)
        {
            TilePassData data = TilePassData.CurrentTilePassDatas[index];
            //free
            if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + index.ToString()))
            {
                for (int i = 0; i < data.FreeRewardList.Count; i++)
                {
                    //RewardManager.Instance.SaveRewardData(data.FreeRewardList[i], data.FreeRewardNumList[i], true);
                    RewardManager.Instance.AddNeedGetReward(data.FreeRewardList[i], data.FreeRewardNumList[i]);
                    if (data.FreeRewardList[i] == TotalItemData.Coin) hasCoinReward = true;
                }
                TilePassModel.Instance.AddRewardGetStatus("TilePassFreeRewardGet" + index.ToString());
            }
            //vip
            if (TilePassModel.Instance.IsVIP)
            {
                if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + index.ToString()))
                {
                    for (int i = 0; i < data.VIPRewardList.Count; i++)
                    {
                        //RewardManager.Instance.SaveRewardData(data.VIPRewardList[i], data.VIPRewardNumList[i], true);
                        RewardManager.Instance.AddNeedGetReward(data.VIPRewardList[i], data.VIPRewardNumList[i]);
                        if (data.VIPRewardList[i] == TotalItemData.Coin) hasCoinReward = true;
                    }
                    TilePassModel.Instance.AddRewardGetStatus("TilePassVIPRewardGet" + index.ToString());
                }
            }
        }

        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
        {
            coinBar.SetActive(false);
        }, onCreatePanelSuccess: () =>
        {
            //GameManager.UI.HideUIForm(this);
            if (hasCoinReward)
            {
                coinBar.SetActive(true);
            }
        });

        //刷新
        foreach (ScrollColumn scrollColumn in scrollArea.scrollColumnList)
        {
            scrollColumn.RefreshRewardStatus();
        }
        RefreshClaimAll();

        //claimAllButton.GetComponentInChildren<TextMeshProUGUILocalize>().SetTerm("Common.Play");
        //claimAllButton.OnReset();
        //claimAllButton.SetBtnEvent(() =>
        //{
        //    ProcedureUtil.ProcedureMapToGame();
        //});
    }

    #region anim
    private int _sliderFillCount = 0;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">范围是[1, 30]</param>
    private void ShowSliderAnim(int index)
    {
        OnPause();
        index++;
        index = Mathf.Min(index, TilePassData.CurrentTilePassDatas.Count - 1);
        sliderIndex.SetText(index.ToString());
        
        int lastIndexNeedTargetNum = TilePassData.GetTotalTargetNum(index - 1);
        slider.OnReset();
        slider.TotalNum = TilePassData.CurrentTilePassDatas[index].TargetNum;
        slider.CurrentNum = TilePassModel.Instance.LastRecordTargetNum - lastIndexNeedTargetNum;
        
        float endValue = Mathf.Min(TilePassModel.Instance.TotalTargetNum - lastIndexNeedTargetNum, slider.TotalNum);
        float duration = (endValue - slider.CurrentNum) / slider.TotalNum * 0.5f;
        slider.DOValue(endValue, slider.TotalNum, duration).onComplete += () =>
        {
            //进度条走满播特效
            if (Mathf.Approximately(endValue, slider.TotalNum))
            {
                _sliderFillCount++;
                GameManager.Sound.PlayAudio("SFX_Goldenpass_ActiveReward");
                sliderEffect1.AnimationState.ClearTracks();
                sliderEffect1.AnimationState.SetAnimation(0, "active", false);
                sliderEffect2.AnimationState.ClearTracks();
                sliderEffect2.AnimationState.SetAnimation(0, "level", false);
                sliderStage.localScale = Vector3.one;
                sliderStage.DOPunchScale(new Vector3(0.15f, 0.15f), 0.15f, 1);
            }

            TilePassModel.Instance.LastRecordTargetNum = lastIndexNeedTargetNum + (int)endValue;
            //如果全部完成，补齐超额记录，后续不再走动画
            if (index == TilePassData.CurrentTilePassDatas.Count - 1)
                TilePassModel.Instance.LastRecordTargetNum = TilePassModel.Instance.TotalTargetNum;

            if (index < TilePassModel.Instance.CurrentIndex && index < TilePassData.CurrentTilePassDatas.Count - 1)
                ShowSliderAnim(index);
            else
            {
                //阶段完成动画
                if (_sliderFillCount > 0)
                {
                    _sliderFillCount = 0;
                    StartCoroutine(ShowRoyalPassProcessAnim(lastRecordIndex, TilePassModel.Instance.CurrentIndex - 1));
                }
                else
                {
                    OnResume();
                }
            }
        };
    }
    
    IEnumerator ShowRoyalPassProcessAnim(int oldIndex, int newIndex)
    {
        OnPause();
        scrollArea.CenterTheTargetColumn(oldIndex, 0);
        yield return null;

        Vector3 oldPosition = scrollArea.GetColumnLocalPosition(oldIndex);
        Vector3 newPosition = scrollArea.GetColumnLocalPosition(newIndex);

        //左右边动画
        currentLineLeft.DOScaleX(0, 0.5f);
        currentLineRight.DOScaleX(0, 0.5f).onComplete = () =>
        {
            currentLines.localPosition = newPosition;
        };
        yield return new WaitForSeconds(0.5f);

        GameManager.Sound.PlayAudio("SE_ScrewPass_Move");
        airplane.DOLocalMove(newPosition, 0.5f * (newIndex - oldIndex)).SetEase(Ease.Linear);
        scrollArea.CenterTheTargetColumn(newIndex, (newIndex - oldIndex) * 0.5f, Ease.Linear);

        //更新column
        LinkedListNode<ScrollColumn> column = scrollArea.scrollColumnList.First;
        while (column != null)
        {
            float duration = column.Value.Refresh();
            column = column.Next;
            if (column != null && duration > 0)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        GameManager.Sound.PlayAudio("SFX_goldenpass_missioncomplete");
        foreach (ScrollColumn scrollColumn in scrollArea.scrollColumnList)
        {
            scrollColumn.RefreshRewardStatus();
        }
        RefreshClaimAll();

        //左右边动画
        currentLineLeft.DOScale(1, 0.5f);
        currentLineRight.DOScale(1, 0.5f);
        yield return new WaitForSeconds(0.5f);

        lastRecordIndex = TilePassModel.Instance.CurrentIndex - 1;
        OnResume();
    }

    public void ShowRewardGetTip(TotalItemData type, int num, Vector3 startPos, float delayTime)
    {
        UnityUtility.InstantiateAsync("TilePassRewardGetTip", transform, obj =>
        {
            ItemSlot slot = obj.GetComponent<ItemSlot>();
            m_RewardGetTips.Add(slot);

            slot.OnInit(type, num);
            obj.GetComponent<CanvasGroup>().alpha = 1;

            Transform cachedTrans = obj.transform;
            cachedTrans.localScale = Vector3.zero;
            obj.SetActive(true);

            cachedTrans.position = startPos;

            cachedTrans.DOScale(0, 0).SetDelay(delayTime).onComplete = () =>
            {
                cachedTrans.DOScale(1, 0.2f);
                cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 1f).SetEase(Ease.InSine);
                obj.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetDelay(0.6f).onComplete = () =>
                {
                    m_RewardGetTips.Remove(slot);
                    slot.OnRelease();
                    UnityUtility.UnloadInstance(obj);
                };
            };
        });
    }
    #endregion

    #region guide
    private TilePassGuideMenu guideMenu;

    private void OnEnable()
    {
        StartCoroutine(ShowGuide());
    }

    IEnumerator ShowGuide()
    {
        if (!TilePassModel.Instance.ShowedGuide)
        {
            yield return new WaitUntil(scrollArea.CheckSpawnComplete);

            UnityUtility.InstantiateAsync("TilePassGuideMenu", transform, menu =>
            {
                guideMenu = menu.GetComponent<TilePassGuideMenu>();
                guideMenu.GuideBg.InitClick();

                // if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet0"))
                // {
                //     ShowGuide1();
                // }
                // else
                // {
                    ShowGuide2();
                // }
            });
        }
    }

    private void ShowGuide1()
    {
        if (guideMenu == null || scrollArea.GetColumn("TilePassColumn") == null || scrollArea.GetColumn("TilePassColumn").Instance == null) return;

        TilePassColumn column = scrollArea.GetColumn("TilePassColumn").Instance.GetComponent<TilePassColumn>();
        Transform target = column.freeButton.transform;
        Transform originalParent = target.parent;
        Vector3 originalLocalPos = target.localPosition;
        target.SetParent(guideMenu.transform);

        guideMenu.showBlackBg = true;
        guideMenu.SetText("TilePass.Guide1");
        guideMenu.ShowNpc(target.position.y - 0.45f);
        guideMenu.Showhand(target.position.x + 0.02f, target.position.y - 0.08f);
        guideMenu.OnShow();
        column.freeButton.onClick.AddListener(() =>
        {
            target.SetParent(originalParent);
            target.localPosition = originalLocalPos;
            guideMenu.OnReset();

            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                if (guideMenu != null)
                {
                    guideMenu.OnHide();
                    ShowGuide2();
                }
            });
        });
        column.freeClaimButton.onClick.AddListener(() =>
        {
            target.SetParent(originalParent);
            target.localPosition = originalLocalPos;
            guideMenu.OnReset();

            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                if (guideMenu != null)
                {
                    guideMenu.OnHide();
                    ShowGuide2();
                }
            });
        });
    }

    private void ShowGuide2()
    {
        if (guideMenu == null) return;

        if (TilePassModel.Instance.ShowedGuide) return;

        Transform target = VIP.transform;
        Transform originalParent = target.parent;
        Vector3 originalLocalPos = target.localPosition;
        target.SetParent(guideMenu.transform);

        guideMenu.showBlackBg = true;
        guideMenu.SetText("TilePass.Guide2");
        TextMeshProUGUILocalize loc = guideMenu.textPromptBox.promptText.GetComponent<TextMeshProUGUILocalize>();
        loc.SetParameterValue("0", "<color=#1c7800>");
        loc.SetParameterValue("1", "</color>");
        guideMenu.ShowNpc(target.position.y - 0.45f);
        guideMenu.Showhand(target.position.x + 0.01f, target.position.y - 0.07f);
        guideMenu.OnShow();
        activateButton.onClick.AddListener(() =>
        {
            target.SetParent(originalParent);
            target.localPosition = originalLocalPos;

            if (guideMenu != null)
            {
                guideMenu.OnHide();
                guideMenu.OnReset();
                Destroy(guideMenu.gameObject);
            }

            TilePassModel.Instance.ShowedGuide = true;
        });
    }
    #endregion

    #region reciver
    public ReceiverType ReceiverType => ReceiverType.Common;

    public GameObject GetReceiverGameObject() => gameObject;

    private bool startSliderAnim = true;

    public void OnFlyHit(TotalItemData type)
    {
        if (type == TotalItemData.Gasoline)
        {
            //受击动画
            if (sliderTarget != null)
            {
                GameManager.Sound.PlayAudio("SFX_itemget");
                sliderTarget.localScale = Vector3.one;
                sliderTarget.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1);
                
                // sliderTarget.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                // {
                //     sliderTarget.transform.localScale = Vector3.one;
                //
                //     if (sliderEffect != null)
                //     {
                //         sliderEffect.gameObject.SetActive(true);
                //         sliderEffect.Play();
                //     }
                //
                //     if (startSliderAnim && TilePassModel.Instance.CurrentIndex < TilePassData.CurrentTilePassDatas.Count)
                //     {
                //         startSliderAnim = false;
                //         SliderAnim();
                //     }
                // };
            }
        }
    }

    private void SliderAnim()
    {
        sliderIndex.gameObject.SetActive(false);
        slider.sliderText.gameObject.SetActive(false);

        float duration = (1 - slider.Value) * 0.2f;
        slider.DOValue(1, duration).onComplete = () =>
        {
            slider.Value = 0;
            SliderAnim();
        };
    }

    public void OnFlyEnd(TotalItemData type)
    {
        if (type == TotalItemData.Gasoline)
        {
            GameManager.Sound.PlayAudio("SFX_itemget");
            sliderTarget.localScale = Vector3.one;
            sliderTarget.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1);
            OnShow();
            return;
            
            //受击动画
            if (sliderTarget != null)
            {
                GameManager.Sound.PlayAudio("SFX_itemget");
                sliderTarget.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                {
                    sliderTarget.transform.localScale = Vector3.one;

                    if (sliderEffect != null)
                    {
                        sliderEffect.gameObject.SetActive(true);
                        sliderEffect.Play();
                    }

                    if (TilePassModel.Instance.CurrentIndex < TilePassData.CurrentTilePassDatas.Count)
                    {
                        slider.transform.DOKill();
                        float duration = (1 - slider.Value) * 0.2f;
                        slider.DOValue(1, duration).onComplete = () =>
                        {
                            slider.Value = 0;
                            OnShow();
                        };
                    }
                    else
                    {
                        foreach (ScrollColumn scrollColumn in scrollArea.scrollColumnList)
                        {
                            scrollColumn.RefreshRewardStatus();
                        }
                        RefreshClaimAll();
                    }
                };
            }
        }
        else
        {
            if (claimAllButton != null)
            {
                claimAllButton.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                {
                    claimAllButton.transform.localScale = Vector3.one;

                    if (claimEffect != null)
                    {
                        claimEffect.gameObject.SetActive(true);
                        claimEffect.Play();
                    }
                };
            }
        }
    }

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        if (type == TotalItemData.Gasoline) return sliderTarget.transform.position;
        else return claimAllButton.transform.position;
    }

    public Vector3 LifeFlyTargetPos => claimAllButton.transform.position;

    public void Show()
    {
    }

    public void OnLifeFlyHit()
    {
    }

    public void OnLifeFlyEnd()
    {
        OnFlyEnd(TotalItemData.Life);
    }
    #endregion
}
