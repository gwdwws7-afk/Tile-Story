using Firebase.Extensions;
using Firebase.Firestore;
using MySelf.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 成就目标组件
/// </summary>
public sealed class ObjectiveComponent : GameFrameworkComponent, IServiceModel
{
    private DTAllTimeObjective m_AllTimeObjectiveData;
    private DTDailyObjective m_DTDailyObjectiveData;
    private List<int> m_CurAllTimeObjectiveIds = new List<int>();
    private List<int> m_CurDailyObjectiveIds = new List<int>();
    private List<int> m_ClaimedAllTimeObjectiveIds = new List<int>();
    private List<int> m_ClaimedDailyObjectiveIds = new List<int>();

    /// <summary>
    /// 当前成就目标编号
    /// </summary>
    public List<int> CurAllTimeObjectiveIds
    {
        get
        {
            return m_CurAllTimeObjectiveIds;
        }
    }

    /// <summary>
    /// 当前每日目标编号
    /// </summary>
    public List<int> CurDailyObjectiveIds
    {
        get
        {
            return m_CurDailyObjectiveIds;
        }
    }

    public void Initialize()
    {
        IDataTable<DTAllTimeObjective> alltimeDatatable = GameManager.DataTable.CreateDataTable<DTAllTimeObjective>();
        alltimeDatatable.ReadData("DTAllTimeObjective", OnReadAllTimeDataSuccess, OnReadDataFailure);

        IDataTable<DTDailyObjective> dailyObjective = GameManager.DataTable.CreateDataTable<DTDailyObjective>();
        dailyObjective.ReadData("DTDailyObjective", OnReadDailyDataSuccess, OnReadDataFailure);
    }

    public void Shutdown()
    {
        GameManager.DataTable.DestroyDataTable<DTAllTimeObjective>();
        GameManager.DataTable.DestroyDataTable<DTDailyObjective>();
        m_AllTimeObjectiveData = null;
        m_DTDailyObjectiveData = null;
        m_CurAllTimeObjectiveIds.Clear();
        m_CurDailyObjectiveIds.Clear();
        m_ClaimedAllTimeObjectiveIds.Clear();
        m_ClaimedDailyObjectiveIds.Clear();
    }

    public bool CheckObjectiveUnlock()
    {
        return GameManager.PlayerData.NowLevel >= Constant.Objective.UnlockLevel;
    }

    /// <summary>
    /// 获取类别编号
    /// </summary>
    public int GetObjectiveTypeId()
    {
        if (m_DTDailyObjectiveData == null)
        {
            Log.Error("ObjectiveComponent GetObjectiveTypeId fail - data is null");
            return 0;
        }

        int passDay = (DateTime.Now - GameManager.PlayerData.FirstDateTime).Days;
        int maxTypeId = 0;
        for (int i = 0; i < m_DTDailyObjectiveData.ObjectiveDatas.Count; i++)
        {
            if (m_DTDailyObjectiveData.ObjectiveDatas[i].TypeId > maxTypeId)
                maxTypeId = m_DTDailyObjectiveData.ObjectiveDatas[i].TypeId;
        }

        return passDay % maxTypeId + 1;
    }

