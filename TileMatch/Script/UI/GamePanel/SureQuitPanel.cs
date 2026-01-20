using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SureQuitPanel : PopupMenuForm
{
	[SerializeField] private DelayButton Close_Btn, Quit_Btn;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		BtnEvent();
		base.OnInit(uiGroup, completeAction, userData);
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
