using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Firebase.Extensions;
using Firebase.Firestore;
using MySelf.Model;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace MySelf.Model
{
	using System;
	using UnityEngine;
    using Newtonsoft.Json;

    public interface ILocalModel<T>
    {
        T GetLocalModel();
        void SaveToLocal();
    }

    interface IServiceModel
    {
        Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion();
        void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary);
    }

    public class BaseModel<T0, T>: ILocalModel<T> where T0 : BaseModel<T0, T>,new() where T :new()
    {
        private static T0 instance = null;
        public static T0 Instance
        {
            get
            {
                if (instance==null)
                {
                    instance =new T0();
                    instance.Data = instance.GetLocalModel();
                }
                return instance;
            }
        }

        protected internal T Data;

        public T EditorData => Data;
        
        protected virtual string PrefName=> $"{ModelUtil.LastPrefName}.{typeof(T).Name}";

		#region 
		public virtual T GetLocalModel()
        {
            var content = GetString(PrefName);
            return Deserialize(content);
        }

        public virtual void SaveToLocal()
        {
#if UNITY_EDITOR
           Log.Info($"{PrefName}::Serialize::SaveToLocal");
#endif
            //GameManager.Task.AddExecutionByFrame(PrefName, ()=>SetString(PrefName, Serialize()));
            SetString(PrefName, Serialize(),false);
        }
        #endregion

        #region 
        protected virtual string Serialize()
        {
            return JsonConvert.SerializeObject(Data, Formatting.None);
        }

        protected virtual T Deserialize(string content)
        {
            var data = JsonConvert.DeserializeObject<T>(content);
            return data==null? new T() : data;
        }
        #endregion

        #region 
        protected virtual void ClearData()
        {
            DeleteKey(PrefName);
        }
        #endregion

        #region
        protected bool HasKey(string name)
        {
            return PlayerPrefs.HasKey(name);
        }

        protected string GetString(string name, string defaultValue = null)
        {
            return PlayerPrefs.GetString(name, defaultValue);
        }

        protected void SetString(string name, string value, bool saveNow)
        {
            PlayerPrefs.SetString(name, value);
            
            if(saveNow)
                GameManager.Task.AddSaveDataTask();
        }

        private bool GetBool(string name, bool defaultValue = false)
        {
            int value = PlayerPrefs.GetInt(name, 0);
            switch (value)
            {
                case 2:
                    return true;
                case 1:
                    return false;
                default:
                    return defaultValue;
            }
        }

        private void SetBool(string name, bool value)
        {
            PlayerPrefs.SetInt(name, value ? 2 : 1);
        }

        private int GetInt(string name, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(name, defaultValue);
        }

        private void SetInt(string name, int value)
        {
            PlayerPrefs.SetInt(name, value);
            PlayerPrefs.Save();
        }

        private float GetFloat(string name, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(name, defaultValue);
        }

        private void SetFloat(string name, float value)
        {
            PlayerPrefs.SetFloat(name, value);
        }

        private DateTime GetDateTime(string name)
        {
            string saveValue = GetString(name, string.Empty);
            if (DateTime.TryParse(saveValue, out DateTime result)) return result;
            return DateTime.MinValue;
        }

        private void SetDateTime(string name, DateTime value)
        {
            SetString(name, value.ToString(Constant.GameConfig.DefaultDateTimeFormet),true);
        }

        protected void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
        #endregion
    }

    public abstract class BaseModelService<T0, T> : BaseModel<T0, T>,IServiceModel where T0 : BaseModel<T0, T>, new() where T : new()
    {
        #region Service
        /// <summary>
        /// 需要保存到服务器的数据
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion();

        /// <summary>
        /// 服务器数据转化为本地数据 并覆盖
        /// </summary>
        /// <param name="dictionary"></param>
        public abstract void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary);
        /// <summary>
        /// 文档路径
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string DocumentPath(string userId) => $"AllUsers/{userId}/Data/{DocumentName}";
        
        /// <summary>
        /// 文档名称
        /// </summary>
        private string DocumentName=> typeof(T0).Name;
        
        /// <summary>
        /// 文档弱引用
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DocumentReference GetBatchToServiceRefernce(string userId) =>
            FirebaseFirestore.DefaultInstance.Document(DocumentPath(userId));
        
        // //获取服务器数据
        // public virtual void GetServiceModel(string userId,Action<object> successAction,Action failAction)
        // {
        //     FirebaseFirestore.DefaultInstance.Document(DocumentPath(userId)).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        //     {
        //         if (task.IsCompleted)
        //         {
        //             Log.Info($"GetServiceModel is Success:{userId}:{DocumentPath(userId)}");
        //             successAction?.Invoke(task.Result);
        //         }else if (task.IsCanceled || task.IsFaulted)
        //         {
        //             Log.Error($"GetServiceModel is Fail:{userId}:{DocumentPath(userId)}|||{task.Exception?.Message}");
        //             failAction?.Invoke();
        //         }
        //     });
        // }
        // //保存数据到服务器
        // public virtual void SaveToService(string userId,Action<bool> upLoadSuccess)
        // {
        //     FirebaseFirestore.DefaultInstance.Document(DocumentPath(userId)).SetAsync(GetNeedSaveToServiceDict()).ContinueWithOnMainThread(task =>
        //     {
        //         if (task.IsCompleted)
        //         {
        //             Log.Info($"GetServiceModel is Success:{userId}:{DocumentPath(userId)}|||{task.Exception?.Message}");
        //             upLoadSuccess?.Invoke(true);
        //         }else if (task.IsCanceled || task.IsFaulted)
        //         {
        //             Log.Error($"GetServiceModel is Fail:{userId}:{DocumentPath(userId)}|||{task.Exception?.Message}");
        //             upLoadSuccess?.Invoke(false);
        //         }
        //     });
        // }
        #endregion
    }
    
    public static class FirebaseServiceUtil
    {
        public static string AllModelDocumentPath(string userId) => $"AllUsers/{userId}/Data/AllModel";
        private static List<IServiceModel> NeedSyncModelList => new List<IServiceModel>()
        {
            ItemModel.Instance,
            LevelModel.Instance,
            DecorationModel.Instance,
            BGModel.Instance,
            PlayerBehaviorModel.Instance,
            CommonModel.Instance,
            GameManager.Objective,
            CalendarChallengeModel.Instance,
            TilePassModel.Instance,
            CardModel.Instance
        };

        public static void DeleteServiceDataInOneDoc(Action<bool> finishAction)
        {
            if (!GameManager.Firebase.IsInitFirebaseApp)
            {
                return;
            }
            var userId = PlayerLoginModel.Instance.Data.UserID;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();

            DocumentReference allModelDocumentRef = FirebaseFirestore.DefaultInstance.Document(AllModelDocumentPath(userId));
            batch.Delete(allModelDocumentRef);

            Log.Info($"DeleteServiceData Start!:{userId}");
            batch.CommitAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Log.Info($"DeleteServiceData is Success:{userId}");
                    finishAction?.InvokeSafely(true);
                }
                else if (task.IsCanceled || task.IsFaulted)
                {
                    Log.Error($"DeleteServiceData is Fail:{userId}");
                    finishAction?.InvokeSafely(false);
                }
            });
        }

        public static void SaveToServiceInOneDoc(Action<bool> finishAction)
        {
            if (!GameManager.Firebase.IsInitFirebaseApp)
            {
                finishAction?.InvokeSafely(false);
                return;
            }

            var userId = PlayerLoginModel.Instance.Data.UserID;
            if (string.IsNullOrEmpty(userId))
            {
                finishAction?.InvokeSafely(false);
                return;
            }


            DocumentReference allModelDocumentRef = FirebaseFirestore.DefaultInstance.Document(AllModelDocumentPath(userId));
            WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();
            Dictionary<string, object> allModelDic = new Dictionary<string, object>();
            foreach (var model in NeedSyncModelList)
            {
                Dictionary<string,object> singleModelDic = model.GetNeedSaveToServiceDictAllModelVersion();
                foreach(KeyValuePair<string,object> kv in singleModelDic)
                {
                    if (!allModelDic.ContainsKey(kv.Key))
                    {
                        allModelDic.Add(kv.Key, kv.Value);
                    }
                    else
                    {
                        Debug.LogError("DuplicateKey kv.key = " + kv.Key);
                    }
                }
            }
            //特别的 再存一个当前时间戳
            allModelDic.Add("SaveData.TimeStamp", GetCurrentUnixTimeStampInSeconds());
            batch.Set(allModelDocumentRef, allModelDic);


            Log.Info($"SaveToService Start!:{userId}");
            batch.CommitAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Log.Info($"SaveToService is Success:{userId}");
                    finishAction?.InvokeSafely(true);
                    GameManager.PlayerData.RecordSaveDataTime();
                }
                else if (task.IsCanceled || task.IsFaulted)
                {
                    Log.Error($"SaveToService is Fail:{userId}");
                    finishAction?.InvokeSafely(false);
                }
            });
        }

        public static void GetDataFromServiceInOneDoc(Action<Dictionary<string, string>, Dictionary<string, string>, Action, Action> successAction, Action failAction)
        {
            if (!GameManager.Firebase.IsInitFirebaseApp)
            {
                failAction?.InvokeSafely();
                return;
            }

            var userId = PlayerLoginModel.Instance.Data.UserID;
            if (string.IsNullOrEmpty(userId))
            {
                failAction?.InvokeSafely();
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            List<Action> syncDataList = new List<Action>();
            List<Task> list = new List<Task>();


            string allModelDocPath = AllModelDocumentPath(userId);
            Task getAllModelTask = FirebaseFirestore.DefaultInstance.Document(allModelDocPath).GetSnapshotAsync().ContinueWithOnMainThread(
                (task) =>
                {
                    if (task.IsCompleted)
                    {
                        Log.Info($"GetServiceData is Success:{userId}:{allModelDocPath}");

                        Dictionary<string, object> resultDic = task.Result.ToDictionary();
                        foreach (var item in resultDic) dictionary.TryAdd(item.Key, item.Value);
                        foreach (var model in NeedSyncModelList)
                        {
                            syncDataList.Add(() => model.SaveServiceDataToLocalAllModelVersion(resultDic));
                        }
                    }
                    else if (task.IsCanceled || task.IsFaulted)
                    {
                        Log.Error($"GetServiceModel is Fail:{userId}:{allModelDocPath}|||{task.Exception?.Message}");
                    }
                });
            list.Add(getAllModelTask);

            Log.Info($"GetDataFromService Start!:{userId}");
            Task.WhenAll(list).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Log.Info($"GetDataFromService is Success:{userId}");
                    Dictionary<string, string> localData = new Dictionary<string, string>()
                    {
                        {"Level",LevelModel.Instance.Data.Level.ToString()},
                        {"Star",ItemModel.Instance.GetItemTotalNum(TotalItemType.Star).ToString()},
                        {"Coin",ItemModel.Instance.GetItemTotalNum(TotalItemType.Coin).ToString()},
                        {"LastUploadTime",DateTime.Now.ToString()},
                    };

                    string serverLastUploadTime = DateTime.Now.ToString();
                    try
                    {
                        if (dictionary.TryGetValue("SaveData.TimeStamp", out object content))
                        {
                            DateTime uploadDateTime = GetDateTimeFromUnixTimeStampInSeconds(Convert.ToInt32(content));
                            serverLastUploadTime = uploadDateTime.ToString();
                        }
                        else
                        {
                            //如果没有拉到数据不弹出存档界面
                            failAction?.InvokeSafely();
                            return;
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"SererUploadTime convert failed {e.Message}");
                    }
                    
                    //获取服务器数据成功
                    Dictionary<string, string> serviceData = new Dictionary<string, string>()
                    {
                        {"Level",GetDataByDictinary(dictionary,"LevelModel.Level",localData["Level"])},
                        {"Star",GetDataByDictinary(dictionary,"ItemModel.Star",localData["Star"])},
                        {"Coin",GetDataByDictinary(dictionary,"ItemModel.Coin",localData["Coin"])},
                        //{"LastUploadTime",DateTime.Now.ToString()},
                        {"LastUploadTime", serverLastUploadTime},
                    };
                    successAction?.Invoke(serviceData, localData, () =>
                    {
                        foreach (var action in syncDataList)
                        {
                            action?.InvokeSafely();
                        }
                    }, () =>
                    {
                        //上传数据
                        //SaveToService(null);
                        SaveToServiceInOneDoc(null);
                    });
                }
                else if (task.IsCanceled || task.IsFaulted)
                {
                    failAction?.InvokeSafely();
                    Log.Error($"GetDataFromService is Fail:{userId}:{task.Exception?.Message}");
                }
            });
        }

        public static string GetDataByDictinary(this Dictionary<string,object> dictionary,string keyString,string defaultString)
        {
            if (dictionary.TryGetValue(keyString,out object content))
            {
                return content.ToString();
            }
            return defaultString;
        }

        public static int GetCurrentUnixTimeStampInSeconds()
        {
            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        public static DateTime GetDateTimeFromUnixTimeStampInSeconds(int unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}

public static class ModelUtil
{
    private static string lastPrefName = null;
    public static string LastPrefName
    {
        get
        {
            if (lastPrefName == null)
            {
                string str = PlayerPrefs.GetString("BaseModel.PlayerPrefs.LastPrefName",null);
                lastPrefName = str;
                if (string.IsNullOrEmpty(str))
                {
                    //获取等级，
                    int level = 0;
                    
                    Log.Info($"LastPrefName111:{lastPrefName}");

                    var contentPlayer = PlayerPrefs.GetString("Player.LevelModelData", null);
                    if (!string.IsNullOrEmpty(contentPlayer))
                    {
                        //说明有数据
                        var model = JsonConvert.DeserializeObject<LevelModelData>(contentPlayer);
                        if (model != null&&model.Level>level)
                        {
                            lastPrefName = "Player";
                            level = model.Level;
                        }
                    }
                    Log.Info($"LastPrefName222:{lastPrefName}");
                    
                    var contentTs = PlayerPrefs.GetString("TS.LevelModelData", null);
                    if (!string.IsNullOrEmpty(contentTs))
                    {
                        //说明有数据
                        var model = JsonConvert.DeserializeObject<LevelModelData>(contentTs);
                        if (model != null&&model.Level>level)
                        {
                            lastPrefName = "TS";
                        }
                    }
      
                    Log.Info($"LastPrefName333:{lastPrefName}");
                    
                    if (string.IsNullOrEmpty(lastPrefName)) lastPrefName = "TS";
                    
                    Log.Info($"LastPrefName444:{lastPrefName}");
                    PlayerPrefs.SetString("BaseModel.PlayerPrefs.LastPrefName",lastPrefName);
                    PlayerPrefs.Save();
                }
            }
            return lastPrefName;
        }
    }
}




