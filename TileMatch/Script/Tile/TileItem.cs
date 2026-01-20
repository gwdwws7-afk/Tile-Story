using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum TileMoveDirectionType
{
	None=1,
	Up=-3,
	Down=-2,
	Left=-1,
	Right=0,
}

[Flags]
public enum TileItemState
{
	None=0,

	DisableClick=1,
}

/// <summary>
/// 棋子实例
/// </summary>
public class TileItem : MonoBehaviour
{
	[SerializeField]
	private float YDelay = 0.02f;
	[SerializeField]
	private float XDelay = 0.04f;
	[SerializeField]
	private float LayerDelay = 0.06f;
	[SerializeField]
	private float TotalAnimTime = 0.8f;

	public TileItemData Data;
	public CallBack<TileItem> ClickEvent;

	public bool IsBeCover;

	[SerializeField]
	private Image Image;
	[SerializeField]
	private TileDelayButton Btn;

	public TileItemState State;
	public AttachLogic AttachLogic;
	public TileClickInfo ClickInfo;
	public bool IsDestroyed;
	public bool IsCanClick;

	public void Init(int layer, int mapIndex, int tileIndex, int attachIndex, TileMoveDirectionType moveIndex,
		Dictionary<int, List<int>> coverIndexs = null, Dictionary<int, List<int>> beCoverIndexs = null,
		CallBack<TileItem> btnEvent = null)
	{
		Init(layer, mapIndex, tileIndex, attachIndex, 1, moveIndex, coverIndexs, beCoverIndexs, btnEvent);
	}

    public void Init(int layer, int mapIndex, int tileIndex, int attachIndex, int attachState, TileMoveDirectionType moveIndex,
        Dictionary<int, List<int>> coverIndexs = null, Dictionary<int, List<int>> beCoverIndexs = null,
        CallBack<TileItem> btnEvent = null)
    {
		Init(layer, mapIndex, tileIndex, attachIndex, attachState, moveIndex, Vector3.back, coverIndexs, beCoverIndexs, btnEvent, false);
	}

	public void Init(int layer, int mapIndex, int tileIndex, int attachId, int attachState, TileMoveDirectionType moveIndex, Vector3 targetPos,
		Dictionary<int, List<int>> coverIndexs = null, Dictionary<int, List<int>> beCoverIndexs = null,
		CallBack<TileItem> btnEvent = null, bool isHaveAnim = true, bool triggerAnimFinishedEvent = true)
	{
		this.Data = new TileItemData(layer, mapIndex, tileIndex, attachId, moveIndex, coverIndexs, beCoverIndexs);
		if (attachId == 3)
		{
			Btn.DisableButtonAnim = true;
		}
		this.ClickEvent = btnEvent;

		SetImage();
		SetAttachment(attachState);
		// if (layer != 0 && mapIndex != 0)
		// 	ClickInfo = null;
		IsDestroyed = false;
		IsCanClick = true;
		Btn.interactable = false;

		void OnAnimCompleted()
        {
			Btn.interactable = true;
			SetBtnEvent();
			SetColor();

			if(triggerAnimFinishedEvent)
				OnShowAnimFinished();
		}

		if (isHaveAnim)
		{
			transform.localPosition = new Vector3(targetPos.x, targetPos.y + 1800);

			float delayTime = Mathf.Min(2f, (16 - (mapIndex / 16)) * YDelay) + Mathf.Min(1f, (mapIndex % 16) * XDelay) + Mathf.Min(1f, layer * LayerDelay);
			AttachLogic?.Hide();

			// DOTween.Sequence()
			// 	.SetDelay(delayTime)
			// 	.Append(transform.DOLocalMove(targetPos, TotalAnimTime).SetEase(Ease.OutSine).OnComplete(OnAnimCompleted));
			transform.DOLocalMove(targetPos, TotalAnimTime).SetEase(Ease.OutSine).SetDelay(delayTime)
				.OnComplete(OnAnimCompleted);
		}
		else
		{
			if (targetPos != Vector3.back) 
				transform.localPosition = targetPos;
			OnAnimCompleted();
		}
	}

	protected virtual void OnShowAnimFinished()
    {
		AttachLogic?.Show();
	}

	public void RemoveIndex(int layer, int mapIndex)
	{
		if (this.Data.CoverIndexs!=null&& this.Data.CoverIndexs.ContainsKey(layer)) this.Data.CoverIndexs[layer].Remove(mapIndex);
		if (this.Data.BeCoverIndexs !=null&& this.Data.BeCoverIndexs.ContainsKey(layer)) this.Data.BeCoverIndexs[layer].Remove(mapIndex);
		//刷新遮挡关系
		SetColor();
	}

	public void RemoveCoverIndexs(int layer, int mapIndex)
	{
		if (this.Data.CoverIndexs.ContainsKey(layer)) this.Data.CoverIndexs[layer].Remove(mapIndex);
	}

	public void HandleClickEvent()
	{
		//技能专用
		this.ClickEvent?.Invoke(this);
	}

	public void SetClickState(bool isActive)
	{
		IsCanClick = isActive;
		Btn.interactable = isActive;
	}

