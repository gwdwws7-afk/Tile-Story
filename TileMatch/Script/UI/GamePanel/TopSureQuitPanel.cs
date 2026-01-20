using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TopSureQuitPanel : UIForm
{
	[SerializeField] private DelayButton Close_Btn, Quit_Btn;
	[SerializeField] private Transform Root_Parent;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		BtnEvent();
		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
	{
		Root_Parent.DOKill();
		Root_Parent.localScale = Vector3.one;
		Root_Parent.DOScale(1.03f, 0.12f).onComplete = () =>
		{
			Root_Parent.DOScale(0.99f, 0.1f).onComplete = () =>
			{
				Root_Parent.DOScale(1f, 0.1f);
			};
		};
		base.OnShow(showSuccessAction, userData);
	}


	private void BtnEvent()
	{
		Close_Btn.SetBtnEvent(() =>
		{
			GameManager.UI.HideUIForm(this);
		});
		Quit_Btn.SetBtnEvent(() =>
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			return;
#endif
			Application.Quit();
		});
	}
}
