using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.AddressableAssets;
using TMPro;
using MySelf.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class TileMatch_LevelTypePanel : UIForm
{
	public static int NowLevel = 0;
	[SerializeField]
	private Text LayerNum_Text, TotalTileNum_Text, TileTypeNum_Text, UnmatchedNum_Text;
	[SerializeField]
	private ToggleGroup TileGroup, AttachmentGroup;
	[SerializeField]
	private Transform DirectionGroup;
	[SerializeField]
	private Button LevelTypeLeft_Btn,LevelTypeRight_Btn,LayerNumLeft_Btn,LayerNumRight_Btn,PlayLevel_Btn;
	[SerializeField]
	private UnityOnPointEvent TileMap_Point;
	[SerializeField]
	private UnityOnPointEvent TilePrefab;
	[SerializeField]
	private Transform AllTileMap,ChooseAllTileItems;
	[SerializeField]
	private GameObject TilePoolGameObject;
	[SerializeField]
	private InputField TileTypeNum_InputField,LevelNum_InputField;
	[SerializeField]
	private TMP_Dropdown HardType_Dropdown;
	[SerializeField]
	private Button UpDownBtn,LeftRightBtn,SaveLevelBtn;
	[SerializeField]
	private ToggleGroup TileControlsGroup;
	[SerializeField]
	private UICommonController UICommonController;
	[SerializeField]
	private DelayButton PathBtn;
	[SerializeField]
	private Text PathBtn_Text;
	[SerializeField]
	private Button InterpositionRightLevelBtn;

	[SerializeField] private Button ClearLevelBtn;
	[SerializeField] private Camera Camera;

	private List<int> chooseTileItems = new List<int>();
	private int curDirectionIndex = 1;

	private int curChooseTileIndex
	{
		get
		{
			if (int.TryParse(TileGroup.GetFirstActiveToggle().name, out int result))
			{
				return result;
			}
			else
				throw new System.Exception($"{TileGroup.GetFirstActiveToggle().name} is Not Int!");
		}
	}

	private int curChooseAttachIndex
    {
        get
        {
			if (int.TryParse(AttachmentGroup.GetFirstActiveToggle().name, out int result))
			{
				return result;
			}
			else
				throw new System.Exception($"{AttachmentGroup.GetFirstActiveToggle().name} is Not Int!");
		}
    }

	private int curChooseDirectionIndex
    {
        get
        {
			return curDirectionIndex;
		}
    }

	private int curControlIndex
	{
		get
		{
			if(int.TryParse(TileControlsGroup.GetFirstActiveToggle().name,out int result))
			{
				return result;
			}
			else
				throw new System.Exception($"{TileControlsGroup.GetFirstActiveToggle().name} is Not Int!");
		}
	}

	private Dictionary<int, Dictionary<int, UnityOnPointEvent>> recordSetTileMap = new Dictionary<int, Dictionary<int, UnityOnPointEvent>>();
	private int levelIndex;//关卡数
	private int layerIndex;//层数
	private int totalItemNum;
	private TileLevelHardType tileLevelHardType= TileLevelHardType.Normal;
	private Dictionary<int, Dictionary<int, TileInfo>> allLayerTileDict = new Dictionary<int, Dictionary<int, TileInfo>>();//记录

	private int LevelType
	{
		set
		{
			if (this.levelIndex != value)
				this.levelIndex = value;

			ShowLevelType();

			LevelModel.Instance.SetLevelNum(value);
		}
	}

	private int LayerIndex
	{
		set
		{
			if (this.layerIndex != value)
				this.layerIndex = value;
			ShowLayerIndex();
		}
	}

	private TileLevelHardType TileLevelHardType
	{
		get
		{
			return tileLevelHardType;
		}
		set
		{
			tileLevelHardType = value;
			//设置显示
			SetTileHardTypeDropDown();
		}
	}

	private string PathName
	{
		get => PlayerPrefs.GetString("RecordLevelPathName","Level");
		set
		{
			if (PathName != value)
			{
				PlayerPrefs.SetString("RecordLevelPathName", value);
				//refresh panel
				Init(LevelModel.Instance.Data.Level);
			}
		}
	}

	private void Start()
	{
		Init(LevelModel.Instance.Data.Level);
		GetComponentInParent<Canvas>().worldCamera = Camera;
	}
	
	public void Init(int levelNum,bool isForceRefresh=false)
	{
		if (isForceRefresh || this.levelIndex != levelNum)
		{
			//初始化数据
			this.allLayerTileDict.Clear();
			this.recordSetTileMap.Clear();
			this.chooseTileItems.Clear();
			this.LevelType = levelNum;
			this.TileLevelHardType =levelNum%10==0?TileLevelHardType.Hard:TileLevelHardType.Normal;
			this.chooseTileItems.Clear();
			LayerIndex = 1;
			totalItemNum = 0;
			TileTypeNum_InputField.text = totalItemNum.ToString();
			//初始化按钮事件
			BtnEvent();
			SetDirection(1);

			//删除所有卡
			foreach (var child in AllTileMap.GetComponentsInChildren<UnityOnPointEvent>())
			{
				SavePoolObj(child);
			}

			TileMatch_LevelData data = TileMatch_LevelData.GetTileMatchLevelData(levelNum);
			if (data != null)
			{
				allLayerTileDict = data.AllLayerTileDict;

				LayerIndex = allLayerTileDict.Count > 0?allLayerTileDict.Keys.Max():1;
				chooseTileItems = data.TileItemArray!=null?data.TileItemArray.ToList():new List<int>();
				chooseTileItems.RemoveAll(a=>a<=1);

				totalItemNum = data.TotalItemNum;
				TileTypeNum_InputField.text = totalItemNum.ToString();
				TileLevelHardType = data.TileLevelHardType;

				foreach (var child in allLayerTileDict.Keys.OrderBy(a=>a))
				{
					foreach(var map in allLayerTileDict[child])
					{
						SetAllLayerObjDict(child, map.Key, map.Value.TileID, map.Value.AttachID, map.Value.DirectionType);
					}
				}
			}
			RefreshLayer(layerIndex);
		}
		ShowTotalNum();
	}

	private void BtnEvent()
	{
#if UNITY_EDITOR
		PlayLevel_Btn.SetBtnEvent(()=> 
		{
			SaveLocal(true);
			if (GameManager.UI.GetUIForm("TileMatch_LevelTypePanel") != null)
			{
				GameManager.UI.HideUIForm("TileMatch_LevelTypePanel");
				if (GameManager.UI.GetUIForm("TileMatchPanel") != null)
				{
					GameManager.UI.HideUIForm("TileMatchPanel");
					GameManager.UI.ShowUIForm("TileMatchPanel",form =>
					{
						(form as TileMatchPanel).StartEditorTimer();
					});
				}
			}
		});
#endif

		ClearLevelBtn.SetBtnEvent(() =>
		{
			UICommonController.Texts[0].text = $"是否删除当前关卡内容？";
			UICommonController.Texts[0].color = Color.black;
			UICommonController.Btns[0].SetBtnEvent(() =>
			{
				UICommonController.gameObject.SetActive(false);
			});
			UICommonController.Btns[1].SetBtnEvent(() =>
			{
				TileMatch_LevelData.SaveLocal(
					levelIndex, new Dictionary<int, Dictionary<int, TileInfo>>(),
					null,
					1,
					TileLevelHardType.Normal);
				Init(levelIndex,true);
				UICommonController.gameObject.SetActive(false);
			});
			UICommonController.gameObject.SetActive(true);
		});
		//切换到其他类型的关卡类型 重新刷新数据
		LevelTypeLeft_Btn.SetBtnEvent(() =>
		{
			//--
			SaveLocal();
			Init(Mathf.Max(levelIndex-1, 1));
		});
		LevelTypeRight_Btn.SetBtnEvent(() =>
		{
			//++
			SaveLocal();
			Init(Mathf.Min(levelIndex+1, int.MaxValue));
		});

		//层数切换
		LayerNumLeft_Btn.SetBtnEvent(() =>
		{
			//--
			SaveLocal();
			int curLayerIndex = Mathf.Max(1, layerIndex-1);
			RefreshLayer(curLayerIndex);
		});
		LayerNumRight_Btn.SetBtnEvent(() =>
		{
			//++
			SaveLocal();
			int curLayerIndex = layerIndex + 1;
			RefreshLayer(curLayerIndex);
		});

		//点击的时候获取位置然后生成 tile摆放
		TileMap_Point.OnLeftMouseEvent = (v) => 
		{
			Vector3 vector = TileMap_Point.transform.InverseTransformPoint(Camera.ScreenToWorldPoint(v));
			vector.z = 0;
			//根据坐标位置计算出坐标index
			//高度宽度区间
			(int, int) range = (-420,420);

			vector = new Vector3(
				Mathf.Max(Mathf.Min(range.Item2,vector.x),range.Item1),
				Mathf.Max(Mathf.Min(range.Item2, vector.y), range.Item1))+new Vector3(420,-420);

			int indexX = (int)vector.x/ 60+ ((int)vector.x)%60/30;
			int indexY =((int)vector.y)/ 60 + (int)vector.y%60/30;

			int mapIndex =indexX -indexY *16;

			if (!IsCanAddTile(mapIndex)) return;

			//数据处理 预设处理
			SetAllLayerTileDataDict(layerIndex, mapIndex, curChooseTileIndex, curChooseAttachIndex, curChooseDirectionIndex);
		};

		TileTypeNum_InputField.onEndEdit.RemoveAllListeners();
		TileTypeNum_InputField.onEndEdit.AddListener((content)=> 
		{
			SetInputNum();
			SaveLocal();
		});
		LevelNum_InputField.onEndEdit.RemoveAllListeners();
		LevelNum_InputField.onEndEdit.AddListener((content) =>
		{
			SaveLocal();
			SetLevelInputNum();
		});

		UpDownBtn.SetBtnEvent(()=>
		{
			//上下对称
			SetSymmetryData(true);
			SaveLocal();
		});
		LeftRightBtn.SetBtnEvent(()=>
		{
			//左右对称
			SetSymmetryData(false);
			SaveLocal();
		});
		SaveLevelBtn.SetBtnEvent(()=> 
		{
			int totalNum = 0;
			if (allLayerTileDict != null)
				foreach (var child in allLayerTileDict)
				{
					foreach (var tileMap in child.Value)
						totalNum++;
				}

			SaveLocal(true);
			UICommonController.gameObject.SetActive(false);
			return;
			//if (totalNum % 3 == 0)
			//{
			//	SaveLocal(true);
			//	UICommonController.gameObject.SetActive(false);
			//	return;
			//}
			//else
			//{
			//	UICommonController.Texts[0].text = $"当前总数为{totalNum},不是3的倍数！";
			//	UICommonController.Texts[0].color = Color.red;
			//}
			//UICommonController.Btns[0].SetBtnEvent(() =>
			//{
			//	UICommonController.gameObject.SetActive(false);
			//});
			//UICommonController.Btns[1].SetBtnEvent(() =>
			//{
			//	SaveLocal(true);
			//	UICommonController.gameObject.SetActive(false);
			//});
			//UICommonController.gameObject.SetActive(true);
		});

		PathBtn_Text.text = PathName;
		PathBtn.SetBtnEvent(()=> 
		{
#if UNITY_EDITOR
			string folderPath = Application.dataPath+PlayerPrefs.GetString("EPME_LastParticleCheckPath")+ "/TileMatch/Res";
			string searchPath = UnityEditor.EditorUtility.OpenFolderPanel("select path", folderPath, "");

			if (!string.IsNullOrEmpty(searchPath))
			{
				string simpPath = Path.GetFileNameWithoutExtension(searchPath);
				PathBtn_Text.text = simpPath;
				PathName = simpPath;

				Init(levelIndex,true);
			}
#endif
		});

		InterpositionRightLevelBtn.SetBtnEvent(()=> 
		{
			InterpositionRightLevelEvent(levelIndex+1);

			//++
			SaveLocal();
			Init(Mathf.Min(levelIndex + 1, int.MaxValue));
		});

        foreach (Transform directionButton in DirectionGroup)
        {
			directionButton.GetComponent<UnityOnPointEvent>().OnLeftMouseEvent = v =>
			{
				SetDirection(int.Parse(directionButton.name));
			};

			directionButton.GetComponent<UnityOnPointEvent>().OnRightMouseEvent = () =>
			{
				if (curDirectionIndex == int.Parse(directionButton.name)) 
					SetDirection(1);
			};
		}
	}

	private void SetDirection(int directionIndex)
    {
		curDirectionIndex = directionIndex;

		foreach (Transform directionButton in DirectionGroup)
        {
			directionButton.GetChild(0).gameObject.SetActive(directionButton.name == curDirectionIndex.ToString());
		}
	}

	private static void InterpositionRightLevelEvent(int nextLevelNum)
	{
#if UNITY_EDITOR
		string levelPath = PlayerPrefs.GetString("RecordLevelPathName", "Level");
		string path = $"Assets/TileMatch/Res/{levelPath}";

		int index = nextLevelNum;
		List<string> listPaths = new List<string>();
		while (index <= 1000)
		{
			string nowPath = $"{path}/{index}.json";
			if (System.IO.File.Exists(nowPath))
			{
				listPaths.Add(nowPath);
			}
			index++;
		}

		UnityEditor.AssetDatabase.StartAssetEditing();
		int startIndex = listPaths.Count + nextLevelNum;
		for (int i = listPaths.Count-1; i >= 0; i--)
		{
			UnityEditor.AssetDatabase.RenameAsset(listPaths[i], $"{startIndex}.json");
			startIndex--;
		}
		UnityEditor.AssetDatabase.StopAssetEditing();

		UnityEditor.AssetDatabase.Refresh();
		UnityEditor.AssetDatabase.SaveAssets();
#endif
	}

	private void RefreshLayer(int layerIndex)
	{
		this.LayerIndex = layerIndex;
		if (!recordSetTileMap.ContainsKey(layerIndex))
			recordSetTileMap.Add(layerIndex,new Dictionary<int, UnityOnPointEvent>());
		if (!allLayerTileDict.ContainsKey(layerIndex))
			allLayerTileDict.Add(layerIndex,new Dictionary<int, TileInfo>());
		//层数变化时刷新显示
		foreach (var child in recordSetTileMap)
		{
			int layer = child.Key;

			bool isSecondLayer = layer == layerIndex - 1;
			foreach (var tileMap in child.Value)
			{
				tileMap.Value.gameObject.SetActive(layer <= layerIndex);
				if (isSecondLayer)
				{
					tileMap.Value.SetColor(new Color(0.6f, 0.6f, 0.6f, 1f));
				}
				else
					tileMap.Value.SetColor(layer == layerIndex ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f));
				tileMap.Value.SetEnable(layer == layerIndex);
			}
		}
	}

	private void SetTilePrefabClickEvent(UnityOnPointEvent btn)
	{
		btn.OnLeftMouseEvent = (v) => 
		{
			if (int.TryParse(btn.name, out int result))
			{
				//数据处理 预设处理
				SetAllLayerTileDataDict(layerIndex, result, curChooseTileIndex, curChooseAttachIndex, curChooseDirectionIndex);
			}
		};
		btn.OnRightMouseEvent = () => 
		{
			if (int.TryParse(btn.name, out int result))
			{
				CallBackR<bool,int, int> callBack = (layer, map) =>
				{
					//删除数据
					if (allLayerTileDict.ContainsKey(layer)&&allLayerTileDict[layer].ContainsKey(map))
					{
						allLayerTileDict[layer].Remove(map);
						//删除obj
						DestroyImmediate(recordSetTileMap[layer][map].gameObject);
						//删除obj数据
						recordSetTileMap[layer].Remove(map);
						return true;
					}
					return false;
				};

				switch (curControlIndex)
				{
					case 1:
						callBack(layerIndex,result);
						break;
					case 2:
						int layer = layerIndex;
						while (layer > 0)
						{
							if (callBack(layer, result))
							{
								layer--;
							}
							else break;
						}
						break;
				}
				ShowTotalNum();
			}
		};
	}

	private void SetAllLayerTileDataDict(int layerIndex, int mapIndex, int tileIndex, int attachIndex, int directionType)
	{
		int curLayer = layerIndex;
		if (recordSetTileMap.ContainsKey(layerIndex)&&
			recordSetTileMap[layerIndex].TryGetValue(mapIndex,out UnityOnPointEvent obj))
		{
			SavePoolObj(obj);
			recordSetTileMap[layerIndex].Remove(mapIndex);
		}

		CallBack<int, int, int, int,int> callBack = (layer, map, tile, attach,direction) =>
		{
			if (!allLayerTileDict.ContainsKey(layer))
				allLayerTileDict.Add(layer, new Dictionary<int, TileInfo>());
			allLayerTileDict[layer][map] = new TileInfo(tile, attach, direction);

			//预设处理
			SetAllLayerObjDict(layer, map, tile, attach, direction);
		};

		switch (curControlIndex)
		{
			case 1://单层
				callBack.Invoke(layerIndex, mapIndex, tileIndex, attachIndex, directionType);
				break;
			case 2://多层 [填补底层空白地方]
				callBack.Invoke(layerIndex, mapIndex, tileIndex, attachIndex, directionType);
				while (layerIndex >1)
				{
					layerIndex--;

					//如果位置被占
					bool isOver = IsContainMapIndex(layerIndex,mapIndex);
					if (!isOver)
					{
						if (!allLayerTileDict.ContainsKey(layerIndex)) allLayerTileDict.Add(layerIndex, new Dictionary<int, TileInfo>());
						allLayerTileDict[layerIndex][mapIndex] = new TileInfo(tileIndex, attachIndex, directionType);
						callBack.Invoke(layerIndex, mapIndex, tileIndex, attachIndex, directionType);
					}
					else break;
				}
				break;
		}


		switch (tileIndex)
		{
			case -3:
			case -2:
			case -1:
			case 0:
			case 1:
				//需要额外处理
				while (layerIndex > 1)
				{
					layerIndex--;
					if (allLayerTileDict.ContainsKey(layerIndex) && allLayerTileDict[layerIndex].ContainsKey(mapIndex))
					{
						int attachId = 0;
						if (allLayerTileDict[layerIndex][mapIndex] != null)
							attachId = allLayerTileDict[layerIndex][mapIndex].AttachID;
						allLayerTileDict[layerIndex][mapIndex] = new TileInfo(tileIndex, attachId, directionType);
						SetAllLayerObjDict(layerIndex, mapIndex, tileIndex, attachId, directionType);
					}
					else break;
				}
				break;
		}

        switch (attachIndex)
        {
			//胶水点击一个棋子，右边棋子自动加上胶水
			case 2:
				if (mapIndex % 16 < 14 && allLayerTileDict[curLayer].ContainsKey(mapIndex + 2)) 
                {
					int tileId = allLayerTileDict[curLayer][mapIndex + 2].TileID;
					allLayerTileDict[curLayer][mapIndex + 2] = new TileInfo(tileId, attachIndex, 1);
					SetAllLayerObjDict(curLayer, mapIndex + 2, tileId, attachIndex, 1);
				}
				break;
        }
	}

	bool IsContainMapIndex(int layer,int mapIndex)
	{
		int[] mapAll = new int[] { mapIndex,mapIndex+1,mapIndex-1,mapIndex-16,mapIndex+16,mapIndex-17,mapIndex+17,mapIndex-15,mapIndex+15 };
		foreach (var child in mapAll)
		{
			if (allLayerTileDict.ContainsKey(layer) && allLayerTileDict[layer].ContainsKey(child)) return true;
		}
		return false;
	}

	private void SetAllLayerObjDict(int layerIndex, int mapIndex, int tileIndex, int attachIndex, int directionType)
	{
		Vector3 targetVect3 = new Vector3((mapIndex%16) * 60, (mapIndex%16-mapIndex)/16 * 60) - new Vector3(420, -420);

		if (!recordSetTileMap.ContainsKey(layerIndex))
			recordSetTileMap.Add(layerIndex,new Dictionary<int, UnityOnPointEvent>());
		Transform indexParent=null;
		if (!recordSetTileMap[layerIndex].TryGetValue(mapIndex,out UnityOnPointEvent obj))
		{
			indexParent = AllTileMap.Find($"{layerIndex}");
			if (indexParent == null)
			{
				GameObject layerObj = new GameObject();
				indexParent = layerObj.transform;
				indexParent.SetParent(AllTileMap);
				indexParent.localPosition = Vector3.zero;
				indexParent.localScale = Vector3.one;
			}
			obj = GetPoolObj(indexParent);
		}
		if(indexParent) indexParent.name = $"{layerIndex}";

		recordSetTileMap[layerIndex][mapIndex] = obj;
		obj.name = mapIndex.ToString();
		SetTilePrefabClickEvent(obj);
		ShowTotalNum();
		obj.SetSprite(TileMatchUtil.GetTileSprite(tileIndex));
		((TileOnPointEvent)obj).SetAttachSprite(GetAttachSprite(attachIndex));
		((TileOnPointEvent)obj).SetDirectionSprite(GetDirectionSprite((TileMoveDirectionType)directionType));
		obj.SetColor(layerIndex == this.layerIndex ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f));
		obj.transform.localPosition = targetVect3;

		int index = 0;
		foreach (var child in recordSetTileMap.Keys.OrderBy(a => a))
		{
			indexParent = AllTileMap.Find($"{child}");
			if (indexParent == null) continue;
			indexParent.SetSiblingIndex(index);
			index++;
		}
	}

	private Sprite GetAttachSprite(int attachId)
	{
		if (attachId == 0) return null;

		UnityEngine.U2D.SpriteAtlas atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>("TileEditor");
		return atlas.GetSprite(attachId.ToString());
    }

	private Sprite GetDirectionSprite(TileMoveDirectionType directionType)
	{
		if (directionType == TileMoveDirectionType.None) return null;

		UnityEngine.U2D.SpriteAtlas atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>("TileEditor");
		return atlas.GetSprite(directionType.ToString());
	}

	private void ShowLevelType()
	{
		LevelNum_InputField.text = levelIndex.ToString();
	}

	private void ShowLayerIndex()
	{
		LayerNum_Text.text = layerIndex.ToString();
	}

	private void ShowTotalNum()
	{
		int index = 0;
		if(allLayerTileDict!=null)
			foreach (var child in allLayerTileDict)
			{
				foreach (var tileMap in child.Value)
					index++;
			}
		TotalTileNum_Text.text =$"总数:"+index.ToString();
		//TotalTileNum_Text.color = index % 3 == 0 ? Color.white : Color.red;

		Dictionary<int, List<int>> recordTileTexIndexDict = new Dictionary<int, List<int>>();
		foreach (var child in allLayerTileDict)
		{
			foreach (var map in child.Value)
			{
				if (!recordTileTexIndexDict.ContainsKey(map.Value.TileID)) recordTileTexIndexDict.Add(map.Value.TileID, new List<int>());

				recordTileTexIndexDict[map.Value.TileID].Add(map.Value.TileID);
			}
		}

		int randomTileNum = 0;
		int missingTileNum = 0;
        foreach (var pair in recordTileTexIndexDict)
        {
			if (pair.Key >= -3 && pair.Key <= 1)
			{
				randomTileNum += pair.Value.Count;
			}
			else if (pair.Value.Count % 3 != 0 && !TileMatchUtil.IsSpecial(pair.Key)) 
            {
				missingTileNum += (3 - pair.Value.Count % 3);
            }
		}

		int unmatchNum = 0;
		if (missingTileNum > randomTileNum)
		{
			unmatchNum = missingTileNum - randomTileNum;
		}
		else if ((randomTileNum - missingTileNum) % 3 != 0) 
        {
			unmatchNum = 3 - (randomTileNum - missingTileNum) % 3;
		}

		UnmatchedNum_Text.text = $"未匹配数:" + unmatchNum.ToString();
		UnmatchedNum_Text.color = unmatchNum == 0 ? Color.white : Color.red;

		ShowAllTileItem();
		ShowAllAttachItem();
		ShowChooseTileArray();
		ShowChooseItemObjs();
	}

	private void ShowChooseTileArray()
	{
		List<int> itemList = new List<int>();
		foreach (var child in allLayerTileDict)
		{
			foreach (var map in child.Value)
			{
				if (!itemList.Contains(map.Value.TileID)) itemList.Add(map.Value.TileID);
			}
		}
		chooseTileItems.AddRange(itemList);
		chooseTileItems=chooseTileItems.Distinct().ToList();
		chooseTileItems.RemoveAll(a => a <= 1 || TileMatchUtil.IsSpecial(a));//排除第一个

		SetInputNum();
	}

	private void ShowChooseItemObjs()
	{
		UnityUtility.FillGameObjectWithFirstChild<Button>(ChooseAllTileItems.gameObject,TileMatchUtil.TileMatchSprites.Count,(index,comp)=> 
		{
			int itemIndex = TileMatchUtil.TileMatchSprites.ElementAt(index).Key;//从第二个开始
			Sprite sprite = TileMatchUtil.TileMatchSprites.ElementAt(index).Value;

			comp.name = (itemIndex).ToString();
			comp.image.color = chooseTileItems.Contains(itemIndex)?Color.white:new Color(82/255f, 82/255f, 82 / 255f, 1);
			comp.image.sprite = sprite;
			comp.SetBtnEvent(()=> 
			{
				if (chooseTileItems.Contains(itemIndex)) chooseTileItems.Remove(itemIndex);
				else chooseTileItems.Add(itemIndex);

				ShowChooseTileArray();
				ShowChooseItemObjs();
			});
		});
	}

	private bool IsCanAddTile(int mapIndex)
	{
		bool isCanAdd = true;
		List<int> indexs = TileMatchUtil.GetLinkTileIndex(mapIndex);
		foreach (var child in indexs)
		{
			if (child == mapIndex) continue;
			isCanAdd= !(allLayerTileDict[layerIndex].ContainsKey(child));
			if (!isCanAdd) break;
		}
		return isCanAdd;
	}

	bool isFist = true;
	private void ShowAllTileItem()
	{
		Dictionary<int, int> dict = new Dictionary<int, int>();
		if(allLayerTileDict!=null)
			foreach (var child in allLayerTileDict)
			{
				foreach (var map in child.Value)
				{
					if (!dict.ContainsKey(map.Value.TileID)) dict.Add(map.Value.TileID, 0);
					dict[map.Value.TileID] = dict[map.Value.TileID] + 1;
				}
			}
        Dictionary<int, Sprite> spriteDict = TileMatchUtil.AllTileMatchSprites(TileMatchUtil.CurTileImageIndex);
		//special tile
		UnityEngine.U2D.SpriteAtlas atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>("TileEditor");
		int[] specialTileIds = new int[] { 20 };
		if (atlas != null)
		{
			for (int i = 0; i < specialTileIds.Length; i++)
			{
				if (!spriteDict.ContainsKey(specialTileIds[i]))
					spriteDict.Add(specialTileIds[i], atlas.GetSprite(specialTileIds[i].ToString()));
			}
		}

		UnityUtility.FillGameObjectWithFirstChild<UICommonController>(TileGroup.gameObject, spriteDict.Count, (index,comp)=> 
		{
			int itemIndex = spriteDict.ElementAt(index).Key;
			Sprite sprite = spriteDict.ElementAt(index).Value;

			int curNum = dict.ContainsKey(itemIndex) ?dict[itemIndex] :0;

			comp.name = (itemIndex).ToString();
			if (isFist && itemIndex == 1)
			{
				comp.Toggle.isOn = true;
				isFist = false;
			}
			else
				comp.Toggle.isOn = false;

			comp.Images[0].sprite = comp.Images[1].sprite = sprite;
			comp.Texts[0].text = curNum==0?"":curNum.ToString();
			if (!TileMatchUtil.IsSpecial(itemIndex))
				comp.Texts[0].color = curNum % 3 == 0 ? Color.green : Color.red;
			else
				comp.Texts[0].color = Color.green;
		});
    }

	bool isFirst = true;
	private void ShowAllAttachItem()
    {
		Dictionary<int, int> dict = new Dictionary<int, int>();
		if (allLayerTileDict != null)
			foreach (var child in allLayerTileDict)
			{
				foreach (var map in child.Value)
				{
					if (map.Value.AttachID == 0) continue;
					if (!dict.ContainsKey(map.Value.AttachID))  dict.Add(map.Value.AttachID, 0);
					dict[map.Value.AttachID] = dict[map.Value.AttachID] + 1;
				}
			}

		Dictionary<int, Sprite> spriteDict = new Dictionary<int, Sprite>();
        UnityEngine.U2D.SpriteAtlas atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>("TileEditor");

		int index = 0;
		while (true)
		{
			if (atlas == null) break;
			var sprite = atlas.GetSprite(index.ToString());
			if (sprite == null) break;

			spriteDict.Add(index, sprite);
			index += 1;
		}

		UnityUtility.FillGameObjectWithFirstChild<UICommonController>(AttachmentGroup.gameObject, spriteDict.Count, (index, comp) =>
		{
			int itemIndex = spriteDict.ElementAt(index).Key;
			Sprite sprite = spriteDict.ElementAt(index).Value;

			int curNum = dict.ContainsKey(itemIndex) ? dict[itemIndex] : 0;

			comp.name = (itemIndex).ToString();
			if (isFirst && itemIndex == 0)
			{
				comp.Toggle.isOn = true;
				isFirst = false;
			}
			else
				comp.Toggle.isOn = false;

			comp.Images[0].sprite = comp.Images[1].sprite = sprite;
			comp.Texts[0].text = curNum == 0 ? "" : curNum.ToString();

			if (itemIndex == 2)
				comp.Texts[0].color = curNum % 2 == 0 ? Color.green : Color.red;
		});
	}

	private UnityOnPointEvent GetPoolObj(Transform parent)
	{
		if (TilePoolGameObject.transform.childCount > 0)
		{
			var obj= TilePoolGameObject.transform.GetChild(0);
			obj.SetParent(parent);
			return obj.gameObject.GetComponent<UnityOnPointEvent>();
		}
		else
		{
			return GameObject.Instantiate(TilePrefab, parent);
		}
	}

	private void SavePoolObj(UnityOnPointEvent point)
	{
		DestroyImmediate(point.gameObject);
		//point.transform.SetParent(TilePoolGameObject.transform);
	}

	private void SetInputNum()
	{
		if (int.TryParse(TileTypeNum_InputField.text, out int reslut))
		{
			TileTypeNum_InputField.text = Mathf.Clamp(reslut, chooseTileItems.Count, TileMatchUtil.TileMatchSprites.Count).ToString();
		}else
		{
			TileTypeNum_InputField.text = Mathf.Clamp(totalItemNum, chooseTileItems.Count, TileMatchUtil.TileMatchSprites.Count).ToString();
		}
	}

	private void SetLevelInputNum()
	{
		if (int.TryParse(LevelNum_InputField.text, out int reslut))
		{
			Init(Mathf.Min(reslut, int.MaxValue));
		}
		else
		{
			Init(Mathf.Min(levelIndex, int.MaxValue));
		}
	}

	private void SetTileHardTypeDropDown()
	{
		HardType_Dropdown.value = (int)TileLevelHardType;
		HardType_Dropdown.onValueChanged.RemoveAllListeners();
		HardType_Dropdown.onValueChanged.AddListener((index)=>
		{
			TileLevelHardType = (TileLevelHardType)index;
		});
	}

	//保存当前数据
	private void SaveLocal(bool isAuto = false)
	{
		if(allLayerTileDict.Count<=0)return;
		if (isAuto)
		{
			while (allLayerTileDict != null && allLayerTileDict.Count > 0)
			{
				var last = allLayerTileDict.LastOrDefault();
				if (last.Value.Count == 0)
					allLayerTileDict.Remove(last.Key);
				else
					break;
			}
			TileMatch_LevelData.SaveLocal(
				levelIndex, allLayerTileDict,
				chooseTileItems.ToArray(),
				int.Parse(TileTypeNum_InputField.text),
				tileLevelHardType);
		}
	}

	private void OnDisable()
	{
		SaveLocal(true);
		NowLevel = levelIndex;
	}

	private void SetSymmetryData(bool isUpDown)
	{
		Dictionary<int,TileInfo> dict = new Dictionary<int, TileInfo>();

		if(allLayerTileDict.ContainsKey(layerIndex))
			foreach (var map in allLayerTileDict[layerIndex])
			{
				dict[map.Key] = map.Value;

				int x = map.Key % 16;
				int y = map.Key / 16;

                int newMap;
                if (isUpDown)
					newMap = x + (14 - y) * 16;
				else
					newMap = (14 - x) + y * 16;
				if (newMap != map.Key&&!IsContainMapIndex(layerIndex,newMap))
				{
					dict[newMap]=map.Value;
				}
			}
		allLayerTileDict[layerIndex] = dict;

		var trans = AllTileMap.Find($"{layerIndex}");
		if (trans != null) DestroyImmediate(trans.gameObject);
		recordSetTileMap.Remove(layerIndex);
		foreach (var map in allLayerTileDict[layerIndex])
		{
			SetAllLayerObjDict(layerIndex, map.Key, map.Value.TileID, map.Value.AttachID, map.Value.DirectionType);
		}
	}
}

