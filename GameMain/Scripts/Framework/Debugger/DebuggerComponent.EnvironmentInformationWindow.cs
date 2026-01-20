using UnityEngine;

public sealed partial class DebuggerComponent : GameFrameworkComponent
{
    /// <summary>
    /// 环境信息窗口
    /// </summary>
    private sealed class EnvironmentInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Environment Information</b>");
            GUILayout.BeginVertical("box");
            {
                //DrawItem("Company Name", Application.companyName);
                //DrawItem("Game Identifier", Application.identifier);
                DrawItem("Unity Version", Application.unityVersion);
                DrawItem("Platform", Application.platform.ToString());
                //DrawItem("Cloud Project Id", Application.cloudProjectId);
                //DrawItem("Build Guid", Application.buildGUID);
                //DrawItem("Background Loading Priority", Application.backgroundLoadingPriority.ToString());
                //DrawItem("Is Playing", Application.isPlaying.ToString());
                //DrawItem("Run In Background", Application.runInBackground.ToString());
                //DrawItem("Install Name", Application.installerName);
                //DrawItem("Install Mode", Application.installMode.ToString());
                //DrawItem("Sandbox Type", Application.sandboxType.ToString());
                //DrawItem("Is Mobile Platform", Application.isMobilePlatform.ToString());
                //DrawItem("Is Console Platform", Application.isConsolePlatform.ToString());
                DrawItem("Is Editor", Application.isEditor.ToString());
                //DrawItem("Is Debug Build", Debug.isDebugBuild.ToString());
                //DrawItem("Is Focused", Application.isFocused.ToString());
                //DrawItem("Is Batch Mode", Application.isBatchMode.ToString());
                DrawItem("Current Procedure", GameManager.Procedure.CurrentProcedure.ProcedureName);
                DrawItem("Current Process", GameManager.Process.CurrentProcessName == null ? string.Empty : GameManager.Process.CurrentProcessName);
                DrawItem("Current Process Count", GameManager.Process.Count.ToString());
            }
            GUILayout.EndVertical();
        }
    }
}
