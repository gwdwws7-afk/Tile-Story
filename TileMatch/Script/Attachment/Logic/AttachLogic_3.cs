using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttachLogic_3 : AttachLogic
{
    public override int AttachId => 3;

    public override string AttachAssetName => "Gold";

    public override void OnClick()
    {
        base.OnClick();

        TileMatchPanel tileMatchPanel = (TileMatchPanel)GameManager.UI.GetUIForm("TileMatchPanel");
        if (tileMatchPanel.CheckIsLose())
            return;

        if (attachState >= 5)
            return;

        //Debug.LogError("金块收集");
        //GameManager.PlayerData.AddItemNum(TotalItemData.Tile_Gold, 1);
        int count = GameManager.DataNode.GetData("GoldTileCurrentCount", 0);
        count++;
        GameManager.DataNode.SetData("GoldTileCurrentCount", count);
        //飞金块特效
        ShowCollectEffect();
    }

    public override void SpecialCollect(bool showEffect, bool clearId = true)
    {
        OnClick();
        base.SpecialCollect(showEffect, clearId);
    }

    public override void OnAnyTileGet()
    {
        if ((TileItem != null && TileItem.IsBeCover) || attachState >= 5)
            return;

        //Debug.LogError("金块剥落");
        attachState++;
        AttachItem.SetSprite(attachState);
        ShowBrokenEffect();
    }

    private void ShowCollectEffect()
    {
        TileMatchPanel tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        GameGoldBar goldBar = tileMatchPanel.Gold_Bar.GetComponent<GameGoldBar>();

        Transform cachedTrans = TileItem.transform;
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "GoldFlyObject",
            "GoldFlyObjectPool",
            5f,
            cachedTrans.position,
            Quaternion.identity,
            cachedTrans.parent,
            (obj) =>
            {
                GameObject flyObject = (GameObject)obj.Target;
                flyObject.SetActive(true);
                GoldFlyObject goldFlyObject = flyObject.GetComponent<GoldFlyObject>();
                goldFlyObject.OnInit();
                goldFlyObject.OnShow(goldBar.iconImage.position, () =>
                {
                    goldBar.RefreshGoldText();
                    goldBar.PunchGoldBar();
                });
            });

        if (GameManager.Sound != null)
            GameManager.Sound.PlayAudio("SFX_Butterfly_Tile_Collect");
    }

    private void ShowBrokenEffect()
    {
        Transform cachedTrans = TileItem.transform;
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "Effect_Attachment_golden_broken",
            "TileItemDestroyEffectPool",
            1f,
            cachedTrans.position,
            cachedTrans.rotation,
            cachedTrans.parent,
            null);

        if (GameManager.Sound != null)
            GameManager.Sound.PlayAudio("SFX_Butterfly_Tile_Break");
    }
}