[SerializeField]
public enum TileLevelHardType
{
	Normal=0,
	Hard,
	Heroic,
}

public class TilePos
{
	public int X;
	public int Y;
	public int TileID;
}

[Serializable]
public class TileInfo
{
	public int TileID;
	public int AttachID;
	public int DirectionType;

	public TileInfo(int tileId,int attachId,int directionType)
    {
		TileID = tileId;
		AttachID = attachId;
		DirectionType = directionType;
	}
}

//关卡类型
[Serializable]
public class TileMatch_LevelData
{
	//所有层数对应的Tile
	[JsonConverter(typeof(TileDictConverter))]
	public Dictionary<int, Dictionary<int, TileInfo>> AllLayerTileDict;
	//如果是金块关，记录金块的数量
	public int GoldTileCount = 0;
	//如果是油桶关，记录油桶的数量
	public int GasolineCount = 0;

	//选中元素
	public int[] TileItemArray;
	//需要的总的资源数
	public int TotalItemNum;

	public float[] MoveVect;

	public int TileTotalNum;

	//难度
	[JsonConverter(typeof(StringEnumConverter))]
	public TileLevelHardType TileLevelHardType;

	[JsonIgnore]
	public float Scale = 1f;
	[JsonIgnore]
	public Dictionary<int, Dictionary<int, TileMoveDirectionType>> MapMoveDIct;

