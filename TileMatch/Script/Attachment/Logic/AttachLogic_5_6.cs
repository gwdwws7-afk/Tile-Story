using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurtainState
{
    None,
    Close,
    Open
}

/// <summary>
/// ´°Á±
/// </summary>
public sealed class AttachLogic_5_6 : AttachLogic
{
    public override int AttachId => m_State == CurtainState.Open ? 6 : 5;

    public override string AttachAssetName => "Curtain";

    private CurtainState m_State;

    public CurtainState State
    {
        get
        {
            return m_State;
        }
        set
        {
            if (m_State != value)
            {
                m_State = value;
                Refresh(true);
            }
        }
    }

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        if (userData != null)
        {
            if ((string)userData == "close")
                m_State = CurtainState.Close;
            else
                m_State = CurtainState.Open;
        }

        Refresh(false);
    }

    protected override void OnRelease(bool showEffect)
    {
        TileItem.State &= ~TileItemState.DisableClick;
        TileItem.GetComponent<TileDelayButton>().DisableAnim = false;

        base.OnRelease(showEffect);
    }

    protected override void ReleaseInstance()
    {
        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            base.ReleaseInstance();
        });
    }

    public override void OnAnyTileGet()
    {
        if (TileItem == null || TileItem.IsBeCover || TileItem.IsDestroyed) 
            return;

        if (!CheckAllTileIsCurtainAttach())
        {
            if (m_State == CurtainState.Open)
                State = CurtainState.Close;
            else if (m_State == CurtainState.Close)
                State = CurtainState.Open;
        }
        else
        {
            State = CurtainState.Open;
        }

        base.OnAnyTileGet();
    }

    private void Refresh(bool isShowAnim)
    {
        if (m_State == CurtainState.Open)
        {
            TileItem.State &= ~TileItemState.DisableClick;
            TileItem.GetComponent<TileDelayButton>().DisableAnim = false;
        }
        else if (m_State == CurtainState.Close)
        {
            TileItem.State |= TileItemState.DisableClick;
            TileItem.GetComponent<TileDelayButton>().DisableAnim = true;
        }

        if (isShowAnim)
        {
            Curtain curtain = AttachItem as Curtain;
            if (curtain != null)
            {
                curtain.State = m_State;
            }
        }
    }

    private bool CheckAllTileIsCurtainAttach()
    {
        Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = null;
        TileMatchPanel panel = null;
        if (GameManager.UI != null)
        {
            panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

            if (panel != null)
                dict = panel.tileMapDict;
        }

        bool result = false;
        if (dict != null)
        {
            result = true;
            foreach (var child in dict.Values)
            {
                foreach (var item in child.Values)
                {
                    if (item.Item2.AttachLogic != null && (item.Item2.AttachLogic.AttachId == 5 || item.Item2.AttachLogic.AttachId == 6))
                    {
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
            }
        }

        return result;
    }
}
