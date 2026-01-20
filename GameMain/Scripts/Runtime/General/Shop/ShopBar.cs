using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商品栏基类
/// </summary>
public abstract class ShopBar : MonoBehaviour
{
    public Transform root;
    public DelayButton buyButton;
    public TextMeshProUGUILocalize buyText;

    protected ProductNameType m_ProductNameType;
    protected List<ItemData> m_ItemDatas;

    public abstract string BarName { get; }

    public bool IsAvailable { get => m_ItemDatas != null; }

    public virtual void OnInit(ShopPackageData shopPackageData)
    {
        m_ProductNameType = shopPackageData.Type;
        m_ItemDatas = shopPackageData.GetItemDatas();
        buyButton.onClick.AddListener(OnBuyButtonClick);

        string price = GameManager.Purchase.GetPrice(m_ProductNameType);
        if (!string.IsNullOrEmpty(price))
        {
            buyText.SetTerm(price);
        }
    }

    public virtual void OnReset()
    {
        m_ItemDatas = null;
        buyButton.onClick.RemoveAllListeners();
    }

    public virtual void OnRelease()
    {

    }

    public virtual void OnShow(float delayTime, bool skipAnim)
    {
        gameObject.SetActive(true);

        if (!skipAnim)
        {
            root.localPosition = new Vector3(1010, root.localPosition.y);
            root.DOLocalMoveX(-40f, 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                root.DOLocalMoveX(0, 0.2f);
            };
        }
        else
        {
            root.localPosition = new Vector3(0, root.localPosition.y);
        }
    }

    public virtual void OnHide()
    {
        root.DOKill();
        root.position = new Vector3(0, root.position.y, 0);
        gameObject.SetActive(false);
    }

    public virtual void OnBuyButtonClick()
    {
        GameManager.Purchase.BuyProduct(m_ProductNameType, () =>
        {
            ShopMenuManager.RecordBuySuccess = true;
            GameManager.UI.HideUIForm("ShopMenuManager");

            //if (productNameType == ProductNameType.Remove_ads || productNameType == ProductNameType.Remove_ads_discount)
            //{
            //    GameManager.UI.ShowUIForm<RemovePopupAdsBuySuccessMenu>();
            //}
            //else
            //{
            //    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false,null);
            //}
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ShopBuyGetRewardComplete));
            });
        });
    }
}
