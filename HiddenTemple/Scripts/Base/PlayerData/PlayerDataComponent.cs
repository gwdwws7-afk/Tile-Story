using System;
using UnityEngine;

namespace HiddenTemple
{
    /// <summary>
    /// 玩家数据模块
    /// </summary>
    public class PlayerDataComponent : MonoBehaviour
    {
        private const string ActivityPrefix = "HiddenTemple_";

        /// <summary>
        /// 获取活动预告开启关卡
        /// </summary>
        public int GetActivityPreviewLevel()
        {
            return 50;
        }

        /// <summary>
        /// 获取活动解锁关卡
        /// </summary>
        public int GetActivityUnlockLevel()
        {
            return 55;
        }

        /// <summary>
        /// 获取最大寻宝阶段
        /// </summary>
        public int GetMaxStage()
        {
            return 5;
        }

        /// <summary>
        /// 能自动弹出弹窗的稿子数量
        /// </summary>
        public int CanAutoShowMenuPickaxeNum()
        {
            return 20;
        }

        /// <summary>
        /// 获取当前活动周期
        /// </summary>
        /// <returns>为0表示不在活动周期内</returns>
        public int GetCurActivityPeriod()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "CurActivityPeriod", 0);
        }

        /// <summary>
        /// 设置当前活动周期
        /// </summary>
        /// <param name="period">期数，不在活动周期为0</param>
        public void SetCurActivityPeriod(int period)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "CurActivityPeriod", period);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取当前寻宝阶段
        /// </summary>
        public int GetCurrentStage()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "Stage", 1);
        }

        /// <summary>
        /// 设置当前寻宝阶段
        /// </summary>
        public void SetCurrentStage(int stage)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "Stage", stage);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取宝箱是否被领取
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        public bool GetChestIsClaimed(int stage)
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "ChestIsClaimed" + stage.ToString(), 0) != 0;
        }

        /// <summary>
        /// 设置宝箱是否被领取
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        /// <param name="isClaimed">是否被领取</param>
        public void SetChestIsClaimed(int stage, bool isClaimed)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "ChestIsClaimed" + stage.ToString(), isClaimed ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取本地保存的关卡数据
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        public string GetSavedStageLevelData(int stage)
        {
            return PlayerPrefs.GetString(ActivityPrefix + "SavedStageLevelData" + stage.ToString(), null);
        }

        /// <summary>
        /// 保存关卡数据至本地
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        /// <param name="levelData">关卡数据</param>
        public void SetSavedStageLevelData(int stage, string levelData)
        {
            PlayerPrefs.SetString(ActivityPrefix + "SavedStageLevelData" + stage.ToString(), levelData);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取本地保存的双重土块数据
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        public string GetSavedDoubleBlockData(int stage)
        {
            return PlayerPrefs.GetString(ActivityPrefix + "SavedDoubleBlockData" + stage.ToString(), null);
        }

        /// <summary>
        /// 保存双重土块数据至本地
        /// </summary>
        /// <param name="stage">寻宝阶段</param>
        /// <param name="data">双重土块数据</param>
        public void SetSavedDoubleBlockData(int stage, string data)
        {
            PlayerPrefs.SetString(ActivityPrefix + "SavedDoubleBlockData" + stage.ToString(), data);
        }

        /// <summary>
        /// 获取稿子数
        /// </summary>
        public int GetPickaxeNum()
        {
            return GameManager.PlayerData.GetCurItemNum(TotalItemData.Pickaxe);
        }

        /// <summary>
        /// 增加稿子数
        /// </summary>
        public void AddPickaxeNum(int addNum)
        {
            if (addNum <= 0)
                return;
            GameManager.PlayerData.AddItemNum(TotalItemData.Pickaxe, addNum);
        }

        /// <summary>
        /// 减少稿子数
        /// </summary>
        public void SubtractPickaxeNum(int subNum)
        {
            if (subNum <= 0)
                return;
            GameManager.PlayerData.UseItem(TotalItemData.Pickaxe, subNum, false);
        }

        /// <summary>
        /// 获取稿子关卡收集数
        /// </summary>
        public int GetPickaxeLevelCollectNum()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "PickaxeLevelCollectNum", 0);
        }

        /// <summary>
        /// 增加稿子关卡收集数
        /// </summary>
        public void AddPickaxeLevelCollectNum(int addNum)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "PickaxeLevelCollectNum", GetPickaxeLevelCollectNum() + addNum);
        }

        /// <summary>
        /// 清空稿子关卡收集数
        /// </summary>
        public void ClearPickaxeLevelCollectNum()
        {
            PlayerPrefs.SetInt(ActivityPrefix + "PickaxeLevelCollectNum", 0);
        }

        /// <summary>
        /// 获取稿子获取的连胜等级
        /// </summary>
        public int GetPickaxeWinStreakStage()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "PickaxeWinStreakStage", 1);
        }

        /// <summary>
        /// 增加稿子获取的连胜等级
        /// </summary>
        public void SetPickaxeWinStreakStage(int stage)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "PickaxeWinStreakStage", stage);
        }

        /// <summary>
        /// 清空稿子获取的连胜等级
        /// </summary>
        public void ClearPickaxeWinStreakStage()
        {
            PlayerPrefs.SetInt(ActivityPrefix + "PickaxeWinStreakStage", 1);
        }

        /// <summary>
        /// 获取看广告拿稿子的下个时间
        /// </summary>
        public DateTime GetAdsPickaxeNextReadyTime()
        {
            if (DateTime.TryParse(PlayerPrefs.GetString(ActivityPrefix + "AdsPickaxeNextReadyTime", string.Empty), out DateTime time))
                return time;
            else
                return Constant.GameConfig.DateTimeMin;
        }

        /// <summary>
        /// 设置看广告拿稿子的下个时间
        /// </summary>
        public void SetAdsPickaxeNextReadyTime(DateTime time)
        {
            PlayerPrefs.SetString(ActivityPrefix + "AdsPickaxeNextReadyTime", time.ToString());
        }

        /// <summary>
        /// 获取开启活动的次数
        /// </summary>
        public int GetOpenActivityTime()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "OpenActivityTime", 0);
        }

        /// <summary>
        /// 增加开启活动的次数
        /// </summary>
        public void AddOpenActivityTime()
        {
            PlayerPrefs.SetInt(ActivityPrefix + "OpenActivityTime", GetOpenActivityTime() + 1);
        }

        /// <summary>
        /// 获取是否已经自动弹出过礼包界面
        /// </summary>
        public bool GetHasAutoShowedGiftPackMenu()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "IsAutoShowGiftPackMenu", 0) != 0;
        }

        /// <summary>
        /// 设置是否已经自动弹出过礼包界面
        /// </summary>
        public void SetHasAutoShowedGiftPackMenu(bool isShowed)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "IsAutoShowGiftPackMenu", isShowed ? 1 : 0);
        }

        /// <summary>
        /// 获取今天是否弹出过最后机会界面
        /// </summary>
        public bool GetTodayShowedLastChanceMenu()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "IsTodayShowedLastChanceMenu", 0) != 0;
        }

        /// <summary>
        /// 设置今天是否弹出过最后机会界面
        /// </summary>
        public void SetTodayShowedLastChanceMenu(bool isShowed)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "IsTodayShowedLastChanceMenu", isShowed ? 1 : 0);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAllData()
        {
            SetCurActivityPeriod(0);
            SetCurrentStage(1);
            for (int i = 1; i <= 5; i++)
            {
                SetChestIsClaimed(i, false);
                SetSavedStageLevelData(i, null);
                SetSavedDoubleBlockData(i, null);
            }
            SubtractPickaxeNum(GetPickaxeNum());
            ClearPickaxeLevelCollectNum();
            ClearPickaxeWinStreakStage();
            SetAdsPickaxeNextReadyTime(Constant.GameConfig.DateTimeMin);
            SetHasAutoShowedGiftPackMenu(false);
        }
    }
}
