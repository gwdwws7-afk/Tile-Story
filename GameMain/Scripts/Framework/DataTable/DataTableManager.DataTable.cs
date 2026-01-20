using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;

public sealed partial class DataTableManager: GameFrameworkModule, IDataTableManager
{
    /// <summary>
    /// 数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    private sealed class DataTable<T>:DataTableBase,IDataTable<T> where T:class
    {
        private T data;
        private bool isLoading;

        public DataTable(string name)
            :base(name)
        {
        }

        public override Type Type { get => typeof(T); }

        public T Data { get => data; }

        public bool IsLoading { get => isLoading; }

        public override void ReadData(string dataTableAssetName, Action<GameEventMessage> readDataSuccess, Action<GameEventMessage> readDataFailure)
        {
            isLoading = true;
            Addressables.LoadAssetAsync<TextAsset>(dataTableAssetName).Completed += (obj) =>
            {
                isLoading = false;
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    data = JsonConvert.DeserializeObject<T>(obj.Result.text);
                    readDataSuccess(GameEventMessage.Create(dataTableAssetName, data));

                    Addressables.Release(obj);
                }
                else
                {
                    Log.Error("load {0} dataTable fail", dataTableAssetName);
                    readDataFailure(GameEventMessage.Create(dataTableAssetName));
                }
            };
        }

        public override void Shutdown()
        {
            data = null;
        }
    }
}
