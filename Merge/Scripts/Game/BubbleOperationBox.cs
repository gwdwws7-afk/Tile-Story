using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class BubbleOperationBox : MonoBehaviour
    {
        public Transform m_Body;
        public RectTransform m_Box;
        //public Transform m_Root;
        public Button m_PunctureButton;
        public Button m_AdsBuyButton;
        public Button m_CoinBuyButton;
        public TextMeshProUGUI m_CoinCostText;

        private PropLogic m_Prop;
        private int bubbleCostCoinNum = 0;

        public PropLogic Prop => m_Prop;

        public void Initialize(int propId, PropLogic prop, bool isShowAdsButton)
        {
            m_Prop = prop;

            IDataTable<DRMergeGenerateBubble> dataTable = MergeManager.DataTable.GetDataTable<DRMergeGenerateBubble>(MergeManager.Instance.GetMergeDataTableName());
            var allData = dataTable.GetAllDataRows();
            foreach (var targetData in allData)
            {
                if (targetData.GenerateBubble == propId)
                {
                    bubbleCostCoinNum = targetData.BubbleCostCoinNum;
                    m_CoinCostText.text = bubbleCostCoinNum.ToString();
                    break;
                }
            }

            m_PunctureButton.onClick.RemoveAllListeners();
            m_PunctureButton.onClick.AddListener(OnPunctureButtonClick);
            m_PunctureButton.interactable = true;

            m_AdsBuyButton.onClick.RemoveAllListeners();
            m_AdsBuyButton.onClick.AddListener(OnAdsBuyButtonClick);
            m_AdsBuyButton.interactable = true;

            m_CoinBuyButton.onClick.RemoveAllListeners();
            m_CoinBuyButton.onClick.AddListener(OnCoinBuyButtonClick);
            m_CoinBuyButton.interactable = true;

            m_AdsBuyButton.gameObject.SetActive(isShowAdsButton);
        }

        public void Show(Vector3 position, float offsetX = 0f)
        {
            Hide();

            if (MergeManager.Merge.SelectedProp != null && MergeManager.Merge.SelectedProp.AttachmentId == 1 && MergeManager.Merge.SelectedProp.Square != null)
            {
                transform.position = new Vector3(position.x, position.y, -1);
                m_Body.DOKill();
                m_Body.localScale = Vector3.zero;
                m_Box.localPosition = new Vector3(offsetX, m_Box.localPosition.y, 0);
                //m_Root.localPosition = new Vector3(offsetX, m_Root.localPosition.y, 0);
                gameObject.SetActive(true);
                m_Body.DOScale(1.1f, 0.15f).onComplete = () =>
                {
                    m_Body.DOScale(1, 0.15f);
                };
            }
        }

        public virtual void Hide()
        {
            m_Prop = null;
            m_Body.DOKill();
            gameObject.SetActive(false);
        }

        private void OnPunctureButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeConfirmMenu"), userData: true);
        }

        private void OnAdsBuyButtonClick()
        {
            if (MergeManager.Merge.SelectedProp != null && MergeManager.Merge.SelectedProp.AttachmentId == 1 && MergeManager.Merge.SelectedProp.Square != null)
            {
                if (GameManager.Ads.CheckRewardedAdIsLoaded())
                {
                    var bubbleLogic = MergeManager.Merge.SelectedProp.AttachmentLogic as BubbleLogic;
                    if (bubbleLogic != null)
                    {
                        bubbleLogic.TimePause = true;
                    }

                    Hide();
                    GameManager.Ads.ShowRewardedAd("MergeBubble");
                }
                else
                {
                    GameManager.UI.ShowWeakHint("Common.Ad is still loading...");
                }
            }
        }

        private void OnCoinBuyButtonClick()
        {
            if (bubbleCostCoinNum <= 0)
            {
                Hide();
                return;
            }

            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeConfirmMenu"), userData: false);
        }
    }
}
