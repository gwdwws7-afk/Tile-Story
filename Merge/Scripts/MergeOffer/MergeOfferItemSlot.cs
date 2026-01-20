using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeOfferItemSlot : ItemSlot
    {
        public Image itemImage;
        public TextMeshProUGUI itemNumText;

        private string m_CurrentRewardSpriteKey = null;

        public override void OnInit(TotalItemData type, int num)
        {
            base.OnInit(type, num);

            string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);
            if (rewardSpriteKey != m_CurrentRewardSpriteKey)
            {
                m_CurrentRewardSpriteKey = rewardSpriteKey;
                ClearAsyncHandleList();
                var asyncHandle = UnityUtility.LoadGeneralSpriteAsync(rewardSpriteKey, sp =>
                {
                    itemImage.sprite = sp;
                    itemImage.SetNativeSize();
                });
                AddAsyncHandle(asyncHandle);
            }

            SetItemText(num, type, true);

            if (type == TotalItemData.MergeEnergyBox)
                itemImage.transform.localScale = new Vector3(1f, 1f, 1f);
            else if (type == TotalItemData.InfiniteLifeTime || type == TotalItemData.Life || type == TotalItemData.Coin)
                itemImage.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            else if (type == TotalItemData.FireworkBoost)
                itemImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            else
                itemImage.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        }

        public override void OnReset()
        {
            m_CurrentRewardSpriteKey = null;

            base.OnReset();
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
