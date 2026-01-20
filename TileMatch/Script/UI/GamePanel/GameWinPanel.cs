using DG.Tweening;
using Spine;
using Spine.Unity;
using System;
using UnityEngine;

public sealed class GameWinPanel : PopupMenuForm
{
	public SkeletonAnimation winAnim;
	public GameObject winEffect;
	public GameObject salvoEffect;
    public DelayButton bgButton;
    public GameObject TapText;

	private TrackEntry trackEntry;
	private bool isComplete;
	private int? audioId;

	public Action finishAction;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

		if (userData != null)
		{
			try
			{
				var data = userData.ChangeType(
					new
					{
						curLevel = 0,
						starVector = Vector3.zero,
					});
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

		isComplete = false;
		salvoEffect.SetActive(false);
		bgButton.SetBtnEvent(OnBgButtonClick);
	}

	public void SetAction(Action finishAction)
	{
		this.finishAction = finishAction;
	}

	public override void OnReset()
    {
		bgButton.onClick.RemoveAllListeners();
		trackEntry = null;
		TapText.transform.DOKill();

		base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

		if (!isComplete && trackEntry != null) 
		{
			if (trackEntry.TrackTime >= 1.9f && !salvoEffect.activeSelf)
			{
				salvoEffect.SetActive(true);

				TapText.SetActive(true);
				TapText.transform.DOScale(1.1f, 0.2f).onComplete = () =>
				{
					TapText.transform.DOScale(1f, 0.2f);
				};
			}
		}
	}

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
		winAnim.Initialize(false);
		trackEntry = winAnim.state.SetAnimation(0, "idle", false);
		trackEntry.Complete += t =>
		{
			trackEntry = null;

			GameManager.Task.AddDelayTriggerTask(0.5f, () =>
			{
				salvoEffect.SetActive(false);
				OnBgButtonClick();
			});
		};

		audioId = GameManager.Sound.PlayAudio(SoundType.WIN_Complete.ToString());

		base.OnShow(showSuccessAction, userData);
    }

    public override void OnReveal()
    {
    }

    private void OnBgButtonClick()
    {
		if (isComplete)
			return;
		isComplete = true;

		if (trackEntry != null)
        {
			trackEntry.TimeScale = 0;
			trackEntry = null;
		}

		if (audioId != null)
        {
			GameManager.Sound.StopSound(audioId.Value);
			audioId = null;
		}

		GameManager.UI.HideUIForm(this);

		float delay = 0;
        if (GameManager.Ads.CheckInterstitialAdIsLoaded())
        {
			if (SystemInfoManager.DeviceType == DeviceType.SurpLow)
				delay = 0.5f;
			else if(SystemInfoManager.DeviceType == DeviceType.Low)
				delay = 0.3f;
			else
				delay = 0.1f;
        }

		GameManager.Task.AddDelayTriggerTask(delay, () =>
		{
			finishAction?.InvokeSafely();
		});
	}
}
