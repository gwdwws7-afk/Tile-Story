using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class DebuggerComponent : GameFrameworkComponent
{
    [Serializable]
    private sealed class ConsoleWindow : IDebuggerWindow
    {
        private readonly Queue<LogNode> m_LogNodes = new Queue<LogNode>();

        private Vector2 m_LogScrollPosition = Vector2.zero;
        private Vector2 m_StackScrollPosition = Vector2.zero;
        private int m_InfoCount = 0;
        private int m_WarningCount = 0;
        private int m_ErrorCount = 0;
        private int m_FatalCount = 0;
        private LogNode m_SelectedNode = null;

        [SerializeField]
        private bool m_LockScroll = true;

        [SerializeField]
        private int maxLine = 100;

        [SerializeField]
        private bool m_InfoFilter = true;

        [SerializeField]
        private bool m_WarningFilter = true;

        [SerializeField]
        private bool m_ErrorFilter = true;

        [SerializeField]
        private bool m_FatalFilter = true;

        [SerializeField]
        private Color32 m_InfoColor = Color.white;

        [SerializeField]
        private Color32 m_WarningColor = Color.yellow;

        [SerializeField]
        private Color32 m_ErrorColor = Color.red;

        [SerializeField]
        private Color32 m_FatalColor = new Color(0.7f, 0.2f, 0.2f);

        public bool LockScroll { get => m_LockScroll; set => m_LockScroll = value; }
        public int MaxLine { get => maxLine; set => maxLine = value; }
        public bool InfoFilter { get => m_InfoFilter; set => m_InfoFilter = value; }
        public bool WarningFilter { get => m_WarningFilter; set => m_WarningFilter = value; }
        public bool ErrorFilter { get => m_ErrorFilter; set => m_ErrorFilter = value; }
        public bool FatalFilter { get => m_FatalFilter; set => m_FatalFilter = value; }
        public Color32 InfoColor { get => m_InfoColor; set => m_InfoColor = value; }
        public Color32 WarningColor { get => m_WarningColor; set => m_WarningColor = value; }
        public Color32 ErrorColor { get => m_ErrorColor; set => m_ErrorColor = value; }
        public Color32 FatalColor { get => m_FatalColor; set => m_FatalColor = value; }
        public int InfoCount { get => m_InfoCount; }
        public int WarningCount { get => m_WarningCount; }
        public int ErrorCount { get => m_ErrorCount; }
        public int FatalCount { get => m_FatalCount; }

        public void Initialize(params object[] args)
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Clear();
        }

        public void OnDraw()
        {
            RefreshCount();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Clear All", GUILayout.Width(100f))) 
                {
                    Clear();
                }
                m_LockScroll = GUILayout.Toggle(m_LockScroll, "Lock Scroll", GUILayout.Width(90f));
                GUILayout.FlexibleSpace();
                m_InfoFilter = GUILayout.Toggle(m_InfoFilter, string.Format("Info({0})", m_InfoCount), GUILayout.Width(90f));
                m_WarningFilter = GUILayout.Toggle(m_WarningFilter, string.Format("Warning({0})", m_WarningCount), GUILayout.Width(90f));
                m_ErrorFilter = GUILayout.Toggle(m_ErrorFilter, string.Format("Error({0})", m_ErrorCount), GUILayout.Width(90f));
                m_FatalFilter = GUILayout.Toggle(m_FatalFilter, string.Format("Fatal({0})", m_FatalCount), GUILayout.Width(90f));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                if (m_LockScroll)
                {
                    m_LogScrollPosition.y = float.MaxValue;
                }

                m_LogScrollPosition = GUILayout.BeginScrollView(m_LogScrollPosition);
                {
                    bool selected = false;
                    foreach (var logNode in m_LogNodes)
                    {
                        switch (logNode.LogType)
                        {
                            case LogType.Log:
                                if (!m_InfoFilter)
                                {
                                    continue;
                                }
                                break;
                            case LogType.Warning:
                                if (!m_WarningFilter)
                                {
                                    continue;
                                }
                                break;
                            case LogType.Error:
                                if (!m_ErrorFilter)
                                {
                                    continue;
                                }
                                break;
                            case LogType.Exception:
                                if (!m_FatalFilter)
                                {
                                    continue;
                                }
                                break;
                        }
                        if (GUILayout.Toggle(m_SelectedNode == logNode, GetLogString(logNode))) 
                        {
                            selected = true;
                            if (m_SelectedNode != logNode)
                            {
                                m_SelectedNode = logNode;
                                m_StackScrollPosition = Vector2.zero;
                            }
                        }
                    }
                    if (!selected)
                    {
                        m_SelectedNode = null;
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                m_StackScrollPosition = GUILayout.BeginScrollView(m_StackScrollPosition, GUILayout.Height(100f));
                {
                    if (m_SelectedNode != null)
                    {
                        Color color = GetLogStringColor(m_SelectedNode.LogType);
                        if (GUILayout.Button(string.Format("<color=#{0:2:X}{1:2:X}{2:2:X}{3:2:X}><b>{4}</b></color>{6}{6}{5}", color.r, color.g, color.b, color.a,
                            m_SelectedNode.LogMessage, m_SelectedNode.StackTrack, Environment.NewLine),"label"))
                        {
                            CopyToClipboard(string.Format("{0}{2}{2}{1}", m_SelectedNode.LogMessage, m_SelectedNode.StackTrack, Environment.NewLine));
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        private void Clear()
        {
            m_LogNodes.Clear();
        }

        public void RefreshCount()
        {
            m_InfoCount = 0;
            m_WarningCount = 0;
            m_ErrorCount = 0;
            m_FatalCount = 0;
            foreach (LogNode logNode in m_LogNodes)
            {
                switch (logNode.LogType)
                {
                    case LogType.Log:
                        m_InfoCount++;
                        break;

                    case LogType.Warning:
                        m_WarningCount++;
                        break;

                    case LogType.Error:
                        m_ErrorCount++;
                        break;

                    case LogType.Exception:
                        m_FatalCount++;
                        break;
                }
            }
        }

        private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
        {
            if (logType == LogType.Assert)
            {
                logType = LogType.Error;
            }

            m_LogNodes.Enqueue(LogNode.Create(logType, logMessage, stackTrace));
            while (m_LogNodes.Count > maxLine)
            {
                m_LogNodes.Dequeue();
                //ReferencePool.Release(m_LogNodes.Dequeue());
            }
        }

        private string GetLogString(LogNode logNode)
        {
            Color32 color = GetLogStringColor(logNode.LogType);
            return string.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}>[{4:HH:mm:ss.fff}][{5}] {6}</color>", color.r, color.g, color.b, color.a, logNode.LogTime.ToLocalTime(), logNode.LogFrameCount, logNode.LogMessage);
        }

        internal Color32 GetLogStringColor(LogType logType)
        {
            Color32 color = Color.white;
            switch (logType)
            {
                case LogType.Log:
                    color = m_InfoColor;
                    break;

                case LogType.Warning:
                    color = m_WarningColor;
                    break;

                case LogType.Error:
                    color = m_ErrorColor;
                    break;

                case LogType.Exception:
                    color = m_FatalColor;
                    break;
            }

            return color;
        }
    }
}
