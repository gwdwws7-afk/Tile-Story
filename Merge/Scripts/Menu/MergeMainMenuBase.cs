using DG.Tweening;
using GameFramework;
using GameFramework.Event;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public abstract class MergeMainMenuBase : UIForm
    {
        public MergeBoard m_MergeBoard;
        public SelectedBox m_SelectedBox;
        public DelayButton m_CloseButton;
        public DelayButton m_TipButton;
        public ChestPromptBox m_ChestPromptBox;
        public BubbleOperationBox m_BubbleOperationBox;
        public SkeletonAnimation m_Finger;
        public Transform m_EffectRoot;
        public ClockBar m_Timer;
        public MergeGuideMenu m_GuideMenu;
        public CountdownTimer m_AdsTimer;

        protected Square[,] m_SquareMap;
        protected int m_SpawnPropCount;
        private List<StoredProp> m_StorePropIds;
        public PropLogic m_SelectedChestProp;
        protected List<PropLogic> showingMergeHintProps = new List<PropLogic>();
        protected LinkedList<List<PropLogic>> needHintProps = null;

        public virtual int BoardRow => MergeManager.Instance.BoardRow;

        public virtual int BoardCol => MergeManager.Instance.BoardCol;

        public List<StoredProp> StorePropIds
        {
            get
            {
                if (m_StorePropIds == null)
                    m_StorePropIds = MergeManager.PlayerData.GetAllStorePropIds();

                return m_StorePropIds;
            }
        }

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            GameManager.Event.Subscribe(SelectedPropChangeEventArgs.EventId, OnSelectedPropChange);
            GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

            if (!MergeManager.Merge.IsInitialized)
            {
                int row = BoardRow;
                int col = BoardCol;
                m_SquareMap = new Square[row, col];
                for (int i = 0; i < m_SquareMap.GetLength(0); i++)
                {
                    for (int j = 0; j < m_SquareMap.GetLength(1); j++)
                    {
                        int index = j + i * col;
                        m_SquareMap[i, j] = m_MergeBoard.GetSquare(index);
                        m_SquareMap[i, j].Initialize(i, j);
                        m_SquareMap[i, j].FilledProp = null;
                    }
                }
                MergeManager.Merge.Initialize(m_SquareMap);
                LoadPropDistributedMap();
            }

            InitializeButton();

            m_MergeBoard.gameObject.SetActive(true);
            RefreshStorage();

            m_GuideMenu.Initialize();

            if (!GameManager.ObjectPool.HasObjectPool("PropEffectPool"))
            {
                GameManager.ObjectPool.CreateObjectPool<EffectObject>("PropEffectPool", float.PositiveInfinity, 50, float.PositiveInfinity);
                GameManager.ObjectPool.PreloadObjectPool<EffectObject>("PropEffectPool", "PropMergeEffect", transform, 3);
            }

            //记录merge进入过活动
            MergeManager.Instance.RecordEnterActivity();

            if (DateTime.Now < MergeManager.Instance.GetActivityEndTime() && GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel())
            {
                m_Timer.OnReset();
                m_Timer.CountdownOver += OnCountdownOver;
                m_Timer.StartCountdown(MergeManager.Instance.GetActivityEndTime());
            }
            else
            {
                m_Timer.SetFinishState();
            }

            DateTime readyTime = MergeManager.PlayerData.GetBubbleBreakNextAdsReadyTime();
            if (readyTime > DateTime.Now)
            {
                m_AdsTimer.OnReset();
                m_AdsTimer.CountdownOver += OnBubbleAdsCountdownOver;
                m_AdsTimer.StartCountdown(readyTime);
            }

            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Merge_Open_Activity_Interface, new Firebase.Analytics.Parameter("Num", MergeManager.PlayerData.GetMergeEnergyBoxNum()));
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(SelectedPropChangeEventArgs.EventId, OnSelectedPropChange);
            GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

            MergeManager.Merge.SelectedProp = null;
            m_SelectedBox.Hide();

            if (m_StorePropIds != null)
            {
                foreach (var storeProp in m_StorePropIds)
                {
                    if (storeProp.SavedData != null)
                    {
                        ReferencePool.Release(storeProp.SavedData);
                    }
                }
            }
            m_StorePropIds = null;

            m_Timer.OnReset();
            m_GuideMenu.OnSkipButtonClick();
            m_AdsTimer.OnReset();

            showingMergeHintProps.Clear();
            needHintProps = null;

            base.OnReset();
        }

        public override void OnRelease()
        {
            OnReset();
            m_SquareMap = null;
            MergeManager.Merge.Release();
            GameManager.ObjectPool.DestroyObjectPool("PropEffectPool");

            m_ChestPromptBox.Release();

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_Timer.OnUpdate(elapseSeconds, realElapseSeconds);
            m_AdsTimer.OnUpdate(elapseSeconds, realElapseSeconds);

            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                bool clearSelected = true;
                bool hitBubbleBox = false;
                RaycastHit2D hit = Physics2D.Raycast(MergeManager.Merge.EyeCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"));
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.name == "BubbleOperationBox")
                    {
                        hitBubbleBox = true;
                    }
                    else
                    {
                        Square hitSquare = hit.collider.GetComponent<Square>();
                        if (hitSquare != null && hitSquare.FilledProp == MergeManager.Merge.SelectedProp)
                        {
                            clearSelected = false;
                        }
                    }
                }

                HideChestRewardTipBox(clearSelected);
                if (!hitBubbleBox)
                    HideBubbleOperationBox();
            }
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
            if (userData == null)
            {
                transform.DOScale(1.03f, 0.12f).onComplete = () =>
                {
                    transform.DOScale(0.99f, 0.1f).onComplete = () =>
                    {
                        transform.DOScale(1f, 0.1f);
                    };
                };
            }

            GameManager.Sound.PlayMusic(MergeManager.Instance.BgMusicName, 1f);

            base.OnShow(showSuccessAction, userData);
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);

            GameManager.Sound.PlayMusic(GameManager.PlayerData.BGMusicName);

            base.OnHide(hideSuccessAction, userData);
        }

        public override void OnPause()
        {
            m_CloseButton.interactable = false;
            m_TipButton.interactable = false;

            base.OnPause();
        }

        public override void OnResume()
        {
            m_CloseButton.interactable = true;
            m_TipButton.interactable = true;

            base.OnResume();
        }

        public override void OnCover()
        {
            //合成棋盘元素层级高，重叠界面会导致穿帮，所以界面被遮挡时隐藏棋盘
            if (m_MergeBoard != null)
                m_MergeBoard.gameObject.SetActive(false);

            base.OnCover();
        }

        public override void OnReveal()
        {
            if (m_MergeBoard != null)
                m_MergeBoard.gameObject.SetActive(true);

            base.OnReveal();
        }

        public override bool CheckInitComplete()
        {
            return m_SpawnPropCount <= 0;
        }

        protected virtual void InitializeButton()
        {
            m_CloseButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

            m_CloseButton.interactable = true;
            m_TipButton.interactable = true;
        }

        protected virtual void LoadPropDistributedMap()
        {
            string savedPropDistributedMap = MergeManager.PlayerData.GetSavedPropDistributedMap();
            if (!string.IsNullOrEmpty(savedPropDistributedMap))
            {
                string[] savedPropsString = savedPropDistributedMap.Split('$');
                for (int i = 0; i < savedPropsString.Length; i++)
                {
                    string[] savedPropsSplits = savedPropsString[i].Split('#');
                    PropSavedData savedData = null;
                    if (savedPropsSplits.Length > 4)
                    {
                        savedData = ReferencePool.Acquire<PropSavedData>();
                        savedData.Load(savedPropsSplits[4]);
                    }

                    int propId = int.Parse(savedPropsSplits[2]);
                    if (propId > 0)
                    {
                        int row = int.Parse(savedPropsSplits[0]);
                        int col = int.Parse(savedPropsSplits[1]);
                        int attachmentId = int.Parse(savedPropsSplits[3]);

                        PropLogic prop = MergeManager.Merge.InternalGenerateProp(propId, attachmentId, m_SquareMap[row, col].transform.position, m_SquareMap[row, col], PropMovementState.Static, savedData);
                        if (prop != null)
                        {
                            m_SpawnPropCount++;
                            prop.SpawnPropComplete += p => m_SpawnPropCount--;
                        }
                    }
                }
            }
            else
            {
                IDataTable<DRPropDistributedMap> map = MergeManager.DataTable.GetDataTable<DRPropDistributedMap>(MergeManager.Instance.GetMergeDataTableName());
                if (map.Count < BoardRow)
                {
                    Log.Error("PropDistributedMap is invalid!");
                    return;
                }

                for (int i = 0; i < BoardRow; i++)
                {
                    for (int j = 0; j < BoardCol; j++)
                    {
                        if (map[i].PropsId[j] > 0)
                        {
                            PropLogic targetProp = MergeManager.Merge.InternalGenerateProp(map[i].PropsId[j], map[i].AttachmentsId[j], m_SquareMap[i, j].transform.position, m_SquareMap[i, j], PropMovementState.Static, null);
                            if (targetProp != null)
                            {
                                m_SpawnPropCount++;
                                targetProp.SpawnPropComplete += p => m_SpawnPropCount--;
                            }
                        }
                    }
                }
            }
        }

        public virtual void StoreProp(int propId, PropSavedData data = null, bool refresh = true)
        {
            MergeManager.PlayerData.AddStorePropId(propId, 1, data);
            if (StorePropIds != null)
                StorePropIds.Add(new StoredProp(propId, data));

            if(refresh)
                RefreshStorage();
        }

        protected virtual void WithdrawProp()
        {
            if (StorePropIds != null && StorePropIds.Count > 0)
            {
                Square randomSquare = MergeManager.Merge.GetRandomEmptySquare();
                if (randomSquare != null)
                {
                    int index = StorePropIds.Count - 1;
                    StoredProp storedProp = StorePropIds[index];
                    MergeManager.Merge.GenerateProp(storedProp.PropId, 0, GetStoragePropGeneratePos(), randomSquare, PropMovementState.Bouncing, storedProp.SavedData);
                    StorePropIds.RemoveAt(index);
                    MergeManager.PlayerData.RemoveLastStorePropId();
                    RefreshStorage();

                    GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
                }
                else
                {
                    ShowWeakHint("Merge.Board is full!");
                }
            }
        }

        public virtual Vector3 GetStoragePropGeneratePos()
        {
            return Vector3.one;
        }

        public virtual void RefreshStorage()
        {
        }

        public virtual void RefreshSale()
        {
        }

        #region ChestRewardTipBox

        public virtual void ShowChestRewardTipBox(List<ItemData> datas, PropLogic propLogic)
        {
            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                if (m_ChestPromptBox == null || propLogic == null || propLogic.Square == null)
                    return;

                m_SelectedChestProp = propLogic;

                Square square = propLogic.Square;
                float offsetX = 0f;
                if (datas.Count == 3)
                {
                    if (square.m_Col <= 0)
                        offsetX = 120;
                    else if (square.m_Col >= 4)
                        offsetX = -120;
                }

                m_ChestPromptBox.Init(datas);
                m_ChestPromptBox.Show(square.transform.position, offsetX);
            });
        }

        public void HideChestRewardTipBox(bool clearSelected = true)
        {
            if (clearSelected)
                m_SelectedChestProp = null;

            m_ChestPromptBox.Hide();
        }

        #endregion

        #region BubbleOperationBox

        public virtual void ShowBubbleOperationBox(int propId, PropLogic propLogic)
        {
            bool isShowAdsButton = CanShowBubbleAdsButton(propId);
            Square square = propLogic.Square;
            float offsetX = 0f;
            if (isShowAdsButton)
            {
                if (square.m_Col <= 0)
                    offsetX = 100;
                else if (square.m_Col >= 4)
                    offsetX = -100;
            }

            m_BubbleOperationBox.Initialize(propId, propLogic, isShowAdsButton);
            m_BubbleOperationBox.Show(square.transform.position, offsetX);
        }

        public void HideBubbleOperationBox()
        {
            m_BubbleOperationBox.Hide();
        }

        public virtual bool CanShowBubbleAdsButton(int propId)
        {
            if (MergeManager.PlayerData.GetBubbleBreakNextAdsReadyTime() > DateTime.Now)
                return false;

            if (!GameManager.Ads.CheckRewardedAdIsLoaded() || MergeManager.PlayerData.GetShowAdsGetBubbleTime() >= 5)
                return false;

            IDataTable<DRMergeGenerateBubble> dataTable = MergeManager.DataTable.GetDataTable<DRMergeGenerateBubble>(MergeManager.Instance.GetMergeDataTableName());
            var allData = dataTable.GetAllDataRows();
            foreach (var data in allData)
            {
                if (data.GenerateBubble == propId)
                {
                    return data.CanBreakByAds;
                }
            }

            return false;
        }

        #endregion

        #region FingerAnim

        public virtual void ShowClickSupplyFingerAnim()
        {
        }

        public virtual void ShowClickStorageFingerAnim()
        {
        }

        public virtual void ShowFingerAnim(Vector3 pos)
        {
            if (!m_MergeBoard.gameObject.activeSelf || m_Finger == null) 
                return;

            m_Finger.transform.position = pos;
            m_Finger.gameObject.SetActive(true);
            m_Finger.AnimationState.SetAnimation(0, "01", false);

            GameManager.Task.AddDelayTriggerTask(0.87f, HideFingerAnim);
        }

        public virtual void HideFingerAnim()
        {
            if (m_Finger != null) 
                m_Finger.gameObject.SetActive(false);
        }

        #endregion

        #region PropHint

        public virtual void ShowPropHint()
        {
            for (int i = 0; i < showingMergeHintProps.Count; i++)
            {
                if (showingMergeHintProps[i].PropId == 0 || showingMergeHintProps[i].Square == null)
                {
                    showingMergeHintProps.Clear();
                    return;
                }
            }

            if (showingMergeHintProps.Count == 0)
            {
                if (needHintProps == null || needHintProps.Count == 0)
                {
                    //棋盘上是否有可merge道具
                    needHintProps = MergeManager.Merge.GetCanMergeProps();

                    if (needHintProps.Count == 0)
                    {
                        //棋盘上是否有空格
                        if (MergeManager.Merge.GetRandomEmptySquare() != null)
                        {
                            //是否有体力
                            if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                            {
                                //提示点击体力按钮
                                ShowClickSupplyFingerAnim();
                                return;
                            }
                            else
                            {
                                //是否有存储的道具
                                if (MergeManager.PlayerData.GetAllStorePropIds().Count > 0)
                                {
                                    //提示点击存储按钮
                                    ShowClickStorageFingerAnim();
                                    return;
                                }
                                else
                                {
                                    //棋盘上是否有特殊道具
                                    needHintProps = MergeManager.Merge.GetCanClickSpecialProps();
                                }
                            }
                        }
                        else
                        {
                            //棋盘上是否有特殊道具
                            needHintProps = MergeManager.Merge.GetCanClickSpecialProps();
                        }
                    }
                }

                while (needHintProps.Count > 0)
                {
                    List<PropLogic> list = needHintProps.First.Value;
                    needHintProps.RemoveFirst();

                    showingMergeHintProps.Clear();
                    bool haveImmovableProp = false;

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null && list[i].Square != null)
                        {
                            //两个不可移动的元素不触发合成提示
                            if (list[i].IsImmovable)
                            {
                                if (!haveImmovableProp)
                                    haveImmovableProp = true;
                                else
                                    continue;
                            }

                            showingMergeHintProps.Add(list[i]);
                        }

                        if (!haveImmovableProp && list.Count == 1 && showingMergeHintProps.Count == 1)
                            break;
                    }

                    if (list.Count >= 2 && showingMergeHintProps.Count >= 2)
                        break;

                    if (!haveImmovableProp && list.Count == 1 && showingMergeHintProps.Count == 1)
                        break;

                    if (haveImmovableProp && showingMergeHintProps.Count == 1)
                        showingMergeHintProps.Clear();
                }
            }

            if (showingMergeHintProps.Count == 1)
            {
                showingMergeHintProps[0].ShowMergeHintAnim(Vector3.zero);
            }
            else if (showingMergeHintProps.Count >= 2)
            {
                int index1 = 0;
                int index2 = 1;

                int ImmovableIndex = -1;
                for (int i = 0; i < showingMergeHintProps.Count; i++)
                {
                    if (showingMergeHintProps[i].IsImmovable)
                    {
                        ImmovableIndex = i;
                        break;
                    }
                }

                if (ImmovableIndex != -1)
                {
                    index1 = ImmovableIndex;
                    index2 = index2 == index1 ? 0 : index2;
                }

                Vector3 direction = (showingMergeHintProps[index1].Square.transform.position - showingMergeHintProps[index2].Square.transform.position).normalized;
                showingMergeHintProps[index1].ShowMergeHintAnim(-direction);
                showingMergeHintProps[index2].ShowMergeHintAnim(direction);
            }
            else if(DateTime.Now < MergeManager.Instance.GetActivityEndTime())
            {
                //提示点击体力按钮
                ShowClickSupplyFingerAnim();
            }
            showingMergeHintProps.Clear();
        }

        public void ClearPropHint()
        {
            showingMergeHintProps.Clear();
            needHintProps = null;
        }

        #endregion

        public void ShowWeakHint(string content)
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeWeakHintMenu"), form =>
            {
                MergeWeakHintMenu weakHintMenu = form.GetComponent<MergeWeakHintMenu>();
                weakHintMenu.SetHintText(content, Camera.main.ViewportToScreenPoint(Vector3.zero));
                weakHintMenu.OnShow();
            });
        }

        public void ShowShiningFlyEffect(Vector3 startPos, Vector3 endPos, Action callback, bool playAudio)
        {
            ShowShiningFlyEffect(startPos, endPos, 0.5f, callback, playAudio);
        }

        public void ShowShiningFlyEffect(Vector3 startPos, Vector3 endPos, float duration, Action callback, bool playAudio)
        {
            UnityUtility.InstantiateAsync("ShinningFlyEffect", startPos, Quaternion.identity, m_EffectRoot,
                (effect) =>
                {
                    effect.transform.DOMove(endPos, duration).onComplete = () =>
                    {
                        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
                        {
                            UnityUtility.UnloadInstance(effect);
                        });
                        callback?.Invoke();
                    };

                    if (playAudio)
                        GameManager.Sound.PlayAudio(SoundType.SFX_Merge_Unlock_Process.ToString());
                });
        }

        protected virtual void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Process.EndProcess(ProcessType.AutoShowMergeProcess);
        }

        protected virtual void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }

        protected virtual void OnSelectedPropChange(object sender, GameEventArgs e)
        {
            if (MergeManager.Merge.SelectedProp == null || MergeManager.Merge.SelectedProp.MergeRouteId != 3)
                HideChestRewardTipBox();

            HideBubbleOperationBox();
            HideFingerAnim();
        }

        protected virtual void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_Timer.SetFinishState();
        }

        protected virtual void OnBubbleAdsCountdownOver(object sender, CountdownOverEventArgs e)
        {
            GameManager.Event.Fire(this, RefreshMergeEnergyBoxEventArgs.Create());
        }

        protected virtual void OnRewardAdEarned(object sender, GameEventArgs e)
        {
            RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
            if (ne.UserData.ToString() != "MergeBubble")
            {
                return;
            }

            bool isUserEarnedReward = true;
#if UNITY_ANDROID && !UNITY_EDITOR && !AmazonStore
            isUserEarnedReward = ne.EarnedReward;
#endif
            if (isUserEarnedReward)
            {
                MergeManager.PlayerData.SetBubbleBreakNextAdsReadyTime(DateTime.Now.AddMinutes(5));
                m_AdsTimer.OnReset();
                m_AdsTimer.CountdownOver += OnBubbleAdsCountdownOver;
                m_AdsTimer.StartCountdown(DateTime.Now.AddMinutes(5));

                var m_SelectedProp = MergeManager.Merge.SelectedProp;
                if (m_SelectedProp == null)
                    return;

                if (m_SelectedProp.Prop != null)
                {
                    m_SelectedProp.Prop.BodyScale = 1f;
                    m_SelectedProp.Prop.ClearAnim();
                    m_SelectedProp.Prop.ShowPunchAnim();
                    m_SelectedProp.Prop.ShowPropGenerateEffect();

                    GameManager.Sound.PlayAudio(SoundType.SFX_Pop_Element_Break.ToString());
                }
                m_SelectedProp.ReleaseAttachment();
                MergeManager.Merge.SavePropDistributedMap();

                MergeManager.PlayerData.AddShowAdsGetBubbleTime();

                GameManager.Firebase.RecordMessageByEvent("Merge_Watch_Pop_Ads");
            }
            else
            {
                var m_SelectedProp = MergeManager.Merge.SelectedProp;
                if (m_SelectedProp != null && m_SelectedProp.Prop != null && m_SelectedProp.AttachmentId == 1)  
                {
                    var bubbleLogic = m_SelectedProp.AttachmentLogic as BubbleLogic;
                    if (bubbleLogic != null)
                    {
                        bubbleLogic.TimePause = false;
                    }
                }
            }
        }
    }
}
