using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MySelf.Model;

public class MapDecorationBGPanel : UIForm
{
	[SerializeField]
	private Transform decorationAreaRoot;
	[SerializeField]
	private Transform decorateBgEffectRoot;
	[SerializeField]
	private DecorationBGDialogBubble dialogBubble;

	private DecorationArea recentDecorationArea;
	private List<IdleDialogueData> idleDialogTermList = new List<IdleDialogueData>();
	private int lastIdleDialogueDataID;
	private List<ClickToPlayDialog> ctpdScriptInThisAreaList = new List<ClickToPlayDialog>();

	//通过即时释放装修来降低内存
	public override bool IsAutoRelease => false;
	private bool initAreaFinished = false;
	private string m_IsChangingAreaName = null;

	public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
	{
		GameManager.Event.Subscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
		GameManager.Event.Subscribe(CommonEventArgs.EventId, OnChangeSize);
		
		ShowDecorationArea();
		HideDialogBubble();

		base.OnInit(uiGroup, completeAction, userData);
	}

	public override void OnRelease()
	{
		GameManager.Event.Unsubscribe(ChangeDecorationAreaEventArgs.EventId, OnDecorationAreaChange);
		GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnChangeSize);

        if (recentDecorationArea != null)
        {
            recentDecorationArea.OnRelease();
            if (recentDecorationArea) UnityUtility.UnloadInstance(recentDecorationArea.gameObject);
            recentDecorationArea = null;
        }
        m_IsChangingAreaName = null;

