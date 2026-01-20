using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class StoryPanel : CenterForm, ICustomOnEscapeBtnClicked
{
	[SerializeField] private DelayButton Skip_Btn,Next_Btn;
	[SerializeField] private TextMeshProUGUILocalize Name_Text, Content_Text;
	[SerializeField] private RawImage Avator_Image;
	[SerializeField] private Transform triangleMarkOnNextBtn;

	[SerializeField] private CanvasGroup[] Anim_CanvasGroups;

	private int chapterID;
	private int buildSchedule;
	private int storyIndex = 1;
	private int avatorPosY;

	private AsyncOperationHandle textureAssetHandle;
	
	private TweenerCore<int, int, NoOptions> recordTweenCallback;

	private Dictionary<int, HelpData> curStoryDict;
	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		var data= userData.ChangeType(
			new { 
				chapterID = 0,
				buildSchedule=0,
			});
		chapterID = data.chapterID;
		buildSchedule = data.buildSchedule;

		curStoryDict = GameManager.DataTable.GetDataTable<DTHelp>().Data.GetDictByBuildSchedule(chapterID,buildSchedule);
		
		GameManager.PlayerData.RecordShowStoryData(chapterID,buildSchedule);
		
		SetBtn();
		GameManager.Task.AddDelayTriggerTask(0.7f, () =>
		{
			try { SetText(); } catch { }
		});
		SetAvatorImage();

		bool isRightToLeftLanguage = GameManager.Localization.Language == Language.Arabic;
		Content_Text.Target.horizontalAlignment = isRightToLeftLanguage ? TMPro.HorizontalAlignmentOptions.Right : TMPro.HorizontalAlignmentOptions.Left;
		Vector3 formerV3 = triangleMarkOnNextBtn.position;
		triangleMarkOnNextBtn.position = new Vector3(Mathf.Abs(formerV3.x) * (isRightToLeftLanguage ? -1 : 1), formerV3.y, formerV3.z);

		if (chapterID == 26)
			avatorPosY = 698;
		else
			avatorPosY = 624;

		base.OnInit(uiGroup, completeAction, userData);
	}

	private Sequence sequence;
	public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
	{
		Anim_CanvasGroups[0].transform.localPosition = new Vector3(0,600, 0);
		Anim_CanvasGroups[1].transform.localPosition = new Vector3(0,-600, 0);
		Anim_CanvasGroups[2].alpha = 0;
		Anim_CanvasGroups[3].transform.localPosition=new Vector3(-328-460, avatorPosY);
		Anim_CanvasGroups[4].transform.localScale=new Vector3(0, 0f);
		Anim_CanvasGroups[4].alpha=0;

		GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.DecorationScaleUp));

		if (sequence != null) sequence.Kill();
		sequence = DOTween.Sequence()
			.SetEase(Ease.InOutSine)
			.Append(Anim_CanvasGroups[0].transform.DOLocalMove(Vector3.zero, 0.5f))
			.Join(Anim_CanvasGroups[1].transform.DOLocalMove(Vector3.zero, 0.5f))
			.Append(Anim_CanvasGroups[2].DOFade(1, 0.2f))
			.Append(Anim_CanvasGroups[3].transform.DOLocalMove(new Vector3(-328, avatorPosY), 0.4f))
			.Join(Anim_CanvasGroups[4].transform.DOScale(1, 0.6f).SetDelay(0.1f).SetEase(Ease.OutSine))
			.Join(Anim_CanvasGroups[4].DOFade(1, 0.2f).SetEase(Ease.OutSine))
			.OnKill(()=>sequence=null);
		
		base.OnShow(showSuccessAction, userData);
	}

	private void PlayFinishAnim(Action finishAction)
	{
		Skip_Btn.interactable = false;
		GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.DecorationScaleDown));
		DOTween.Sequence()
			.Append(Anim_CanvasGroups[4].DOFade(0, 0.2f))
			.Join(Anim_CanvasGroups[4].transform.DOScale(0, 0.2f))
			.Join(Anim_CanvasGroups[3].transform.DOLocalMove(new Vector3(-328-500, avatorPosY), 0.3f).SetDelay(0.1f))
			.Append(Anim_CanvasGroups[2].DOFade(0, 0.15f))
			.Append(Anim_CanvasGroups[1].transform.DOLocalMove(new Vector3(0,-600, 0), 0.4f))
			.Join(Anim_CanvasGroups[0].transform.DOLocalMove(new Vector3(0,600, 0), 0.4f).OnComplete(() =>
			{
				finishAction?.Invoke();
				finishAction = null;
			}));

		GameManager.Task.AddDelayTriggerTask(1.2f, () =>
		{
			finishAction?.Invoke();
			finishAction = null;
		});
	}

	public override void OnRelease()
	{
		chapterID = 0;
		buildSchedule = 0;
		curStoryDict.Clear();
        if (sequence != null)
        {
			sequence.Kill();
			sequence = null;
		}
		storyIndex = 1;
		
		recordTweenCallback.Kill();
		
		base.OnRelease();
	}

	private void OnDestroy()
	{
		UnityUtility.UnloadAssetAsync(textureAssetHandle);
		Avator_Image.texture = null;
	}

	private void SetText()
	{
		if (String.IsNullOrEmpty(curStoryDict[storyIndex].Name))
		{
			Name_Text.Target.enabled = false;
		}
		else
		{
			Name_Text.Target.enabled = true;
			Name_Text.SetTerm($"Story.{curStoryDict[storyIndex].Name}");
		}

		Content_Text.SetTerm($"Story.{curStoryDict[storyIndex].Dialogue}");
		if (!string.IsNullOrEmpty(curStoryDict[storyIndex].SoundEffect))
			GameManager.Sound.PlayAudio(curStoryDict[storyIndex].SoundEffect);
        //DOTween.To(() => string.Empty, t => Content_Text.Target.text = t, Content_Text.Target.text, 2f);

        Content_Text.Target.maxVisibleCharacters = 0;
        recordTweenCallback.Kill(true);
        GameManager.Task.AddDelayTriggerTask(0.0f, () =>
        {
            int finalCharacterCount = Content_Text.Target.textInfo.characterCount;
            float characterPerSecond = 20.0f;
            if (GameManager.Localization.RecentLanguageIsEastAsianLanguage())
                characterPerSecond = 10.0f;
            recordTweenCallback=DOTween.To(() => 0, t => Content_Text.Target.maxVisibleCharacters = t, finalCharacterCount, finalCharacterCount / characterPerSecond);
        });
    }

	private void SetAvatorImage()
	{
		if (Avator_Image.texture != null && Avator_Image.texture.name == curStoryDict[storyIndex].Avatar) 
		{
			Avator_Image.DOColor(Color.white,0);
		}
		else
		{
			//暂时认为只有第一句剧情需要头像从黑色渐变，后续头像变换直接切换图即可
			if (storyIndex == 1)
				Avator_Image.color = Color.black;
			if (String.IsNullOrEmpty(curStoryDict[storyIndex].Avatar))
			{
				Avator_Image.color = new Color(1, 1, 1, 0);
			}
			else
			{
				if(Avator_Image.texture!=null&&Avator_Image.texture.name==curStoryDict[storyIndex].Avatar)return;
				UnityUtility.UnloadAssetAsync(textureAssetHandle);
				textureAssetHandle = UnityUtility.LoadAssetAsync<Texture>(curStoryDict[storyIndex].Avatar, (texture) =>
				 {
					 Avator_Image.texture = null;
					 Avator_Image.texture = texture as Texture;
					 Avator_Image.DOColor(Color.white, 0.4f);
				 });
			}
		}
	}

	private void SetBtn()
	{
		Skip_Btn.interactable = true;
		Skip_Btn.SetBtnEvent(() =>
		{
			GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.DecorationScaleDown));
			if (sequence != null && !sequence.IsComplete()) sequence.Kill(true);
			//skip
			PlayFinishAnim(() =>
			{
				GameManager.UI.HideUIForm(this);
			});
		});
		Next_Btn.gameObject.SetActive(true);
		Next_Btn.SetBtnEvent(() =>
		{
			if (recordTweenCallback.IsPlaying())
			{
				recordTweenCallback.Kill(true);
				return;
			}
			if (storyIndex == curStoryDict.Keys.Last())
			{
				Next_Btn.gameObject.SetActive(false);
				//hide

				PlayFinishAnim(() =>
				{
					GameManager.UI.HideUIForm(this);
				});
				return;
			}

			//next story
			storyIndex++;
			SetText();
			SetAvatorImage();
		});
	}

	public void OnEscapeBtnClicked()
    {
		if (!Skip_Btn.isActiveAndEnabled || !Skip_Btn.interactable)
			return;

		Skip_Btn?.onClick?.Invoke();
	}
}
