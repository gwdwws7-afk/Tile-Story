using GameFramework;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Merge
{
    /// <summary>
    /// 默认数据表辅助器
    /// </summary>
    public sealed class DefaultDataTableHelper : DataTableHelperBase
    {
        public override bool ReadData(DataTableBase dataTable, string dataTableAssetName, EventHandler<ReadDataSuccessEventArgs> ReadDataTableSuccess, EventHandler<ReadDataFailureEventArgs> ReadDataTableFailure, object userData)
        {
            Addressables.LoadAssetAsync<TextAsset>(dataTableAssetName).Completed += obj =>
            {
                if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    TextAsset dataTableTextAsset = obj.Result;

                    if (dataTable.ParseData(dataTableTextAsset.text))
                    {
                        if (ReadDataTableSuccess != null)
                        {
                            ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReadDataSuccessEventArgs.Create(dataTableAssetName, 0f, userData);
                            ReadDataTableSuccess(this, loadDataSuccessEventArgs);
                            ReferencePool.Release(loadDataSuccessEventArgs);
                        }
                    }
                    else
                    {
                        if (ReadDataTableFailure != null)
                        {
                            ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataTableAssetName, obj.OperationException?.ToString(), userData);
                            ReadDataTableFailure(this, loadDataFailureEventArgs);
                            ReferencePool.Release(loadDataFailureEventArgs);
                            return;
                        }
                    }

                    Addressables.Release(obj);
                }
                else
                {
                    Log.Error("Load Datatable Asset {0} Failed", dataTableAssetName);

                    if (ReadDataTableFailure != null)
                    {
                        ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataTableAssetName, obj.OperationException?.ToString(), userData);
                        ReadDataTableFailure(this, loadDataFailureEventArgs);
                        ReferencePool.Release(loadDataFailureEventArgs);
                        return;
                    }
                }
            };
            return true;
        }

        public override bool ParseData(DataTableBase dataTable, string dataTableString)
        {
            try
            {
                int position = 0;
                string dataRowString = null;
                while ((dataRowString = dataTableString.ReadLine(ref position)) != null)
                {
                    if (dataRowString[0] == '#')
                    {
                        continue;
                    }

                    if (!dataTable.AddDataRow(dataRowString))
                    {
                        Log.Error("Can not parse data row string '{0}'.", dataRowString);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Error("Can not parse data table string with exception '{0}'.", exception);
                return false;
            }
        }
    }
}