using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Analytics;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class GetBGOrTileItemPanel : PopupMenuForm
{
	[SerializeField]
	private CanvasGroup CanvasGroup;
	[SerializeField]
	private Image blackBg;
	[SerializeField]
	private DelayButton Use_Btn, Collect_Btn;
	[SerializeField]
	private Image BG_Image;
	[SerializeField]
	private Transform Collect_Icon,Title_Trans,Btn_Trans;
	[SerializeField]
	private GameObject titleForNewBg;
	[SerializeField]
	private GameObject titleForNewTile;
	[SerializeField]
	private Transform BG_Parent, effectBehindBg, effectBehindItem;

	[SerializeField]
	private GameObject startEffect;

	[SerializeField]
	private Transform inputItemParent;
	[SerializeField]
	private Image inputItemImage;

	private FlyReward flyRewardCache;
	private TotalItemData totalItemDataCache;

	private AsyncOperationHandle asyncHandle;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		base.OnInit(uiGroup, completeAction, userData);

		if (userData != null)
		{
			flyRewardCache = userData as FlyReward;
			totalItemDataCache = flyRewardCache.RewardType;

			inputItemParent.position = flyRewardCache.transform.position;
			BG_Parent.position = flyRewardCache.transform.position;
		}

		CanvasGroup.DOFade(1, 0);
		ShowImage();
		BtnEvent();

		if (totalItemDataCache.TotalItemType == TotalItemType.Item_BgID)
		{
			titleForNewBg.SetActive(true);
			titleForNewTile.SetActive(false);

			PlayChangeItemToBgAnim();
		}
		else
		{
			titleForNewBg.SetActive(false);
			titleForNewTile.SetActive(true);

			PlayItemScaleAnim();
		}
	}

	public override void OnReset()
	{
		base.OnReset();

		UnityUtility.UnloadAssetAsync(asyncHandle);
	}

	public override void OnRelease()
	{
		base.OnRelease();

		if (asyncHandle.IsValid())
		{
			Addressables.Release(asyncHandle);
		}
	}

	private void ShowBGCollectAnim()
	{
		effectBehindBg.gameObject.SetActive(false);
		effectBehindItem.gameObject.SetActive(false);
		blackBg.DOColor(new Color(1, 1, 1, 0.01f), 0.2f);

		Transform moveTarget = BG_Parent;
		if (totalItemDataCache.TotalItemType != TotalItemType.Item_BgID)
			moveTarget = inputItemParent;

		Transform changeBGBtnTrans = null;
		if (GameManager.UI.HasUIForm("MapTopPanelManager"))
		{
			MapTopPanelManager panel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
			if (panel != null && panel.isActiveAndEnabled)
			{
				changeBGBtnTrans = panel.GetChangeBGBtnTrans();
			}
		}
		if (changeBGBtnTrans != null)
		{
			DOTween.Sequence()
				.Append(Title_Trans.DOScale(Vector3.zero, 0.2f))
				.Join(Btn_Trans.DOScale(Vector3.zero, 0.25f))
				.Append(moveTarget.DOMove(changeBGBtnTrans.position, 0.4f).SetEase(Ease.InQuad))
				.Join(moveTarget.DOScale(Vector3.one * 0.2f, 0.4f).SetEase(Ease.InQuad))
				.OnComplete(() =>
				{
					moveTarget.gameObject.SetActive(false);
					changeBGBtnTrans.DOPunchScale(Vector3.one * 0.95f, 0.15f, 1).OnComplete(() =>
					{
						GameManager.UI.HideUIForm(this);
					});
				});
		}
		else
		{
			DOTween.Sequence()
				.Append(Title_Trans.DOScale(Vector3.zero, 0.2f).OnComplete(() => Collect_Icon.gameObject.SetActive(true)))
				.Join(Btn_Trans.DOScale(Vector3.zero, 0.25f))
				.Append(moveTarget.DOMove(Collect_Icon.position, 0.4f).SetEase(Ease.InQuad))
				.Join(moveTarget.DOScale(Vector3.one * 0.2f, 0.4f).SetEase(Ease.InQuad))
				.OnComplete(() =>
				{
					moveTarget.gameObject.SetActive(false);
					Collect_Icon.transform.DOPunchScale(Vector3.one * 0.95f, 0.15f, 1).OnComplete(() =>
					{
						GameManager.UI.HideUIForm(this);
					});
				});
		}
	}

	private void ShowBGUseAnim()
    {
		effectBehindBg.gameObject.SetActive(false);
		effectBehindItem.gameObject.SetActive(false);
		blackBg.DOColor(new Color(1, 1, 1, 0.01f), 0.2f);

		Transform moveTarget = BG_Parent;
		if (totalItemDataCache.TotalItemType != TotalItemType.Item_BgID)
			moveTarget = inputItemParent;

		Transform levelBtnTrans = null;
		if (GameManager.UI.HasUIForm("MapTopPanelManager"))
		{
			MapTopPanelManager panel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
			if (panel != null)
			{
				levelBtnTrans = panel.GetLevelBtnTrans();
			}
		}
		if (levelBtnTrans != null)
		{
			DOTween.Sequence()
				.Append(Title_Trans.DOScale(Vector3.zero, 0.2f))
				.Join(Btn_Trans.DOScale(Vector3.zero, 0.25f))
				.Append(moveTarget.DOMove(levelBtnTrans.position, 0.4f).SetEase(Ease.InQuad))
				.Join(moveTarget.DOScale(Vector3.one * 0.2f, 0.4f).SetEase(Ease.InQuad))
				.OnComplete(() =>
				{
					moveTarget.gameObject.SetActive(false);
					levelBtnTrans.DOPunchScale(Vector3.one * 0.2f, 0.15f, 1).OnComplete(() =>
					{
						GameManager.UI.HideUIForm(this);
					});
				});
		}
		else
		{
			Log.Error("Not Implemented");
			GameManager.UI.HideUIForm(this);
		}
	}

	private void ShowImage()
	{
		if (totalItemDataCache.TotalItemType != TotalItemType.Item_BgID || totalItemDataCache.RefID != 0)
		{
			string spriteKey = UnityUtility.GetRewardSpriteKey(totalItemDataCache, 1);
			asyncHandle = UnityUtility.LoadGeneralSpriteAsync(spriteKey, sp =>
			{
				inputItemImage.sprite = sp;
				GameManager.Task.AddDelayTriggerTask(0f, () =>
				{
					if (flyRewardCache != null) 
						flyRewardCache.gameObject.SetActive(false);
				});
			});
		}

		Collect_Icon.gameObject.SetActive(false);

		if (totalItemDataCache.TotalItemType == TotalItemType.Item_BgID)
		{
			if (totalItemDataCache.RefID != 0)
				BG_Image.sprite = BGSmallUtil.GetSprite(totalItemDataCache.RefID);
			else
				BG_Image.sprite = BGSmallUtil.GetSprite(totalItemDataCache.ID);
		}
	}

	private void BtnEvent()
	{
		Use_Btn.SetBtnEvent(() =>
		{
			ShowBGUseAnim();

			if (totalItemDataCache.TotalItemType == TotalItemType.Item_BgID)
			{
				if (totalItemDataCache.RefID != 0) 
					GameManager.PlayerData.BGImageIndex = totalItemDataCache.RefID;
				else
					GameManager.PlayerData.BGImageIndex = totalItemDataCache.ID;
			}
			else if(totalItemDataCache.TotalItemType == TotalItemType.Item_TileID)
			{
				GameManager.PlayerData.TileImageIndex = totalItemDataCache.RefID;
			}
			GameManager.Sound.PlayAudio(SoundType.SFX_UseBG.ToString());
		});

		Collect_Btn.SetBtnEvent(() =>
		{

			ShowBGCollectAnim();
		});
	}

	private void PlayChangeItemToBgAnim()
    {
		if (totalItemDataCache.TotalItemType != TotalItemType.Item_BgID || totalItemDataCache.RefID != 0)
		{
			inputItemParent.gameObject.SetActive(true);
			BG_Parent.gameObject.SetActive(false);
			effectBehindBg.gameObject.SetActive(false);
			effectBehindItem.gameObject.SetActive(false);
			Use_Btn.gameObject.SetActive(false);
			Collect_Btn.gameObject.SetActive(false);

			float finalRotateTarget = 360.0f * 5;

			inputItemParent.DORotate(new Vector3(0, finalRotateTarget, 0), 1.0f, RotateMode.FastBeyond360).SetEase(Ease.InQuart).onComplete = () =>
			{
				inputItemParent.gameObject.SetActive(false);
				BG_Parent.gameObject.SetActive(true);
				effectBehindBg.gameObject.SetActive(true);

				startEffect.SetActive(true);

				GameManager.Task.AddDelayTriggerTask(1.0f, () =>
				{
					startEffect.SetActive(false);
				});

				BG_Parent.DORotate(new Vector3(0, -finalRotateTarget, 0), 1.0f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart).onComplete = () =>
				{
					Use_Btn.gameObject.SetActive(true);
					Collect_Btn.gameObject.SetActive(true);
				};
			};
		}
		else
		{
			inputItemParent.gameObject.SetActive(false);
			BG_Parent.gameObject.SetActive(true);
			effectBehindBg.gameObject.SetActive(true);
			Use_Btn.gameObject.SetActive(true);
			Collect_Btn.gameObject.SetActive(true);

			startEffect.SetActive(true);
			GameManager.Task.AddDelayTriggerTask(1.0f, () =>
			{
				startEffect.SetActive(false);
			});

			BG_Parent.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			BG_Parent.DOScale(Vector3.one * 1.1f, 0.15f).onComplete = () =>
			{
				BG_Parent.DOScale(Vector3.one * 0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
				{
					BG_Parent.DOScale(Vector3.one, 0.15f);
				};
			};
		}
	}

	private void PlayItemScaleAnim()
    {
		inputItemParent.gameObject.SetActive(true);
		BG_Parent.gameObject.SetActive(false);
		effectBehindItem.gameObject.SetActive(false);
		effectBehindBg.gameObject.SetActive(false);
		Use_Btn.gameObject.SetActive(false);
		Collect_Btn.gameObject.SetActive(false);


		inputItemParent.DOScale(Vector3.one * 1.1f, 0.15f).onComplete = () =>
		{
			inputItemParent.DOScale(Vector3.one * 0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
			{
				inputItemParent.DOScale(Vector3.one, 0.15f).onComplete = () =>
				{
					effectBehindItem.gameObject.SetActive(true);
					Use_Btn.gameObject.SetActive(true);
					Collect_Btn.gameObject.SetActive(true);
				};
			};
		};
	}
}
