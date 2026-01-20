using System;
using UnityEngine;

namespace Merge
{
    public class BubbleLogic : PropAttachmentLogic
    {
        private const int TimeUpConvertToPropId = 20101;
        private const float BubbleBreakMinutes = 5f;

        private bool m_IsChangeScale;
        private DateTime m_BubbleBreakTime;
        private bool m_IsTimeUp;

        public bool TimePause
        {
            get;
            set;
        }

        public override void OnInitialize(PropLogic propLogic)
        {
            m_IsTimeUp = false;
            TimePause = false;
            propLogic.IsSilenced = true;

            //获取气泡爆破时间
            if (propLogic.SerialId != 0)
            {
                if (!GetDateTime("BubbleBreakTime_" + propLogic.SerialId, out m_BubbleBreakTime))
                {
                    m_BubbleBreakTime = DateTime.Now.AddMinutes(BubbleBreakMinutes);
                    SetDateTime("BubbleBreakTime_" + propLogic.SerialId, m_BubbleBreakTime);
                }

                if (DateTime.Now > m_BubbleBreakTime)
                    m_BubbleBreakTime = DateTime.Now.AddSeconds(1);
            }
            else
            {
                m_BubbleBreakTime = DateTime.Now.AddSeconds(1);
            }

            if (propLogic.Prop != null)
            {
                propLogic.Prop.BodyScale = propLogic.Prop.BodyScale * 0.8f;
                propLogic.Prop.transform.localScale = new Vector3(propLogic.Prop.BodyScale, propLogic.Prop.BodyScale, propLogic.Prop.BodyScale);

                m_IsChangeScale = true;
            }
            else
            {
                propLogic.SpawnPropComplete += p =>
                {
                    propLogic.Prop.BodyScale = propLogic.Prop.BodyScale * 0.8f;
                    propLogic.Prop.transform.localScale = new Vector3(propLogic.Prop.BodyScale, propLogic.Prop.BodyScale, propLogic.Prop.BodyScale);

                    m_IsChangeScale = true;
                };
            }
        }

        public override void OnRelease(PropLogic propLogic, bool isShutdown)
        {
            if (!isShutdown)
            {
                m_IsTimeUp = true;
                propLogic.IsSilenced = false;

                if (m_IsChangeScale && propLogic.Prop != null)
                {
                    m_IsChangeScale = false;
                    propLogic.Prop.BodyScale = 1f;
                    propLogic.Prop.transform.localScale = Vector3.one;
                }

                //TODO:显示破坏特效
            }
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            if (!m_IsTimeUp && !TimePause)  
            {
                if (Attachment != null)
                {
                    var timeSpan = m_BubbleBreakTime - DateTime.Now;
                    ((BubbleAttachment)Attachment).m_TimeText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
                }

                //倒计时结束，破坏道具生成金币堆
                if (DateTime.Now > m_BubbleBreakTime)
                {
                    m_IsTimeUp = true;
                    if (PropLogic != null)
                    {
                        void CovertToCoin()
                        {
                            PropLogic propLogic = PropLogic;
                            int propId = propLogic.PropId;
                            Square square = propLogic.Square;
                            GameManager.Task.AddDelayTriggerTask(0.01f, () =>
                            {
                                MergeManager.Merge.ReplaceProp(TimeUpConvertToPropId, propLogic, square, p =>
                                {
                                    p?.ShowPropGenerateEffect();
                                });
                            });
                            GameManager.Sound.PlayAudio(SoundType.SFX_Pop_Element_Break.ToString());

                            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                            if (mainBoard != null && mainBoard.m_BubbleOperationBox.Prop == PropLogic)
                                mainBoard.HideBubbleOperationBox();
                        }

                        if (PropLogic.MovementState == PropMovementState.Static)
                        {
                            CovertToCoin();
                        }
                        else
                        {
                            PropLogic.SetStaticAction += CovertToCoin;
                        }
                    }
                }
            }
        }

        private const string DefaultDateTimeFormet = "yyyy-MM-dd HH:mm:ss";
        private DateTime m_MinDateTime = new DateTime(2000, 1, 1);

        /// <summary>
        /// 从指定游戏配置项中读取日期
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="value">读取的日期，默认值为DateTime(2000, 1, 1)</param>
        /// <returns>是否读取日期成功</returns>
        public bool GetDateTime(string settingName, out DateTime value)
        {
            string saveValue = PlayerPrefs.GetString(settingName, null);
            if (string.IsNullOrEmpty(saveValue) || !DateTime.TryParse(saveValue, out value) || value == m_MinDateTime)
            {
                value = m_MinDateTime;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 向指定游戏配置项写入日期
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">写入的日期，最低日期为DateTime(2000, 1, 1)</param>
        public void SetDateTime(string settingName, DateTime value)
        {
            if (value < m_MinDateTime)
            {
                value = m_MinDateTime;
            }

            PlayerPrefs.SetString(settingName, value.ToString(DefaultDateTimeFormet));
        }
    }
}