using MySelf.Model;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

namespace Merge.Editor
{
    /// <summary>
    /// 道具初始布局编辑器
    /// </summary>
    public sealed class PropDistributedMapEditor : EditorWindow
    {
        private static int BoardRow = 5;
        private static int BoardCol = 5;
        private static MergeTheme s_MergeTheme = MergeTheme.None;

        private List<DRProp> m_PropDataTable = null;
        private List<DRAttachment> m_AttachmentDataTable = null;
        private Dictionary<string, Texture2D> m_SpriteDic = new Dictionary<string, Texture2D>();
        private Dictionary<string, Texture2D> m_AttachmentSpriteDic = new Dictionary<string, Texture2D>();
        private int[] m_PropDataMap = new int[BoardRow * BoardCol];
        private int[] m_AttachmentDataMap = new int[BoardRow * BoardCol];

        private Vector2 leftScrollViewVector;
        private Vector2 rightScrollViewVector;
        private int m_SelectedPropId;
        private int m_SelectedAttachmentId;
        private bool m_IsMouseDown;
        private Rect m_LastRect;
        private bool m_LoadMapData;

        private string PropAtlasPath
        {
            get
            {
                return $"Assets/Merge/Sprites_{s_MergeTheme}/Atlas/MergePropAtlas_{s_MergeTheme}.spriteatlas";
            }
        }
        private string AttachmentAtlasPath
        {
            get
            {
                return $"Assets/Merge/Sprites_{s_MergeTheme}/Atlas/MergePropAtlas_{s_MergeTheme}.spriteatlas";
            }
        }
        private string PropDataPath
        {
            get
            {
                return $"Assets/Merge/Data/PropData_{s_MergeTheme}.txt";
            }
        }
        private string AttachmentDataPath
        {
            get
            {
                return $"Assets/Merge/Data/AttachmentData_{s_MergeTheme}.txt";
            }
        }
        private string PropDistributedMapPath
        {
            get
            {
                return $"Assets/Merge/Data/PropDistributedMap_{s_MergeTheme}.txt";
            }
        }

        [MenuItem("Window/PropDistributedMapEditor")]
        static void Init()
        {
            PropDistributedMapEditor editor = (PropDistributedMapEditor)GetWindow(typeof(PropDistributedMapEditor), false, "PropDistributedMapEditor", true);
            editor.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(25, 45, 1500, position.height));
            {
                DrawPropDistributedMapEditorWindow();
            }
            GUILayout.EndArea();
        }

        private void DrawPropDistributedMapEditorWindow()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            InitializeData();

            GUILayout.BeginHorizontal();
            {
                leftScrollViewVector = GUI.BeginScrollView(new Rect(25, 45, 420, position.height), leftScrollViewVector, new Rect(0, 0, 400, 10345), false, true);
                {
                    GUILayout.BeginVertical();
                    {
                        GUIMergeTheme();

                        GUILayout.Space(10);

                        GUIBoardLayout();

                        GUILayout.Space(10);

                        //GUIClearPlayerData();

                        //GUILayout.Space(10);

                        GUIPropItems();

                        GUILayout.Space(10);

                        GUIAttachmentItems();
                    }
                    GUILayout.EndVertical();
                }
                GUI.EndScrollView();

                rightScrollViewVector = GUI.BeginScrollView(new Rect(25, 45, 950, position.height), rightScrollViewVector, new Rect(0, 0, 400, 10345), false, true);
                {
                    GUILayout.BeginArea(new Rect(450, 0, 400, 10345));
                    {
                        GUIPropDistributedMap();
                    }
                    GUILayout.EndArea();
                }
                GUI.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void InitializeData()
        {
            if (s_MergeTheme == MergeTheme.None)
                return;

            if (m_PropDataTable == null)
            {
                m_PropDataTable = new List<DRProp>();

                TextAsset propDataTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PropDataPath);
                int position = 0;
                string dataRowString = null;
                while ((dataRowString = propDataTextAsset.text.ReadLine(ref position)) != null)
                {
                    if (dataRowString[0] == '#')
                    {
                        continue;
                    }

                    DRProp drProp = new DRProp();
                    if (!drProp.ParseDataRow(dataRowString))
                    {
                        Debug.LogError("PropDistributedMapEditor Parse Prop Data Error");
                    }
                    else
                    {
                        m_PropDataTable.Add(drProp);
                    }
                }
            }

            if (m_AttachmentDataTable == null)
            {
                m_AttachmentDataTable = new List<DRAttachment>();

                TextAsset attachmentDataTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AttachmentDataPath);
                int position = 0;
                string dataRowString = null;
                while ((dataRowString = attachmentDataTextAsset.text.ReadLine(ref position)) != null)
                {
                    if (dataRowString[0] == '#')
                    {
                        continue;
                    }

                    DRAttachment drAttachment = new DRAttachment();
                    if (!drAttachment.ParseDataRow(dataRowString))
                    {
                        Debug.LogError("PropDistributedMapEditor Parse Attachment Data Error");
                    }
                    else
                    {
                        m_AttachmentDataTable.Add(drAttachment);
                    }
                }
            }

