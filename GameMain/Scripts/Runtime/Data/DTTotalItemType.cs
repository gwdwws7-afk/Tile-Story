using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DTTotalItemType
{
    public List<TotalItemData> TotalItemData;

	private Dictionary<int, TotalItemData> totalItemDataDic;

	public Dictionary<int, TotalItemData> TileDataDict
	{
		get
		{
			if (totalItemDataDic == null) totalItemDataDic = TotalItemData.ToDictionary(a => a.ID, b => b);

			return totalItemDataDic;
		}
	}

    public bool ContainData(int id)
    {
        return TileDataDict.ContainsKey(id);
    }

	public TotalItemData GetData(int id)
	{
		return TileDataDict[id];
	}
}

[System.Serializable]
public class TotalItemData
{
    public int ID; // 编号
    public int RefID; // 特定TotalItemType(TotalItemType.8~TotalItemType.10)时 该物品所指向的确切(背景/棋子/头像)ID
    public int TotalItemTypeInt; // TotalItemType

    public TotalItemType TotalItemType
    {
        get
        {
            return (TotalItemType)TotalItemTypeInt;
        }
    }

    public TotalItemData()
    {
        ID = 0;
        RefID = 0;
        TotalItemTypeInt = 0;
    }

    public TotalItemData(int inputID, int inputRefID, int inputTotalItemTypeInt)
    {
        ID = inputID;
        RefID = inputRefID;
        TotalItemTypeInt = inputTotalItemTypeInt;
    }

    public static TotalItemData None { get => NoneItem; }
    private static TotalItemData NoneItem = new TotalItemData();

    public static TotalItemData Coin { get => CointItem; }
    private static TotalItemData CointItem = new TotalItemData(1, 0, 1);

    public static TotalItemData Prop_Back { get => Prop_BackItem; }
    private static TotalItemData Prop_BackItem = new TotalItemData(2, 0, 2);

    public static TotalItemData Prop_ChangePos { get => Prop_ChangePosItem; }
    private static TotalItemData Prop_ChangePosItem = new TotalItemData(3, 0, 3);

    public static TotalItemData Prop_Absorb { get => Prop_AbsorbItem; }
    private static TotalItemData Prop_AbsorbItem = new TotalItemData(4, 0, 4);

    public static TotalItemData Prop_AddOneStep { get => Prop_AddOneStepItem; }
    private static TotalItemData Prop_AddOneStepItem = new TotalItemData(5, 0, 5);

    public static TotalItemData Prop_Grab { get => Prop_GrabItem; }
    private static TotalItemData Prop_GrabItem = new TotalItemData(6, 0, 6);

    public static TotalItemData Star { get => StarItem; }
    private static TotalItemData StarItem = new TotalItemData(7, 0, 7);

    public static TotalItemData Gasoline { get => GasolineItem; }
    private static TotalItemData GasolineItem = new TotalItemData(12, 0, 12);

    public static TotalItemData RemoveAds { get => RemoveAdsItem; }
    private static TotalItemData RemoveAdsItem = new TotalItemData(16, 0, 16);

    public static TotalItemData Life { get => LifeItem; }
    private static TotalItemData LifeItem = new TotalItemData(17, 0, 17);

    public static TotalItemData InfiniteLifeTime { get => InfiniteLifeTimeItem; }
    private static TotalItemData InfiniteLifeTimeItem = new TotalItemData(18, 0, 18);

    public static TotalItemData MagnifierBoost { get => MagnifierBoostItem; }
    private static TotalItemData MagnifierBoostItem = new TotalItemData(20, 0, 20);

    public static TotalItemData FireworkBoost { get => FireworkBoostItem; }
    private static TotalItemData FireworkBoostItem = new TotalItemData(21, 0, 21);

    public static TotalItemData InfiniteMagnifierBoost { get => InfiniteMagnifierBoostItem; }
    private static TotalItemData InfiniteMagnifierBoostItem = new TotalItemData(22, 0, 22);

    public static TotalItemData InfiniteAddOneStepBoost { get => InfiniteAddOneStepBoostItem; }
    private static TotalItemData InfiniteAddOneStepBoostItem = new TotalItemData(23, 0, 23);

    public static TotalItemData InfiniteFireworkBoost { get => InfiniteFireworkBoostItem; }
    private static TotalItemData InfiniteFireworkBoostItem = new TotalItemData(24, 0, 24);

    public static TotalItemData Pickaxe { get => PickaxeItem; }
    private static TotalItemData PickaxeItem = new TotalItemData(25, 0, 25);

