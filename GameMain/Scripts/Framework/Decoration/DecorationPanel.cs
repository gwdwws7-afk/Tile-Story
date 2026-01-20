using Coffee.UIEffects;
using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// DecorationViewPanel上的 代表单个DecorationArea的小板子
/// </summary>
public class DecorationPanel : MonoBehaviour
{
    public DelayButton helpButton;
    public GameObject arrowGOOnButton;
    public DelayButton viewButton;
    public GameObject finishedRoot;
    public Image backgroundImage;
    public Image backgroundImageOldVersionMask;//GreyScale FillSprite for UnlockAnimation
    public SkeletonGraphic unlockLockSpine;//锁头解锁Spine
    public GameObject unlockLockSpineBackEffect;//锁头解锁Spine背后的光
    public GameObject unlockEffect;//带拖尾的横向移动的竖长光效果
    public TextMeshProUGUILocalize titleText;
    public TextMeshProUGUILocalize chapterDescriptionText;
    public TextMeshProUGUILocalize helpBtnText;
    public List<UIEffect> UIEffectToControlList;//部分UIEffect 不能随整体切换GrayScale/None 这里只把需要控制的拖进来

    public TextMeshProUGUI finishTimeStampText;

    //都是给当前区域完成时使用
    public Transform bodyScaleTrans;
    public Transform finishedRootScaleTrans;
    //public SkeletonGraphic finishedEffectSpine;
    public GameObject panelFinishEffect;

    private bool isCompleted;
    private List<AsyncOperationHandle> loadAssetList = new List<AsyncOperationHandle>();
    private string currentUseId;
    private int currentAreaId;
    private int waitingCount = 0;

    private bool isWaitingDecorationAreaLoading = false;
    private DecorationViewPanel parentPanelCache;

    [SerializeField] private DelayButton DownloadBtn;
    [SerializeField] private UIEffect DownloadBtn_UIEffect;
    [SerializeField] private TextMeshProUGUILocalize Download_Text;
    [SerializeField] private Slider Downloading_Slider;
    [SerializeField] private TextMeshProUGUI Downloading_Text;
    [SerializeField] private UIEffect ViewBtn_Effect;
    [SerializeField] private TextMeshProUGUILocalize ViewBtn_Text;
    [SerializeField] private GameObject DownloadArrow;

    private bool isHaveAsset = false;
    private string areaName;
    private bool isCurNeedDownloadArea;//是否是当前需要下载的

    public void InitializePanel(int areaId)
    {
        currentAreaId = areaId;
        int alteredAreaID = DecorationModel.Instance.GetAlteredDecorationAreaID(currentAreaId);
        titleText.SetTerm("Story.Chapter{0}");
        titleText.SetParameterValue("", areaId.ToString());

        //最后都会补一个 ComingSoon 的板子,它身上传入的areaId就是大于Constant.GameConfig.MaxDecorationArea的
        //这种情况下 除了标题显示下ChapterX 其他按理说都不用设置
        if (areaId > Constant.GameConfig.MaxDecorationArea)
        {
            return;
        }

        isHaveAsset = GameManager.Download.IsHaveAssetByAreaId(areaId);//判断是否有这个ab包

        var newId = DecorationModel.Instance.GetAlteredDecorationAreaID(areaId);//转换一下章节id编号
        areaName = DecorationModel.GetAreaNameById(newId);//这个ab包名称
        isCurNeedDownloadArea = GameManager.Download.GetCurNeedDownloadAreaId() == areaId;//是否是当前最大装修章节

        GameManager.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
        GameManager.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        GameManager.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);

