using Firebase.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySelf.Model
{
	public class ItemModelData
	{
        public Dictionary<TotalItemType, int> ItemDict = new Dictionary<TotalItemType, int>()
        {
            {TotalItemType.Coin,1000},
            {TotalItemType.Star,2},
            {TotalItemType.Prop_AddOneStep ,3},
            {TotalItemType.MagnifierBoost ,3},
            {TotalItemType.FireworkBoost ,3},
        };
        public Dictionary<TotalItemType, int> AdditionalDict=new Dictionary<TotalItemType, int>();
    }
	public class ItemModel:BaseModelService<ItemModel,ItemModelData>
	{
        #region Service
        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var item in Data.ItemDict) dictionary.Add("ItemModel." + item.Key.ToString(), item.Value);
            return dictionary;
        }

        public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
        {
            if (dictionary != null)
            {
                var names = Enum.GetNames(typeof(TotalItemType));
                foreach (var item in dictionary)
                {
                    if (item.Key.StartsWith("ItemModel."))
                    {
                        string tureName = item.Key.Replace("ItemModel.", "");
                        if (names.Contains(tureName))
                        {
                            Data.ItemDict[Enum.Parse<TotalItemType>(tureName)] = Convert.ToInt32(item.Value);
                        }
                    }
                }
                SaveToLocal();
            }
        }
        #endregion

        public bool CheckNum(TotalItemType type) => GetItemTotalNum(type) > 0;

        public bool UseItem(TotalItemType type, int num)
        {
            int curNum = GetItemTotalNum(type);
            if (curNum >= num)
            {
                AddItem(type, -num);
                return true;
            }
            return false;
        }

        public int GetItemTotalNum(TotalItemType type)
        {
            int additionalNum = GetAdditionalItem(type);
            // if (additionalNum > 0)
            // {
            //     AddAdditionalItem(type, -additionalNum, false);
            //     AddItem(type, additionalNum);
            // }
            return GetItem(type)+additionalNum;
        }

        public int GetItem(TotalItemType type)
        {
            if (!Data.ItemDict.ContainsKey(type))return 0;

            return Data.ItemDict[type];
        }

        public void AddItem(TotalItemType type, int num,bool isSave=true)
        {
            if (num == 0) return;
            if (!Data.ItemDict.ContainsKey(type)) Data.ItemDict.Add(type,0);

            Data.ItemDict[type] += num;

            if (isSave)
            {
                if(type!=TotalItemType.Life)
                    SaveToLocal();
            }
        }

        public void AddAdditionalItem(TotalItemType type, int num,bool isSave=true)
        {
            if(!Data.AdditionalDict.ContainsKey(type))Data.AdditionalDict.Add(type,0);
            
            Data.AdditionalDict[type] += num;
            if(isSave) SaveToLocal();
        }

        private int GetAdditionalItem(TotalItemType type)
        {
            if (Data.AdditionalDict.ContainsKey(type)) return Data.AdditionalDict[type];
            return 0;
        }

        public void SyncAllItemData()
        {
            bool isSave = false;

            if(Data.AdditionalDict!=null)
                for (int i = 0; i < Data.AdditionalDict.Count; i++)
                {
                    var item = Data.AdditionalDict.ElementAt(i);
                    if (item.Value > 0)
                    {
                        AddAdditionalItem(item.Key, -item.Value, false);
                        AddItem(item.Key, item.Value,false);
                        isSave = true;
                    }
                }
            if(isSave)SaveToLocal();
        }

        public void RecordItemByFirebase()
        {
            RecordItemStateByFirebase();
        }

        private void RecordItemStateByFirebase()
        {
            TotalItemType[] types = new[]
            {
                TotalItemType.Coin,
                TotalItemType.Prop_Absorb,
                TotalItemType.Prop_Back,
                TotalItemType.Prop_Grab,
                TotalItemType.Prop_ChangePos,
                TotalItemType.Prop_AddOneStep,
                TotalItemType.FireworkBoost,
            };
            Parameter[] parameters = new Parameter[types.Length + 1];
            parameters[0] = new Parameter("NowLevel", GameManager.PlayerData.NowLevel);
            for (int i = 1; i <= types.Length; i++)
            {
                parameters[i] = new Parameter(types[i - 1].ToString(), GetItemTotalNum(types[i - 1]));
            }
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Player_Item_Remain, parameters);
        }

        public override void SaveToLocal()
        {
#if UNITY_EDITOR
            Log.Info($"{PrefName}::Serialize::SaveToLocal");
#endif
            
            SetString(PrefName, Serialize(),true);
        }
    }
}

