using System;
using System.Collections.Generic;
using System.Text;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝关卡数据
    /// </summary>
    public sealed class HiddenTempleLevelData
    {
        private int m_Stage;
        private int m_LevelId;
        private int m_BoardRow;
        private int m_BoardCol;
        private List<GemLayoutData> m_GemDatas;
        private List<DiggedGridLayoutData> m_DiggedGridDatas;
        private List<DoubleBlockLayoutData> m_DoubleBlockDatas;

        public HiddenTempleLevelData(int levelId, string levelDataText)
        {
            m_Stage = 1;
            m_LevelId = levelId;
            m_BoardRow = 0;
            m_BoardCol = 0;
            m_GemDatas = new List<GemLayoutData>();
            m_DiggedGridDatas = null;
            m_DoubleBlockDatas = null;
            ReadLevelData(levelDataText);
        }

        public HiddenTempleLevelData(int stage, int levelId, int boardRow, int boardCol, List<GemLayoutData> gemDatas, List<DiggedGridLayoutData> diggedGridDatas, List<DoubleBlockLayoutData> doubleBlockLayoutDatas)
        {
            m_Stage = stage;
            m_LevelId = levelId;
            m_BoardRow = boardRow;
            m_BoardCol = boardCol;
            m_GemDatas = gemDatas;
            m_DiggedGridDatas = diggedGridDatas;
            m_DoubleBlockDatas = doubleBlockLayoutDatas;
        }

        public HiddenTempleLevelData(string localLevelData, string localDoubleBlockData)
        {
            m_Stage = 1;
            m_LevelId = 0;
            m_BoardRow = 0;
            m_BoardCol = 0;
            m_GemDatas = new List<GemLayoutData>();
            m_DiggedGridDatas = null;
            m_DoubleBlockDatas = null;
            LoadLocalLevelData(localLevelData);
            LoadLocalDoubleBlockData(localDoubleBlockData);
        }

        /// <summary>
        /// 寻宝阶段
        /// </summary>
        public int Stage => m_Stage;

        /// <summary>
        /// 关卡编号
        /// </summary>
        public int LevelId => m_LevelId;

        /// <summary>
        /// 棋盘行数
        /// </summary>
        public int BoardRow => m_BoardRow;

        /// <summary>
        /// 棋盘列数
        /// </summary>
        public int BoardCol => m_BoardCol;

        /// <summary>
        /// 宝石数据
        /// </summary>
        public GemLayoutData[] GemDatas => m_GemDatas.ToArray();

        /// <summary>
        /// 挖掘过的格子数据
        /// </summary>
        public DiggedGridLayoutData[] DiggedGridDatas => m_DiggedGridDatas != null ? m_DiggedGridDatas.ToArray() : null;

        /// <summary>
        /// 双重土块数据
        /// </summary>
        public DoubleBlockLayoutData[] DoubleBlockLayoutDatas => m_DoubleBlockDatas != null ? m_DoubleBlockDatas.ToArray() : null;

        public bool IsValid()
        {
            return m_BoardRow > 0 && m_BoardCol > 0;
        }

        /// <summary>
        /// 解析关卡文本
        /// </summary>
        private void ReadLevelData(string levelDataText)
        {
            string[] lines = levelDataText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            int index = 0;

            while (index < lines.Length)
            {
                string line = lines[index];
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith("STAGE ", StringComparison.Ordinal))
                    {
                        m_Stage = int.Parse(line.Replace("STAGE ", string.Empty).Trim());
                    }
                    else if (line.StartsWith("SIZE ", StringComparison.Ordinal))
                    {
                        string blocksString = line.Replace("SIZE", string.Empty).Trim();
                        string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        m_BoardRow = int.Parse(sizes[0]);
                        m_BoardCol = int.Parse(sizes[1]);
                    }
                    else if (line.StartsWith("DoubleBlock ", StringComparison.Ordinal)) 
                    {
                        string blocksString = line.Replace("DoubleBlock", string.Empty).Trim();
                        string[] blockDataStrings = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var data in blockDataStrings)
                        {
                            string[] blockDatas = data.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                            int row = int.Parse(blockDatas[0]);
                            int col = int.Parse(blockDatas[1]);
                            int canDigTime = int.Parse(blockDatas[2]);
                            if (m_DoubleBlockDatas == null)
                                m_DoubleBlockDatas = new List<DoubleBlockLayoutData>();
                            m_DoubleBlockDatas.Add(new DoubleBlockLayoutData(row, col, canDigTime));
                        }
                    }
                    else
                    {
                        string[] splits = line.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        int gemId = int.Parse(splits[0]);
                        string[] posSplits = splits[1].Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                        if (m_GemDatas == null)
                            m_GemDatas = new List<GemLayoutData>();
                        m_GemDatas.Add(new GemLayoutData(gemId, int.Parse(posSplits[0]), int.Parse(posSplits[1])));
                    }
                }

                index++;
            }
        }

        /// <summary>
        /// 获取保存至本地的关卡数据文本
        /// </summary>
        public string SaveLocalLevelData()
        {
            if (!IsValid())
                return null;

            string lineSplit = "#";
            string gSplit = "/";
            string bSplit = "&";
            StringBuilder sb = new StringBuilder();
            sb.Append(m_Stage);
            sb.Append(lineSplit);
            sb.Append(m_LevelId);
            sb.Append(lineSplit);
            sb.Append(m_BoardRow);
            sb.Append(lineSplit);
            sb.Append(m_BoardCol);
            sb.Append(lineSplit);
            StringBuilder gemString = new StringBuilder();
            foreach (GemLayoutData gemData in m_GemDatas)
            {
                gemString.Append(gemData.GemId);
                gemString.Append(gSplit);
                gemString.Append(gemData.Row);
                gemString.Append(gSplit);
                gemString.Append(gemData.Col);
                gemString.Append(bSplit);
            }
            sb.Append(gemString.ToString());
            if (m_DiggedGridDatas != null)
            {
                sb.Append(lineSplit);
                StringBuilder gridString = new StringBuilder();
                foreach (DiggedGridLayoutData gridData in m_DiggedGridDatas)
                {
                    gridString.Append(gridData.Row);
                    gridString.Append(gSplit);
                    gridString.Append(gridData.Col);
                    gridString.Append(bSplit);
                }
                sb.Append(gridString.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 加载保存至本地的关卡数据文本
        /// </summary>
        public void LoadLocalLevelData(string levelData)
        {
            string lineSplit = "#";
            string gSplit = "/";
            string bSplit = "&";

            int index = 0;
            string[] lineSplits = levelData.Split(lineSplit, StringSplitOptions.RemoveEmptyEntries);
            m_Stage = int.Parse(lineSplits[index++]);
            m_LevelId = int.Parse(lineSplits[index++]);
            m_BoardRow = int.Parse(lineSplits[index++]);
            m_BoardCol = int.Parse(lineSplits[index++]);
            
            if (index < lineSplits.Length)
            {
                string gemString = lineSplits[index++];
                string[] gemSplits = gemString.Split(bSplit, StringSplitOptions.RemoveEmptyEntries);
                m_GemDatas.Clear();
                foreach (var gemSplit in gemSplits)
                {
                    string[] splits = gemSplit.Split(gSplit, StringSplitOptions.RemoveEmptyEntries);
                    m_GemDatas.Add(new GemLayoutData(int.Parse(splits[0]), int.Parse(splits[1]), int.Parse(splits[2])));
                }
            }

            if (index < lineSplits.Length) 
            {
                string gridString = lineSplits[index++];
                string[] gridSplits = gridString.Split(bSplit, StringSplitOptions.RemoveEmptyEntries);
                m_DiggedGridDatas = new List<DiggedGridLayoutData>();
                foreach (var gridSplit in gridSplits)
                {
                    string[] splits = gridSplit.Split(gSplit, StringSplitOptions.RemoveEmptyEntries);
                    m_DiggedGridDatas.Add(new DiggedGridLayoutData(int.Parse(splits[0]), int.Parse(splits[1])));
                }
            }
        }

        public string SaveLocalDoubleBlockData()
        {
            if (!IsValid() || m_DoubleBlockDatas == null) 
                return null;

            string gSplit = "_"; 
            string bSplit = "/";
            StringBuilder blockString = new StringBuilder();
            foreach (var data in m_DoubleBlockDatas)
            {
                blockString.Append(data.Row);
                blockString.Append(gSplit);
                blockString.Append(data.Col);
                blockString.Append(gSplit);
                blockString.Append(data.CanDigTime);
                blockString.Append(bSplit);
            }

            return blockString.ToString();
        }

        public void LoadLocalDoubleBlockData(string data)
        {
            if (m_DoubleBlockDatas == null)
                m_DoubleBlockDatas = new List<DoubleBlockLayoutData>();
            else
                m_DoubleBlockDatas.Clear();

            if (string.IsNullOrEmpty(data))
                return;

            string gSplit = "_";
            string bSplit = "/";
            string[] groupSplits = data.Split(bSplit, StringSplitOptions.RemoveEmptyEntries);
            foreach (string group in groupSplits)
            {
                string[] dataSplits = group.Split(gSplit, StringSplitOptions.RemoveEmptyEntries);
                int row= int.Parse(dataSplits[0]);
                int col= int.Parse(dataSplits[1]);
                int canDigTime = int.Parse(dataSplits[2]);
                m_DoubleBlockDatas.Add(new DoubleBlockLayoutData(row, col, canDigTime));
            }
        }
    }

    /// <summary>
    /// 宝石分布数据
    /// </summary>
    public class GemLayoutData
    {
        private int m_GemId;
        private int m_Row;
        private int m_Col;

        public GemLayoutData(int gemId, int row, int col)
        {
            m_GemId = gemId;
            m_Row = row;
            m_Col = col;
        }

        public int GemId => m_GemId;
        public int Row => m_Row;
        public int Col => m_Col;
    }

    /// <summary>
    /// 挖掘过的格子分布数据
    /// </summary>
    public class DiggedGridLayoutData
    {
        private int m_Row;
        private int m_Col;

        public DiggedGridLayoutData(int row, int col)
        {
            m_Row = row;
            m_Col = col;
        }

        public int Row => m_Row;
        public int Col => m_Col;
    }

    /// <summary>
    /// 双重土块分布数据
    /// </summary>
    public class DoubleBlockLayoutData
    {
        private int m_Row;
        private int m_Col;
        private int m_CanDigTime;

        public DoubleBlockLayoutData(int row, int col,int canDigTime)
        {
            m_Row = row;
            m_Col = col;
            m_CanDigTime = canDigTime;
        }

        public int Row => m_Row;
        public int Col => m_Col;
        public int CanDigTime => m_CanDigTime;
    }
}
