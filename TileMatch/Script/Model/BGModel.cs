using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MySelf.Model
{
	public class BGModelData
	{
		public int MapBGImage=1;
		public int BGImageID=1;
		public int TileIconID=1;
		public bool IsChooseTileID = false;

		public bool IsShowBGRedPoint = false;
		public List<int> ShowRedPointBGIds = new List<int>();

		public List<int> HaveBuyTileIDs=new List<int>();
		public List<int> HaveBuyBGIDs = new List<int>();
	}

	public class BGModel:BaseModelService<BGModel,BGModelData>
	{
		#region Service
		public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
		{
			return new Dictionary<string, object>()
			{
				{"BGModel.TileIconID",Data.TileIconID},
				{"BGModel.BGImageID",Data.BGImageID},
				{"BGModel.HaveBuyTileIDs",JsonConvert.SerializeObject(Data.HaveBuyTileIDs)},
				{"BGModel.HaveBuyBGIDs",JsonConvert.SerializeObject(Data.HaveBuyBGIDs)},
			};
		}

		public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
		{
			if (dictionary != null)
			{
				foreach (var item in dictionary)
				{
					switch (item.Key)
					{
						case "BGModel.TileIconID":
							Data.TileIconID = Convert.ToInt32(item.Value);
							break;
						case "BGModel.BGImageID":
							Data.BGImageID = Convert.ToInt32(item.Value);
							break;
						case "BGModel.HaveBuyTileIDs":
							Data.HaveBuyTileIDs = JsonConvert.DeserializeObject<List<int>>((string)item.Value);
							break;
						case "BGModel.HaveBuyBGIDs":
							Data.HaveBuyBGIDs = JsonConvert.DeserializeObject<List<int>>((string)item.Value);
							break;
					}
				}
				SaveToLocal();
			}
		}
		#endregion
		public int GetTileId()
		{
			return Data.TileIconID;
		}

		public void BuyBGID(int id)
		{
			Data.HaveBuyBGIDs.Add(id);
			SaveToLocal();
		}

		public void BuyTileID(int id)
		{
			Data.HaveBuyTileIDs.Add(id);
			SaveToLocal();
		}

		public bool SetBGImageID(int id)
		{
			if (id == Data.BGImageID) return false;

			Data.BGImageID = id;
			SaveToLocal();
			return true;
		}
		public bool SetTileIconID(int id)
		{
			if (id == Data.TileIconID) return false;

			Data.IsChooseTileID = true;
			Data.TileIconID = id;
			SaveToLocal();
			return true;
		}

		public bool IsOwnTileID(int id)
		{
			return Data.HaveBuyTileIDs != null && Data.HaveBuyTileIDs.Contains(id);
		}

		public bool IsOwnBGID(int id)
		{
			return Data.HaveBuyBGIDs != null && Data.HaveBuyBGIDs.Contains(id);
		}

		public bool IsOwnBGID(int[] ids)
		{
			foreach (var id in ids) if (!IsOwnBGID(id)) return false;

			return true;
		}

		public void SetBGRedPointStatus(int id)
		{
			Data.IsShowBGRedPoint = true;
			Data.ShowRedPointBGIds.Add(id);
			SaveToLocal();
		}

		public void RemoveBGRedPointStatus()
		{
			Data.IsShowBGRedPoint = false;
			SaveToLocal();
		}

		public void RemoveBGIDRedPoint(int id)
		{
			Data.IsShowBGRedPoint = false;
			Data.ShowRedPointBGIds.Remove(id);
			SaveToLocal();
		}
	}
}

