using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MySelf.Model
{
	public class PurchaseData
	{
		public List<ProductNameType> RecordHaveBuyItems = new List<ProductNameType>();

		public Dictionary<TotalItemType, List<ProductNameType>> RecordBuyProductByPos =
			new Dictionary<TotalItemType, List<ProductNameType>>();
	}
	public class PurchaseModel:BaseModel<PurchaseModel, PurchaseData>
	{
		public void RecordBuyItem(ProductNameType type)
		{
			if (!Data.RecordHaveBuyItems.Contains(type))
			{
				Data.RecordHaveBuyItems.Add(type);
				SaveToLocal();
			}
		}

		public bool IsHaveBuy(ProductNameType type)
		{
			return Data.RecordHaveBuyItems.Contains(type);
		}

		//按照每个位置记录商品的购买情况
		public void RecordProductByPos(TotalItemType itemType ,ProductNameType productType)
		{
			if (!Data.RecordBuyProductByPos.ContainsKey(itemType))
			{
				Data.RecordBuyProductByPos.Add(itemType,new List<ProductNameType>());
			}
			Data.RecordBuyProductByPos[itemType].Add(productType);
			SaveToLocal();
		}

		public bool IsHaveBuyByPos(TotalItemType itemType,ProductNameType productType)
		{
			return Data.RecordBuyProductByPos.ContainsKey(itemType) &&
			       Data.RecordBuyProductByPos[itemType].Contains(productType);
		}

		public int GetBuyProductCountByPos(TotalItemType itemType)
		{
			return Data.RecordBuyProductByPos.ContainsKey(itemType) ? Data.RecordBuyProductByPos[itemType].Count : 0;
		}
	}
}

