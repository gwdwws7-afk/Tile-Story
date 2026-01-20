using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// �ż�Ѱ�����ӻ�ȡ����
    /// </summary>
    public sealed class HiddenTempleAccessMenu : PopupMenuForm
    {
        public Button m_BeatLevelButton, m_PackBuyButton, m_CloseButton;
        public TextMeshProUGUI m_PriceText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_BeatLevelButton.SetBtnEvent(OnBeatLevelButtonClick);
            m_PackBuyButton.SetBtnEvent(OnPackBuyButtonClick);
            m_CloseButton.SetBtnEvent(OnCloseButtonClick);

            string price = GameManager.Purchase.GetPrice(ProductNameType.Temple_Package);
            if (!string.IsNullOrEmpty(price))
            {
                m_PriceText.text = price;
            }
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override void OnRelease()
        {
            OnReset();

            base.OnRelease();
        }

        private void OnBeatLevelButtonClick()
        {
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }

        private void OnPackBuyButtonClick()
        {
            GameManager.Purchase.BuyProduct(ProductNameType.Temple_Package, () =>
            {
                GameManager.UI.HideUIForm("HiddenTempleAccessMenu");

                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    GameManager.Event.Fire(this, PickaxeNumChangeEventArgs.Create());

                    GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Pickaxes_Purchase);
                });
            });
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Sound.PlayAudio(SoundType.SFX_UI_close.ToString());
        }
    }
}
