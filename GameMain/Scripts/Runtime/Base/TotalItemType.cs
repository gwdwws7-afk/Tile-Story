using UnityEngine.UI.Extensions;

/// <summary>
/// 道具种类
/// </summary>
public enum TotalItemType
{
    None,
    /// <summary>
    /// 金币
    /// </summary>
    Coin=1,
    /// <summary>
    /// 回退道具 1
    /// </summary>
    Prop_Back=2,
    /// <summary>
    /// 交换道具 4
    /// </summary>
    Prop_ChangePos=3,
    /// <summary>
    /// 吸收道具 3
    /// </summary>
    Prop_Absorb=4,
    /// <summary>
    /// 加一步道具
    /// </summary>
    Prop_AddOneStep=5,
    /// <summary>
    /// 抓取道具 2
    /// </summary>
    Prop_Grab=6,
    /// <summary>
    /// Stars
    /// </summary>
    Star=7,
    /// <summary>
    /// (通过礼包等奖励发放的)背景道具
    /// </summary>
    Item_BgID = 8,
    /// <summary>
    /// (通过礼包等奖励发放的)棋子道具
    /// </summary>
    Item_TileID = 9,
    /// <summary>
    /// (通过礼包等奖励发放的)头像道具
    /// </summary>
    Item_PortraitID = 10,
    /// <summary>
    /// 通行证油桶
    /// </summary>
    Gasoline = 12,
    /// <summary>
    /// 去广告
    /// </summary>
    RemoveAds =16,
    /// <summary>
    /// 生命
    /// </summary>
    Life=17,
    /// <summary>
    /// 无限生命时间
    /// </summary>
    InfiniteLife=18,
    /// <summary>
    /// 放大镜boost
    /// </summary>
    MagnifierBoost=20,
    /// <summary>
    /// 烟花boost
    /// </summary>
    FireworkBoost=21,
    /// <summary>
    /// 无限放大镜boost时间
    /// </summary>
    InfiniteMagnifierBoost = 22,
    /// <summary>
    /// 无限加一格boost时间
    /// </summary>
    InfiniteAddOneStepBoost = 23,
    /// <summary>
    /// 无限烟花boost时间
    /// </summary>
    InfiniteFireworkBoost = 24,
    /// <summary>
    /// 遗迹寻宝稿子
    /// </summary>
    Pickaxe = 25,
    /// <summary>
    /// 合成体力
    /// </summary>
    MergeEnergyBox = 26,
    /// <summary>
    /// 厨房厨师帽
    /// </summary>
    ChefHat = 27,
    
    /// <summary>
    /// pk赛道具
    /// </summary>
    PkItem = 28,
    
    /// <summary>
    /// 感恩节厨房篮子
    /// </summary>
    Basket=29,
    
    /// <summary>
    /// 感恩节厨房钥匙
    /// </summary>
    KitchenKey=30,
    
    CardPack1 = 51,
    CardPack2,
    CardPack3,
    CardPack4,
    CardPack5
}
