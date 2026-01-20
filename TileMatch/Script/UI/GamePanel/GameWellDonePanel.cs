using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using System.Linq;
using Firebase.Analytics;

public sealed class GameWellDonePanel : PopupMenuForm, IItemFlyReceiver
{
	[SerializeField]
	private GameWellDonePanel_RewardsArea RewardsArea;
	[SerializeField]
	private TextMeshProUGUI Multiple_Text;
	[SerializeField]
	private DelayButton AdsFreeCoin_Btn, HomeSmall_Btn, Home_Btn, Reward_Btn;
	[SerializeField]
	private TextMeshProUGUILocalize AdsCoins_Text, HomeSmall_Text, Home_Text;
	[SerializeField]
	private Image Progress_Image, BGSmall_Image;
	[SerializeField]
	private TextMeshProUGUI Progress_Text;
	[SerializeField]
	private GameObject Progress_Obj, NewBgTip, BgReward;
	[SerializeField]
	private ItemPromptBox RewardPreviewBox;
	[SerializeField]
	private GameObject[] Titles;
	[SerializeField]
	private CanvasGroup[] CanvasGroups;
	[SerializeField]
	private GameObject[] StarGroups;
	[SerializeField]
	private CanvasGroup[] TargetStars;
	[SerializeField]
	private GameObject HardFirstTryBanner, SuperHardFirstTryBanner;

	[SerializeField]
	[Range(50, 690)]
	private float ProgressImageInterval;

	[SerializeField]
	private CanvasGroup FirstWinIcon;

	#region 连胜爬藤的变量
	[SerializeField]
	private TextMeshProUGUI currentWinStreakText;

	[SerializeField] private Image winStreakImage;
	#endregion

	int curLevel = 0;
	int nowLevel = 0;
	int recordCoinNum;
	int coinMultiple = 6;
	bool isAdBtn = false;
	Vector3 starVector;
	//Vector3 starTargetPos;

	public Vector3 RewardBtnPos => Reward_Btn.transform.position;

	// protected override void Awake()
	// {
	// 	starTargetPos = CanvasGroups[0].transform.localPosition;
	// 	base.Awake();
	// }

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		RewardManager.Instance.RegisterItemFlyReceiver(this);

