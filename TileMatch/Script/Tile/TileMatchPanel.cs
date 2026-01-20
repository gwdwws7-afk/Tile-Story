using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;
using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using MySelf.Tools.AnimUtil;
using Spine.Unity;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public sealed class TileMatchPanel : CenterForm, IItemFlyReceiver
{
	private const string TileItemPrefabPool = "TileItemPool";
	[SerializeField]
	private TextMeshProUGUILocalize Level_Text, Date_Text;

	public GameGoldBar Gold_Bar;
	//public GameGasolineBar Gasoline_Bar;
    
	[SerializeField]
    private DelayButton Setting_Btn;
	private Transform[] TileMatchPositions;
	[SerializeField]
	private Transform[] TileMatchPositions_7;
	[SerializeField]
	private Transform[] TileMatchPositions_8;
	[SerializeField]
	private RectTransform TileMatchParent, CenterRectTrans;
	[SerializeField]
	private Transform BackParent, ChooseParent, SkillItemParent, activityParent;
	[SerializeField]
	private Transform[] BackTileTrans;
	[SerializeField]
	private RectTransform Bottom, Top;
	[SerializeField]
	private Image RedBottomBG, BlackBottomBG;
	[SerializeField]
	private MaterialPresetName[] CombText_Materials;
	[SerializeField]
	private SkeletonGraphic CombAnim;

	[SerializeField]
	private Transform Prop_Parent, LevelBarOverPos_Trans;

	[SerializeField] private DelayButton Editor_Btn;
	[SerializeField] private Image[] Editor_Images;
	[SerializeField] private GameObject[] Editor_Objs;
	//[SerializeField] private RectTransform Editor_Rect;

	[SerializeField] private DaliyWatchAdsPrefab DaliyWatchAds_Prefab;
	[SerializeField] private DaliyWatchAdsPrefabNew DaliyWatchAdsPrefabNew;
	[SerializeField] private Transform DaliyWatchPos_Trans;
	[SerializeField] private CanvasGroup CanvasGroup;
	[SerializeField] private Image SettingImage;
	[SerializeField] private Sprite[] SettingSprites;

	[SerializeField] private CanvasGroup FirstLevelGuide;

	[SerializeField] private TimeBar timeBar;
	
	public (int, int) SurplusNum
	{
		get
		{
			int surplusNum = GetBackTotalNum() + GetMapTotalNum() + GetChooseTotalNum(true);

			if (CheckCanShowGameAdsPropButton(surplusNum))  
			{
				DaliyWatchAds_Prefab.transform.localPosition = Vector3.zero;
				DaliyWatchAds_Prefab.transform.parent.SetParent(DaliyWatchPos_Trans, false);
				DaliyWatchAds_Prefab.SetActive(true);
			}
			else
			{
				DaliyWatchAds_Prefab.SetActive(false);
			}

			return (surplusNum, panelData.TileMatchData.TotalCount);
		}
	}
	private TileMatchPanelData panelData;

	public Dictionary<int, SortedDictionary<int, (int, TileItem)>> tileMapDict = new Dictionary<int, SortedDictionary<int, (int, TileItem)>>();
	public OrderDictionary<int, List<TileItem>> chooseItemDict = new OrderDictionary<int, List<TileItem>>();
	public Dictionary<TileItem, int> backTileDict = new Dictionary<TileItem, int>();
	public List<SkillItem> skillList = new List<SkillItem>(4);

	public Transform ActivityParent => activityParent;

	private bool isAddOneStepState;
	[SerializeField]
	private RectTransform ChosenBar;
	[SerializeField]
	private Image Split, AddStepTip;
	public DelayButton AddOneStep_Btn;
	public TextMeshProUGUI AddOneStepNumText;
	public GameObject AddOneStepAddIcon;
	public GameObject AddOneStepAdsBtn;

	public Vector3 BarPos => RedBottomBG.transform.position;

	public bool IsAddOneStepState
    {
		set => isAddOneStepState = value;
    }

	public Action ElementGuideFinish;
	public GameObject ChosenBarEffect;

	private bool willTriggerAddOneStep;

	#region 打点需要数据
	private int openShopBuyItemPanelCount = 1;

	private void ClearRecordData()
	{
		openShopBuyItemPanelCount = 1;
	}
	#endregion

	private bool If_Have_Thumb=>GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Have_Thumb, true);
	
	public override void OnInit(UIGroup uiGroup, System.Action completeAction = null, object userData = null)
	{
		isStartTileMoveAnim = false;
		isCheckGameLostOver = true;
		isGameLose = false;
		isGameWin = false;
		willTriggerAddOneStep = GameManager.DataNode.GetData<bool>("BoostIsSelected_" + (TotalItemType.Prop_AddOneStep).ToString(), false);
		if (!willTriggerAddOneStep && GameManager.DataNode.GetData<bool>("UsedAdsBoost", false)) 
        {
			willTriggerAddOneStep = Random.Range(0, 2) >= 1 ? true : false;
		}

		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChangBGImageID, Constant.GameConfig.CalendarBgIndex));
		}
		else
		{
			GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChangBGImageID, GameManager.PlayerData.BGImageIndex));
		}

		RewardManager.Instance.RegisterItemFlyReceiver(this);
		GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEventCallBack);
		GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId,RefreshRv);
		GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, RewardCallBack);

		CanvasGroup.blocksRaycasts = true;
		panelData = TileMatchPanelData.InitData();

		GameManager.Task.AddDelayTriggerTask(ShowPkStart()?3f:0, () =>
		{
			if (RefreshSkillUnlockData(panelData.LevelNum) || ShowElementUnlockPanel(panelData.LevelNum))
			{
				float delayTime = 0.4f;
				delayTime += 0.4f;
				if (panelData.LevelNum == 51)//火箭教程关延后时间长点
					delayTime = 1.2f;
				ElementGuideFinish = null;
				ElementGuideFinish = () =>
				{
					ShowBoost(delayTime);
				};
			}
			else if (ShowFirstTryPanel(() => { ShowBoost(0); })) 
	        {
	        }
			else if(ShowLevelSupportPackMenu(() => { ShowBoost(0); }))
            {
            }
			else
	        {
				ShowBoost(2f);
			}

			ShowFirstLevelGuideText(true);
			SetText();
			SetSettingSprite();
			SetBtnEvent();

			RefreshSkillItemState(true);

			ShowTileMatchMap(panelData.TileMatchData.AllLayerTileDict, panelData.RecordRandomMoveDict);
			ChangeTileScale();

			GameManager.Task.AddDelayTriggerTask(1f, () =>
			{
				RefreshSkillItemState();
				RefreshLevelBar();
			});
			GameManager.Task.AddDelayTriggerTask(1.8f, () =>
			{
				ShowGamePlayGuide();

				int timeLimit = 0;
				var levelID = GameManager.DataTable.GetDataTable<DTLevelID>();
				if (levelID != null)
				{
					timeLimit = levelID.Data.GetLevelLimitTime(panelData.LevelNum);
				}
				else
				{
					Debug.LogError("TileMatchPAnel: GetDataTable<DTLevelID> is null");
				}
				if (timeLimit > 0)
				{
					timeBar.SetCountDownAction((isTimeOver) =>
					{
						if (GetMapTotalNum() > 0 || backTileDict.Count > 0)
						{
							GameManager.DataNode.SetData("TimeLimit_AddTime", timeBar.ContinueAddTime);
							GameManager.DataNode.SetData("TimeLimit_IsTimeOver", isTimeOver);
							ShowLoseItemPanel(false);
						}
					});
					timeBar.SetNoFoucsAction(() =>
					{
						GameManager.UI.ShowUIForm("GameSettingPanel",UIFormType.PopupUI,null, null, "ShowByClick");
					});
					// 显示倒计时关的提示
					GameManager.UI.ShowUIForm("TimeLimitGuidePrefab",form =>
					{
						var uiform = form as TimeLimitGuidePrefab;
						uiform.SetInfo(timeLimit, timeBar.GetClockTarget());
						uiform.SetCallBack(() =>
						{
							timeBar.ResetData();
							timeBar.SetLevelTotalTime(timeLimit);
							timeBar.PlayShowAnim();
						});
					});
				}
			});

			BlackBottomBG.enabled = true;
			RedBottomBG.gameObject.SetActive(false);

			GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.EnterTileMatchPanel));
			GameManager.Task.ClearTargetCollectNum(TaskTarget.CollectLadybug);

			GameManager.Sound.PlayMusic(GameManager.PlayerData.HappyBgMusicName);
			GameManager.Sound.ForbidSound(SoundType.LOSE.ToString(), false);

			isAddOneStepState = false;
			ChosenBarEffect.SetActive(false);
			RefreshChosenBar();
			triggerStage = 0;
		});

		if (GameManager.Ads.IsBannerAdInHideStatus)
			GameManager.Ads.ShowBanner(clearRequests: true);

		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
	{
#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(0))
#else
		if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0)) 
