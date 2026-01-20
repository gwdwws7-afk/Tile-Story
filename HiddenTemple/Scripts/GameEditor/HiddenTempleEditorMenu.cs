using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝编辑器
    /// </summary>
    public sealed class HiddenTempleEditorMenu : HiddenTempleBaseMenu
    {
        [SerializeField]
        private Button m_LevelLeftButton, m_LevelRightButton, m_SaveLevelButton, m_CloseButton, m_DoubleBlockButton;
        [SerializeField]
        private InputField m_levelInput, m_RowInput, m_ColInput, m_StageInput;
        [SerializeField]
        private GameObject m_GemEditorSlotPrefab, m_EditorDoubleBlockMask;
        [SerializeField]
        private Transform m_GemEditorSlotRoot;

        //Data
        private const string m_DataPath = "Assets/HiddenTemple/Data/Level/";
        private int m_Stage;
        private int m_LevelId;
        private int m_BoardRow;
        private int m_BoardCol;
        private List<GemLayoutData> m_GemDatas = new List<GemLayoutData>();
        private List<DoubleBlockLayoutData> m_DoubleBlockDatas = new List<DoubleBlockLayoutData>();

        //Editor
        private List<EditorGemSlot> m_GemEditorSlots = new List<EditorGemSlot>();
        private int m_CurSelectedGemId;
        private bool m_SelectedDoubleBlock;

        /// <summary>
        /// 当前选中的宝石编号
        /// </summary>
        public int CurSelectedGemId => m_CurSelectedGemId;

        /// <summary>
        /// 是否选中双重土块
        /// </summary>
        public bool SelectedDoubleBlock => m_SelectedDoubleBlock;

        private void Start()
        {
            InitializeButton();
        }

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            RefreshGemSlotBar();
        }

        public override void OnRelease()
        {
            m_CurSelectedGemId = 0;
            m_SelectedDoubleBlock = false;
            m_GemDatas.Clear();
            m_DoubleBlockDatas.Clear();

            for (int i = 0; i < m_GemEditorSlots.Count; i++)
            {
                Destroy(m_GemEditorSlots[i].gameObject);
            }
            m_GemEditorSlots.Clear();

            base.OnRelease();
        }

        private void InitializeButton()
        {
            m_LevelLeftButton.onClick.AddListener(() =>
            {
                if (m_LevelId <= 1)
                    return;
                ResetEditor();
                m_LevelId--;
                m_levelInput.text = m_LevelId.ToString();
                LoadLevel(m_LevelId);
            });

            m_LevelRightButton.onClick.AddListener(() =>
            {
                if (m_LevelId < 0)
                    return;
                ResetEditor();
                m_LevelId++;
                m_levelInput.text = m_LevelId.ToString();
                LoadLevel(m_LevelId);
            });

            m_SaveLevelButton.onClick.AddListener(() =>
            {
                SaveLevel();
            });

            m_CloseButton.onClick.AddListener(() =>
            {
                GameManager.UI.HideUIForm(this);
            });

            m_levelInput.onEndEdit.AddListener(s =>
            {
                if (int.TryParse(s, out int res) && res > 0 && res != m_LevelId)   
                {
                    m_LevelId = res;
                    ResetEditor();
                    LoadLevel(m_LevelId);
                }
            });

            m_RowInput.onEndEdit.AddListener(s =>
            {
                if (int.TryParse(s, out int res) && res > 0 && res != m_BoardRow) 
                {
                    m_BoardRow = res;
                    m_BoardCol = res;
                    RefreshDigBoard();
                }
            });

            m_ColInput.onEndEdit.AddListener(s =>
            {
                if (int.TryParse(s, out int res) && res > 0 && res != m_BoardRow)
                {
                    m_BoardRow = res;
                    m_BoardCol = res;
                    RefreshDigBoard();
                }
            });

            m_StageInput.onEndEdit.AddListener(s =>
            {
                if (int.TryParse(s, out int res) && res > 0)
                {
                    m_Stage = res;
                    m_GemDatas.Clear();
                    m_DoubleBlockDatas.Clear();
                    RefreshDigBoard();
                    RefreshGemSlotBar();
                }
                else
                {
                    m_StageInput.text = m_Stage.ToString();
                }
            });

            m_DoubleBlockButton.onClick.AddListener(() =>
            {
                SelectDoubleBlock(!m_SelectedDoubleBlock);
            });
        }

        #region Level

        private bool LoadLevel(int levelId)
        {
            if (m_LevelId <= 0)
                return false;

            string levelAssetName = GetLevelAssetName(levelId);
            string levelText = null;
            try
            {
#if UNITY_EDITOR
                levelText = ((TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(m_DataPath + levelAssetName + ".txt", typeof(TextAsset))).text;
#endif
            }
            catch(Exception e)
            {
                Debug.LogError("HiddenTempleEditor load level fail:" + e.Message);
            }

            if (levelText == null)
                return false;

            m_LevelId = levelId;
            var levelData = new HiddenTempleLevelData(levelId, levelText);
            m_BoardRow = levelData.BoardRow;
            m_BoardCol = levelData.BoardCol;
            m_GemDatas = new List<GemLayoutData>(levelData.GemDatas);
            if (levelData.DoubleBlockLayoutDatas != null && levelData.DoubleBlockLayoutDatas.Length > 0) 
                m_DoubleBlockDatas = new List<DoubleBlockLayoutData>(levelData.DoubleBlockLayoutDatas);

            RefreshDigBoard();

            if (m_GemDatas.Count > 0)
            {
                m_Stage = m_GemDatas[0].GemId / 100;
                RefreshGemSlotBar();
            }

            return true;
        }

        private void SaveLevel()
        {
#if UNITY_EDITOR
            if (m_LevelId <= 0 || m_GemDatas.Count == 0 || m_BoardRow <= 0 || m_BoardCol <= 0)   
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append("STAGE ");
            sb.Append(m_Stage);
            sb.Append("\r\n");
            sb.Append("SIZE ");
            sb.Append(m_BoardRow);
            sb.Append("/");
            sb.Append(m_BoardCol);
            sb.Append("\r\n");
            sb.Append("DoubleBlock ");
            foreach (DoubleBlockLayoutData blockData in m_DoubleBlockDatas)
            {
                sb.Append(blockData.Row);
                sb.Append("_");
                sb.Append(blockData.Col);
                sb.Append("_");
                sb.Append(blockData.CanDigTime);
                sb.Append("/");
            }
            sb.Append("\r\n");

            foreach (GemLayoutData gemData in m_GemDatas)
            {
                sb.Append(gemData.GemId);
                sb.Append("/");
                sb.Append(gemData.Row);
                sb.Append("_");
                sb.Append(gemData.Col);
                sb.Append("\r\n");
            }

            string path = System.IO.Path.Combine(m_DataPath, GetLevelAssetName(m_LevelId) + ".txt");
            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                byte[] bts = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                fileStream.Write(bts, 0, bts.Length);
                fileStream.Flush();
                fileStream.Close();
            }
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Save level {m_LevelId} success...");
            GameManager.UI.ShowWeakHint($"Save level {m_LevelId} success...");
#endif
        }

        private string GetLevelAssetName(int levelId)
        {
            return "HT_Level_" + levelId.ToString();
        }

        #endregion

        #region Gem

        public void SelectGem(int gemId)
        {
            SelectDoubleBlock(false);

            m_CurSelectedGemId = gemId;
            foreach (var slot in m_GemEditorSlots)
            {
                slot.m_Mask.SetActive(slot.GemId != gemId);
            }
        }

        public void AddGem(int gemId, int row, int col)
        {
            m_GemDatas.Add(new GemLayoutData(gemId, row, col));
            RefreshDigBoard();
        }

        public bool RemoveGem(int row, int col)
        {
            IDataTable<DRGemData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRGemData>();
            foreach (GemLayoutData gemData in m_GemDatas)
            {
                DRGemData data = dataTable.GetDataRow(gemData.GemId);
                if(row>= gemData.Row && row <= gemData.Row + data.Height - 1 && col >= gemData.Col && col <= gemData.Col + data.Width - 1)
                {
                    m_GemDatas.Remove(gemData);
                    RefreshDigBoard();
                    break;
                }
            }

            return false;
        }

        private void RefreshGemSlotBar()
        {
            m_CurSelectedGemId = 0;
            for (int i = 0; i < m_GemEditorSlots.Count; i++)
            {
                Destroy(m_GemEditorSlots[i].gameObject);
            }
            m_GemEditorSlots.Clear();

            if (m_Stage <= 0)
                return;
            IDataTable<DRGemData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRGemData>();
            DRGemData[] allData = dataTable.GetAllDataRows();
            foreach (DRGemData data in allData)
            {
                int stage = data.Id / 100;
                if (m_Stage == stage)
                {
                    GameObject newSlot = Instantiate(m_GemEditorSlotPrefab, m_GemEditorSlotRoot);
                    newSlot.name = "GemSlot_" + data.Id.ToString();
                    EditorGemSlot slot = newSlot.GetComponent<EditorGemSlot>();
                    slot.Initialize(data, this);
                    m_GemEditorSlots.Add(slot);
                    newSlot.SetActive(true);
                }
            }

            m_StageInput.text = m_Stage.ToString();
        }

        #endregion

        #region Double Block

        public void SelectDoubleBlock(bool isSelect)
        {
            m_SelectedDoubleBlock = isSelect;
            m_EditorDoubleBlockMask.SetActive(!isSelect);

            if (isSelect)
            {
                m_CurSelectedGemId = 0;
                foreach (var slot in m_GemEditorSlots)
                {
                    slot.m_Mask.SetActive(true);
                }
            }
        }

        public void AddDoubleBlock(int row, int col)
        {
            foreach (var data in m_DoubleBlockDatas)
            {
                if (data.Row == row && data.Col == col)
                    return;
            }

            m_DoubleBlockDatas.Add(new DoubleBlockLayoutData(row, col, 2));
            RefreshDigBoard();
        }

        public bool RemoveDoubleBlock(int row, int col)
        {
            foreach (DoubleBlockLayoutData blockData in m_DoubleBlockDatas)
            {
                if (blockData.Row == row && blockData.Col == col)
                {
                    m_DoubleBlockDatas.Remove(blockData);
                    RefreshDigBoard();
                    break;
                }
            }

            return false;
        }

        #endregion

        private void RefreshDigBoard()
        {
            m_RowInput.text = m_BoardRow.ToString();
            m_ColInput.text = m_BoardCol.ToString();
            m_DigBoard.Initialize(new HiddenTempleLevelData(1, m_LevelId, m_BoardRow, m_BoardCol, m_GemDatas, null, m_DoubleBlockDatas), this);
        }

        private void ResetEditor()
        {
            m_BoardRow = 0;
            m_BoardCol = 0;
            m_RowInput.text = m_BoardRow.ToString();
            m_ColInput.text = m_BoardCol.ToString();
            m_GemDatas.Clear();
            m_DoubleBlockDatas.Clear();
            m_DigBoard.Release();

            m_SelectedDoubleBlock = false;
            m_EditorDoubleBlockMask.SetActive(true);
        }
    }
}
