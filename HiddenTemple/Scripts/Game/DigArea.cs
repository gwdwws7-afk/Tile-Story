using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 挖掘区域
    /// </summary>
    public sealed class DigArea : MonoBehaviour
    {
        [SerializeField]
        private DigBoard m_DigBoard_1;
        [SerializeField]
        private DigBoard m_DigBoard_2;
        [SerializeField]
        private Image m_DigAreaBlack;
        [SerializeField]
        private GameObject m_CompleteAllText;
        [SerializeField]
        private GameObject m_PickaxePrefab;
        [SerializeField]
        private GameObject m_GridBrokenEffectPrefab;

        private HiddenTempleBaseMenu m_Menu;
        private DigBoard m_CurDigBoard;
        private float m_DigBoardLocalPosY = -530;
        private List<Pickaxe> m_PickaxeList = new List<Pickaxe>();
        private List<Effect_GridBroken> m_GridBrokenEffectList = new List<Effect_GridBroken>();

        public void Initialize(int stage, HiddenTempleBaseMenu menu)
        {
            m_Menu = menu;

            if (stage <= HiddenTempleManager.PlayerData.GetMaxStage())
            {
                m_CurDigBoard = m_DigBoard_1;
                m_CurDigBoard.transform.localPosition = new Vector3(m_CurDigBoard.transform.localPosition.x, m_DigBoardLocalPosY, 0);
                LoadLevel(stage);
            }
            else
            {
                m_DigAreaBlack.color = Color.white;
                m_DigAreaBlack.gameObject.SetActive(true);
                m_CompleteAllText.SetActive(true);
            }
        }

        public void Release()
        {
            m_CurDigBoard = null;
            m_DigAreaBlack.DOKill();
            m_DigAreaBlack.gameObject.SetActive(false);
            m_CompleteAllText.transform.DOKill();
            m_CompleteAllText.SetActive(false);
            m_DigBoard_1.Release();
            m_DigBoard_2.Release();

            foreach (Pickaxe pickaxe in m_PickaxeList)
            {
                pickaxe.Release();
                Destroy(pickaxe.gameObject);
            }
            m_PickaxeList.Clear();

            foreach (Effect_GridBroken effect in m_GridBrokenEffectList)
            {
                Destroy(effect.gameObject);
            }
            m_GridBrokenEffectList.Clear();
        }

        private void LoadLevel(int stage, Action callback = null)
        {
            string levelDataString = HiddenTempleManager.PlayerData.GetSavedStageLevelData(stage);
            string doubleBlockString = HiddenTempleManager.PlayerData.GetSavedDoubleBlockData(stage);
            if (!string.IsNullOrEmpty(levelDataString))
            {
                var levelData = new HiddenTempleLevelData(levelDataString, doubleBlockString);
                m_CurDigBoard.Initialize(levelData, m_Menu);
                callback?.Invoke();
            }
            else
            {
                int levelId = GetLevelId(stage);
                Addressables.LoadAssetAsync<TextAsset>(GetLevelAssetName(levelId)).Completed += res =>
                {
                    if (res.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (m_CurDigBoard != null)
                        {
                            var levelData = new HiddenTempleLevelData(levelId, res.Result.text);
                            m_CurDigBoard.Initialize(levelData, m_Menu);
                            callback?.Invoke();
                        }

                        Addressables.Release(res);
                    }
                };
            }
        }

        public bool SaveLevel()
        {
            if (m_CurDigBoard == null)
                return false;

            HiddenTempleLevelData data = m_CurDigBoard.SaveLevelData();
            if (data != null)
            {
                string localLevelData = data.SaveLocalLevelData();
                string localDoubleBlockData = data.SaveLocalDoubleBlockData();
                if (!string.IsNullOrEmpty(localLevelData))
                {
                    HiddenTempleManager.PlayerData.SetSavedStageLevelData(data.Stage, localLevelData);
                    HiddenTempleManager.PlayerData.SetSavedDoubleBlockData(data.Stage, localDoubleBlockData);
                    return true;
                }
            }

            return false;
        }

        private int GetLevelId(int stage)
        {
            IDataTable<DRLevelData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRLevelData>();
            DRLevelData data = dataTable.GetDataRow(stage);

            //第一次开启活动，固定关卡
            if (stage == 1 && HiddenTempleManager.PlayerData.GetOpenActivityTime() == 1)
                return data.LevelsId[0];
            else
                return data.LevelsId[UnityEngine.Random.Range(0, data.LevelsId.Count)];
        }

        private string GetLevelAssetName(int levelId)
        {
            return "HT_Level_" + levelId.ToString();
        }

        public void OnDigGrid(Vector3 digPos)
        {
            m_CurDigBoard?.OnDigGrid();

            SaveLevel();

            ShowPickaxeAnim(digPos);
        }

        public void ShowDigBoardBlack()
        {
            if (m_CurDigBoard == null)
                throw new Exception("ShowDigBoardBlack fail:CurDigBoard is null!");

            m_CurDigBoard.ShowDestroyAllCoverAnim();

            m_DigAreaBlack.DOKill();
            m_DigAreaBlack.color = new Color(1, 1, 1, 0);
            m_DigAreaBlack.gameObject.SetActive(true);
            m_DigAreaBlack.DOFade(1f, 0.6f);
        }

        public void ShowNextDigBoard()
        {
            if (m_CurDigBoard == null)
                throw new Exception("ShowNextDigBoard fail:CurDigBoard is null!");

            DigBoard lastDigBoard = m_CurDigBoard;
            if (lastDigBoard == m_DigBoard_1)
                m_CurDigBoard = m_DigBoard_2;
            else
                m_CurDigBoard = m_DigBoard_1;

            m_CurDigBoard.transform.localPosition = new Vector3(m_CurDigBoard.transform.localPosition.x, 500, 0);

            LoadLevel(HiddenTempleManager.PlayerData.GetCurrentStage(), () =>
            {
                if (m_CurDigBoard == null)
                    return;
                m_CurDigBoard.OnShowNextDigBoardStart();
                lastDigBoard.transform.DOLocalMoveY(-2000, 0.6f);
                m_CurDigBoard.transform.DOLocalMoveY(m_DigBoardLocalPosY - 10, 0.6f).onComplete = () =>
                  {
                      m_CurDigBoard.transform.DOLocalMoveY(m_DigBoardLocalPosY, 0.2f).onComplete = () =>
                      {
                          m_CurDigBoard.OnShowNextDigBoardComplete();
                      };
                  };

                m_DigAreaBlack.DOKill();
                m_DigAreaBlack.DOFade(0f, 0.6f).SetDelay(0.2f).onComplete = () =>
                {
                    m_DigAreaBlack.gameObject.SetActive(false);
                };
            });
        }

        public void ShowCompleteAllBoard()
        {
            if (m_CurDigBoard == null)
                throw new Exception("ShowCompleteAllBoard fail:CurDigBoard is null!");

            m_CurDigBoard.transform.DOLocalMoveY(-2000, 0.6f).onComplete = () =>
            {
                m_CompleteAllText.transform.localScale = Vector3.zero;
                m_CompleteAllText.SetActive(true);
                m_CompleteAllText.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    m_CompleteAllText.transform.DOScale(1f, 0.2f);
                };

                GameManager.Sound.PlayAudio("SFX_shopBuySuccess");
            };
            m_CurDigBoard = null;
        }

        public DigGrid HighlightTargetGrid(int row, int col)
        {
            if (m_CurDigBoard == null)
                return null;

            return m_CurDigBoard.HighlightTargetGrid(row, col);
        }

        private void ShowPickaxeAnim(Vector3 digPos)
        {
            Pickaxe pickaxe = null;
            foreach (Pickaxe p in m_PickaxeList)
            {
                if (!p.IsUsing)
                {
                    pickaxe = p;
                    break;
                }
            }

            if (pickaxe == null)
            {
                GameObject obj = Instantiate(m_PickaxePrefab, m_PickaxePrefab.transform.parent);
                pickaxe = obj.GetComponent<Pickaxe>();
                m_PickaxeList.Add(pickaxe);
            }

            pickaxe.ShowPickaxeAnim(digPos);
        }

        public void ShowGridBrokenEffect(Vector3 pos)
        {
            Effect_GridBroken effect = null;
            foreach (Effect_GridBroken p in m_GridBrokenEffectList)
            {
                if (!p.IsUsing)
                {
                    effect = p;
                    break;
                }
            }

            if (effect == null)
            {
                GameObject obj = Instantiate(m_GridBrokenEffectPrefab, m_GridBrokenEffectPrefab.transform.parent);
                effect = obj.GetComponent<Effect_GridBroken>();
                m_GridBrokenEffectList.Add(effect);
            }

            effect.Show(pos);
        }

        #region IItemFlyReceiver

        public Vector3 GetItemTargetPos()
        {
            if (m_CurDigBoard == null)
                return Vector3.zero;

            return m_CurDigBoard.PickaxeTrans.position;
        }

        public void OnFlyHit()
        {
            if (m_CurDigBoard == null)
                return;

            m_CurDigBoard.PickaxeTrans.DOScale(0.27f * 1.3f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
              {
                  m_CurDigBoard.PickaxeTrans.DOScale(0.27f, 0.25f).SetEase(Ease.OutBack);
              };

            GameManager.Event.Fire(this, PickaxeNumChangeEventArgs.Create());
        }

        #endregion
    }
}