	public void DestroyTile(bool isPlayAudio = true, string destroySound = null, CallBack finishCallBack = null)
	{
		try
		{
			if (isPlayAudio)
			{
				//播放震动
				UnityUtil.EVibatorType.Short.PlayerVibrator();
				//播放音效
				GameManager.Sound.PlayAudio(destroySound ?? "SFX_DestroyTile_new");
			}
			//播放特效
			GameManager.Task.AddDelayTriggerTask(0.02f, () =>
			{
				PlayEffect(null);
			});

			GameManager.Task.AddDelayTriggerTask(0.2f, () =>
			{
				finishCallBack?.Invoke();
			});
		}
		catch(Exception e)
        {
			Log.Error("DestroyTile fail - " + e.Message);
        }

		transform.DOKill();
		transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutSine);
        OnDestroyTile();
        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
		{
			GameManager.ObjectPool.Unspawn<EffectObject>("TileItemPool", gameObject);
		});
    }

	public virtual void OnDestroyTile()
	{
	}


    public virtual void SetImage()
	{
		try
		{
#if UNITY_EDITOR
			gameObject.name = $"{this.Data.Layer}_{this.Data.MapID % 16}_{this.Data.MapID / 16}_{this.Data.TileID}";
#endif
			Image.sprite = TileMatchUtil.GetTileSprite(this.Data.TileID);
		}
		catch
		{
			Log.Error($"SetImage error:{this.Data.Layer}_{this.Data.MapID}_{this.Data.TileID}");
		}
	}

	public void SetAttachment(int attachState)
    {
		try
		{
			if (this.Data.AttachID != 0) 
            {
				int attachId = this.Data.AttachID;
				if (AttachLogic != null)
                {
					AttachLogic.Release(false, false);
					AttachLogic = null;
				}

                switch (attachId)
                {
					case 1:
						AttachLogic = new AttachLogic_1();
						break;
					case 2:
						AttachLogic = new AttachLogic_2();
						break;
					case 3:
						AttachLogic = new AttachLogic_3();
						break;
					case 4:
						AttachLogic = new AttachLogic_4();
						break;
					case 5:
					case 6:
						AttachLogic = new AttachLogic_5_6();
						break;
				}

                if (attachId != 5 && attachId != 6)
                {
					AttachLogic.Init(this);
				}
                else
                {
					AttachLogic.Init(this, attachId == 5 ? "close" : "open");
				}

				if (attachState > 1)
				{
					AttachLogic.attachState = attachState;
					AttachLogic.SetSprite(attachState);
				}
                else
                {
					AttachLogic.attachState = 1;
				}
			}
            else
            {
				if (AttachLogic != null)
				{
					AttachLogic.Release(false);
					AttachLogic = null;
				}
			}
		}
		catch
		{
			Log.Error($"SetAttachment error:{this.Data.Layer}_{this.Data.MapID}_{this.Data.TileID}_{this.Data.AttachID}");
		}
	}

	public void PlayShakeAnim()
	{
		Transform cachedTransform = transform;
		cachedTransform.DOScale(Vector3.one * 1.2f, 0.15f).OnComplete(() =>
		   {
			   cachedTransform.DOShakeRotation(0.15f, 10, 1, 1).OnComplete(() =>
			   {
				   cachedTransform.DOScale(Vector3.one, 0.15f);
			   });
		   });
	}

	Sequence Sequence = null;
	public void PlayImageAnim(bool isPlay)
	{
		if (Sequence != null)
		{
			Sequence.Kill(true);
		}
		Transform cachedTrans = Image.transform;
		cachedTrans.DOKill();

		if (isPlay)
		{
			Sequence = DOTween.Sequence()
				.AppendInterval(1f)
				.Append(cachedTrans.DOScale(Vector3.one * 1.24f, 0.6f).SetEase(Ease.OutBack))
				.Append(cachedTrans.DOShakeRotation(1.2f, Vector3.forward * 15, 6, randomness: 15, fadeOut: false, ShakeRandomnessMode.Harmonic))
				.Append(cachedTrans.DOScale(Vector3.one, 0.4f))
				.AppendInterval(2f)
				.SetLoops(-1, LoopType.Restart).OnKill(() => Sequence = null);
		}
		else
		{
			cachedTrans.localScale = Vector3.one;
			cachedTrans.localEulerAngles = Vector3.zero;
			cachedTrans.localPosition = Vector3.zero;
		}
	}

	public void PlayPropAbsorbAnim(bool isPlay,CallBack animCenterCallBack,CallBack finishCallBack)
	{
		if (Sequence != null)
		{
			Sequence.Kill(true);
		}

		Transform cacheTrans = Image.transform;
		if (isPlay)
		{
			Image.DOColor(Color.white, 0.2f);

			cacheTrans.DOKill();

			Sequence = DOTween.Sequence()
				.AppendInterval(0.2f)
				.Append(cacheTrans.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack))
				.Append(cacheTrans.DOLocalRotate(Vector3.forward * 12, 0.1f).SetLoops(4, LoopType.Yoyo))
				.AppendInterval(0.4f)
				.AppendCallback(()=>animCenterCallBack?.Invoke())
				.AppendCallback(() => finishCallBack?.Invoke())
				.Join(cacheTrans.DOScale(Vector3.one, 0.2f))
				.SetAutoKill(true).OnKill(()=>Sequence=null);

			GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
			"SpineTilePropAnims",
			"TileItemDestroyEffectPool",
			1f,
			transform.position,
			transform.rotation,
			transform);
		}
		else
		{
			cacheTrans.localScale = Vector3.one;
			cacheTrans.localEulerAngles = Vector3.zero;
			cacheTrans.localPosition = Vector3.zero;
		}
	}

	private void SetBtnEvent()
	{
		Btn.SetBtnEvent(() => 
		{
            if (AttachLogic != null)
            {
				AttachLogic.OnClick();
			}

			if (!State.HasFlag(TileItemState.DisableClick)) 
            {
				this.ClickEvent?.Invoke(this);
			}
		});
	}

	public void SetColor()
	{
		IsBeCover = IsTileBeCover();
		if (!IsBeCover)
		{
			if (AttachLogic != null && AttachLogic.AttachId == 2)
			{
				AttachLogic_2 glueLogic = AttachLogic as AttachLogic_2;
				if (glueLogic.TileItem != null && glueLogic.BindAttachment != null && glueLogic.BindAttachment.TileItem != null)
				{
					if (glueLogic.TileItem.IsBeCover || glueLogic.BindAttachment.TileItem.IsTileBeCover()) 
					{
						SetCoverState();
						glueLogic.BindAttachment.TileItem.SetCoverState();
					}
                    else
                    {
						SetUncoverState();
						glueLogic.BindAttachment.TileItem.SetUncoverState();
					}
				}
                else
                {
					SetUncoverState();
				}
			}
            else
            {
				SetUncoverState();
			}
		}
		else
		{
			SetCoverState();
		}
		Btn.interactable = !IsBeCover;

        if (AttachLogic != null)
			AttachLogic.SetColor(IsBeCover);
	}
	
	public virtual void SetCoverState()
    {
		Image.DOKill();
		Image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
	}

	public virtual void SetUncoverState()
    {
		Image.DOColor(Color.white, 0.2f);
	}

	public virtual void StopAllAnim()
    {
		if (AttachLogic != null)
			AttachLogic.StopAllAnim();
    }

	public virtual void StartAllAnim()
	{
        if (AttachLogic != null)
            AttachLogic.StartAllAnim();
    }

	public bool IsTileBeCover()
	{
		if (this.Data.BeCoverIndexs != null) 
        {
			foreach (var layer in this.Data.BeCoverIndexs)
			{
				if (layer.Value != null && layer.Value.Count > 0) return true;
			}
		}

		return false;
	}

	private void PlayEffect(CallBack finishCallBack)
	{
		GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
			"TileDestroyEffect",
			"TileItemDestroyEffectPool",
			1f,
			transform.position,
			transform.rotation,
			transform.parent,
			obj =>
			{
				finishCallBack?.Invoke();
			});
	}

	private void OnEnable()
	{
		if (IsBeCover)
			Image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		else
			Image.color = Color.white;
	}

	private void OnDisable()
	{
		StopAllAnim();
		if (Sequence != null) 
        {
			Sequence.Kill(true);
		}
		Image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
	}

	private void OnDestroy()
	{
		Image.sprite = null;

		if (AttachLogic != null)
        {
			AttachLogic.Release(false, true, true);
			AttachLogic = null;
		}
	}
}

