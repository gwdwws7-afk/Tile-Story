using System;
using UnityEngine;


/// <summary>
/// ÅÀÌÙ½×¶Î¹ö¶¯À¸
/// </summary>
public class ClimbBeanstalkScrollColumn : ScrollColumn
{
    private string m_ColumnName;
    private ClimbBeanstalkTaskDatas m_Data;
    private ClimbBeanstalkMenu m_BalloonRiseMenu;
    private bool m_IsSpawning;
    private int m_Stage;

    public override string ColumnName => m_ColumnName;

    public int Stage => m_Stage;

    private string NORMAL_COLUMN_PREFAB_NAME
    {
        get
        {
            if (ClimbBeanstalkManager.Instance.ActivityTypeIndex == 0)
                return "ClimbBeanstalkGO";
            return "ClimbBeanstalkGO1";
        }
    }
    private string LAST_COLUMN_PREFAB_NAME
    {
        get
        {
            if (ClimbBeanstalkManager.Instance.ActivityTypeIndex == 0)
                return "ClimbBeanstalkDestinationGO";
            return "ClimbBeanstalkDestinationGO1";
        }
    }

    public ClimbBeanstalkScrollColumn(string columnName, ClimbBeanstalkTaskDatas data, float height, ClimbBeanstalkMenu menu)
    {
        m_ColumnName = columnName;
        m_Data = data;
        m_IsSpawning = false;
        instance = null;
        this.height = height;
        m_BalloonRiseMenu = menu;
        this.scrollArea = menu.scrollArea;
        if (data != null)
            m_Stage = data.TargetNum;
    }

    public override void Spawn(Action<bool> callback)
    {
        if (m_IsSpawning)
            return;

        if (instance == null)
        {
            m_IsSpawning = true;
            GameManager.ObjectPool.Spawn<ColumnObject>(m_ColumnName, "ClimbBeanstalkColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                if (!m_IsSpawning)
                {
                    GameManager.ObjectPool.Unspawn<ColumnObject>("ClimbBeanstalkColumnPool", target);
                    return;
                }

                instance = target;

                callback?.Invoke(true);

                if (m_ColumnName == NORMAL_COLUMN_PREFAB_NAME || m_ColumnName == LAST_COLUMN_PREFAB_NAME)
                    target.GetComponent<ClimbBeanstalkGO>().Init(m_Data, m_BalloonRiseMenu);

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
            if (m_ColumnName == NORMAL_COLUMN_PREFAB_NAME || m_ColumnName == LAST_COLUMN_PREFAB_NAME)
                instance.GetComponent<ClimbBeanstalkGO>().Release();

            GameManager.ObjectPool.Unspawn<ColumnObject>("ClimbBeanstalkColumnPool", instance);
            instance = null;
        }
    }

    public override void Release()
    {
        m_IsSpawning = false;

        if (instance != null)
        {
            if (m_ColumnName == NORMAL_COLUMN_PREFAB_NAME || m_ColumnName == LAST_COLUMN_PREFAB_NAME)
                instance.GetComponent<ClimbBeanstalkGO>().Release();

            GameManager.ObjectPool.Unspawn<ColumnObject>("ClimbBeanstalkColumnPool", instance);
            instance = null;
        }
    }

    public override float Refresh()
    {
        if (instance != null && (m_ColumnName == NORMAL_COLUMN_PREFAB_NAME || m_ColumnName == LAST_COLUMN_PREFAB_NAME))
        {
            return instance.GetComponent<ClimbBeanstalkGO>().RefreshLayout(false);
        }

        return 0;
    }

    public override bool CheckSpawnComplete()
    {
        return !m_IsSpawning;
    }
}
