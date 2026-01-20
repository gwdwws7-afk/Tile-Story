using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 神庙区域
    /// </summary>
    public sealed class TempleArea : MonoBehaviour
    {
        public Transform m_ChestFlyRoot;
        public Transform[] m_TempleRoots;

        private HiddenTempleMainMenu m_Menu;
        private Temple m_CurTemple;
        private Temple m_LastTemple;
        private List<AsyncOperationHandle<GameObject>> m_TempleHandleList = new List<AsyncOperationHandle<GameObject>>();
        private Action m_DefferedShowTempleDoorOpenAction;
        private Action m_CurTempleGeneratedAction;

        public HiddenTempleMainMenu Menu => m_Menu;

        public Temple CurTemple => m_CurTemple;

        /// <summary>
        /// 当前神庙实例生成完毕事件
        /// </summary>
        public event Action CurTempleGenerated
        {
            add
            {
                m_CurTempleGeneratedAction += value;

                if (m_CurTemple != null) 
                {
                    m_CurTempleGeneratedAction?.Invoke();
                    m_CurTempleGeneratedAction = null;
                }
            }
            remove
            {
                m_CurTempleGeneratedAction -= value;
            }
        }

        public void Initialize(int stage, HiddenTempleBaseMenu menu)
        {
            m_Menu = menu as HiddenTempleMainMenu;

            m_DefferedShowTempleDoorOpenAction = null;
            m_CurTempleGeneratedAction = null;
            GenerateTemple(stage, true);
        }

        public void Release()
        {
            m_Menu = null;
            m_CurTemple = null;
            m_LastTemple = null;
            m_DefferedShowTempleDoorOpenAction = null;
            m_CurTempleGeneratedAction = null;

            foreach (AsyncOperationHandle<GameObject> han in m_TempleHandleList)
            {
                if (han.IsValid())
                {
                    if (han.Result != null)
                        han.Result.GetComponent<Temple>().Release();
                    UnityUtility.UnloadInstance(han);
                }
            }
            m_TempleHandleList.Clear();
        }

        public bool CheckInitComplete()
        {
            foreach (AsyncOperationHandle<GameObject> han in m_TempleHandleList)
            {
                if(han.IsValid())
                    return m_CurTemple != null;
            }

            return true;
        }

        public void GenerateTemple(int stage, bool isCurTemple)
        {
            if (stage < 1 && stage > HiddenTempleManager.PlayerData.GetMaxStage() + 1) 
            {
                Log.Error("GenerateTemple fail: stage {0} is invalid", stage);
                return;
            }

            string templeAssetName = "Temple_" + (stage > HiddenTempleManager.PlayerData.GetMaxStage() + 1 ? HiddenTempleManager.PlayerData.GetMaxStage() + 1 : stage).ToString();
            var handle = UnityUtility.InstantiateAsync(templeAssetName, m_TempleRoots[stage - 1], obj =>
              {
                  if (m_Menu != null)
                  {
                      if (m_CurTemple != null)
                          m_LastTemple = m_CurTemple;

                      m_CurTemple = obj.GetComponent<Temple>();
                      m_CurTemple.Initialize(stage, isCurTemple, this);

                      if (isCurTemple)
                      {
                          m_CurTempleGeneratedAction?.Invoke();
                          m_CurTempleGeneratedAction = null;
                      }
                      else
                      {
                          m_DefferedShowTempleDoorOpenAction?.Invoke();
                          m_DefferedShowTempleDoorOpenAction = null;
                      }
                  }
              });
            m_TempleHandleList.Add(handle);
        }

        public bool IsAllGemSlotFilled()
        {
            return m_CurTemple.IsAllGemSlotFilled();
        }

        public TempleGemSlot GetTempleGemSlot(int gemId)
        {
            return m_CurTemple.GetTempleGemSlot(gemId);
        }

        public void ShowTempleDoorOpenAnim()
        {
            if (m_LastTemple == null || m_CurTemple == null)
            {
                Log.Error("ShowTempleDoorOpenAnim fail: temple is null.try deffered action trigger");
                m_DefferedShowTempleDoorOpenAction = ShowTempleDoorOpenAnim;
                return;
            }

            m_LastTemple.ShowTempleDoorOpenAnim(() =>
            {
                if (m_CurTemple.m_Black != null) 
                    m_CurTemple.m_Black.GetComponent<Image>().material.SetFloat("_FadeDistance", 3.5f);
                m_CurTemple.ShowTempleChestShowAnim();
            });
        }

        public void OnChestJumpIn(int stage)
        {
            m_Menu.ChestArea.ShowChestSlotUnlockAnim(stage);
        }

        public void OnShowNextTemple()
        {
            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                m_Menu.ChestArea.ShowNextStageChestAnim();
                m_Menu.DigArea.ShowNextDigBoard();
            });
        }
    }
}
