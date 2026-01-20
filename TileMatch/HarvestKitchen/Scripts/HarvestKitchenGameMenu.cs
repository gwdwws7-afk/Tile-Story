using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HarvestKitchenGameMenu : CenterForm
{
    private bool isStartTileMove = false;
    // 是否能关闭教程
    private bool canCloseGuide = true;
    // 当前显示的顾客编号
    private int currentCustomerIndex = 0;
    // 当前服务的顾客波数
    private int currentWaveIndex = 0;
    // 当前点赞的数量
    private int currentPraiseNum = 0;
    // 最大点赞数量
    private int MaxPraiseNum = 0;
    // 开始增长速度的行数
    private int startIncreaseSpeedLineNum;
    // 两个tilePanelPrefab之间的间距
    public float differValue = 134f;
    // 棋盘上升的速度，由生成新一行棋子的速度决定
    private float speed;// => KitchenManager.Instance.tileItemSpeed;
    // 上升速度增长速率
    private float speedGrowthRate;
    // 消除金币棋子单次获得的金币数量
    private int coinNum = 10;
    // 新生成的 tilePanelPrefab 所在的位置
    private Vector3 lastPosition = Vector3.zero;
    
    [SerializeField] private GameObject tilePanelPrefab;
    [SerializeField] private GameObject guide;
    [SerializeField] private GameObject guideBg;
    
    [SerializeField] private Transform tileMovePanel;
    [SerializeField] private Transform customerParent;
    [SerializeField] private Transform customerInitTarget;
    [SerializeField] private Transform customerLeaveTarget;
    // 只有一个顾客时，顾客所在位置
    [SerializeField] private Transform oneCustomerTarget;
    [SerializeField] private Transform[] customerTarget;
    
    [SerializeField] private DelayButton closeBtn;
    [SerializeField] private HarvestKitchenChosenBar chosenBar;
    [SerializeField] private CoinBarManager coinBar;
    [SerializeField] private TextMeshProUGUI currentLisksNumText;
    [SerializeField] private TextMeshProUGUI totalCustomerNumText;

    private HarvestKitchenGamePanelData panelData;
    
    [SerializeField] private Image cordon;
    [SerializeField] private Image topFail;
    [SerializeField] private Image chosenFail;

    [SerializeField] private GameObject kitchenFlyObj;
    [SerializeField] private TextMeshProUGUI coinFadeText;
    
    private List<int> customerIdList;
    
    [SerializeField] private GameObject[] guideObj;
    
    [SerializeField] private List<HarvestKitchenTileItemPanel> kitchenTileItemPanelPool;
    [SerializeField] private List<HarvestKitchenTileItemPanel> noUseKitchenTileItemPanelPool;

    private List<HarvestKitchenTileItem> kitchenTileItemPool;
    
    // 当前服务的顾客，最大为2
    private List<HarvestKitchenCustomer> currentCustomerList;
    
    public List<HarvestKitchenCustomer> KitchenCustomerPool;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Sound.PlayMusic(SoundType.SFX_Kitchen_Match_Level_Harvest_BGM.ToString());

        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEventCallBack);
        
        DTHarvestKitchenTaskDatas currentTaskData = HarvestKitchenManager.Instance.GetCurrentTaskDatas();
        startIncreaseSpeedLineNum = currentTaskData.StartingLineNum;
        speed = currentTaskData.StartingSpeed;
        speedGrowthRate = currentTaskData.GrowthRate;

        // 初始化关卡数据
        panelData = new HarvestKitchenGamePanelData();

        currentCustomerIndex = 0;
        currentWaveIndex = 0;

        currentPraiseNum = 0;
        MaxPraiseNum = panelData.GetTotalCustomerNum();
        currentLisksNumText.text = currentPraiseNum.ToString();
        totalCustomerNumText.text = MaxPraiseNum.ToString();
        
        lastPosition = Vector3.zero;
        
        customerIdList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        
        closeBtn.SetBtnEvent(OnCloseBtnClicked);
        closeBtn.interactable = false;
        
        cordon.gameObject.SetActive(false);

        // 清空当前餐厅的两个顾客位置
        currentCustomerList = new List<HarvestKitchenCustomer>();

        tileMovePanel.localPosition = new Vector3(0, -140f, 0);

        // 每次调用OnRelease后都需要重新申请空间，否则在关闭界面调用OnRelease时会报错
        kitchenTileItemPool = ListPool<HarvestKitchenTileItem>.Get();
        kitchenTileItemPanelPool = ListPool<HarvestKitchenTileItemPanel>.Get();
        noUseKitchenTileItemPanelPool = ListPool<HarvestKitchenTileItemPanel>.Get();
        KitchenCustomerPool = ListPool<HarvestKitchenCustomer>.Get();
        
        // 初始化棋子
        if (kitchenTileItemPanelPool.Count + noUseKitchenTileItemPanelPool.Count < 9)
        {
            for (int i = kitchenTileItemPanelPool.Count + noUseKitchenTileItemPanelPool.Count; i < 9; i++)
            {
                GameObject tilePanel = Instantiate(tilePanelPrefab, tileMovePanel);
                tilePanel.name = i.ToString();
                HarvestKitchenTileItemPanel panel = tilePanel.GetComponent<HarvestKitchenTileItemPanel>();

                if (kitchenTileItemPanelPool.Count < 4)
                {
                    panel.Init(i == 0 ? null : kitchenTileItemPanelPool[i - 1], panelData.coinProbability,
                        panelData.CurrentTileTypeList, panelData.tileTypeProbabilityList, null, RecoveryTileItemPanel,
                        OnTileClick, ref kitchenTileItemPool);
                    kitchenTileItemPanelPool.Add(panel);
                }
                else
                {
                    noUseKitchenTileItemPanelPool.Add(panel);
                }
                panel.transform.localPosition = lastPosition;
                panel.gameObject.SetActive(true);
                lastPosition.y -= differValue;
            }
        }
        
        // 创建顾客池
        InitKitchenCustomerPool();
        
        // 隐藏Banner
        GameManager.Ads.HideBanner("Kitchen");
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        guide.SetActive(false);
        
        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        // 游戏开场，新顾客进入
        if (currentCustomerList != null) 
        {
            NewCustomerEnter(() =>
            {
                if (!HarvestKitchenManager.Instance.IsShowGameGuide)
                {
                    // 顾客进入后开启游戏
                    isStartTileMove = true;
                    HarvestKitchenManager.Instance.canClickTile = true;
                    closeBtn.interactable = true;
                }
                else
                {
                    HarvestKitchenManager.Instance.IsShowGameGuide = false;
                    ShowGameGuide(0);
                }
            });
        }
    }

    public override void OnReveal()
    {
        gameObject.SetActive(true);
    }

    private float time = 0;
    // 刷新棋子行的等待时间
    private float refreshTilePanelTime = 0;
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (isStartTileMove)
        {
            tileMovePanel.localPosition += Vector3.up * (speed * elapseSeconds);
            time += elapseSeconds;
            refreshTilePanelTime += elapseSeconds;
            
            if (time >= 0.2f)
            {
                time -= 0.2f;

                if (kitchenTileItemPanelPool.Count <= 0) return;

                // 子节点在父节点的父节点中的y轴位置，368f为顶层棋子触碰顶部的位置
                if (kitchenTileItemPanelPool[0].transform.localPosition.y + tileMovePanel.localPosition.y >= 368f)
                {
                    HarvestKitchenManager.Instance.toContinueMenuType = 0;
                    GamePause();
                    topFail.color = new Color(1, 1, 1, 0);
                    topFail.gameObject.SetActive(true);
                    topFail.DOFade(1, 1f).onComplete += () =>
                    {
                        ShowGameContinue();
                    };
                }

                if (kitchenTileItemPanelPool[0].transform.localPosition.y + tileMovePanel.localPosition.y >= 167f)
                {
                    if (!isPlayCordonAnim)
                    {
                        // 播放Cordon闪烁动画
                        SetCordonAnim(true);
                        if (HarvestKitchenManager.Instance.IsShowTopGuide)
                        {
                            HarvestKitchenManager.Instance.IsShowTopGuide = false;
                            ShowGameGuide(1);
                        }
                    }
                }
                else
                {
                    SetCordonAnim(false);
                }
            }

            // differValue / speed为刷新新一行棋子的时间间隔，当前速度的增加为越接近速度上限速度增长越快
            if (refreshTilePanelTime >= differValue / speed)
            {
                refreshTilePanelTime -= differValue / speed;
                
                // 加快棋盘上升的速度
                if (speed < 40)
                {
                    // 在未到达目标行数前，速度不增长
                    if (startIncreaseSpeedLineNum > 0)
                        startIncreaseSpeedLineNum--;
                    else
                        speed += speedGrowthRate;
                }
                else
                {
                    speed = 40;
                }

                // 刷新下方的棋子
                HarvestKitchenTileItemPanel panel = null;
                if (noUseKitchenTileItemPanelPool.Count <= 0)
                {
                    GameObject tilePanel = Instantiate(tilePanelPrefab, tileMovePanel);
                    panel = tilePanel.GetComponent<HarvestKitchenTileItemPanel>();
                    panel.transform.localPosition = lastPosition;
                    panel.gameObject.SetActive(true);
                    lastPosition.y -= differValue;
                }
                else
                {
                    panel = noUseKitchenTileItemPanelPool[0];
                    noUseKitchenTileItemPanelPool.RemoveAt(0);
                }

                //客人当前需要的食品类型
                List<int> neededFoodsId = new List<int>();
                for (int i = 0; i < currentCustomerList.Count; i++)
                {
                    List<int> foodsId = currentCustomerList[i].GetNeededFoodsId();
                    foreach (int id in foodsId)
                    {
                        if (!neededFoodsId.Contains(id))
                            neededFoodsId.Add(id);
                    }
                }

                panel.Init(kitchenTileItemPanelPool.Count > 0 ? kitchenTileItemPanelPool.Last() : null,
                    panelData.coinProbability, panelData.CurrentTileTypeList, panelData.tileTypeProbabilityList, neededFoodsId,
                    RecoveryTileItemPanel, OnTileClick, ref kitchenTileItemPool);
                kitchenTileItemPanelPool.Add(panel);

                Log.Info($"当前速度为：{speed}");
            }
        }

        if (canCloseGuide)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
            {
                canCloseGuide = false;

                if (oldParent != null)
                {
                    customerParent.SetParent(oldParent);
                    customerParent.SetSiblingIndex(1);
                }
                
                guide.SetActive(false);
                // 开启游戏
                GameContinue();
                closeBtn.interactable = true;
                
                UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
            }
        }
        
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private bool isPlayCordonAnim = false;
    /// <summary>
    /// 播放触顶的红色提示
    /// </summary>
    /// <param name="state">显示的状态</param>
    private void SetCordonAnim(bool state)
    {
        if (state)
        {
            cordon.color = new Color(1, 1, 1, 0.3f);
            cordon.gameObject.SetActive(true);
            cordon.DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
            isPlayCordonAnim = true;
        }
        else
        {
            cordon.DOKill();
            cordon.gameObject.SetActive(false);
            isPlayCordonAnim = false;
        }
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEventCallBack);

        // 隐藏失败提示，防止重新开始游戏时失败提示依旧存在
        topFail.gameObject.SetActive(false);
        chosenFail.gameObject.SetActive(false);

        // 回收顾客
        if (currentCustomerList != null)
        {
            while (currentCustomerList.Count > 0)
            {
                var customer = currentCustomerList[0];
                customer.gameObject.SetActive(false);
                customer.transform.localPosition = customerInitTarget.localPosition;
                if (!KitchenCustomerPool.Contains(customer))
                    KitchenCustomerPool.Add(customer);
                currentCustomerList.RemoveAt(0);
            }
        }

        if (KitchenCustomerPool != null)
        {
            for (int i = 0; i < KitchenCustomerPool.Count; i++)
            {
                KitchenCustomerPool[i].OnRelease();
            }
            if (KitchenCustomerPool.Count > 0)
            {
                ListPool<HarvestKitchenCustomer>.Release(KitchenCustomerPool);
            }
            KitchenCustomerPool.Clear();
        }
        
        // 回收棋子列
        if (kitchenTileItemPanelPool != null)
        {
            for (int i = 0; i < kitchenTileItemPanelPool.Count; i++)
            {
                kitchenTileItemPanelPool[i].OnRelease();
            }
            if (kitchenTileItemPanelPool.Count > 0)
            {
                ListPool<HarvestKitchenTileItemPanel>.Release(kitchenTileItemPanelPool);
            }
            kitchenTileItemPanelPool.Clear();
        }

        if (noUseKitchenTileItemPanelPool != null)
        {
            for (int i = 0; i < noUseKitchenTileItemPanelPool.Count; i++)
            {
                noUseKitchenTileItemPanelPool[i].OnRelease();
            }
            if (noUseKitchenTileItemPanelPool.Count > 0)
            {
                ListPool<HarvestKitchenTileItemPanel>.Release(noUseKitchenTileItemPanelPool);
            }
            noUseKitchenTileItemPanelPool.Clear();
        }
        
        // 回收棋子
        if (kitchenTileItemPool != null)
        {
            for (int i = 0; i < kitchenTileItemPool.Count; i++)
            {
                kitchenTileItemPool[i].OnRelease();
            }
            if (kitchenTileItemPool.Count > 0)
            {
                ListPool<HarvestKitchenTileItem>.Release(kitchenTileItemPool);
            }
            kitchenTileItemPool.Clear();
        }
        
        // 清空消除栏
        chosenBar.OnRelease();
        // 清除加载的数据
        if (panelData != null) panelData.OnRelease();
        // 显示Banner
        GameManager.Ads.ShowBanner("Kitchen");
        // 清除所有协程
        StopAllCoroutines();
        
        base.OnRelease();
    }

    /// <summary>
    /// 游戏继续
    /// </summary>
    public void GameContinue()
    {
        canfocusPause = true;
        
        // 开启棋盘移动
        isStartTileMove = true;
        // 开启顾客情绪值降低
        for (int i = 0; i < currentCustomerList.Count; i++)
            currentCustomerList[i].GameContinue();
        // 开启棋子点击事件
        HarvestKitchenManager.Instance.canClickTile = true;
    }

    /// <summary>
    /// 游戏暂停
    /// </summary>
    /// <param name="isNoFocus">是否是由于焦点丢失</param>
    public void GamePause(bool isNoFocus = true)
    {
        if (isNoFocus)
            canfocusPause = false;
            
        // 停止棋盘移动
        isStartTileMove = false;
        // 停止顾客情绪值降低
        for (int i = 0; i < currentCustomerList.Count; i++)
            currentCustomerList[i].GamePause();
        // 关闭棋子点击事件
        HarvestKitchenManager.Instance.canClickTile = false;
    }

    /// <summary>
    /// 回收棋子行
    /// </summary>
    /// <param name="panel">棋子行</param>
    public void RecoveryTileItemPanel(HarvestKitchenTileItemPanel panel)
    {
        kitchenTileItemPanelPool.Remove(panel);
        // 清除下一行对当前行的引用，被回收的永远都是第一行
        if(kitchenTileItemPanelPool.Count > 0)
            kitchenTileItemPanelPool[0].upTileItemPanel = null;

        panel.transform.localPosition = lastPosition;
        panel.transform.SetAsLastSibling();
        panel.gameObject.SetActive(true);
        lastPosition.y -= differValue;
        noUseKitchenTileItemPanelPool.Add(panel);
    }

    private void OnCloseBtnClicked()
    {
        //玩家主动点击退出按钮时
        HarvestKitchenManager.Instance.toContinueMenuType = 1;
        GamePause();
        ShowGameContinue();
    }

    // 点击棋子后的回调
    private void OnTileClick(HarvestKitchenTileItem item)
    {
        GameManager.Sound.PlayAudio(SoundType.SFX_ClickTile_new.ToString());
        
        // 棋子飞入下方的选择栏
        bool gameLose = chosenBar.RecordChooseTile(item, (tileItem) =>
        {
            tileItem.gameObject.SetActive(false);
            tileItem.transform.DOKill();
            if(!kitchenTileItemPool.Contains(tileItem))
                kitchenTileItemPool.Add(tileItem);
        });

        if (!gameLose && HarvestKitchenManager.Instance.IsShowChooseBarGuide)
        {
            if (chosenBar.GetChooseTotalNum() == 6)
            {
                HarvestKitchenManager.Instance.IsShowChooseBarGuide = false;
                ShowGameGuide(2);
            }
        }
        
        if (gameLose)
        {
            HarvestKitchenManager.Instance.toContinueMenuType = 0;
            GamePause();
            chosenFail.color = new Color(1, 1, 1, 0);
            chosenFail.gameObject.SetActive(true);
            chosenFail.DOFade(1, 1f).onComplete += () =>
            {
                ShowGameContinue();
            };
        }
    }

    /// <summary>
    /// 方块消除的处理
    /// </summary>
    /// <param name="item">消除的方块类型</param>
    /// <param name="initPos">消除的方块的中心位置</param>
    /// <returns>是否有顾客需要该材料</returns>
    public bool FoodElimination(HarvestKitchenTileItem item, Vector3 initPos)
    {
        // 判断是否是金币，金币直接飞入金币栏
        if (item.itemID == 43)
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Get_Level_Coin);
            StartCoroutine(ShowGetCoinAnim(initPos));
        
            return true;
        }
        
        // 判断是否有顾客需要该材料
        for (int i = 0; i < currentCustomerList.Count; i++)
        {
            var customer = currentCustomerList[i];
            int needIndex = customer.IsNeedFood(item.itemID);
            if (needIndex >= 0)
            {
                List<int> num = new List<int>();
                // 使用AddRange避免动画棋子飞行动画完成后刷新需求显示的值提前发生了变动
                num.AddRange(customer.GetFood(item.itemID));
                
                if (currentCustomerList.Count <= 1 && num.Sum() <= 0 && !HasNewCustomer())
                {
                    isStartTileMove = false;
                    HarvestKitchenManager.Instance.canClickTile = false;
                }

                GameManager.Task.AddDelayTriggerTask(0.25f, () =>
                {
                    // 拷贝 icon 用于飞行动画
                    var icon = Instantiate(kitchenFlyObj, initPos, Quaternion.identity, customer.transform).transform;
                    KitchenFlyObj flyObj = icon.GetComponent<KitchenFlyObj>();
                    flyObj.m_IconImage.sprite = HarvestKitchenManager.Instance.GetTileSpriteById(item.itemID);
                    flyObj.m_IconImage.SetNativeSize();
                    icon.localScale = Vector3.zero;
                    icon.gameObject.SetActive(true);

                    icon.DOScale(1.4f, 0.15f).SetEase(Ease.InOutCubic).onComplete = () =>
                    {
                        icon.DOScale(1.35f, 0.15f).SetEase(Ease.InCubic);
                    };

                    Vector3 targetPos = customer.needImg[needIndex].transform.position;
                    icon.DOScale(1f, 0.4f).SetEase(Ease.InOutSine).SetDelay(0.3f);
                    icon.DOMove(targetPos, 0.4f).SetEase(Ease.InOutSine).SetDelay(0.3f).onComplete += () =>
                    {
                        icon.DOKill();
                        // 食物飞到气泡上播放音效
                        GameManager.Sound.PlayAudio("SFX_itemget");
                        UnityUtil.EVibatorType.Medium.PlayerVibrator();

                        customer.ReduceFlyAnimNum();

                        // 判断当前的顾客是否离去
                        if (customer.CheckCustomerState())
                        {
                            // 等待顾客离开，下一个顾客开始入场
                            customer.SetLeaveEvent(() =>
                            {
                                // 增加点赞数量
                                currentPraiseNum++;
                                currentLisksNumText.text = currentPraiseNum.ToString();
                                // 推迟清空顾客的逻辑可以 防止同时完成两个顾客的需求时出现老顾客和新顾客重叠在一起的情况
                                // 清空当前位置顾客
                                currentCustomerList.Remove(customer);
                                // 服务的波次增加
                                if (currentCustomerList.Count <= 0) ++currentWaveIndex;

                                NewCustomerEnter();
                            });
                            
                            //每满足一位顾客，（当前屏内棋子堆叠超过4行时）阵型向下压2行
                            if (HasNewCustomer() && kitchenTileItemPanelPool.Count > 0 &&
                                kitchenTileItemPanelPool[0].transform.localPosition.y + tileMovePanel.localPosition.y >
                                0)
                                MoveTileItemPanelDown(1);
                        }

                        icon.DOKill();
                        icon.gameObject.SetActive(false);
                        Destroy(icon.gameObject);
                        // if (!kitchenTileItemPool.Contains(item))
                        //     kitchenTileItemPool.Add(item);
                        customer.ResfershNeedText(num, needIndex, i);
                        customer.SetCustomerState(HarvestKitchenCustomer.CustomerState.happy);
                    };

                    GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                    {
                        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>("KitchenMergeEffect", "TileItemDestroyEffectPool", 1.3f, targetPos, transform.rotation, transform, obj =>
                        {
                            GameObject effect = obj.Target as GameObject;
                            //effect.transform.position = targetPos;
                            var m_PropMergeEffect = effect.GetComponent<Merge.PropMergeEffect>();
                            m_PropMergeEffect.Show();
                        });
                    });
                });
                
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 局内获取金币动画
    /// </summary>
    /// <param name="pos">金币生成的初始位置</param>
    /// <returns></returns>
    IEnumerator ShowGetCoinAnim(Vector3 pos)
    {
        yield return new WaitForSeconds(0.2f);

        int subCoinCount = 3;
        List<RotateCoin> rotateCoins = new List<RotateCoin>();
        if (rotateCoins.Count < subCoinCount)
        {
            int delta = subCoinCount - rotateCoins.Count;
            for (int i = 0; i < delta; i++)
            {
                GameManager.ObjectPool.Spawn<RotateCoinObject>("RotateCoin", "RotateCoin", pos, Quaternion.identity, coinBar.transform.parent, (obj) =>
                {
                    if (obj != null && obj.Target != null)
                    {
                        GameObject coinObject = (GameObject)obj.Target;
                        RotateCoin rotateCoin = coinObject.GetComponent<RotateCoin>();
                        if (rotateCoin != null)
                        {
                            rotateCoin.OnHide();
                            rotateCoins.Add(rotateCoin);
                        }
                        else
                        {
                            subCoinCount--;
                        }
                    }
                    else
                    {
                        subCoinCount--;
                    }
                });
            }

            while (rotateCoins.Count < subCoinCount)
            {
                yield return null;
            }

            coinFadeText.text = "+" + coinNum.ToString();
            Transform fadeTextTrans = coinFadeText.transform;
            coinFadeText.DOKill();
            fadeTextTrans.DOKill();
            fadeTextTrans.localScale = Vector3.zero;
            fadeTextTrans.transform.position = pos;
            coinFadeText.color = new Color(coinFadeText.color.r, coinFadeText.color.g, coinFadeText.color.b, 0);
            
            coinFadeText.DOFade(1, 0.8f);
            fadeTextTrans.DOScale(1f, 0.34f).onComplete = () =>
            {
                fadeTextTrans.DOBlendableLocalMoveBy(new Vector3(0, 250, 0), 2.5f).SetEase(Ease.Linear);
            };
            
            for (int i = 0; i < rotateCoins.Count; i++)
            {
                RotateCoin rotateCoin = rotateCoins[i];
                Transform rotateCoinTrans = rotateCoins[i].transform;
                
                var graphic = rotateCoin.skeletonGraphic;
                rotateCoin.OnShow();
                graphic.transform.DOScale(0.35f, 0.4f);

                rotateCoinTrans.DOLocalMove(coinBar.transform.localPosition, 0.6f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    rotateCoin.OnHide();
                    GameManager.ObjectPool.Unspawn<RotateCoinObject>("RotateCoin", rotateCoin.gameObject);
                    coinBar.OnCoinFlyHit();
                    
                    UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                });
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.3f);
            RewardManager.Instance.SaveRewardData(TotalItemData.Coin, coinNum, true);
            
            coinFadeText.DOFade(0, 0.5f);
        }
    }

    /// <summary>
    /// 新顾客进入
    /// </summary>
    /// <param name="action">新顾客进入后的回调</param>
    private void NewCustomerEnter(Action action = null)
    {
        // 有顾客离去
        if (currentCustomerList.Count < 2)
        {
            // 是检测否还有新顾客
            if (HasNewCustomer())
            {
                Log.Info($"Kitchen:  即将生成的顾客ID {currentCustomerIndex}");
                // 判断当前波次显示的人数
                int waveNum = panelData.GetShowCustomerNum(currentWaveIndex);
                if (waveNum == 1 && currentCustomerList.Count <= 0)
                {
                    // 将新顾客移至单人位
                    var next = GetNewCustomer();
                    next.gameObject.SetActive(true);
                    currentCustomerIndex++;
                    next.transform.SetAsFirstSibling();
                    next.transform.DOLocalMove(oneCustomerTarget.localPosition, 0.5f).onComplete +=
                        () =>
                        {
                            next.ShowNeedInfo(action, () =>
                            {
                                if (!isStartTileMove)
                                {
                                    next.GamePause();
                                }
                            });
                        };
                    currentCustomerList.Add(next);
                }
                else
                {
                    int index = 0;
                    // 有新顾客，
                    if (currentCustomerList.Count > 0)
                    {
                        Log.Info($"Kitchen:{currentCustomerList[0].transform.name}从二号位移到一号位");
                        // 2号位存在顾客，将老顾客移至1号位
                        currentCustomerList[0].transform.DOLocalMove(customerTarget[index].localPosition, 0.5f);
                        index++;
                    }

                    // 顾客数量不足
                    while (index < waveNum)
                    {
                        var next = GetNewCustomer();
                        // 注：此处next的值为空不会在控制台报错，但是会导致顾客的移动出现问题
                        if (next == null) break;
                        next.gameObject.SetActive(true);
                        currentCustomerIndex++;
                        next.transform.SetAsFirstSibling();
                        Log.Info($"Kitchen:{next.transform.name}顾客进入动画{index}");
                        next.transform.DOLocalMove(customerTarget[index].localPosition, 0.5f).onComplete +=
                            () =>
                            {
                                next.ShowNeedInfo(action, () =>
                                {
                                    if (!isStartTileMove)
                                    {
                                        next.GamePause();
                                    }
                                });
                            };
                        currentCustomerList.Add(next);
                        index++;
                    }
                }
            }
            else if (currentCustomerList.Count > 0)
            {
                Log.Info($"Kitchen:没有新顾客");
                if (!currentCustomerList[0].CheckCollectComplete() && currentCustomerList[0].transform.localPosition.x > oneCustomerTarget.localPosition.x)
                {
                    Log.Info($"Kitchen:{currentCustomerList[0].transform.name}将移至单人位");
                    // 顾客在二号位，将顾客移至单人位置
                    currentCustomerList[0].transform.DOLocalMove(oneCustomerTarget.localPosition, 0.5f);
                }
            }
            
            if (currentCustomerList.Count <= 0)
            {
                Log.Info($"Kitchen：没有新顾客，游戏结束，总共获得{currentPraiseNum}个赞");
                // 游戏胜利
                GameWin();
            }
        }
    }

    /// <summary>
    /// 判断是否有新顾客
    /// </summary>
    /// <returns>是否存在</returns>
    private bool HasNewCustomer()
    {
        return panelData.HasNewCustomer(currentCustomerIndex);
    }

    /// <summary>
    /// 获取新的顾客
    /// </summary>
    /// <param name="isInit">是否是在初始化</param>
    /// <returns>新顾客</returns>
    private HarvestKitchenCustomer GetNewCustomer()
    {
        KitchenCustomerData data = panelData.GetCustomerDataByIndex(currentCustomerIndex);
        if (data == null) return null;

        if (customerIdList.Count <= 0)
            customerIdList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        int id = 6;//要是没有出过教程，第一个出老头，腿短
        if (!HarvestKitchenManager.Instance.IsShowGameGuide)
        {
            id = customerIdList[Random.Range(0, customerIdList.Count)];
        }
        customerIdList.Remove(id);
        
        HarvestKitchenCustomer customer = null;
        for (int i = 0; i < KitchenCustomerPool.Count; i++)
        {
            if (KitchenCustomerPool[i].CustomerId == id)
            {
                customer = KitchenCustomerPool[i];
                break;
            }
        }
        
        string customerName = "Customer_" + id.ToString();
        GameObject obj = UnityUtility.InstantiateSync(customerName, Vector3.zero, Quaternion.identity, customerParent);
        customer = obj.GetComponent<HarvestKitchenCustomer>();
        customer.SetTimeoutEvent(()=>
        {
            HarvestKitchenManager.Instance.toContinueMenuType = 0;
            GamePause();
            // 播放玩家需要的食物变为X，然后弹出失败框
            customer.ShowFail(ShowGameContinue);
        });


        customer.Init(id, data, customerLeaveTarget.localPosition, () =>
        {
            panelData.ChangeTileProbability(data.needFoodId, false);
            // 回收逻辑
            customer.gameObject.SetActive(false);
            customer.transform.localPosition = customerInitTarget.localPosition;
            if (!KitchenCustomerPool.Contains(customer))
                KitchenCustomerPool.Add(customer);
        });
        panelData.ChangeTileProbability(data.needFoodId, true);
            
        customer.transform.localPosition = customerInitTarget.localPosition;
        
#if UNITY_EDITOR
        customer.transform.name = currentCustomerIndex.ToString();
#endif
        
        return customer;
    }

    /// <summary>
    /// 初始化顾客对象池
    /// </summary>
    public void InitKitchenCustomerPool()
    {
        for (int i = 0; i < 4; i++)
        {
            string customerName = "Customer_1";
            GameObject obj = UnityUtility.InstantiateSync(customerName, Vector3.zero, Quaternion.identity, customerParent);
            var customer = obj.GetComponent<HarvestKitchenCustomer>();
            customer.SetTimeoutEvent(()=>
            {
                HarvestKitchenManager.Instance.toContinueMenuType = 0;
                GamePause();
                // 播放玩家需要的食物变为X，然后弹出失败框
                customer.ShowFail(ShowGameContinue);
            });
            
            KitchenCustomerPool.Add(customer);
        }
    }

    /// <summary>
    /// 游戏胜利
    /// </summary>
    private void GameWin()
    {
        canfocusPause = false;
        
        // 停止棋盘移动
        isStartTileMove = false;
        
        // 记录胜利数据
        HarvestKitchenManager.Instance.ActivityLevelWin(currentPraiseNum);
        
        // 弹出胜利提示
        GameManager.UI.ShowUIForm("HarvestKitchenGameWinMenu");
    }
    
    /// <summary>
    /// 弹出接关提示框
    /// </summary>
    private void ShowGameContinue()
    {
        GameManager.UI.ShowUIForm("HarvestKitchenGameContinueMenu", form =>
        {
            var uiform = form as HarvestKitchenGameContinueMenu;
            uiform.SetLisks(currentPraiseNum, MaxPraiseNum);
        });
    }

    /// <summary>
    /// 接关的广播处理
    /// </summary>
    private void CommonEventCallBack(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        if (ne.Type == CommonEventType.KitchenLoseContinue)
        {
            // 给当前顾客恢复情绪值
            for (int i = 0; i < currentCustomerList.Count; i++)
            {
                currentCustomerList[i].LevelContinue();
            }
            // 消除选择栏中的所有棋子
            chosenFail.gameObject.SetActive(false);
            chosenBar.LevelContinue((tileItem) =>
            {
                tileItem.gameObject.SetActive(false);
                if (!kitchenTileItemPool.Contains(tileItem))
                    kitchenTileItemPool.Add(tileItem);
            });
            // 将棋盘下移至只显示3行
            topFail.gameObject.SetActive(false);
            Vector3 nowPos = tileMovePanel.localPosition;
            if (kitchenTileItemPanelPool.Count > 0 && kitchenTileItemPanelPool[0].transform.localPosition.y + tileMovePanel.localPosition.y > -140f)
            {
                nowPos.y += -140 - (kitchenTileItemPanelPool[0].transform.localPosition.y + tileMovePanel.localPosition.y);
                tileMovePanel.DOLocalMove(nowPos, 0.5f).onComplete += () =>
                {
                    // 继续游戏
                    GameContinue();
                    // 将刷新棋子的等待时间置0
                    refreshTilePanelTime = 0;
                    // 回收多余的棋子行
                    for (int i = kitchenTileItemPanelPool.Count - 1; i >= 4; i--)
                    {
                        var panel = kitchenTileItemPanelPool[i];
                        kitchenTileItemPanelPool.RemoveAt(i);
                        noUseKitchenTileItemPanelPool.Insert(0, panel);
                    }
                };
            }
            else
            {
                GameContinue();
            }
        }

        if (ne.Type == CommonEventType.KitchenContinue)
        {
            // 继续游戏
            GameContinue();
        }
    }

    public void MoveTileItemPanelDown(int downLayer)
    {
        isStartTileMove = false;
        
        Vector3 nowPos = tileMovePanel.localPosition;
        nowPos.y = tileMovePanel.localPosition.y - downLayer * 140;
        tileMovePanel.DOLocalMove(nowPos, 0.5f).onComplete += () =>
        {
            isStartTileMove = true;
            
            // 继续游戏
            //GameContinue();
            // // 将刷新棋子的等待时间置0
            // refreshTilePanelTime = 0;
            // // 回收多余的棋子行
            // for (int i = kitchenTileItemPanelPool.Count - 1; i >= 4; i--)
            // {
            //     var panel = kitchenTileItemPanelPool[i];
            //     kitchenTileItemPanelPool.RemoveAt(i);
            //     noUseKitchenTileItemPanelPool.Insert(0, panel);
            // }
        };
    }

    private Transform oldParent = null;
    /// <summary>
    /// 显示游戏教程
    /// </summary>
    /// <param name="guideIndex">教程编号</param>
    public void ShowGameGuide(int guideIndex)
    {
        GamePause();
        canCloseGuide = false;
        guide.SetActive(true);
        switch (guideIndex)
        {
            case 0:
                guideBg.SetActive(true);
                oldParent = customerParent.parent;
                customerParent.SetParent(guide.transform);
                guideObj[0].SetActive(true);
                guideObj[1].SetActive(false);
                guideObj[2].SetActive(false);
                break;
            case 1:
                guideBg.SetActive(false);
                guideObj[0].SetActive(false);
                guideObj[1].SetActive(true);
                guideObj[2].SetActive(false);
                break;
            case 2:
                guideBg.SetActive(false);
                guideObj[0].SetActive(false);
                guideObj[1].SetActive(false);
                guideObj[2].SetActive(true);
                break;
            default:
                break;
        }
        GameManager.Task.AddDelayTriggerTask(1f, () => canCloseGuide = true);
    }

    private bool canfocusPause = true;
    public void OnApplicationFocus(bool hasFocus)
    {
        Log.Info($"Kitchen: OnApplicationFocus  {canfocusPause}");
        if (!canfocusPause) return;
        if (hasFocus)
        {
            // 获得焦点
            if (!isStartTileMove)
            {
                GameContinue();
            }
        }
        else
        {
            // 失去焦点
            GamePause(false);
        }
    }
}
