using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BuyBGPanel : PopupMenuForm
{
	[SerializeField]
	private RawImage BG_Image;
	[SerializeField]
	private DelayButton Buy_Btn,Back_Btn;
	[SerializeField]
	private TextMeshProUGUI Coin_Num;

	int bgIndex = 1;
	int coinNum = 600;
	Action buySuccessAction=null;
	private string curBgName;
	private AsyncOperationHandle handle;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		(int, int,Action)? data = userData as (int,int,Action)?;
		if (data != null)
		{
			bgIndex = data.Value.Item1;
			coinNum = data.Value.Item2;
			buySuccessAction = data.Value.Item3;
		}
		else throw new Exception("BG ID is Null!");

		ShowBG();
		SetBtn();
		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnRelease()
	{
		UnloadBg();
		base.OnRelease();
	}

	private void ShowBG()
	{
		string imageName = bgIndex.GetBigBgImageName();

		if (curBgName != imageName) 
        {
			UnloadBg();
			BG_Image.color = new Color(0, 0, 0, 1f);
            handle = UnityUtility.LoadAssetAsync<Texture>(imageName, bg =>
			{
                if (bg != null)
                {
					BG_Image.texture = bg;
					BG_Image.DOColor(Color.white, 0.2f);
					curBgName = imageName;
				}
                else
                {
					GameManager.PlayerData.BGImageIndex = 1;
					ShowBG();
				}
			});
		}

		SetSize();
	}

	private void SetBtn()
	{
		Coin_Num.text = coinNum.ToString();
		Buy_Btn.gameObject.SetActive(true);
		Buy_Btn.SetBtnEvent(()=> 
		{
			if (GameManager.PlayerData.UseItem(TotalItemData.Coin, coinNum))
			{
				GameManager.UI.HideUIForm(this);
				GameManager.PlayerData.BuyBGID(bgIndex);
				Buy_Btn.gameObject.SetActive(false);
				buySuccessAction?.Invoke();

				GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.Objective));
			}
			else
			{
				//show shop
				GameManager.UI.ShowUIForm("ShopMenuManager");
				GameManager.Firebase.RecordCoinNotEnough(3, GameManager.PlayerData.NowLevel);
			}
		});
		Back_Btn.SetBtnEvent(()=> 
		{
			GameManager.UI.HideUIForm(this);
		});
	}

	private void SetSize()
	{
		float otehrRatio = (Screen.height / Screen.safeArea.height-1f)*2f+1f;
		if (otehrRatio*(Screen.height * 1080f) > 1920f * Screen.width)
		{
			float ratio = otehrRatio* Screen.height * 1080f / (1920f * Screen.width);
			BG_Image.rectTransform.sizeDelta = new Vector2(1080,1920)* ratio;
		}
	}

	private void UnloadBg()
	{
		if (BG_Image.texture != null)
		{
			BG_Image.texture = null;
		}
		UnityUtility.UnloadAssetAsync(handle);
	}
}
