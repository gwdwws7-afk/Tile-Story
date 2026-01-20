using System;
using Newtonsoft.Json;
using UnityEngine;

namespace MySelf.Model
{
	public class AdsModelData
	{
		public bool IsRemoveAds=false;

		public bool IsUseMytarget=false;
		
		public DateTime EndShowRemoveAdsIconDateTime=DateTime.MinValue;
	}
	public class AdsModel:BaseModel<AdsModel, AdsModelData>
	{
        public void SetRemoveAds(bool isRemoveAds)
		{
			if (Data.IsRemoveAds == isRemoveAds) return;

			Data.IsRemoveAds = isRemoveAds;
			SaveToLocal();
		}
		public void SetUseMytarget(bool isUseMytarget)
		{
			if (Data.IsUseMytarget == isUseMytarget) return;

			Data.IsUseMytarget = isUseMytarget;
			SaveToLocal();
		}
		
		public DateTime EndDateTime
		{
			get
			{
				if (Data.EndShowRemoveAdsIconDateTime == DateTime.MinValue)
				{
					Data.EndShowRemoveAdsIconDateTime = DateTime.UtcNow.AddDays(3);
					SaveToLocal();
				}
				return Data.EndShowRemoveAdsIconDateTime.ToLocalTime();
			}
		}

		public bool IsCanShowPiggyBank => Data.IsRemoveAds || EndDateTime <= DateTime.Now;
	}
}

