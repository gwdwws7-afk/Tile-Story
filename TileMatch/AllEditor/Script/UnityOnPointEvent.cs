using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnityOnPointEvent : Button
{
	public CallBack<Vector2> OnLeftMouseEvent;
	public CallBack OnRightMouseEvent;

	private PointerEventData.InputButton? input;

	private int pressIndex= 0;
	private bool active = false;

    protected override void OnDisable()
    {
		active = false;
		input = null;
		pressIndex = 0;

		base.OnDisable();
    }

    public override void OnPointerDown(PointerEventData eventData)
	{
		active = true;
		input = eventData.button;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (pressIndex == 2)
		{
			if (OnRightMouseEvent == null) return;
			OnRightMouseEvent();
		}
		input = eventData.button;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		input = null;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		active = false;
		input = null;
	}

	private void Update()
	{
		if (active && input != null)
		{
			if (input == PointerEventData.InputButton.Left)
			{
				OnLeftMouseEvent?.Invoke(Input.mousePosition);
			}
			else if (input == PointerEventData.InputButton.Right)
			{
				OnRightMouseEvent?.Invoke();
			}
		}
		if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
		{
			active = false;
			pressIndex = 0;
		}

		if (Input.GetMouseButtonDown(0))
		{
			active = true;
			pressIndex = 1;
		}
		else if(Input.GetMouseButtonDown(1))
		{
			pressIndex = 2;
		}
	}


	public virtual void SetColor(Color color)
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
		if (image != null)
			image.color = color;
	}

	public void SetSprite(Sprite sprite)
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
		if (image != null)
			image.sprite = sprite;
	}

	public void SetEnable(bool isActive)
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
		if (image != null)
			image.raycastTarget = isActive;
		enabled = isActive;
	}

	public static UnityOnPointEvent Get(GameObject gameObject)
	{
		var unityOnPointEvent = gameObject.GetComponent<UnityOnPointEvent>();
		if (unityOnPointEvent == null)
		{
			unityOnPointEvent=gameObject.AddComponent<UnityOnPointEvent>();
		}
		return unityOnPointEvent;
	}
}
