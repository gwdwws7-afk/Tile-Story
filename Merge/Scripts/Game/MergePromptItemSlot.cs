using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergePromptItemSlot : ItemSlot
{
    public Image itemImage;
    public TextMeshProUGUI itemNumText;
    public bool isShowSlotTight;

    private string currentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);

        if (rewardSpriteKey != currentRewardSpriteKey)
        {
            currentRewardSpriteKey = rewardSpriteKey;
            ClearAsyncHandleList();
            var asyncHandle = UnityUtility.LoadGeneralSpriteAsync(rewardSpriteKey, sp =>
            {
                itemImage.sprite = sp;
                if (type.TotalItemType == TotalItemType.Item_BgID || type.TotalItemType == TotalItemType.Item_TileID)
                    itemImage.rectTransform.sizeDelta = new Vector2(350, 350);
                else
                    itemImage.SetNativeSize();
            });
            AddAsyncHandle(asyncHandle);
        }

        SetItemText(num, type, true);

        if (isShowSlotTight)
        {
            itemImage.transform.localPosition = new Vector3(0, 10.5f, 0);
            itemNumText.transform.localPosition = new Vector3(0, -30.4f, 0);
        }
        else
        {
            itemImage.transform.localPosition = new Vector3(0, 27.5f, 0);
            itemNumText.transform.localPosition = new Vector3(0, -45.4f, 0);
        }

        if (type == TotalItemData.Star)
            itemImage.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        //else if(type==TotalItemData.Coin)
        //    itemImage.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        else
            itemImage.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
    }

    private void SetItemText(int num, TotalItemData type, bool tight)
    {
        if (type == TotalItemData.Coin)
        {
            itemNumText.SetText(num.ToString());
        }
        else if (type == TotalItemData.InfiniteLifeTime || type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost)
        {
            if (num < 60)
                itemNumText.SetText(num.ToString() + "m");
            else
                itemNumText.SetText((num / 60f).ToString() + "h");
        }
        else
        {
            if (tight)
            {
                itemNumText.SetText("x" + num.ToString());
            }
            else
            {
                itemNumText.SetText("x " + num.ToString());
            }
        }
    }
}
