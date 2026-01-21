using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using DG.Tweening;
using Firebase.Analytics;

public class TileImageCell : FancyScrollRectCell<int, Context>
{
    [SerializeField]
    private Image[] TileImages;
    [SerializeField]
    private ExtensionsToggle Toggle;
    [SerializeField]
    private DelayButton Buy_Btn, Use_Btn, JumpToPackBuyBtn, JumpToActivityBtn;
    [SerializeField]
    private TextMeshProUGUI Coin_Text;

    [SerializeField] 
    private TextMeshProUGUILocalize TileName_Text,TileName1_Text;
    [SerializeField]
    private GameObject newTileMark;
    [SerializeField]
    private GameObject eventLimitedMark;

    [SerializeField]
    int recordID;
    [SerializeField]
    TileData data;

    private void Awake()
    {
        SetBtnEvent();
    }

    private void SetBtnEvent()
    {
        Toggle.onValueCanChanged = IsCanToggle;
        Toggle.SetBtnEvent(ToggleHandle);

        Buy_Btn.SetBtnEvent(()=> 
        {
            if (GameManager.PlayerData.UseItem(TotalItemData.Coin, data.TilePrice))
            {
                GameManager.PlayerData.BuyTileID(data.ID);
                Buy_Btn.gameObject.SetActive(false);

                Use_Btn.body.transform.DOScale(Vector3.one,0.4f).SetEase(Ease.OutBack);
                Use_Btn.gameObject.SetActive(true);

                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.Theme_Tile_Buy,
                    new Parameter("TileId",data.ID));
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.Objective));
            }
            else
			{
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.Coin_Not_Enough,
                    new Parameter("EntranceID",4));
                GameManager.UI.ShowUIForm("ShopMenuManager");
			}
        });
        Use_Btn.SetBtnEvent(()=> 
        {
            GameManager.PlayerData.TileImageIndex = data.ID;
            Toggle.IsOn = true;
            GameManager.Firebase.RecordMessageByEvent(
                Constant.AnalyticsEvent.Theme_Tile_Use,
                new Parameter("TileId",data.ID));
        });
        JumpToPackBuyBtn.SetBtnEvent(() =>
        {
            if (IsSpecialID())
            {
                GameManager.UI.ShowUIForm("ValentinePackMenu", UIFormType.PopupUI);
            }
        });
        JumpToActivityBtn.SetBtnEvent(() =>
        {
            if (data.ID == Merge.MergeManager.Instance.TileId && Merge.MergeManager.Instance.CheckActivityHasStarted())
            {
                GameManager.UI.ShowUIForm(Merge.MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
                GameManager.UI.HideUIForm("ChangeImagePanel");
                return;
            }
        });
    }

    private void ShowOther()
    {
        Coin_Text.text = data.TilePrice.ToString();
        TileName_Text.SetTerm("General.Theme" + data.ID);
        TileName1_Text.SetTerm("General.Theme" + data.ID);

        if (IsCurTile())
        {
            TileName_Text.SetMaterialPreset(MaterialPresetName.Text_Orange);
        }
        else
        {
            TileName_Text.SetMaterialPreset(MaterialPresetName.Shop_Purple2);
        }
        // TileName_Text.text = data.TileName.ToString();
        // TileName1_Text.text = data.TileName.ToString();

        newTileMark.SetActive(IsSpecialID());
    }

    private bool IsCanToggle()
    {
        return IsUnLock() && IsOwn() && !IsCurTile();
    }

    private void ToggleHandle(bool isActive)
    {
        if (isActive)
        {
            //GameManager.PlayerData.TileImageIndex = data.ID;
        }
    }

	public override void UpdateContent(int tileImageIndex)
    {
        //if (this.recordID == tileImageIndex) return;
        this.recordID = tileImageIndex;

        data = GameManager.DataTable.GetDataTable<DTTileID>().Data.GetData(tileImageIndex);

        bool isOwn = IsOwn();
        bool isUnlock = IsUnLock();
        bool isCurImage = IsCurTile();
        bool isSpecialID = IsSpecialID();
        bool isToActivity = IsToActivity(isOwn);
        bool isEventLimited = IsEventLimited();

        gameObject.name = tileImageIndex.ToString();
        Toggle.interactable = isUnlock && isOwn;
        Toggle.IsOn = isCurImage;
        Buy_Btn.gameObject.SetActive(isUnlock && !isOwn && !isSpecialID && !isToActivity && !isEventLimited);
        Use_Btn.gameObject.SetActive(isUnlock && isOwn);
        JumpToPackBuyBtn.gameObject.SetActive(isUnlock && !isOwn && isSpecialID);
        JumpToActivityBtn.gameObject.SetActive(isUnlock && !isOwn && !isSpecialID && isToActivity);
        eventLimitedMark.SetActive(isUnlock && !isOwn && !isSpecialID && !isToActivity && isEventLimited);

        ShowOther();

        for (int i = 0; i < TileImages.Length; i++)
        {
            TileImages[i].sprite = TileMatchUtil.GetTileSprite(tileImageIndex,i+2);
        }
    }

    private bool IsUnLock()
    {
        return true;
        return GameManager.PlayerData.NowLevel >= data.TileUnlockLevel;
    }

    private bool IsOwn()
    {
        return GameManager.DataTable.GetDataTable<DTTileID>().Data.IsOwn(data.ID)||GameManager.PlayerData.IsOwnTileID(data.ID);
    }

    private bool IsCurTile()
    {
        return GameManager.PlayerData.TileImageIndex == data.ID;
    }

#if UNITY_EDITOR
	private void OnValidate()
	{
        if (TileImages == null|| TileImages.Length==0)
        {
            TileImages = transform.Find("TileGrid").GetComponentsInChildren<Image>();
        }
	}
#endif

    private void OnDestroy()
    {
        for (int i = 0; i < TileImages.Length; i++)
        {
            TileImages[i].sprite = null;
        }
        TileMatchUtil.ClearTileSprite();
    }

    private bool IsSpecialID()
    {
        //如果在礼包周期内
        if (data.ID == 1004 && DateTime.Now > ValentinePackMenu.GiftPackStartTime &&
            DateTime.Now < ValentinePackMenu.GiftPackEndTime &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPackLevel) 
        {
            return true;
        }
        return false;
    }

    //是否点击跳转活动
    private bool IsToActivity(bool isOwn)
    {
        return data.ID == Merge.MergeManager.Instance.TileId && !isOwn && Merge.MergeManager.Instance.CheckActivityHasStarted();
    }

    //是否活动限定
    private bool IsEventLimited()
    {
        if (data.ID == 1001 || data.ID == 1002)
            return true;

        Merge.IDataTable<Merge.DRMergeSchedule> dataTable = Merge.MergeManager.DataTable.GetDataTable<Merge.DRMergeSchedule>();
        Merge.DRMergeSchedule[] allDatas = dataTable.GetAllDataRows();

        foreach (Merge.DRMergeSchedule scheduleData in allDatas)
        {
            if (scheduleData.TileId == data.ID)
                return true;
        }

        return false;
    }
}