[Serializable]
public class TileItemData
{
	public int Layer;
	public int MapID;
	public int TileID;
	public int AttachID;
	public TileMoveDirectionType MoveIndex;
	public Dictionary<int, List<int>> CoverIndexs;
	public Dictionary<int, List<int>> BeCoverIndexs;

	public TileItemData(
		int layer,
		int mapIndex,
		int tileID,
		int attachID,
		TileMoveDirectionType moveIndex,
		Dictionary<int, List<int>> coverIndexs,
		Dictionary<int, List<int>> beCoverIndexs)
	{
		this.Layer = layer;
		this.MapID = mapIndex;
		this.TileID = tileID;
		this.AttachID = attachID;
		this.MoveIndex = moveIndex;
		this.CoverIndexs = coverIndexs;
		this.BeCoverIndexs = beCoverIndexs;
	}

	public static TileItemData CopyTo(TileItemData data)
	{
		//return Newtonsoft.Json.JsonConvert.DeserializeObject<TileItemData>(Newtonsoft.Json.JsonConvert.SerializeObject(data));
		return new TileItemData(
			data.Layer,
			data.MapID,
			data.TileID,
			data.AttachID,
			data.MoveIndex,
			data.CoverIndexs != null ? new Dictionary<int, List<int>>(data.CoverIndexs) : null,
			data.BeCoverIndexs != null ? new Dictionary<int, List<int>>(data.BeCoverIndexs) : null);
	}

	public static TileItemData CopyTo(TileItem tileItem)
	{
		return CopyTo(tileItem.Data);
	}
}
