using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TilePassRewardSlot : ItemSlot
{
    public Image rewardImage;
    public TextMeshProUGUI rewardNum;

    private string m_CurrentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);
        if (rewardSpriteKey != m_CurrentRewardSpriteKey)
        {
            m_CurrentRewardSpriteKey = rewardSpriteKey;
            ClearAsyncHandleList();
            AsyncOperationHandle asyncHandle = UnityUtility.LoadSpriteAsync(rewardSpriteKey, "TotalItemAtlas", sp =>
            {
                rewardImage.sprite = sp;
            });
            AddAsyncHandle(asyncHandle);
        }
        rewardNum.SetItemText(num, type, false);

        rewardNum.gameObject.SetActive(true);
        rewardImage.gameObject.SetActive(true);
    }

    public override void OnRelease()
    {
        m_CurrentRewardSpriteKey = null;

        base.OnRelease();
    }
}
