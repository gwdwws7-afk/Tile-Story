// 文件：Assets/Editor/AndroidDependencyReporterFull.cs
// 说明：将此文件放到项目的 Assets/Editor/ 下。
// 依赖：Newtonsoft.Json（Unity Package Manager: com.unity.nuget.newtonsoft-json）

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public class AndroidDependencyReporterFull : EditorWindow
{
    // UI state
    private Vector2 leftScroll;
    private Vector2 rightScroll;
    private string[] reportFiles;
    private int selectedIndex = -1;
    private DependencyNode[] currentDeps;
    private string searchText = "";

    // config / paths
    private string resolverXmlPath;
    private string gradleExePath; // user configured or auto-found
    private string tempGradlePath;

    // auto-check state
    private DateTime lastResolverWriteTime = DateTime.MinValue;
    private bool autoModeEnabled = true; // whether to auto-run after resolver updates
    private bool autoSaveJson = true; // auto save json on auto-run
    private bool manualModeNoSave = false; // when generating manually, do not auto-save unless user clicks save

    // UI toggles
    private bool showOnlyConflicts = false;

    // Menu
    [MenuItem("Tools/Android Dependency Reporter Full/Get Android Dependency Window")]
    public static void ShowWindow()
    {
        var w = GetWindow<AndroidDependencyReporterFull>("Android Dependency Reporter Full");
        w.Initialize();
    }
    
    public static void GenerateJsonSilent()
    {
        var w = CreateInstance<AndroidDependencyReporterFull>();
        w.GenerateAndShow(true);
    }

    private void Initialize()
    {
        resolverXmlPath = FindResolverXmlPath();
        gradleExePath = EditorPrefs.GetString("DependencyReporter_GradlePath", "");
        RefreshFileList();
        // start polling for changes
        EditorApplication.update -= PollResolverFile;
        EditorApplication.update += PollResolverFile;
    }

    private void OnDisable()
    {
        EditorApplication.update -= PollResolverFile;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        DrawLeftPanel();

        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(340));
        EditorGUILayout.LabelField("Settings & Actions", EditorStyles.boldLabel);

        // Resolver XML path (auto found)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Resolver XML:", GUILayout.Width(80));
        EditorGUILayout.LabelField(resolverXmlPath ?? "Not found", GUILayout.MaxWidth(200));
        if (GUILayout.Button("Select...", GUILayout.Width(50)))
        {
            string path = EditorUtility.OpenFilePanel("Select AndroidResolverDependencies.xml", Application.dataPath, "xml");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                resolverXmlPath = path;
                UnityEngine.Debug.Log("[DependencyReporter] Selected XML: " + resolverXmlPath);
                // update lastWriteTime to avoid immediate auto-run
                TryUpdateLastWriteTime();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Gradle executable
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Gradle:", GUILayout.Width(80));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(gradleExePath) ? "Not set" : gradleExePath, GUILayout.MaxWidth(200));
        if (GUILayout.Button("Set...", GUILayout.Width(50)))
        {
            string ext = Application.platform == RuntimePlatform.WindowsEditor ? "bat" : "";
            string p = EditorUtility.OpenFilePanel("Select gradle executable", "", ext);
            if (!string.IsNullOrEmpty(p) && File.Exists(p))
            {
                gradleExePath = p;
                EditorPrefs.SetString("DependencyReporter_GradlePath", gradleExePath);
                UnityEngine.Debug.Log("[DependencyReporter] Gradle path set: " + gradleExePath);
            }
        }
        if (GUILayout.Button("AutoFind", GUILayout.Width(60)))
        {
            gradleExePath = AutoFindGradleExecutable();
            if (!string.IsNullOrEmpty(gradleExePath))
            {
                EditorPrefs.SetString("DependencyReporter_GradlePath", gradleExePath);
                UnityEngine.Debug.Log("[DependencyReporter] Gradle auto-found: " + gradleExePath);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[DependencyReporter] Gradle not found automatically.");
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Auto mode toggles
        EditorGUILayout.BeginHorizontal();
        autoModeEnabled = EditorGUILayout.ToggleLeft("Auto-run after Resolver updates", autoModeEnabled, GUILayout.Width(220));
        autoSaveJson = EditorGUILayout.ToggleLeft("Auto-save JSON on auto-run", autoSaveJson, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Generate buttons
        if (GUILayout.Button("Generate Dependency Tree (Manual)"))
        {
            manualModeNoSave = true;
            GenerateAndShow(false);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate & Save"))
        {
            manualModeNoSave = false;
            GenerateAndShow(true);
        }
        if (GUILayout.Button("Open Reports Folder"))
        {
            string dir = Path.Combine(Application.dataPath, "DependencyReports");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            EditorUtility.RevealInFinder(dir);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Reports list
        EditorGUILayout.LabelField("Saved Reports", EditorStyles.boldLabel);
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll, GUILayout.Height(240));
        if (reportFiles != null)
        {
            for (int i = 0; i < reportFiles.Length; i++)
            {
                string fn = Path.GetFileName(reportFiles[i]);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(fn, (i == selectedIndex) ? EditorStyles.toolbarButton : EditorStyles.miniButton))
                {
                    selectedIndex = i;
                    LoadJson(reportFiles[i]);
                    // load into view
                }
                if (GUILayout.Button("Compare", GUILayout.Width(70)))
                {
                    string leftFile = reportFiles[i];
                    string rightFile = EditorUtility.OpenFilePanel("Select report to compare with", Path.GetDirectoryName(leftFile), "json");
                    if (!string.IsNullOrEmpty(rightFile))
                        DependencyCompareWindow.ShowWindow(leftFile, rightFile);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(6);

        // Display controls
        showOnlyConflicts = EditorGUILayout.ToggleLeft("Show only conflicts / failures", showOnlyConflicts);
        if (GUILayout.Button("Refresh Report List")) RefreshFileList();

        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Dependency Tree Viewer", EditorStyles.boldLabel);
        searchText = EditorGUILayout.TextField("Search", searchText);

        // If manual no-save mode and we have currentDeps, give Save button
        if (manualModeNoSave && currentDeps != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save / Overwrite JSON", GUILayout.Width(180)))
            {
                SaveCurrentDepsToJson(currentDeps);
                RefreshFileList();
            }
            if (GUILayout.Button("Discard", GUILayout.Width(100)))
            {
                // reload from selected file if any
                if (selectedIndex >= 0 && selectedIndex < reportFiles.Length) LoadJson(reportFiles[selectedIndex]);
                else currentDeps = null;
                manualModeNoSave = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
        if (currentDeps == null)
        {
            EditorGUILayout.HelpBox("No dependency tree loaded. Generate or open a JSON report.", MessageType.Info);
        }
        else
        {
            foreach (var root in currentDeps)
            {
                DrawNodeRecursive(root, 0);
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    // Draw node with color marking for conflict/failure
    private void DrawNodeRecursive(DependencyNode node, int indent)
    {
        if (node == null) return;
        if (showOnlyConflicts && !node.IsConflict && !node.IsFailed && !node.HasConflictDescendant())
            return;

        if (!string.IsNullOrEmpty(searchText))
        {
            // perform simple contains search across name/version
            if (!(node.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                  (node.Version != null && node.Version.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                  (node.ResolvedVersion != null && node.ResolvedVersion.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)))
            {
                // skip node unless descendant matches
                if (!node.HasDescendantMatching(searchText))
                    return;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent * 14);

        // background color rectangle
        Rect r = GUILayoutUtility.GetRect(10, 20, GUILayout.ExpandWidth(true));
        Color bg = Color.clear;
        if (node.IsFailed) bg = new Color(1f, 0.5f, 0.5f, 0.25f); // red-ish
        else if (node.IsConflict) bg = new Color(1f, 1f, 0.5f, 0.25f); // yellow-ish
        if (bg.a > 0f)
        {
            EditorGUI.DrawRect(r, bg);
        }

        GUIStyle labelStyle = new GUIStyle(EditorStyles.foldout) { richText = false };
        node.foldout = EditorGUILayout.Foldout(node.foldout, $"{node.Name}  <{node.Version}> → {node.ResolvedVersion}", true, labelStyle);
        EditorGUILayout.EndHorizontal();

        if (node.foldout && node.Children != null)
        {
            foreach (var c in node.Children)
                DrawNodeRecursive(c, indent + 1);
        }
    }

    // ------------------------
    // Polling for resolver changes
    // ------------------------
    private void PollResolverFile()
    {
        if (!autoModeEnabled) return;
        string xml = resolverXmlPath ?? FindResolverXmlPath();
        if (string.IsNullOrEmpty(xml) || !File.Exists(xml)) return;

        DateTime writeTime = File.GetLastWriteTimeUtc(xml);
        
        UnityEngine.Debug.Log("[DependencyReporter] Start...");
        if (writeTime > lastResolverWriteTime)
        {
            // Updated — perform auto-run
            lastResolverWriteTime = writeTime;
            UnityEngine.Debug.Log("[DependencyReporter] Resolver XML changed. Auto-generating dependency tree...");
            // Run generation in editor context
            GenerateAndShow(autoSaveJson);
        }
    }

    private void TryUpdateLastWriteTime()
    {
        if (!string.IsNullOrEmpty(resolverXmlPath) && File.Exists(resolverXmlPath))
            lastResolverWriteTime = File.GetLastWriteTimeUtc(resolverXmlPath);
    }

    // ------------------------
    // Core flows
    // ------------------------
    public void GenerateAndShow(bool saveJson)
    {
        // Ensure resolver path
        if (string.IsNullOrEmpty(resolverXmlPath)) resolverXmlPath = FindResolverXmlPath();
        if (string.IsNullOrEmpty(resolverXmlPath) || !File.Exists(resolverXmlPath))
        {
            UnityEngine.Debug.LogError("[DependencyReporter] AndroidResolverDependencies.xml not found. Please select it or run resolver.");
            return;
        }

        // ensure gradle path
        if (string.IsNullOrEmpty(gradleExePath)) gradleExePath = EditorPrefs.GetString("DependencyReporter_GradlePath", "");
        if (string.IsNullOrEmpty(gradleExePath) || !File.Exists(gradleExePath))
        {
            // attempt autofind
            gradleExePath = AutoFindGradleExecutable();
            if (!string.IsNullOrEmpty(gradleExePath)) EditorPrefs.SetString("DependencyReporter_GradlePath", gradleExePath);
        }

        if (string.IsNullOrEmpty(gradleExePath) || !File.Exists(gradleExePath))
        {
            UnityEngine.Debug.LogError("[DependencyReporter] Gradle executable not found. Please set it with 'Set...' or install Gradle.");
            return;
        }

        // read top-level packages
        var topDeps = ReadPackagesFromResolverXml(resolverXmlPath);
        if (topDeps == null || topDeps.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[DependencyReporter] No top-level packages found in resolver XML.");
            return;
        }

        // setup temp gradle project
        tempGradlePath = Path.Combine(Application.dataPath, "../TempGradle_DependencyReporter");
        if (Directory.Exists(tempGradlePath)) Directory.Delete(tempGradlePath, true);
        Directory.CreateDirectory(tempGradlePath);

        GenerateBuildGradle(tempGradlePath, topDeps);
        GenerateSettingsGradle(tempGradlePath);

        // run gradle dependencies
        string output, error;
        bool ok = RunGradleDependencies(gradleExePath, tempGradlePath, out output, out error);
        if (!ok)
        {
            UnityEngine.Debug.LogError("[DependencyReporter] Gradle failed: " + error);
            // cleanup
            TryDeleteTempGradle();
            return;
        }

        // parse gradle output into tree
        var roots = ParseGradleOutputToTree(output);

        // 根节点去重
        roots = DeduplicateRootNodes(roots);

        // mark conflict/failed flags
        MarkConflictsAndFailures(roots);

        currentDeps = roots;
        // save if required
        if (saveJson)
        {
            SaveCurrentDepsToJson(currentDeps);
            RefreshFileList();
        }

        // open window focus
        Focus();
        // cleanup temp
        TryDeleteTempGradle();
        manualModeNoSave = !saveJson;
    }

    private void TryDeleteTempGradle()
    {
        try
        {
            if (!string.IsNullOrEmpty(tempGradlePath) && Directory.Exists(tempGradlePath))
                Directory.Delete(tempGradlePath, true);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("[DependencyReporter] Failed to delete temp gradle: " + e.Message);
        }
    }

    // ------------------------
    // Finders & Readers
    // ------------------------
    // Find ProjectSettings/AndroidResolverDependencies.xml relative to Assets
    private string FindResolverXmlPath()
    {
        string assets = Application.dataPath; // .../Project/Assets
        string projectRoot = Path.GetDirectoryName(assets);
        string candidate = Path.Combine(projectRoot, "ProjectSettings", "AndroidResolverDependencies.xml");
        if (File.Exists(candidate)) return candidate;
        // sometimes EDM writes to ProjectSettings/ExternalDependencyManager/AndroidResolverDependencies.xml or similar
        string alt = Path.Combine(projectRoot, "ProjectSettings", "ExternalDependencyManager", "AndroidResolverDependencies.xml");
        if (File.Exists(alt)) return alt;
        // search project for any file with that name (expensive but fallback)
        try
        {
            var files = Directory.GetFiles(projectRoot, "AndroidResolverDependencies.xml", SearchOption.AllDirectories);
            if (files.Length > 0) return files[0];
        }
        catch { }
        return null;
    }

    // Try to auto-find gradle: check EditorApplication.applicationContentsPath candidate, then PATH using 'where'/'which'
    private string AutoFindGradleExecutable()
    {
        // 1. check Unity Editor contents typical path
        try
        {
            string editorContents = EditorApplication.applicationContentsPath;
            // candidate paths
            string p1 = Path.Combine(editorContents, "PlaybackEngines", "AndroidPlayer", "Tools", "gradle", "bin", "gradle");
            string p2 = Path.Combine(editorContents, "PlaybackEngines", "AndroidPlayer", "Tools", "gradle", "gradle");
            string p3 = p1 + (Application.platform == RuntimePlatform.WindowsEditor ? ".bat" : "");
            string p4 = p2 + (Application.platform == RuntimePlatform.WindowsEditor ? ".bat" : "");
            if (File.Exists(p3)) return p3;
            if (File.Exists(p4)) return p4;
        }
        catch { }

        // 2. search system PATH
        try
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                var psi = new System.Diagnostics.ProcessStartInfo("where", "gradle") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                var proc = System.Diagnostics.Process.Start(psi);
                string outp = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();
                if (!string.IsNullOrEmpty(outp))
                {
                    var first = outp.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (File.Exists(first)) return first;
                }
            }
            else
            {
                var psi = new System.Diagnostics.ProcessStartInfo("which", "gradle") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                var proc = System.Diagnostics.Process.Start(psi);
                string outp = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit();
                if (!string.IsNullOrEmpty(outp) && File.Exists(outp)) return outp;
            }
        }
        catch { }

        return null;
    }

    private List<DependencyNode> ReadPackagesFromResolverXml(string xmlPath)
    {
        var list = new List<DependencyNode>();
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(xmlPath);
            // support <packages><package>group:artifact:version</package></packages>
            var nodes = doc.SelectNodes("//packages/package");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (System.Xml.XmlNode node in nodes)
                {
                    string spec = node.InnerText.Trim();
                    if (string.IsNullOrEmpty(spec)) continue;
                    var parts = spec.Split(':');
                    if (parts.Length < 3) continue;
                    string group = parts[0], artifact = parts[1], version = parts[2];
                    list.Add(new DependencyNode { Name = $"{group}:{artifact}", Version = version, ResolvedVersion = version });
                }
                return list;
            }

            // fallback: try <dependency> or <androidPackage>... (older EDM)
            var depNodes = doc.SelectNodes("//dependencies/*[self::dependency or self::androidPackage]");
            if (depNodes != null)
            {
                foreach (System.Xml.XmlNode node in depNodes)
                {
                    if (node.Name == "androidPackage")
                    {
                        string spec = node.Attributes["spec"]?.Value;
                        if (!string.IsNullOrEmpty(spec))
                        {
                            var parts = spec.Split(':');
                            if (parts.Length >= 3)
                            {
                                list.Add(new DependencyNode { Name = $"{parts[0]}:{parts[1]}", Version = parts[2], ResolvedVersion = parts[2] });
                            }
                        }
                    }
                    else
                    {
                        string group = node.Attributes["group"]?.Value;
                        string artifact = node.Attributes["artifact"]?.Value;
                        string version = node.Attributes["version"]?.Value;
                        if (!string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(artifact) && !string.IsNullOrEmpty(version))
                        {
                            list.Add(new DependencyNode { Name = $"{group}:{artifact}", Version = version, ResolvedVersion = version });
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[DependencyReporter] Failed to read resolver XML: " + e.Message);
        }
        return list;
    }

    // ------------------------
    // Gradle execution & parsing
    // ------------------------
    private bool RunGradleDependencies(string gradleExe, string workingDir, out string stdOut, out string stdErr)
    {
        stdOut = "";
        stdErr = "";
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = gradleExe,
                Arguments = "dependencies --console=plain",
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi);
            stdOut = proc.StandardOutput.ReadToEnd();
            stdErr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (!string.IsNullOrEmpty(stdErr)) return false;
            return true;
        }
        catch (Exception ex)
        {
            stdErr = ex.Message;
            return false;
        }
    }

    // Basic but robust parsing of Gradle 'dependencies' plain output into a tree.
    // It recognizes lines like:
    // +--- group:artifact:version
    // |    \--- child:artifact:version -> resolvedVersion
    // \--- group:artifact:version -> resolved
    private DependencyNode[] ParseGradleOutputToTree(string output)
    {
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var roots = new List<DependencyNode>();
        var stack = new Stack<(DependencyNode node, int depth)>();

        // regex to match tokens like group:artifact:version and possibly "-> resolved"
        var tokenRegex = new Regex(@"(?<gav>[\w\-\._]+:[\w\-\._]+:[^\s\[\(]+)(?:\s*->\s*(?<resolved>[^\s\[\(]+))?");

        foreach (var raw in lines)
        {
            string line = raw;
            // skip lines that are not dependency lines (e.g., BUILD SUCCESSFUL)
            if (line.IndexOf("---") >= 0 || line.IndexOf("+---") >= 0 || line.IndexOf("\\---") >= 0 || line.TrimStart().StartsWith("|") || line.TrimStart().StartsWith("+") || line.TrimStart().StartsWith("\\"))
            {
                // compute depth by counting leading chars before token (count of '|' and spaces and +--- etc)
                int depth = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (c == '|' || c == ' ' || c == '+' || c == '\\' || c == '-' ) depth++;
                    else break;
                }

                var m = tokenRegex.Match(line);
                if (!m.Success) continue;

                string gav = m.Groups["gav"].Value.Trim();
                string resolved = m.Groups["resolved"]?.Value?.Trim();

                var gavParts = gav.Split(':');
                if (gavParts.Length < 3) continue;
                string name = $"{gavParts[0]}:{gavParts[1]}";
                string ver = gavParts[2];

                var node = new DependencyNode
                {
                    Name = name,
                    Version = ver,
                    ResolvedVersion = string.IsNullOrEmpty(resolved) ? ver : resolved,
                    Children = new List<DependencyNode>()
                };

                // if resolved differs -> it's a conflict resolution
                if (!string.IsNullOrEmpty(resolved) && resolved != ver) node.IsConflict = true;

                // attach node according to depth
                while (stack.Count > 0 && stack.Peek().depth >= depth)
                    stack.Pop();

                if (stack.Count == 0)
                {
                    roots.Add(node);
                }
                else
                {
                    var parent = stack.Peek().node;
                    if (parent.Children == null) parent.Children = new List<DependencyNode>();
                    parent.Children.Add(node);
                }

                stack.Push((node, depth));
            }
            else
            {
                // skip otherwise
            }
        }

        // collapse empty Children to null
        void Clean(List<DependencyNode> list)
        {
            foreach (var n in list)
            {
                if (n.Children != null && n.Children.Count == 0) n.Children = null;
                if (n.Children != null) Clean(n.Children);
            }
        }
        Clean(roots);

        return roots.ToArray();
    }
    
    // ------------------------
// 根节点去重
// ------------------------
    private DependencyNode[] DeduplicateRootNodes(DependencyNode[] roots)
    {
        if (roots == null || roots.Length <= 1) return roots;

        var map = new Dictionary<string, DependencyNode>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in roots)
        {
            if (!map.ContainsKey(node.Name))
            {
                map[node.Name] = node; // 首次出现保留
            }
            else
            {
                var existing = map[node.Name];
                // 可选策略：保留版本号更高的
                if (CompareVersion(node.ResolvedVersion, existing.ResolvedVersion) > 0)
                    map[node.Name] = node;
            }
        }

        return new List<DependencyNode>(map.Values).ToArray();
    }

// 简单版本比较函数，按数字点分割比较
    private int CompareVersion(string v1, string v2)
    {
        if (v1 == v2) return 0;
        if (string.IsNullOrEmpty(v1)) return -1;
        if (string.IsNullOrEmpty(v2)) return 1;

        var parts1 = v1.Split('.');
        var parts2 = v2.Split('.');
        int len = Math.Max(parts1.Length, parts2.Length);
        for (int i = 0; i < len; i++)
        {
            int n1 = i < parts1.Length ? int.TryParse(parts1[i], out var t1) ? t1 : 0 : 0;
            int n2 = i < parts2.Length ? int.TryParse(parts2[i], out var t2) ? t2 : 0 : 0;
            if (n1 != n2) return n1 - n2;
        }
        return 0;
    }


    // Mark conflict flags more broadly and failed nodes.
    private void MarkConflictsAndFailures(DependencyNode[] roots)
    {
        // detect failures in output was handled earlier by RunGradleDependencies.
        // But we also detect nodes with versions like "FAILED" or unresolved tokens.
        void Rec(DependencyNode n)
        {
            if (n == null) return;
            if (string.IsNullOrEmpty(n.ResolvedVersion) || n.ResolvedVersion.ToUpper().Contains("FAILED") || n.ResolvedVersion.Contains("UNRESOLVED"))
                n.IsFailed = true;

            // propagate conflict info: if child resolved != declared, mark child.IsConflict
            if (n.Children != null)
            {
                foreach (var c in n.Children)
                {
                    if (c != null && !string.IsNullOrEmpty(c.ResolvedVersion) && !string.IsNullOrEmpty(c.Version))
                    {
                        if (c.ResolvedVersion != c.Version) c.IsConflict = true;
                    }
                    Rec(c);
                }
            }
        }

        if (roots != null)
        {
            foreach (var r in roots) Rec(r);
        }
    }

    // ------------------------
    // Save & load JSON
    // ------------------------
    // Save file pattern: ProjectName_bundleVersion.json
    private void SaveCurrentDepsToJson(DependencyNode[] deps)
    {
        string projectName = PlayerSettings.productName.Replace(" ", "_");
        string version = PlayerSettings.bundleVersion;
        if (string.IsNullOrEmpty(version)) version = "v" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{projectName}_{version}.json";
        string dir = Path.Combine(Application.dataPath, "DependencyReports");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string filePath = Path.Combine(dir, fileName);

        string json = JsonConvert.SerializeObject(deps, Formatting.Indented);
        File.WriteAllText(filePath, json);
        UnityEngine.Debug.Log("[DependencyReporter] Saved dependency JSON: " + filePath);
    }

    private void SaveCurrentDepsToJson(List<DependencyNode> depsList)
    {
        SaveCurrentDepsToJson(depsList.ToArray());
    }

    private void SaveAsJson(DependencyNode[] deps)
    {
        SaveCurrentDepsToJson(deps);
    }

    private void RefreshFileList()
    {
        string dir = Path.Combine(Application.dataPath, "DependencyReports");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        reportFiles = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
        Array.Sort(reportFiles);
        Array.Reverse(reportFiles);
    }

    private void LoadJson(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            currentDeps = JsonConvert.DeserializeObject<DependencyNode[]>(json);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[DependencyReporter] Failed to load JSON: " + ex.Message);
        }
    }

    // ------------------------
    // Build Gradle helpers
    // ------------------------
    private void GenerateBuildGradle(string path, List<DependencyNode> topDeps)
    {
        string gradleFile = Path.Combine(path, "build.gradle");
        using (var sw = new StreamWriter(gradleFile))
        {
            sw.WriteLine("plugins { id 'java' }");
            sw.WriteLine("repositories { google(); mavenCentral() }");
            sw.WriteLine("dependencies {");
            foreach (var d in topDeps)
            {
                // use implementation with explicit version
                sw.WriteLine($"    implementation '{d.Name}:{d.Version}'");
            }
            sw.WriteLine("}");
        }
    }

    private void GenerateSettingsGradle(string path)
    {
        File.WriteAllText(Path.Combine(path, "settings.gradle"), "rootProject.name = 'TempGradleDependencyReport'");
    }

    // ------------------------
    // compare window launcher
    // ------------------------
    [MenuItem("Tools/Android Dependency Reporter Full/Compare Two Reports")]
    private static void OpenCompareWindowMenu()
    {
        string a = EditorUtility.OpenFilePanel("Select first report", Application.dataPath, "json");
        if (string.IsNullOrEmpty(a)) return;
        string b = EditorUtility.OpenFilePanel("Select second report", Path.GetDirectoryName(a), "json");
        if (string.IsNullOrEmpty(b)) return;
        DependencyCompareWindow.ShowWindow(a, b);
    }

    // ------------------------
    // Dependency Node data class
    // ------------------------
    [Serializable]
    public class DependencyNode
    {
        public string Name;
        public string Version;
        public string ResolvedVersion;
        public List<DependencyNode> Children;
        public bool foldout;

        // flags (not serialized)
        [JsonIgnore]
        public bool IsConflict = false;
        [JsonIgnore]
        public bool IsFailed = false;
        [NonSerialized] public int Indent;

        // Helper utils
        public bool HasConflictDescendant()
        {
            if (IsConflict || IsFailed) return true;
            if (Children == null) return false;
            foreach (var c in Children)
                if (c != null && c.HasConflictDescendant()) return true;
            return false;
        }

        public bool HasDescendantMatching(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return true;
            if ((Name != null && Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (Version != null && Version.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (ResolvedVersion != null && ResolvedVersion.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                return true;
            if (Children == null) return false;
            foreach (var c in Children)
                if (c != null && c.HasDescendantMatching(keyword)) return true;
            return false;
        }
    }
}

public class DependencyCompareWindow : EditorWindow
{
    private string leftPath;
    private string rightPath;

    private List<string> added = new List<string>();
    private List<string> removed = new List<string>();
    private List<string> versionChanged = new List<string>();

    private enum Tab { Added, Removed, VersionChanged }
    private Tab currentTab = Tab.Added;

    // 每个 Tab 的滚动状态
    private Vector2 scrollAdded = Vector2.zero;
    private Vector2 scrollRemoved = Vector2.zero;
    private Vector2 scrollVersionChanged = Vector2.zero;

    public static void ShowWindow(string left, string right)
    {
        var w = GetWindow<DependencyCompareWindow>("Dependency Compare");
        w.leftPath = left;
        w.rightPath = right;
        w.ComputeDiff();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Compare Reports", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Left: " + leftPath);
        EditorGUILayout.LabelField("Right: " + rightPath);

        EditorGUILayout.Space();

        // 三个菜单按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentTab == Tab.Added, $"Added ({added.Count})", EditorStyles.toolbarButton))
            currentTab = Tab.Added;
        if (GUILayout.Toggle(currentTab == Tab.Removed, $"Removed ({removed.Count})", EditorStyles.toolbarButton))
            currentTab = Tab.Removed;
        if (GUILayout.Toggle(currentTab == Tab.VersionChanged, $"Version Changed ({versionChanged.Count})", EditorStyles.toolbarButton))
            currentTab = Tab.VersionChanged;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 显示具体内容
        EditorGUILayout.BeginVertical();
        switch (currentTab)
        {
            case Tab.Added:
                scrollAdded = EditorGUILayout.BeginScrollView(scrollAdded);
                foreach (var s in added)
                    EditorGUILayout.LabelField(s, new GUIStyle() { normal = { textColor = Color.green } });
                EditorGUILayout.EndScrollView();
                break;

            case Tab.Removed:
                scrollRemoved = EditorGUILayout.BeginScrollView(scrollRemoved);
                foreach (var s in removed)
                    EditorGUILayout.LabelField(s, new GUIStyle() { normal = { textColor = Color.red } });
                EditorGUILayout.EndScrollView();
                break;

            case Tab.VersionChanged:
                scrollVersionChanged = EditorGUILayout.BeginScrollView(scrollVersionChanged);
                foreach (var s in versionChanged)
                {
                    var idx = s.LastIndexOf("->", StringComparison.Ordinal);
                    if (idx > 0)
                    {
                        string leftPart = s.Substring(0, idx).Trim();
                        string rightPart = s.Substring(idx + 2).Trim();
                        EditorGUILayout.LabelField($"{leftPart} → {rightPart}", new GUIStyle() { normal = { textColor = Color.yellow } });
                    }
                    else
                    {
                        EditorGUILayout.LabelField(s, new GUIStyle() { normal = { textColor = Color.yellow } });
                    }
                }
                EditorGUILayout.EndScrollView();
                break;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        if (GUILayout.Button("Recompute"))
            ComputeDiff();
    }

    private void ComputeDiff()
    {
        added.Clear(); removed.Clear(); versionChanged.Clear();
        if (string.IsNullOrEmpty(leftPath) || string.IsNullOrEmpty(rightPath)) return;

        try
        {
            var leftJson = File.ReadAllText(leftPath);
            var rightJson = File.ReadAllText(rightPath);

            var leftNodes = JsonConvert.DeserializeObject<AndroidDependencyReporterFull.DependencyNode[]>(leftJson);
            var rightNodes = JsonConvert.DeserializeObject<AndroidDependencyReporterFull.DependencyNode[]>(rightJson);

            var leftMap = FlattenToMap(leftNodes);
            var rightMap = FlattenToMap(rightNodes);

            foreach (var k in rightMap.Keys)
            {
                if (!leftMap.ContainsKey(k))
                {
                    added.Add($"{k} : {rightMap[k]}");
                }
                else
                {
                    string leftVersion = leftMap[k];
                    string rightVersion = rightMap[k];
                    if (leftVersion != rightVersion)
                        versionChanged.Add($"{k} : {leftVersion} -> {rightVersion}");
                }
            }

            foreach (var k in leftMap.Keys)
            {
                if (!rightMap.ContainsKey(k))
                    removed.Add($"{k} : {leftMap[k]}");
            }

            currentTab = Tab.Added;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[DependencyCompare] Failed to compare: " + ex.Message);
        }
    }

    private Dictionary<string, string> FlattenToMap(AndroidDependencyReporterFull.DependencyNode[] roots)
    {
        var map = new Dictionary<string, string>();
        if (roots == null) return map;

        var stack = new Stack<AndroidDependencyReporterFull.DependencyNode>();
        foreach (var r in roots) stack.Push(r);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (n != null)
            {
                map[n.Name] = n.ResolvedVersion ?? n.Version;
                if (n.Children != null)
                    foreach (var c in n.Children) stack.Push(c);
            }
        }
        return map;
    }
}


[InitializeOnLoad]
public static class AutoGenerateDepsPolling
{
    static string xmlPath;
    static DateTime lastWrite = DateTime.MinValue;
    static double lastPollTime = 0;

    static AutoGenerateDepsPolling()
    {
        string assets = Application.dataPath;
        string projectRoot = Path.GetDirectoryName(assets);
        xmlPath = Path.Combine(projectRoot, "ProjectSettings", "AndroidResolverDependencies.xml");

        if (File.Exists(xmlPath))
        {
            // 初始化时就记住当前时间，避免脚本刷新误触发
            lastWrite = File.GetLastWriteTimeUtc(xmlPath);
        }

        EditorApplication.update += PollResolverXml;
    }

    private static void PollResolverXml()
    {
        // 限制 3 秒轮询一次
        if (EditorApplication.timeSinceStartup - lastPollTime < 3.0)
            return;
        lastPollTime = EditorApplication.timeSinceStartup;

        if (!File.Exists(xmlPath))
            return;

        DateTime writeTime = File.GetLastWriteTimeUtc(xmlPath);
        if (writeTime > lastWrite)
        {
            lastWrite = writeTime;
            UnityEngine.Debug.Log("[DependencyReporter] XML changed. Auto-generating JSON...");

            EditorApplication.delayCall += () =>
            {
                // 如果窗口开着 → 刷新
                var window = EditorWindow.HasOpenInstances<AndroidDependencyReporterFull>()
                    ? EditorWindow.GetWindow<AndroidDependencyReporterFull>()
                    : null;

                if (window != null)
                {
                    window.GenerateAndShow(true);
                }
                else
                {
                    // 如果没开，就后台生成，不弹窗
                    AndroidDependencyReporterFull.GenerateJsonSilent();
                }
            };
        }
    }
}

