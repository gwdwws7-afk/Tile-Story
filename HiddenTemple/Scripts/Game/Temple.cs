using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 神庙
    /// </summary>
    public sealed class Temple : MonoBehaviour
    {
        public Transform m_Body;
        public Transform m_Main;
        public Transform m_Door;
        public Transform m_ChestRoot;
        public Transform m_Chest;
        public CanvasGroup m_Black;
        public Image m_DoorBlack;
        public SkeletonGraphic m_DoorOpenEffect;
        public TempleGemSlot[] m_GemSlots;

        private int m_Stage;
        private TempleArea m_TempleBoard;

        public int Stage => m_Stage;

        public void Initialize(int stage, bool isCurTemple, TempleArea templeBoard)
        {
            m_Stage = stage;
            m_TempleBoard = templeBoard;

            if (m_Door != null)
                m_Door.SetAsLastSibling();
            foreach (TempleGemSlot slot in m_GemSlots)
            {
                slot.ClearFilledGem();
            }

            if (isCurTemple)
            {
                float mainOriginalScale = 1f;
                float chestOriginalScale = 1f;
                if (m_Stage > 1 && m_Stage <= HiddenTempleManager.PlayerData.GetMaxStage()) 
                {
                    m_Main.localScale = new Vector3(mainOriginalScale, mainOriginalScale, mainOriginalScale);
                    m_ChestRoot.gameObject.SetActive(false);
                    m_Black.alpha = 0;
                }
                else if (m_Stage > HiddenTempleManager.PlayerData.GetMaxStage()) 
                {
                    m_Main.localScale = new Vector3(mainOriginalScale * 0.5f, mainOriginalScale * 0.5f, mainOriginalScale * 0.5f);
                    m_ChestRoot.localScale = new Vector3(chestOriginalScale, chestOriginalScale, chestOriginalScale);
                    m_Chest.gameObject.SetActive(false);

                    var images = m_ChestRoot.GetComponentsInChildren<Image>();
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].color = Color.white;
                    }
                    m_Black.alpha = 1;
                }
            }
        }

        public void Release()
        {
            foreach (TempleGemSlot slot in m_GemSlots)
            {
                slot.ClearFilledGem();
            }
            m_TempleBoard = null;

            if (m_Body != null)
                m_Body.DOKill();

            if (m_Main != null)
                m_Main.DOKill();

            if (m_Door != null)
                m_Door.DOKill();

            if (m_ChestRoot != null)
                m_ChestRoot.DOKill();

            if (m_Chest != null)
                m_Chest.DOKill();

            if (m_Black != null)
                m_Black.DOKill();

            if (m_DoorBlack != null)
                m_DoorBlack.DOKill();
        }

        public bool IsAllGemSlotFilled()
        {
            foreach (TempleGemSlot slot in m_GemSlots)
            {
                if (!slot.IsFilled)
                    return false;
            }

            return true;
        }

        public TempleGemSlot GetTempleGemSlot(int gemId)
        {
            foreach (TempleGemSlot slot in m_GemSlots)
            {
                if (slot.GemId == gemId && !slot.IsFilled) 
                {
                    return slot;
                }
            }

            return null;
        }

        public void ShowTempleDoorOpenAnim(Action callback)
        {
            m_DoorOpenEffect.Initialize(false);
            m_DoorOpenEffect.AnimationState.SetAnimation(0, "open", false);
            m_DoorOpenEffect.gameObject.SetActive(true);

            m_Body.DOShakePosition(0.5f, new Vector3(0, -4, 0), 10, 0, false, true, ShakeRandomnessMode.Harmonic);

            GameManager.Task.AddDelayTriggerTask(0.74f, () =>
            {
                float shrinkDoorScale = m_Stage == 1 ? 0.92f : 0.9f;//第一扇门不知道为何尺寸大
                m_Door.DOShakePosition(0.7f, new Vector3(0, -2, 0), 30, 0, false, true, ShakeRandomnessMode.Harmonic);
                m_Door.DOScale(shrinkDoorScale, 0.7f).SetEase(Ease.Linear).onComplete = () =>
                {
                    m_Door.DOShakePosition(0.2f, new Vector3(0, 3, 0), 10, 0, false, true, ShakeRandomnessMode.Harmonic);
                    m_Body.DOShakePosition(0.2f, new Vector3(0, 2, 0), 10, 0, false, true, ShakeRandomnessMode.Harmonic);
                };

                m_DoorBlack.DOFade(0.25f, 0.5f).SetDelay(0.2f);
            });

            GameManager.Task.AddDelayTriggerTask(1.8f, () =>
            {
                m_Door.SetAsFirstSibling();
                m_Door.DOLocalMoveX(500, 2.3f).SetEase(Ease.InOutQuad);
                m_Door.DOLocalRotate(new Vector3(0, 0, -90), 2.3f).SetEase(Ease.InOutQuad);

                GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                {
                    m_Body.DOScale(0.98f, 0.3f).onComplete = () =>
                    {
                        //第一扇门不知道为何尺寸大
                        if (m_Stage == 1) 
                            m_Body.DOScale(3.8f, 1.5f);
                        else
                            m_Body.DOScale(4f, 1.5f);
                    };

                    callback.Invoke();
                });
            });

            GameManager.Sound.PlayAudio("SFX_Temple_Door_Open");
        }

        public void ShowTempleChestShowAnim()
        {
            float mainOriginalScale = 1f;
            float chestOriginalScale = 1f;

            float mainStartScale = mainOriginalScale * 0.3f;
            float chestStartScale = chestOriginalScale * 0.5f;
            m_Main.transform.localScale = new Vector3(mainStartScale, mainStartScale, 1);
            m_ChestRoot.transform.localScale = new Vector3(chestStartScale, chestStartScale, 1);
            m_Main.transform.localPosition = Vector3.zero;
            m_ChestRoot.transform.localPosition = new Vector3(0, -25, 0);

            m_Main.DOScale(mainStartScale * 0.98f, 0.3f);
            m_ChestRoot.DOScale(chestStartScale * 0.98f, 0.3f).onComplete = () =>
            {
                m_Main.DOScale(mainOriginalScale * 0.5f, 1.5f);
                m_ChestRoot.DOScale(chestOriginalScale, 1.5f);

                var images = m_ChestRoot.GetComponentsInChildren<Image>();
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].DOColor(Color.white, 1.3f).SetDelay(0.2f);
                }

                Image blackImg = m_Black.GetComponent<Image>();
                float startValue = 3.5f;
                float endValue = 4.3f;
                DOTween.To(() => startValue, t => startValue = t, endValue, 1.3f).SetDelay(0.2f).onUpdate = () =>
                {
                    blackImg.material.SetFloat("_FadeDistance", startValue);
                };
                m_Black.alpha = 1;
            };

            //宝箱跳入进度条动画
            GameManager.Task.AddDelayTriggerTask(1.9f, () =>
            {
                float jumpPower = 0.4f;
                if (m_Stage == 1 || m_Stage == 5) 
                    jumpPower = 0.5f;

                Vector3 pos = m_TempleBoard.Menu.ChestArea.GetChestSlotPos(m_Stage - 1);
                m_Chest.SetParent(m_TempleBoard.m_ChestFlyRoot);
                m_Chest.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    m_Chest.DOScale(0.6f, 0.3f).SetEase(Ease.InQuad);
                };
                m_Chest.DOJump(pos, jumpPower, 1, 0.5f).SetEase(Ease.InQuad).onComplete = () =>
                {
                    m_Chest.gameObject.SetActive(false);

                    m_TempleBoard.OnChestJumpIn(m_Stage - 1);
                };

                GameManager.Sound.PlayAudio("SFX_Temple_Reward_Show_Next_Door");
            });

            //显示下一道门
            if (m_Stage <= HiddenTempleManager.PlayerData.GetMaxStage())
            {
                GameManager.Task.AddDelayTriggerTask(2.6f, () =>
                {
                    m_Main.DOScale(mainOriginalScale * 0.5f * 0.99f, 0.2f);
                    m_ChestRoot.DOScale(chestOriginalScale * 0.99f, 0.2f).onComplete = () =>
                    {
                        m_Main.DOScale(mainOriginalScale * 1.01f, 1.5f).onComplete = () =>
                        {
                            m_Main.DOScale(mainOriginalScale, 0.3f);
                        };
                        m_ChestRoot.DOScale(chestOriginalScale * 2f, 1.5f);
                        m_ChestRoot.DOLocalMoveY(-100f, 1.5f);
                        m_Black.DOFade(0, 1.2f).SetDelay(0.3f);
                    };

                    m_TempleBoard.OnShowNextTemple();
                });
            }
            else
            {
                m_TempleBoard.Menu.DigArea.ShowCompleteAllBoard();
            }
        }
    }
}