	private int totalCount = 0;
	public int TotalCount
	{
		get
		{
			if (totalCount == 0)
			{
				foreach (var layer in AllLayerTileDict)
				{
					foreach (var map in layer.Value)
					{
						totalCount++;
					}
				}
			}
			return totalCount;
		}
	}

	public TileMatch_LevelData() { }

	public TileMatch_LevelData(Dictionary<int, Dictionary<int, TileInfo>> dict, int[] tileItems, int totalItemNum, TileLevelHardType tileLevelHardType)
	{
		this.AllLayerTileDict = dict;
		this.TileItemArray = tileItems;
		this.TotalItemNum = totalItemNum;
		this.TileLevelHardType = tileLevelHardType;
		this.MoveVect = GetMoveVect(dict, out Scale, out MapMoveDIct);
	}

	public static void SaveLocal(int levelIndex, Dictionary<int, Dictionary<int, TileInfo>> dict, int[] tileItems,
		int totalItemNum, TileLevelHardType tileLevelHardType)
	{
#if UNITY_EDITOR
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			TileMatch_LevelData data = new TileMatch_LevelData(dict, tileItems, totalItemNum, tileLevelHardType);

			string levelPath = PlayerPrefs.GetString("RecordLevelPathName", string.Empty);
			levelPath = string.IsNullOrEmpty(levelPath) ? "Level" : levelPath;
			string path = Application.dataPath + $"/TileMatch/Res/{levelPath}/{levelIndex}.json";
			using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
			{
				var json = ToJson(data);
				byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
				fileStream.Write(byteArray, 0, byteArray.Length);
				fileStream.Close();
			}
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
		}
#endif
	}

	public static TileMatch_LevelData GetTileMatchLevelData(int levelNum, bool isMain = false)
	{
#if UNITY_EDITOR
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			string levelPath = PlayerPrefs.GetString("RecordLevelPathName", string.Empty);
			levelPath = string.IsNullOrEmpty(levelPath) ? "Level" : levelPath;
			string path = $"Assets/TileMatch/Res/{levelPath}/{levelNum}.json";
			Log.Info($"{path}");
			var text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
			if (text == null)
			{
				path = $"Assets/TileMatch/Res/Level/{levelNum}.json";
				text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
			}
			if (text != null)
			{
				TileMatch_LevelData data = FromJson(text.text);
				data.MoveVect = GetMoveVect(data.AllLayerTileDict, out data.Scale, out data.MapMoveDIct);
				return data;
			}
		}
		else