            if (!m_LoadMapData)
            {
                m_LoadMapData = true;
                LoadMapData(true);
            }
        }

        private void GUIMergeTheme()
        {
            GUI.changed = false;

            s_MergeTheme = (MergeTheme)EditorGUILayout.EnumPopup("Theme:", s_MergeTheme, GUILayout.Width(300));

            if (GUI.changed)
            {
                GUI.changed = false;

                m_PropDataMap = new int[BoardRow * BoardCol];
                m_AttachmentDataMap = new int[BoardRow * BoardCol];
                m_PropDataTable = null;
                m_AttachmentDataTable = null;
                m_SpriteDic.Clear();
                m_AttachmentSpriteDic.Clear();
                m_SelectedPropId = 0;
                m_SelectedAttachmentId = 0;
                m_LoadMapData = false;

                InitializeData();
            }
        }

        private void GUIClearPlayerData()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(30);
                if (GUILayout.Button("Test Map", GUILayout.Width(160)))
                {
                    SaveMapData();
                    MergeModel.Instance.SavedPropDistributedMap = null;
                    MergeModel.Instance.CurrentMaxMergeStage = 0;
                    MergeModel.Instance.StorePropIds = null;
                    for (int i = 1; i <= 9; i++)
                    {
                        PlayerPrefs.SetInt("Merge.GuideIsComplete_" + i.ToString(), 0);
                    }

                    EditorSceneManager.OpenScene("Assets/GameMain/Scenes/menu.unity");
                    EditorApplication.isPlaying = true;
                }

                if (GUILayout.Button("Open Excel Folder", GUILayout.Width(160)))
                {
                    DirectoryInfo di = Directory.GetParent(Application.dataPath);
                    string parentParentPath = di.Parent.FullName;

                    System.Diagnostics.Process.Start("explorer.exe", Path.Combine(parentParentPath, @"Assets/Merge/Documents"));
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GUIBoardLayout()
        {
            if (s_MergeTheme == MergeTheme.None)
                return;

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;

                GUILayout.Label("Row:", EditorStyles.label, GUILayout.Width(40));
                int row = int.Parse(GUILayout.TextField(BoardRow.ToString(), GUILayout.Width(100)));
                GUILayout.Label("Col:", EditorStyles.label, GUILayout.Width(40));
                int col = int.Parse(GUILayout.TextField(BoardCol.ToString(), GUILayout.Width(100)));

                if (GUI.changed)
                {
                    if (row > 0 && col > 0)
                    {
                        BoardRow = row;
                        BoardCol = col;

                        m_PropDataMap = new int[BoardRow * BoardCol];
                        m_AttachmentDataMap = new int[BoardRow * BoardCol];
                        LoadMapData(false);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GUIPropItems()
        {
            if (s_MergeTheme == MergeTheme.None)
                return;

            GUILayout.BeginVertical();
            {
                GUILayout.Label("Props:", EditorStyles.label, GUILayout.Width(150));

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(30);

                    GUILayout.BeginVertical();
                    {
                        int row = (int)Mathf.Ceil(m_PropDataTable.Count / 6f);
                        for (int i = 0; i < row; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    int index = i * 6 + j;
                                    if (index < m_PropDataTable.Count)
                                    {
                                        Texture2D tex = LoadEditorSprite(m_PropDataTable[index].AssetName, PropAtlasPath);
                                        int id = m_PropDataTable[index].Id;

                                        if (id == m_SelectedPropId)
                                        {
                                            GUI.backgroundColor = Color.gray;
                                        }

                                        if (GUILayout.Button(tex, GUILayout.Width(50), GUILayout.Height(50)))
                                        {
                                            if (m_SelectedPropId == id)
                                                m_SelectedPropId = 0;
                                            else
                                                m_SelectedPropId = id;
                                        }
                                        GUI.backgroundColor = Color.white;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void GUIAttachmentItems()
        {
            if (s_MergeTheme == MergeTheme.None)
                return;

            GUILayout.BeginVertical();
            {
                GUILayout.Label("Attachments:", EditorStyles.label, GUILayout.Width(150));

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(30);

                    GUILayout.BeginVertical();
                    {
                        int row = (int)Mathf.Ceil(m_AttachmentDataTable.Count / 6f);
                        for (int i = 0; i < row; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    int index = i * 6 + j;
                                    if (index < m_AttachmentDataTable.Count)
                                    {
                                        Texture2D tex = LoadEditorSprite(m_AttachmentDataTable[index].AssetName, AttachmentAtlasPath);
                                        int id = m_AttachmentDataTable[index].Id;

                                        if (id == m_SelectedAttachmentId)
                                        {
                                            GUI.backgroundColor = Color.gray;
                                        }

                                        if (GUILayout.Button(tex, GUILayout.Width(50), GUILayout.Height(50)))
                                        {
                                            if (m_SelectedAttachmentId == id)
                                                m_SelectedAttachmentId = 0;
                                            else
                                                m_SelectedAttachmentId = id;
                                        }
                                        GUI.backgroundColor = Color.white;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void GUIPropDistributedMap()
        {
            RecordMouseControl();

            GUILayout.BeginVertical();
            {
                GUILayout.Space(25);

                int row = BoardRow;
                int col = BoardCol;

                GUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < col; i++)
                    {
                        GUILayout.Label((i + 1).ToString(), GUILayout.Width(50));
                    }
                }
                GUILayout.EndHorizontal();

                for (int i = 0; i < row; i++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        for (int j = 0; j < col; j++)
                        {
                            int index = i * col + j;

                            Texture2D propSprite = null;
                            if (m_PropDataMap[index] != 0)
                            {
                                propSprite = LoadPropEditorSprite(m_PropDataMap[index], PropAtlasPath);
                            }

                            if (GUILayout.Button(propSprite, GUILayout.Width(50), GUILayout.Height(50)))
                            {
                            }

                            if (m_AttachmentDataMap[index] != 0)
                            {
                                Texture2D attachmentSprite = LoadAttachmentEditorSprite(m_AttachmentDataMap[index], AttachmentAtlasPath);

                                GUILayout.Space(-30);

                                GUILayout.Box(attachmentSprite, GUILayout.Width(25), GUILayout.Height(25));
                            }

                            Rect temp = GUILayoutUtility.GetLastRect();
                            if (temp != m_LastRect && m_IsMouseDown && temp.Contains(Event.current.mousePosition) && !m_LastRect.Contains(temp.center))
                            {
                                m_LastRect = temp;
                                if (m_SelectedPropId != 0 || m_SelectedAttachmentId == 0)
                                {
                                    m_PropDataMap[index] = m_SelectedPropId;
                                }

                                if (m_PropDataMap[index] != 0 || m_SelectedAttachmentId == 0)
                                {
                                    m_AttachmentDataMap[index] = m_SelectedAttachmentId;
                                }
                            }
                        }
                        GUILayout.Label((i + 1).ToString());
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void RecordMouseControl()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                m_IsMouseDown = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                m_IsMouseDown = false;
                m_LastRect = Rect.zero;
                SaveMapData();
                m_LoadMapData = false;
            }
        }

        private void SaveMapData()
        {
            if (s_MergeTheme == MergeTheme.None)
                return;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < BoardRow; i++)
            {
                for (int j = 0; j < BoardCol; j++)
                {
                    int index = i * BoardCol + j;

                    if (j == 0)
                    {
                        sb.Append("\t");
                        sb.Append(i.ToString());
                        sb.Append("\t");
                    }

                    sb.Append(m_PropDataMap[index]);
                    if (m_AttachmentDataMap[index] != 0)
                    {
                        sb.Append("/");
                        sb.Append(m_AttachmentDataMap[index]);
                    }

                    if (j < BoardCol - 1)
                    {
                        sb.Append("\t");
                    }
                }

                if (i < BoardRow - 1)
                {
                    sb.Append("\r\n");
                }
            }

            StreamWriter sw = new StreamWriter(PropDistributedMapPath);
            sw.Write(sb.ToString());
            sw.Close();

            AssetDatabase.Refresh();

            Debug.Log("Save PropDistributedMap Success");
        }

        private void LoadMapData(bool check)
        {
            TextAsset mapTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PropDistributedMapPath);

            int position = 0;
            string dataRowString = null;
            string lastRowString = null;
            int lineCount = 0;
            int columnCount = 0;
            while ((dataRowString = mapTextAsset.text.ReadLine(ref position)) != null)
            {
                if (dataRowString[0] == '#')
                {
                    continue;
                }

                lastRowString = dataRowString;
                lineCount++;
            }

            if (lastRowString != null)
            {
                string[] columnStrings = lastRowString.Split('\t');
                columnCount = columnStrings.Length - 2;
            }

            if (check && (lineCount != BoardRow || columnCount != BoardCol))  
            {
                BoardRow = lineCount;
                BoardCol = columnCount;
                m_PropDataMap = new int[BoardRow * BoardCol];
                m_AttachmentDataMap = new int[BoardRow * BoardCol];
                LoadMapData(true);
                return;
            }

            position = 0;
            dataRowString = null;
            while ((dataRowString = mapTextAsset.text.ReadLine(ref position)) != null)
            {
                if (dataRowString[0] == '#')
                {
                    continue;
                }

                DRPropDistributedMap drMap = new DRPropDistributedMap();
                if (drMap.ParseDataRow(dataRowString))
                {
                    for (int i = 0; i < drMap.PropsId.Length; i++)
                    {
                        int index = drMap.Id * BoardCol + i;
                        m_PropDataMap[index] = drMap.PropsId[i];
                        m_AttachmentDataMap[index] = drMap.AttachmentsId[i];
                    }
                }
                else
                {
                    Debug.LogError("PropDistributedMapEditor Parse Map Data Error");
                }
            }

            Debug.Log("Load PropDistributedMap Success");
        }

        private Texture2D LoadPropEditorSprite(int id, string spriteAtlasPath)
        {
            for (int i = 0; i < m_PropDataTable.Count; i++)
            {
                if (m_PropDataTable[i].Id == id)
                {
                    return LoadEditorSprite(m_PropDataTable[i].AssetName, spriteAtlasPath);
                }
            }

            return null;
        }

        private Texture2D LoadAttachmentEditorSprite(int id, string spriteAtlasPath)
        {
            for (int i = 0; i < m_AttachmentDataTable.Count; i++)
            {
                if (m_AttachmentDataTable[i].Id == id)
                {
                    return LoadEditorSprite(m_AttachmentDataTable[i].AssetName, spriteAtlasPath);
                }
            }

            return null;
        }

        private Texture2D LoadEditorSprite(string assetName, string spriteAtlasPath)
        {
            if (m_SpriteDic.TryGetValue(assetName, out Texture2D texture))
            {
                return texture;
            }

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasPath);
            texture = atlas.GetSprite(assetName).texture;
            m_SpriteDic.Add(assetName, texture);
            return texture;
        }
    }
}