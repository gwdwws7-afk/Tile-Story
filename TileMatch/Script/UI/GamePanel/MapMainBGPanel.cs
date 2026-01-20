using GameFramework.Event;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Coffee.UIEffects;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapMainBGPanel : UIForm
{
	[SerializeField]
	private RawImage BG_Image;

	int bgIndex = -1;
	bool isInBlack = false;

	private AsyncOperationHandle textureAssetHandle;
	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);

		if (bgIndex != GameManager.PlayerData.BGImageIndex && !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			bgIndex = GameManager.PlayerData.BGImageIndex;

			ShowBG();
		}

		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnRelease()
	{
		GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);
		base.OnRelease();
	}

	private void OnDestroy()
	{
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
		BG_Image.texture = null;
	}

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
		SetNormalBg();
		BG_Image.gameObject.SetActive(true);

		base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
		BG_Image.gameObject.SetActive(false);

		base.OnHide(hideSuccessAction, userData);
    }

    public override bool CheckInitComplete()
    {
		if (!textureAssetHandle.IsValid() || textureAssetHandle.IsDone)
			return true;

		return false;
    }

    private void ShowBG()
    {
	    string imageName = bgIndex.GetBigBgImageName();
		if (BG_Image.texture != null && BG_Image.texture.name == imageName) return;
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
        textureAssetHandle = UnityUtility.LoadAssetAsync<Texture>(imageName, (t) =>
		 {
			 BG_Image.texture = t;
		 });
	}

	public void SetBlackBg()
    {
		isInBlack = true;
		BG_Image.DOColor(new Color(0.3f, 0.3f, 0.3f, 1f), 0.3f);
	}

	public void SetNormalBg()
    {
		isInBlack = false;
		BG_Image.color = Color.white;
	}

	public void CommonHandle(object sender, GameEventArgs e)
	{
		CommonEventArgs ne = (CommonEventArgs)e;
		switch (ne.Type)
		{
			case CommonEventType.ChangBGImageID:
				if (ne.UserDatas != null)
				{
					var newBgIndex= (int)ne.UserDatas[0];
					if (newBgIndex == bgIndex) return;
					bgIndex = newBgIndex;
				}
				ShowBG();
				break;
			case CommonEventType.UseBGSmall:
				bgIndex = (int)ne.UserDatas[0];

				var newBG_image= Instantiate(BG_Image,transform);
				newBG_image.transform.SetAsFirstSibling();

				string newBGName = bgIndex.GetBigBgImageName();
				var newTextureAssetHandle= UnityUtility.LoadAssetAsync<Texture>(newBGName, (t) =>
				{
					newBG_image.texture = t as Texture;
					newBG_image.color = new Color(1, 1, 1, 0.1f);
					newBG_image.DOColor(Color.white, 0.2f);
				});

				//animation
				var uiDissolve = BG_Image.gameObject.AddComponent<UIDissolve>();
				uiDissolve.width = 0.15f;
				uiDissolve.softness = 0.5f;
				uiDissolve.effectFactor = 0f;
				DOTween.To(() => uiDissolve.effectFactor, (t) => uiDissolve.effectFactor = t, 1, 2f).OnComplete(() =>
						 {
							 if (BG_Image.texture != null && BG_Image.mainTexture.name != newBG_image.mainTexture.name)
							 {
								 UnloadBg();
							 }
							 Destroy(BG_Image.gameObject);

							 BG_Image = newBG_image;
							 if (isInBlack)
								 BG_Image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
							 GameManager.PlayerData.BGImageIndex = bgIndex;
							 textureAssetHandle = newTextureAssetHandle;
						 });
				return;
			case CommonEventType.SetMainBgBlack:
				SetBlackBg();
				break;
			case CommonEventType.SetMainBgNormal:
				SetNormalBg();
				break;
		}
	}

	private void UnloadBg()
	{
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
		BG_Image.texture = null;
	}
}
