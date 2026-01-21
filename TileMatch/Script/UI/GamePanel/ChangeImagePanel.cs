using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;
using DG.Tweening;
using GameFramework.Event;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ChangeImagePanel : CenterForm
{
	[SerializeField]
	private DelayButton Close_Btn;
	[SerializeField]
	private Toggle[] AllToggles;
	// [SerializeField]
	// private LoopScrollView BG_GridScrollView;
	[SerializeField]
	private LoopScrollView TileImage_LoopScrollViews;
	[SerializeField]
	private Scroller Scrollbar;

	[SerializeField] private ScrollRect BG_Scroll;
	[SerializeField] private StoryBGItem StoryBgItem;
	[SerializeField] private EventBGItem EventBgItem;
	[SerializeField] private Transform NormalBG_Parent;

	[SerializeField] private RawImage BG_RawImage;
	[SerializeField] private Image[] TileImages;
	[SerializeField] private DelayButton BG_DelayBtn;
	[SerializeField] private RectTransform Root_RectTrans;
	[SerializeField] private GameObject RedPoint;

	private int curBtnIndex = 0;

	private bool isOpenAinm = true;

	private AsyncOperationHandle textureAssetHandle;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);
		
		float ratio = (Screen.height * 1080f / Screen.width / 1920f);
		BG_RawImage.transform.parent.localScale =Vector3.one*1.1f*Mathf.Max(1f,ratio);
		//按钮事件
		BtnEvent();
		BGBtnEvent();

		RedPoint.SetActive(GameManager.PlayerData.IsShowChangeImageToggleRedPoint);

		base.OnInit(uiGroup, completeAction, userData);
	}

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
		ShowTileImages();
		ShowBG(true);
		SetToggleBtns();
		base.OnShowInit(showInitSuccessAction, userData);
    }

	public override void OnRelease()
	{
		GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);
		curBtnIndex = 0;
		
		base.OnRelease();
	}

	private void OnDestroy()
	{
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
		BG_RawImage = null;
	}

	private void BtnEvent()
	{
		Close_Btn.SetBtnEvent(()=> 
		{
			GameManager.UI.HideUIForm(this);
		});
	}

	private void SetToggleBtns()
	{
		SetLoopScrollViews(false);
		for (int i = 0; i < AllToggles.Length; i++)
		{
			int curIndex = i;
			bool isActive = curBtnIndex == i;

			AllToggles[i].isOn = isActive;
			AllToggles[i].onValueChanged.RemoveAllListeners();
			AllToggles[i].onValueChanged.AddListener((b) =>
			{
				if (b)
				{
					GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
					curBtnIndex = curIndex;
					SetLoopScrollViews(false);

					if (curIndex == 1 && RedPoint.activeSelf) 
                    {
						RedPoint.SetActive(false);
						GameManager.PlayerData.IsShowChangeImageToggleRedPoint = false;
					}
				}
			});
		}
	}

	private void SetLoopScrollViews(bool keepScrollIndex)
	{
		switch (curBtnIndex)
		{
			//BG
			case 0:
				// int bgImageIndex = GameManager.PlayerData.BGImageIndex;
				// bgImageIndex = Math.Min(Math.Max(1, bgImageIndex), GameManager.DataTable.GetDataTable<DTBGID>().Data.BGItemDataDict.Keys.Last());
				// int themeId = GameManager.DataTable.GetDataTable<DTBGID>().Data.BGItemDataDict[bgImageIndex].Theme;
				//
				// TileImage_LoopScrollViews.gameObject.SetActive(false);
				// BG_GridScrollView.gameObject.SetActive(true);
				// BG_GridScrollView.OnCellClicked(null);
				// BG_GridScrollView.UpdateData(GetThemeList());
				// BG_GridScrollView.JumpTo(themeId-1);
				// Scrollbars[0].ScrollSensitivity = 8;
				BG_Scroll.gameObject.SetActive(true);
				TileImage_LoopScrollViews.gameObject.SetActive(false);
				ShowBGScroll();
				break;
			//TileImage
			case 1:
				// BG_GridScrollView.gameObject.SetActive(false);
				BG_Scroll.gameObject.SetActive(false);
				TileImage_LoopScrollViews.gameObject.SetActive(true);
				TileImage_LoopScrollViews.OnCellClicked((i)=> { });
				TileImage_LoopScrollViews.UpdateData(GetTileIconList());
				if (!keepScrollIndex)
				{
					int recentTileImageIndexSort = GameManager.DataTable.GetDataTable<DTTileID>().Data.GetData(GameManager.PlayerData.TileImageIndex).TileSort;
					if (!GameManager.PlayerData.GetEverFocusedOnFirstTileItemInChangeImagePanel())
					{
						GameManager.PlayerData.RecordEverFocusedOnFirstTileItemInChangeImagePanel(true);
						recentTileImageIndexSort = 1;
					}
					TileImage_LoopScrollViews.JumpTo(recentTileImageIndexSort - 1);
				}
				Scrollbar.ScrollSensitivity = 3;
				break;
		}
	}

	private void ShowBGScroll()
	{
		StoryBgItem.gameObject.SetActive(true);
		StoryBgItem.Init(Constant.GameConfig.MaxDecorationArea,GameManager.PlayerData.DecorationAreaID);
		bool hasEventBg = false;
        if (hasEventBg)
        {
			EventBgItem.gameObject.SetActive(true);
			EventBgItem.Init();
		}
        var themeList = GetNoneEventThemeList();
		NormalBG_Parent.gameObject.SetActive(true);
		UnityUtility.FillGameObjectWithFirstChild<BGThemeCell>(NormalBG_Parent.gameObject,themeList.Count,
			(index, comp) =>
			{
				int themeId = themeList[index];
				comp.Init(themeId);
			});

		int index = GameManager.PlayerData.BGImageIndex;
		float extraThemeCount = hasEventBg ? 2f : 1f;
		float pos = (index > 1000) ? 1 : 1 - (index / 5 + extraThemeCount) / (themeList.Count + extraThemeCount);
		pos = index > 2000 ? 1 - 1 / (themeList.Count + extraThemeCount) : pos;

		if(!GameManager.PlayerData.GetEverFocusedOnEventBGItemInChangeImagePanel())
        {
			GameManager.PlayerData.RecordEverFocusedOnEventBGItemInChangeImagePanel(true);
			//定位到活动
			pos = 1 - 1 / (themeList.Count + 2f);
		}

		GameManager.Task.AddDelayTriggerTask(0.1f, () =>
		{
			DOTween.To(() => BG_Scroll.verticalNormalizedPosition, (t) => BG_Scroll.verticalNormalizedPosition = t, pos, 0.2f);
		});
	}

	private List<int> GetNoneEventThemeList()
	{
		return GameManager.DataTable.GetDataTable<DTBGID>().Data.NonEventThemeList;
		//return GameManager.DataTable.GetDataTable<DTBGID>().Data.BGThemeDict.Keys.ToList();
	}

	private List<int> GetTileIconList()
	{
		var tileIdDic = GameManager.DataTable.GetDataTable<DTTileID>().Data.TileDataDict;

		//合成游戏棋子不达到目标关卡不显示
		if (tileIdDic.ContainsKey(1001) && GameManager.PlayerData.NowLevel < Merge.MergeManager.PlayerData.GetActivityUnlockLevel()) 
		{
			tileIdDic.Remove(1001);
		}
		return tileIdDic.Keys.ToList();
	}
	
	public void CommonHandle(object sender, GameEventArgs e)
	{
		CommonEventArgs ne = (CommonEventArgs)e;
		switch (ne.Type)
		{
			case CommonEventType.ChangeTileIconID:
				ShowTileImages();
				SetLoopScrollViews(true);
				break;
			case CommonEventType.ChangBGImageID:
				ShowBG();
				break;
			case CommonEventType.BuyTileID:
				SetLoopScrollViews(true);
				break;
		}
	}

	private void BGBtnEvent()
	{
		Root_RectTrans.anchoredPosition = new Vector2(0, -1492f);
		Root_RectTrans.DOAnchorPosY(0f,1f);
		isOpenAinm = true;
		BG_DelayBtn.SetBtnEvent(() =>
		{
			if (isOpenAinm)
			{
				isOpenAinm = false;
				Root_RectTrans.DOAnchorPosY(-1492f,1f);
			}
			else
			{
				isOpenAinm = true;
				Root_RectTrans.DOAnchorPosY(0f,1f);
			}
		});
	}

	private void ShowTileImages()
	{
		int index = 2;
		foreach (var image in TileImages)
		{
			image.sprite = TileMatchUtil.GetTileSprite(index);
			index++;
		}
	}

	private void ShowBG(bool isAnim=false)
	{
		string imageName = GameManager.PlayerData.BGImageIndex.GetBigBgImageName();
		if(BG_RawImage.texture!=null&&BG_RawImage.texture.name==imageName)return;
		if(isAnim) BG_RawImage.color = new Color(0f, 0f, 0f, 1f);
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
		textureAssetHandle = UnityUtility.LoadAssetAsync<Texture>(imageName, t =>
		  {
			  BG_RawImage.texture = t;
			  BG_RawImage.DOColor(Color.white, 0.05f);
		  });
	}
}


