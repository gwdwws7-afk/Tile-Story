using System;
using UnityEngine;

public class TilePassScrollColumn : ScrollColumn
{
    public override string ColumnName => "TilePassColumn";

    private TilePassData m_Data;
    private TilePassMainMenu m_TilePassMainMenu;
    private bool m_IsSpawning;

    public TilePassScrollColumn(int height, TilePassData data, TilePassMainMenu tilePassMainMenu)
    {
        instance = null;
        this.height = height;
        m_Data = data;
        index = data.Index;
        m_TilePassMainMenu = tilePassMainMenu;
        scrollArea = tilePassMainMenu.scrollArea;
        m_IsSpawning = false;
    }

    public override void Spawn(Action<bool> callback)
    {
        if (instance == null && !m_IsSpawning)
        {
            m_IsSpawning = true;
            GameManager.ObjectPool.Spawn<ColumnObject>(ColumnName, "TilePassColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                if (!m_IsSpawning)
                {
                    GameManager.ObjectPool.Unspawn<ColumnObject>("TilePassColumnPool", target);
                    return;
                }
                instance = target;
                callback?.Invoke(true);
                target.GetComponent<TilePassColumn>().OnInit(m_TilePassMainMenu, m_Data);
                m_IsSpawning = false;
            });
        }
    }

    public override void Unspawn()
    {
        m_IsSpawning = false;
        if (instance != null)
        {
            instance.GetComponent<TilePassColumn>().OnRelease();
            GameManager.ObjectPool.Unspawn<ColumnObject>("TilePassColumnPool", instance);
            instance = null;
        }
    }

    public override float Refresh()
    {
        float duration = 0;
        if (instance != null)
        {
            duration = instance.GetComponent<TilePassColumn>().RefreshLayout();
        }
        return duration;
    }

    public override void Release()
    {
        Unspawn();
    }

    public override void Unlock()
    {
        if (instance != null)
        {
            instance.GetComponent<TilePassColumn>().Unlock();
        }
    }

    public override void RefreshRewardStatus()
    {
        if (instance != null)
        {
            instance.GetComponent<TilePassColumn>().RefreshRewardStatus();
        }
    }

    public override void ClaimAll()
    {
        if (instance != null)
        {
            TilePassColumn column = instance.GetComponent<TilePassColumn>();
            column.OnFreeClaimButtonClicked();
            column.OnVIPClaimButtonClicked();
        }
    }

    public override bool CheckSpawnComplete()
    {
        return !m_IsSpawning;
    }
}