		base.OnRelease();
	}

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
		HideDialogBubble();
		ChangeAllDecorationObjectPrefabToNormalMaterial();
	}
	public override bool CheckInitComplete()
	{
		return initAreaFinished;
	}

	private float accumulatedTime = 0.0f;
    private float autoPlayDialogBubbleInterval = 60.0f;
	private float timeToCheckPanelState = 1.0f;
	
	public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
	{
		timeToCheckPanelState -= Time.deltaTime;
		if (timeToCheckPanelState <= 0)
		{
			timeToCheckPanelState = 1.0f;
			if (!MapTopPanelIsOnTop())
			{
				accumulatedTime = 0;
			}
		}
		
		if (CheckClickOrTouch())
        {
			accumulatedTime = 0;
        }
		else
        {
			accumulatedTime += Time.deltaTime;
			if (accumulatedTime > autoPlayDialogBubbleInterval)
			{
				accumulatedTime = 0;
				PlayARandomIdleDialogue();

			}
		}
		base.OnUpdate(elapseSeconds,realElapseSeconds);
	}

    private void ShowDecorationArea()
	{
		int areaId = DecorationModel.Instance.GetDecorationOperatingAreaID();
		string nowAreaResourceName = DecorationModel.GetAreaNameById(areaId);
		if (areaId >= Constant.GameConfig.StartNeedDownloadArea && !AddressableUtils.IsHaveAsset(DecorationModel.Instance.GetNowAreaResourceName())) 
        {
			DecorationModel.Instance.SetTempDecoratingOperatingAreaID(1);
			return;
		}

		if (recentDecorationArea)
        {

        }
        else
        {
			initAreaFinished = false;
			UnityUtility.InstantiateAsync(nowAreaResourceName, decorationAreaRoot, obj =>
	        {
		        if (obj != null)
		        {
			        if (recentDecorationArea != null)
			        {
				        initAreaFinished = true;
				        UnityUtility.UnloadInstance(obj);
				        return;
			        }
			        
			        recentDecorationArea = obj.GetComponent<DecorationArea>();
			        recentDecorationArea.InitArea(() =>
			        {
				        initAreaFinished = true;
				        InitClickToPlayDialogScript();
			        });   
		        }
		        else
		        {
			        GameManager.Task.AddDelayTriggerTask(0, () =>
			        {
				        UnityUtility.InstantiateAsync(nowAreaResourceName, decorationAreaRoot, obj =>
				        {
					        if (obj != null)
					        {
						        if (recentDecorationArea != null)
						        {
							        initAreaFinished = true;
							        UnityUtility.UnloadInstance(obj);
							        return;
						        }
			        
						        recentDecorationArea = obj.GetComponent<DecorationArea>();
						        recentDecorationArea.InitArea(() =>
						        {
							        initAreaFinished = true;
							        InitClickToPlayDialogScript();
						        });   
					        }
				        });
			        });
		        }
	        });
        }
	}

	private void ChangeDecorationArea()
    {
		string nowAreaResourceName = DecorationModel.Instance.GetNowAreaResourceName();
		if (m_IsChangingAreaName == nowAreaResourceName)
			return;
		m_IsChangingAreaName = nowAreaResourceName;

		initAreaFinished = false;
		UnityUtility.InstantiateAsync(nowAreaResourceName, decorationAreaRoot, obj =>
		{
			m_IsChangingAreaName = null;

			if (obj == null)
            {
				Log.Error("DecorationArea {0} Instantiate fail.", nowAreaResourceName);
				DecorationModel.Instance.SetTempDecoratingOperatingAreaID(1);
				return;
			}

			DecorationArea formerDecorationArea = recentDecorationArea;
			recentDecorationArea = obj.GetComponent<DecorationArea>();
			recentDecorationArea.InitArea(() =>
			{
				initAreaFinished = true;
				InitClickToPlayDialogScript();
			});

			GameManager.Task.AddDelayTriggerTask(2.0f, () =>
			{
				if (formerDecorationArea != null)
				{
					formerDecorationArea.OnRelease();
					UnityUtility.UnloadInstance(formerDecorationArea.gameObject);
				}
			});
		});
	}

	public Vector3[] GetAllUnlockBtnPosition()
    {
		if (recentDecorationArea != null)
		{
			return recentDecorationArea.GetAllObjectPosition();
		}
		return null;
	}

	public void ChangeAllDecorationObjectPrefabMaterial()
    {
		if (recentDecorationArea != null)
		{
			recentDecorationArea.ChangeAllDecorationObjectPrefabMaterial();
		}
	}

	public void ChangeAllDecorationObjectPrefabToNormalMaterial()
    {
		if (recentDecorationArea != null)
		{
			recentDecorationArea.ChangeAllDecorationObjectPrefabToNormalMaterial();
		}
	}

	public void FirstTimeDecorate(int positionFlag, bool isQuick = false)
	{
		if (recentDecorationArea != null)
		{
			recentDecorationArea.FirstTimeDecorate(positionFlag, isQuick);
		}
	}

	public void PlayCharacterAction()
    {
		if (recentDecorationArea != null)
		{
			recentDecorationArea.PlayCharacterAction();
		}
	}


    #region DialogBubble
    public void HideDialogBubble()
    {
		dialogBubble.Hide();
	}

	public void PlayDialogBubble(int characterIndex, Transform dialogBubblePosRef, bool dialogFaceLeft, bool useTriangleEdge, bool skipClickSound)
    {
		IdleDialogueData dialogueData = GetARandomIdleDialog(characterIndex);
		if (GameManager.UI.HasUIForm("DecorationOperationPanel"))
		{
			DecorationOperationPanel panel = GameManager.UI.GetUIForm("DecorationOperationPanel") as DecorationOperationPanel;
			if (panel != null && panel.isActiveAndEnabled)
			{
				panel.PlayDialogBubble(dialogueData, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge, skipClickSound);
				return;
			}
		}

        if (!skipClickSound)
			GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
		dialogBubble.ShowTalk(dialogueData, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge);
    }

	public void PlayDialogBubble(Transform dialogBubblePosRef, bool dialogFaceLeft, bool useTriangleEdge, bool skipClickSound, string specificTerm)
	{
		if (GameManager.UI.HasUIForm("DecorationOperationPanel"))
		{
			DecorationOperationPanel panel = GameManager.UI.GetUIForm("DecorationOperationPanel") as DecorationOperationPanel;
			if (panel != null)
				panel.PlayDialogBubble(specificTerm, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge, skipClickSound);
			return;
		}

		if (!skipClickSound)
			GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
		dialogBubble.ShowTalk(specificTerm, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge);
	}

	public void InitClickToPlayDialogScript()
	{
		ctpdScriptInThisAreaList.Clear();
		ctpdScriptInThisAreaList.AddRange(recentDecorationArea.GetComponentsInChildren<ClickToPlayDialog>());

		for (int i = 0; i < ctpdScriptInThisAreaList.Count; ++i)
		{
			ctpdScriptInThisAreaList[i].Init(this);
		}
	}

	public void PlayDialogueForSpecificCharacter(int characterIndex, string term)
    {
		for (int i = 0; i < ctpdScriptInThisAreaList.Count; ++i)
		{
			ctpdScriptInThisAreaList[i].PlayDialogue(characterIndex, term);
		}
	}

	public void PlayARandomIdleDialogue()
	{
		int result = UnityEngine.Random.Range(0, ctpdScriptInThisAreaList.Count);
		if (ctpdScriptInThisAreaList[result] != null)
			ctpdScriptInThisAreaList[result].OnButtonClicked(true);
	}

	private IdleDialogueData GetARandomIdleDialog(int characterIndex)
	{
		idleDialogTermList.Clear();
		int areaID = DecorationModel.Instance.GetDecorationOperatingAreaID();
		int decorationProgress = DecorationModel.Instance.GetTargetAreaFinishedDecorationCount(areaID);
		idleDialogTermList = GameManager.DataTable.GetDataTable<DTIdleDialogue>().Data.GetAllAvailableIdleDialogueData(areaID, decorationProgress, characterIndex);

		if (lastIdleDialogueDataID > 0 && idleDialogTermList.Count > 1)
			idleDialogTermList.RemoveAll(data => data.ID == lastIdleDialogueDataID);

		int randomResult = UnityEngine.Random.Range(0, idleDialogTermList.Count);
		if (idleDialogTermList.Count > 0)
		{
			lastIdleDialogueDataID = idleDialogTermList[randomResult].ID;
			return idleDialogTermList[randomResult];
		}
		else
			return GameManager.DataTable.GetDataTable<DTIdleDialogue>().Data.GetDefaultDialogueData();
	}

	private bool CheckClickOrTouch()
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				return true;
			}
		}
		else
		{
			if (Input.touchCount > 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool MapTopPanelIsOnTop()
	{
		UIGroup centerGroup = GameManager.UI.GetUIGroup(UIFormType.CenterUI);
		if (centerGroup.CurrentUIForm!=null&&
		    centerGroup.CurrentUIForm.GetType() != typeof(MapTopPanelManager))
			return false;
		UIGroup popUpGroup = GameManager.UI.GetUIGroup(UIFormType.PopupUI);
		if (popUpGroup.UIFormCount > 0)
			return false;

		return true;
	}
	#endregion

	private void OnDecorationAreaChange(object sender, GameEventArgs e)
	{
		//ChangeDecorationAreaEventArgs changeEvent = (ChangeDecorationAreaEventArgs)e;
		ChangeDecorationArea();
	}
	
	private void OnChangeSize(object sender, GameEventArgs e)
	{
		CommonEventArgs ne = (CommonEventArgs)e;
		decorationAreaRoot.DOKill();
		switch (ne.Type)
		{
			case CommonEventType.DecorationScaleUp:
				decorationAreaRoot.DOScale(Vector3.one * 1.2f, 0.6f);
				break;
			case CommonEventType.DecorationScaleDown:
				decorationAreaRoot.DOScale(Vector3.one * 1f, 0.6f);
				break;
			case CommonEventType.RefreshUIBySyncData:
				if (recentDecorationArea != null)
				{
					recentDecorationArea.OnRelease();
					if(recentDecorationArea)UnityUtility.UnloadInstance(recentDecorationArea.gameObject);
					recentDecorationArea = null;
				}
				break;
		}
	}

}