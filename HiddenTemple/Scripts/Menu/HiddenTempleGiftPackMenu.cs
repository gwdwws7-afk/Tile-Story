using System;
using TMPro;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// ÒÅ¼£Ñ°±¦ÄÚ¹ºÀñ°ü
    /// </summary>
    public sealed class HiddenTempleGiftPackMenu : PopupMenuForm
    {
        public Button m_CloseButton, m_BuyButton;
        public TextMeshProUGUI m_PriceText;
        public ClockBar m_ClockBar;

        private ProductNameType m_ProductType = ProductNameType.Temple_Package;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_BuyButton.SetBtnEvent(OnBuyButtonClick);

            m_ClockBar.OnReset();
            if (HiddenTempleManager.Instance.CheckActivityHasStarted())
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(GameManager.Activity.GetCurActivityEndTime());
            }
            else
            {
                m_ClockBar.SetFinishState();
            }

            string price = GameManager.Purchase.GetPrice(m_ProductType);
            if (!string.IsNullOrEmpty(price))
            {
                m_PriceText.text = price;
            }
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_BuyButton.onClick.RemoveAllListeners();

            base.OnReset();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Sound.PlayAudio(SoundType.SFX_UI_close.ToString());
        }

        private void OnBuyButtonClick()
        {
            GameManager.Purchase.BuyProduct(m_ProductType, () =>
            {
                try
                {
                    GameManager.UI.HideUIForm("HiddenTempleAccessMenu");
                    GameManager.UI.HideUIForm("HiddenTempleGiftPackMenu");
                }
                catch(Exception e)
                {
                    Log.Error(e.ToString());
                }
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    GameManager.Event.Fire(this, PickaxeNumChangeEventArgs.Create());

                    GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Pickaxes_Purchase);
                });
            });
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_ClockBar.SetFinishState();
        }
    }
}
