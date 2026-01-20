using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Analytics;

public class GetBGPanel : PopupMenuForm
{
	[SerializeField]
	private CanvasGroup CanvasGroup;
	[SerializeField]
	private DelayButton Use_Btn, Collect_Btn;
	[SerializeField]
	private Image BG_Image;
	[SerializeField]
	private Transform Collect_Icon,Title_Trans,Btn_Trans;
	[SerializeField]
	private Transform BG_Parent ,Effect_Patrent;

	int bgID = 0;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		base.OnInit(uiGroup, completeAction, userData);

		if (userData != null)
		{
			var data = userData.ChangeType(new { BGID=0 });

			bgID = data.BGID;
		}
		CanvasGroup.DOFade(1, 0);
		ShowImage();
		BtnEvent();
	}

	public override void OnRelease()
	{
		isUse = false;
        if (BG_Image.sprite != null)
        {
			AddressableUtils.ReleaseAsset<Sprite>(BG_Image.sprite);
		}
		BG_Image.sprite = null;
		base.OnRelease();
	}

	private void ShowBGCollectAnim()
	{
		Effect_Patrent.gameObject.SetActive(false);
		DOTween.Sequence()
			.Append(Title_Trans.DOScale(Vector3.zero,0.2f).OnComplete(() => Collect_Icon.gameObject.SetActive(true)))
			.Join(Btn_Trans.DOScale(Vector3.zero, 0.25f))
			.Append(BG_Parent.DOMove(Collect_Icon.position, 0.5f).SetEase(Ease.InSine))
			.Join(BG_Parent.DOScale(Vector3.one*0.2f,0.5f).SetEase(Ease.InSine))
			.OnComplete(()=> 
			{
				BG_Parent.gameObject.SetActive(false);
				Collect_Icon.transform.DOPunchScale(Vector3.one * 0.95f, 0.15f, 1).OnComplete(()=> 
				{
					GameManager.UI.HideUIForm(this);
				});
			});
	}

	private void ShowImage()
	{
		Collect_Icon.gameObject.SetActive(false);
		BG_Image.sprite = AddressableUtils.LoadAsset<Sprite>(UnityUtility.GetAltasSpriteName(bgID.ToString(), "BGSmall"));
	}

	private bool isUse = false;
	private void BtnEvent()
	{
		Use_Btn.SetBtnEvent(() =>
		{
			if (isUse) return;
			isUse = true;
			
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_BG_Use,
				new Parameter("Level", (GameManager.PlayerData.NowLevel)));

			CanvasGroup.DOFade(0,0);

			GameManager.Task.AddDelayTriggerTask(2f,()=> 
			{
				GameManager.UI.HideUIForm(this);
				try
				{
					GameManager.UI.HideUIForm("TileMatchPanel");
				}
				catch (Exception e)
				{
					Log.Debug(e.Message);
				}
			});

			GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.UseBGSmall, bgID));

			GameManager.Sound.PlayAudio(SoundType.SFX_UseBG.ToString());
		});

		Collect_Btn.SetBtnEvent(() =>
		{
			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Level_BG_Collect, 
				new Parameter("Level", GameManager.PlayerData.NowLevel));

			ShowBGCollectAnim();

			GameManager.PlayerData.RecordShowBGRedPoint(bgID);
		});
	}
}