public static class BGSmallUtil
{
	private static UnityEngine.U2D.SpriteAtlas atlas;
	private static AsyncOperationHandle handle;
	private static Dictionary<int, Sprite> dict;
	public static Dictionary<int, Sprite> Dict
	{
		get
		{
			if (dict == null)
			{
				dict = new Dictionary<int, Sprite>();
				var async = Addressables.LoadAssetAsync<UnityEngine.U2D.SpriteAtlas>("BGSmall");
				handle = async;
				atlas = async.WaitForCompletion();
				//var atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>("BGSmall", ref handle);
				int index = 0;

				while (index < 100)
				{
					index++;
					try
					{
						var sprite = atlas.GetSprite($"{index}");
						if (sprite == null) break;
						dict.Add(index, atlas.GetSprite($"{index}"));
					}
					catch (Exception e)
					{
						Log.Debug(e);
					}
				}

				index = 1000;
				while (index >= 1000 && index < 1100)
				{
					index++;
					try
					{
						var sprite = atlas.GetSprite($"{index}");
						if (sprite == null) break;
						dict.Add(index, atlas.GetSprite($"{index}"));
					}
					catch (Exception e)
					{
						Log.Debug(e);
					}
				}

				index = 2000;
				while (index >= 2000 && index < 2100)
				{
					index++;
					try
					{
						var sprite = atlas.GetSprite($"{index}");
						if (sprite == null) break;
						dict.Add(index, atlas.GetSprite($"{index}"));
					}
					catch (Exception e)
					{
						Log.Debug(e);
					}
				}
				
				index = 10000;
				while (index >= 10000 && index < 10100)
				{
					index++;
					try
					{
						var sprite = atlas.GetSprite($"{index}");
						if (sprite == null) break;
						dict.Add(index, atlas.GetSprite($"{index}"));
					}
					catch (Exception e)
					{
						Log.Debug(e);
					}
				}
			}
			return dict;
		}
	}

	public static Sprite GetSprite(int index)
	{
		//这里有可能ab 变化，所以已添加处理
		int newIndex =index.GetNewBgImageIndex();
		if (Dict.TryGetValue(newIndex,out Sprite sprite))
		{
			return sprite;
		}
		return null;
	}
	
	public static void ClearRes()
	{
		return;
		dict = null;
		Resources.UnloadAsset(atlas);
		atlas = null;
		try
		{
			Addressables.Release(handle);
		}
		catch (Exception e)
		{
			Log.Error(e.Message);
		}
	}
}
