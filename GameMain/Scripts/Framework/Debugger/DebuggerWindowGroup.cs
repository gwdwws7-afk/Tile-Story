using System.Collections.Generic;

public sealed partial class DebuggerManager: GameFrameworkModule, IDebuggerManager
{
    /// <summary>
    /// 调试器窗口组。
    /// </summary>
    private sealed class DebuggerWindowGroup: IDebuggerWindowGroup
    {
        private readonly List<KeyValuePair<string, IDebuggerWindow>> debuggerWindows;
        private int selectedIndex;
        private string[] debuggerWindowNames;

        public DebuggerWindowGroup()
        {
            debuggerWindows = new List<KeyValuePair<string, IDebuggerWindow>>();
            selectedIndex = 0;
            debuggerWindowNames = null;
        }

        /// <summary>
        /// 获取调试器窗口数量。
        /// </summary>
        public int DebuggerWindowCount
        {
            get
            {
                return debuggerWindows.Count;
            }
        }

        /// <summary>
        /// 获取或设置当前选中的调试器窗口索引。
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
            }
        }

        /// <summary>
        /// 获取当前选中的调试器窗口。
        /// </summary>
        public IDebuggerWindow SelectedWindow
        {
            get
            {
                if (selectedIndex >= debuggerWindows.Count)
                {
                    return null;
                }

                return debuggerWindows[selectedIndex].Value;
            }
        }

        /// <summary>
        /// 初始化调试组。
        /// </summary>
        /// <param name="args">初始化调试组参数。</param>
        public void Initialize(params object[] args)
        {
        }

        /// <summary>
        /// 关闭调试组。
        /// </summary>
        public void Shutdown()
        {
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in debuggerWindows)
            {
                debuggerWindow.Value.Shutdown();
            }

            debuggerWindows.Clear();
        }

        /// <summary>
        /// 进入调试器窗口。
        /// </summary>
        public void OnEnter()
        {
            SelectedWindow.OnEnter();
        }

        /// <summary>
        /// 离开调试器窗口。
        /// </summary>
        public void OnLeave()
        {
            SelectedWindow.OnLeave();
        }

        /// <summary>
        /// 调试组轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            SelectedWindow.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 调试器窗口绘制。
        /// </summary>
        public void OnDraw()
        {
        }

        private void RefreshDebuggerWindowNames()
        {
            int index = 0;
            debuggerWindowNames = new string[debuggerWindows.Count];
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in debuggerWindows)
            {
                debuggerWindowNames[index++] = debuggerWindow.Key;
            }
        }

        /// <summary>
        /// 获取调试组的调试器窗口名称集合。
        /// </summary>
        public string[] GetDebuggerWindowNames()
        {
            return debuggerWindowNames;
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                return InternalGetDebuggerWindow(path);
            }

            string debuggerWindowGroupName = path.Substring(0, pos);
            string leftPath = path.Substring(pos + 1);
            DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
            if (debuggerWindowGroup == null)
            {
                return null;
            }

            return debuggerWindowGroup.GetDebuggerWindow(leftPath);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                return InternalSelectDebuggerWindow(path);
            }

            string debuggerWindowGroupName = path.Substring(0, pos);
            string leftPath = path.Substring(pos + 1);
            DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
            if (debuggerWindowGroup == null || !InternalSelectDebuggerWindow(debuggerWindowGroupName))
            {
                return false;
            }

            return debuggerWindowGroup.SelectDebuggerWindow(leftPath);
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.Exception("Path is invalid.");
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                if (InternalGetDebuggerWindow(path) != null)
                {
                    throw new System.Exception("Debugger window has been registered.");
                }

                debuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow));
                RefreshDebuggerWindowNames();
            }
            else
            {
                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null)
                {
                    if (InternalGetDebuggerWindow(debuggerWindowGroupName) != null)
                    {
                        throw new System.Exception("Debugger window has been registered, can not create debugger window group.");
                    }

                    debuggerWindowGroup = new DebuggerWindowGroup();
                    debuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(debuggerWindowGroupName, debuggerWindowGroup));
                    RefreshDebuggerWindowNames();
                }

                debuggerWindowGroup.RegisterDebuggerWindow(leftPath, debuggerWindow);
            }
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        public bool UnregisterDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                IDebuggerWindow debuggerWindow = InternalGetDebuggerWindow(path);
                bool result = debuggerWindows.Remove(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow));
                debuggerWindow.Shutdown();
                RefreshDebuggerWindowNames();
                return result;
            }

            string debuggerWindowGroupName = path.Substring(0, pos);
            string leftPath = path.Substring(pos + 1);
            DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
            if (debuggerWindowGroup == null)
            {
                return false;
            }

            return debuggerWindowGroup.UnregisterDebuggerWindow(leftPath);
        }

        private IDebuggerWindow InternalGetDebuggerWindow(string name)
        {
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in debuggerWindows)
            {
                if (debuggerWindow.Key == name)
                {
                    return debuggerWindow.Value;
                }
            }

            return null;
        }

        private bool InternalSelectDebuggerWindow(string name)
        {
            for (int i = 0; i < debuggerWindows.Count; i++)
            {
                if (debuggerWindows[i].Key == name)
                {
                    selectedIndex = i;
                    return true;
                }
            }

            return false;
        }
    }
}
