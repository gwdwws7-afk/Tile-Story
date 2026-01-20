using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.AddressableAssets;

public class HeadPortraitPanel : PopupMenuForm
{
    [SerializeField]
    private DelayButton Close_Btn;
    [SerializeField]
    private GameObject HeadPortrait_Parent;

    int firstHeadIndex = 0;
    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        SetBtnEvent();
        ShowHeadPortraits();

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

	public override void OnRelease()
	{
        UnloadSprite();

        base.OnRelease();
	}

	private void SetBtnEvent()
    {
        Close_Btn.SetBtnEvent(()=> 
        {
            if (firstHeadIndex != GameManager.PlayerData.HeadPortrait)
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Edit_Avatar_Count);
            }
            GameManager.UI.HideUIForm(this);
        });
    }
    private void ShowHeadPortraits()
    {
        int headPortrait = GameManager.PlayerData.HeadPortrait;
        firstHeadIndex = GameManager.PlayerData.HeadPortrait;
        UnityUtility.FillGameObjectWithFirstChild<UICommonController>(HeadPortrait_Parent, 10, (index, comp) => 
        {
            bool isCurHead = (headPortrait == index);
            int curHeadPortrait = index;

            comp.Toggle.isOn = isCurHead;
            comp.Toggle.onValueChanged.RemoveAllListeners();
            comp.Toggle.onValueChanged.AddListener((b) =>
            {
                GameManager.PlayerData.HeadPortrait = curHeadPortrait;

                GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Change_Avatar, 1);

                if (b)
                {
                    GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
                }
            });

            if (index == 0)
            {
                comp.gameObject.SetActive(false);
                return;
            }

            SetHeadPortrait(comp.Images[0],index,false);
            SetHeadPortrait(comp.Images[1], index, true);
        });
    }

    Dictionary<int, Sprite> defaultSprites = new Dictionary<int, Sprite>();
    Dictionary<int, Sprite> chooseSprites = new Dictionary<int, Sprite>();

    void SetHeadPortrait(Image image,int index, bool isChoose)
    {
        if (isChoose)
        {
            string name = $"HeadPortrait_{index}_{index}";
            if (!chooseSprites.TryGetValue(index, out Sprite sprite))
            {
                UnityUtility.LoadAssetAsync<Sprite>(name, (s) =>
                 {
                     image.sprite =s;
                     chooseSprites[index] = s;
                 });
            }
        }
        else
        {
            string name = $"HeadPortrait_{index}";
            if (!defaultSprites.TryGetValue(index, out Sprite sprite))
            {
                UnityUtility.LoadAssetAsync<Sprite>(name, (s) =>
                {
                    image.sprite = s;
                    defaultSprites[index] = s;
                });
            }
        }
    }

    void UnloadSprite()
    {
        foreach (var item in defaultSprites)
        {
            Addressables.Release<Sprite>(item.Value);
        }
        foreach (var item in chooseSprites)
        {
            Addressables.Release<Sprite>(item.Value);
        }
        defaultSprites.Clear();
        chooseSprites.Clear();
    }
}
