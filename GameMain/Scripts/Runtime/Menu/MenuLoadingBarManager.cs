using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu场景进度条管理器
/// </summary>
public sealed class MenuLoadingBarManager : MonoBehaviour
{
    public Image loadingFill;
    public Text progressText;

    private void Awake()
    {
        progressText.text = "Loading...0%";
        loadingFill.fillAmount = 0;
    }

    private void Update()
    {
        float percent = GameManager.Scene.GetPercentComplete();
        progressText.text = string.Format("Loading...{0}%", percent * 100);
        loadingFill.fillAmount = percent;
    }
}
