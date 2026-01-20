using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HarvestKitchenGamePanelData
{
    public int TileCellNum = 7;
    public int CurrentFailNumber = 0;
    public int CurrentTileNumber = 0;
    
    public int TargetCustomer = 0;
    public int TileNumber = 0;
    public int MinTileNumber = 0;
    public int ReduceTileFailedNumber = 0;
    public int FoodTypeNum = 0;
    public float emotionReduce = 0;
    public float coinProbability = 0;
    
    public List<int> CurrentTileTypeList;
    // 棋子出现的概率
    public List<int> tileTypeProbabilityList;
    public List<int> FoodNumberList;
    // 每波同时出现的顾客数量
    public List<int> WaveCustomerNumList;
    public List<int> FoodNumberProbabilityList;
    
    public List<KitchenCustomerData> customerDatas = null;


    public HarvestKitchenGamePanelData()
    {
        CurrentFailNumber = HarvestKitchenManager.Instance.FailedNum;

        DTHarvestKitchenLevelDatas currentLevelData = HarvestKitchenManager.Instance.GetCurrentLevelData();
        
        TargetCustomer = currentLevelData.TargetCustomer;
        TileNumber = currentLevelData.NormalTileNumber;
        MinTileNumber = currentLevelData.MinTileNumber;
        ReduceTileFailedNumber = currentLevelData.FailedNumber;
        FoodTypeNum = currentLevelData.FoodTypeNumber;
        emotionReduce = currentLevelData.EmotionReduce;
        coinProbability = currentLevelData.CoinProbability;
        FoodNumberList = new List<int>(currentLevelData.FoodNumberList);
        FoodNumberProbabilityList = new List<int>(currentLevelData.FoodProbabilityList);
        WaveCustomerNumList = new List<int>(currentLevelData.WaveCustomerNumList);

        // 初始化关卡的花色数量
        CurrentTileNumber = Mathf.Max(TileNumber - (CurrentFailNumber / ReduceTileFailedNumber), MinTileNumber);
        
        // 初始化当前关卡的花色
        // 花色列表
        List<int> tileTypeList = new List<int>(currentLevelData.FoodNeedNumberList);
        List<int> remainingTileTypeList = new List<int>() { 27, 36, 41, 39, 31, 26, 17, 10, 4 };
        
        CurrentTileTypeList = new List<int>();
        tileTypeProbabilityList = new List<int>();
        List<int> customerFoodList = new List<int>();
        for (int i = 0; i < CurrentTileNumber; i++)
        {
            if (tileTypeList.Count > 0)
            {
                int index = Random.Range(0, tileTypeList.Count);
                CurrentTileTypeList.Add(tileTypeList[index]);
                customerFoodList.Add(tileTypeList[index]);
                tileTypeProbabilityList.Add(1);
                tileTypeList.RemoveAt(index);   
            }
            else
            {
                while (remainingTileTypeList.Count > 0)
                {
                    int index = Random.Range(0, remainingTileTypeList.Count);
                    if (!CurrentTileTypeList.Contains(remainingTileTypeList[index]))
                    {
                        CurrentTileTypeList.Add(remainingTileTypeList[index]);
                        tileTypeProbabilityList.Add(1);
                        remainingTileTypeList.RemoveAt(index);

                        break;
                    }
                    else
                    {
                        remainingTileTypeList.RemoveAt(index);
                    }
                }
            }
        }
        
        // 计算食物数量出现概率的总和
        float probabilityTotal = FoodNumberProbabilityList.Sum();
        
        // 初始化顾客数据
        customerDatas = new List<KitchenCustomerData>();
        for (int i = 0; i < TargetCustomer; i++)
        {
            // 随机顾客需要的食物种类
            int foodType = Random.Range(0, FoodTypeNum) + 1;
            List<int> foodId = new List<int>();
            List<int> foodNum = new List<int>();
            for (int j = 0; j < foodType; j++)
            {
                // 随机需要的食物
                int index = Random.Range(0, customerFoodList.Count);
                while (foodId.Contains(customerFoodList[index % customerFoodList.Count]))
                {
                    index++;
                }
                foodId.Add(customerFoodList[index % customerFoodList.Count]);
                // 随机需要的食物数量
                float foodNumP = Random.Range(0, probabilityTotal);
                for (int p = 0; p < FoodNumberProbabilityList.Count; p++)
                {
                    if (foodNumP < FoodNumberProbabilityList[p])
                    {
                        foodNum.Add(FoodNumberList[p]);
                        break;
                    }
                    foodNumP -= FoodNumberProbabilityList[p];
                }
            }
            
            // 保存顾客数据
            customerDatas.Add(new KitchenCustomerData(foodId, foodNum, emotionReduce));
        }
    }

    public int GetTotalCustomerNum()
    {
        if (customerDatas == null) return 0;
        return customerDatas.Count;
    }

    public KitchenCustomerData GetCustomerDataByIndex(int index)
    {
        if (HasNewCustomer(index))
            return customerDatas[index];
        return null;
    }

    public bool HasNewCustomer(int index)
    {
        return customerDatas != null && customerDatas.Count > 0 && index < customerDatas.Count;
    }

    public int GetShowCustomerNum(int index)
    {
        return WaveCustomerNumList[Mathf.Min(index, WaveCustomerNumList.Count - 1)];
    }

    public void ChangeTileProbability(List<int> tiletype, bool isAdd)
    {
        for (int i = 0; i < tiletype.Count; i++)
        {
            for (int j = 0; j < CurrentTileTypeList.Count; j++)
            {
                if (CurrentTileTypeList[j] == tiletype[i])
                {
                    tileTypeProbabilityList[j] += isAdd ? 1 : -1;
                }
            }
        }
    }

    public void OnRelease()
    {
        CurrentTileTypeList = null;
        tileTypeProbabilityList = null;
        FoodNumberList = null;
        WaveCustomerNumList = null;
        FoodNumberProbabilityList = null;
        customerDatas = null;
    }
}
