using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    public sealed class HiddenTempleItemSlot : ItemSlot
    {
        public Image itemImage;
        public TextMeshProUGUI itemNumText;
        public GameObject showEffect;

        private string currentRewardSpriteKey = null;

        public override void OnInit(TotalItemData type, int num)
        {
            base.OnInit(type, num);

            string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);
            if (rewardSpriteKey != currentRewardSpriteKey)
            {
                currentRewardSpriteKey = rewardSpriteKey;
                ClearAsyncHandleList();
                var asyncHandle = UnityUtility.LoadSpriteAsync(rewardSpriteKey, "TotalItemAtlas", sp =>
                {
                    itemImage.sprite = sp;
                    itemImage.SetNativeSize();
                });
                AddAsyncHandle(asyncHandle);
            }

            SetItemText(num, type, true);

            if (type == TotalItemData.Life || type == TotalItemData.InfiniteLifeTime || type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost)
                itemImage.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            else
                itemImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }

        public override void OnReset()
        {
            currentRewardSpriteKey = null;

            base.OnReset();
        }

        public void Show()
        {
            showEffect.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            showEffect.SetActive(false);
            gameObject.SetActive(false);
        }

        private void SetItemText(int num, TotalItemData type, bool tight)
        {
            if (type == TotalItemData.Coin)
            {
                itemNumText.transform.localPosition = new Vector3(0, itemNumText.transform.localPosition.y, 0);
                itemNumText.SetText(num.ToString());
            }
            else if (type == TotalItemData.InfiniteLifeTime || type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost)
            {
                if (num < 60)
                    itemNumText.SetText(num.ToString() + "m");
                else
                    itemNumText.SetText((num / 60).ToString() + "h");
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
}
