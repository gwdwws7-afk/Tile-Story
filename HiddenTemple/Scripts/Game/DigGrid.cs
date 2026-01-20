using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 挖掘区域的格子
    /// </summary>
    public class DigGrid : MonoBehaviour
    {
        public GameObject m_Cover;
        public GameObject m_BrokenCover;
        public Button m_Button;

        protected int m_Row;
        protected int m_Col;
        protected int m_CanDigTime;
        protected HiddenTempleBaseMenu m_Menu;

        public bool IsDigged { get; private set; }

        public int CanDigTime => m_CanDigTime;

        public virtual void Initialize(int row, int col, int canDigTime, HiddenTempleBaseMenu menu)
        {
            m_Row = row;
            m_Col = col;
            m_CanDigTime = canDigTime;
            m_Menu = menu;

            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(OnButtonClick);
            m_Button.interactable = true;

            m_BrokenCover.SetActive(m_CanDigTime == 1);
        }

        public void Release()
        {
            m_Button.onClick.RemoveAllListeners();
        }

        public void ShowDestroyCoverAnim(bool isForce)
        {
            void Callback()
            {
                if (!isForce)
                {
                    m_Cover.SetActive(m_CanDigTime == 1);
                    m_BrokenCover.SetActive(m_CanDigTime == 1);
                }
                else
                {
                    m_Cover.SetActive(false);
                    m_BrokenCover.SetActive(false);
                }

                ((HiddenTempleMainMenu)m_Menu).DigArea.ShowGridBrokenEffect(transform.position);
            }

            if (!isForce)
            {
                GameManager.Task.AddDelayTriggerTask(0.267f, () =>
                {
                    Callback();
                    GameManager.Sound.PlayAudio("SFX_Temple_Shovel_Dig");
                });
            }
            else
            {
                Callback();
            }
        }

        protected virtual void OnButtonClick()
        {
            if (IsDigged)
                return;

            HiddenTempleMainMenu mainMenu = m_Menu as HiddenTempleMainMenu;
            if (HiddenTempleManager.PlayerData.GetPickaxeNum() > 0)
            {
                HiddenTempleManager.PlayerData.SubtractPickaxeNum(1);
                GameManager.Event.Fire(this, PickaxeNumChangeEventArgs.Create());

                if (mainMenu.GuidePanel.activeSelf)
                {
                    mainMenu.GuidePanel.SetActive(false);
                    mainMenu.ChestArea.ShowCurChestItemPromptBox();
                }

                if (m_CanDigTime == 2)
                {
                    m_CanDigTime--;
                }
                else
                {
                    m_CanDigTime = 0;
                    IsDigged = true;
                }

                mainMenu.DigArea.OnDigGrid(transform.position);

                ShowDestroyCoverAnim(false);
            }
            else
            {
                if (!mainMenu.IsFinished) 
                    GameManager.UI.ShowUIForm("HiddenTempleAccessMenu");
                else
                    GameManager.UI.ShowUIForm("HiddenTempleGiftPackMenu");

                GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
            }
        }
    }
}