    /// <summary>
    /// 刷新成就目标
    /// </summary>
    public void RefreshAllTimeObjective(bool isObjectiveClaim = false)
    {
        if (m_AllTimeObjectiveData == null)
        {
            Log.Error("ObjectiveComponent RefreshAllTimeObjective fail - data is null");
            return;
        }

        LoadClaimedObjectiveIds(true);

        if (!isObjectiveClaim)
        {
            List<int> uncompletedObjectiveIds = new List<int>();
            m_CurAllTimeObjectiveIds.Clear();
            foreach (ObjectiveData objectiveData in m_AllTimeObjectiveData.ObjectiveDatas)
            {
                if (!m_CurAllTimeObjectiveIds.Contains(objectiveData.ID) && !m_ClaimedAllTimeObjectiveIds.Contains(objectiveData.ID)
                    && (objectiveData.PreObjective == 0 || m_ClaimedAllTimeObjectiveIds.Contains(objectiveData.PreObjective) /*|| CheckObjectiveCompleted(objectiveData.PreObjective, true)*/))
                {
                    if (CheckObjectiveCompleted(objectiveData.ID, true))
                        m_CurAllTimeObjectiveIds.Add(objectiveData.ID);
                    else
                        uncompletedObjectiveIds.Add(objectiveData.ID);
                }
            }
            m_CurAllTimeObjectiveIds.AddRange(uncompletedObjectiveIds);
        }
        else
        {
            foreach (ObjectiveData objectiveData in m_AllTimeObjectiveData.ObjectiveDatas)
            {
                if (!m_CurAllTimeObjectiveIds.Contains(objectiveData.ID) && !m_ClaimedAllTimeObjectiveIds.Contains(objectiveData.ID)
                    && (objectiveData.PreObjective == 0 || m_ClaimedAllTimeObjectiveIds.Contains(objectiveData.PreObjective)))
                {
                    if (!CheckObjectiveCompleted(objectiveData.ID, true))
                    {
                        m_CurAllTimeObjectiveIds.Add(objectiveData.ID);
                    }
                    else
                    {
                        List<int> tempList = new List<int>() { objectiveData.ID };
                        tempList.AddRange(m_CurAllTimeObjectiveIds);
                        m_CurAllTimeObjectiveIds = tempList;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 刷新每日目标
    /// </summary>
    public void RefreshDailyObjective()
    {
        if (m_DTDailyObjectiveData == null)
        {
            Log.Error("ObjectiveComponent RefreshDailyObjective fail - data is null");
            return;
        }

        LoadClaimedObjectiveIds(false);

        int typeId = GetObjectiveTypeId();
        List<int> uncompletedObjectiveIds = new List<int>();
        m_CurDailyObjectiveIds.Clear();
        foreach (ObjectiveData objectiveData in m_DTDailyObjectiveData.ObjectiveDatas)
        {
            if (!m_CurDailyObjectiveIds.Contains(objectiveData.ID) && objectiveData.TypeId == typeId && !m_ClaimedDailyObjectiveIds.Contains(objectiveData.ID))
            {
                if (CheckObjectiveCompleted(objectiveData.ID, false))
                    m_CurDailyObjectiveIds.Add(objectiveData.ID);
                else
                    uncompletedObjectiveIds.Add(objectiveData.ID);
            }
        }
        m_CurDailyObjectiveIds.AddRange(uncompletedObjectiveIds);
    }

    public ObjectiveData GetAllTimeFirstObjectiveData()
    {
        List<ObjectiveData> dataList = new List<ObjectiveData>();
        foreach (ObjectiveData data in m_AllTimeObjectiveData.ObjectiveDatas)
        {
            if (m_CurAllTimeObjectiveIds.Contains(data.ID) && !m_ClaimedAllTimeObjectiveIds.Contains(data.ID))
            {
                dataList.Add(data);
            }
        }

        if (dataList.Count > 0)
        {
            dataList.Sort(Compare);

            return dataList[0];
        }

        return null;
    }

    int Compare(ObjectiveData a, ObjectiveData b)
    {
        int a_Id = a.ID;
        int b_Id = b.ID;
        var a_data = GameManager.Objective.GetObjectiveStatus(a_Id, true);
        var b_data = GameManager.Objective.GetObjectiveStatus(b_Id, true);

        if (a_data.Item2 != b_data.Item2)
        {
            return b_data.Item2 ? 1 : -1;
        }
        else if (a_data.Item2 && b_data.Item2)
        {
            if (b_Id == -1) return 1;
            if (a_Id == -1) return -1;
            return a_Id.CompareTo(b_Id);
        }
        else
        {
            if (b_data.Item1 > a_data.Item1)
            {
                return 1;
            }
            else if (b_data.Item1 == a_data.Item1)
            {
                return a_Id.CompareTo(b_Id);
            }
            else
            {
                return -1;
            }
        }
    }

    public ObjectiveData GetDailyCompletedObjectiveData()
    {
        foreach (ObjectiveData data in m_DTDailyObjectiveData.ObjectiveDatas)
        {
            if (m_CurDailyObjectiveIds.Contains(data.ID) && !m_ClaimedDailyObjectiveIds.Contains(data.ID) && CheckObjectiveCompleted(data.ID, false))
            {
                return data;
            }
        }

        return null;
    }

    public ObjectiveData GetAllTimeCompletedObjectiveData()
    {
        foreach (ObjectiveData data in m_AllTimeObjectiveData.ObjectiveDatas)
        {
            if (m_CurAllTimeObjectiveIds.Contains(data.ID) && !m_ClaimedAllTimeObjectiveIds.Contains(data.ID) && CheckObjectiveCompleted(data.ID, true))
            {
                return data;
            }
        }

        return null;
    }

    /// <summary>
    /// 确认成就目标是否已经完成
    /// </summary>
    /// <param name="id">成就目标编号</param>
    /// <param name="isAllTime">是否是成就目标</param>
    /// <returns>是否已经完成</returns>
    public bool CheckObjectiveCompleted(int id, bool isAllTime)
    {
        if (isAllTime)
        {
            if (m_AllTimeObjectiveData == null)
            {
                Log.Error("ObjectiveComponent CheckObjectiveCompleted fail - data is null");
                return false;
            }

            ObjectiveData objectiveData = m_AllTimeObjectiveData.GetData(id);
            if (objectiveData != null)
            {
                if (GetObjectiveProgress(objectiveData, true) >= objectiveData.TargetNum)
                    return true;
            }
        }
        else
        {
            if (m_DTDailyObjectiveData == null)
            {
                Log.Error("ObjectiveComponent CheckObjectiveCompleted fail - data is null");
                return false;
            }

            ObjectiveData objectiveData = m_DTDailyObjectiveData.GetData(id);
            if (objectiveData != null)
            {
                if (GetObjectiveProgress(objectiveData, false) >= objectiveData.TargetNum)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取目标进度
    /// </summary>
    /// <param name="data">目标数据</param>
    /// <param name="isAllTime">是否是成就目标</param>
    /// <returns>目标进度</returns>
    public int GetObjectiveProgress(ObjectiveData data, bool isAllTime)
    {
        if (data == null)
            return 0;

        ObjectiveType type = data.Type;
        int objectiveProgress = 0;
        if (type == ObjectiveType.Unlock_Scenes)
        {
            int nowLevel = GameManager.PlayerData.NowLevel;
            var bgItemData = GameManager.DataTable.GetDataTable<DTBGID>().Data;
            Dictionary<int, BGItemData> itemDataDic = bgItemData.BGItemDataDict;
            foreach (var dataPair in itemDataDic)
            {
                if (dataPair.Value.BGUnlockLevel != 1 && nowLevel >= dataPair.Value.BGUnlockLevel)
                {
                    if (GameManager.PlayerData.IsOwnBGID(dataPair.Key) || !bgItemData.IsNeedBuyBG(dataPair.Key))
                        objectiveProgress++;
                }
            }
        }
        else if (type == ObjectiveType.Unlock_SceneSet)
        {
            List<int> themeList = GameManager.DataTable.GetDataTable<DTBGID>().Data.BGThemeDict.Keys.ToList();
            var dtBGID = GameManager.DataTable.GetDataTable<DTBGID>().Data;
            for (int i = 0; i < themeList.Count; i++)
            {
                var bgList = dtBGID.BGThemeDict[themeList[i]];
                bool isAllUnlock = true;
                foreach (var bgData in bgList)
                {
                    if (bgData.BGPrice > 0 && !GameManager.PlayerData.IsOwnBGID(bgData.ID))
                    {
                        isAllUnlock = false;
                        break;
                    }
                }

                if (isAllUnlock)
                    objectiveProgress++;
            }
        }
        else if (type == ObjectiveType.Unlock_TileSets)
        {
            DTTileID tileIdData = GameManager.DataTable.GetDataTable<DTTileID>().Data;
            Dictionary<int, TileData> tileDataDict = tileIdData.TileDataDict;
            foreach (var dataPair in tileDataDict)
            {
                if (/*tileIdData.IsOwn(dataPair.Key) || */GameManager.PlayerData.IsOwnTileID(dataPair.Key))
                    objectiveProgress++;
            }
        }
        else if (type == ObjectiveType.Complete_Chapter)
        {
            return GameManager.PlayerData.GetHighestFinishedDecorationAreaID();
        }
        else if (type == ObjectiveType.Complete_Level)
        {
            return GameManager.PlayerData.NowLevel - 1;
        }
        else if (type == ObjectiveType.LogIntoTheGame)
        {
            return 1;
        }
        else
        {
            int defaultValue = 0;
            if (type == ObjectiveType.Continuous_Login || type == ObjectiveType.Accumulated_Login)
                defaultValue = 1;

            string prefix = isAllTime ? Constant.Objective.AllTimeObjectiveProgressPrefix : Constant.Objective.DailyObjectiveProgressPrefix;
            objectiveProgress = PlayerPrefs.GetInt(prefix + type.ToString(), defaultValue);
        }

        //if (isAllTime)
        //{
        //    int preObjective = data.PreObjective;
        //    while (preObjective > 0)
        //    {
        //        ObjectiveData preData = m_AllTimeObjectiveData.GetData(preObjective);
        //        if (preData != null && preData.Type == type)
        //        {
        //            objectiveProgress -= preData.TargetNum;
        //            preObjective = preData.PreObjective;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //}

        if (objectiveProgress < 0)
        {
            // Log.Warning("objectiveProgress is less than 0");
            objectiveProgress = 0;
        }

        return objectiveProgress;
    }

    /// <summary>
    /// 获取任务进度信息 进度、是否完成
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isAllTime"></param>
    /// <returns></returns>
    public (float, bool) GetObjectiveStatus(int id, bool isAllTime)
    {
        ObjectiveData objectiveData = null;
        if (isAllTime)
        {
            objectiveData = m_AllTimeObjectiveData.GetData(id);
        }
        else
        {
            objectiveData = m_DTDailyObjectiveData.GetData(id);
        }

        if (objectiveData == null)
        {
            return (1, true);
        }

        return (GetObjectiveProgress(objectiveData, isAllTime) / ((float)objectiveData.TargetNum), CheckObjectiveCompleted(id, isAllTime));
    }

    /// <summary>
    /// 获取关卡相关 没有完成的任务Id
    /// </summary>
    /// <returns></returns>
    public (List<int>, List<int>) GetObjectiveStatusIdByLevel(bool isComplete)
    {
        List<int> daliyIds = new List<int>();
        foreach (var id in m_CurDailyObjectiveIds)
        {
            List<int> daliyLevelType = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 12, 14 };

            ObjectiveData objectiveData = m_DTDailyObjectiveData.GetData(id);
            if (daliyLevelType.Contains((int)objectiveData.Type))
            {
                if (isComplete == CheckObjectiveCompleted(id, false))
                {
                    daliyIds.Add(id);
                }
            }
        }

        List<int> allTimeIds = new List<int>();
        foreach (var id in m_CurAllTimeObjectiveIds)
        {
            List<int> allTimeLevelType = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 12, 14 };
            ObjectiveData objectiveData = m_AllTimeObjectiveData.GetData(id);
            if (allTimeLevelType.Contains((int)objectiveData.Type))
            {
                if (isComplete == CheckObjectiveCompleted(id, true))
                {
                    allTimeIds.Add(id);
                }
            }
        }
        return (daliyIds, allTimeIds);
    }

    public (List<int>, List<int>) GetObjectiveCompleteIds((List<int>, List<int>) lastNoCompleteIds)
    {
        var completeIds = GetObjectiveStatusIdByLevel(true);
        return (
            completeIds.Item1.Intersect(lastNoCompleteIds.Item1).ToList(),
            completeIds.Item2.Intersect(lastNoCompleteIds.Item2).ToList());
    }

    /// <summary>
    /// 设置成就目标进度
    /// </summary>
    /// <param name="type">成就目标类型</param>
    /// <param name="num">改变量</param>
    public void SetObjectiveProgress(ObjectiveType type, int num)
    {
        if (type != ObjectiveType.Change_Avatar && type != ObjectiveType.Set_Name && !CheckObjectiveUnlock())
            return;

        PlayerPrefs.SetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + type.ToString(), num);
        PlayerPrefs.SetInt(Constant.Objective.DailyObjectiveProgressPrefix + type.ToString(), num);
        PlayerPrefs.Save();

        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.Objective));
    }

    /// <summary>
    /// 改变成就目标进度
    /// </summary>
    /// <param name="type">成就目标类型</param>
    /// <param name="changeNum">改变量</param>
    public void ChangeObjectiveProgress(ObjectiveType type, int changeNum)
    {
        if (type != ObjectiveType.Change_Avatar && type != ObjectiveType.Set_Name && !CheckObjectiveUnlock())
            return;

        int defaultValue = 0;
        if (type == ObjectiveType.Continuous_Login || type == ObjectiveType.Accumulated_Login)
            defaultValue = 1;

        PlayerPrefs.SetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + type.ToString(), PlayerPrefs.GetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + type.ToString(), defaultValue) + changeNum);
        PlayerPrefs.SetInt(Constant.Objective.DailyObjectiveProgressPrefix + type.ToString(), PlayerPrefs.GetInt(Constant.Objective.DailyObjectiveProgressPrefix + type.ToString(), defaultValue) + changeNum);
        PlayerPrefs.Save();

        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.Objective));
    }

    /// <summary>
    /// 清除成就目标进度
    /// </summary>
    /// <param name="type">成就目标类型</param>
    public void ClearObjectiveProgress(ObjectiveType type, bool isAllTime)
    {
        if (isAllTime)
            PlayerPrefs.SetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + type.ToString(), 0);
        else
            PlayerPrefs.SetInt(Constant.Objective.DailyObjectiveProgressPrefix + type.ToString(), 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 重置每日目标进度
    /// </summary>
    public void ResetDailyObjectiveProgress()
    {
        foreach (string item in Enum.GetNames(typeof(ObjectiveType)))
        {
            PlayerPrefs.SetInt(Constant.Objective.DailyObjectiveProgressPrefix + item, 0);
        }
        PlayerPrefs.SetString(Constant.Objective.ClaimedDailyObjectiveIds, string.Empty);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 当成就目标领取时
    /// </summary>
    /// <param name="id">成就目标编号</param>
    public void OnObjectiveClaim(ObjectiveData data, bool isAllTime)
    {
        int id = data.ID;
        if (isAllTime)
        {
            if (m_CurAllTimeObjectiveIds.Contains(id))
            {
                m_CurAllTimeObjectiveIds.Remove(id);
                m_ClaimedAllTimeObjectiveIds.Add(id);
                SaveClaimedObjectiveIds(true);

                if (data.Type == ObjectiveType.Pass_Levels
                    || data.Type == ObjectiveType.Clear_Tiles
                    || data.Type == ObjectiveType.Use_Crane
                    || data.Type == ObjectiveType.Use_ExtraSlot
                    || data.Type == ObjectiveType.Use_Magnet
                    || data.Type == ObjectiveType.Use_Shuffle
                    || data.Type == ObjectiveType.Use_Undo
                    || data.Type == ObjectiveType.Turntable)
                    ClearObjectiveProgress(data.Type, true);

                RefreshAllTimeObjective(true);
            }
        }
        else
        {
            if (m_CurDailyObjectiveIds.Contains(id))
            {
                m_CurDailyObjectiveIds.Remove(id);
                m_ClaimedDailyObjectiveIds.Add(id);
                SaveClaimedObjectiveIds(false);
            }
        }

        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.Objective));
    }

    private void SaveClaimedObjectiveIds(bool isAllTime)
    {
        if (isAllTime)
        {
            if (m_ClaimedAllTimeObjectiveIds.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_ClaimedAllTimeObjectiveIds.Count; i++)
            {
                sb.Append(m_ClaimedAllTimeObjectiveIds[i].ToString());

                if (i != m_ClaimedAllTimeObjectiveIds.Count - 1)
                    sb.Append("_");
            }

            PlayerPrefs.SetString(Constant.Objective.ClaimedAllTimeObjectiveIds, sb.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            if (m_ClaimedDailyObjectiveIds.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_ClaimedDailyObjectiveIds.Count; i++)
            {
                sb.Append(m_ClaimedDailyObjectiveIds[i].ToString());

                if (i != m_ClaimedDailyObjectiveIds.Count - 1)
                    sb.Append("_");
            }

            PlayerPrefs.SetString(Constant.Objective.ClaimedDailyObjectiveIds, sb.ToString());
            PlayerPrefs.Save();
        }
    }

    private void LoadClaimedObjectiveIds(bool isAllTime)
    {
        if (isAllTime)
        {
            m_ClaimedAllTimeObjectiveIds.Clear();
            string claimedObjectiveIdsString = PlayerPrefs.GetString(Constant.Objective.ClaimedAllTimeObjectiveIds, string.Empty);
            if (!string.IsNullOrEmpty(claimedObjectiveIdsString))
            {
                string[] splits = claimedObjectiveIdsString.Split('_');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int result) && !m_ClaimedAllTimeObjectiveIds.Contains(result))
                    {
                        m_ClaimedAllTimeObjectiveIds.Add(result);
                    }
                }
            }
        }
        else
        {
            m_ClaimedDailyObjectiveIds.Clear();
            string claimedObjectiveIdsString = PlayerPrefs.GetString(Constant.Objective.ClaimedDailyObjectiveIds, string.Empty);
            if (!string.IsNullOrEmpty(claimedObjectiveIdsString))
            {
                string[] splits = claimedObjectiveIdsString.Split('_');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int result) && !m_ClaimedDailyObjectiveIds.Contains(result))
                    {
                        m_ClaimedDailyObjectiveIds.Add(result);
                    }
                }
            }
        }
    }

    private void OnReadAllTimeDataSuccess(GameEventMessage e)
    {
        string dataName = e.Items[0].ToString();
        GameFramework.ReferencePool.Release(e);

        m_AllTimeObjectiveData = GameManager.DataTable.GetDataTable<DTAllTimeObjective>().Data;

        GameManager.Objective.RefreshAllTimeObjective();

        Log.Info("Read {0} success", dataName);
    }

    private void OnReadDailyDataSuccess(GameEventMessage e)
    {
        string dataName = e.Items[0].ToString();
        GameFramework.ReferencePool.Release(e);

        m_DTDailyObjectiveData = GameManager.DataTable.GetDataTable<DTDailyObjective>().Data;

        GameManager.Objective.RefreshDailyObjective();

        Log.Info("Read {0} success", dataName);
    }

    private void OnReadDataFailure(GameEventMessage e)
    {
        string dataName = e.Items[0].ToString();
        GameFramework.ReferencePool.Release(e);

        Log.Info("Read {0} fail", dataName);
    }

    #region Service

    /// <summary>
    /// 需要保存到服务器的数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> GetNeedSaveToServiceDict()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add(Constant.Objective.ClaimedAllTimeObjectiveIds, PlayerPrefs.GetString(Constant.Objective.ClaimedAllTimeObjectiveIds));
        result.Add(Constant.Objective.ClaimedDailyObjectiveIds, PlayerPrefs.GetString(Constant.Objective.ClaimedDailyObjectiveIds));

        foreach (string item in Enum.GetNames(typeof(ObjectiveType)))
        {
            result.Add(Constant.Objective.DailyObjectiveProgressPrefix + item, PlayerPrefs.GetInt(Constant.Objective.DailyObjectiveProgressPrefix + item, 0));
            result.Add(Constant.Objective.AllTimeObjectiveProgressPrefix + item, PlayerPrefs.GetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + item, 0));
        }

        return result;
    }

    public Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add(AllModelVersionPrefix + Constant.Objective.ClaimedAllTimeObjectiveIds, PlayerPrefs.GetString(Constant.Objective.ClaimedAllTimeObjectiveIds));
        result.Add(AllModelVersionPrefix + Constant.Objective.ClaimedDailyObjectiveIds, PlayerPrefs.GetString(Constant.Objective.ClaimedDailyObjectiveIds));

        foreach (string item in Enum.GetNames(typeof(ObjectiveType)))
        {
            result.Add(AllModelVersionPrefix + Constant.Objective.DailyObjectiveProgressPrefix + item, PlayerPrefs.GetInt(Constant.Objective.DailyObjectiveProgressPrefix + item, 0));
            result.Add(AllModelVersionPrefix + Constant.Objective.AllTimeObjectiveProgressPrefix + item, PlayerPrefs.GetInt(Constant.Objective.AllTimeObjectiveProgressPrefix + item, 0));
        }

        return result;
    }

    /// <summary>
    /// 服务器数据转化为本地数据 并覆盖
    /// </summary>
    /// <param name="dictionary"></param>
    public void SaveServiceDataToLocal(Dictionary<string, object> dictionary)
    {
        if (dictionary != null)
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case Constant.Objective.ClaimedAllTimeObjectiveIds:
                        if (item.Value != null)
                            PlayerPrefs.SetString(Constant.Objective.ClaimedAllTimeObjectiveIds, Convert.ToString(item.Value));
                        break;
                    case Constant.Objective.ClaimedDailyObjectiveIds:
                        if (item.Value != null)
                            PlayerPrefs.SetString(Constant.Objective.ClaimedDailyObjectiveIds, Convert.ToString(item.Value));
                        break;
                }

                foreach (string typeName in Enum.GetNames(typeof(ObjectiveType)))
                {
                    if (item.Key == Constant.Objective.DailyObjectiveProgressPrefix + typeName || item.Key == Constant.Objective.AllTimeObjectiveProgressPrefix + typeName)
                    {
                        if (item.Value != null)
                            PlayerPrefs.SetInt(item.Key, Convert.ToInt32(item.Value));
                    }
                }
            }
            PlayerPrefs.Save();
            RefreshDailyObjective();
            RefreshAllTimeObjective();
        }
    }

    public void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
    {
        if (dictionary != null)
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case AllModelVersionPrefix + Constant.Objective.ClaimedAllTimeObjectiveIds:
                        if (item.Value != null)
                            PlayerPrefs.SetString(Constant.Objective.ClaimedAllTimeObjectiveIds, Convert.ToString(item.Value));
                        break;
                    case AllModelVersionPrefix + Constant.Objective.ClaimedDailyObjectiveIds:
                        if (item.Value != null)
                            PlayerPrefs.SetString(Constant.Objective.ClaimedDailyObjectiveIds, Convert.ToString(item.Value));
                        break;
                }

                foreach (string typeName in Enum.GetNames(typeof(ObjectiveType)))
                {
                    if (item.Key == AllModelVersionPrefix + Constant.Objective.DailyObjectiveProgressPrefix + typeName || item.Key == AllModelVersionPrefix + Constant.Objective.AllTimeObjectiveProgressPrefix + typeName)
                    {
                        if (item.Value != null)
                        {
                            string trueKey = item.Key.Replace(AllModelVersionPrefix, "");
                            PlayerPrefs.SetInt(trueKey, Convert.ToInt32(item.Value));
                        }
                    }
                }
            }
            PlayerPrefs.Save();
            RefreshDailyObjective();
            RefreshAllTimeObjective();
        }
    }

    public Task GetServiceData(string userId, Action<Dictionary<string, object>> dictAction)
    {
        return FirebaseFirestore.DefaultInstance.Document(DocumentPath(userId)).GetSnapshotAsync().ContinueWithOnMainThread(
            (task) =>
            {
                if (task.IsCompleted)
                {
                    Log.Info($"GetServiceData is Success:{userId}:{DocumentPath(userId)}");
                    dictAction?.Invoke(task.Result.ToDictionary());
                }
                else if (task.IsCanceled || task.IsFaulted)
                {
                    Log.Error($"GetServiceModel is Fail:{userId}:{DocumentPath(userId)}|||{task.Exception?.Message}");
                }
            });
    }
    /// <summary>
    /// 文档路径
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private string DocumentPath(string userId) => $"AllUsers/{userId}/Data/{DocumentName}";

    /// <summary>
    /// 文档名称
    /// </summary>
    private string DocumentName => "ObjectModel";

    /// <summary>
    /// 文档弱引用
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public DocumentReference GetBatchToServiceRefernce(string userId) =>
        FirebaseFirestore.DefaultInstance.Document(DocumentPath(userId));

    /// <summary>
    /// 存入一个文档时用的前缀
    /// </summary>
    private const string AllModelVersionPrefix = "ObjectModel.";
    #endregion
}
