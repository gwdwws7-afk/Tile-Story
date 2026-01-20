using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 调试器组件
/// </summary>
public sealed partial class DebuggerComponent : GameFrameworkComponent
{
    /// <summary>
    /// 默认调试器窗口大小。
    /// </summary>
    internal static Rect DefaultWindowRect => new Rect(100f,200f, Screen.width-40, Screen.height*0.8f);

    /// <summary>
    /// 默认调试器窗口缩放比例
    /// </summary>
    internal static readonly float DefaultWindowScale = 1f;

    private static TextEditor s_TextEditor;
    private IDebuggerManager m_DebuggerManager = null;

    private Rect m_DragRect = new Rect(0f, 0f, float.MaxValue, 25f);
    private Rect m_IconRect = new Rect(100f, 200f, 150f, 150f);
    private Rect m_WindowRect = DefaultWindowRect;
    private float m_WindowScale = DefaultWindowScale;

    [SerializeField]
    private GUISkin m_Skin = null;

    [SerializeField]
    private DebuggerActiveWindowType m_ActiveWindow = DebuggerActiveWindowType.AlwaysOpen;

    [SerializeField]
    private bool m_ShowFullWindow = false;

    [SerializeField]
    private ConsoleWindow m_ConsoleWindow = new ConsoleWindow();

    private SystemInformationWindow m_SystemInformationWindow = new SystemInformationWindow();
    private EnvironmentInformationWindow m_EnvironmentInformationWindow = new EnvironmentInformationWindow();
    private OperationsWindow m_OperationsWindow = new OperationsWindow();

    private FpsCounter m_FpsCounter = null;

    /// <summary>
    /// 获取或设置调试器窗口是否激活。
    /// </summary>
    public bool ActiveWindow
    {
        get
        {
            return m_DebuggerManager.ActiveWindow;
        }
        set
        {
            m_DebuggerManager.ActiveWindow = value;
            enabled = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        s_TextEditor = new TextEditor();

        m_DebuggerManager = GameFrameworkEntry.GetModule<DebuggerManager>();
        if (m_DebuggerManager == null)
        {
            Log.Fatal("Debugger manager is invalid");
            return;
        }

        m_FpsCounter = new FpsCounter(0.5f);
    }

    private void Start()
    {
        RegisterDebuggerWindow("Console", m_ConsoleWindow);
        RegisterDebuggerWindow("Information/System", m_SystemInformationWindow);
        RegisterDebuggerWindow("Information/Environment", m_EnvironmentInformationWindow);
        RegisterDebuggerWindow("Other/Operations", m_OperationsWindow);

        switch (m_ActiveWindow)
        {
            case DebuggerActiveWindowType.AlwaysOpen:
                ActiveWindow = true;
                break;
            case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                ActiveWindow = Debug.isDebugBuild;
                break;
            case DebuggerActiveWindowType.OnlyOpenInEditor:
                ActiveWindow = Application.isEditor;
                break;
            case DebuggerActiveWindowType.AlwaysClose:
                ActiveWindow = false;
                break;
        }
    }

    private void Update()
    {
        m_FpsCounter.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    private void OnGUI()
    {
        if (m_DebuggerManager == null || !m_DebuggerManager.ActiveWindow)
        {
            return;
        }

        GUISkin cachedSkin = GUI.skin;
        Matrix4x4 cachedMatrix = GUI.matrix;

        GUI.skin = m_Skin;
        GUI.matrix = Matrix4x4.Scale(new Vector3(m_WindowScale, m_WindowScale, 1f));

        if (m_ShowFullWindow)
        {
            m_WindowRect = GUILayout.Window(0, m_WindowRect, DrawWindow, "<b>GAME FRAMEWORK DEBUGGER</b>");
        }
        else
        {
            m_IconRect = GUILayout.Window(0, m_IconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
        }

        GUI.matrix = cachedMatrix;
        GUI.skin = cachedSkin;
    }

    /// <summary>
    /// 注册调试器窗口。
    /// </summary>
    /// <param name="path">调试器窗口路径。</param>
    /// <param name="debuggerWindow">要注册的调试器窗口。</param>
    /// <param name="args">初始化调试器窗口参数。</param>
    public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
    {
        m_DebuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
    }

    /// <summary>
    /// 解除注册调试器窗口。
    /// </summary>
    /// <param name="path">调试器窗口路径。</param>
    /// <returns>是否解除注册调试器窗口成功。</returns>
    public bool UnregisterDebuggerWindow(string path)
    {
        return m_DebuggerManager.UnregisterDebuggerWindow(path);
    }

    /// <summary>
    /// 获取调试器窗口。
    /// </summary>
    /// <param name="path">调试器窗口路径。</param>
    /// <returns>要获取的调试器窗口。</returns>
    public IDebuggerWindow GetDebuggerWindow(string path)
    {
        return m_DebuggerManager.GetDebuggerWindow(path);
    }

    /// <summary>
    /// 选中调试器窗口。
    /// </summary>
    /// <param name="path">调试器窗口路径。</param>
    /// <returns>是否成功选中调试器窗口。</returns>
    public bool SelectDebuggerWindow(string path)
    {
        return m_DebuggerManager.SelectDebuggerWindow(path);
    }

    /// <summary>
    /// 还原调试器窗口布局。
    /// </summary>
    public void ResetLayout()
    {
        m_IconRect = new Rect(100f, 200f, 150f, 150f);
        m_WindowRect = DefaultWindowRect;
        m_WindowScale = DefaultWindowScale;
    }

    private void DrawWindow(int windowId)
    {
        GUI.DragWindow(m_DragRect);
        DrawDebuggerWindowGroup(m_DebuggerManager.DebuggerWindowRoot);
    }

    private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
    {
        if (debuggerWindowGroup == null)
        {
            return;
        }

        List<string> names = new List<string>();
        string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
        for (int i = 0; i < debuggerWindowNames.Length; i++)
        {
            names.Add(string.Format("<b>{0}</b>", debuggerWindowNames[i]));
        }

        if (debuggerWindowGroup == m_DebuggerManager.DebuggerWindowRoot)
        {
            names.Add("<b>Close</b>");
        }

        int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
        if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
        {
            m_ShowFullWindow = false;
            return;
        }

        if (debuggerWindowGroup.SelectedWindow == null)
        {
            return;
        }

        if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
        {
            debuggerWindowGroup.SelectedWindow.OnLeave();
            debuggerWindowGroup.SelectedIndex = toolbarIndex;
            debuggerWindowGroup.SelectedWindow.OnEnter();
        }

        IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
        if (subDebuggerWindowGroup != null)
        {
            DrawDebuggerWindowGroup(subDebuggerWindowGroup);
        }

        debuggerWindowGroup.SelectedWindow.OnDraw();
    }

    private void DrawDebuggerWindowIcon(int windowId)
    {
        GUI.DragWindow(m_DragRect);
        GUILayout.Space(5);
        Color32 color = Color.white;

        string title = string.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>FPS: {4:F2}</b></color>", color.r, color.g, color.b, color.a, m_FpsCounter.CurrentFps);
        if (GUILayout.Button(title, GUILayout.Width(150f), GUILayout.Height(100f)))
        {
            m_ShowFullWindow = true;
        }
    }

    private static void CopyToClipboard(string content)
    {
        s_TextEditor.text = content;
        s_TextEditor.OnFocus();
        s_TextEditor.Copy();
        s_TextEditor.text = string.Empty;
    }
}
