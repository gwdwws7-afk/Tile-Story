using DG.Tweening;
using System;
using UnityEngine;

namespace HiddenTemple
{
    /// <summary>
    /// ÍÚ¾òÇøÓòµÄ±¦Ê¯
    /// </summary>
    public sealed class DigGem : MonoBehaviour
    {
        public Transform m_Body;
        public GameObject m_DigGemImage;
        public GameObject m_FlyGemImage;

        private int m_Id;
        private int m_Row;
        private int m_Col;
        private DRGemData m_Data;
        private bool m_IsDigOut;
        private int m_DelayTaskId_1;
        private int m_DelayTaskId_2;

        public int Id => m_Id;
        public int Row => m_Row;
        public int Col => m_Col;
        public DRGemData Data => m_Data;
        public bool IsDigOut
        {
            get => m_IsDigOut;
            set => m_IsDigOut = value;
        }

        public void Initialize(int row, int col, DRGemData data, bool isDigOut)
        {
            m_Id = data.Id;
            m_Row = row;
            m_Col = col;
            m_Data = data;
            m_IsDigOut = isDigOut;

            m_DigGemImage.SetActive(!isDigOut);
            m_FlyGemImage.SetActive(isDigOut);
            if (isDigOut)
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            else
                transform.localScale = Vector3.one;
        }

        public void Release()
        {
            m_Body.DOKill();
            if (m_DelayTaskId_1 > 0) GameManager.Task.RemoveDelayTriggerTask(m_DelayTaskId_1);
            if (m_DelayTaskId_2 > 0) GameManager.Task.RemoveDelayTriggerTask(m_DelayTaskId_2);
        }

        public void ShowDigOutAnim(Vector3 targetPos, Action callback = null)
        {
            m_DigGemImage.SetActive(false);
            m_FlyGemImage.SetActive(true);

            float scale = 2f;
            m_Body.DOScale(scale, 0.16f).SetEase(Ease.OutCubic).onComplete = () =>
            {
                m_Body.DOScale(scale * 0.75f, 0.12f).SetEase(Ease.InOutQuad).onComplete = () =>
                {
                    m_Body.DOScale(scale * 0.85f, 0.1f).SetEase(Ease.InOutQuad);
                };
            };

            m_DelayTaskId_1 = GameManager.Task.AddDelayTriggerTask(0.4f, () =>
              {
                  if (m_Row <= 4)
                      transform.DOJump(targetPos, -0.6f, 1, 0.6f);
                  else
                      transform.DOJump(targetPos, -0.7f, 1, 0.6f);
                  transform.DOScale(0.6f / scale, 0.6f / scale).SetDelay(0.2f);
              });

            m_DelayTaskId_2 = GameManager.Task.AddDelayTriggerTask(1f, () =>
              {
                  callback?.Invoke();
              });

            GameManager.Sound.PlayAudio("SFX_Temple_Gem_Show_Into_Door");
        }
    }
}
