using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class BGThemeCell : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUILocalize ThemeName_Text,LevelNum_Text;
    [SerializeField]
    private Transform BGParent;
    [SerializeField]
    private Image BG_Image;
    [SerializeField]
    private GameObject Medal_Obj;
    [SerializeField]
    private Sprite[] BGSprites;
    [SerializeField]
    private MaterialPresetName[] TextMaterial;

    private int themeID;
    private List<BGItemData> bgList;
    public void Init(int bgThemeID)
    {
        //if (this.themeID == bgThemeID) return;
        this.themeID = bgThemeID;

        int nowLevel = GameManager.PlayerData.NowLevel;
        var dtBGID = GameManager.DataTable.GetDataTable<DTBGID>().Data;
        bgList = dtBGID.BGThemeDict[themeID];
        bool isAllUnlock = true;
        foreach (var bgData in bgList)
        {
            if (bgData.BGPrice > 0)
            {
                if (!GameManager.PlayerData.IsOwnBGID(bgData.ID))
                {
                    isAllUnlock = false;
                    break;
                }
            }
            else
            {
                if (nowLevel < bgData.BGUnlockLevel)
                {
                    isAllUnlock = false;
                    break;
                }
            }
        }
        (int,int) levelInterval = dtBGID.GetLevelInterval(themeID);

        Medal_Obj.gameObject.SetActive(isAllUnlock);
        gameObject.name = themeID.ToString();

        int index = (themeID - 1) % BGSprites.Length;
        index = Mathf.Max(index, 0);
        BG_Image.sprite = BGSprites[index];
        ThemeName_Text.SetTerm($"General.{bgList[0].ThemeName}");
        ThemeName_Text.SetMaterialPreset(TextMaterial[index]);
        
        if (levelInterval.Item1 <= 0)
        {
            LevelNum_Text.gameObject.SetActive(false);
        }
        else
        {
            LevelNum_Text.gameObject.SetActive(true);
            LevelNum_Text.SetMaterialPreset(TextMaterial[index]);
            LevelNum_Text.SetParameterValue("{0}", levelInterval.Item1.ToString());
            LevelNum_Text.SetParameterValue("{1}", levelInterval.Item2.ToString());
        }

        UnityUtility.FillGameObjectWithFirstChild<BGImageCell>(BGParent.gameObject,bgList.Count,(index,comp)=> 
        {
            comp.Init(bgList[index].ID,RefreshEvent);
        });
    }

    private void RefreshEvent()
    {
        var dtBGID = GameManager.DataTable.GetDataTable<DTBGID>().Data;
        bgList = dtBGID.BGThemeDict[themeID];
        bool isAllUnlock = true;
        foreach (var bgData in bgList)
        {
            if (bgData.BGPrice > 0 && !GameManager.PlayerData.IsOwnBGID(bgData.ID))
            {
                isAllUnlock = false;
                break;
            }
        }
        Medal_Obj.gameObject.SetActive(isAllUnlock);
    }
}
