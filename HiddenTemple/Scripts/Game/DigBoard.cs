using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using GameFramework.Event;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// ÍÚ¾òÃæ°å
    /// </summary>
    public sealed class DigBoard : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_GridPrefab;
        [SerializeField]
        private Transform m_GridRoot;
        [SerializeField]
        private Transform m_GemRoot;
        [SerializeField]
        private Transform m_GemFlyRoot;
        [SerializeField]
        private RectTransform m_Bg, m_TopBar, m_Main, m_Body, m_PickaxeImg;
        [SerializeField]
        private TextMeshProUGUI m_PickaxeNumText;

        private DigGrid[,] m_Grids = null;
        private List<DigGem> m_Gems = new List<DigGem>();
        private Vector3 m_CenterLocalPos = Vector3.zero;
        private int m_CellWidth = 105;
        private int m_CellHeight = 106;
        private int m_Stage = 0;
        private int m_LevelId = 0;
        private int m_BoardRow = 0;
        private int m_BoardCol = 0;
        private HiddenTempleBaseMenu m_Menu = null;
        private bool m_IsReleased = false;

        public RectTransform PickaxeTrans => m_PickaxeImg;

        public void Initialize(HiddenTempleLevelData data, HiddenTempleBaseMenu menu)
        {
            m_Menu = menu;

            Release();
            m_IsReleased = false;

            GameManager.Event.Subscribe(PickaxeNumChangeEventArgs.EventId, OnPickaxeNumChange);

            LoadLevelData(data);
            RefreshPickaxeNum();

            m_TopBar.transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
        }

        public void Release()
        {
            GameManager.Event.Unsubscribe(PickaxeNumChangeEventArgs.EventId, OnPickaxeNumChange);

            m_TopBar.DOKill();
            gameObject.SetActive(false);
            m_IsReleased = true;

            ReleaseGrids();
            ReleaseGems();
        }

        #region LevelData

        private void LoadLevelData(HiddenTempleLevelData data)
        {
            m_Stage = data.Stage;
            m_LevelId = data.LevelId;
            m_BoardRow = data.BoardRow;
            m_BoardCol = data.BoardCol;

            InitializeGrids(data.DiggedGridDatas, data.DoubleBlockLayoutDatas);

            foreach (GemLayoutData gemData in data.GemDatas)
            {
                InitializeGems(gemData.GemId, gemData.Row, gemData.Col);
            }
        }

        public HiddenTempleLevelData SaveLevelData()
        {
            if (m_Stage == 0 || m_Grids == null) 
            {
                Log.Error("SaveLevelData fail:data is invalid");
                return null;
            }

            List<GemLayoutData> m_GemDatas = new List<GemLayoutData>();
            foreach (DigGem gem in m_Gems)
            {
                m_GemDatas.Add(new GemLayoutData(gem.Id, gem.Row, gem.Col));
            }

            List<DiggedGridLayoutData> diggedGrids = new List<DiggedGridLayoutData>();
            for (int i = 0; i < m_Grids.GetLength(0); i++)
            {
                for (int j = 0; j < m_Grids.GetLength(1); j++)
                {
                    if (m_Grids[i, j] == null || m_Grids[i, j].IsDigged)
                        diggedGrids.Add(new DiggedGridLayoutData(i, j));
                }
            }

            List<DoubleBlockLayoutData> blockDatas = new List<DoubleBlockLayoutData>();
            for (int i = 0; i < m_Grids.GetLength(0); i++)
            {
                for (int j = 0; j < m_Grids.GetLength(1); j++)
                {
                    if (m_Grids[i, j] != null && m_Grids[i, j].CanDigTime > 0)
                        blockDatas.Add(new DoubleBlockLayoutData(i, j, m_Grids[i, j].CanDigTime));
                }
            }

            return new HiddenTempleLevelData(m_Stage, m_LevelId, m_BoardRow, m_BoardCol, m_GemDatas, diggedGrids, blockDatas);
        }

        #endregion

        #region Grid

        private void InitializeGrids(DiggedGridLayoutData[] diggedGrids, DoubleBlockLayoutData[] doubleBlocks)
        {
            m_Grids = new DigGrid[m_BoardRow, m_BoardCol];
            for (int i = 0; i < m_BoardRow; i++)
            {
                for (int j = 0; j < m_BoardCol; j++)
                {
                    bool isDigged = false;
                    if (diggedGrids != null)
                    {
                        for (int w = 0; w < diggedGrids.Length; w++)
                        {
                            if (diggedGrids[w].Row == i && diggedGrids[w].Col == j)
                            {
                                isDigged = true;
                                break;
                            }
                        }
                    }

                    if (!isDigged)
                    {
                        int canDigTime = 1;
                        if (doubleBlocks != null)
                        {
                            foreach (DoubleBlockLayoutData data in doubleBlocks)
                            {
                                if (data.Row == i && data.Col == j && data.CanDigTime > 0)
                                {
                                    canDigTime = data.CanDigTime;
                                    break;
                                }
                            }
                        }

                        GameObject newGrid = Instantiate(m_GridPrefab, m_GridRoot);
                        newGrid.name = $"Grid_{i}_{j}";
                        newGrid.transform.localPosition = GetGridLocalPos(i, j);
                        DigGrid digGrid = newGrid.GetComponent<DigGrid>();
                        digGrid.Initialize(i, j, canDigTime, m_Menu);
                        m_Grids[i, j] = digGrid;
                        newGrid.SetActive(true);
                    }
                }
            }

            float bodyOffsetY = 0f;
            float mainSize = 1f;
            if((Screen.height / (float)Screen.width) <= 1920f / 1080f)
            {
                switch (m_BoardRow)
                {
                    case 6:
                        mainSize = 0.95f;
                        bodyOffsetY = -50;
                        break;
                    case 7:
                        mainSize = 0.85f;
                        bodyOffsetY = -50;
                        break;
                    case 8:
                        mainSize = 0.75f;
                        bodyOffsetY = -50;
                        break;
                    case 9:
                        mainSize = 0.7f;
                        bodyOffsetY = -50;
                        break;
                    case 10:
                        mainSize = 0.65f;
                        bodyOffsetY = -50;
                        break;
                }
            }
            else
            {
                switch (m_BoardRow)
                {
                    case 10:
                        mainSize = 0.9f;
                        break;
                }
            }

            if (m_Body != null)
                m_Body.localPosition = new Vector3(0, bodyOffsetY, 0);
            if (m_Main != null) 
                m_Main.localScale = new Vector3(mainSize, mainSize, mainSize);

            m_Bg.sizeDelta = new Vector2(m_BoardCol * (m_CellWidth + 10), m_BoardRow * (m_CellHeight + 10));
            m_TopBar.sizeDelta = new Vector2(m_BoardCol * (m_CellWidth + 10) * mainSize, m_BoardRow * (m_CellHeight + 10) * mainSize);
        }

        private void ReleaseGrids()
        {
            if (m_Grids != null)
            {
                for (int i = 0; i < m_Grids.GetLength(0); i++)
                {
                    for (int j = 0; j < m_Grids.GetLength(1); j++)
                    {
                        if (m_Grids[i, j] != null) 
                            Destroy(m_Grids[i, j].gameObject);
                    }
                }
                m_Grids = null;
            }
        }

        private Vector3 GetGridLocalPos(int row, int col)
        {
            float localPosX;
            float localPosY;
            if (m_BoardCol % 2 == 0)
            {
                int half = m_BoardCol / 2;
                if (col < half)
                    localPosX = m_CenterLocalPos.x - (half - col - 0.5f) * m_CellWidth;
                else
                    localPosX = m_CenterLocalPos.x + (col - half + 0.5f) * m_CellWidth;
            }
            else
            {
                int half = m_BoardCol / 2;
                localPosX = m_CenterLocalPos.x - (half - col) * m_CellWidth;
            }

            if (m_BoardRow % 2 == 0)
            {
                int half = m_BoardRow / 2;
                if (row < half)
                    localPosY = m_CenterLocalPos.y - (row - half + 0.5f) * m_CellHeight;
                else
                    localPosY = m_CenterLocalPos.y + (half - row - 0.5f) * m_CellHeight;
            }
            else
            {
                int half = m_BoardRow / 2;
                localPosY = m_CenterLocalPos.y - (row - half) * m_CellHeight;
            }

            return new Vector3(localPosX, localPosY, 0);
        }

        #endregion

        #region Gem

        private void InitializeGems(int gemId, int row, int col)
        {
            IDataTable<DRGemData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRGemData>();
            DRGemData data = dataTable.GetDataRow(gemId);
            if (data == null)
                throw new System.Exception($"Initialize Gems fail:Gem id {gemId} is invalid!");

            UnityUtility.InstantiateAsync(data.AssetName, m_GemRoot, obj =>
            {
                if (m_IsReleased)
                    return;

                obj.name = $"{data.AssetName}_{row}_{col}";

                bool isDigOut = CheckIsGemCompleteDigOut(row, col, data.Height, data.Width);
                if (isDigOut)
                {
                    obj.SetActive(false);
                    HiddenTempleMainMenu mainMenu = m_Menu as HiddenTempleMainMenu;
                    void callback()
                    {
                        if (m_IsReleased || mainMenu == null) 
                            return;
                        TempleGemSlot slot = mainMenu.TempleArea.GetTempleGemSlot(gemId);
                        if (slot == null)
                        {
                            Debug.LogError($"GetTempleGemSlot error - gem slot {gemId} is null");
                            return;
                        }
                        obj.transform.SetParent(slot.transform);
                        obj.transform.localPosition = Vector3.zero;
                        DigGem digGem = obj.GetComponent<DigGem>();
                        digGem.Initialize(row, col, data, isDigOut);
                        m_Gems.Add(digGem);
                        slot.SetFilledGem(digGem);
                        obj.SetActive(true);

                        if (mainMenu.TempleArea.IsAllGemSlotFilled() && mainMenu.TempleArea.CurTemple.Stage == HiddenTempleManager.PlayerData.GetCurrentStage()) 
                        {
                            int newStage = HiddenTempleManager.PlayerData.GetCurrentStage() + 1;
                            HiddenTempleManager.PlayerData.SetCurrentStage(newStage);
                            foreach (DigGrid grid in m_Grids)
                            {
                                if (grid != null)
                                    grid.m_Button.interactable = false;
                            }
                            m_TopBar.transform.DOLocalMoveY(6, 0.2f).onComplete = () =>
                            {
                                m_TopBar.transform.DOLocalMoveY(-120, 0.2f).onComplete = () =>
                                {
                                    OnAllGemDigOut();
                                };
                            };
                        }
                    }

                    mainMenu.TempleArea.CurTempleGenerated += callback;
                }
                else
                {
                    obj.transform.localPosition = GetGemLocalPos(row, row + data.Height - 1, col, col + data.Width - 1);
                    DigGem digGem = obj.GetComponent<DigGem>();
                    digGem.Initialize(row, col, data, isDigOut);
                    m_Gems.Add(digGem);
                    obj.SetActive(true);
                }
            });
        }

        private void ReleaseGems()
        {
            for (int i = 0; i < m_Gems.Count; i++)
            {
                m_Gems[i].Release();
                UnityUtility.UnloadInstance(m_Gems[i].gameObject);
            }
            m_Gems.Clear();
        }

        private Vector3 GetGemLocalPos(int minRow, int maxRow, int minCol, int maxCol)
        {
            return (GetGridLocalPos(minRow, minCol) + GetGridLocalPos(maxRow, minCol) + GetGridLocalPos(maxRow, maxCol) + GetGridLocalPos(minRow, maxCol)) / 4f;
        }

        #endregion

        public void RefreshPickaxeNum()
        {
            m_PickaxeNumText.text = HiddenTempleManager.PlayerData.GetPickaxeNum().ToString();
        }

        public void ShowDestroyAllCoverAnim()
        {
            bool needShowSound = false;
            foreach (var grid in m_Grids)
            {
                if (grid != null && !grid.IsDigged)
                {
                    grid.ShowDestroyCoverAnim(true);
                    needShowSound = true;
                }
            }

            if(needShowSound)
                GameManager.Sound.PlayAudio("SFX_Temple_Checker_Board_Clear");
        }

        public bool CheckIsGemCompleteDigOut(int row, int col, int height, int width)
        {
            if (m_Grids == null)
                return false;

            for (int i = row; i < row + height; i++)
            {
                for (int j = col; j < col + width; j++)
                {
                    if (m_Grids[i, j] != null && !m_Grids[i, j].IsDigged)
                        return false;
                }
            }

            return true;
        }

        public void OnDigGrid()
        {
            if (m_IsReleased)
                return;

            foreach (DigGem gem in m_Gems)
            {
                if (gem.IsDigOut)
                    continue;

                if (CheckIsGemCompleteDigOut(gem.Row, gem.Col, gem.Data.Height, gem.Data.Width)) 
                {
                    HiddenTempleMainMenu mainMenu = m_Menu as HiddenTempleMainMenu;
                    TempleGemSlot slot = mainMenu.TempleArea.GetTempleGemSlot(gem.Data.Id);
                    slot.SetFilledGem(gem);
                    gem.IsDigOut = true;

                    bool isAllGemSlotFilled = mainMenu.TempleArea.IsAllGemSlotFilled();
                    if (isAllGemSlotFilled)
                    {
                        int usePickaxeNum = 0;
                        foreach (DigGrid grid in m_Grids)
                        {
                            if (grid != null)
                            {
                                grid.m_Button.interactable = false;

                                if (grid.IsDigged)
                                    usePickaxeNum++;
                            }
                            else
                            {
                                usePickaxeNum++;
                            }
                        }

                        if (m_TopBar != null)
                        {
                            m_TopBar.transform.DOLocalMoveY(6, 0.2f).onComplete = () =>
                            {
                                m_TopBar.transform.DOLocalMoveY(-120, 0.2f);
                            };
                        }

                        int newStage = HiddenTempleManager.PlayerData.GetCurrentStage() + 1;
                        HiddenTempleManager.PlayerData.SetCurrentStage(newStage);

                        if (newStage > HiddenTempleManager.PlayerData.GetMaxStage())
                        {
                            mainMenu.HideSaleButton();

                            GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Complete);
                        }

                        GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Stage, new Firebase.Analytics.Parameter("Stage", newStage - 1), new Firebase.Analytics.Parameter("Use", usePickaxeNum));
                    }

                    GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                    {
                        if (m_IsReleased) return;

                        if (m_Gems.Count > 0)
                        {
                            gem.transform.SetParent(m_GemFlyRoot);
                            gem.ShowDigOutAnim(slot.transform.position, () =>
                            {
                                if (m_IsReleased) return;

                                slot.ShowFillGemEffect();

                                GameManager.Task.AddDelayTriggerTask(0.24f, () =>
                                {
                                    if (m_IsReleased) return;

                                    gem.transform.SetParent(slot.transform);

                                    if (isAllGemSlotFilled)
                                        OnAllGemDigOut();
                                });
                            });
                        }
                    });

                    break;
                }
            }
        }

        public DigGrid HighlightTargetGrid(int row, int col)
        {
            if (m_Grids == null || m_Grids.GetLength(0) <= row || m_Grids.GetLength(1) <= col)  
                return null;

            if (m_Grids[row, col] == null || m_Grids[row, col].IsDigged)
                return null;

            DigGrid targetGrid = m_Grids[row, col];
            var canvas = targetGrid.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 7;
            targetGrid.gameObject.AddComponent<GraphicRaycaster>();

            return targetGrid;
        }

        public void OnShowNextDigBoardStart()
        {
            m_TopBar.transform.localPosition = new Vector3(m_TopBar.transform.localPosition.x, -120, 0);
        }

        public void OnShowNextDigBoardComplete()
        {
            m_TopBar.transform.DOLocalMoveY(6, 0.2f).onComplete = () =>
            {
                m_TopBar.transform.DOLocalMoveY(0, 0.2f);
            };
        }

        private void OnAllGemDigOut()
        {
            HiddenTempleMainMenu mainMenu = m_Menu as HiddenTempleMainMenu;
            mainMenu.DigArea.ShowDigBoardBlack();

            mainMenu.TempleArea.GenerateTemple(HiddenTempleManager.PlayerData.GetCurrentStage(), false);
            GameManager.Task.AddDelayTriggerTask(0.6f, mainMenu.TempleArea.ShowTempleDoorOpenAnim);
        }

        private void OnPickaxeNumChange(object sender, GameEventArgs e)
        {
            RefreshPickaxeNum();
        }
    }
}