#endif
		{
			watchAdsItemTimer = 5;
		}

		if (watchAdsItemTimer > 0)
		{
			watchAdsItemTimer -= elapseSeconds;
			if (watchAdsItemTimer <= 0)
			{
				ShowWatchAdsItemGiftPack();
				watchAdsItemTimer = 5;
			}
		}
		
		RecordLastTouchTime();

		if (rotateTiles.Count > 0)
        {
            int num = 0;
            rotateDegree += 1;
            foreach (RotateTile tile in rotateTiles)
            {
				int centerNum = 40;
				if (tile.IsFinish || rotateDegree > (centerNum + tile.EndIndex)) 
                {
                    tile.IsFinish = true;
					tile.CachedTrans.localPosition = tile.TargetPos;
                    tile.CachedTrans.localRotation = Quaternion.Euler(0, 0, 0);
                    foreach (Transform child in tile.CachedTrans)
                    {
                        child.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                    num++;
                }
                else
                {
					if (rotateDegree <= tile.StartIndex)
						continue;

					float degree = tile.TargetRotation / (float)(centerNum - tile.StartIndex + tile.EndIndex);
					int nowDegree = rotateDegree - tile.StartIndex;
					tile.CachedTrans.RotateAround(CenterRectTrans.position, Vector3.forward, -degree);
                    foreach (Transform child in tile.CachedTrans)
                    {
                        child.localRotation = Quaternion.Euler(0, 0, degree * nowDegree);
                    }

					if (nowDegree > 0 && nowDegree <= 10)
					{
						Vector2 targetDir = tile.CachedTrans.position - CenterRectTrans.position;
						tile.CachedTrans.position += (Vector3)(targetDir.normalized * tile.AroundCenterDelta / 10f);
					}
					else if (nowDegree > centerNum - tile.StartIndex + tile.EndIndex - 10 && nowDegree <= centerNum - tile.StartIndex + tile.EndIndex) 
                    {
                        Vector2 targetDir = tile.CachedTrans.position - CenterRectTrans.position;
						tile.CachedTrans.position -= (Vector3)(targetDir.normalized * tile.TowardCenterDelta / 10f);
                    }
                }
            }

            if (num >= rotateTiles.Count)
            {
                foreach (RotateTile tile in rotateTiles)
                {
					tile.Callback?.Invoke();
				}
                rotateTiles.Clear();
            }
        }

		base.OnUpdate(elapseSeconds, realElapseSeconds);
	}

	public override void OnShow(System.Action<UIForm> showSuccessAction = null, object userData = null)
	{
		base.OnShow(showSuccessAction, userData);

		bool isSpecialGoldTile = DTLevelUtil.IsSpecialGoldTile(GameManager.PlayerData.RealLevel());
		if (isSpecialGoldTile)
		{
			//如果是金块关，显示金块栏，预加载金块飞行对象
			if (isSpecialGoldTile)
			{
				Gold_Bar.totalText.text = panelData.TileMatchData.GoldTileCount.ToString();
				if (!GameManager.ObjectPool.HasObjectPool("GoldFlyObjectPool"))
				{
					GameManager.ObjectPool.CreateObjectPool<EffectObject>("GoldFlyObjectPool", float.PositiveInfinity,
						50, float.PositiveInfinity);

					if (!SystemInfoManager.IsSuperLowMemorySize)
					{
						GameManager.ObjectPool.PreloadObjectPool<EffectObject>("GoldFlyObjectPool", "GoldFlyObject",
							CenterRectTrans.GetChild(0), 3);	
					}
				}
			}
			Gold_Bar.gameObject.SetActive(isSpecialGoldTile);
			//如果有通行证，且奖励未拿满，显示油桶栏
			// if (isTilePassGasoline)
			// {
			// 	Gasoline_Bar.totalText.text = panelData.TileMatchData.GasolineCount.ToString();
			// }
			// Gasoline_Bar.gameObject.SetActive(isTilePassGasoline);
		}
		else
		{
            Gold_Bar.gameObject.SetActive(false);
            // Gasoline_Bar.gameObject.SetActive(false);
        }
		
		int timeLimit = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum);
		if (timeLimit > 0)
		{
			Date_Text.gameObject.SetActive(false);
		}
		timeBar.body.SetActive(false);
		
		Top.DOKill();
		Top.anchoredPosition = new Vector3(0, 500);
		Top.DOAnchorPos(Vector3.zero, 0.4f).SetEase(Ease.OutBack).SetDelay(0.02f);

		Bottom.DOKill();
		if (!panelData.IsHaveRecord)
		{
			Bottom.anchoredPosition = new Vector3(0, -500f);
			Bottom.DOAnchorPos(Vector3.zero, 0.4f).SetEase(Ease.OutBack).SetDelay(0.02f);
		}
		else
        {
			Bottom.anchoredPosition = Vector2.zero;
		}
	}

	public override void OnRelease()
	{
		RewardManager.Instance.UnregisterItemFlyReceiver(this);
		GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEventCallBack);
		GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, RewardCallBack);
		GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId,RefreshRv);

		foreach (var child in TileMatchParent.GetComponentsInChildren<TileItem>()) UnSpawnTile(child.gameObject);
		foreach (var child in BackParent.GetComponentsInChildren<TileItem>()) UnSpawnTile(child.gameObject);
		foreach (var child in ChooseParent.GetComponentsInChildren<TileItem>()) UnSpawnTile(child.gameObject);
		foreach (var child in Prop_Parent.GetComponentsInChildren<Transform>()) if (child != Prop_Parent) Destroy(child.gameObject);
		foreach (var child in ActivityParent.GetComponentsInChildren<Transform>()) if (child != ActivityParent) Destroy(child.gameObject);

		panelData = null;

		tileMapDict.Clear();
		chooseItemDict.Clear();
		backTileDict.Clear();
		recordSequenceDict.Clear();

		DaliyWatchAds_Prefab.SetActive(false);
		DaliyWatchAdsPrefabNew.gameObject.SetActive(false);
		RedBottomBG.gameObject.SetActive(false);
		CombAnim.gameObject.SetActive(false);

		Bottom.DOKill();
		Top.DOKill();
		Top.anchoredPosition = new Vector3(0, 500f);
		Bottom.anchoredPosition = new Vector3(0, -500f);
		// if (redBGSequece != null) redBGSequece.Kill();
		// redBGSequece = null;
		if (punchBGSequece != null) punchBGSequece.Kill();
		punchBGSequece = null;

		//sequence = null;
		isShowingRewardAnim = false;

		if (GameManager.ObjectPool.HasObjectPool("GoldFlyObjectPool"))
		{
			GameManager.ObjectPool.DestroyObjectPool("GoldFlyObjectPool");
		}
		ClearRecordData();
		base.OnRelease();
	}

	public void RefreshChosenBar(bool showAnim = false)
	{
		void Callback()
		{
			Split.gameObject.SetActive(isAddOneStepState);
			AddStepTip.gameObject.SetActive(isAddOneStepState);
			Split.color = Color.white;
			AddStepTip.color = Color.white;
			if (isAddOneStepState)
			{
                TileMatchPositions = TileMatchPositions_8;
                BlackBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 139);
                ChosenBar.anchoredPosition = new Vector2(-476.5f, -4);
                RedBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(992, 162);
                panelData.TileCellNum = 8;
				panelData.IsUseAddSkill = true;
				AddOneStep_Btn.gameObject.SetActive(false);
			}
			else
			{
				panelData.TileCellNum = 7;
				panelData.IsUseAddSkill = false;

				TileMatchPositions = TileMatchPositions_8;
				if (willTriggerAddOneStep || GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockGameAddOneStepBoostLevel || GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge) 
                {
                    BlackBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 139);
                    ChosenBar.anchoredPosition = new Vector2(-420.1f, -4);
                    RedBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(872, 162);
                    AddOneStep_Btn.gameObject.SetActive(false);
				}
                else
                {
					BlackBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 139);
					ChosenBar.anchoredPosition = new Vector2(-476.5f, -4);
					RedBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(992, 162);
					RefreshAddOneStepButton();
					AddOneStep_Btn.gameObject.SetActive(true);
				}
			}
		}

		if (showAnim && isAddOneStepState) 
        {
			Transform root = ChosenBar.parent;
			BlackBottomBG.GetComponent<RectTransform>().DOSizeDelta(new Vector2(960, 139), 0.2f);
			ChosenBar.DOAnchorPos(new Vector2(-476.5f, -4), 0.2f);
			Split.color = new Color(1, 1, 1, 0);
			AddStepTip.color = new Color(1, 1, 1, 0);
			Split.gameObject.SetActive(true);
			AddStepTip.gameObject.SetActive(true);
			Split.DOColor(Color.white, 0.1f).SetDelay(0.05f);
			AddStepTip.DOColor(Color.white, 0.1f).SetDelay(0.05f);
			root.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.1f).SetEase(Ease.OutQuart).onComplete = () =>
			{
				root.DOScale(new Vector3(0.9f, 0.9f, 1f), 0.15f).SetEase(Ease.InCubic).onComplete = () =>
				{
					root.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
					{
						RecordChooseTileMove(null);
					};
					Callback();
				};

				GameManager.Sound.PlayAudio(SoundType.SFX_Booster_Grid_Appear.ToString());
				UnityUtil.EVibatorType.Medium.PlayerVibrator();
			};

			ChosenBarEffect.SetActive(false);
			GameManager.Task.AddDelayTriggerTask(0.15f, () =>
			{
				ChosenBarEffect.SetActive(true);
				GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
			});
		}
        else
        {
			Callback();
		}
	}

	public void RefreshAddOneStepButton()
    {
		int num = GameManager.PlayerData.GetItemNum(TotalItemData.Prop_AddOneStep);
		AddOneStepNumText.text = num.ToString();
		AddOneStepAddIcon.SetActive(num <= 0);
		AddOneStepAddIcon.transform.parent.gameObject.SetActive(true);
		AddOneStep_Btn.transform.localScale = Vector3.one;

		bool isShowAdsBtn = num <= 0 &&
		                    GameManager.Ads.CheckRewardedAdIsLoaded();
		AddOneStepAdsBtn.gameObject.SetActive(isShowAdsBtn);
		AddOneStepAddIcon.SetActive(num<=0 && !isShowAdsBtn);
    }

	private void SetSettingSprite()
	{
		SettingImage.sprite = SettingSprites[DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel())];
	}

	private void SetBtnEvent()
	{
		Setting_Btn.OnReset();
		Setting_Btn.OnInit(() =>
		{
			GameManager.UI.ShowUIForm("GameSettingPanel",UIFormType.PopupUI,null, null, "ShowByClick");

			GameManager.Firebase.RecordMessageByEvent
			(Constant.AnalyticsEvent.Level_Setting,
			new Parameter("Level", GameManager.PlayerData.NowLevel));
		});

		AddOneStep_Btn.OnReset();
		AddOneStep_Btn.OnInit(() =>
		{
			bool isHaveAddOneStepItem = GameManager.PlayerData.GetItemNum(TotalItemData.Prop_AddOneStep)>0;
			bool isHaveRv = GameManager.Ads.CheckRewardedAdIsLoaded();
			if (!isHaveAddOneStepItem && isHaveRv)
			{
				GameManager.Ads.ShowRewardedAd("AddOneStepByRV");
			}else if (!GameManager.PlayerData.UseItem(TotalItemData.Prop_AddOneStep, 1))  
			{
				GameManager.UI.ShowUIForm("BoostPurchaseMenu",UIFormType.PopupUI, null, null, TotalItemData.Prop_AddOneStep);
				return;
			}else  ShowUseAddOneStepSkill();
		});
		
		skillList.Clear();
		UnityUtility.FillGameObjectWithFirstChild<SkillItem>(SkillItemParent.gameObject, 4, (index, comp) =>
		  {
			  ShowSkillItem(comp, index);
			  skillList.Add(comp);
			  //comp.GetComponent<RectTransform>().anchoredPosition = new Vector2(125 + 250 * index, -50);
		  });

#if UNITY_EDITOR
		Editor_Btn.SetBtnEvent(() =>
		{
			foreach (var image in Editor_Images)
			{
				image.enabled = false;
				image.color = new Color(0, 0, 0, 0);
			}
			foreach (var obj in Editor_Objs)
			{
				obj.gameObject.SetActive(false);
			}

			//Editor_Rect.anchoredPosition = new Vector2(-476.5f, 300f);
			GameManager.UI.HideUIForm("MapMainBGPanel");
		});
#endif
	}

	private void ShowUseAddOneStepSkill()
	{
		AddOneStep_Btn.interactable = false;
		IsAddOneStepState = true;
		panelData.TileCellNum = 8;
		panelData.IsUseAddSkill = true;
		Split.gameObject.SetActive(true);
		AddStepTip.gameObject.SetActive(true);
		AddOneStepAddIcon.transform.parent.gameObject.SetActive(false);
		AddOneStepAdsBtn.SetActive(false);

		AddOneStep_Btn.transform.DOKill();
		AddOneStep_Btn.transform.DOScale(Vector3.one * 1.2f, 0.15f).OnComplete(() =>
		{
			GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
				"TileDestroyEffect",
				"TileItemDestroyEffectPool",
				0.4f,
				AddOneStep_Btn.transform.position,
				AddOneStep_Btn.transform.rotation,
				parent: transform);
			AddOneStep_Btn.transform.DOScale(Vector3.one * 0.2f, 0.2f).OnComplete(() =>
			{
				AddOneStep_Btn.gameObject.SetActive(false);
			});
			GameManager.Sound.PlayAudio(SoundType.SFX_PlusOneStep.ToString());
		});

		ShowLastBoxTip();
	}

	private void ShowSkillItem(SkillItem skillItem, int index)
	{
		TotalItemData type = panelData.SkillItemList[index];
		int num = GameManager.PlayerData.GetItemNum(type);
#if UNITY_EDITOR
		skillItem.gameObject.name = type.ToString();
#endif
		skillItem.Init(
			num, type, IsUnLock(type, out int levelNum),
			panelData.TileMatchData.TileLevelHardType, index, NeedCoinNumBuyProp(type), () =>
		{
			timeBar.PauseClock();
			HandleSkillEvent(skillItem, type, () =>
			 {
				 ShowSkillItem(skillItem, index);
			 });
		});
	}

	private void ShowPropBackToBackItemsAnim(GameObject AnimObj)
	{
		var s = AnimObj.GetComponentInChildren<SkeletonGraphic>(true);
		Transform animTrans = s.transform;
		animTrans.position = CenterRectTrans.position;
		animTrans.localScale = Vector3.zero;
		s.gameObject.SetActive(true);
		var bgImage = AnimObj.GetComponentInChildren<Image>(true);
		void animCallback()
		{
			s.gameObject.SetActive(false);
			GameManager.Task.AddDelayTriggerTask(0.3f, () =>
			{
				CanvasGroup.blocksRaycasts = true;

				if (AnimObj != null)
				{
					UnityUtility.UnloadInstance(AnimObj);
					GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
				}
			});
		}

		animTrans.DOScale(1.2f, 0.2f).onComplete = () => { animTrans.DOScale(1f, 0.2f); };
		bgImage.color = new Color(1, 1, 1, 0);
		bgImage.gameObject.SetActive(true);
		bgImage.DOFade(1, 0.2f);

		int key = chooseItemDict.GetLastKey();
		TileItem tileItem = null;
		if (chooseItemDict[key].Count > 0)
			tileItem = chooseItemDict[key][chooseItemDict[key].Count - 1];

		if (tileItem == null)
		{
			chooseItemDict.Remove(key);
			ChooseTileToBackTileItems();
			// 没有查证tileItem判断的是什么，可能不需要添加开启倒计时
			GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
			return;
		}

		chooseItemDict[key].Remove(tileItem);
		if (chooseItemDict[key].Count <= 0)
		{
			chooseItemDict.Remove(key);
		}

		OnChooseItemDictChange();

		tileItem.transform.SetParent(BackParent, true);

		AddToBackDict(tileItem);
		DOTween.Sequence()
			.AppendInterval(0.6f)
			.Append(animTrans.DOMove(tileItem.transform.position, 0.4f).OnComplete(() =>
			{
				s.AnimationState.SetAnimation(0, "03_1", false);
				float newScale = tileItem.transform.localScale.x * 1.25f;
				tileItem.transform.DOScale(newScale, 0.15f).SetDelay(0.3f);
			}))
			.Join(bgImage.DOFade(0, 0.2f))
			.AppendInterval(0.5f)
			.AppendCallback(() => { GameManager.Sound.PlayAudio(SoundType.SFX_UndoTool.ToString()); })
			.Append(tileItem.transform.DOMove(
				BackTileTrans[backTileDict[tileItem] % BackTileTrans.Length].position +
				Vector3.up * BackOffsetY(tileItem), 0.2f))
			.Join(animTrans
				.DOMove(
					BackTileTrans[backTileDict[tileItem] % BackTileTrans.Length].position +
					Vector3.up * BackOffsetY(tileItem), 0.2f).OnComplete(() =>
				{
					s.AnimationState.SetAnimation(0, "03_2", false);
					tileItem.transform.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Back), 0.15f)
						.SetEase(Ease.Linear).SetDelay(0.1f);
				}))
			.SetEase(Ease.OutSine)
			.AppendInterval(0.1f)
			.OnComplete(animCallback);

		tileItem.Init(0, 0, tileItem.Data.TileID, tileItem.Data.AttachID, TileMoveDirectionType.None, null, null,
			SetTileBtnEvent);
		tileItem.transform.SetAsLastSibling();
		tileItem.SetClickState(true);
		SaveToJson();
	}

	private void ShowPropBackToTileBoardAnim(GameObject AnimObj)
	{
		var s = AnimObj.GetComponentInChildren<SkeletonGraphic>(true);
		Transform animTrans = s.transform;
		animTrans.position = CenterRectTrans.position;
		animTrans.localScale = Vector3.zero;
		s.gameObject.SetActive(true);
		var bgImage = AnimObj.GetComponentInChildren<Image>(true);

		animTrans.DOScale(1.2f, 0.2f).onComplete = () => { animTrans.DOScale(1f, 0.2f); };
		bgImage.color = new Color(1, 1, 1, 0);
		bgImage.gameObject.SetActive(true);
		bgImage.DOFade(1, 0.2f);

		int key = chooseItemDict.GetLastKey();
		TileItem tileItem = null;
		if (chooseItemDict[key].Count > 0)
			tileItem = chooseItemDict[key][chooseItemDict[key].Count - 1];

		if (tileItem == null)
		{
			chooseItemDict.Remove(key);
			ChooseTileToBackTileItems();
			// 没有查证tileItem判断的是什么，可能不需要添加开启倒计时
			GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
			return;
		}

		void animCallback()
		{
			s.gameObject.SetActive(false);
			GameManager.Task.AddDelayTriggerTask(0.3f, () =>
			{
				CanvasGroup.blocksRaycasts = true;

				if (AnimObj != null)
				{
					UnityUtility.UnloadInstance(AnimObj);
					GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
				}
			});
		}
		
		chooseItemDict[key].Remove(tileItem);
		if (chooseItemDict[key].Count <= 0)
		{
			chooseItemDict.Remove(key);
		}

		OnChooseItemDictChange();

		int tileID = tileItem.Data.TileID;
		int layer = tileItem.ClickInfo.Layer;
		int mapId = tileItem.ClickInfo.MapID;
		TileMoveDirectionType moveIndex = tileItem.ClickInfo.moveIndex;
		if (!tileMapDict.ContainsKey(tileItem.ClickInfo.Layer))
		{
			tileMapDict.Add(layer, new SortedDictionary<int, (int, TileItem)>());
		}
		tileMapDict[layer].Add(mapId, (tileID, tileItem));
		var targetPos = TileMatchUtil.GetTilePos(layer, mapId, moveIndex, panelData.RecordRandomMoveDict);
		
		DOTween.Sequence()
			.AppendInterval(0.6f)
			.Append(animTrans.DOMove(tileItem.transform.position, 0.4f).OnComplete(() =>
			{
				s.AnimationState.SetAnimation(0, "03_1", false);
				float newScale = tileItem.transform.localScale.x * 1.25f;
				tileItem.transform.DOScale(newScale, 0.15f).SetDelay(0.3f);
			}))
			.Join(bgImage.DOFade(0, 0.2f))
			.AppendInterval(0.5f)
			.AppendCallback(() =>
			{
				tileItem.transform.SetParent(TileMatchParent, true);
				if (tileMapDict.ContainsKey(layer))
				{
					foreach (var pair in tileMapDict[layer])
					{
						if (pair.Key > mapId && pair.Value.Item2 != null)  
						{
							tileItem.transform.SetSiblingIndex(pair.Value.Item2.transform.GetSiblingIndex());
							break;
						}
					}
				}
					
				Dictionary<int, int> emptyIndexs = new Dictionary<int, int>();
				if (tileItem.ClickInfo.CoverIndexs != null)
				{
					foreach (var child in tileItem.ClickInfo.CoverIndexs)
					{
						foreach (var index in child.Value)
						{
							if (tileMapDict.ContainsKey(child.Key) && tileMapDict[child.Key].ContainsKey(index))
							{
								TileItem tempItem = tileMapDict[child.Key][index].Item2;
								if (tempItem != null && !tempItem.IsDestroyed && tempItem.Data.BeCoverIndexs != null &&
								    tempItem.Data.BeCoverIndexs.ContainsKey(layer)) 
								{
									tempItem.Data.BeCoverIndexs[layer].Add(mapId);
									tempItem.SetColor();
								}
								else
								{
									emptyIndexs.TryAdd(child.Key, index);
								}
							}
							else
							{
								emptyIndexs.TryAdd(child.Key, index);
							}
						}
					}

					foreach (var pair in emptyIndexs)
					{
						if (tileItem.ClickInfo.CoverIndexs.ContainsKey(pair.Key))
						{
							tileItem.ClickInfo.CoverIndexs[pair.Key].Remove(pair.Value);
						}
					}
				}
		
				if (tileItem.ClickInfo.BeCoverIndexs != null)
				{
					emptyIndexs.Clear();
					foreach (var child in tileItem.ClickInfo.BeCoverIndexs)
					{
						foreach (var index in child.Value)
						{
							if (tileMapDict.ContainsKey(child.Key) && tileMapDict[child.Key].ContainsKey(index))
							{
								TileItem tempItem = tileMapDict[child.Key][index].Item2;
								if (tempItem != null && !tempItem.IsDestroyed && tempItem.Data.CoverIndexs != null &&
								    tempItem.Data.CoverIndexs.ContainsKey(layer)) 
								{
									tempItem.Data.CoverIndexs[layer].Add(mapId);
								}
								else
								{
									emptyIndexs.TryAdd(child.Key, index);
								}
							}
							else
							{
								emptyIndexs.TryAdd(child.Key, index);
							}
						}
					}
			
					foreach (var pair in emptyIndexs)
					{
						if (tileItem.ClickInfo.BeCoverIndexs.ContainsKey(pair.Key))
						{
							tileItem.ClickInfo.BeCoverIndexs[pair.Key].Remove(pair.Value);
						}
					}
				}

				tileItem.AttachLogic = null;
				tileItem.Init(layer, mapId, tileID, 0, moveIndex, tileItem.ClickInfo.CoverIndexs, tileItem.ClickInfo.BeCoverIndexs,
					SetTileBtnEvent);
				
				GameManager.Sound.PlayAudio(SoundType.SFX_UndoTool.ToString());
			})
			.Append(tileItem.transform.DOLocalMove(
				targetPos, 0.2f))
			.Join(animTrans
				.DOMove(
					TileMatchParent.TransformPoint(targetPos), 0.2f).OnComplete(() =>
				{
					s.AnimationState.SetAnimation(0, "03_2", false);
					tileItem.transform.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Back), 0.15f)
						.SetEase(Ease.Linear).SetDelay(0.1f);
				}))
			.SetEase(Ease.OutSine)
			.AppendInterval(0.1f)
			.OnComplete(animCallback);
		
		tileItem.SetClickState(true);
		SaveToJson();
	}
	
	private void ShowBackPropAnim()
    {
		CanvasGroup.blocksRaycasts = false;

		UnityUtility.InstantiateAsync("BackPropAnim", Prop_Parent, s =>
		{
			if(GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
				ShowPropBackToTileBoardAnim(s);
			else
				ShowPropBackToBackItemsAnim(s);
		});
	}

	private void ShowShufflePropAnim()
    {
		GameObject shufflePropAnim = null;
		CanvasGroup.blocksRaycasts = false;
		CallBack<GameObject> callBack = obj =>
		{
			var bgImage = obj.GetComponentInChildren<Image>(true);

			void animCompleteCallback()
			{
				GameManager.Sound.PlayAudio(SoundType.SFX_ShuffleTool.ToString());

				GameManager.Task.AddDelayTriggerTask(0.3f, () =>
				{
					CanvasGroup.blocksRaycasts = true;

					if (shufflePropAnim != null)
					{
						UnityUtility.UnloadInstance(shufflePropAnim);
						shufflePropAnim = null;
						GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
					}
				});
			};

			Transform fan = obj.transform.GetChild(1);
			fan.position = CenterRectTrans.position;
			fan.localScale = Vector3.zero;
			fan.gameObject.SetActive(true);
			bgImage.color = new Color(1, 1, 1, 0);
			bgImage.gameObject.SetActive(true);
			bgImage.DOFade(1, 0.2f);

			fan.DOScale(1.2f, 0.2f).onComplete = () =>
			{
				fan.DOScale(1f, 0.2f).onComplete = () =>
				{
					bgImage.DOFade(0, 0.2f);

					fan.DORotate(new Vector3(0,0,7200), 1.4f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetDelay(0.2f).onComplete = () =>
					{
						animCompleteCallback();
					};

					fan.DOLocalMove(new Vector3(-15, -35, 0), 0.2f).SetEase(Ease.Linear).SetDelay(0.9f).onComplete = () =>
					{
						fan.DOLocalJump(new Vector3(1171, 1068, 0), 0.2f, 1, 0.4f).SetEase(Ease.Linear);
						fan.DOScale(Vector3.zero, 0.4f).SetEase(Ease.Linear);
					};
				};
			};

			GameManager.Task.AddDelayTriggerTask(0.65f, () =>
			{
				ChangeTotalTilePos();
			});

			GameManager.Task.AddDelayTriggerTask(0.62f, () =>
			{
				GameManager.Sound.PlayAudio(SoundType.SFX_Boost_Boomerang.ToString());
			});

			SaveToJson();
		};

		UnityUtility.InstantiateAsync("ShufflePropAnim", Prop_Parent, (s) =>
		{
			shufflePropAnim = s;
			callBack(shufflePropAnim);
		});
	}

	private void ShowGrabPropAnim()
	{
		if (GetChooseTotalNum() <= 0)
		{
			return;
		}
		
		TileItem[] f1 = null;
		TileItem[] f2 = null;

		int num1 = 0;
		int num2 = 0;
		foreach (var child in chooseItemDict.Keys)
		{
			var listItems = chooseItemDict[child];
			int childNum = listItems.Count % 3;
			if (childNum >= num1)
			{
				num2 = num1;
				num1 = childNum;

				if (num2 != 0) 
					f2 = f1;
				
				var list = listItems.GetRange(listItems.Count - childNum, childNum);
				f1 = new TileItem[3];
				for (int i = 0; i < list.Count; i++)
				{
					f1[i] = list[i];
				}
			}
			else if (num2 == 0)
			{
				num2 = childNum;
				
				var list = listItems.GetRange(listItems.Count - childNum, childNum);
				f2 = new TileItem[3];
				for (int i = 0; i < list.Count; i++)
				{
					f2[i] = list[i];
				}
			}
		}

		isCheckGameLostOver = false;
		
		bool isTrigger = false;
		void FinishCallback()
		{
			if (isTrigger)
				return;
			isTrigger = true;

			foreach (var kvp in tileMapDict)
			{
				foreach ((int, TileItem) map in kvp.Value.Values)
				{
					if (map.Item1 != 17 && map.Item2 != null && map.Item2.AttachLogic != null)
					{
						map.Item2.AttachLogic.OnAnyTileGet();
					}
				}
			}
		}
		
		if (f1 != null)
		{
			//优先可点击棋子
			foreach (var layer in tileMapDict)
			{
				if (num1 >= 3) break;
				foreach (var map in layer.Value)
				{
					if (map.Value.Item2 != null &&
					    !map.Value.Item2.IsBeCover &&
					    map.Value.Item1 == f1[0].Data.TileID &&
					    !f1.Contains(map.Value.Item2))
					{
						f1[num1] = map.Value.Item2;
						num1 += 1;
						if (num1 >= 3) break;
					}
				}
			}

			foreach (var layer in tileMapDict)
			{
				if (num1 >= 3) break;
				foreach (var map in layer.Value)
				{
					if (map.Value.Item2 != null &&
					    map.Value.Item2.IsBeCover &&
					    map.Value.Item1 == f1[0].Data.TileID &&
					    !f1.Contains(map.Value.Item2))
					{
						f1[num1] = map.Value.Item2;
						num1 += 1;
						if (num1 >= 3) break;
					}
				}
			}

			//因为部分附属物逻辑与是否被遮盖有关，所以在遮盖逻辑之前执行
			foreach (var tile in f1)
			{
				if (tile == null) continue;
				if (chooseItemDict != null
				    && chooseItemDict.ContainsKey(tile.Data.TileID)
				    && chooseItemDict[tile.Data.TileID].Contains(tile)) continue;

				var tileData = tile.Data;
				tile.IsDestroyed = true;
				OnTileGet(tileData.Layer, tileData.MapID, TotalItemData.Prop_Absorb);
			}
		}

		if (f2 != null)
		{
			//优先可点击棋子
			foreach (var layer in tileMapDict)
			{
				if (num2 >= 3) break;
				foreach (var map in layer.Value)
				{
					if (map.Value.Item2 != null &&
					    !map.Value.Item2.IsBeCover &&
					    map.Value.Item1 == f2[0].Data.TileID &&
					    !f2.Contains(map.Value.Item2) &&
					    !f1.Contains(map.Value.Item2)) 
					{
						f2[num2] = map.Value.Item2;
						num2 += 1;
						if (num2 >= 3) break;
					}
				}
			}

			foreach (var layer in tileMapDict)
			{
				if (num2 >= 3) break;
				foreach (var map in layer.Value)
				{
					if (map.Value.Item2 != null &&
					    map.Value.Item2.IsBeCover &&
					    map.Value.Item1 == f2[0].Data.TileID &&
					    !f2.Contains(map.Value.Item2) &&
					    !f1.Contains(map.Value.Item2)) 
					{
						f2[num2] = map.Value.Item2;
						num2 += 1;
						if (num2 >= 3) break;
					}
				}
			}

			//因为部分附属物逻辑与是否被遮盖有关，所以在遮盖逻辑之前执行
			foreach (var tile in f2)
			{
				if (tile == null) continue;
				if (chooseItemDict != null
				    && chooseItemDict.ContainsKey(tile.Data.TileID)
				    && chooseItemDict[tile.Data.TileID].Contains(tile)) continue;

				var tileData = tile.Data;
				tile.IsDestroyed = true;
				OnTileGet(tileData.Layer, tileData.MapID, TotalItemData.Prop_Absorb);
			}
		}
		
		//先进行f1花色消除
		foreach (var tile in f1)
		{
			if (tile == null) continue;
			if (chooseItemDict != null
			    && chooseItemDict.ContainsKey(tile.Data.TileID)
			    && chooseItemDict[tile.Data.TileID].Contains(tile)) continue;
				
			var tileData = tile.Data;
			bool isBeCover = tile.IsBeCover;

			CallBack animCenterCallBack = () =>
			{
				if (tileData.CoverIndexs != null)
				{
					foreach (var child in tileData.CoverIndexs)
					{
						foreach (var index1 in child.Value)
						{
							tileMapDict[child.Key][index1].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
							OnCoverStateChange(tileMapDict[child.Key][index1].Item2);
						}
					}
				}

				if (tileData.BeCoverIndexs != null)
				{
					foreach (var child in tileData.BeCoverIndexs)
					{
						foreach (var index1 in child.Value)
						{
							tileMapDict[child.Key][index1].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
						}
					}
				}

				tile.Data.CoverIndexs = null;
				tile.Data.BeCoverIndexs = null;
			};
			CallBack finishAction = () =>
			{
				OnTileClick(tile, TotalItemData.Prop_Absorb, false, isCheckLose: false);

				if (f2 == null) 
					FinishCallback();
			};

			Transform cacheTrans = tile.transform;
			cacheTrans.SetAsLastSibling();
			tile.AttachLogic?.SpecialCollect(!isBeCover);
			//tile.PlayPropAbsorbAnim(true, animCenterCallBack, finishAction);
			cacheTrans.DOKill();
			cacheTrans.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack);
			tile.SetUncoverState();
			GameManager.Task.AddDelayTriggerTask(0.3f, () =>
			{
				animCenterCallBack();
				finishAction();
			});
		}

		//再进行f2花色消除
		if (f2 != null)
		{
			foreach (var tile in f2)
			{
				if (tile == null) continue;
				if (chooseItemDict != null
				    && chooseItemDict.ContainsKey(tile.Data.TileID)
				    && chooseItemDict[tile.Data.TileID].Contains(tile)) continue;
				
				var tileData = tile.Data;
				bool isBeCover = tile.IsBeCover;

				CallBack animCenterCallBack = () =>
				{
					if (tileData.CoverIndexs != null)
					{
						foreach (var child in tileData.CoverIndexs)
						{
							foreach (var index1 in child.Value)
							{
								tileMapDict[child.Key][index1].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
								OnCoverStateChange(tileMapDict[child.Key][index1].Item2);
							}
						}
					}

					if (tileData.BeCoverIndexs != null)
					{
						foreach (var child in tileData.BeCoverIndexs)
						{
							foreach (var index1 in child.Value)
							{
								tileMapDict[child.Key][index1].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
							}
						}
					}

					tile.Data.CoverIndexs = null;
					tile.Data.BeCoverIndexs = null;
				};
				CallBack finishAction = () =>
				{
					OnTileClick(tile, TotalItemData.Prop_Absorb, false, isCheckLose: false);

					FinishCallback();
				};

				Transform cacheTrans = tile.transform;
				cacheTrans.SetAsLastSibling();
				tile.AttachLogic?.SpecialCollect(!isBeCover);
				//tile.PlayPropAbsorbAnim(true, animCenterCallBack, finishAction);
				cacheTrans.DOKill();
				cacheTrans.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack);
				tile.SetUncoverState();
				GameManager.Task.AddDelayTriggerTask(0.7f, () =>
				{
					animCenterCallBack();
					finishAction();
				});
			}
		}
		
		isCheckGameLostOver = true;
	}
	
	private void ShowPropAnim(TotalItemData skillType, CallBack animCallBack, float delayTime = 2)
	{
		GameObject skillAnim = null;
		CanvasGroup.blocksRaycasts = false;

		CallBack<GameObject> callBack = obj =>
		{
			var boneFollower = obj.GetComponentInChildren<BoneFollowerGraphic>(true);
			var s = obj.GetComponentInChildren<SkeletonGraphic>(true);
			s.gameObject.SetActive(true);
			Spine.AnimationState.TrackEntryDelegate trackEntry = t =>
			{
				try
				{
                    s.gameObject.SetActive(false);
                    var image = obj.transform.parent.GetComponentInChildren<Image>(true);
                    image.DOFade(0, 0.1f).OnComplete(() =>
                    {
                        image.gameObject.SetActive(false);

                        animCallBack?.Invoke();
                        GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                        {
                            CanvasGroup.blocksRaycasts = true;

							if (skillAnim != null)
							{
								UnityUtility.UnloadInstance(skillAnim);
								skillAnim = null;
							}
							GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
						});
                    });
				}
				catch(Exception e)
				{
					Debug.LogError("ShowPropAnim callback error:" + e.Message);
                    animCallBack?.Invoke();
                    var image = obj.transform.parent.GetComponentInChildren<Image>(true);
                    image.gameObject.SetActive(false);
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
                }
			};

			var animName = GetSkillSkeletonAnimName(skillType);
			s.AnimationState.SetAnimation(0, animName, false).Complete -= trackEntry;
			s.AnimationState.SetAnimation(0, animName, false).Complete += trackEntry;
			// animCallBack?.Invoke();

			var bgImage = obj.GetComponentInChildren<Image>(true);
			bgImage.color = new Color(1, 1, 1, 0);
			bgImage.gameObject.SetActive(true);
			bgImage.DOFade(1, 0.2f);

			boneFollower.gameObject.SetActive(false);
			boneFollower.gameObject.SetActive(skillType == TotalItemData.Prop_Absorb);
		};

		UnityUtility.InstantiateAsync("PropAnim", Prop_Parent, (s) =>
		{
			skillAnim = s;
			callBack(skillAnim);
		});
	}

	private string GetSkillSkeletonAnimName(TotalItemData type)
	{
		if (type == TotalItemData.Prop_Grab)
		{
			return "01";
		}
		else if (type == TotalItemData.Prop_ChangePos)
		{
			return "02";
		}
		else if (type == TotalItemData.Prop_Back)
		{
			return "03";
		}
		else if (type == TotalItemData.Prop_Absorb)
		{
			return "04";
		}
		else
		{
			return "01";
		}
	}

	private bool IsUnLock(TotalItemData type, out int level)
	{
		if (type == TotalItemData.Prop_Back)
		{
			level = Prop_Back_UnlockLevel;
		}
		else if (type == TotalItemData.Prop_ChangePos)
		{
			level = Prop_ChangePos_UnlockLevel;
		}
		else if (type == TotalItemData.Prop_Absorb)
		{
			level = Prop_Absorb_UnlockLevel;
		}
		else if (type == TotalItemData.Prop_Grab)
		{
			level = Prop_Grab_UnlockLevel;
		}
		else if (type == TotalItemData.Prop_AddOneStep)
		{
			level = 1;
		}
		else
		{
			level = 1;
		}

		return GameManager.PlayerData.NowLevel >= level &&
			   GameManager.PlayerData.IsSkillUnlock(type.TotalItemType);
	}

	private void ShowTileMatchMap(
		Dictionary<int, Dictionary<int, TileInfo>> mapData,
		Dictionary<int, Dictionary<int, TileMoveDirectionType>> recordRandomMoveDict)
	{
        //主线关打点
        if(panelData.LevelNum==GameManager.PlayerData.NowLevel)
        	GameManager.Firebase.RecordMessageByEvent("Level_Start_Total",new Parameter("Level",GameManager.PlayerData.NowLevel));
		isLoadAllTileComplete = false;
		var isShowTileAnim = IsShowTileAnim();
		if (isShowTileAnim && !panelData.IsOldLevelData)
		{
			GameManager.Task.AddDelayTriggerTask(0.6f, () =>
			{
				GameManager.Sound.PlayAudio(SoundType.TileMatchOpen.ToString());
			});
		}

		TileMatchParent.transform.localScale = Vector3.one * GetTileScale(TileMatchPosType.Map);
		tileMapDict.Clear();
		int totalIndex = 0;
		if (mapData != null)
        {
	        List<int> layerList = new List<int>(mapData.Count);
	        foreach (var key in mapData.Keys)
	        {
		        layerList.Add(key);
	        }
	        layerList.Sort();

			int totalCount = 0;
			int curCount = 0;
			foreach (var layerMap in mapData)
            {
				if (layerMap.Value != null)
                {
					foreach (var map in layerMap.Value)
					{
						totalCount++;
					}
				}
			}

			foreach (var layerMap in mapData)
			{
				int layer = layerMap.Key;
				tileMapDict.Add(layer, new SortedDictionary<int, (int, TileItem)>());
				if (layerMap.Value != null)
					foreach (var map in layerMap.Value)
					{
						int mapIndex = map.Key;
						int tileID = map.Value.TileID;
						int attachID = map.Value.AttachID;
						TileMoveDirectionType moveIndex = (TileMoveDirectionType)map.Value.DirectionType;
						if (recordRandomMoveDict != null &&
							recordRandomMoveDict.ContainsKey(layer) &&
							recordRandomMoveDict[layer].ContainsKey(mapIndex))
						{
							moveIndex = recordRandomMoveDict[layer][mapIndex];
						}

						var targetPos = TileMatchUtil.GetTilePos(layer, mapIndex, moveIndex, recordRandomMoveDict);

						System.Action<TileItem> callBack = (tileItem) =>
						{
							curCount++;
							if (!tileMapDict[layer].ContainsKey(mapIndex)) 
                            {
								var dict = TileMatchUtil.GetLinkMaskByLayer(layer, mapIndex, mapData);
								tileItem.Init(layer, mapIndex, tileID, attachID, 1, moveIndex, targetPos, dict.Item1, dict.Item2, SetTileBtnEvent, isShowTileAnim && !panelData.IsOldLevelData);
								tileMapDict[layer].Add(mapIndex, (tileID, tileItem));
							}

							if (curCount == totalCount)
							{
                                try
                                {
									foreach (int curLayer in layerList)
									{
										if (tileMapDict.ContainsKey(curLayer))
											foreach (var newMap in tileMapDict[curLayer].Values)
											{
												totalIndex += 1;
												newMap.Item2.transform.SetSiblingIndex(totalIndex);
											}
									}
								}
								catch(Exception e)
                                {
									Log.Error("tileMapDict SetSiblingIndex error:{0}", e.Message);
                                }

								isLoadAllTileComplete = true;
							}
						};
						SpawnTile(tileID, callBack);
					}
			}
		}
	}

	private bool isLoadAllTileComplete;
    public override bool CheckInitComplete()
    {
        return isLoadAllTileComplete;
    }

    private void ShowBoost(float delayTime)
    {
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			return;
		
		List<TotalItemType> boostList = new List<TotalItemType>();

		TotalItemType[] list = new TotalItemType[3] { TotalItemType.MagnifierBoost, TotalItemType.Prop_AddOneStep, TotalItemType.FireworkBoost };
        for (int i = 0; i < list.Length; i++)
        {
			TotalItemType boostType = list[i];
			bool isSelected = GameManager.DataNode.GetData<bool>("BoostIsSelected_" + boostType.ToString(), false);
			if (isSelected)
			{
				if (GameManager.PlayerData.GetInfiniteBoostTime(boostType) <= 0) 
					GameManager.PlayerData.UseItem(TotalItemData.FromInt((int)boostType), 1);

				//选中booster，之后一直为选中状态，除非booster数量为0或者再次点击booster取消选中状态
				if (GameManager.PlayerData.GetItemNum(TotalItemData.FromInt((int)boostType)) <= 0)
					GameManager.DataNode.SetData<bool>("BoostIsSelected_" + boostType.ToString(), false);

				boostList.Add(boostType);

				GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Booster_Use, new Parameter("Level", GameManager.PlayerData.NowLevel),
					new Parameter("boosterID", (long)boostType));
			}
		}
        
		if (GameManager.DataNode.GetData<bool>("UsedAdsBoost", false))
		{
			GameManager.DataNode.SetData<bool>("UsedAdsBoost", false);
			
			TotalItemType adBoostType;
			if (boostList.Contains(TotalItemType.Prop_AddOneStep))
				adBoostType = TotalItemType.FireworkBoost;
			else
				adBoostType = willTriggerAddOneStep || Random.Range(0, 2) >= 1 ? TotalItemType.Prop_AddOneStep : TotalItemType.FireworkBoost;

			if (adBoostType == TotalItemType.Prop_AddOneStep) 
				willTriggerAddOneStep = true;

			CanvasGroup.blocksRaycasts = false;
			GameManager.Task.AddDelayTriggerTask(delayTime, () =>
			{
				GameManager.UI.ShowUIForm("AdShowBoostPanel",UIFormType.PopupUI, f =>
				{
					CanvasGroup.blocksRaycasts = true;
					f.m_OnHideCompleteAction = () =>
					{
						if (boostList.Count > 0)
						{
							GameManager.UI.ShowUIForm("GameShowBoostPanel",UIFormType.PopupUI, null, null, boostList);
						}
					};
				}, () => { }, adBoostType);
			});
		}
		else
		{
			if (boostList.Count > 0)
			{
				if (boostList.Contains(TotalItemType.Prop_AddOneStep))
					willTriggerAddOneStep = true;

				CanvasGroup.blocksRaycasts = false;
				GameManager.Task.AddDelayTriggerTask(delayTime, () =>
				{
					GameManager.UI.ShowUIForm("GameShowBoostPanel",UIFormType.PopupUI,f =>
					{
						CanvasGroup.blocksRaycasts = true;
					}, () =>
					{
						CanvasGroup.blocksRaycasts = true;
					}, boostList);
				});
			}
		}
	}

	public Vector3 GetChoosePos(int index)
	{
		if (TileMatchPositions == null) 
			TileMatchPositions = TileMatchPositions_7;

		return (TileMatchPositions[1].position - TileMatchPositions[0].position) * index + TileMatchPositions[0].position;
	}

	public void SpawnTile(int tileId, System.Action<TileItem> finishAction)
	{
		string tilePrefabName = TileMatchUtil.GetTilePrefabName(tileId);

		GameManager.ObjectPool.Spawn<EffectObject>(
			tilePrefabName, TileItemPrefabPool, Vector3.one, Quaternion.identity, TileMatchParent, (obj) =>
					 {
						 GameObject target = (GameObject)obj.Target;
						 target.SetActive(true);
						 target.transform.DOKill();
						 target.transform.localScale = Vector3.one;
						 finishAction?.Invoke(target.GetComponent<TileItem>());
					 });
	}

	public void UnSpawnTile(GameObject tileObj)
	{
		if (tileObj == null)
			return;
		GameManager.ObjectPool.Unspawn<EffectObject>(TileItemPrefabPool, tileObj);
	}

	private void SetTileBtnEvent(TileItem tile)
	{
		if (tile.Data.TileID == 20)
		{
			OnTileFireworkClick(tile);
		}
		else
		{
			ClickTileItemEvent(tile, () =>
			{
				OnTileClick(tile, TotalItemData.None, true);
			});
		}
	}

	public bool isCheckGameLostOver = true;

	private (TotalItemData, bool) skillFree = (TotalItemData.Prop_Back, false);
	private void OnTileClick(TileItem tileItem, TotalItemData type, bool isPlayAudio = true, bool isHaveAnim = true,bool isCheckLose=true)
	{
		if (tileItem == null || (type != TotalItemData.Prop_Absorb && tileItem.IsDestroyed))
			return;

		var tileData = tileItem.Data;
		int curDictNum = GetChooseTotalNum();
		if (isCheckLose && isCheckGameLostOver && curDictNum >= panelData.TileCellNum)
		{
			return;
		}
		else
		{
			tileItem.SetClickState(false);
			tileItem.transform.SetAsLastSibling();
			int tileID = tileData.TileID;
			if (backTileDict.ContainsKey(tileItem)) backTileDict.Remove(tileItem);
			else
			{
				if (!tileMapDict.ContainsKey(tileData.Layer))
					return;
				tileMapDict[tileData.Layer].Remove(tileData.MapID);
				if (tileMapDict[tileData.Layer].Count <= 0)
					tileMapDict.Remove(tileData.Layer);
			}

			if (!chooseItemDict.ContainsKey(tileID))
				chooseItemDict.Add(tileID, new List<TileItem>());
			chooseItemDict[tileID].Add(tileItem);
			OnChooseItemDictChange();

			tileItem.transform.SetParent(ChooseParent, true);

			SaveToJson();

			if(type != TotalItemData.Prop_Absorb)
            {
				foreach (var kvp in tileMapDict)
				{
					foreach ((int, TileItem) map in kvp.Value.Values)
					{
						if (map.Item2 != null && map.Item2.AttachLogic != null)
						{
							map.Item2.AttachLogic.OnAnyTileGet();
						}
					}
				}
			}

			if (tileData.CoverIndexs != null)
				foreach (var child in tileData.CoverIndexs)
				{
					foreach (var index in child.Value)
					{
						tileMapDict[child.Key][index].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
						OnCoverStateChange(tileMapDict[child.Key][index].Item2);
					}
				}
			if (tileData.BeCoverIndexs != null)
			{
				foreach (var child in tileData.BeCoverIndexs)
				{
					foreach (var index in child.Value)
					{
						tileMapDict[child.Key][index].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
					}
				}
			}

			//记录点击的棋子数据
			if (type != TotalItemData.Prop_Absorb)
			{
				tileItem.ClickInfo = TileClickInfo.Create(tileData.Layer, tileData.MapID, tileData.MoveIndex,
					tileData.CoverIndexs,
					tileData.BeCoverIndexs);
			}
			
			RecordChooseTileMove(tileItem, isHaveAnim);
			if (Random.Range(0, 2) == 0) 
				GameManager.Sound.PlayAudio(SoundType.SFX_ClickTile_new.ToString());
			else
				GameManager.Sound.PlayAudio(SoundType.SFX_ClickTile_pop.ToString());

			tileItem.Init(0, 0, tileData.TileID, 0, TileMoveDirectionType.None, null, null, null);//���³�ʼ��
			tileItem.SetClickState(false);

			if (isCheckLose) CheckLose();

			RecordClickTileCount();
		}

		if (type != TotalItemData.Prop_Absorb)
		{
			OnTileGet(tileData.Layer, tileData.MapID, type);
		}
	}

	private void DestroyTile(TileItem tileItem, bool isPlayAudio = true, bool isHaveAnim = true, bool isCheckLose = true, bool includeChooseItem = false)
    {
		var tileData = tileItem.Data;
		int curDictNum = GetChooseTotalNum();
		if (isCheckLose && isCheckGameLostOver && curDictNum >= panelData.TileCellNum) return;

		int tileId = tileData.TileID;
		if (includeChooseItem && chooseItemDict.ContainsKey(tileId) && chooseItemDict[tileId] != null && chooseItemDict[tileId].Contains(tileItem)) 
        {
			tileItem.DestroyTile(isPlayAudio);
			chooseItemDict[tileId].Remove(tileItem);
			OnChooseItemDictChange();
			SaveToJson();

            if (!CheckWin())
            {
				GameManager.Task.AddDelayTriggerTask(0.02f, () =>
				{
					RecordChooseTileMove(null);
				});
			}

			return;
		}

		tileItem.SetClickState(false);
		tileItem.transform.SetAsLastSibling();
		int tileID = tileData.TileID;
		if (backTileDict.ContainsKey(tileItem)) backTileDict.Remove(tileItem);
		else
		{
			tileMapDict[tileData.Layer].Remove(tileData.MapID);
			if (tileMapDict[tileData.Layer].Count <= 0)
				tileMapDict.Remove(tileData.Layer);
		}

		SaveToJson();

		if (tileData.CoverIndexs != null)
			foreach (var child in tileData.CoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
					OnCoverStateChange(tileMapDict[child.Key][index].Item2);
				}
			}
		if (tileData.BeCoverIndexs != null)
		{
			foreach (var child in tileData.BeCoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
				}
			}
		}

		tileItem.DestroyTile(isPlayAudio);

		if (isCheckLose) CheckLose();

		CheckWin();
	}

	private void OnChooseItemDictChange()
    {
		//触发抓取道具弱教程
		if (GameManager.PlayerData.NowLevel == Prop_Grab_UnlockLevel && skillFree.Item1 == TotalItemData.Prop_Grab && skillFree.Item2) 
        {
			if (GetChooseTotalNum() >= 3)
			{
				CommonGuideMenuUtil.ShowWeakSkillGuide(GuideType.Skill_Grab);
			}
            else
            {
				CommonGuideMenuUtil.HideWeakSkillGuide(GuideType.Skill_Grab);
			}
		}
    }

    #region Tile_Firework

    private void OnTileFireworkClick(TileItem tileItem)
    {
		if (isGameWin || isGameLose) return;

		var tileData = tileItem.Data;
		int curDictNum = GetChooseTotalNum();
		if (curDictNum >= panelData.TileCellNum) return;

		tileItem.SetClickState(false);
		tileItem.transform.SetAsLastSibling();

		if (!tileMapDict.ContainsKey(tileData.Layer))
			return;
		tileMapDict[tileData.Layer].Remove(tileData.MapID);
		if (tileMapDict[tileData.Layer].Count <= 0)
			tileMapDict.Remove(tileData.Layer);

		//如果是金块关，金块剥落
		//bool isGoldTileLevel = DTLevelUtil.IsSpecialGoldTile(panelData.LevelNum);

		foreach (var kvp in tileMapDict)
		{
			foreach ((int, TileItem) map in kvp.Value.Values)
			{
				if(map.Item2 != null && map.Item2.AttachLogic != null)
                {
					map.Item2.AttachLogic.OnAnyTileGet();
				}
			}
		}

		if (tileData.CoverIndexs != null)
			foreach (var child in tileData.CoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
					OnCoverStateChange(tileMapDict[child.Key][index].Item2);
				}
			}
		if (tileData.BeCoverIndexs != null)
		{
			foreach (var child in tileData.BeCoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
				}
			}
		}

		GameManager.Sound.PlayAudio(SoundType.SFX_Prop_Bomb.ToString());

		Vector3 pos = tileItem.transform.position;
		float deltaY = CenterRectTrans.position.y - pos.y;
		OnTileGet(tileData.Layer, tileData.MapID, TotalItemData.None);
		tileItem.DestroyTile();

		var targetTiles = GetFireworkTargetTile();
		if (targetTiles != null && targetTiles.Count == 3) 
        {
            for (int i = 0; i < targetTiles.Count; i++)
            {
				targetTiles[i].SetClickState(false);
				targetTiles[i].IsDestroyed = true;
			}

			GameObject animObj = AddressableUtils.LoadAsset<GameObject>("TileAnim_Firework");
			GameObject animInstance = Instantiate(animObj, pos, Quaternion.identity, Prop_Parent);
			TileAnim_Firework anim = animInstance.GetComponent<TileAnim_Firework>();

			anim.Clear();
			anim.m_EndAction = () =>
			{
				SaveToJson();

				GameManager.Task.AddDelayTriggerTask(2f, () =>
				{
					Destroy(animInstance);
					UnityEngine.AddressableAssets.Addressables.Release(animObj);
				});
			};
			anim.ShowAnim(targetTiles, deltaY, t =>
			{
				TriggerFireworkTileEffect(t);
			});
		}
	}

	private List<TileItem> GetTargetTileList(Dictionary<int, List<TileItem>> dict, int includeCoveredNum, bool includeAttached, bool includeNotMatch)
    {
		foreach (KeyValuePair<int, List<TileItem>> pair in dict)
		{
			if (includeNotMatch || pair.Value.Count >= 3) 
			{
				int coveredNum = 0;
				List<TileItem> res = new List<TileItem>();
				for (int i = 0; i < pair.Value.Count; i++)
				{
					if ((includeCoveredNum > coveredNum || !pair.Value[i].IsBeCover)
						&& (includeAttached || pair.Value[i].Data.AttachID == 0)) 
					{
						if (pair.Value[i].IsBeCover)
							coveredNum++;

						res.Add(pair.Value[i]);
						if (res.Count == 3)
							return res;
					}
				}

				if (includeNotMatch && chooseItemDict.ContainsKey(pair.Key))
				{
					List<TileItem> listItems = chooseItemDict[pair.Key];
					if (listItems != null)
					{
						int childNum = listItems.Count % 3;
						if (childNum >= (3 - res.Count))
						{
							res.AddRange(listItems.GetRange(listItems.Count - childNum, childNum));
						}

						if (res.Count == 3)
							return res;
					}
				}
			}
		}

		return null;
	}

	public List<TileItem> GetFireworkTargetTile(bool includeBackTile = true, bool includeGoldTile = true)
    {
		Dictionary<int, List<TileItem>> dict = new Dictionary<int, List<TileItem>>();
		var list = new List<KeyValuePair<int, SortedDictionary<int, (int, TileItem)>>>();
		foreach (KeyValuePair<int, SortedDictionary<int, (int, TileItem)>> layer in tileMapDict)
		{
			list.Add(layer);
		}

		list.Sort((a, b) => a.Key < b.Key ? 1 : -1);
        foreach (var pair in list)
        {
			foreach (var map in pair.Value.Values)
			{
				if (TileMatchUtil.IsSpecial(map.Item1) || map.Item2 == null || map.Item2.IsDestroyed || !map.Item2.IsCanClick) 
					continue;

				if (!dict.ContainsKey(map.Item1))
					dict[map.Item1] = new List<TileItem>();
				dict[map.Item1].Add(map.Item2);
			}
		}

        if (includeBackTile)
        {
			foreach (KeyValuePair<TileItem, int> item in backTileDict)
			{
				if (item.Key.IsDestroyed)
					continue;

				if (!dict.ContainsKey(item.Key.Data.TileID))
					dict[item.Key.Data.TileID] = new List<TileItem>();
				dict[item.Key.Data.TileID].Add(item.Key);
			}
		}

		if (!includeGoldTile)
		{
			dict.Remove(17);
		}

		//      List<TileItem> res = GetTargetTileList(dict, false, false, false); if (res != null) return res;
		//res = GetTargetTileList(dict, true, false, false); if (res != null) return res;
		//res = GetTargetTileList(dict, false, true, false); if (res != null) return res;
		//res = GetTargetTileList(dict, true, true, false); if (res != null) return res;
		//res = GetTargetTileList(dict, false, true, true); if (res != null) return res;
		//res = GetTargetTileList(dict, true, true, true); if (res != null) return res;

		List<TileItem> res = GetTargetTileList(dict, 0, true, false); if (res != null) return res;
		res = GetTargetTileList(dict, 1, true, false); if (res != null) return res;
		res = GetTargetTileList(dict, 2, true, false); if (res != null) return res;
		res = GetTargetTileList(dict, 0, true, true); if (res != null) return res;
		res = GetTargetTileList(dict, 3, true, true); if (res != null) return res;

		return res;
	}

	private bool TriggerFireworkTileEffect(TileItem tile)
	{
		//因为部分附属物逻辑与是否被遮盖有关，所以在遮盖逻辑之前执行
		var tileData = tile.Data;
		tile.IsDestroyed = true;
		OnTileGet(tileData.Layer, tileData.MapID, TotalItemData.None);
		bool isBeCover = tile.IsBeCover;

		CallBack animCenterCallBack = () =>
		{
			if (tileData.CoverIndexs != null)
			{
				foreach (var child in tileData.CoverIndexs)
				{
					foreach (var index1 in child.Value)
					{
						tileMapDict[child.Key][index1].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
						OnCoverStateChange(tileMapDict[child.Key][index1].Item2);
					}
				}
			}
			if (tileData.BeCoverIndexs != null)
			{
				foreach (var child in tileData.BeCoverIndexs)
				{
					foreach (var index1 in child.Value)
					{
						tileMapDict[child.Key][index1].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
					}
				}
			}
			tile.Data.CoverIndexs = null;
			tile.Data.BeCoverIndexs = null;
		};
        void finishAction() => DestroyTile(tile, true, isCheckLose: false, includeChooseItem: true);

        tile.transform.SetAsLastSibling();
        //////////////
        tile.AttachLogic?.SpecialCollect(!isBeCover);
		tile.SetUncoverState();
		animCenterCallBack();

		tile.transform.DOShakePosition(0.2f, 10);
		tile.transform.DOScale(new Vector3(1.3f, 1.3f), 0.2f);
		GameManager.Task.AddDelayTriggerTask(0.2f, finishAction);

		return true;
	}

    #endregion

    private void OnTileGet(int layer, int mapid, TotalItemData type)
    {
        //周围的tile的附属物触发OnAroundTileGet
        int row = mapid / 16;
		int col = mapid % 16;
		//上，上右，右上，右，右下，下右，下，下左，左下，左，左上，上左
		int[] rows = new int[] { 2, 2, 1, 0, -1, -2, -2, -2, -1, 0, 1, 2 };
		int[] cols = new int[] { 0, 1, 2, 2, 2, 1, 0, -1, -2, -2, -2, -1 };
		if (tileMapDict.TryGetValue(layer, out SortedDictionary<int, (int, TileItem)> dict))
		{
			for (int i = 0; i < rows.Length; i++)
			{
				int aroundRow = row + rows[i];
				int aroundCol = col + cols[i];
				if (aroundCol < 0 || aroundCol >= 16) 
					continue;

				int aroundMapIndex = aroundRow * 16 + aroundCol;
				if (dict.TryGetValue(aroundMapIndex, out (int, TileItem) map))
				{
					if (map.Item2 != null && map.Item2.AttachLogic != null)
					{
						map.Item2.AttachLogic.OnAroundTileGet(type);
					}
				}
			}
		}
	}

	private bool isGameWin = false;
	public bool IsGameWin => isGameWin;

	public bool CheckWin(bool isForce = false)
	{
		if (!isForce && (GetMapTotalNum() > 0 || backTileDict.Count > 0 || GetChooseTotalNum(false) > 0)) return false;
		if (isGameWin) return true;
		isGameWin = true;
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			GameManager.Task.CalendarChallengeManager.CalendarChallengeWin();
			timeBar.PauseClock();
			GameManager.UI.ShowUIForm("CalendarChallengeWin",UIFormType.PopupUI,f =>
			{
				GameManager.Task.AddDelayTriggerTask(0.2f, () =>
				{
					GameManager.UI.HideUIForm(this);
				});
			});
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Level_Win, new Parameter("DailyLevel", panelData.LevelNum));
			//if (GameManager.PlayerData.GetInfiniteLifeTime() <= 0)
			//	GameManager.PlayerData.AddItemNum(TotalItemData.Life, 1);
			return true;
		}
		
		LevelModel.Instance.ClearEnterLevelCountByWin();
		
		GameManager.Firebase.RecordMessageByEvent("Level_Pass_Time",
	new Parameter("Level", GameManager.PlayerData.NowLevel),
	new Parameter("UseTime", (int)Time.realtimeSinceStartup - panelData.RecordStartGameTime));
		GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Win,
			new Parameter("Level", GameManager.PlayerData.NowLevel));
		//if (GameManager.PlayerData.NowLevel % 5 == 0 && GameManager.PlayerData.NowLevel < 2500)
		//	GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Level_Win}_{GameManager.PlayerData.NowLevel}");
		if (GameManager.PlayerData.AFPassLevel % 5 == 0 && GameManager.PlayerData.AFPassLevel >= 5 && GameManager.PlayerData.AFPassLevel <= 100)
			GameManager.AppsFlyer.SendGameWinEvent();
		GameManager.DataNode.SetData<int>("ContinueLevelCount", 0);

        if (GameManager.PlayerData.NowLevel <= 100)
        {
            if (GameManager.PlayerData.NowLevel % 5 == 0)
                GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Level_Win}_{GameManager.PlayerData.NowLevel}");
        }
        else if (GameManager.PlayerData.NowLevel <= 300)
        {
            if (GameManager.PlayerData.NowLevel % 10 == 0)
                GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Level_Win}_{GameManager.PlayerData.NowLevel}");
        }
        else if (GameManager.PlayerData.NowLevel <= 2000)
        {
            if (GameManager.PlayerData.NowLevel % 100 == 0)
                GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Level_Win}_{GameManager.PlayerData.NowLevel}");
        }
        else if (GameManager.PlayerData.NowLevel <= 10000)
        {
            if (GameManager.PlayerData.NowLevel % 1000 == 0)
                GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Level_Win}_{GameManager.PlayerData.NowLevel}");
        }

        int nowLevel = GameManager.PlayerData.NowLevel;
		if ((nowLevel >= 50 && nowLevel <= 200 && nowLevel % 50 == 0) ||
			(nowLevel > 200 && nowLevel <= 1000 && nowLevel % 100 == 0))
		{
			GameManager.PlayerData.RecordPlayerItemDataByLevel();
		}

		//piggybank
		PiggyBankModel.Instance.RecordCoinByLevelWin(nowLevel);
		
		//如果是金块关，记录金块收集数
		if (DTLevelUtil.IsSpecialGoldTile(GameManager.PlayerData.RealLevel()))
		{
			int goldCount = GameManager.DataNode.GetData("GoldTileCurrentCount", 0);
			GameManager.DataNode.RemoveNode("GoldTileCurrentCount");
			GameManager.Task.GoldCollectionTaskManager.LevelCollectNum = goldCount;
			GameManager.Task.GoldCollectionTaskManager.TotalCollectNum += goldCount;
			Log.Info("玩家当前金块收集总数：" + GameManager.Task.GoldCollectionTaskManager.TotalCollectNum);
		}

		//如果是油桶关，记录油桶收集数
		int gasolineCount = panelData.TileMatchData.GasolineCount;
		TilePassModel.Instance.LevelCollectTargetNum = gasolineCount;

		//气球比赛
		//stage开启
		if (DateTime.Now < GameManager.Task.BalloonRiseManager.StageEndTime && !GameManager.Network.CheckInternetIsNotReachable()) 
		{
			GameManager.Task.BalloonRiseManager.WinningStreakTimes += 1;
			// if (GameManager.Task.BalloonRiseManager.Score < GameManager.Task.BalloonRiseManager.WinningStreakTimes)
			{
				GameManager.Task.BalloonRiseManager.Score = GameManager.Task.BalloonRiseManager.WinningStreakTimes;
				GameManager.Task.BalloonRiseManager.ScoreChange = true;
				if (GameManager.Task.BalloonRiseManager.Score >= GameManager.Task.BalloonRiseManager.StageTarget)
					GameManager.Task.BalloonRiseManager.CheckIsFinishStage = true;
			}
		}
		
		GameManager.Sound.StopMusic(0.1f);
        DaliyWatchAds_Prefab.SetActive(false);
        //GameManager.Task.AddDelayTriggerTask(2.4f, () =>
        //{
        //	//播放背景音乐
        //	GameManager.Sound.PlayMusic(GameManager.PlayerData.HappyBgMusicName);
        //});

        GameManager.PlayerData.RecordPlayerLevelWinBehavior();
		GameManager.PlayerData.RecordPlayerLevelWinByDaliy_Rate(GameManager.PlayerData.NowLevel);
		if (GameManager.DataNode.GetData<bool>("UseLife", false))
        {
			GameManager.PlayerData.AddItemNum(TotalItemData.Life, 1);
			GameManager.DataNode.SetData<bool>("UseLife", false);
		}

		//GameManager.DataNode.RemoveNode("levelData" + GameManager.PlayerData.RealLevel().ToString());
		if (panelData.LevelNum == GameManager.PlayerData.NowLevel)
		{
			GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Pass_Levels, 1);
			GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Clear_Tiles, panelData.TileMatchData.TotalCount);
			GameManager.PlayerData.WinLastGame = true;
			GameManager.PlayerData.NowLevel += 1;
			GameManager.Task.AddTargetCollection(TaskTarget.LevelPass, 1);
			GameManager.PlayerData.AFPassLevel += 1;
		}

		int level = GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel - 1);
		int levelType = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelModelType(level);
        LevelType levelData = GameManager.DataTable.GetDataTable<DTLevelTypeID>().Data.GetLevelTypeData(levelType);
        if (levelData != null)
        {
			int levelWinCoin = PlayerPrefs.GetInt("LevelWinCoin", 0);
			int levelWinStar = PlayerPrefs.GetInt("LevelWinStar", 0);
			if (levelWinCoin < 0) levelWinCoin = 0;
			if (levelWinStar < 0) levelWinStar = 0;

			PlayerPrefs.SetInt("LevelWinCoin", levelWinCoin + levelData.CoinNum);

			int levelFailTime = PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + level, 0);
			int getStarNum = 1;
			int hardIndex = DTLevelUtil.GetLevelHard(level);
			if (hardIndex > 0)
			{
				if (hardIndex == 1)
					getStarNum = 2;
				else if (hardIndex == 2)
					getStarNum = 3;
			}
			PlayerPrefs.SetInt("LevelWinStar", levelWinStar + getStarNum);

			GameManager.Activity.OnLevelWin(levelFailTime, hardIndex);

			if (levelFailTime == 0) 
				GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Pass_First_Time, new Parameter("Level", GameManager.PlayerData.NowLevel));
		}
		GameManager.Task.ConfirmTargetCollection();
		GameManager.Task.AddDelayTriggerTask(0.4f, WinAnim);

		if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			GameManager.DataNode.SetData<int>("FailCount", 0);
		else
			GameManager.DataNode.SetData<int>("CalendarFailCount", 0);
		GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.GameWin));

		//搜寻所有的剩余棋子并破坏
		GameManager.Task.AddDelayTriggerTask(0.2f, () =>
		{
			List<TileItem> list = new List<TileItem>();
			foreach (var child in tileMapDict.Values)
			{
				foreach (var item in child.Values)
					list.Add(item.Item2);
			}
			tileMapDict.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				if (TileMatchUtil.IsSpecial(list[i].Data.TileID))
					list[i].DestroyTile(true, null, null);
			}
		});

		//清楚关卡数据缓存
		TileMatch_LevelData.ClearTileMatchLevelData();

		return true;
	}

	private bool isGameLose = false;
	public bool IsGameLose => isGameLose;

	public bool CheckIsLose()
    {
		return isCheckGameLostOver && GetChooseTotalNum() >= panelData.TileCellNum;
    }

	public void CheckLose(bool isForce = false)
	{
		if (!isForce && (!isCheckGameLostOver || GetChooseTotalNum() < panelData.TileCellNum)) return;

		if (isGameLose || isGameWin) return;
		isGameLose = true;

		GameManager.Firebase.RecordMessageByEvent(
			Constant.AnalyticsEvent.Level_Fail,
			new Parameter("Level", GameManager.PlayerData.NowLevel),
			new Parameter("times", openShopBuyItemPanelCount));

		GameManager.Sound.StopMusic(0.1f);
		GameManager.Task.AddDelayTriggerTask(2.5f, () =>
		{
			//播放背景音乐
			GameManager.Sound.PlayMusic(GameManager.PlayerData.HappyBgMusicName);
		});

		if(!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			GameManager.DataNode.SetData<int>("FailCount", GameManager.DataNode.GetData<int>("FailCount", 0) + 1);
		else
			GameManager.DataNode.SetData<int>("CalendarFailCount", GameManager.DataNode.GetData<int>("CalendarFailCount", 0) + 1);

		panelData.CombCount = 0;
		//GameManager.Sound.PlayAudio(SoundType.LOSE.ToString());
		//GameManager.Sound.ForbidSound(SoundType.LOSE.ToString(), true);
		int? loseSoundId = GameManager.Sound.PlayUISound(SoundType.SFX_Level_Fail_Lose_Lives.ToString());
		if (loseSoundId != null)
			GameManager.DataNode.SetData<int>("LoseSoundId", loseSoundId.Value);
		DaliyWatchAds_Prefab.gameObject.SetActive(false);
        int num = 0;
		DOTween.To(() => num, (t) => num = t, 5, 0.4f).OnComplete(() =>
		{
			// Action<UIForm> action = 
			// GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum) > 0 ? (uiform) =>
			// 	{
			// 		ShopBuyItemPanel panel = uiform as ShopBuyItemPanel;
			// 		panel.CheckIsShowTimeLimitLostPanel(timeBar.IsTimeOver, timeBar.ContinueAddTime);
			// 	}
			// 	: null;
			if (GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum) > 0)
			{
				GameManager.DataNode.SetData("TimeLimit_AddTime", timeBar.ContinueAddTime);
				GameManager.DataNode.SetData("TimeLimit_IsTimeOver", timeBar.IsTimeOver);
			}
			ShowLoseItemPanel(false);
		});

		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Level_Fail
				, new Parameter("DailyLevel", panelData.LevelNum));
		}
	}

	public void ShowLoseItemPanel(bool isDirectQuit = false, Action<UIForm> callback = null)
	{
		openShopBuyItemPanelCount++;
		int needCoin = GameManager.DataNode.GetData<int>("ContinueLevelCount", 0) < 1 ? 900 : 1800;
		GameManager.UI.ShowUIForm("ShopBuyItemPanel",UIFormType.PopupUI,callback, userData: new
		{
			needCoin,
			isHaveAdsContinue = panelData.IsHaveContinue,
			watchAdsAction = new Action(panelData.SetIsHaveContinue),
			isDirectQuit,
		});
		timeBar.PauseClock();
	}

	public void RecordGameLoseData()
    {
	    PkGameModel.Instance.Lose();
		PersonRankModel.Instance.GameLose();
		BalloonRiseModel.Instance.GameLose();

		GameManager.Activity.OnLevelLose();
    }

	public void StartGameLoseToMapProcess(Action callback)
    {
		int nowLevel = GameManager.PlayerData.NowLevel;

		GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Home, new Parameter("Level", nowLevel));

		bool isGameSettingQuit = GameManager.DataNode.GetData<bool>("GameSettingQuit", false);
		if (isGameSettingQuit)
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Setting_Home, new Parameter("Level", nowLevel));

		RecordGameLoseData();
		
		GameManager.Ads.ShowInterstitialAd(() =>
		{
			GameManager.UI.ShowUIForm("MapDecorationBGPanel",UIFormType.GeneralUI, f =>
			{
				callback?.Invoke();

				var bgPanel = GameManager.UI.GetUIForm("MapMainBGPanel");
				if (bgPanel != null)
					bgPanel.OnHide();

				ProcedureUtil.ProcedureGameToMap();
			});
		});
	}

	private bool isStartTileMoveAnim = false;

	private void RecordChooseTileMove(TileItem tileItem, bool isHaveAnim = true)
	{
		int itemIndex = 0;
		bool isStartAnim = (tileItem == null);
		foreach (var key in chooseItemDict.Keys)
		{
			int valueIndex = 0;
			foreach (var item in chooseItemDict[key])
			{
				bool isRemove = ((valueIndex + 1) % 3 == 0);
				if (!isStartAnim) isStartAnim = (item == tileItem);
				int animType = item == tileItem ? 1 : 2;
				if (isRemove)
				{
					if (isStartAnim)
						TilePosMove(animType, itemIndex, item, () =>
						{
							int tileID = item.Data.TileID;
							int canDestroyNum = 0;

                            if (chooseItemDict.ContainsKey(tileID))
                            {
								canDestroyNum = chooseItemDict[tileID].Count >= 3 ? 3 : 0;
								if (canDestroyNum > 0)
								{
									GameManager.Task.AddTargetCollection(TaskTarget.MatchTiles, 1);
									GameManager.Event.FireNow(this, TileMatchDestroyEventArgs.Create());
									GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.TileMatch, GetChooseTotalNum()));
									RecordComb();
									ShowFirstLevelGuideText(false);
								}
							}

                            void AnimEndAction()
                            {
								while (canDestroyNum > 0 && chooseItemDict.ContainsKey(key) && chooseItemDict[key] != null && chooseItemDict[key].Count > 0)  
                                {
                                    chooseItemDict[key][0].DestroyTile(canDestroyNum == 1, null, RefreshLevelBar);
                                    chooseItemDict[key].RemoveAt(0);
                                    canDestroyNum--;
                                }
								if (chooseItemDict.ContainsKey(key) && chooseItemDict[key].Count <= 0) chooseItemDict.Remove(key);

                                CheckWin();
							}

							bool isLastThreeTile = canDestroyNum > 0 && IsMapTotalNumZeroAndBackTotalNumZero(false) && GetChooseTotalNum(false) == 3;
							if (isLastThreeTile)
							{
								if (isStartTileMoveAnim)
									return;

								Transform[] transforms = new[]
								{
									chooseItemDict[key][0].transform,
									chooseItemDict[key][1].transform,
									chooseItemDict[key][2].transform
								};
								GameManager.Sound.PlayAudio(SoundType.SFX_Tile_Happy_Ending.ToString());
								AnimUtil.ShowLastTileDestroyAnim(transforms, AnimEndAction);
								DaliyWatchAds_Prefab.SetActive(false);
							}
							else
							{
								AnimEndAction();

								isStartTileMoveAnim = true;
								GameManager.Task.AddDelayTriggerTask(0.1f, () =>
								{
									isStartTileMoveAnim = false;
									RecordChooseTileMove(null);
								});
							}
						}, true, isHaveAnim);
				}
				else
                {
					TilePosMove(animType, itemIndex, item, null, isHaveAnim: isHaveAnim);
				}

				itemIndex++;
				valueIndex++;
			}
		}

		OnTilePosFill(GetChooseTotalNum(false));
	}

	private void OnTilePosFill(int posIndex)
    {
		GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.TilePosFill, posIndex));
    }

	private void ShowFirstLevelGuideText(bool isShow)
	{
		if (isShow&&GameManager.PlayerData.NowLevel == 1)
		{
			GameManager.Task.AddDelayTriggerTask(1.5f, () =>
			{
				FirstLevelGuide.alpha = 0;
				FirstLevelGuide.gameObject.SetActive(true);
				FirstLevelGuide.DOFade(1, 0.2f);
			});
		}
		else
		{
			FirstLevelGuide.gameObject.SetActive(false);
		}
	}

	private void SetText()
	{
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			Level_Text.gameObject.SetActive(false);
			Date_Text.gameObject.SetActive(true);
			var dateStr =
				CalendarChallengeManager.GetSelectedDateString(
					GameManager.DataNode.GetData("CalendarChallengeDate", DateTime.MinValue));
			Date_Text.SetTerm(dateStr);
			//Bar_BG.gameObject.SetActive(false);
		}
		else
		{
			Level_Text.gameObject.SetActive(true);
			Date_Text.gameObject.SetActive(false);
			Level_Text.SetParameterValue("{0}", panelData.LevelNum.ToString());
			//Bar_BG.gameObject.SetActive(true);
		}
	}

	class TileSequence
    {
		public Sequence sequ;
		public float startTime;
		public int taskId;

		public TileSequence(Sequence seq,float time,int id)
        {
			sequ = seq;
			startTime = time;
			taskId = id;
		}
	}

	Dictionary<TileItem, TileSequence> recordSequenceDict = new Dictionary<TileItem, TileSequence>();
	private void TilePosMove(int animType, int posIndex, TileItem tile, CallBack finishAction = null, bool isDestroy = false, bool isHaveAnim = true)
	{
		float animTime = 0.4f;
		Vector3 targetPos = GetChoosePos(posIndex);

		Transform tileTrans = tile.transform;
		if (!isHaveAnim)
		{
			tileTrans.localEulerAngles = Vector3.zero;
			tileTrans.position = targetPos;
			tileTrans.localScale = Vector3.one * GetTileScale(TileMatchPosType.Choose);
			recordSequenceDict.Remove(tile);
			finishAction?.Invoke();
			return;
		}

		float useTime = 0;
		if (recordSequenceDict.TryGetValue(tile, out TileSequence sequenceData))
		{
			if (sequenceData.sequ != null)
			{
				sequenceData.sequ.Kill();
			}
			useTime = Time.time - sequenceData.startTime;
			if (sequenceData.taskId != 0) 
				GameManager.Task.RemoveDelayTriggerTask(sequenceData.taskId);
		}
		tile.transform.DOKill();

		animTime = Mathf.Max(animTime - useTime, 0.1f);
		int id = 0;
        if (finishAction != null)
        {
			id = GameManager.Task.AddDelayTriggerTask(animTime, () =>
			{
				finishAction.Invoke();
			});
		}

		Sequence sequence1 = DOTween.Sequence();
		switch (animType)
		{
			case 1://当前飞下来的动画  放大然后左右摇晃 然后缩小 回正
				sequence1.Append(tileTrans.DOMove(targetPos, animTime)).SetEase(Ease.OutSine);
				sequence1.Join(tileTrans.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Choose), animTime * 0.5f));
				sequence1.Join(tileTrans.DOLocalRotate(tile.transform.position.x >= 0 ? Vector3.back * 20 : Vector3.forward * 20, animTime * 2 / 3f));
				sequence1.Join(tileTrans.DOLocalRotate(Vector3.zero, animTime / 3f).SetDelay(animTime * 2 / 3f));
				if (!isDestroy)
					sequence1.Append(tileTrans.DOShakeRotation(0.1f, Vector3.forward * 5, 8, 180));
				break;
			default://
				sequence1.Append(tileTrans.DOMove(targetPos, animTime)).SetEase(Ease.OutSine);
				sequence1.Join(tileTrans.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Choose), animTime * 0.7f));
				sequence1.Join(tileTrans.DOLocalRotate(Vector3.zero, animTime));
				break;
		}
		sequence1.OnKill(() => sequence1 = null);
		sequence1
				.OnComplete(() =>
				{
					tileTrans.localEulerAngles = Vector3.zero;
					tileTrans.localScale = Vector3.one * GetTileScale(TileMatchPosType.Choose);
					tileTrans.position = targetPos;
					recordSequenceDict.Remove(tile);
				});
		recordSequenceDict[tile] = new TileSequence(sequence1, Time.time, id);
	}

	private int GetBackTotalNum()
	{
		int num = 0;

		foreach (var item in backTileDict.Keys) if (item != null) num++;
		return num;
	}

	private bool IsMapTotalNumZeroAndBackTotalNumZero(bool includeSpecial)
	{
		foreach (var item in backTileDict.Keys)
			if (item != null)
				return false;
		foreach (var layer in tileMapDict)
		{
			foreach (int map in layer.Value.Keys)
            {
				if (!includeSpecial && TileMatchUtil.IsSpecial(layer.Value[map].Item1)) 
                {
					continue;
                }
				return false;
			}
		}
		return true;
	}

	private int GetMapTotalNum(bool includeSpecialTile = false)
	{
		int num = 0;
		foreach (var layer in tileMapDict)
		{
			foreach (var map in layer.Value)
            {
				if (!includeSpecialTile && TileMatchUtil.IsSpecial(map.Value.Item1))
					continue;

				num++;
			}
		}
		return num;
	}

	private int GetMapTypeTotalNum()
    {
		List<int> typeList = new List<int>();
		foreach (var layer in tileMapDict)
		{
			foreach (var map in layer.Value)
            {
				if (map.Value.Item2 != null && map.Value.Item2.Data != null && !typeList.Contains(map.Value.Item2.Data.TileID)) 
					typeList.Add(map.Value.Item2.Data.TileID);
			}
		}
		return typeList.Count;
	}

	private int GetMapCoverTotalNum()
	{
		int num = 0;
		foreach (var layer in tileMapDict)
		{
			foreach (var map in layer.Value) if (!map.Value.Item2.IsBeCover) num++;
		}
		return num;
	}

	private int GetChooseTotalNum(bool isEliminate = true)
	{
		int num = 0;
		foreach (var key in chooseItemDict.Keys)
		{
			num += isEliminate ? chooseItemDict[key].Count % 3 : chooseItemDict[key].Count;
		}
		return num;
	}
	
	private int GetChooseTotalTypeNum()
	{
		int typeNum = 0;
		foreach (var key in chooseItemDict.Keys)
		{
			typeNum += (chooseItemDict[key].Count % 3 >0) ? 1:0;
		}
		return typeNum;
	}
	/// <summary>
	/// 是否消除槽 有相同棋子
	/// </summary>
	/// <returns></returns>
	private bool IsHaveSameTileByChoose()
	{
		foreach (var key in chooseItemDict.Keys)
		{
			if (chooseItemDict[key].Count >= 2)
			{
				return true;
			}
		}
		return false;
	}

	private void HandleSkillEvent(SkillItem skill, TotalItemData type, CallBack callBack, bool isForceUse = false)
	{
		if (skillFree.Item1 == type && skillFree.Item2)
		{
			isForceUse = true;
			skillFree = (type, false);

			if (type == TotalItemData.Prop_Grab) 
				CommonGuideMenuUtil.HideWeakSkillGuide(GuideType.Skill_Grab);
		}

		bool isUnLock = IsUnLock(type, out int levelNum);
		if (!isForceUse && !isUnLock)
		{
			GameManager.UI.ShowWeakHint("Theme.Unlock at Level {0}", new Vector3(0, -0.2f), levelNum.ToString());
			skill.ShowLockAnim();
			return;
		}

		if (isGameLose || isGameWin) 
			return;

		System.Func<bool> UseItemHandle = () =>
		{
			if (isForceUse) return true;
			bool isHaveSkill = GameManager.PlayerData.CheckNum(type);
			if (!isHaveSkill)
			{
				GameManager.UI.ShowUIForm("SkillPropPurchaseMenu",UIFormType.PopupUI,null, null, type);
				return false;
				//int needCoinBuyProp = NeedCoinNumBuyProp(type);
				//if (!GameManager.PlayerData.UseItem(TotalItemData.Coin, needCoinBuyProp, false))
				//{
				//	GameManager.Firebase.RecordMessageByEvent(
				//		Constant.AnalyticsEvent.Level_Prop_Purchase_Fail,
				//		new Parameter("Level", GameManager.PlayerData.NowLevel));
				//	GameManager.UI.ShowUIForm<ShopMenuManager>(userData: true);
				//	return false;
				//}
				//else
				//{
				//	GameManager.Firebase.RecordMessageByEvent(
				//		Constant.AnalyticsEvent.Level_Prop_Purchase,
				//		new Parameter("Level", GameManager.PlayerData.NowLevel),
				//		new Parameter("PropID", (type.ID)));
				//}
			}
			else
			{
				if (GameManager.PlayerData.UseItem(type, 1))
				{
					GameManager.Firebase.RecordMessageByEvent(
						Constant.AnalyticsEvent.Level_Prop_Use,
						new Parameter("Level", GameManager.PlayerData.NowLevel),
						new Parameter("PropID", (type.ID)));
					callBack?.Invoke();

					if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
					{
						GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Level_Prop_Use, new Parameter("DailyLevel", panelData.LevelNum));
					}
				}
			}

			if (type == TotalItemData.Prop_Back)
			{
				GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_Undo, 1);
			}
			else if (type == TotalItemData.Prop_Grab)
			{
				GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_Crane, 1);
			}
			else if (type == TotalItemData.Prop_Absorb)
			{
				GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_Magnet, 1);
			}
			else if (type == TotalItemData.Prop_ChangePos)
			{
				GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_Shuffle, 1);
			}

			return true;
		};

		if (type == TotalItemData.Prop_Back)
		{
			if (GetChooseTotalNum() > 0)
			{
				if (UseItemHandle())
				{
					GameManager.Sound.PlayAudio(SoundType.SFX_UndoTool_Choose.ToString());
					ShowBackPropAnim();
				}
			}
		}
		else if (type == TotalItemData.Prop_Grab)
		{
			if (GetMapTotalNum() > 0)
			{
				if (UseItemHandle())
				{
					GameManager.Sound.PlayAudio(SoundType.SFX_GrappleTool_Choose.ToString());
					ShowPropAnim(type, () =>
					{
						UseCoinContinueLevel();
					}, 1);
				}
			}
		}
		else if (type == TotalItemData.Prop_Absorb)
		{
			if (GetMapTotalNum() > 0 || GetBackTotalNum() > 0)
			{
				if (UseItemHandle())
				{
					GameManager.Sound.PlayAudio(SoundType.SFX_MagnetTool_Choose.ToString());
					ShowPropAnim(type, () =>
					{
						GameManager.Sound.PlayAudio(SoundType.SFX_MagnetTool.ToString());
						MapTileToChooseTileDict();
						SaveToJson();
					}, 1.5f);
				}
			}
		}
		else if (type == TotalItemData.Prop_ChangePos)
		{
			if (GetMapTotalNum(true) > 1)
			{
				if (UseItemHandle())
				{
					GameManager.Sound.PlayAudio(SoundType.SFX_ShuffleTool_Choose.ToString());
					ShowShufflePropAnim();
				}
			}
		}
	}

	private void RefreshSkillItemState(bool isFirst = false)
	{
		if (isFirst)
		{
			skillList[0].SetState(false);
			skillList[1].SetState(true);
			skillList[2].SetState(true);
			skillList[3].SetState(false);
		}
		else
		{
			int chooseNum = GetChooseTotalNum();
			int mapNum = GetMapTotalNum();
			int backNum = GetBackTotalNum();
			skillList[0].SetState(chooseNum > 0);
			skillList[1].SetState(mapNum > 0 && GetMapTypeTotalNum() > 1);
			skillList[2].SetState((backNum > 0 || mapNum > 0));
			skillList[3].SetState((chooseNum >= 1));
		}
	}

	private int triggerStage = 0;
	private bool delayOneMatch = false;
	private float[] playEffectStages = new float[] { 0.3f, 0.6f, 0.9f };
	private void RefreshLevelBar()
	{
		(int, int) totalItem = SurplusNum;
		float targetAmount = (float)(totalItem.Item2 - totalItem.Item1) / totalItem.Item2;
		//DOTween.To(() => LevelBar_Image.fillAmount, (t) => LevelBar_Image.fillAmount = t, targetAmount, 0.4f);

		if(!delayOneMatch)
        {
			if (triggerStage < playEffectStages.Length && targetAmount >= playEffectStages[triggerStage])
			{
				triggerStage++;
				while (triggerStage < playEffectStages.Length && targetAmount >= playEffectStages[triggerStage])
				{
					triggerStage++;
				}

				delayOneMatch = true;
				PlayRibbonEffect();
			}
		}
		else
        {
			delayOneMatch = false;
		}
	}

	private void PlayRibbonEffect()
    {
		GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
			"CombEffect02",
			"TileItemDestroyEffectPool",
			2.2f,
			transform.position,
			transform.rotation,
			Prop_Parent, (t) =>
			{
				var effect = t?.Target as GameObject;
				if (effect != null)
				{
					var skeleton = effect.transform.GetComponentInChildren<SkeletonGraphic>();
					if (skeleton != null)
					{
						skeleton.AnimationState.ClearTracks();
						skeleton.SetToFirst();
						skeleton.AnimationState.SetAnimation(0, "active", false);
					}
				}
			});
	}

	private void WinAnim()
	{
		int levelNum = panelData.LevelNum;
		Vector3 starVector = LevelBarOverPos_Trans.position;

		GameManager.UI.HideUIForm(this);
		GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.SetMainBgBlack));

		GameManager.UI.ShowUIForm("GameWinPanel",UIFormType.PopupUI,form =>
		{
			(form as GameWinPanel).SetAction(() =>
			{
				ShowPkWinPanel(() =>
				{
					ShowWellDone(levelNum,starVector);
				});
			});
		}, userData: new
		{
			curLevel = levelNum,
			starVector = starVector,
		});
	}

	private void ChooseTileToBackTileItems(int index = 0)
	{
		int key = chooseItemDict.GetLastKey();
		TileItem tileItem = null;
		if (chooseItemDict[key].Count > 0)
			tileItem = chooseItemDict[key][chooseItemDict[key].Count - 1];
		if (tileItem == null)
		{
			chooseItemDict.Remove(key);
			ChooseTileToBackTileItems();
			return;
		}

		chooseItemDict[key].Remove(tileItem);
		if (chooseItemDict[key].Count <= 0)
		{
			chooseItemDict.Remove(key);
		}
		OnChooseItemDictChange();

		tileItem.transform.SetParent(BackParent,true);

		AddToBackDict(tileItem);
		DOTween.Sequence()
			.AppendInterval(0.4f)
			.AppendCallback(() => { GameManager.Sound.PlayAudio(SoundType.SFX_UndoTool.ToString()); })
			.Append(tileItem.transform.DOMove(BackTileTrans[backTileDict[tileItem] % BackTileTrans.Length].position + Vector3.up * BackOffsetY(tileItem), 0.2f))
			.Join(tileItem.transform.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Back), 0.2f))
			.SetEase(Ease.OutSine)
			.SetDelay(index * 0.2f);

		tileItem.Init(0, 0, tileItem.Data.TileID, tileItem.Data.AttachID, TileMoveDirectionType.None, null, null, SetTileBtnEvent);//���³�ʼ��
		tileItem.transform.SetAsLastSibling();
		tileItem.SetClickState(true);
	}

	private void MapToBack(int num)
	{
		if (num <= 0 || GetMapTotalNum() <= 0) return;

		List<TileItem> list = new List<TileItem>();
		foreach (var child in tileMapDict.Values)
		{
			foreach (var item in child.Values)
			{
				if (!item.Item2.IsBeCover)
				{
					list.Add(item.Item2);
				}
			}
		}

		TileItem tileItem = list[Random.Range(0, list.Count)];
		var tileData = tileItem.Data;
		if (tileData.CoverIndexs != null)
		{
			foreach (var child in tileData.CoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
					OnCoverStateChange(tileMapDict[child.Key][index].Item2);
				}
			}
		}
		if (tileData.BeCoverIndexs != null)
		{
			foreach (var child in tileData.BeCoverIndexs)
			{
				foreach (var index in child.Value)
				{
					tileMapDict[child.Key][index].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
				}
			}
		}

		tileMapDict[tileData.Layer].Remove(tileData.MapID);

		tileItem.transform.SetParent(BackParent,true);

		AddToBackDict(tileItem);

		float animTime = 0.2f + (3 - num) * 0.2f;
		DOTween.Sequence()
			.AppendInterval(0.6f)
			.Append(tileItem.transform.DOMove(BackTileTrans[backTileDict[tileItem] % BackTileTrans.Length].position + Vector3.up * BackOffsetY(tileItem), animTime))
			.Join(tileItem.transform.DOScale(Vector3.one * GetTileScale(TileMatchPosType.Back), animTime));

		tileItem.Init(0, 0, tileData.TileID, tileData.AttachID, TileMoveDirectionType.None, null, null, SetTileBtnEvent);
		tileItem.transform.SetAsLastSibling();
		tileItem.SetClickState(true);

		MapToBack(num - 1);
	}


	private bool MapTileToChooseTileDict()
	{
		TileItem[] tileItems = new TileItem[3];
		int num = 0;
		if (GetChooseTotalNum() <= 0)
		{
			int random = Random.Range(1, GetMapCoverTotalNum() + 1);
			int totalNum = 0;
			
			foreach (var layer in tileMapDict)
			{
				foreach (var map in layer.Value)
				{
					if (map.Value.Item2 != null && !map.Value.Item2.IsBeCover && !TileMatchUtil.IsSpecial(map.Value.Item1)) 
					{
						totalNum += 1;
						if (random == totalNum && map.Value.Item2 != null)
						{
							tileItems[0] = map.Value.Item2;
							break;
						}
					}
				}
				if (tileItems[0] != null) break;
			}
			if (tileItems[0] == null)
			{
				// if (backTileDict.Keys.Count > 0)
    //             {
				// 	tileItems[0] = backTileDict.Keys.ElementAt(Random.Range(0, backTileDict.Count));
				// 	if (tileItems[0] == null)
				// 		tileItems[0] = backTileDict.Keys.ElementAt(Random.Range(0, backTileDict.Count));
				// }
				if (backTileDict.Count > 0)
				{
					// 将 Keys 缓存为数组（只创建一次，如果频繁调用可复用）
					var keys = new List<TileItem>(backTileDict.Count);
					foreach (var key in backTileDict.Keys)
						keys.Add(key);

					// 随机获取 key
					int index2 = Random.Range(0, keys.Count);
					tileItems[0] = keys[index2];

					// 再次随机尝试（保持原逻辑）
					if (tileItems[0] == null && keys.Count > 0)
					{
						index2 = Random.Range(0, keys.Count);
						tileItems[0] = keys[index2];
					}
				}

				if (tileItems[0] == null)
                {
					foreach (var layer in tileMapDict)
					{
						foreach (var map in layer.Value)
						{
							if (map.Value.Item2 != null && !TileMatchUtil.IsSpecial(map.Value.Item1))
							{
								totalNum += 1;
								if (random == totalNum && map.Value.Item2 != null)
								{
									tileItems[0] = map.Value.Item2;
									break;
								}
							}
						}
						if (tileItems[0] != null) break;
					}
					if (tileItems[0] == null) return false;
				}
			}
			num = 1;
		}
		else
		{
			foreach (var child in chooseItemDict.Keys)
			{
				var listItems = chooseItemDict[child];
				int childNum = listItems.Count % 3;
				if (childNum >= num)
				{
					num = childNum;
					var list = listItems.GetRange(listItems.Count - childNum, childNum);
					for (int i = 0; i < list.Count; i++)
					{
						tileItems[i] = list[i];
					}
				}
			}
		}

		isCheckGameLostOver = false;

        //优先可点击棋子
        foreach (var layer in tileMapDict)
        {
	        if (num >= 3) break;
	        foreach (var map in layer.Value)
	        {
		        if (map.Value.Item2 != null&&
		            !map.Value.Item2.IsBeCover && 
		            map.Value.Item1 == tileItems[0].Data.TileID && 
		            !tileItems.Contains(map.Value.Item2))
		        {
			        tileItems[num] = map.Value.Item2;
			        num += 1;
			        if (num >= 3) break;
		        }
	        }
        } 
		
		foreach (var layer in tileMapDict)
		{
			if (num >= 3) break;
			foreach (var map in layer.Value)
			{
				if (map.Value.Item2 != null&&
				    map.Value.Item2.IsBeCover && 
				    map.Value.Item1 == tileItems[0].Data.TileID && 
				    !tileItems.Contains(map.Value.Item2))
				{
					tileItems[num] = map.Value.Item2;
					num += 1;
					if (num >= 3) break;
				}
			}
		}

		if (num < 3)
		{
			foreach (var tile in backTileDict.Keys)
			{
				if (tile != null && 
				    tile.Data.TileID == tileItems[0].Data.TileID &&
				    !tileItems.Contains(tile))
				{
					tileItems[num] = tile;
					num += 1;
					if (num >= 3) break;
				}
			}
		}

        //因为部分附属物逻辑与是否被遮盖有关，所以在遮盖逻辑之前执行
        foreach (var tile in tileItems)
        {
            if (tile == null) continue;
            if (chooseItemDict != null
                && chooseItemDict.ContainsKey(tile.Data.TileID)
                && chooseItemDict[tile.Data.TileID].Contains(tile)) continue;

            var tileData = tile.Data;
			tile.IsDestroyed = true;
			OnTileGet(tileData.Layer, tileData.MapID, TotalItemData.Prop_Absorb);
        }

		bool isTrigger = false;
		void FinishCallback()
        {
			if (isTrigger)
				return;
			isTrigger = true;

			foreach (var kvp in tileMapDict)
			{
				foreach ((int, TileItem) map in kvp.Value.Values)
				{
					if (map.Item1 != 17 && map.Item2 != null && map.Item2.AttachLogic != null)
					{
						map.Item2.AttachLogic.OnAnyTileGet();
					}
				}
			}
		}

		int index = 0;
		foreach (var tile in tileItems)
		{
			if (tile == null) continue;
			if (chooseItemDict != null
				&& chooseItemDict.ContainsKey(tile.Data.TileID)
				&& chooseItemDict[tile.Data.TileID].Contains(tile)) continue;

			index += 1;
			var tileData = tile.Data;
			bool isBeCover = tile.IsBeCover;

			CallBack animCenterCallBack = () =>
			{
				if (tileData.CoverIndexs != null)
				{
					foreach (var child in tileData.CoverIndexs)
					{
						foreach (var index1 in child.Value)
						{
							tileMapDict[child.Key][index1].Item2.RemoveIndex(tileData.Layer, tileData.MapID);
							OnCoverStateChange(tileMapDict[child.Key][index1].Item2);
						}
					}
				}
				if (tileData.BeCoverIndexs != null)
				{
					foreach (var child in tileData.BeCoverIndexs)
					{
						foreach (var index1 in child.Value)
						{
							tileMapDict[child.Key][index1].Item2.RemoveCoverIndexs(tileData.Layer, tileData.MapID);
						}
					}
				}
				tile.Data.CoverIndexs = null;
				tile.Data.BeCoverIndexs = null;            
            };
			CallBack finishAction = () =>
			{
				OnTileClick(tile, TotalItemData.Prop_Absorb, false, isCheckLose: false);

				FinishCallback();
			};

			tile.transform.SetAsLastSibling();
			//////////////
			tile.AttachLogic?.SpecialCollect(!isBeCover);
			tile.PlayPropAbsorbAnim(true, animCenterCallBack, finishAction);
		}
		isCheckGameLostOver = true;

		return true;
	}

	private void OnCoverStateChange(TileItem tileItem)
	{
		//当tile周围没有tile时附属物触发OnAroundTileEmpty
		if (!tileItem.IsBeCover && tileItem.AttachLogic != null)
		{
			int row = tileItem.Data.MapID / 16;
			int col = tileItem.Data.MapID % 16;
			//上，上右，右上，右，右下，下右，下，下左，左下，左，左上，上左
			int[] rows = new int[] { 2, 2, 1, 0, -1, -2, -2, -2, -1, 0, 1, 2 };
			int[] cols = new int[] { 0, 1, 2, 2, 2, 1, 0, -1, -2, -2, -2, -1 };
			SortedDictionary<int, (int, TileItem)> dict = tileMapDict[tileItem.Data.Layer];

			bool isAroundEmpty = true;
			for (int i = 0; i < rows.Length; i++)
			{
				int aroundRow = row + rows[i];
				int aroundCol = col + cols[i];
				if (aroundCol < 0 || aroundCol >= 16)
					continue;

				int mapIndex2 = aroundRow * 16 + aroundCol;
				if (dict.TryGetValue(mapIndex2, out (int, TileItem) map2) && map2.Item2 != null && !map2.Item2.IsDestroyed && (map2.Item2.AttachLogic == null || map2.Item2.AttachLogic.AttachId != 1)) 
				{
					isAroundEmpty = false;
					break;
				}
			}

			if (isAroundEmpty)
				tileItem.AttachLogic.OnAroundTileEmpty();
		}
	}

	private void AddToBackDict(TileItem tileItem)
	{
		int posIndex = -1;
		//List<int> backListKeys = backTileDict.Values.ToList();
		List<int> backListKeys = new List<int>(backTileDict.Values.Count);
		foreach (var val in backTileDict.Values)
		{
			backListKeys.Add(val);
		}

		while (posIndex == -1)
		{
			for (int i = 0; i < BackTileTrans.Length; i++)
			{
				if (backListKeys != null && backListKeys.Contains(i))
				{
					backListKeys.Remove(i);
				}
				else
				{
					posIndex = i;
					break;
				}
			}
		}
		backTileDict.Add(tileItem, posIndex);
	}

	private float BackOffsetY(TileItem tile)
	{
		int offsetIndex = 0;
		var posIndex = backTileDict[tile];
		foreach (var item in backTileDict)
		{
			if (item.Value == posIndex)
			{
				if (item.Key == tile) break;
				else offsetIndex++;
			}
		}
		return offsetIndex * 2 * 9f / Screen.height;
	}

	private void UseCoinContinueLevel()
	{
		isGameLose = false;

		if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
		{
			ShowGrabPropAnim();
		}
		else
		{
			int index = 0;
			int maxChooseNum = System.Math.Min(3, GetChooseTotalNum());
			while (index < maxChooseNum)
			{
				index++;
				ChooseTileToBackTileItems(index);
			}
			SaveToJson();
			ChangeTileScale();	
		}
	}

	class RotateTile
    {
		public RotateTile(Transform trans, Vector3 pos, float towardDelta, float aroundDelta, int startIndex, int endIndex, float targetRotation, Action callback)
        {
			CachedTrans = trans;
			TargetPos = pos;
			TowardCenterDelta = towardDelta;
			AroundCenterDelta = aroundDelta;
			StartIndex = startIndex;
			EndIndex = endIndex;
			TargetRotation = targetRotation;
			Callback = callback;
			IsFinish = false;
		}

		public Transform CachedTrans;
		public Vector3 TargetPos;
		public float TowardCenterDelta;
		public float AroundCenterDelta;
		public int StartIndex;
		public int EndIndex;
		public float TargetRotation;
		public Action Callback;
		public bool IsFinish;
    }

	private List<RotateTile> rotateTiles = new List<RotateTile>();
	private int rotateDegree;
	private bool ChangeTotalTilePos()
	{
		int totalNum = GetMapTotalNum(true);
		if (totalNum <= 1)
		{
			return false;
		}
		else
		{
			//取储存条内 花色 优先级：有两个相同花色 单个花色 没有花色
			int changePosNum = 1;
			int tileChooseID = 0;
			foreach (var item in chooseItemDict.Values)
			{
				if (item.Value.Count >= 2)
				{
					tileChooseID = item.Key;
					break;
				}
			}
			
			//取最后一个花色
			if (tileChooseID == 0)
			{
				if (chooseItemDict.Values.Count > 0)
				{
					changePosNum = 2;
					tileChooseID = chooseItemDict.Values.Last().Key;
				}
			}
			
			//如果无选中棋子，则取棋盘最后一个棋子
			if (tileChooseID == 0)
			{
				changePosNum = 3;
				tileChooseID = tileMapDict.Values.Last().Values.Last().Item2.Data.TileID;
			}

			Dictionary<int, (TileItem, TileItemData)> dict = new Dictionary<int, (TileItem, TileItemData)>();
			Dictionary<int, int> recordCanClickDict = new Dictionary<int, int>();
			Dictionary<int, int> fixedTileDic = new Dictionary<int, int>();
			//int tileChooseIdPos = 0;
			int[] tileChooseIdPoss = new int[changePosNum];
			int index = 0;

			foreach (var layer in tileMapDict)
			{
				foreach (var map in layer.Value.Values)
				{
					index++;
					dict.Add(index, (map.Item2, TileItemData.CopyTo(map.Item2.Data)));
					if (tileChooseID != 0)
					{
						if (!map.Item2.IsBeCover && !TileMatchUtil.IsSpecial(map.Item2.Data.TileID) && (map.Item2.State == TileItemState.None || (map.Item2.AttachLogic != null && map.Item2.AttachLogic.AttachId == 2))) 
						{
							recordCanClickDict.Add(index, map.Item2.Data.TileID);
						}
						
						if (changePosNum > 0&&map.Item2.Data.TileID == tileChooseID)
						{
							changePosNum--;
							tileChooseIdPoss[changePosNum] = index;
						}

						// if (tileChooseIdPos == 0 && map.Item2.Data.TileID == tileChooseID)
						// {
						// 	tileChooseIdPos = index;
						// }
					}

					if (TileMatchUtil.IsSpecial(map.Item2.Data.TileID))
						fixedTileDic.Add(index, map.Item2.Data.TileID);
				}
			}

			//确保目标棋子不动
			List<int> randoms = TileMatchUtil.GetRandmonList(totalNum);
            foreach (var pair in fixedTileDic)
            {
				int oldIndex = pair.Key;
				int newIndex = randoms[oldIndex - 1];
      //          if (fixedTileDic.ContainsKey(newIndex))
      //          {
      //              foreach (var item in dict)
      //              {
      //                  if (!TileMatchUtil.IsSpecial(item.Value.Item2.TileID))
      //                  {
						//	newIndex = item.Key;
						//	break;
						//}
      //              }
      //          }

                try
                {
					randoms[randoms.IndexOf(oldIndex)] = newIndex;
					randoms[oldIndex - 1] = oldIndex;
				}
				catch(Exception e)
                {
					Debug.LogError($"!!!!!!!!!!!!!!!!!!{oldIndex}!!!!!!{randoms.IndexOf(oldIndex)}!!!!!!!{totalNum}!!!!!!!!!!{randoms.Count}");
                }
			}

			//check if ok
			if (tileChooseID != 0 && tileChooseIdPoss.Length>0)
			{
				//找一个 表面位置的交换下
				foreach (var pos in tileChooseIdPoss)
				{
					if(pos==0)continue;
					if (recordCanClickDict.Count <= 0) break;
					int posNum = pos;
					int range = Random.Range(0, recordCanClickDict.Count);
					int openPosNum =recordCanClickDict.ElementAt(range).Key;
					recordCanClickDict.Remove(openPosNum);
					
					int newPos2Num = randoms[openPosNum - 1];

					if (newPos2Num != posNum)
					{
						int newPos1Num = randoms.IndexOf(posNum);
						randoms[newPos1Num] = newPos2Num;
						randoms[openPosNum - 1] = posNum;
					}
				}
			}

			rotateTiles.Clear();
			rotateDegree = 0;
			for (int i = 1; i <= randoms.Count; i++)
			{
				var curTileItemData = dict[i].Item2;
				int targetIndex = randoms[i - 1];

				int lastLayer = dict[targetIndex].Item2.Layer;
				int lastRow = dict[targetIndex].Item2.MapID / 16;
				int lastCol = dict[targetIndex].Item2.MapID % 16;
				int curLayer = curTileItemData.Layer;
				int curRow = curTileItemData.MapID / 16;
				int curCol = curTileItemData.MapID % 16;
				int attachID = curTileItemData.AttachID == 3 ? dict[targetIndex].Item1.Data.AttachID : curTileItemData.AttachID;
				if (dict[targetIndex].Item1.Data.TileID == 17 && dict[targetIndex].Item1.AttachLogic?.attachState < 5)
				{
					attachID = 3;
				}
				int attachState = attachID == 3 ? dict[targetIndex].Item1.AttachLogic.attachState : 1;

				dict[targetIndex].Item1.Init(
					curTileItemData.Layer,
					curTileItemData.MapID,
					dict[targetIndex].Item1.Data.TileID,
					attachID,
					attachState,
					curTileItemData.MoveIndex,
					Vector3.back,
					curTileItemData.CoverIndexs,
					curTileItemData.BeCoverIndexs,
					SetTileBtnEvent, false, false);
				dict[targetIndex].Item1.AttachLogic?.Hide();
				var targetPos = TileMatchUtil.GetTilePos(curTileItemData.Layer, curTileItemData.MapID, curTileItemData.MoveIndex, panelData.RecordRandomMoveDict);
				dict[targetIndex].Item1.transform.DOKill(false);

				int siblingIndex = i;
				//dict[targetIndex].Item1.transform.DOLocalMove(dict[targetIndex].Item1.transform.localPosition * 1.4f, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
				//   {
				//	   dict[targetIndex].Item1.transform.DOLocalMove(targetPos, 0.4f).SetEase(Ease.OutBack);
				//	   dict[targetIndex].Item1.AttachLogic?.Show();
				//   });
				//Vector3 centerLocalPos = dict[targetIndex].Item1.transform.InverseTransformPoint(CenterRectTrans.position);
				Vector2 tilePos = dict[targetIndex].Item1.transform.position;
				dict[targetIndex].Item1.transform.localPosition = targetPos;
				Vector2 targetWorldPos = dict[targetIndex].Item1.transform.position;
				dict[targetIndex].Item1.transform.position = tilePos;
				Vector2 centerPos = CenterRectTrans.position;

				Vector2 dir1 = (Vector2)dict[targetIndex].Item1.transform.position - (Vector2)CenterRectTrans.position;
				Vector2 dir2 = targetWorldPos - (Vector2)CenterRectTrans.position;
				float z = Vector3.Cross(dir1, dir2).z;
				float targetDegree;
				if (z > 0)
					targetDegree = 360 - Vector2.Angle(dir1, dir2);
				else
					targetDegree = 360 + Vector2.Angle(dir1, dir2);

				float aroundDis = 0.32f;
				float towardDelta = aroundDis - Vector2.Distance(targetWorldPos, centerPos);
				float aroundDelta = aroundDis - Vector2.Distance(tilePos, centerPos);

				int startIndex = lastRow * 16 + lastCol;
				int endIndex = curRow * 16 + curCol;
				dict[targetIndex].Item1.SetUncoverState();
				dict[targetIndex].Item1.StopAllAnim();
				rotateTiles.Add(new RotateTile(dict[targetIndex].Item1.transform, targetPos, towardDelta, aroundDelta, startIndex, endIndex, targetDegree, () =>
						 {
							 dict[targetIndex].Item1.SetColor();
							 dict[targetIndex].Item1.AttachLogic?.Show();
							 dict[targetIndex].Item1.StartAllAnim();
                         }));

				dict[targetIndex].Item1.transform.SetSiblingIndex(siblingIndex);
				tileMapDict[curTileItemData.Layer][curTileItemData.MapID] = (dict[targetIndex].Item1.Data.TileID, dict[targetIndex].Item1);
			}

			rotateTiles.Sort((a, b) =>
			{
				if (a == null || b == null || a.StartIndex == b.StartIndex) 
					return 0;
				return a.StartIndex > b.StartIndex ? 1 : -1;
			});

			int perCount = rotateTiles.Count / 10 + 1;
			if (perCount == 0)
				perCount = 1;
			for (int i = 0; i < rotateTiles.Count; i++)
            {
				rotateTiles[i].StartIndex = (i + 1) / perCount;
			}

			rotateTiles.Sort((a, b) =>
			{
				if (a == null || b == null || a.EndIndex == b.EndIndex) 
					return 0;
				return a.EndIndex > b.EndIndex ? 1 : -1;
			});
			for (int i = 0; i < rotateTiles.Count; i++)
			{
				rotateTiles[i].EndIndex = (i + 1) / perCount;
			}

			return true;
		}
	}

	private enum TileMatchPosType
	{
		Map,
		Back,
		Choose,
	}
	private float GetTileScale(TileMatchPosType type)
	{
		switch (type)
		{
			case TileMatchPosType.Map:
				if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
					return Mathf.Clamp(panelData.TileMatchData.Scale, TileMatchUtil.ChangeScale, 2f);
				else
					return Mathf.Clamp(panelData.TileMatchData.Scale, TileMatchUtil.ChangeScale, 1f);
			case TileMatchPosType.Back:
				return 1;
			case TileMatchPosType.Choose:
				return TileMatchUtil.ChangeScale;
			default:
				return 1;
		}
	}

	private void ChangeTileScale()
	{
		float[] moveVect = panelData.TileMatchData.MoveVect;
		TileMatchParent.DOAnchorPos(
			new Vector2(TileMatchUtil.EachWidth * moveVect[0], -TileMatchUtil.EachHeight * moveVect[1]), 0f).SetEase(Ease.InOutSine);
	}

	#region Comb

	//private Sequence sequence;
	private void RecordComb()
	{
		int curClickCount = panelData.ClickTileCount;
		panelData.CombCount++;
		panelData.ClickTileCount = 0;

		string combAnimName = null;

		if (curClickCount < 4)
		{
			string[] strs = { "great", "Awesome", "Perfect", "Amazing", "Execllent" };
			combAnimName = strs[Random.Range(0, strs.Length)];
		}
		else
		{
			//comb show
			switch (panelData.CombCount)
			{
				case 9:
					combAnimName = "great";
					break;
				case 18:
					combAnimName = "Perfect";
					break;
				case 27:
					combAnimName = "Amazing";
					break;
			}
		}

		if (combAnimName != null)
        {
			CombAnim.gameObject.SetActive(true);
			CombAnim.AnimationState.ClearTracks();
			CombAnim.Skeleton.SetToSetupPose();
			CombAnim.AnimationState.SetAnimation(0, combAnimName, loop: false).Complete += (t) => CombAnim.gameObject.SetActive(false);
			GameManager.Sound.PlayAudio($"SFX_Level_Encourage_{combAnimName}_Vocals");
        }

		//if (!isPlayEffect && GetChooseTotalNum() == 0) 
		//	PlayEffect();

		//if (combAnimName != null)
		//{
		//	CombText_BoneFollower.SetBone($"{combAnimName}");
		//	CombAnim.gameObject.SetActive(true);
		//	CombAnim.AnimationState.ClearTracks();
		//	CombAnim.Skeleton.SetToSetupPose();
		//	CombAnim.AnimationState.SetAnimation(0, combAnimName, loop: false).Complete += (t) => CombAnim.gameObject.SetActive(false);

		//	switch (combAnimName)
		//	{
		//		case "01":
		//			Comb_TextUGUILocal.SetTerm("Game.Great!");
		//			Comb_TextUGUILocal.SetMaterialPreset(CombText_Materials[0]);
		//			break;
		//		case "02":
		//			Comb_TextUGUILocal.SetTerm("Game.Awesome!");
		//			Comb_TextUGUILocal.SetMaterialPreset(CombText_Materials[1]);
		//			break;
		//		case "03":
		//			Comb_TextUGUILocal.SetTerm("Game.Perfect!");
		//			Comb_TextUGUILocal.SetMaterialPreset(CombText_Materials[2]);
		//			break;
		//		case "04":
		//			Comb_TextUGUILocal.SetTerm("Game.Amazing!");
		//			Comb_TextUGUILocal.SetMaterialPreset(CombText_Materials[3]);
		//			break;
		//		case "05":
		//			Comb_TextUGUILocal.SetTerm("Game.Excellent!");
		//			Comb_TextUGUILocal.SetMaterialPreset(CombText_Materials[4]);
		//			break;
		//	}

		//	var graphic = Comb_TextUGUILocal.GetComponent<Graphic>();
		//	if (graphic != null)
		//	{
		//		graphic.color = new Color(1, 1, 1, 0);
		//		if (sequence != null) sequence.Kill();
		//		sequence = DOTween.Sequence()
		//			.Append(graphic.DOFade(1, 8f / 30f))
		//			.AppendInterval(27f / 30f)
		//			.Append(graphic.DOFade(0, 8f / 30f));
		//	}

		//	// GameManager.Sound.PlayAudio(SoundType.SFX_Comb.ToString());
		//}
	}

	private void RecordClickTileCount()
	{
		panelData.ClickTileCount++;
	}

	//Sequence redBGSequece = null;
	private void ShowLastBoxTip()
	{
		if ((GetChooseTotalNum() + 1) >= panelData.TileCellNum)
		{
			ShowPunchBottomBGTip();
		}
		else
		{
			//if (redBGSequece != null) redBGSequece.Kill();
			if (punchBGSequece != null) punchBGSequece.Kill();

			RedBottomBG.color = new Color(1, 1, 1, 0.4f);
			RedBottomBG.gameObject.SetActive(false);
			BlackBottomBG.enabled = true;
			ChooseParent.localScale = Vector3.one;
			BlackBottomBG.transform.localScale = Vector3.one;
		}
	}

	// private void ShowRedBottomBGTip()
 //    {
	// 	if (redBGSequece != null && redBGSequece.IsPlaying()) return;
	// 	if (redBGSequece != null) redBGSequece.Kill();
 //
	// 	if (!isAddOneStepState && !AddOneStep_Btn.gameObject.activeSelf)
	// 		RedBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(872, 162);
	// 	else
	// 		RedBottomBG.GetComponent<RectTransform>().sizeDelta = new Vector2(992, 162);
 //
	// 	RedBottomBG.gameObject.SetActive(true);
	// 	BlackBottomBG.enabled = false;
	// 	RedBottomBG.color = new Color(1, 1, 1, 0.4f);
	// 	redBGSequece = DOTween.Sequence()
	// 		.Append(RedBottomBG.DOFade(1, 1f))
	// 		.SetLoops(-1, LoopType.Yoyo).OnKill(() => redBGSequece = null);
	// }

	Sequence punchBGSequece = null;

	private void ShowPunchBottomBGTip()
    {
		if (punchBGSequece != null && punchBGSequece.IsPlaying()) return;
		if (punchBGSequece != null) punchBGSequece.Kill();

		ChooseParent.localScale = Vector3.one;
		BlackBottomBG.transform.localScale = Vector3.one;
		Vector3 targetScale = new Vector3(1.05f, 1.05f, 1f);
		Vector3 originalScale = Vector3.one;

		punchBGSequece = DOTween.Sequence();

		// 停顿
		punchBGSequece.AppendInterval(0.6f);

		// 第一次放大收缩
		punchBGSequece.Append(ChooseParent.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad));
		punchBGSequece.Join(BlackBottomBG.transform.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad));
		punchBGSequece.Append(ChooseParent.DOScale(originalScale, 0.25f).SetEase(Ease.InQuad));
		punchBGSequece.Join(BlackBottomBG.transform.DOScale(originalScale, 0.25f).SetEase(Ease.InQuad));

		// 第二次放大收缩
		punchBGSequece.Append(ChooseParent.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad));
		punchBGSequece.Join(BlackBottomBG.transform.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad));
		punchBGSequece.Append(ChooseParent.DOScale(originalScale, 0.25f).SetEase(Ease.InQuad));
		punchBGSequece.Join(BlackBottomBG.transform.DOScale(originalScale, 0.25f).SetEase(Ease.InQuad));

		// 无限循环
		punchBGSequece.SetLoops(-1).OnKill(() => punchBGSequece = null);
	}

	List<TileItem> needAnimTileItems = new List<TileItem>();
	SkillItem recordSkillItemAnim = null;
	private DaliyWatchAdsPrefab recordDaliy = null;
	private void CkeckActiveTile(bool isCheckOrCancel = true)
	{
		if (!isCheckOrCancel)
		{
			needAnimTileItems.ForEach(a => a.PlayImageAnim(false));
			if (recordSkillItemAnim != null) recordSkillItemAnim.PlayFingerAnim(false);

			if (recordDaliy != null) recordDaliy.PlayFingerAnim(false);
			recordDaliy = null;

			needAnimTileItems.Clear();
			recordSkillItemAnim = null;
			return;
		}
		else
		{
			ShowLastBoxTip();

			if (GameManager.PlayerData.TurnOffTips)
            {
				needAnimTileItems.Clear();
				recordSkillItemAnim = null;
				recordDaliy = null;
				return;
			}

			if (!GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
			{
				if (recordDaliy == null && !GameManager.PlayerData.NeedShowDaliyWatchAdsGuide)
				{
					recordDaliy = DaliyWatchAds_Prefab;
					recordDaliy.PlayFingerAnim(true);

					if (recordSkillItemAnim != null) recordSkillItemAnim.PlayFingerAnim(false);
					recordSkillItemAnim = null;
					return;
				}
				else
				{
					recordDaliy = null;
				}	
			}

			if (needAnimTileItems.Count >= 3)
			{
				needAnimTileItems.ForEach(a => a.PlayImageAnim(true));
				if (recordSkillItemAnim != null) recordSkillItemAnim.PlayFingerAnim(false);
				return;
			}
			else if ((GetChooseTotalNum() + 1) >= panelData.TileCellNum && recordSkillItemAnim != null)
			{
				recordSkillItemAnim.PlayFingerAnim(true);
				return;
			}
			else
			{
				if (recordSkillItemAnim != null) recordSkillItemAnim.PlayFingerAnim(false);
			}
		}
		int surplusTileNum = panelData.TileCellNum - GetChooseTotalNum();
		if (surplusTileNum > 0)
		{
			List<TileItem> list = new List<TileItem>();
			foreach (var layer in tileMapDict)
			{
				foreach (var map in layer.Value)
				{
					if (!map.Value.Item2.IsBeCover)
					{
						list.Add(map.Value.Item2);
					}
				}
			}
			List<int> backList = new List<int>(backTileDict.Count);
			for (int i = backTileDict.Count - 1; i >= 0; i--)
			{
				int pos = backTileDict.ElementAt(i).Value;
				if (!backList.Contains(pos))
				{
					backList.Add(pos);
					list.Add(backTileDict.ElementAt(i).Key);
				}
			}
			var dict = list.GroupBy(a => a.Data.TileID).ToDictionary(a => a.Key, a => a.ToList());

			needAnimTileItems.Clear();
			if (chooseItemDict.Values.Count > 0)
			{
				LinkedList<int> recordTileID = new LinkedList<int>();
				foreach (var items in chooseItemDict.ToDictionary())
				{
					if (items.Value.Count > 1)
						recordTileID.AddFirst(items.Key);
					else if (items.Value.Count > 0)
						recordTileID.AddLast(items.Key);
				}

				foreach (var tileId in recordTileID)
				{
					if (dict.TryGetValue(tileId, out List<TileItem> targetList))
					{
						List<TileItem> itemList = new List<TileItem>();
						for (int i = 0; i < targetList.Count; i++)
                        {
							if (!targetList[i].State.HasFlag(TileItemState.DisableClick) && targetList[i].Data.TileID != 20) 
								itemList.Add(targetList[i]);
						}

						var items = chooseItemDict[tileId];
						if (items.Count + itemList.Count >= 3)
						{
							for (int i = 0; i < 3 - items.Count; i++)
								needAnimTileItems.Add(itemList[i]);
							break;
						}
					}
				}
			}

			if (needAnimTileItems.Count > surplusTileNum)
			{
				needAnimTileItems.Clear();
			}

			if (needAnimTileItems.Count == 0 && surplusTileNum >= 3)
			{
				foreach (var items in dict)
				{
					if (items.Value.Count >= 3)
					{
						List<TileItem> itemList = new List<TileItem>();
						for (int i = 0; i < items.Value.Count; i++)
						{
							if (!items.Value[i].State.HasFlag(TileItemState.DisableClick) && items.Value[i].Data.TileID != 20)
								itemList.Add(items.Value[i]);
						}

                        if (itemList.Count >= 3)
                        {
							for (int i = 0; i < 3; i++)
								needAnimTileItems.Add(itemList[i]);
							break;
						}
					}
				}
			}

			if (needAnimTileItems.Count > 0)
			{
				needAnimTileItems.ForEach(a => a.PlayImageAnim(true));
			}
			else
			{
				if ((GetChooseTotalNum() + 1) < panelData.TileCellNum) return;
				//skill finger anim
				List<SkillItem> recordCanUseSkillItems = new List<SkillItem>();
				for (int i = 0; i < 4; i++)
				{
					if (skillList[i].IsCanShowFingerAnim && GameManager.PlayerData.GetItemNum(panelData.SkillItemList[i]) > 0)
					{
						recordCanUseSkillItems.Add(skillList[i]);
					}
				}
				if (recordCanUseSkillItems.Count == 0)
				{
					for (int i = 0; i < 4; i++)
					{
						if (skillList[i].IsCanShowFingerAnim)
						{
							recordCanUseSkillItems.Add(skillList[i]);
						}
					}
				}
				if (recordCanUseSkillItems.Count > 0)
				{
					recordSkillItemAnim = recordCanUseSkillItems[Random.Range(0, recordCanUseSkillItems.Count - 1)];
					recordSkillItemAnim.PlayFingerAnim(true);
				}
			}
		}
	}

	private CallBack<bool> callBack;

	private CallBack<bool> CallBack
	{
		get
		{
			if (callBack == null)
			{
				callBack = (b) =>
				{
					try
					{
						CkeckActiveTile(b);
					}
					catch (Exception e)
					{
						Log.Debug($"RecordLastTouchTime Error:{e.Message}");
					}
				};
			}
			return callBack;
		}
	}

	private void RecordLastTouchTime()
	{
		if (panelData == null) return;

		if (panelData.LevelNum == 1 && GameManager.UI.HasUIForm("GamePlayGuide")) return;

		if (panelData.LastInputTime == 0) panelData.LastInputTime = Time.realtimeSinceStartup;
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				panelData.LastInputTime = Time.realtimeSinceStartup;
				CallBack?.Invoke(false);
			}
			else if (Time.realtimeSinceStartup - panelData.LastInputTime > 10)
			{
				panelData.LastInputTime = Time.realtimeSinceStartup;
				CallBack?.Invoke(true);
			}
		}
		else
		{
			if (Input.touchCount > 0)
			{
				panelData.LastInputTime = Time.realtimeSinceStartup;
				CallBack?.Invoke(false);
			}
			else if (Time.realtimeSinceStartup - panelData.LastInputTime > 10)
			{
				panelData.LastInputTime = Time.realtimeSinceStartup;
				CallBack?.Invoke(true);
			}
		}
	}
	#endregion

	#region FlyItem

	public ReceiverType ReceiverType => ReceiverType.Common;
	public GameObject GetReceiverGameObject() => gameObject;
	private GameObject GetTargetGameObj(TotalItemData type)
	{
		if (type == TotalItemData.Prop_Back)
		{
			return skillList[0].gameObject;
		}
		else if (type == TotalItemData.Prop_ChangePos)
		{
			return skillList[1].gameObject;
		}
		else if (type == TotalItemData.Prop_Absorb)
		{
			return skillList[2].gameObject;
		}
		else if (type == TotalItemData.Prop_Grab)
		{
			return skillList[3].gameObject;
		}
		else if (type == TotalItemData.Prop_AddOneStep && !isAddOneStepState) 
        {
			return AddOneStep_Btn.gameObject;
        }
		else
		{
			return null;
		}
	}

	public Vector3 GetItemTargetPos(TotalItemData type)
	{
		var targetObj = GetTargetGameObj(type);
		if (targetObj != null)
		{
			return targetObj.transform.position;
		}
		else return new Vector3(0, -2f, 0);
	}

	private bool isRefresh = true;
	public void OnFlyEnd(TotalItemData type)
	{
		var targetObj = GetTargetGameObj(type);
		if (targetObj != null)
        {
			targetObj.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
			{
				targetObj.transform.localScale = Vector3.one;
			};
		}

		if (!isRefresh)
		{
			isRefresh = true;
			return;
		}

		if (type == TotalItemData.Prop_Back || type == TotalItemData.Prop_Grab || type == TotalItemData.Prop_Absorb || type == TotalItemData.Prop_ChangePos) 
		{
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].GetTotalItemType() == type)
                {
					ShowSkillItem(skillList[i], panelData.SkillItemList.IndexOf(type));
				}
            }
		}

        if (type == TotalItemData.Prop_AddOneStep)
        {
			RefreshAddOneStepButton();
        }
	}
	
	public void OnFlyHit(TotalItemData type) { }
	#endregion

	#region CommonEventCallBack
	private void CommonEventCallBack(object sender, GameEventArgs e)
	{
		CommonEventArgs ne = (CommonEventArgs)e;
		switch (ne.Type)
		{
			case CommonEventType.BuyPackageBannerToContinueLevel:
			case CommonEventType.UseCoinToAddThreeBack:
				GameManager.DataNode.SetData<int>("ContinueLevelCount", GameManager.DataNode.GetData<int>("ContinueLevelCount", 0) + 1);
				if (GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum) <= 0)
				{
					UseCoinContinueLevel();
					float delayTime=GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false)?0.72f:0f;
					GameManager.Task.AddDelayTriggerTask(delayTime, () =>
					{
						var skillItem = skillList.FirstOrDefault(s => s.GetTotalItemType() == TotalItemData.Prop_ChangePos);
						HandleSkillEvent(skillItem,TotalItemData.Prop_ChangePos,null,true);
					});
				}
				else
				{
					if (!timeBar.IsTimeOver || GetChooseTotalNum() >= panelData.TileCellNum)
					{
						UseCoinContinueLevel();
					}
					// 恢复倒计时
					timeBar.RefreshCountdown();
				}
				break;
			case CommonEventType.WatchAdsToAddThreeBack:
                if (GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum) > 0)// 判断是否是限时关
                {
					if (!timeBar.IsTimeOver || GetChooseTotalNum() >= panelData.TileCellNum)
					{
						UseCoinContinueLevel();
					}	
					// 恢复倒计时
					timeBar.RefreshCountdown();
                }
                else
                {
					UseCoinContinueLevel();
                }
                break;
            case CommonEventType.ChangeTileIconID:
				//change all tile item icon
				ChangeAllTileIcon();
				break;
            case CommonEventType.FreePlayPropSkill:
	            TotalItemData type=(TotalItemData)ne.UserDatas[0];
	            foreach (var skill in skillList)
	            {
		            if (skill.GetTotalItemType() == type)
		            {
			            HandleSkillEvent(skill,type,null,true);
		            }
	            }
	            break;
			case CommonEventType.AutoUsePropSkill:
				TotalItemData targetType = (TotalItemData)ne.UserDatas[0];
				foreach (var skill in skillList)
				{
					if (skill.GetTotalItemType() == targetType)
					{
						HandleSkillEvent(skill, targetType, () =>
						{
							ShowSkillItem(skill, panelData.SkillItemList.IndexOf(targetType));
						});
					}
				}
				break;
		}
	}

	private void RewardCallBack(object sender, GameEventArgs e)
	{
		RewardAdEarnedRewardEventArgs ne = e as RewardAdEarnedRewardEventArgs;
		if (ne != null && ne.UserData.ToString() == "AddOneStepByRV") 
		{
			ShowUseAddOneStepSkill();
		}
		else
		{
			int surplusNum = GetBackTotalNum() + GetMapTotalNum() + GetChooseTotalNum(true);

			if (CheckCanShowGameAdsPropButton(surplusNum))
			{
				DaliyWatchAds_Prefab.transform.localPosition = Vector3.zero;
				DaliyWatchAds_Prefab.transform.SetParent(DaliyWatchPos_Trans, false);
				DaliyWatchAds_Prefab.SetActive(true);
			}
			else
			{
				DaliyWatchAds_Prefab.SetActive(false);
			}
		}
	}

	public float GetFuuRate()
    {
        if (panelData != null)
        {
			int chooseNum = GetChooseTotalNum();
			int mapNum = GetMapTotalNum();
			int backNum = GetBackTotalNum();

			return (panelData.TileMatchData.TotalCount - chooseNum - mapNum - backNum) / (float)panelData.TileMatchData.TotalCount;
		}

		return 0;
    }

	private bool CheckCanShowGameAdsPropButton(int surplusNum)
	{
		if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
			return false;
	    
		int todayMaxTime = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.RV_Times_Props, 99);
		if (GameManager.PlayerData.TodayWatchPropsAdTime >= todayMaxTime)
		{
			return false;
		}

		return !panelData.IsWatchDaliyAds
			&& panelData.LevelNum >= Constant.GameConfig.UnlockGameAdsPropLevel
			&& GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Show_Level_PropAD, true)
			&& surplusNum <= (panelData.TileMatchData.TotalCount * 7 / 10)
			&& GameManager.Ads.CheckRewardedAdIsLoaded()
			&& GameManager.Ads.CheckRewardAdCanShow();
	}

	private void ChangeAllTileIcon()
	{
		var allTile = GetComponentsInChildren<TileItem>(true);
		foreach (var tile in allTile)
		{
			tile.SetImage();
		}
	}
	#endregion

	private void SaveToJson()
	{
		ShowLastBoxTip();
		RefreshSkillItemState();
	}

	public void ShowGamePlayGuide()
	{
		try
		{
			if (panelData.LevelNum != 1 || panelData.IsOldLevelData) return;
			var tileItems = TileMatchParent.GetComponentsInChildren<TileItem>().ToList();
			var targetId = tileItems[tileItems.Count - 1].Data.TileID;
			var targets = tileItems.FindAll(x => x.Data.TileID == targetId);
			if (targets != null && targets.Count >= 3) 
            {
				Vector3[] positions = new Vector3[targets.Count];
				float targetY = targets[0].transform.position.y;
				for (int i = 0; i < targets.Count; i++)
				{
					positions[i] = new Vector3(targets[i].transform.position.x, targetY, targets[i].transform.position.z);
				}
				GameManager.UI.ShowUIForm("GamePlayGuide",UIFormType.PopupUI,null, null, positions);
			}
		}
		catch(Exception e)
        {
			Log.Error("ShowGamePlayGuide fail - " + e.Message);
        }
	}

	private bool ShowFirstTryPanel(Action callback = null)
    {
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			return false;

		int level = GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel);
		//if (PlayerPrefs.GetInt(Constant.PlayerData.FirstPassFail + level, 0) != 0)
		//	return;

		int hardIndex = DTLevelUtil.GetLevelHard(level);
		if (hardIndex <= 0)
			return false;

		if (PlayerPrefs.GetInt("IsFirstTryPanelPopup_" + hardIndex, 0) == 1) 
			return false;
		PlayerPrefs.SetInt("IsFirstTryPanelPopup_" + hardIndex, 1);

		GameManager.Task.AddDelayTriggerTask(1.7f, () =>
		{
			GameManager.UI.ShowUIForm("GameFirstTryPanel",UIFormType.PopupUI,f =>
			{
				f.SetHideAction(callback);
			});
		});

		return true;
	}

	private bool ShowLevelSupportPackMenu(Action callback = null)
    {
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			return false;

		int level = GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel);
		if (level <= 200)
			return false;

		//氪金玩家才会弹出来
		if (!GameManager.Ads.IsRemovePopupAds)
			return false;

		int hardIndex = DTLevelUtil.GetLevelHard(level);
		if (hardIndex <= 0)
			return false;

		if (level == PlayerPrefs.GetInt("LevelSupportPackBuyedLevel", 0))
			return false;

		string menuName = hardIndex == 1 ? "HardLevelSupportPackMenu" : "SuperhardLevelSupportPackMenu";
		GameManager.UI.ShowUIForm(menuName,UIFormType.PopupUI, f =>
		{
			f.m_OnHideCompleteAction = callback;
		});

		return true;
	}

	#region daliyWatchAds

	public List<TotalItemData> GetCanUseSkills()
	{
		List<TotalItemData> list = new List<TotalItemData>();
		if (!panelData.IsUseAddSkill)
		{
			list.Add(TotalItemData.Prop_AddOneStep);
		}

		for (int i = 0; i < panelData.SkillItemList.Count; i++)
		{
			if (skillList[i].IsCanUse())
			{
				list.Add(panelData.SkillItemList[i]);
			}
		}

		while (list.Count > 3)
		{
			list.RemoveAt(Random.Range(0,list.Count));
		}
        
		return list;
	}

	public void GetSkillAction(TotalItemData type)
	{
		if (type == TotalItemData.Prop_AddOneStep)
		{
			IsAddOneStepState = true;
			RefreshChosenBar(true);
			//ShowAddOneStepSkill();
		}
		else
		{
			int index = panelData.SkillItemList.IndexOf(type);
			
			HandleSkillEvent(skillList[index],type, () =>
			{
				ShowSkillItem(skillList[index], index);
			},true);
		}
	}

	public void RecordWatchAds()
	{
		panelData.IsWatchDaliyAds = true;
		SaveToJson();
	}

	#endregion

	#region show skill guide

	private const int Prop_Back_UnlockLevel = 5;
	private const int Prop_AddOneStep_UnlockLevel = -1;
	private const int Prop_ChangePos_UnlockLevel = 9;
	private const int Prop_Absorb_UnlockLevel = 13;
	private const int Prop_Grab_UnlockLevel = 20;

	public bool IsShowTileAnim()
	{
		Dictionary<int, TotalItemData> skillUnlockDict = new Dictionary<int, TotalItemData>()
		{
			{Prop_Back_UnlockLevel,TotalItemData.Prop_Back},
			{Prop_AddOneStep_UnlockLevel,TotalItemData.Prop_AddOneStep},
			{Prop_ChangePos_UnlockLevel,TotalItemData.Prop_ChangePos},
			{Prop_Absorb_UnlockLevel,TotalItemData.Prop_Absorb},
			{Prop_Grab_UnlockLevel,TotalItemData.Prop_Grab},
		};
		bool isShowTileAnim = true;
		int nowLevel = GameManager.PlayerData.NowLevel;
		switch (nowLevel)
		{
			case Prop_Back_UnlockLevel:
			case Prop_AddOneStep_UnlockLevel:
			case Prop_ChangePos_UnlockLevel:
			case Prop_Absorb_UnlockLevel:
			case Prop_Grab_UnlockLevel:
				if (!GameManager.PlayerData.IsSkillUnlock(skillUnlockDict[nowLevel].TotalItemType))
				{
					isShowTileAnim = false;
				}
				break;
		}

		return isShowTileAnim;
	}

	private bool RefreshSkillUnlockData(int nowLevel)
	{
		bool isCalendarChallenge = GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge;
		List<TotalItemType> hasUnlockSkills = new List<TotalItemType>(4);
		
		if(nowLevel > Prop_Back_UnlockLevel || isCalendarChallenge)
			hasUnlockSkills.Add(TotalItemData.Prop_Back.TotalItemType);
		if(nowLevel > Prop_ChangePos_UnlockLevel || isCalendarChallenge)
			hasUnlockSkills.Add(TotalItemData.Prop_ChangePos.TotalItemType);
		if(nowLevel > Prop_Absorb_UnlockLevel || isCalendarChallenge)
			hasUnlockSkills.Add(TotalItemData.Prop_Absorb.TotalItemType);
		if(nowLevel > Prop_Grab_UnlockLevel || isCalendarChallenge)
			hasUnlockSkills.Add(TotalItemData.Prop_Grab.TotalItemType);

		if (isCalendarChallenge)
		{
			return false;
		}
		PlayerBehaviorModel.Instance.RecordSkillUnlock(hasUnlockSkills.ToArray());

		if (nowLevel == Prop_Back_UnlockLevel && !GameManager.PlayerData.IsSkillUnlock(TotalItemData.Prop_Back.TotalItemType)) 
		{
			ShowUnlockPanel(TotalItemData.Prop_Back);
			return true;
		}
		if (nowLevel == Prop_ChangePos_UnlockLevel && !GameManager.PlayerData.IsSkillUnlock(TotalItemData.Prop_ChangePos.TotalItemType)) 
		{
			ShowUnlockPanel(TotalItemData.Prop_ChangePos);
			return true;
		}
		if (nowLevel == Prop_Absorb_UnlockLevel && !GameManager.PlayerData.IsSkillUnlock(TotalItemData.Prop_Absorb.TotalItemType)) 
		{
			ShowUnlockPanel(TotalItemData.Prop_Absorb);
			return true;
		}
		if (nowLevel == Prop_Grab_UnlockLevel && !GameManager.PlayerData.IsSkillUnlock(TotalItemData.Prop_Grab.TotalItemType)) 
		{
			ShowUnlockPanel(TotalItemData.Prop_Grab);
			return true;
		}

		return false;
	}

	public void ShowUnlockPanel(TotalItemData type)
	{
		RewardManager.Instance.AddNeedGetReward(type,3);
		if (RewardManager.Instance.RewardArea)
		{
			RewardManager.Instance.RewardArea.GetRewardWorldPositionFunc = (a) => { return Vector3.up*0.21f;};
		}

		GameManager.Task.AddDelayTriggerTask(0.4f, () =>
		{
			GameManager.PlayerData.RecordSkillUnlock(type.TotalItemType);
		});
		isRefresh = false;
		CanvasGroup.blocksRaycasts = false;
		GameManager.Task.AddDelayTriggerTask(4, () =>
		{
			CanvasGroup.blocksRaycasts = true;
		});
		GameManager.Task.AddDelayTriggerTask(0.5f, () =>
		{
            //if (type == TotalItemData.Prop_Grab)
            //{
            //    //找三个亮的放到choose栏
            //    List<TileItem> listItem = new List<TileItem>();
            //    List<int> ids = new List<int>();
            //    foreach (var layer in tileMapDict)
            //    {
            //        foreach (var map in layer.Value)
            //        {
            //            if (listItem.Count < 3 && !map.Value.Item2.IsBeCover)
            //            {
            //                if (ids.Count == 2 && ids[0] == map.Value.Item2.Data.TileID && ids[1] == map.Value.Item2.Data.TileID)
            //                {
            //                    continue;
            //                }
            //                ids.Add(map.Value.Item2.Data.TileID);
            //                listItem.Add(map.Value.Item2);
            //            }
            //        }

            //        if (listItem.Count >= 3) break;
            //    }

            //    foreach (var tile in listItem)
            //    {
            //        OnTileClick(tile, type, false, false);
            //    }
            //}

            if (type == TotalItemData.Prop_Back) 
			{
				TileItem tileItem=null;
				foreach (var layer in tileMapDict)
				{
					foreach (var map in layer.Value)
					{
						if (map.Value.Item2 != null)
						{
							tileItem = map.Value.Item2;
							break;
						}
					}

					if (tileItem == null) break;
				}
				OnTileClick(tileItem, type, false, false);
			}
		});
		isShowingRewardAnim = true;
		RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.SkillUnlockRewardPanel, false, () =>
		{
			isShowingRewardAnim = false;
			//执行技能解锁动画 
			SkillUnlockAnim(type);
		});
	}

	public void SkillUnlockAnim(TotalItemData type)
	{
		var skillItem = skillList.FirstOrDefault(s => s.GetTotalItemType() == type);
		if (skillItem != null)
		{
			GuideType guideType = GuideType.Skill_Back;
			if (type == TotalItemData.Prop_Back)
			{
				guideType = GuideType.Skill_Back;
			}
			else if (type == TotalItemData.Prop_ChangePos)
			{
				guideType = GuideType.Skill_ChangePos;
			}
			else if (type == TotalItemData.Prop_Absorb)
			{
				guideType = GuideType.Skill_Absorb;
			}
			else if (type == TotalItemData.Prop_Grab)
			{
				guideType = GuideType.Skill_Grab;
			}

			skillFree = (type, true);
			Action unLockFinishAction = () =>
			{
				if ((guideType == GuideType.Skill_Grab|| guideType==GuideType.Skill_Back)
				    &&GetChooseTotalNum() <= 0)
				{
					CanvasGroup.blocksRaycasts = true;
					return;
				}

				CommonGuideMenuUtil.ShowSkillGuide(guideType, skillItem.GuideButton, () =>
				{
					CanvasGroup.blocksRaycasts = true;
				}, () =>
				 {
					 float time = guideType == GuideType.Skill_Absorb ? 4f : 2f;
					 GameManager.Task.AddDelayTriggerTask(time, () =>
					 {
						 var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
						 if (panel != null)
						 {
							 panel.ElementGuideFinish?.Invoke();
							 panel.ElementGuideFinish = null;
						 }
					 });
				 });
				skillItem.SetUnlockEvent();
			};
			skillItem.ShowSkillUnlockAnim(unLockFinishAction);
		}
	}

	private bool ShowElementUnlockPanel(int nowLevel)
    {
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge) 
			return false;

		Dictionary<int, int> elementUnlockDict = new Dictionary<int, int>()
		{
			{Constant.GameConfig.UnlockElementIceLevel,1},
			{Constant.GameConfig.UnlockElementGlueLevel,2},
			{Constant.GameConfig.UnlockElementFireworksLevel,20},
			{Constant.GameConfig.UnlockElementCurtainLevel,6},
		};

		if (CommonModel.Instance.Data.NewPlayerForGoldCollection && DTLevelUtil.IsSpecialGoldTile(GameManager.PlayerData.RealLevel()) && !elementUnlockDict.ContainsKey(nowLevel))
		{
			elementUnlockDict.Add(nowLevel,3);
			CommonModel.Instance.SetNewPlayerForGoldCollection(false);
		}

		//if (nowLevel == 81 && ((int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Use_Level_Type_Index, 0) == 2 || PlayerPrefs.GetString("RecordLevelPathName", string.Empty) == "Level_B")) 
  //      {
		//	elementUnlockDict.Add(nowLevel, 6);
		//}

		foreach (var data in elementUnlockDict)
		{
			if (nowLevel == data.Key)
            {
				CanvasGroup.blocksRaycasts = false;
				GameManager.Task.AddDelayTriggerTask(2f, () =>
				{
					GameManager.UI.ShowUIForm("ElementUnlockPanel",UIFormType.PopupUI,form =>
					{
						CanvasGroup.blocksRaycasts = true;
					}, () =>
					{
						CanvasGroup.blocksRaycasts = true;
					}, data.Value);
				});

				return true;
			}
		}

		return false;
    }

	private int NeedCoinNumBuyProp(TotalItemData type)
	{
		if (type == TotalItemData.Prop_AddOneStep)
		{
			return 300;
		}
		else if (type == TotalItemData.Prop_Back)
		{
			return 200;
		}
		else
		{
			return 500;
		}
	}

	#endregion

	#region
	private int recordTaskId = -1;
	private bool isShowingRewardAnim = false;
    public void OnApplicationPause(bool isPause)
    {
	    if (!isPause && !isShowingRewardAnim && !SystemInfoManager.IsSuperLowMemorySize)  
		{
			GameManager.Task.RemoveDelayTriggerTask(recordTaskId);
			recordTaskId=GameManager.Task.AddDelayTriggerTask(0.06f,() =>
			{
				if (isShowingRewardAnim) return;
				if (GameManager.CurState != null)
				{
					GameManager.CurState = null;
					return;
				}
				if(!RewardManager.Instance.CheckLoadComplete())
					return;

				if (!GameManager.UI.IsHasFormInGroup("GuideUI")
					&& !GameManager.UI.IsHasFormInGroup("PopupUI")
					&& !GameManager.UI.IsHasFormInGroup("TopUI"))
				{
					GameManager.UI.ShowUIForm("GameSettingPanel",UIFormType.PopupUI,null, null, "ShowByFocus");
				}
			});
		}
	}
	#endregion
	
	#region LevelTime

	public bool IsTimeLimitLevel()
	{
		return GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelLimitTime(panelData.LevelNum) > 0;
	}
	#endregion
	