#endif
		{
			int abType = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Use_Level_Type_Index, 0);
			string levelPath = string.Empty;
			switch (abType)
			{
				case 0:
					levelPath = "Level"; break;
				case 1:
					levelPath = "Level_A"; break;
				case 2:
					levelPath = "Level_B"; break;
				case 3:
					levelPath = "Level_C"; break;
			}
			Log.Info("abType is：" + abType.ToString());
			string localPath = PlayerPrefs.GetString("RecordLevelPathName", string.Empty);
			levelPath = string.IsNullOrEmpty(localPath) ? levelPath : localPath;

			string path = $"Assets/TileMatch/Res/{levelPath}/{levelNum}.json";
			Log.Info($"{path}");

			TextAsset text = null;
			try
			{
				text = AddressableUtils.LoadAsset<TextAsset>(path);
			}
			catch(Exception e)
			{
				Debug.LogError("Addressable load level fail:" + path);
			}
			
			if (text == null)
			{
				path = $"Assets/TileMatch/Res/Level/{levelNum}.json";
				text = AddressableUtils.LoadAsset<TextAsset>(path);
			}
			if (text != null)
			{
				TileMatch_LevelData data = FromJson(text.text);
				Addressables.Release(text);
				data.MoveVect = GetMoveVect(data.AllLayerTileDict, out data.Scale, out data.MapMoveDIct);
				return data;
			}
		}
		return null;
	}

	public static void ClearTileMatchLevelData()
    {
	}

	public static TileMatch_LevelData GetTileMatchCalendarChallengeLevelTypeData(int levelNum)
	{
		levelNum -= 9000000;
		string levelPath = "Level_Challenge_Dif";
		string path = $"Assets/TileMatch/Res/{levelPath}/{levelNum}.json";
		//AsyncOperationHandle async=new AsyncOperationHandle();
		var async = Addressables.LoadAssetAsync<TextAsset>(path);
		var text = async.WaitForCompletion();
		//var text= AddressableUtils.LoadAsset<TextAsset>(path,ref async);
		if (text == null)
		{
			path = $"Assets/TileMatch/Res/Level/{levelNum}.json";
			//text = AddressableUtils.LoadAsset<TextAsset>(path,ref async);
			var async2 = Addressables.LoadAssetAsync<TextAsset>(path);
			text = async2.WaitForCompletion();
		}
		if (text != null)
		{
			TileMatch_LevelData data = FromJson(text.text);
			//Addressables.Release(async);
			// GameManager.Resource.Release(text);
			data.MoveVect = GetMoveVect(data.AllLayerTileDict, out data.Scale, out data.MapMoveDIct);
			//data.MoveVect = data.GetMoveVect();
			return data;
		}
		return null;
	}
	
	public static string ToJson(TileMatch_LevelData data)
	{
		return JsonConvert.SerializeObject(data,Formatting.Indented);
	}

	public static TileMatch_LevelData FromJson(string content)
	{
		return JsonConvert.DeserializeObject<TileMatch_LevelData>(content);
	}
	private static float[] GetMoveVect(Dictionary<int, Dictionary<int, TileInfo>> dict,out float scale,out Dictionary<int, Dictionary<int, TileMoveDirectionType>> mapDict)
	{
		mapDict = new Dictionary<int, Dictionary<int, TileMoveDirectionType>>();

		Vector2Int maxVect = -8*Vector2Int.one;
		Vector2Int minVect = 8*Vector2Int.zero;

		int maxDownTileY = -20;
		int downTileLayer = 0;

		foreach (var layer in dict)
		{
			foreach (var map in layer.Value)
			{
				int xIndex = map.Key % 16;
				int yIndex = map.Key / 16;

				maxVect.x = Mathf.Max(xIndex-7,maxVect.x);
				maxVect.y = Mathf.Max(yIndex-7,maxVect.y);
				minVect.x = Mathf.Min(xIndex-7,minVect.x);
				minVect.y = Mathf.Min(yIndex-7,minVect.y);

				if (map.Key == -2)
				{
					if (yIndex >= maxDownTileY)
					{
						downTileLayer = layer.Key;
						maxDownTileY = yIndex;
					}
				}
			}
		}
		float xMove = maxVect.x + minVect.x;
		float yMove = maxVect.y + minVect.y+0.2f* Mathf.Max(0,(downTileLayer-1));


		if (GameManager.Firebase.GetBool(Constant.RemoteConfig.ItemFunction_Change_Scale, false))
		{
			float yChange = SafeArea.GetScaleChangeByScreenHeight();
			float scaleX = 7f/(Mathf.Max((maxVect.x-minVect.x)/2f)+1);
			float scaleY =yChange * 8f/(Mathf.Max((maxVect.y-minVect.y)/2f)+1);
			scale = Mathf.Min(scaleX,scaleY);
			scale = Mathf.Clamp(scale,0.875f,GetMapMaxScaleByRemote);
			return new float[2] { -xMove*scale/2f,-yMove*scale/2f};	
		}
		else
		{
			float scaleY = 7f/(Mathf.Max((maxVect.x-minVect.x)/2f)+1);
			float scaleX = 7f/(Mathf.Max((maxVect.y-minVect.y)/2f)+1);
			if (Screen.safeArea.height / Screen.safeArea.width > 1920 / 1080f)
				scaleX = 1f;
			scale = Mathf.Min(scaleX,scaleY);
			scale = Mathf.Clamp(scale,0.875f,1.4f);
			return new float[2] { -xMove/2f,-yMove/2f};
		}
	}

	public static float GetMapMaxScaleByRemote => (float)GameManager.Firebase.GetDouble("Map_Max_Scale", 1.2d);

	// private static float[] GetMoveVect(Dictionary<int, Dictionary<int, TileInfo>> dict,out float scale,out Dictionary<int, Dictionary<int, TileMoveDirectionType>> mapDict)
	// {
	// 	mapDict = new Dictionary<int, Dictionary<int, TileMoveDirectionType>>();
	//
	// 	Vector2Int maxVect = -8*Vector2Int.one;
	// 	Vector2Int minVect = 8*Vector2Int.zero;
	//
	// 	int maxDownTileY = -20;
	// 	int downTileLayer = 0;
	//
	// 	foreach (var layer in dict)
	// 	{
	// 		foreach (var map in layer.Value)
	// 		{
	// 			int xIndex = map.Key % 16;
	// 			int yIndex = map.Key / 16;
	//
	// 			maxVect.x = Mathf.Max(xIndex-7,maxVect.x);
	// 			maxVect.y = Mathf.Max(yIndex-7,maxVect.y);
	// 			minVect.x = Mathf.Min(xIndex-7,minVect.x);
	// 			minVect.y = Mathf.Min(yIndex-7,minVect.y);
	//
	// 			if (map.Key == -2)
	// 			{
	// 				if (yIndex >= maxDownTileY)
	// 				{
	// 					downTileLayer = layer.Key;
	// 					maxDownTileY = yIndex;
	// 				}
	// 			}
	// 		}
	// 	}
	// 	float xMove = maxVect.x + minVect.x;
	// 	float yMove = maxVect.y + minVect.y+0.2f* Mathf.Max(0,(downTileLayer-1));
	//
	//
	// 	float scaleY = 7f/(Mathf.Max((maxVect.x-minVect.x)/2f)+1);
	// 	float scaleX = 7f/(Mathf.Max((maxVect.y-minVect.y)/2f)+1);
	// 	if (Screen.safeArea.height / Screen.safeArea.width > 1920 / 1080f)
	// 		scaleX = 1f;
	// 	scale = Mathf.Min(scaleX,scaleY);
	// 	scale = Mathf.Clamp(scale,0.875f,1.4f);
	// 	return new float[2] { -xMove/2f,-yMove/2f};
	// }
}