        isCompleted = DecorationModel.Instance.CheckTargetAreaIsComplete(areaId);
        if (DecorationModel.Instance.Data.DecorationAreaID < areaId && !isCompleted)//未解锁
        {
            helpButton.gameObject.SetActive(true);
            helpButton.enabled = false;
            viewButton.gameObject.SetActive(false);
            finishedRoot.SetActive(false);

            titleText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
            helpBtnText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
            //魔法 这里似乎是有个异步加载时序问题 这里设置的Grey可能会失败 (被prefab上默认TextMeshProUGUILocalize (先下指令但后成功)加载的Material给替换掉)
            //这里先粗野的延时再set一下
            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                if (titleText.Target != null)
                    titleText.Refresh();
                if (helpBtnText.Target != null)
                    helpBtnText.Refresh();
            });
            chapterDescriptionText.GetComponent<TextMeshProUGUI>().color = Color.grey;
            chapterDescriptionText.SetTerm($"Story.Chapter{alteredAreaID}.Des_Before");
            for (int i = 0; i < UIEffectToControlList.Count; ++i)
            {
                UIEffectToControlList[i].effectMode = EffectMode.Grayscale;
                UIEffectToControlList[i].effectFactor = 1;
            }

            backgroundImageOldVersionMask.gameObject.SetActive(false);
            unlockLockSpine.gameObject.SetActive(true);

            SetFinishTimeStampText(-1);
        }
        else
        {
            if (isCompleted)
            {
                helpButton.gameObject.SetActive(false);
                viewButton.gameObject.SetActive(true);
                finishedRoot.SetActive(true);
                chapterDescriptionText.SetTerm($"Story.Chapter{alteredAreaID}.Des_After");
            }
            else
            {
                helpButton.gameObject.SetActive(true);
                helpButton.enabled = true;
                viewButton.gameObject.SetActive(false);
                finishedRoot.SetActive(false);
                chapterDescriptionText.SetTerm($"Story.Chapter{alteredAreaID}.Des_Before");
            }

            titleText.SetMaterialPreset(MaterialPresetName.Title_Blue);
            helpBtnText.SetMaterialPreset(MaterialPresetName.Btn_Green);
            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                if (titleText.Target != null)
                    titleText.Refresh();
                if (helpBtnText.Target != null)
                    helpBtnText.Refresh();
            });
            chapterDescriptionText.GetComponent<TextMeshProUGUI>().color = new Color(152 / 255.0f, 63 / 255.0f, 43 / 255.0f);
            for (int i = 0; i < UIEffectToControlList.Count; ++i)
            {
                UIEffectToControlList[i].effectMode = EffectMode.None;
            }
            parentPanelCache = GameManager.UI.GetUIForm("DecorationViewPanel") as DecorationViewPanel;
            bool isInAnim = false;
            if (parentPanelCache != null && parentPanelCache.ShowUnlockNewAreaAnim)
                isInAnim = true;
            if (DecorationModel.Instance.Data.DecorationAreaID == areaId && isInAnim)
            {
                backgroundImageOldVersionMask.gameObject.SetActive(true);
                SetFinishTimeStampText(-1);
            }
            else
            {
                backgroundImageOldVersionMask.gameObject.SetActive(false);
                SetFinishTimeStampText(DecorationModel.Instance.GetTargetAreaGetRewardTime(currentAreaId));
            }

            unlockLockSpine.gameObject.SetActive(false);
            unlockLockSpine.AnimationState.SetAnimation(0, "lock", true);//灰色 静止

        }

        helpButton.OnReset();
        helpButton.OnInit(() => OnHelpButtonClick());
        viewButton.OnReset();
        viewButton.OnInit(() => OnViewButtonClick());

        string defaultSpriteName = "decorationDefault";
        string useId = $"Area{alteredAreaID}_Before";
        if (isCompleted)//已完成的图片使用索引
        {
            useId = $"Area{alteredAreaID}_After";
        }

        if (useId != currentUseId)
        {
            backgroundImage.color = new Color(1, 1, 1, 0);
            backgroundImage.sprite = null;
            backgroundImageOldVersionMask.color = new Color(1, 1, 1, 0);
            backgroundImageOldVersionMask.sprite = null;

            if (!AddressableUtils.IsHaveAssetSync<Sprite>(useId))
                useId = defaultSpriteName;

            currentUseId = useId;
            waitingCount++;
            loadAssetList.Add(UnityUtility.LoadAssetAsync<Sprite>(useId, sp =>
            {
                waitingCount--;
                if (sp != null)
                {
                    if (currentUseId == useId)
                    {
                        backgroundImage.sprite = sp;
                        backgroundImage.color = Color.white;
                    }
                }
                else
                {
                    currentUseId = null;
                }
            }));

            string maskKey = $"Area{alteredAreaID}_Before";

            if (!AddressableUtils.IsHaveAssetSync<Sprite>(maskKey))
                maskKey = defaultSpriteName;

            waitingCount++;
            loadAssetList.Add(UnityUtility.LoadAssetAsync<Sprite>(maskKey, sp =>
            {
                waitingCount--;
                if (sp != null)
                {
                    backgroundImageOldVersionMask.sprite = sp;
                    backgroundImageOldVersionMask.color = Color.white;
                }
            }));
        }


        if (DecorationModel.Instance.Data.DecorationAreaID == areaId)
        {
            if (parentPanelCache != null && parentPanelCache.ShowUnlockNewAreaAnim)
                ShowArrowGO();
            else
            {
                if (DecorationModel.Instance.Data.DecorationOperatingAreaID == areaId - 1)
                    ShowArrowGO();
                else
                    HideArrowGO();
            }
        }
        else
            HideArrowGO();

        RefreshBtnStatusByDownload();
    }

    public void Refresh()
    {
        if (currentAreaId > 0)
        {
            InitializePanel(currentAreaId);
        }
    }

    private void OnHelpButtonClick()
    {
        if (isWaitingDecorationAreaLoading)
            return;

        if (parentPanelCache != null && parentPanelCache.InAnim)
            return;

        isWaitingDecorationAreaLoading = true;
        if (parentPanelCache != null)
            parentPanelCache.InAnim = true;

        bool isChange = DecorationModel.Instance.SetDecorationOperatingAreaID(currentAreaId);//会进而触发 ChangeDecorationAreaEventArgs 进而触发 MapDecorationBGPanel.ChangeDecorationArea

        if (DecorationModel.Instance.TempDecoratingOperatingAreaID != 0) 
            DecorationModel.Instance.ClearTempDecoratingOperatingAreaID();

        StartCoroutine(BackToOperationPanelUntilMapDecorationBGPanelInitComplete(false));
        //GameManager.UI.ShowUIForm<LoadingMenu>(obj => {/*甚至可以这里加一个LoadingMenu 更安全*/});
    }

    private void OnViewButtonClick()
    {
        if (!isHaveAsset)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            viewButton.interactable = false;
            OnDownloadButtonClick();
            return;
        }

        if (isWaitingDecorationAreaLoading)
            return;

        if (parentPanelCache != null && parentPanelCache.InAnim)
            return;

        isWaitingDecorationAreaLoading = true;
        if (parentPanelCache != null)
            parentPanelCache.InAnim = true;

        DecorationModel.Instance.SetTempDecoratingOperatingAreaID(currentAreaId);//会进而触发 ChangeDecorationAreaEventArgs 进而触发 MapDecorationBGPanel.ChangeDecorationArea

        StartCoroutine(BackToOperationPanelUntilMapDecorationBGPanelInitComplete(true));
    }

    private IEnumerator BackToOperationPanelUntilMapDecorationBGPanelInitComplete(bool formViewBtn)
    {
        yield return null;//等一帧 避免 decorationBGPanel 根本还没收到 ChangeDecorationAreaEventArgs
        MapDecorationBGPanel decorationBGPanel = GameManager.UI.GetUIForm("MapDecorationBGPanel") as MapDecorationBGPanel;
        if (decorationBGPanel != null)
            yield return new WaitUntil(() => decorationBGPanel.CheckInitComplete());
        //GameManager.UI.HideUIForm<LoadingMenu>();
        if (parentPanelCache != null)
            parentPanelCache.InAnim = true;
        if (!formViewBtn)
        {
            GameManager.UI.HideUIForm("DecorationViewPanel");
            GameManager.UI.ShowUIForm("DecorationOperationPanel",
                (f) =>
                {
                    isWaitingDecorationAreaLoading = false;
                    if (parentPanelCache != null)
                        parentPanelCache.InAnim = false;
                });
        }
        else
        {
            GameManager.UI.ShowUIForm("DecorationViewFinishedAreaPanel",
                (f) =>
                {
                    isWaitingDecorationAreaLoading = false;
                    if (parentPanelCache != null)
                        parentPanelCache.InAnim = false;
                });
        }
    }

    public void PlayRecentChapterUnlockAnim(int areaID)
    {
        if (currentAreaId != areaID)
            return;

        if (backgroundImageOldVersionMask != null)
        {
            InitializePanel(currentAreaId);

            //设置为灰色
            for (int i = 0; i < UIEffectToControlList.Count; ++i)
            {
                UIEffectToControlList[i].effectMode = EffectMode.Grayscale;
                UIEffectToControlList[i].effectFactor = 1;
            }
            TextMeshProUGUI textMeshProUGUI = chapterDescriptionText.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.color = Color.gray;
            //
            helpBtnText.SetMaterialPreset(MaterialPresetName.Btn_Grey);

            unlockLockSpine.gameObject.SetActive(true);
            unlockLockSpineBackEffect.gameObject.SetActive(true);
            unlockLockSpine.AnimationState.SetAnimation(0, "active_lock", false);
            GameManager.Sound.PlayAudio(SoundType.SFX_Help_Chapter_Unlock.ToString());

            GameManager.Task.AddDelayTriggerTask(0.6f, () =>
            {
                if (unlockLockSpineBackEffect != null)
                    unlockLockSpineBackEffect.gameObject.SetActive(false);

                //灰色变彩色
                float grayToNormalTime = 0.3f;
                for (int i = 0; i < UIEffectToControlList.Count; ++i)
                {
                    UIEffectToControlList[i].effectMode = EffectMode.Grayscale;
                    UIEffectToControlList[i].effectFactor = 1;
                    int index = i;
                    DOTween.To(() => UIEffectToControlList[index].effectFactor, t => UIEffectToControlList[index].effectFactor = t, 0, grayToNormalTime);
                }

                if (chapterDescriptionText != null)
                {
                    TextMeshProUGUI textMeshProUGUI = chapterDescriptionText.GetComponent<TextMeshProUGUI>();
                    textMeshProUGUI.color = Color.gray;
                    textMeshProUGUI.DOColor(new Color(152 / 255.0f, 63 / 255.0f, 43 / 255.0f), grayToNormalTime);
                    //
                    helpBtnText.SetMaterialPreset(MaterialPresetName.Btn_Green);

                    GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                    {
                        ShowArrowGO();
                    });
                }
            });
        }
    }

    private void ShowArrowGO()
    {
        if (arrowGOOnButton != null)
        {
            //箭头动画
            arrowGOOnButton.SetActive(true);
            arrowGOOnButton.transform.DOKill();
            arrowGOOnButton.transform.localPosition = Vector3.zero;
            arrowGOOnButton.transform.DOLocalMoveY(60, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void ShowDownloadArrowGO(bool isShow)
    {
        if (DownloadArrow != null)
        {
            DownloadArrow.transform.DOKill();
            DownloadArrow.SetActive(isShow);
            if (!isShow) return;
            //箭头动画
            DownloadArrow.SetActive(true);
            DownloadArrow.transform.localPosition = Vector3.zero;
            DownloadArrow.transform.DOLocalMoveY(60, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void HideArrowGO()
    {
        if (arrowGOOnButton != null)
        {
            arrowGOOnButton.SetActive(false);
        }
    }

    public bool PlayLastChapterFinishAnim(int areaID, Action onFinished)
    {
        if (currentAreaId != areaID)
        {
            //Log.Info($"currentAreaId {currentAreaId} != areaID {areaID}");
            return false;
        }

        //Log.Info($"currentAreaId {currentAreaId} == areaID {areaID}");

        if (backgroundImageOldVersionMask != null)
        {
            InitializePanel(currentAreaId);

            unlockEffect.SetActive(true);
            backgroundImageOldVersionMask.gameObject.SetActive(true);
            backgroundImageOldVersionMask.fillAmount = 1;
            DOTween.To(() => backgroundImageOldVersionMask.fillAmount, t => backgroundImageOldVersionMask.fillAmount = t, 0, 1.0f);

            //光的特效从这张图片左到右扫过
            //float backgroundImageWidth = backgroundImage.preferredWidth;//用这个更准确，但是需要依赖这张图已经加载出来 不如就直接硬编码图片尺寸
            float backgroundImageWidth = 832.0f;//略小于图片的width 848.0f;
            unlockEffect.transform.localPosition = -new Vector3(backgroundImageWidth / 2, 0, 0);
            Vector3 finalLocalPosition = unlockEffect.transform.localPosition + new Vector3(backgroundImageWidth, 0, 0);
            unlockEffect.transform.DOLocalMove(finalLocalPosition, 1.0f);
            GameManager.Sound.PlayAudio(SoundType.SFX_DecorateFinished.ToString());

            GameManager.Task.AddDelayTriggerTask(1.2f, () =>
            {
                if (unlockEffect != null)
                    unlockEffect.SetActive(false);

                //绿色缎带Finished上的扫光Spine
                //finishedEffectSpine.gameObject.SetActive(true);
                //finishedEffectSpine.AnimationState.SetAnimation(0, "idle", false).Complete += (u) =>
                //{
                //遮住整个Panel的大型粒子
                panelFinishEffect.SetActive(true);
                GameManager.Task.AddDelayTriggerTask(1.0f, () =>
                {
                    if (panelFinishEffect != null)
                        panelFinishEffect.SetActive(false);
                });

                bodyScaleTrans.localScale = Vector3.one;
                bodyScaleTrans.DOPunchScale(Vector3.one * 0.1f, 0.4f, 2, 0.1f).onComplete = () =>
                {
                    GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                    {
                        onFinished?.Invoke();
                    });
                };

                finishedRootScaleTrans.localScale = Vector3.one;
                finishedRootScaleTrans.DOPunchScale(new Vector3(0.05f, 0.0f, 0.0f), 0.4f, 2, 0.1f);

                SetFinishTimeStampText(DecorationModel.Instance.GetTargetAreaGetRewardTime(currentAreaId));
                //};
            });
            return true;
        }
        return false;
    }

    public void OnRelease()
    {
        GameManager.Event.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
        GameManager.Event.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        GameManager.Event.Unsubscribe(DownloadStartEventArgs.EventId, OnDownloadUpdate);
        for (int i = 0; i < loadAssetList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(loadAssetList[i]);
        }
        loadAssetList.Clear();

        currentUseId = null;
        currentAreaId = 0;
    }

    private void OnDestroy()
    {
        OnRelease();
    }

    public bool CheckInitComplete()
    {
        return waitingCount == 0;
    }

    private void SetFinishTimeStampText(long inputTimeStamp)
    {
        if (finishTimeStampText == null)
            return;

        long timeStamp = inputTimeStamp;
        if (timeStamp > 0)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();

            CultureInfo cultureInfo = GameManager.Localization.GetCultureInfoFromLanguage(GameManager.Localization.Language);
            DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;
            finishTimeStampText.text = dateTime.ToString("d", dtfi);

            //finishTimeStampText.text = dateTime.ToString(Constant.GameConfig.DefaultDateTimeFormet);
        }
        else
        {
            finishTimeStampText.text = "";
        }
    }

    #region 下载
    private void OnDownloadSuccess(object sender, GameEventArgs e)
    {
        Log.Info($"OnDownloadSuccess:{((DownloadSuccessEventArgs)e).DownloadKey}");
        RefreshUIByDownload(((DownloadSuccessEventArgs)e).DownloadKey, true);
    }

    private void OnDownloadFailure(object sender, GameEventArgs e)
    {
        Log.Info($"OnDownloadFailure:{((DownloadFailureEventArgs)e).DownloadKey}");
        RefreshUIByDownload(((DownloadFailureEventArgs)e).DownloadKey, false);
    }

    private void OnDownloadUpdate(object sender, GameEventArgs e)
    {
        var args = (DownloadUpdateEventArgs)e;
        if (args != null && args.DownloadKey.Equals(areaName))
        {
            //展示进度条
            ShowDownloadingSlider(args.Percent);
        }
    }

    private void RefreshUIByDownload(string name, bool isSuccess)
    {
        //如果是当前的章节下载
        if (name.Equals(areaName))
        {
            ShowDownloadArrowGO(false);

            //刷新ui显示
            InitializePanel(currentAreaId);

            if (!isSuccess)
            {
                Action action = () =>
                {
                    if (gameObject == null || !gameObject.activeInHierarchy) return;
                    if (!isCurNeedDownloadArea) return;

                    ShowDownloadArrowGO(false);
                    GameManager.Download.AddDownload(areaName, "Foreground", priority: 100);
                    DownloadBtn.interactable = false;
                    DownloadBtn_UIEffect.effectMode = EffectMode.Grayscale;
                    Download_Text.SetMaterialPreset(MaterialPresetName.Btn_Grey);
                };
                GameManager.UI.ShowUIForm("ConnectionLostPanel",userData: action);
            }
        }
    }

    private void RefreshBtnStatusByDownload()
    {
        //1、如果正在下载 help view 按钮隐藏，箭头隐藏，download 按钮灰色显示 不可点击，进度条显示
        //2、如果是需要下载，help view按钮隐藏，箭头显示，download按钮绿色显示 可点击 点击之后变成状态1，进度条不显示，
        //3、如果是可以展示，但是未拥有资源，view按钮灰色状态显示，隐藏下载相关
        //4、如果是可以展示，并且拥有资源，按钮正常展示，隐藏下载相关
        DownloadBtn.gameObject.SetActive(false);
        ViewBtn_Effect.effectMode = EffectMode.None;
        viewButton.interactable = true;
        ViewBtn_Text.SetMaterialPreset(MaterialPresetName.Btn_Green);
        ShowDownloadArrowGO(false);
        Downloading_Slider.gameObject.SetActive(false);
        if (isHaveAsset) return;//拥有资源不做额外处理
        //非当前需要下载资源，并且未拥有，view灰态展示
        bool isDownloading = DecorationViewPanel.RecordCurDownloadName!=null&& DecorationViewPanel.RecordCurDownloadName.Equals(areaName) && GameManager.Download.IsDownloading(areaName);//是否正在下载
        SetDownloadBtnStatus(isHaveAsset, isCurNeedDownloadArea, isDownloading);
    }

    private void SetDownloadBtnStatus(bool isHaveAsset, bool isNeedDownload, bool isDownloading)
    {
        bool isGray = !isHaveAsset && !isNeedDownload;
        ViewBtn_Effect.effectMode = isGray ? EffectMode.Grayscale : EffectMode.None;
        //viewButton.interactable = !isGray;
        ViewBtn_Text.SetMaterialPreset(isGray ? MaterialPresetName.Btn_Grey : MaterialPresetName.Btn_Green);

        if (isNeedDownload)
        {
            helpButton.gameObject.SetActive(false);
            DownloadBtn.gameObject.SetActive(true);
            DownloadBtn.interactable = !isDownloading;
            DownloadBtn_UIEffect.effectMode = isDownloading ? EffectMode.Grayscale : EffectMode.None;
            Download_Text.SetMaterialPreset(isDownloading ? MaterialPresetName.Btn_Grey : MaterialPresetName.Btn_Green);
            Downloading_Slider.gameObject.SetActive(isDownloading);
            if (!isDownloading)
                ShowDownloadArrowGO(true);

            DownloadBtn.SetBtnEvent(OnDownloadButtonClick);
        }
        else
        {
            DownloadBtn.gameObject.SetActive(false);
        }
    }

    private void OnDownloadButtonClick()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.UI.ShowUIForm("ConnectionLostPanel");
            return;
        }

        DecorationViewPanel.RecordCurDownloadName = areaName;
        GameManager.Download.AddDownload(areaName, "Foreground", priority: 100);

        try
        {
            ShowDownloadArrowGO(false);
            DownloadBtn.interactable = false;
            DownloadBtn_UIEffect.effectMode = EffectMode.Grayscale;
            Download_Text.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        }
        catch (Exception e)
        {
            Log.Info($"{e.Message}");
        }
    }

    private void ShowDownloadingSlider(float percent)
    {
        try
        {
            if (!DecorationViewPanel.RecordCurDownloadName.Equals(areaName)) return;
            if (Downloading_Slider) Downloading_Slider.gameObject.SetActive(true);
            Downloading_Slider.value = percent;
            Downloading_Text.text = $"{(int)(percent * 100)}%";
            if (percent >= 1f && DownloadBtn.gameObject.activeInHierarchy)
            {
                DownloadBtn.gameObject.SetActive(false);
                InitializePanel(currentAreaId);
            }
        }
        catch (Exception e)
        {
            Log.Info($"ShowDownloadingSlider:{e.Message}");
        }
    }

    #endregion
}