    public static TotalItemData MergeEnergyBox { get => MergeEnergyBoxItem; }
    private static TotalItemData MergeEnergyBoxItem = new TotalItemData(26, 0, 26);
    
    public static TotalItemData KitchenChefHat { get => KitchenChefHatItem; }
    private static TotalItemData KitchenChefHatItem = new TotalItemData(27, 0, 27);
    
    public static TotalItemData Pk { get => PkItem; }
    private static TotalItemData PkItem = new TotalItemData(28, 0, 28);
    
    public static TotalItemData KitchenBasket { get => KitchenBasketItem; }
    private static TotalItemData KitchenBasketItem = new TotalItemData(29, 0, 29); 
    
    public static TotalItemData KitchenKey { get => KitchenKeyItem; }
    private static TotalItemData KitchenKeyItem = new TotalItemData(30, 0, 30); 
    
    public static TotalItemData CardPack1 => CardPack1Item;
    private static TotalItemData CardPack1Item = new TotalItemData(51, 0, 51);
    public static TotalItemData CardPack2 => CardPack2Item;
    private static TotalItemData CardPack2Item = new TotalItemData(52, 0, 52);
    public static TotalItemData CardPack3 => CardPack3Item;
    private static TotalItemData CardPack3Item = new TotalItemData(53, 0, 53);
    public static TotalItemData CardPack4 => CardPack4Item;
    private static TotalItemData CardPack4Item = new TotalItemData(54, 0, 54);
    public static TotalItemData CardPack5 => CardPack5Item;
    private static TotalItemData CardPack5Item = new TotalItemData(55, 0, 55);

    public static TotalItemData FromInt(int inputInt)
    {
        if (inputInt == 0)
            return NoneItem;
        else if (inputInt == 1)
            return CointItem;
        else if (inputInt == 2)
            return Prop_BackItem;
        else if (inputInt == 3)
            return Prop_ChangePosItem;
        else if (inputInt == 4)
            return Prop_AbsorbItem;
        else if (inputInt == 5)
            return Prop_AddOneStepItem;
        else if (inputInt == 6)
            return Prop_GrabItem;
        else if (inputInt == 7)
            return StarItem;
        else if (inputInt == 12)
            return GasolineItem;
        else if (inputInt == 16)
            return RemoveAdsItem;
        else if (inputInt == 17)
            return LifeItem;
        else if (inputInt == 18)
            return InfiniteLifeTimeItem;
        else if (inputInt == 20)
            return MagnifierBoost;
        else if (inputInt == 21)
            return FireworkBoost;
        else if (inputInt == 22)
            return InfiniteMagnifierBoost;
        else if (inputInt == 23)
            return InfiniteAddOneStepBoost;
        else if (inputInt == 24)
            return InfiniteFireworkBoost;
        else if (inputInt == 25)
            return Pickaxe;
        else if (inputInt == 26)
            return MergeEnergyBox;
        else if (inputInt == 27)
            return KitchenChefHat;
        else if (inputInt == 28)
            return PkItem;
        else if (inputInt == 29)
            return KitchenBasket;
        else if (inputInt == 30)
            return KitchenKey;
        else if (inputInt == 51)
            return CardPack1Item;
        else if (inputInt == 52)
            return CardPack2Item;
        else if (inputInt == 53)
            return CardPack3Item;
        else if (inputInt == 54)
            return CardPack4Item;
        else if (inputInt == 55)
            return CardPack5Item;
        else
        {
            Log.Warning($"Notice TotalItemData FromInt {inputInt}");
            return GameManager.DataTable.GetDataTable<DTTotalItemType>().Data.GetData(inputInt);
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        TotalItemData data = (TotalItemData)obj;
        return data.ID == ID && data.RefID == RefID && data.TotalItemTypeInt == TotalItemTypeInt;
    }

    public override int GetHashCode()
    {
        int id_hashcode = ID.GetHashCode();
        int refID_hashcode = RefID.GetHashCode();
        int totalItemTypeInt_hashcode = TotalItemTypeInt.GetHashCode();
        return id_hashcode + refID_hashcode + totalItemTypeInt_hashcode;//并不是很严谨
    }

    public static bool operator == (TotalItemData lhs, TotalItemData rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator != (TotalItemData lhs, TotalItemData rhs) => !(lhs == rhs);

    public override string ToString()
    {
        return $"{ID}_{RefID}_{TotalItemTypeInt}";
    }
}