		if (userData != null)
		{
			try
			{
				var data = userData.ChangeType(
					new {
						coinNum = 0,
						isAdsBtn = false,
						coinsTimesNum = 1,
						curLevel = 0,
						nowLevel = 0,
						starVector = Vector3.zero,
						starNum=1,
					});

				recordCoinNum = data.coinNum;
				isAdBtn = data.isAdsBtn;
				coinMultiple = data.coinsTimesNum;
				curLevel = data.curLevel;
				nowLevel = data.nowLevel;
				starVector = data.starVector;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}

			RewardsArea.Initialize(recordCoinNum);
			ShowAnim();

			Init(GameManager.Ads.CheckRewardedAdIsLoaded());
		}
		base.OnInit(uiGroup, completeAction, userData);
	}

	public void Init(bool isShowAdsBtn)
	{
		HomeSmall_Btn.gameObject.SetActive(true);
		var hardIndex = DTLevelUtil.GetLevelHard(curLevel);
		bool isShowAdsFreeCoinBtn = isAdBtn && isShowAdsBtn && recordCoinNum > 0;

		AdsFreeCoin_Btn.gameObject.SetActive(isShowAdsFreeCoinBtn);
		Multiple_Text.text = $"x{coinMultiple}";
		AdsCoins_Text.SetParameterValue("{0}", $"{coinMultiple * recordCoinNum}", true);
		if(GameManager.PlayerData.NowLevel > (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Win_Direct_Next_End_Level, 5))
        {
			HomeSmall_Text.SetTerm("Common.Home");
			Home_Text.SetTerm("Common.Home");
		}
        else
        {
			HomeSmall_Text.SetTerm("Common.Next");
			Home_Text.SetTerm("Common.Next");
		}

		Home_Btn.gameObject.SetActive(!AdsFreeCoin_Btn.gameObject.activeSelf);
		HomeSmall_Btn.gameObject.SetActive(AdsFreeCoin_Btn.gameObject.activeSelf);
		BtnEvent(isShowAdsBtn);

		if (isShowAdsFreeCoinBtn)
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Bonus_Reward_Show,
				new Parameter("Level", curLevel));

		NewBgTip.SetActive(false);
		DTLevelReward levelRewardData = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
		LevelRewardData nextUnLockLevel = levelRewardData.GetNextUnlockLevelReward(curLevel);
        if (nextUnLockLevel != null)
        {
			List<ItemData> rewardDatas = nextUnLockLevel.GetRewardDatas();
			if (rewardDatas.Count > 1)
			{
				for (int i = 0; i < rewardDatas.Count; i++)
				{
					if (rewardDatas[i].type.TotalItemType == TotalItemType.Item_BgID)
					{
						NewBgTip.SetActive(true);
						break;
					}
				}
			}
		}

        for (int i = 0; i < Titles.Length; i++)
        {
			Titles[i].SetActive(i == hardIndex);
        }
	}

    public override void OnReset()
    {
		RewardManager.Instance.UnregisterItemFlyReceiver(this);

		base.OnReset();
    }

    public override void OnRelease()
	{
		//WellDoneTaskControl.ClearAnim();

		RewardsArea.Release();

		if (BgReward.activeSelf && BGSmall_Image.sprite != null) 
        {
			AddressableUtils.ReleaseAsset<Sprite>(BGSmall_Image.sprite);
        }

		base.OnRelease();
	}

    private void Update()
	{
#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(0))
#else
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
		{
			RewardPreviewBox.HidePromptBox();
		}
	}

	private Sequence sequence;
	private void ShowAnim()
	{
		try
		{
			//star ->bg->title->effect->coin->firstTry->progress->btn
			if (sequence != null) sequence.Kill();

			AdsFreeCoin_Btn.interactable = false;
			HomeSmall_Btn.interactable = false;
			Home_Btn.interactable = false;
			Reward_Btn.interactable = false;

			int hardIndex = DTLevelUtil.GetLevelHard(curLevel);
			if (hardIndex > 0)
            {
				if (hardIndex == 1)
					ShowTwoStarAnim();
				else
					ShowThreeStarAnim();
			}
			else
            {
				ShowOneStarAnim();
			}

			ShowCoinAnim();
			ShowFirstTryAnim();
			bool isGetReward = ShowGetProgressRewardAnim();
			ShowButtonAnim();

			sequence.AppendCallback(() =>
			{
				AdsFreeCoin_Btn.interactable = true;
				HomeSmall_Btn.interactable = true;
				Home_Btn.interactable = true;
				Reward_Btn.interactable = true;
				//if (!isGetReward) WellDoneTaskControl.ShowAnim();
			});
		}
		catch (Exception e)
		{
			OnHomeButtonClick();
			Log.Error("GameWellDonePanel ShowAnim error:{0}", e.Message);
		}
	}

	private void ShowOneStarAnim()
    {
		for (int i = 0; i < StarGroups.Length; i++)
		{
			StarGroups[i].SetActive(i == 0);
		}

		// Vector3 starTargetPos = CanvasGroups[0].transform.localPosition;
		Vector3 starTargetPos = new Vector3(0, 272, 0);
		
		//CanvasGroups.ToList().ForEach(o => o.alpha = 0);
		for (int i = 0; i < CanvasGroups.Length; i++)
		{
			CanvasGroups[i].alpha = 0;
		}

		Transform cachedStarTrans = CanvasGroups[0].transform;
		cachedStarTrans.position = starVector;
		cachedStarTrans.localEulerAngles = new Vector3(0, 0, 90);
		cachedStarTrans.localScale = Vector3.zero * 0.3f;
		CanvasGroups[0].gameObject.SetActive(true);

		sequence = DOTween.Sequence()
			.Append(CanvasGroups[0].DOFade(1, 0f))
			.Join(CanvasGroups[1].DOFade(1, 0.2f))
			.Join(cachedStarTrans.DOLocalMove(starTargetPos, 0.6f).SetEase(Ease.OutBack))
			.Join(cachedStarTrans.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
			.Join(cachedStarTrans.DOScale(Vector3.one * 1.3f, 0.6f).SetEase(Ease.InOutSine))
			.Append(cachedStarTrans.DOScale(0.9f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() => CanvasGroups[0].transform.DOScale(1, 0.15f).SetEase(Ease.InOutSine)))
			.AppendCallback(ShowEffect)
			.Append(CanvasGroups[2].DOFade(1, 0.2f));
		sequence.OnKill(() => sequence = null);
		
		//播放胜利后，显示当前连胜爬藤活动的层数变化
		PlayWinStreakAnim();
	}

	private void ShowTwoStarAnim()
    {
        for (int i = 0; i < StarGroups.Length; i++)
        {
			StarGroups[i].SetActive(i == 1);
		}
		HardFirstTryBanner.transform.localScale = Vector3.zero;

		CanvasGroups.ToList().ForEach(o => o.alpha = 0);
        for (int i = 0; i < 2; i++)
        {
			TargetStars[i].transform.position = starVector;
			TargetStars[i].transform.localEulerAngles = new Vector3(0, 0, 90);
			TargetStars[i].transform.localScale = Vector3.zero;
			TargetStars[i].gameObject.SetActive(true);
		}
		float delayTime = 0.06f;

		sequence = DOTween.Sequence()
			.Append(CanvasGroups[1].DOFade(1, 0.2f))
			.Join(TargetStars[0].transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.OutBack))
			.Join(TargetStars[0].transform.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
			.Join(TargetStars[0].transform.DOScale(Vector3.one * 1.3f, 0.6f).SetEase(Ease.InOutSine).OnComplete(() =>
			{
				TargetStars[0].transform.DOScale(0.9f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() => TargetStars[0].transform.DOScale(1, 0.15f).SetEase(Ease.InOutSine));
			}))
			.Join(TargetStars[1].transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.OutBack).SetDelay(delayTime))
			.Join(TargetStars[1].transform.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetDelay(delayTime))
			.Join(TargetStars[1].transform.DOScale(Vector3.one * 1.15f, 0.5f).SetEase(Ease.InOutSine).SetDelay(delayTime).OnComplete(() =>
			{
				TargetStars[1].transform.DOScale(0.9f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() => TargetStars[1].transform.DOScale(1, 0.15f).SetEase(Ease.InOutSine));
			}))
			.AppendCallback(ShowEffect)
			.Append(CanvasGroups[2].DOFade(1, 0.2f))
			.Join(HardFirstTryBanner.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack));
		sequence.OnKill(() => sequence = null);
		
		//播放胜利后，显示当前连胜爬藤活动的层数变化
		PlayWinStreakAnim();
	}

	private void ShowThreeStarAnim()
    {
		for (int i = 0; i < StarGroups.Length; i++)
		{
			StarGroups[i].SetActive(i == 2);
		}
		SuperHardFirstTryBanner.transform.localScale = Vector3.zero;

		CanvasGroups.ToList().ForEach(o => o.alpha = 0);
		for (int i = 2; i < 5; i++)
		{
			TargetStars[i].transform.position = starVector;
			TargetStars[i].transform.localPosition = new Vector3(0, TargetStars[i].transform.localPosition.y, 0);
			TargetStars[i].transform.localEulerAngles = new Vector3(0, 0, 90);
			TargetStars[i].transform.localScale = Vector3.zero;
			TargetStars[i].gameObject.SetActive(true);
		}
		float delayTime = 0.07f;
		float delayTime2 = 0.05f;

		sequence = DOTween.Sequence()
			.Append(CanvasGroups[1].DOFade(1, 0.2f))
			.Join(TargetStars[2].transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.OutBack))
			.Join(TargetStars[2].transform.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
			.Join(TargetStars[2].transform.DOScale(Vector3.one * 1.3f, 0.6f).SetEase(Ease.InOutSine).OnComplete(() =>
			{
				TargetStars[2].transform.DOScale(0.9f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() => TargetStars[2].transform.DOScale(1, 0.15f).SetEase(Ease.InOutSine));
			}))
			.Join(TargetStars[3].transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.OutBack).SetDelay(delayTime))
			.Join(TargetStars[3].transform.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetDelay(delayTime))
			.Join(TargetStars[3].transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.InOutSine).SetDelay(delayTime).OnComplete(() =>
			{
				TargetStars[3].transform.DOScale(0.95f, 0.2f).SetEase(Ease.InOutSine).OnComplete(() => TargetStars[3].transform.DOScale(1, 0.15f).SetEase(Ease.InOutSine));
			}))
			.Join(TargetStars[4].transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.OutBack).SetDelay(delayTime2))
			.Join(TargetStars[4].transform.DOLocalRotate(Vector3.zero, 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetDelay(delayTime2))
			.Join(TargetStars[4].transform.DOScale(Vector3.one * 1.05f, 0.4f).SetEase(Ease.InOutSine).SetDelay(delayTime2).OnComplete(() =>
			{
				TargetStars[4].transform.DOScale(0.95f, 0.15f).SetEase(Ease.InOutSine).OnComplete(() => TargetStars[4].transform.DOScale(1, 0.1f).SetEase(Ease.InOutSine));
			}))
			.AppendCallback(ShowEffect)
			.Append(CanvasGroups[2].DOFade(1, 0.2f))
			.Join(SuperHardFirstTryBanner.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack));
		sequence.OnKill(() => sequence = null);
		
		//播放胜利后，显示当前连胜爬藤活动的层数变化
		PlayWinStreakAnim();
	}

	private void ShowCoinAnim()
    {
		if (recordCoinNum > 0)
			sequence.Append(CanvasGroups[3].DOFade(1, 0.2f));
	}

	private void ShowFirstTryAnim()
    {
		bool isFirstTry = PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + curLevel, 0) == 0;
        if (isFirstTry)
        {
			FirstWinIcon.alpha = 0f;
			//TripleText.alpha = 0f;
			FirstWinIcon.transform.localPosition = Vector3.zero;
			//TripleText.transform.localPosition = Vector3.zero;
			FirstWinIcon.gameObject.SetActive(true);
			//TripleText.gameObject.SetActive(true);
			//Vector3 jumpFirstPos = TripleText.transform.position + new Vector3(0.2f, 0.1f, 0);
			//Vector3 jumpTargetPos = CoinNum_Text.transform.position - new Vector3(0.05f, 0, 0);

			//sequence.Append(FirstWinIcon.transform.DOLocalMove(Vector3.zero, 0.2f))
			//    .Join(FirstWinIcon.transform.DOScale(1, 0f).OnComplete(() =>
			//    {
			//        FirstWinIcon.DOFade(1, 0.3f);
			//        FirstWinIcon.transform.DORotate(new Vector3(0, -360 * 2f, 0), 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart);
			//        GameManager.Sound.PlayAudio("SFX_Level_FirstTry_Turnover");
			//    }))
			//    .Append(TripleText.transform.DOMove(jumpFirstPos, 0.3f))
			//    .Join(TripleText.DOFade(1, 0.15f))
			//    .Join(TripleText.transform.DOScale(1.3f, 0.2f).OnComplete(() =>
			//    {
			//        TripleText.transform.DOScale(0.9f, 0.15f).SetEase(Ease.InQuad).OnComplete(() =>
			//        {
			//            TripleText.transform.DOScale(1f, 0.15f);
			//        });
			//    }))
			//    .Append(TripleText.transform.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.2f).SetEase(Ease.OutQuart))
			//    .Append(TripleText.transform.DOJump(jumpTargetPos, 0.4f, 1, 0.4f).SetEase(Ease.InQuart).OnComplete(() =>
			//    {
			//        CoinNumChangeEffect.Play();
			//        TripleText.gameObject.SetActive(false);
			//        CoinNum_Text.transform.DOPunchScale(new Vector3(0.5f, 0.5f), 0.3f, 1);
			//        recordCoinNum *= 3;
			//        PlayerPrefs.SetInt("LevelWinCoin", recordCoinNum);
			//        CoinNum_Text.text = $"{recordCoinNum}";
			//        GameManager.Sound.PlayAudio("SFX_Level_FirstTry_Triple");
			//    }))
			//    .Join(TripleText.transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.2f).SetEase(Ease.InQuart));

			int originalCoinNum = recordCoinNum;
			recordCoinNum *= 3;
			PlayerPrefs.SetInt("LevelWinCoin", PlayerPrefs.GetInt("LevelWinCoin", 0) - originalCoinNum + recordCoinNum);

			sequence.Append(FirstWinIcon.transform.DOLocalMove(Vector3.zero, 0.2f))
				.Join(FirstWinIcon.transform.DOScale(1, 0f).OnComplete(() =>
				{
					FirstWinIcon.DOFade(1, 0.3f);
					FirstWinIcon.transform.DORotate(new Vector3(0, -360 * 2f, 0), 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart);
					GameManager.Sound.PlayAudio("SFX_Level_FirstTry_Turnover");
				}));

			RewardsArea.ShowFirstTryIncreaseRewardAnim(sequence, recordCoinNum);
		}
	}

	private bool ShowGetProgressRewardAnim()
    {
		bool isGetReward = false;
		int onlyUnlockBgId = 0;
		DTLevelReward levelRewardData = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
		LevelRewardData nextUnLockLevel = levelRewardData.GetNextUnlockLevelReward(curLevel);
		if (nextUnLockLevel == null || nextUnLockLevel.GetRewardDatas() == null || nextUnLockLevel.GetRewardDatas().Count == 0)
		{
			Progress_Obj.gameObject.SetActive(false);
		}
		else
		{
			List<ItemData> data = nextUnLockLevel.GetRewardDatas();
			Progress_Obj.gameObject.SetActive(true);
			int nextUnlockLevel = nextUnLockLevel.RewardGetLevel;
			int lastUnLockLevel = levelRewardData.GetLastUnlockLevel(curLevel);
			if (data.Count == 1 && data[0].type.TotalItemType == TotalItemType.Item_BgID)
			{
				onlyUnlockBgId = data[0].type.ID;
				BGSmall_Image.sprite = AddressableUtils.LoadAsset<Sprite>(UnityUtility.GetAltasSpriteName(onlyUnlockBgId.ToString(), "BGSmall"));
				BgReward.SetActive(true);
				Reward_Btn.gameObject.SetActive(false);
			}
			else
			{
				BgReward.SetActive(false);
				Reward_Btn.gameObject.SetActive(true);
			}

			float lastProgress = (curLevel - lastUnLockLevel) / (float)(nextUnlockLevel - lastUnLockLevel);
			float nowProgress = (nowLevel - lastUnLockLevel) / (float)(nextUnlockLevel - lastUnLockLevel);

			Progress_Image.fillAmount = lastProgress;
			Progress_Text.text = $"{curLevel - lastUnLockLevel}/{nextUnlockLevel - lastUnLockLevel}";

			isGetReward = nowProgress >= 1;
			int canGetLevelRewardId = 0;
			if (isGetReward && onlyUnlockBgId == 0)
			{
				canGetLevelRewardId = nextUnLockLevel.ID;
				PlayerPrefs.SetInt("CanGetLevelRewardId", canGetLevelRewardId);
			}

			CanvasGroups[4].gameObject.SetActive(true);
			CanvasGroups[6].gameObject.SetActive(true);
			sequence.Append(CanvasGroups[4].DOFade(1, 0.2f));
			sequence.Join(CanvasGroups[6].DOFade(1, 0.2f));
			sequence.Join(DOTween.To(() => lastProgress, (t) => Progress_Image.fillAmount = t, nowProgress, 1f).OnComplete(() =>
			{
				Progress_Text.text = $"{nowLevel - lastUnLockLevel}/{nextUnlockLevel - lastUnLockLevel}";
				if (isGetReward)
				{
					GameManager.PlayerData.RecordUnlockBG(GameManager.PlayerData.NowLevel);

					if (onlyUnlockBgId != 0)
					{
						GameManager.UI.ShowUIForm("GetBGPanel",(u) =>
						{
							var dataTable = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
							LevelRewardData nextUnLockData = dataTable.GetNextUnlockLevelReward(nowLevel);
							if (nextUnLockData == null)
							{
								CanvasGroups[4].gameObject.SetActive(false);
							}
							else
							{
								Progress_Image.fillAmount = 0;
								Progress_Text.text = $"0/{nextUnLockData.RewardGetLevel - dataTable.GetLastUnlockLevel(nowLevel)}";
								
								u.m_OnHideCompleteAction = () =>
								{
									try
									{
										ShowNextProgress(nextUnLockData, dataTable);
									}
									catch (System.Exception e)
									{
										Log.Error("ShowNextProgress error：" + e.Message);
									}
								};
							}
							
							GameManager.Sound.PlayAudio(SoundType.SFX_ShowBGPanel.ToString());
						}, userData: new { BGID = onlyUnlockBgId });
					}
					else
					{
						Reward_Btn.enabled = false;
						RewardPreviewBox.HidePromptBox();

						DTLevelReward levelRewardDataTable = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
						LevelRewardData levelRewardData = levelRewardDataTable.GetLevelReward(canGetLevelRewardId);
						if (levelRewardData != null)
						{
							List<ItemData> datas = levelRewardData.GetRewardDatas();
							PlayerPrefs.SetInt("CanGetLevelRewardId", 0);

							for (int i = 0; i < datas.Count; i++)
							{
								RewardManager.Instance.AddNeedGetReward(datas[i].type, datas[i].num);
							}

							AdsFreeCoin_Btn.interactable = false;

							CanvasGroups[4].transform.DOScale(new Vector3(1.15f, 1.15f, 1f), 0.15f).SetEase(Ease.InOutCubic).onComplete = () =>
							{
								CanvasGroups[4].transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f).SetEase(Ease.InCubic);
							};

							var dataTable = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
							LevelRewardData nextUnLockData = dataTable.GetNextUnlockLevelReward(nowLevel);
							GameManager.Task.AddDelayTriggerTask(0.45f, () =>
							{
								RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.GiftRewardPanel, false, () =>
								{
									AdsFreeCoin_Btn.interactable = true;
									GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.LevelRewardClaim, new Parameter("Level", GameManager.PlayerData.NowLevel - 1));
									try
									{
										ShowNextProgress(nextUnLockData, dataTable);
									}
									catch (System.Exception e)
									{
										Log.Error("ShowNextProgress error：" + e.Message);
									}

								}, null, () =>
								{
									if (nextUnLockData == null)
									{
										CanvasGroups[4].DOFade(0, 0.5f);
									}
									else
									{
										Progress_Image.fillAmount = 0;
										Progress_Text.text = $"0/{nextUnLockData.RewardGetLevel - dataTable.GetLastUnlockLevel(nowLevel)}";
									}
									Reward_Btn.gameObject.SetActive(false);
								});
							});
						}
					}

					GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Unlock_Scenes, 1);
				}
			}));
		}

		if (isGetReward) sequence.AppendInterval(0.5f);

		return isGetReward;
	}

	private void ShowNextProgress(LevelRewardData nextUnLockData, DTLevelReward dataTable)
	{
		//出现下一阶段奖励
		if (nextUnLockData != null || nextUnLockData.GetRewardDatas() == null ||
		    nextUnLockData.GetRewardDatas().Count == 0) 
		{
			Progress_Image.fillAmount = 0;
			Progress_Text.text = $"0/{nextUnLockData.RewardGetLevel - dataTable.GetLastUnlockLevel(nowLevel)}";
			var rewardData = nextUnLockData.GetRewardDatas();
			if (rewardData.Count == 1 && rewardData[0].type.TotalItemType == TotalItemType.Item_BgID)
			{
				BGSmall_Image.sprite = AddressableUtils.LoadAsset<Sprite>(UnityUtility.GetAltasSpriteName(rewardData[0].type.ID.ToString(), "BGSmall"));
				BgReward.transform.localScale=Vector3.zero;
				BgReward.SetActive(true);
				BgReward.transform.DOScale(1.1f, 0.2f).onComplete = () =>
				{
					BgReward.transform.DOScale(1f, 0.2f);
				};
				Reward_Btn.gameObject.SetActive(false);
			}
			else
			{
				BgReward.SetActive(false);
				Reward_Btn.transform.localScale=Vector3.zero;
				Reward_Btn.gameObject.SetActive(true);
				Reward_Btn.transform.DOScale(1.1f, 0.2f).onComplete = () =>
				{
					Reward_Btn.transform.DOScale(1f, 0.2f);
				};
				Reward_Btn.enabled = true;
			}

			curLevel = nowLevel;
		}
	}

	private void ShowButtonAnim()
    {
		sequence.Append(CanvasGroups[5].DOFade(1, 0.2f));
	}

	private void ShowEffect()
	{
		if (SystemInfoManager.CheckIsSpecialDeviceTurnOffParticleEffect())
        {
			return;
        }

		GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
			"TileWellDone",
			"TileItemDestroyEffectPool",
			2.2f,
			transform.position,
			transform.rotation,
			transform, (t) =>
			{
				var effect = t?.Target as GameObject;
				if (effect != null)
				{
					var skeleton = effect.transform.GetComponent<SkeletonGraphic>();
					if (skeleton != null)
					{
						skeleton.AnimationState.ClearTracks();
						skeleton.Skeleton.SetToSetupPose();
						skeleton.AnimationState.SetAnimation(0, "active", false);
					}
				}
			});
	}

	private void BtnEvent(bool isShowAdsBtn)
	{
		HomeSmall_Btn.SetBtnEvent(OnHomeButtonClick);
		Home_Btn.SetBtnEvent(OnHomeButtonClick);

		AdsFreeCoin_Btn.SetBtnEvent(() =>
		{
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_Bonus_Reward_Click_AD,
				new Parameter("Level", (GameManager.PlayerData.NowLevel - 1)));

			AdsFreeCoin_Btn.interactable = false;
			
			GameManager.Ads.ShowRewardedAd("GameWellDonePanel");
		});
		AdsFreeCoin_Btn.interactable = true;

		Reward_Btn.SetBtnEvent(RewardEvent);
	}

	public void OnHomeButtonClick()
	{
		SetData();
		Action action = () =>
		{
            if (GameManager.PlayerData.NowLevel > (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Win_Direct_Next_End_Level, 5))
            {
                GameManager.UI.ShowUIForm("MapDecorationBGPanel", f =>
                 {
                     GameManager.UI.HideUIForm(this);

                     var bgPanel = GameManager.UI.GetUIForm("MapMainBGPanel");
                     if (bgPanel != null)
                         bgPanel.OnHide();

                     ProcedureUtil.ProcedureGameToMap();
                 });
            }
            else
            {
				if (GameManager.PlayerData.GetInfiniteLifeTime() <= 0)
                {
					GameManager.PlayerData.UseItem(TotalItemData.Life, 1);
					GameManager.DataNode.SetData<bool>("UseLife", true);
				}

				GameManager.UI.ShowUIForm("TileMatchPanel", (u) =>
				{
					GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.SetMainBgNormal));
					GameManager.UI.HideUIForm(this);
				});
			}
		};

		if ((GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open ||
			((GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Clear ||
			  GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.ClearTime) &&
			 GameManager.Task.GlacierQuestTaskManager.IsCanClaimedReward)) &&
			 !GameManager.Network.CheckInternetIsNotReachable()) 
		{
			Log.Info("GlacierQuest：显示熔岩副本界面");
			GameManager.UI.HideUIForm(this);
			GameManager.UI.ShowUIForm("GlacierQuestMenu",form =>
			{
				GlacierQuestMenu menu = form as GlacierQuestMenu;
				menu.SetCloseEvent(action);
			});
		}
		else
		{
			action?.Invoke();
		}
	}

	private void RewardEvent()
    {
		DTLevelReward levelRewardData = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
		LevelRewardData nextUnLockLevel = levelRewardData.GetNextUnlockLevelReward(curLevel);
		RewardPreviewBox.Init(nextUnLockLevel.GetRewardDatas());
		RewardPreviewBox.ShowPromptBox(PromptBoxShowDirection.Up, Reward_Btn.transform.position);
	}

	private void SetData()
	{
		GameManager.Firebase.RecordCoinGet("AdForCoin", recordCoinNum);
	}

	private void PlayWinStreakAnim()
	{
		if (ClimbBeanstalkManager.Instance != null && ClimbBeanstalkManager.Instance.CheckActivityHasStarted()) 
		{
			currentWinStreakText.text = (ClimbBeanstalkManager.Instance.CurrentWinStreak - 1).ToString();
			GameManager.Task.AddDelayTriggerTask(1.0f, () =>
			{
				// winStreakSkeletonGraphic.gameObject.SetActive(true);
				// currentWinStreakText.text = ClimbBeanstalkManager.Instance.CurrentWinStreak.ToString();
				// winStreakSkeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
				
				winStreakImage.gameObject.SetActive(true);
				currentWinStreakText.text = ClimbBeanstalkManager.Instance.CurrentWinStreak.ToString();
			});
		}
		else
		{
			// winStreakSkeletonGraphic.gameObject.SetActive(false);
			winStreakImage.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
	}

	private void OnDisable()
	{
		GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);
	}

    private void OnRewardAdEarned(object sender, GameEventArgs e)
    {
		RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.Equals("GameWellDonePanel"))
        {
	        bool isUserEarnedReward = true;//ne.EarnedReward;
			if (isUserEarnedReward)
			{
				int startNum = recordCoinNum;
				int endNum = recordCoinNum * coinMultiple;
				recordCoinNum *= coinMultiple;
				PlayerPrefs.SetInt("LevelWinCoin", PlayerPrefs.GetInt("LevelWinCoin", 0) - startNum + recordCoinNum);

				RewardsArea.ShowCoinTextAnim(startNum,endNum);
				Init(false);
			}
			else
			{
				BtnEvent(true);
			}
		}
    }

	#region Receiver

	public ReceiverType ReceiverType => ReceiverType.Common;
	public GameObject GetReceiverGameObject() => gameObject;
	public Vector3 GetItemTargetPos(TotalItemData type)
	{
		if (HomeSmall_Btn.gameObject.activeSelf)
			return HomeSmall_Btn.transform.position;
		else
			return Home_Btn.transform.position;
	}

	public void OnFlyEnd(TotalItemData type)
	{
		if (HomeSmall_Btn.gameObject.activeSelf)
        {
			HomeSmall_Btn.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
			{
				HomeSmall_Btn.transform.localScale = Vector3.one;
			};
		}
        else
        {
			Home_Btn.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
			{
				Home_Btn.transform.localScale = Vector3.one;
			};
		}
	}

	public void OnFlyHit(TotalItemData type)
	{
	}
	#endregion
}
