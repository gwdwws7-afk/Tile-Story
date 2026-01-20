/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.EasingCore;

public class Context : FancyScrollRectContext
{
    public int SelectedIndex = -1;
    public Action<int> OnCellClicked;
}

public enum Alignment
{
    Upper,
    Middle,
    Lower,
}

public class LoopScrollView : FancyScrollRect<int, Context>
{
    [SerializeField] float cellSize = 100f;
    [SerializeField] GameObject cellPrefab = default;

    protected override float CellSize => cellSize;
    protected override GameObject CellPrefab => cellPrefab;
    public int DataCount => ItemsSource.Count;

    public float PaddingTop
    {
        get => paddingHead;
        set
        {
            paddingHead = value;
            Relayout();
        }
    }

    public float PaddingBottom
    {
        get => paddingTail;
        set
        {
            paddingTail = value;
            Relayout();
        }
    }

    public float Spacing
    {
        get => spacing;
        set
        {
            spacing = value;
            Relayout();
        }
    }

	private void Start()
	{
        cellPrefab.gameObject.SetActive(false);
    }

	public void OnCellClicked(Action<int> callback)
    {
        Context.OnCellClicked = callback;
    }

    public void UpdateData(IList<int> items)
    {
        UpdateContents(items);
    }
    
    public void UpdateData(IList<int> items, int index)
    {
        UpdateContents(items, index);
    }

    public void ScrollTo(int index, float duration, Ease easing, Alignment alignment = Alignment.Middle)
    {
        UpdateSelection(index);
        ScrollTo(index, duration, easing, GetAlignment(alignment));
    }

    public void JumpTo(int index, Alignment alignment = Alignment.Middle)
    {
        UpdateSelection(index);
        JumpTo(index, GetAlignment(alignment));
    }

    float GetAlignment(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.Upper: return 0.0f;
            case Alignment.Middle: return 0.5f;
            case Alignment.Lower: return 1.0f;
            default: return GetAlignment(Alignment.Middle);
        }
    }

    void UpdateSelection(int index)
    {
        if (Context.SelectedIndex == index)
        {
            return;
        }

        Context.SelectedIndex = index;
        Refresh();
    }
}