#if UNITY_EDITOR
	/// 编辑器使用函数
	public void StartEditorTimer()
	{
		timeBar.EditorTimer();
	}
#endif

	#region WellDone

	private void ShowWellDone(int levelNum,Vector3 starVector)
	{
		GameManager.Ads.ShowInterstitialAd(() =>
		{
			int levelType = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelModelType(GameManager.PlayerData.RealLevel(GameManager.PlayerData.NowLevel - 1));
			LevelType levelData = GameManager.DataTable.GetDataTable<DTLevelTypeID>().Data.GetLevelTypeData(levelType);
			GameManager.UI.ShowUIForm("GameWellDonePanel",UIFormType.PopupUI,(u) =>
			{
				GameManager.Sound.PlayAudio(SoundType.WIN.ToString());
			}, userData: new
			{
				coinNum = levelData.CoinNum,
				isAdsBtn = levelData.ADButton == 1,
				coinsTimesNum = levelData.CoinsTimesNum,
				curLevel = levelNum,
				nowLevel = GameManager.PlayerData.NowLevel,
				starVector = starVector,
				starNum = 1,
			});
		});
	}

	#endregion

	#region PkGame
	
	private void StartGameProcess()
	{
		GameManager.Process.Register("",1, () => { });
		GameManager.Process.Register("",2, () => { });
		GameManager.Process.Register("",3, () => { });
		GameManager.Process.Register("",4, () => { });
	}

	//pk赛 进场动画
	private bool ShowPkStart()
	{
		// //处于活动中
		if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge
		    &&PkGameModel.Instance.RecordEnterGameStatus==PkGameStatus.Playing)
		{
			//GameManager.UI.ShowUIForm("PkReadyGoPanel");
			GameManager.Task.AddDelayTriggerTask(1f, () =>
			{
				ShowPkRewardPanelProcess();
			});
			return false;
		}
		return false;
	}
	//pk赛进程之后的奖励动画
	private void ShowPkRewardPanelProcess()
	{
		UnityUtility.InstantiateAsync("PkStartGameRewardPanel", Top, obj =>
		{
			if (obj != null)
			{
				UIForm form = obj.GetComponent<PkStartGameRewardPanel>();
				if (form != null)
				{
					form.OnInit(null);

					GameManager.Task.AddDelayTriggerTask(5, () =>
					{
						UnityUtility.UnloadInstance(obj);
					});
				}
			}
		});
	}

	private bool ShowPkWinPanel(Action finish)
	{
		if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge
		    && PkGameModel.Instance.RecordEnterGameStatus == PkGameStatus.Playing)
		{
			ShowPkGameWin(finishAction:finish);
			return true;
		}
		else
		{
			finish?.InvokeSafely();
		}
		return false;
	}
	
	private void ShowPkGameWin(Action finishAction)
	{
		if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge
		    && PkGameModel.Instance.RecordEnterGameStatus==PkGameStatus.Playing)
		{
			//胜利数据处理 返回一个奖励数量
			PkGameModel.Instance.Win(out int rewardNum);
			//获取服务器敌方信息
			PkGameModel.Instance.GetTargetDataByService(null,0f);
			PkGameModel.Instance.IsShowGameOverGuideText(rewardNum,out int textCode);
			//奖励下发
			RewardManager.Instance.AddNeedGetReward(TotalItemData.Pk,rewardNum);
			RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.PkGameRewardPanel,false, () =>
			{
				GameManager.Task.AddDelayTriggerTask(textCode>0?1.8f:1f, () =>
				{
					finishAction?.InvokeSafely();
					finishAction = null;
				});
			}, () =>
			{
				GameManager.UI.ShowUIForm("PkGameShowPkScorePanel",UIFormType.PopupUI,userData:textCode);
			});
		}else finishAction.InvokeSafely();
	}

	#endregion

	#region RV
	private void RefreshRv(object sender, GameEventArgs e)
	{
		RefreshAddOneStepButton();
	}
	#endregion
	
	#region 新大拇指规则
	
	//展示大拇指特效
	private void ShowThumbEffect(Vector3 pos)
	{
		//是否展示
		if(!If_Have_Thumb)return;
		
		GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
			"GoodJobEffect",
			"TileItemDestroyEffectPool",
			2f,
			pos,
			Quaternion.identity,
			parent: Prop_Parent, (obj) =>
			{
				var skeleton = (obj.Target as GameObject).GetComponentInChildren<SkeletonGraphic>();
				skeleton.AnimationState.SetAnimation(0, "idle", loop: false);
			});
	}

	private void ClickTileItemEvent(TileItem item, Action clickTileItemAction)
	{
        int maxCount = panelData.TileCellNum;

	    // -------- 点击前：获取当前状态（不要从 list 中移除 item） --------
	    List<TileItem> listBefore = ListPool<TileItem>.Get();
	    List<int> backListBefore = ListPool<int>.Get();
	    List<TileItem> chooseBefore = ListPool<TileItem>.Get();

	    GetMapList(ref chooseBefore, ref listBefore, ref backListBefore);

	    // beforeSet 要基于真实的候选池（包括当前这颗可点的棋子）
	    var beforeSet = TileLogic.GetEliminatableIds(chooseBefore, listBefore, maxCount);

	    // 记录点击前选中栏中有哪些花色（只需要 id 集合，用于后面比较）
	    var chooseIdsBefore = HashSetPool<int>.Get();
	    for (int i = 0; i < chooseBefore.Count; i++)
	        chooseIdsBefore.Add(chooseBefore[i].Data.TileID);

	    // 释放临时 list（我们已把需要的信息拷贝出来）
	    ListPool<TileItem>.Release(chooseBefore);
	    ListPool<TileItem>.Release(listBefore);
	    ListPool<int>.Release(backListBefore);

	    // -------- 执行点击动作（UI/逻辑会刷新候选池/选中栏） --------
	    clickTileItemAction?.InvokeSafely();

	    // -------- 点击后：再次读表、计算 afterSet（此时候选池应已刷新，不应包含已选入的 item） --------
	    List<TileItem> listAfter = ListPool<TileItem>.Get();
	    List<int> backListAfter = ListPool<int>.Get();
	    List<TileItem> chooseAfter = ListPool<TileItem>.Get();

	    GetMapList(ref chooseAfter, ref listAfter, ref backListAfter);

	    // 防御性：确保候选池里没有尚已被选入的 item（通常 GetMapList 已处理，但加一行保险）
	    if (listAfter.Contains(item))
	        listAfter.Remove(item);

	    var afterSet = TileLogic.GetEliminatableIds(chooseAfter, listAfter, maxCount);

	    // 如果点击本身在选中栏里造成了即时消除（例如 chooseAfter 中这个 id >=3），
	    // 则把该花色从 afterSet 移除（不作为“点击后使原有花色可消”的证据）
	    int clickedId = item.Data.TileID;
	    int countInChooseAfter = 0;
	    for (int i = 0; i < chooseAfter.Count; i++)
	    {
	        if (chooseAfter[i].Data.TileID == clickedId) countInChooseAfter++;
	    }
	    if (countInChooseAfter >= 3 && afterSet.Contains(clickedId))
	    {
	        afterSet.Remove(clickedId);
	    }

	    // -------- 比较：只关注点击前就在选中栏的花色是否从不可消 -> 可消 --------
	    bool shouldShowThumb = false;
	    foreach (var id in chooseIdsBefore)
	    {
	        bool wasEliminatableBefore = beforeSet.Contains(id);
	        bool isEliminatableAfter = afterSet.Contains(id);
	        if (!wasEliminatableBefore && isEliminatableAfter)
	        {
	            shouldShowThumb = true;
	            break;
	        }
	    }

	    if (shouldShowThumb)
	    {
	        ShowThumbEffect(item.transform.position);
	    }

	    // -------- 清理池 --------
	    HashSetPool<int>.Release(chooseIdsBefore);
	    HashSetPool<int>.Release(beforeSet);
	    HashSetPool<int>.Release(afterSet);

	    ListPool<TileItem>.Release(chooseAfter);
	    ListPool<TileItem>.Release(listAfter);
	    ListPool<int>.Release(backListAfter);
	}



	private void GetMapList(ref List<TileItem> chooseList,ref List<TileItem> list,ref List<int> backList)
	{
		//获取所有高亮的棋子，【包括上方棋盘、回退栏中棋子】
		//1、回退栏
		for (int i = backTileDict.Count - 1; i >= 0; i--)
		{
			int pos = backTileDict.ElementAt(i).Value;
			if (!backList.Contains(pos))
			{
				backList.Add(pos);
				list.Add(backTileDict.ElementAt(i).Key);
			}
		}
		//2、棋盘
		foreach (var layer in tileMapDict)
		{
			foreach (var map in layer.Value)
			{
				if (!map.Value.Item2.IsBeCover)
				{
					list.Add(map.Value.Item2);
				}
			}
		}
		//3、储存栏
		foreach (var value in chooseItemDict.Values)
		{
			chooseList.AddRange(value.Value);
		}
	}

	public static class TileLogic
	{
		struct CountData
		{
			public int Total;
			public int InChoose;
			public int InList;
		}

		/// <summary>
		/// 计算在当前候选池下，哪些花色能消除（受剩余空格限制）
		/// </summary>
		public static HashSet<int> GetEliminatableIds(List<TileItem> chooseList, List<TileItem> candidateList, int maxCount)
		{
			int space = maxCount - chooseList.Count;

			var dict = DictionaryPool<int, CountData>.Get();
			var result = HashSetPool<int>.Get();

			// chooseList
			for (int i = 0; i < chooseList.Count; i++)
			{
				int id = chooseList[i].Data.TileID;
				if (!dict.TryGetValue(id, out var entry)) entry = new CountData();
				entry.Total++;
				entry.InChoose++;
				dict[id] = entry;
			}

			// candidateList
			for (int i = 0; i < candidateList.Count; i++)
			{
				int id = candidateList[i].Data.TileID;
				if (!dict.TryGetValue(id, out var entry)) entry = new CountData();
				entry.Total++;
				entry.InList++;
				dict[id] = entry;
			}

			foreach (var kv in dict)
			{
				var data = kv.Value;
				if (data.Total >= 3 && data.InList <= space)
					result.Add(kv.Key);
			}

			DictionaryPool<int, CountData>.Release(dict);
			return result;
		}
	}

	#endregion

	#region 新的看广告道具礼包
	
	private float watchAdsItemTimer = 10f;
	
	public void ShowWatchAdsItemGiftPack()
	{
		if (GameManager.Process.Count > 0) return;
		bool isCanShow = IsCanShowWatchAdsItemGiftPack();

		if (isCanShow)
		{
			TotalItemType type = GetWatchAdsItemType();
			DaliyWatchAdsPrefabNew.SetActive(true, type, WatchAdsItemGiftPackCallBack);
		}
	}

	private TotalItemType GetWatchAdsItemType()
	{
		//默认是磁铁
		TotalItemType type;
		//是否消除槽有相同棋子
		bool isHaveSameTileByChoose = IsHaveSameTileByChoose();
		if (isHaveSameTileByChoose)
		{
			type = TotalItemType.Prop_ChangePos;
		}
		else
		{
			//没有相同棋子的情况下需要随机
			List<TotalItemType> list = new List<TotalItemType>()
			{
				TotalItemType.Prop_Back,
				TotalItemType.Prop_Grab,
				TotalItemType.Prop_Absorb,
				TotalItemType.Prop_AddOneStep,
			};
			if (panelData.IsUseAddSkill) list.Remove(TotalItemType.Prop_AddOneStep);

			type = list[UnityEngine.Random.Range(0, list.Count)];
		}

		return type;
	}

	public List<TotalItemData> GetCanUseSkillsNew()
	{
		List<TotalItemData> list = new List<TotalItemData>();
		if (!panelData.IsUseAddSkill)
		{
			list.Add(TotalItemData.Prop_AddOneStep);
		}

		for (int i = 0; i < panelData.SkillItemList.Count; i++)
		{
			if (skillList[i].IsCanUse())
			{
				list.Add(panelData.SkillItemList[i]);
			}
		}
        
		return list;
	}
	
	private void WatchAdsItemGiftPackCallBack(TotalItemType type)
	{
		//判断能否使用itemdata，如果不能使用技能，则给道具奖励，能使用，直接使用道具
		TotalItemData itemData = TotalItemData.FromInt((int)type);
		var canUseSkillList = GetCanUseSkillsNew();
		bool isCanUseSkill = canUseSkillList.Contains(itemData);

		//如果不能用，发放道具奖励
		if (!isCanUseSkill)
		{
			RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, new Dictionary<TotalItemData, int>
			{
				{ itemData, 1 },
			}, true, null);
		}
		else
		{
			GetSkillAction(itemData);
		}

		RecordWatchAds();
	}

	//是否能展示道具礼包
	private bool IsCanShowWatchAdsItemGiftPack()
	{
		return !panelData.IsWatchDaliyAds
		       && GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false)
		       && panelData.LevelNum >= 22
		       && GameManager.Ads.CheckRewardedAdIsLoaded();
	}

	#endregion
}
