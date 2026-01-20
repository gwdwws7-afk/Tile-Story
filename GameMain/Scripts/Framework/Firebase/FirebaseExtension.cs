using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

public static class FirebaseExtension
{
    #region Record
    /// <summary>
    /// 记录金币产出
    /// </summary>
    /// <param name="firebaseComponent">firebase组件</param>
    /// <param name="source">金币产出来源</param>
    /// <param name="num">金币产出数量</param>
    public static void RecordCoinGet(this FirebaseComponent firebaseComponent, string source, int num)
    {
        return;
        if (num <= 0)
        {
            return;
        }

        int temp = num / 10;
        if (num % 10 > 0)
            num = temp * 10 + 5;

        firebaseComponent.RecordMessageByEvent(
            Constant.AnalyticsEvent.Coin_Get,
            new Parameter("Source", source),
            new Parameter("Num", num));
    }

    /// <summary>
    /// 记录金币消耗
    /// </summary>
    /// <param name="firebaseComponent">firebase组件</param>
    /// <param name="source">金币消耗来源</param>
    /// <param name="num">金币消耗数量</param>
    public static void RecordCoinSpend(this FirebaseComponent firebaseComponent, string source, int num)
    {
        return;
        if (num <= 0)
        {
            return;
        }

        int temp = num / 10;
        if (num % 10 > 0)
            num = temp * 10 + 5;

        firebaseComponent.RecordMessageByEvent(
            Constant.AnalyticsEvent.Coin_Spend,
            new Parameter("Source", source),
            new Parameter("Num", num));
    }

    /// <summary>
    /// 记录道具产出
    /// </summary>
    /// <param name="firebaseComponent">firebase组件</param>
    /// <param name="source">道具产出来源</param>
    /// <param name="itemType">道具种类</param>
    /// <param name="num">道具产出数量</param>
    public static void RecordLevelToolsGet(this FirebaseComponent firebaseComponent, string source, TotalItemData itemType, int num)
    {
        return;
        if (num <= 0)
        {
            return;
        }

        firebaseComponent.RecordMessageByEvent(
            Constant.AnalyticsEvent.Level_Tools_Get, 
            new Parameter("ID", (itemType.ID)),
            new Parameter("Source", source),
            new Parameter( "Num", num));
    }

    /// <summary>
    /// 因金币不足跳转商店(入口位置：1 - 接关 2 - 5种道具购买 3 - 背景购买 4 - 棋子购买 5 - 大转盘 6 - 每日挑战进关 7 - 每日挑战接关)
    /// </summary>
    public static void RecordCoinNotEnough(this FirebaseComponent firebaseComponent, int wayIndex, int level)
    {
        if (PlayerPrefs.GetInt("IsFirstCoinNotEnough", 0) == 0)
        {
            PlayerPrefs.SetInt("IsFirstCoinNotEnough", 1);
            firebaseComponent.RecordMessageByEvent(Constant.AnalyticsEvent.Coin_Not_Enough_FirstTime, new Parameter("Level", level));
        }

        firebaseComponent.RecordMessageByEvent(Constant.AnalyticsEvent.Coin_Not_Enough, new Parameter("WayIndex", wayIndex), new Parameter("Level", level));
    }
    
    public static void Should_show_Int(this FirebaseComponent firebaseComponent)
    {
        firebaseComponent.RecordMessageByEvent(Constant.AnalyticsEvent.should_show_Int,
            new Parameter("Level",GameManager.PlayerData.NowLevel));
    }

    #endregion
}
