using DG.Tweening;
using System;
using UnityEngine;

public class LeaderBoardRankScrollColumn : ScrollColumn
{
    private string m_ScrollName;
    private ScrollArea m_ScrollArea;
    private PersonRankData m_RankData;
    private bool m_IsSpawning;
    private bool m_IsTmp;

    public LeaderBoardRankScrollColumn(ScrollArea scrollArea, PersonRankData data, int idx, float height = 155f, bool isTmp = false)
    {
        m_RankData = data;
        m_ScrollName = data.IsSelf() ? "PersonRankMyPanel" : "PersonRankPanel";
        m_ScrollArea = scrollArea;
        this.height = height;
        index = idx;
        this.m_IsTmp = isTmp;
    }

    public override void Spawn(Action<bool> callback)
    {
        if (m_IsSpawning || m_IsTmp)
            return;
        if (instance == null)
        {
            m_IsSpawning = true;
            GameManager.ObjectPool.Spawn<ColumnObject>(m_ScrollName, "LeaderBoardRankPool", Vector3.zero,
                Quaternion.identity,
                m_ScrollArea.content, (obj) =>
                {
                    if (!m_IsSpawning)
                    {
                        GameManager.ObjectPool.Unspawn<ColumnObject>("LeaderBoardRankPool", obj.Target as GameObject);
                        return;
                    }

                    instance = obj.Target as GameObject;
                    callback?.Invoke(true);
                    instance.GetComponent<PersonRankPanelManager>().InitPanel(m_RankData, index + 1);
                    m_IsSpawning = false;
                });
        }
        else
        {
            callback?.Invoke(false);
        }
    }

    public override void Unspawn()
    {
        m_IsSpawning = false;
        if (instance != null)
        {
            instance.transform.DOKill();
            //instance.GetComponent<PersonRankPanelManager>().OnReset();
            GameManager.ObjectPool.Unspawn<ColumnObject>("LeaderBoardRankPool", instance);
            instance = null;
        }
    }

    public override void Release()
    {
        m_IsSpawning = false;
        if (instance != null)
        {
            instance.transform.DOKill();
            instance.GetComponent<PersonRankPanelManager>().OnReset();
            GameManager.ObjectPool.Unspawn<ColumnObject>("LeaderBoardRankPool", instance);
            instance = null;
        }
    }

    public override float Refresh()
    {
        if (!ReferenceEquals(instance, null))
            instance.GetComponent<PersonRankPanelManager>().UpdatePanel(index + 1);
        return 0;
        var data = GameManager.Task.PersonRankManager.RankDatas.Find(x => x.Rank == index + 1);
        if (data == null)
        {
            return -1;
        }

        m_RankData = data;
        m_ScrollName = data.IsSelf() ? "PersonRankMyPanel" : "PersonRankPanel";
        if (!ReferenceEquals(instance, null))
            instance.GetComponent<PersonRankPanelManager>().InitPanel(m_RankData);
        return 0;
    }

    public override bool CheckSpawnComplete()
    {
        return !m_IsSpawning;
    }
}