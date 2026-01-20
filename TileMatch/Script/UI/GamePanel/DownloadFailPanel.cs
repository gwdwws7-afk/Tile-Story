using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Firebase.Analytics;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownloadFailPanel : PopupMenuForm
{
	[SerializeField] private DelayButton Close_Btn,Retry_Btn;

	private Action action;
	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		base.OnInit(uiGroup, completeAction, userData);

		action = (Action)userData;
		BtnEvent();
	}

	private void BtnEvent()
	{
		Close_Btn.SetBtnEvent(() =>
		{
			GameManager.UI.HideUIForm(this);
		});
		Retry_Btn.SetBtnEvent(() => 
		{
			action?.InvokeSafely();
			GameManager.UI.HideUIForm(this);
        });
	}
}
