using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class MergeMainMenu_DigTreasure : MergeMainMenuBase, IItemFlyReceiver
    {
        public NewFossilPropMergedBoard m_NewFossilPropMergedBoard;
        public GameObject m_PickaxeBarIcon;
        public TextMeshProUGUI m_PickaxeBarText;
        public SimpleSlider m_ProgressBarSlider;
        public TextMeshProUGUI m_RewardStageText;
        public DelayButton m_RewardStageButton;
        public DelayButton m_RewardStageChestButton;
        public DelayButton m_StorageButton;
        public DelayButton m_SaleButton;
        public DelayButton m_InfoButton;
        public Button m_PickaxeBarButton;
        public Image m_StoragePropImg, m_SignImg;
        public TextMeshProUGUI m_StorageNumText;
        public TextMeshProUGUI m_DeepText;
        public TextMeshProUGUI m_SaleNumText;
        public ClockBar m_SaleButtonTimer;
        public LockPromptBox m_LockPromptBox;
        public GameObject m_RewardStageChest;
        public ParticleSystem m_RewardStageHitEffect;
        public ParticleSystem m_SignUpgradeHitEffect;
        public Transform m_FlyGemHitPos;
        public GameObject m_Mask;
        public GameObject m_PickaxePrefab;
        public GameObject m_DiggableEffectPrefab;
        public SkeletonGraphic m_SmokeEffect;
        public SkeletonGraphic m_StoneEffect;
        public MergeDigCelebrateMenu m_MergeDigCelebrateMenu;

        private AsyncOperationHandle signSpriteHandle;
        private List<Pickaxe> m_PickaxeList = new List<Pickaxe>();
        private List<GameObject> m_DiggableEffectList = new List<GameObject>();
        private int m_CurDepth = 800;

        private List<AsyncOperationHandle> storageSpriteHandleList = new List<AsyncOperationHandle>();

        public bool IsUseSupply
        {
            get;
            set;
        }

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            GameManager.Event.Subscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Subscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);
            GameManager.Event.Subscribe(MaxStageUpgradeEventArgs.EventId, OnMaxStageUpgrade);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            IsUseSupply = false;
            isBoardMoveStoreProp = false;

            RefreshSign();
            RefreshPickaxeBar();
            RefreshProgressBar(false);
            RefreshDepth();
            RefreshSale();

            if (DateTime.Now > MergeManager.Instance.StartTime && DateTime.Now < MergeManager.Instance.EndTime)
            {
                m_SaleButtonTimer.OnReset();
                m_SaleButtonTimer.CountdownOver += (o, e) =>
                {
                    m_SaleButtonTimer.SetFinishState();
                };
                m_SaleButtonTimer.StartCountdown(MergeManager.Instance.EndTime);
            }
            else
            {
                m_SaleButtonTimer.SetFinishState();
            }
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Unsubscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);
            GameManager.Event.Unsubscribe(MaxStageUpgradeEventArgs.EventId, OnMaxStageUpgrade);

            RewardManager.Instance.UnregisterItemFlyReceiver(this);

            for (int i = 0; i < storageSpriteHandleList.Count; i++)
            {
                UnityUtility.UnloadAssetAsync(storageSpriteHandleList[i]);
            }
            storageSpriteHandleList.Clear();
            UnityUtility.UnloadAssetAsync(signSpriteHandle);

            m_SaleButtonTimer.OnReset();

            m_SquareMap = null;
            MergeManager.Merge.Release();

            base.OnReset();
        }

        public override void OnRelease()
        {
            foreach (Pickaxe pickaxe in m_PickaxeList)
            {
                pickaxe.Release();
                Destroy(pickaxe.gameObject);
            }
            m_PickaxeList.Clear();

            foreach (var effect in m_DiggableEffectList)
            {
                Destroy(effect);
            }
            m_DiggableEffectList.Clear();

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_SaleButtonTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void OnEnable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 11;

            GameManager.Ads.HideBanner("Merge");
        }

        private void OnDisable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 8;

            GameManager.Ads.ShowBanner("Merge");
        }

        protected override void InitializeButton()
        {
            base.InitializeButton();

            m_RewardStageButton.OnInit(OnRewardStageButtonClick);
            m_RewardStageChestButton.OnInit(OnRewardStageButtonClick);
            m_StorageButton.OnInit(OnStorageButtonClick);
            m_SaleButton.OnInit(OnSaleButtonClick);
            m_InfoButton.OnInit(OnInfoButtonClick);
            m_PickaxeBarButton.onClick.RemoveAllListeners();
            m_PickaxeBarButton.onClick.AddListener(OnPickaxeBarButtonClick);

            m_RewardStageButton.interactable = true;
            m_RewardStageChestButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_InfoButton.interactable = true;
        }

        public override void OnPause()
        {
            m_RewardStageButton.interactable = false;
            m_RewardStageChestButton.interactable = false;
            m_StorageButton.interactable = false;
            m_SaleButton.interactable = false;
            m_InfoButton.interactable = false;
            m_PickaxeBarButton.interactable = false;

            base.OnPause();
        }

        public override void OnResume()
        {
            m_RewardStageButton.interactable = true;
            m_RewardStageChestButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_InfoButton.interactable = true;
            m_PickaxeBarButton.interactable = true;

            base.OnResume();
        }

        public int GetTopClodLayer()
        {
            for (int i = 0; i < m_SquareMap.GetLength(0); i++)
            {
                for (int j = 0; j < m_SquareMap.GetLength(1); j++)
                {
                    var data = m_SquareMap[i, j];
                    if (data != null && data.FilledProp != null && data.FilledProp.PropId / 10000 == 5)
                    {
                        return BoardRow - i;
                    }
                }
            }

            return 0;
        }

        public void MoveMergeBoard(int moveLayer, float delayTime = 0f)
        {
            StartCoroutine(MoveMergeBoardCor(moveLayer, delayTime));
        }

        private bool isBoardMoveStoreProp = false;

        public IEnumerator MoveMergeBoardCor(int moveLayer, float delayTime)
        {
            if (!m_Mask.activeSelf)
            {
                m_Mask.SetActive(true);
                GameManager.Task.AddDelayTriggerTask(0.1f + moveLayer * 0.5f + delayTime, () =>
                 {
                     m_Mask.SetActive(false);
                 });
            }

            if (delayTime > 0)
            {
                yield return new WaitForSeconds(delayTime);
            }

            MergeManager.Merge.OnInputUp();
            MergeManager.Merge.SelectedProp = null;
            m_SelectedBox.Hide();
            HideLockPromptBox();
            HideBubbleOperationBox();
            HideChestRewardTipBox();
            HideFingerAnim();

            MergeBoard_DigTreasure mergeBoard_DigTreasure = m_MergeBoard as MergeBoard_DigTreasure;
            int curDepth = MergeManager.PlayerData.GetDigTreasureCurDepth();

            for (int i = 0; i < moveLayer; i++)
            {
                float time = 0.1f + i * 0.5f;
                for (int j = 0; j < BoardCol; j++)
                {
                    var square = m_SquareMap[i, j];
                    if (square != null && square.FilledProp != null) 
                    {
                        PropLogic prop = square.FilledProp;
                        int propId = prop.PropId;
                        bool showShine = false;

                        if (square.FilledProp.AttachmentId != 1) 
                        {
                            PropSavedData savedData = prop.Save();
                            StoreProp(propId, savedData);
                            showShine = true;
                            isBoardMoveStoreProp = true;
                        }

                        MergeManager.Merge.ClearProp(prop);

                        GameManager.Task.AddDelayTriggerTask(time, () =>
                        {
                            Vector3 startPos = prop.Prop.transform.position;
                            prop.Release(false);

                            if (showShine)
                            {
                                ShowShiningFlyEffect(startPos, m_StorageButton.transform.position, () =>
                                {
                                    RefreshStorage();
                                }, false);
                            }
                        });
                    }
                }
            }

            mergeBoard_DigTreasure.MoveMergeBoard(moveLayer);

            int row = BoardRow;
            int col = BoardCol;
            var newSquareMap = new Square[row, col];
            for (int i = 0; i < newSquareMap.GetLength(0); i++)
            {
                for (int j = 0; j < newSquareMap.GetLength(1); j++)
                {
                    int index = j + i * col;
                    newSquareMap[i, j] = m_MergeBoard.GetSquare(index);
                    newSquareMap[i, j].Initialize(i, j);

                    if (i + moveLayer < row)
                    {
                        newSquareMap[i, j].FilledProp = m_SquareMap[i + moveLayer, j].FilledProp;
                    }
                    else
                    {
                        newSquareMap[i, j].FilledProp = null;
                    }
                }
            }

            m_SquareMap = newSquareMap;
            MergeManager.Merge.RefreshSquareMap(newSquareMap);

            int spawnPropCount = 0;
            IDataTable<DRPropDistributedMap> map = MergeManager.DataTable.GetDataTable<DRPropDistributedMap>(MergeManager.Instance.GetMergeDataTableName());
            int deepestLayer = map.MaxIdDataRow.Id;
            for (int i = 0; i < moveLayer; i++)
            {
                for (int j = 0; j < BoardCol; j++)
                {
                    int layer = row + curDepth + i;
                    if (layer > deepestLayer)
                    {
                        int cycleMin = 86;
                        int cycleMax = 100;
                        layer = cycleMin + (layer - deepestLayer - 1) % (cycleMax - cycleMin + 1);
                    }
                    var data = map[layer];
                    if (data.PropsId[j] > 0)
                    {
                        Square square = m_SquareMap[row - moveLayer + i, j];
                        PropLogic targetProp = MergeManager.Merge.InternalGenerateProp(data.PropsId[j], data.AttachmentsId[j], square.transform.position, square, PropMovementState.Static, null);
                        if (targetProp != null)
                        {
                            spawnPropCount++;
                            targetProp.SpawnPropComplete += p =>
                            {
                                spawnPropCount--;
                                p.transform.position = square.transform.position;
                            };
                        }
                    }
                }
            }

            MergeManager.Merge.SavePropDistributedMap(true);

            yield return new WaitUntil(() => spawnPropCount <= 0);

            int moveY = mergeBoard_DigTreasure.MovedLayer * mergeBoard_DigTreasure.SquareHeight;
            mergeBoard_DigTreasure.m_Body.DOLocalMove(new Vector3(0, moveY, 0), moveLayer * 0.5f).SetEase(Ease.Linear);
            mergeBoard_DigTreasure.m_ShakeBody.DOShakePosition(moveLayer * 0.5f, new Vector3(-4, 0, 0), 6, 0, false, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.Linear);

            int depth = (MergeManager.PlayerData.GetDigTreasureCurDepth() + BoardRow) * 100;
            var tween = DOTween.To(() => m_CurDepth, (t) => m_CurDepth = t, depth, moveLayer * 0.5f).SetEase(Ease.Linear);
            tween.onUpdate = () =>
            {
                m_DeepText.text = m_CurDepth.ToString() + "M";
            };
            tween.onComplete = () =>
            {
                RefreshDepth();

                MergeManager.Merge.StartShowMergeHint = true;

                int depth = (MergeManager.PlayerData.GetDigTreasureCurDepth() + BoardRow) * 100;
                int stageDepth = MergeManager.PlayerData.GetDigTreasureStageDepth();
                if (depth >= 1000 && depth >= stageDepth + 1000) 
                {
                    if (isBoardMoveStoreProp)
                    {
                        m_MergeDigCelebrateMenu.m_HideAction = () =>
                        {
                            if (m_GuideMenu != null)
                                m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DigDialog7);
                            isBoardMoveStoreProp = false;
                        };
                    }
                    m_MergeDigCelebrateMenu.Show(depth);
                }
                else
                {
                    isBoardMoveStoreProp = false;

                    if (m_GuideMenu != null)
                        m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DigDialog7);
                }
            };

            ShowMoveMergeBoardEffect(moveLayer);
        }

        public void ShowMoveMergeBoardEffect(int moveLayer)
        {
            m_SmokeEffect.AnimationState.SetAnimation(0, "idle", true);
            m_StoneEffect.AnimationState.SetAnimation(0, "breakStone", true);
            m_SmokeEffect.DOKill();
            m_StoneEffect.DOKill();
            m_SmokeEffect.color = Color.white;
            m_StoneEffect.color = Color.white;

            m_SmokeEffect.gameObject.SetActive(true);
            m_StoneEffect.gameObject.SetActive(true);

            GameManager.Task.AddDelayTriggerTask(moveLayer * 0.5f, () =>
            {
                m_SmokeEffect.DOFade(0, 0.45f).onComplete = () =>
                {
                    m_SmokeEffect.gameObject.SetActive(false);
                };
                m_StoneEffect.DOFade(0, 0.45f).onComplete = () =>
                {
                    m_StoneEffect.gameObject.SetActive(false);
                };
            });
        }

        public void RefreshSign()
        {
            //string spriteName = "Fossil_1_" + (MergeManager.PlayerData.GetCurrentMaxMergeStage() + 1).ToString();

            //UnityUtility.UnloadAssetAsync(signSpriteHandle);
            //signSpriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetSpriteKey(spriteName, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            //{
            //    m_SignImg.sprite = sp as Sprite;
            //    m_SignImg.SetNativeSize();
            //});
        }

        public void RefreshPickaxeBar()
        {
            int boxNum = MergeManager.PlayerData.GetMergeEnergyBoxNum();
            m_PickaxeBarText.text = boxNum.ToString();
        }

        public void RefreshProgressBar(bool showAnim, Action callback = null)
        {
            IDataTable<DRMergeStageReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeStageReward>(MergeManager.Instance.GetMergeDataTableName());

            int stage = MergeManager.PlayerData.GetDigTreasureRewardStage();
            m_RewardStageText.text = Mathf.Min(dataTable.Count, stage).ToString();

            var data = dataTable.GetDataRow(stage);
            if (data != null)
            {
                int curProgress = MergeManager.PlayerData.GetDigTreasureRewardProgress();
                m_ProgressBarSlider.TotalNum = data.TargetNum;

                bool canGetReward = curProgress >= data.TargetNum;
                if (canGetReward)
                {
                    MergeManager.PlayerData.SetDigTreasureRewardProgress(curProgress - data.TargetNum);
                    MergeManager.PlayerData.SetDigTreasureRewardStage(stage + 1);

                    if (!m_Mask.activeSelf)
                    {
                        m_Mask.SetActive(true);
                        GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                        {
                            m_Mask.SetActive(false);
                        });
                    }

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Task_Unlock, new Firebase.Analytics.Parameter("Stage", stage));
                }

                if (!m_RewardStageChest.activeSelf)
                {
                    m_RewardStageChest.transform.localScale = Vector3.zero;
                    m_RewardStageChest.SetActive(true);
                    m_RewardStageChest.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                    {
                        m_RewardStageChest.transform.DOScale(1f, 0.2f);
                    };
                }

                void Callback()
                {
                    if (canGetReward)
                    {
                        for (int i = 0; i < data.RewardPropIds.Count; ++i)
                        {
                            RewardManager.Instance.AddNeedGetReward(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]);
                        }

                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.ClimbBeanstalkChestRewardPanel, true, () =>
                        {
                            m_ProgressBarSlider.Value = 0;
                            RefreshProgressBar(true, callback);
                        }, () =>
                        {
                        }, () =>
                        {
                            GameManager.UI.HideUIForm("GlobalMaskPanel");

                            ClimbBeanstalkChestRewardPanel panel = RewardManager.Instance.RewardPanel as ClimbBeanstalkChestRewardPanel;
                            panel.SetChestTypeAndPosition("03", m_RewardStageChest.transform.position, 0.4f, 0.5f);
                            panel.SetOnShowCallback(() =>
                            {
                                m_RewardStageChest.SetActive(false);
                            });
                        });

                        GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(0, null));
                        GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(0, null));
                    }
                    else
                    {
                        m_ProgressBarSlider.CurrentNum = MergeManager.PlayerData.GetDigTreasureRewardProgress();

                        callback?.Invoke();
                    }
                }

                if (showAnim && curProgress > 0) 
                {
                    m_ProgressBarSlider.DOValue(curProgress / (float)data.TargetNum, 0.6f);

                    GameManager.Task.AddDelayTriggerTask(0.6f, Callback);

                    GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Progress.ToString());
                }
                else
                {
                    m_ProgressBarSlider.CurrentNum = curProgress;

                    Callback();
                }
            }
            else
            {
                if (showAnim)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeCompleteMenu"));
                }

                m_ProgressBarSlider.TotalNum = 0;
                m_ProgressBarSlider.CurrentNum = 0;
                m_ProgressBarSlider.slider.value = 1;
                m_RewardStageChest.SetActive(false);

                TextMeshProUGUILocalize loc = m_ProgressBarSlider.sliderText.GetComponent<TextMeshProUGUILocalize>();
                loc.SetTerm("Story.Finished");
                loc.enabled = true;
            }
        }

        public void RefreshDepth()
        {
            m_CurDepth = (MergeManager.PlayerData.GetDigTreasureCurDepth() + BoardRow) * 100;
            m_DeepText.text = m_CurDepth.ToString() + "M"; 
        }

        public override void RefreshSale()
        {
            int curLevel = MergeManager.PlayerData.GetCurMergeOfferLevel();
            IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
            if (curLevel > dataTable.Count)
            {
                m_SaleButton.gameObject.SetActive(false);
            }
            else
            {
                m_SaleButton.gameObject.SetActive(true);

                if (curLevel % 3 != 0)
                {
                    m_SaleNumText.text = (3 - curLevel % 3).ToString();
                    m_SaleNumText.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    m_SaleNumText.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public override void RefreshStorage()
        {
            base.RefreshStorage();

            m_StorageNumText.text = StorePropIds.Count.ToString();
            m_StorageButton.gameObject.SetActive(StorePropIds.Count > 0);

            if (StorePropIds.Count > 0)
            {
                int index = StorePropIds.Count - 1;
                var storedProp = StorePropIds[index];
                IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
                var propData = propDataTable.GetDataRow(storedProp.PropId);
                string spriteName = propData.AssetName;

                m_StoragePropImg.DOKill();
                m_StoragePropImg.color = new Color(1, 1, 1, 0);
                storageSpriteHandleList.Add(UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetSpriteKey(spriteName, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
                {
                    if (sp != null)
                    {
                        m_StoragePropImg.sprite = sp as Sprite;
                        m_StoragePropImg.DOFade(1, 0.1f);

                        if (!isBoardMoveStoreProp && m_GuideMenu != null) 
                        {
                            m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_DigDialog7);
                        }
                    }
                }));
            }
        }

        public override Vector3 GetStoragePropGeneratePos()
        {
            return m_StorageButton.transform.position;
        }

        public void ShowPickaxeAnim(Vector3 digPos)
        {
            Pickaxe pickaxe = null;
            foreach (Pickaxe p in m_PickaxeList)
            {
                if (!p.IsUsing)
                {
                    pickaxe = p;
                    break;
                }
            }

            if (pickaxe == null)
            {
                GameObject obj = Instantiate(m_PickaxePrefab, m_PickaxePrefab.transform.parent);
                obj.SetActive(true);
                pickaxe = obj.GetComponent<Pickaxe>();
                m_PickaxeList.Add(pickaxe);
            }

            pickaxe.ShowPickaxeAnim(digPos);
        }

        public void ShowDiggableEffect()
        {
            foreach (Square square in m_MergeBoard.m_Squares)
            {
                if (square != null && square.FilledProp != null && square.FilledProp.AttachmentId == 0) 
                {
                    if (square.FilledProp.PropId / 10000 == 5)
                    {
                        GameObject targetEffect = null;
                        foreach (GameObject effect in m_DiggableEffectList)
                        {
                            if (effect.transform.position == square.transform.position)
                            {
                                targetEffect = effect;
                                effect.SetActive(true);
                                break;
                            }
                        }

                        if (targetEffect == null)
                        {
                            GameObject obj = Instantiate(m_DiggableEffectPrefab, square.transform.position, Quaternion.identity, m_DiggableEffectPrefab.transform.parent);
                            obj.SetActive(true);
                            m_DiggableEffectList.Add(obj);
                        }
                    }
                }
            }
        }

        public void HideDiggableEffect()
        {
            foreach (GameObject effect in m_DiggableEffectList)
            {
                if (effect.activeSelf)
                {
                    effect.GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle", true);
                    effect.SetActive(false);
                }
            }
        }

        public void ShowLockPromptBox(Vector3 pos)
        {
            m_LockPromptBox.Show(pos);
        }

        public void HideLockPromptBox()
        {
            m_LockPromptBox.Hide();
        }

        public void ShowStageHitAnim()
        {
            m_FlyGemHitPos.DOKill();
            m_FlyGemHitPos.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.1f).onComplete += () =>
            {
                m_FlyGemHitPos.DOScale(Vector3.one, 0.1f);
            };
            m_RewardStageHitEffect.Play();
        }

        private void OnRewardStageButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeStageRewardMenu"));

            m_GuideMenu.FinishGuide(GuideTriggerType.Guide_DigDialog6);
        }

        public void OnStorageButtonClick()
        {
            WithdrawProp();
        }

        private void OnSaleButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
        }

        private void OnInfoButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeInfoMenu"));
        }

        protected override void OnCloseButtonClick()
        {
            if (MergeManager.Instance.CheckActivityHasStarted() && DateTime.Now > MergeManager.Instance.GetActivityEndTime())
            {
                if (GameManager.DataNode.GetData<bool>("ShowedMergeLastChanceMenu", false))
                {
                    if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                    {
                        GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGiveUpMenu"));
                    }
                    else
                    {
                        GameManager.UI.HideUIForm(this);
                        MergeManager.Instance.EndActivity();
                    }
                }
                else
                {
                    if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                    {
                        GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
                    }
                    else
                    {
                        GameManager.UI.HideUIForm(this);
                        MergeManager.Instance.EndActivity();
                    }
                }
            }
            else
            {
                if (!IsUseSupply && GameManager.Process.CurrentProcessName == ProcessType.AutoShowMergeProcess.ToString())
                {
                    GameManager.DataNode.SetData<int>("MergeGameToMapTime", 2);
                }

                GameManager.UI.HideUIForm(this);
            }

            GameManager.Process.EndProcess(ProcessType.AutoShowMergeProcess);
        }

        private void OnPickaxeBarButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGetBoxMenu"));
        }

        private void OnRefreshMergeEnergyBox(object sender, GameEventArgs e)
        {
            RefreshPickaxeBar();
        }

        public void MergeOfferComplete(object sender, GameEventArgs e)
        {
            RefreshSale();
        }
        private void OnMaxStageUpgrade(object sender, GameEventArgs e)
        {
            MaxStageUpgradeEventArgs ne = e as MaxStageUpgradeEventArgs;
            if (ne != null)
            {
                IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
                var propData = propDataTable.GetDataRow(ne.PropId);
                m_NewFossilPropMergedBoard.Init(propData.AssetName, ne.PropLogic, () =>
                {
                    m_NewFossilPropMergedBoard.Show();
                }, this);

                //int propId = ne.PropId;
                //UnityUtility.InstantiateAsync("MergeFlyItemSlot", m_EffectRoot, obj =>
                //{
                //    if (obj != null)
                //    {
                //        var slot = obj.GetComponent<MergeFlyItemSlot>();
                //        slot.Initialize(propId);
                //        slot.transform.position = ne.PropPos;
                //        obj.SetActive(true);
                //        slot.transform.DOScale(0.5f, 0.55f).SetDelay(0.35f);
                //        slot.transform.DOMove(m_SignImg.transform.position, 0.6f).SetEase(Ease.InBack).SetDelay(0.2f).onComplete = () =>
                //        {
                //            m_SignUpgradeHitEffect.Play();
                //            slot.gameObject.SetActive(false);
                //            RefreshSign();
                //        };

                //        GameManager.Task.AddDelayTriggerTask(0.9f, () =>
                //        {
                //            if (slot != null)
                //            {
                //                slot.Release();
                //            }
                //            UnityUtility.UnloadInstance(obj);
                //        });

                //        GameManager.Sound.PlayAudio(SoundType.SFX_Merge_Unlock_New_Item.ToString());
                //    }
                //});

                GameManager.Firebase.RecordMessageByEvent("Merge_Task_Unlock", new Firebase.Analytics.Parameter("stage", ne.PropId % 10100 - 1));
            }
        }

        public void ShowMergeFlyItemSlot(int propId, Vector3 propPos)
        {
            if (propId / 10000 == 1 || propId / 10000 == 2 || propId / 10000 == 4 || propId / 10000 == 5 || propId / 10000 == 7 || propId / 10000 == 11 || propId / 10000 == 12) 
                return;

            UnityUtility.InstantiateAsync("MergeFlyItemSlot", m_EffectRoot, obj =>
            {
                if (obj != null)
                {
                    var slot = obj.GetComponent<MergeFlyItemSlot>();
                    slot.Initialize(propId);
                    slot.transform.position = propPos;
                    obj.SetActive(true);
                    slot.transform.transform.DORotate(new Vector3(0, 0, 15), 0.1f).SetEase(Ease.InOutSine).SetLoops(4, LoopType.Yoyo).SetDelay(0.3f);
                    slot.transform.DOMove(m_SignImg.transform.position, 0.6f).SetEase(Ease.InBack).SetDelay(0.5f).onComplete = () =>
                    {
                        m_SignUpgradeHitEffect.Play();
                        slot.gameObject.SetActive(false);
                        m_SignImg.transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.2f);
                    };

                    GameManager.Task.AddDelayTriggerTask(1.1f, () =>
                    {
                        if (slot != null)
                        {
                            slot.Release();
                        }
                        UnityUtility.UnloadInstance(obj);
                    });

                    GameManager.Sound.PlayAudio(SoundType.SFX_Merge_Unlock_New_Item.ToString());
                }
            });
        }

        #region Anim

        public override void ShowClickSupplyFingerAnim()
        {
            if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                ShowDiggableEffect();
            else
                ShowFingerAnim(m_PickaxeBarButton.transform.position);
        }

        public override void ShowClickStorageFingerAnim()
        {
            ShowFingerAnim(m_StorageButton.transform.position);
        }

        public override void HideFingerAnim()
        {
            base.HideFingerAnim();

            HideDiggableEffect();
            HideLockPromptBox();
        }

        #endregion

        #region PropHint

        public override void ShowPropHint()
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
                    //是否可挖掘
                    if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                    {
                        ShowClickSupplyFingerAnim();
                        return;
                    }

                    //棋盘上是否有特殊道具
                    needHintProps = MergeManager.Merge.GetCanClickSpecialProps();

                    //如果有宝箱道具的话，确认其是否解锁，要是待解锁在宝箱上显示动态手指，同时宝箱钥匙抖动
                    foreach (var pair in needHintProps)
                    {
                        if (pair != null && pair.Count > 0) 
                        {
                            if (pair[0].PropId == 30101 && pair[0].Prop != null) 
                            {
                                PropLogic chestProp = pair[0];
                                ChestProp_DigTreasure chest = chestProp.Prop as ChestProp_DigTreasure;
                                if (chest != null && !chest.IsUnLock)
                                {
                                    var key = MergeManager.Merge.GetPropOnChessboard(110101);
                                    if (key != null)
                                    {
                                        needHintProps.Clear();
                                        showingMergeHintProps.Clear();

                                        Vector3 direction = (chestProp.Square.transform.position - key.Square.transform.position).normalized;
                                        chestProp.ShowMergeHintAnim(-direction);
                                        key.ShowMergeHintAnim(direction);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    //棋盘上是否有可merge道具
                    var canMergeProps = MergeManager.Merge.GetCanMergeProps();

                    foreach (var props in canMergeProps)
                    {
                        needHintProps.AddLast(props);
                    }

                    //是否有存储的道具
                    if (needHintProps.Count == 0 && MergeManager.PlayerData.GetAllStorePropIds().Count > 0) 
                    {
                        //提示点击存储按钮
                        ShowClickStorageFingerAnim();
                        return;
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
                        {
                            //如果是宝箱道具的话，确认其是否已经解锁
                            if (list[0].PropId == 30101)
                            {
                                if (list[0].Prop == null)
                                {
                                    showingMergeHintProps.Clear();
                                }
                                else
                                {
                                    ChestProp_DigTreasure chest = list[0].Prop as ChestProp_DigTreasure;
                                    if (chest == null || !chest.IsUnLock)
                                    {
                                        showingMergeHintProps.Clear();
                                    }
                                }
                            }

                            break;
                        }
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
            else
            {
                //是否有存储的道具
                if (MergeManager.PlayerData.GetAllStorePropIds().Count > 0)
                {
                    //提示点击存储按钮
                    ShowClickStorageFingerAnim();
                    return;
                }

                //提示点击体力按钮
                ShowClickSupplyFingerAnim();
            }
            showingMergeHintProps.Clear();
        }

        #endregion

        #region reciver
        public ReceiverType ReceiverType => ReceiverType.MergeEnergyBox;

        public GameObject GetReceiverGameObject() => m_PickaxeBarIcon;

        public void OnFlyHit(TotalItemData type)
        {

        }

        public void OnFlyEnd(TotalItemData type)
        {
            if (m_PickaxeBarIcon != null)
            {
                GameManager.Sound.PlayAudio("SFX_itemget");
                m_PickaxeBarIcon.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                {
                    m_PickaxeBarIcon.transform.localScale = Vector3.one;
                };
            }
        }

        public Vector3 GetItemTargetPos(TotalItemData type)
        {
            return m_PickaxeBarIcon.transform.position;
        }
        #endregion
    }
}
