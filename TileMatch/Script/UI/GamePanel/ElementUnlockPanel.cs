using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ElementUnlockPanel : PopupMenuForm
{
    public GameObject body;
    public GameObject tipBody;
    public Transform tipBox;
    public SkeletonAnimation guideFinger;
    public Image elementImg;
    public GameObject title;
    public TextMeshProUGUILocalize tipText;
    public TextMeshProUGUILocalize infoText;
    public TextMeshProUGUILocalize elementText;
    public BlackBgManager blackBg;
    public Button okButton;

    private int attachId = 0;
    private AsyncOperationHandle spriteHandle;
    private bool isShowPropInfo = false;
    private List<TileItem> targetItems = new List<TileItem>();

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        attachId = (int)userData;

        spriteHandle = UnityUtility.LoadSpriteAsync(attachId.ToString(), "ElementGuide", sp =>
        {
            elementImg.sprite = sp;
            elementImg.SetNativeSize();
        });

        body.SetActive(true);
        tipBody.SetActive(false);
        guideFinger.gameObject.SetActive(false);
        isShowPropInfo = false;

        string attachTerm = string.Empty;
        string infoTerm = string.Empty;
        switch (attachId)
        {
            case 1:
                attachTerm = "Element.Ice";
                infoTerm = "Element.IceInfo";
                break;
            case 2:
                attachTerm = "Element.Glue";
                infoTerm = "Element.GlueInfo";
                break;
            case 3:
                attachTerm = "Element.Gold";
                infoTerm = "Element.GoldInfo";
                isShowPropInfo = true;
                break;
            case 5:
            case 6:
                attachTerm = "Element.Curtain";
                infoTerm = "Element.CurtainInfo";
                break;
            case 20:
                attachTerm = "Element.Fireworks";
                infoTerm = "Element.FireworksInfo";
                break;
        }
        tipText.SetParameterValue("ItemName", GameManager.Localization.GetString(attachTerm));
        infoText.SetTerm(infoTerm);
        elementText.SetTerm(attachTerm);
        //tipText.SetTerm(infoTerm);

        okButton.SetBtnEvent(OnClose);
        blackBg.clickButton.SetBtnEvent(OnClose);

        GameManager.Sound.PlayAudio(SoundType.SFX_Get_ingameprop.ToString());
    }

    public override void OnReset()
    {
        UnityUtility.UnloadAssetAsync(spriteHandle);

        base.OnReset();
    }

    public override void OnRelease()
    {
        UnityUtility.UnloadAssetAsync(spriteHandle);

        base.OnRelease();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        blackBg.OnShow();

        Transform titleTrans = title.transform;
        Transform tipTrans = tipText.transform;
        Transform trans = elementImg.transform;
        Transform buttonTrans = okButton.transform;

        titleTrans.localScale = Vector3.zero;
        tipTrans.localScale = Vector3.zero;
        trans.localScale = Vector3.zero;
        buttonTrans.localScale = Vector3.zero;

        trans.DOScale(1.1f, 0.15f).onComplete = () =>
        {
            trans.DOScale(0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
            {
                trans.DOScale(1, 0.15f);
            };
        };

        title.gameObject.SetActive(true);
        titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            titleTrans.DOScale(1f, 0.2f);
        };

        tipTrans.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        buttonTrans.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        for (int i = 0; i < targetItems.Count; i++)
        {
            if (targetItems[i] != null && targetItems[i].gameObject.GetComponent<Canvas>() != null) 
            {
                //targetItems[i].gameObject.GetComponent<Canvas>().sortingOrder = 7;
                Destroy(targetItems[i].gameObject.GetComponent<GraphicRaycaster>());
                Destroy(targetItems[i].gameObject.GetComponent<Canvas>());
                targetItems[i].gameObject.GetComponent<TileDelayButton>().onClick.RemoveListener(OnClose);
            }
        }
        targetItems.Clear();

        base.OnHide(hideSuccessAction, userData);
    }

    public override bool CheckInitComplete()
    {
        if (!spriteHandle.IsDone)
            return false;

        return base.CheckInitComplete();
    }

    public override void OnClose()
    {
        if (!isShowPropInfo)
        {
            isShowPropInfo = true;
            blackBg.clickButton.onClick.RemoveAllListeners();

            switch (attachId)
            {
                case 1:
                    ShowIceTip();
                    break;
                case 2:
                    ShowGlueTip();
                    break;
                case 3:
                    break;
                case 5:
                case 6:
                    ShowCurtainTip();
                    break;
                case 20:
                    ShowFireworkTip();
                    break;
            }
        }
        else
        {
            GameManager.UI.HideUIForm(this);

            var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (panel != null)
            {
                panel.ElementGuideFinish?.Invoke();
                panel.ElementGuideFinish = null;
            }
        }

        base.OnClose();
    }

    private void ShowIceTip()
    {
        body.SetActive(false);
        tipBody.SetActive(true);
        tipBox.localScale = Vector3.zero;
        tipBox.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            tipBox.DOScale(1f, 0.2f);
        };

        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null && panel.tileMapDict != null && panel.tileMapDict.Count > 0)
        {
            Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = panel.tileMapDict;
            int maxLayer = 0;
            foreach (KeyValuePair<int, SortedDictionary<int, (int, TileItem)>> item in dict)
            {
                if (item.Key > maxLayer)
                    maxLayer = item.Key;
            }

            SortedDictionary<int, (int, TileItem)> topMap = dict[maxLayer];
            TileItem target = null;
            foreach (KeyValuePair<int, (int, TileItem)> item in topMap)
            {
                if (item.Value.Item2 != null && item.Value.Item2.AttachLogic != null && item.Value.Item2.AttachLogic.AttachId == attachId)
                {
                    target = item.Value.Item2;
                    Canvas canvas = target.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    target.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    targetItems.Add(target);
                    break;
                }
            }

            TileItem rightTarget = null;
            TileItem leftTarget = null;
            if (target != null)
            {
                int mapid = target.Data.MapID;
                if (topMap.TryGetValue(mapid + 2, out (int, TileItem) rightItem))
                {
                    rightTarget = rightItem.Item2;
                    Canvas canvas = rightTarget.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    rightTarget.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    rightTarget.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                    targetItems.Add(rightTarget);

                    guideFinger.transform.position = rightTarget.transform.position;
                    guideFinger.gameObject.SetActive(true);
                    tipBox.localPosition = new Vector3(0, guideFinger.transform.localPosition.y - 400, 0);
                    var anim = guideFinger.AnimationState?.SetAnimation(0, "02", true);
                    if (anim != null) anim.TimeScale = 0.9f;
                }

                if (topMap.TryGetValue(mapid - 2, out (int, TileItem) leftItem))
                {
                    leftTarget = leftItem.Item2;
                    Canvas canvas = leftTarget.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    leftTarget.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    leftTarget.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                    targetItems.Add(leftTarget);
                }
            }
            else
            {
                GameManager.UI.HideUIForm(this);
            }
        }
        else
        {
            GameManager.UI.HideUIForm(this);
        }
    }

    private void ShowGlueTip()
    {
        body.SetActive(false);
        tipBody.SetActive(true);
        tipBox.localScale = Vector3.zero;
        tipBox.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            tipBox.DOScale(1f, 0.2f);
        };

        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null && panel.tileMapDict != null && panel.tileMapDict.Count > 0)
        {
            Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = panel.tileMapDict;
            int maxLayer = 0;
            foreach (KeyValuePair<int, SortedDictionary<int, (int, TileItem)>> item in dict)
            {
                if (item.Key > maxLayer)
                    maxLayer = item.Key;
            }

            SortedDictionary<int, (int, TileItem)> topMap = dict[maxLayer];
            TileItem target = null;
            //����߲����һ��
            foreach (KeyValuePair<int, (int, TileItem)> item in topMap)
            {
                if (item.Value.Item2 != null && item.Value.Item2.AttachLogic != null && item.Value.Item2.AttachLogic.AttachId == attachId)
                {
                    target = item.Value.Item2;
                }
            }

            if (target != null)
            {
                Canvas targetCanvas = target.gameObject.GetOrAddComponent<Canvas>();
                targetCanvas.overrideSorting = true;
                targetCanvas.sortingLayerName = "UI";
                targetCanvas.sortingOrder = 9;
                target.gameObject.GetOrAddComponent<GraphicRaycaster>();
                target.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                targetItems.Add(target);

                AttachLogic_2 glue = target.AttachLogic as AttachLogic_2;
                if (glue != null && topMap.TryGetValue(target.Data.MapID - 2, out (int, TileItem) rightItem)&& rightItem.Item2.AttachLogic.AttachId == attachId) 
                {
                    TileItem leftTarget = rightItem.Item2;
                    Canvas canvas = leftTarget.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    leftTarget.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    leftTarget.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                    targetItems.Add(leftTarget);

                    guideFinger.transform.position = target.transform.position;
                    guideFinger.gameObject.SetActive(true);
                    tipBox.localPosition = new Vector3(0, guideFinger.transform.localPosition.y + 300, 0);
                    var anim = guideFinger.AnimationState?.SetAnimation(0, "02", true);
                    if (anim != null) anim.TimeScale = 0.9f;
                }
                else
                {
                    GameManager.UI.HideUIForm(this);
                }
            }
            else
            {
                GameManager.UI.HideUIForm(this);
            }
        }
        else
        {
            GameManager.UI.HideUIForm(this);
        }
    }

    private void ShowFireworkTip()
    {
        body.SetActive(false);
        tipBody.SetActive(true);
        tipBox.localScale = Vector3.zero;
        tipBox.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            tipBox.DOScale(1f, 0.2f);
        };

        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null && panel.tileMapDict != null && panel.tileMapDict.Count > 0)
        {
            Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = panel.tileMapDict;
            int maxLayer = 0;
            foreach (KeyValuePair<int, SortedDictionary<int, (int, TileItem)>> item in dict)
            {
                if (item.Key > maxLayer)
                    maxLayer = item.Key;
            }

            SortedDictionary<int, (int, TileItem)> topMap = dict[maxLayer];
            TileItem target = null;
            foreach (KeyValuePair<int, (int, TileItem)> item in topMap)
            {
                if (item.Value.Item2 != null && item.Value.Item1 == 20) 
                {
                    target = item.Value.Item2;
                    Canvas canvas = target.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    target.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    target.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                    targetItems.Add(target);

                    guideFinger.transform.position = target.transform.position;
                    guideFinger.gameObject.SetActive(true);
                    tipBox.localPosition = new Vector3(0, guideFinger.transform.localPosition.y + 300, 0);
                    var anim = guideFinger.AnimationState?.SetAnimation(0, "02", true);
                    if (anim != null) anim.TimeScale = 0.9f;

                    break;
                }
            }

            if (target == null)
            {
                GameManager.UI.HideUIForm(this);
            }
        }
        else
        {
            GameManager.UI.HideUIForm(this);
        }
    }

    private void ShowCurtainTip()
    {
        body.SetActive(false);
        tipBody.SetActive(true);
        tipBox.localScale = Vector3.zero;
        tipBox.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            tipBox.DOScale(1f, 0.2f);
        };

        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null && panel.tileMapDict != null && panel.tileMapDict.Count > 0)
        {
            Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = panel.tileMapDict;
            int maxLayer = 0;
            foreach (KeyValuePair<int, SortedDictionary<int, (int, TileItem)>> item in dict)
            {
                if (item.Key > maxLayer)
                    maxLayer = item.Key;
            }

            int count = 1;//用第二个打开的窗帘元素
            SortedDictionary<int, (int, TileItem)> topMap = dict[maxLayer];
            TileItem target = null;
            foreach (KeyValuePair<int, (int, TileItem)> item in topMap)
            {
                if (item.Value.Item2 != null && item.Value.Item2.AttachLogic != null && item.Value.Item2.AttachLogic.AttachId == attachId) 
                {
                    if (count > 0)
                    {
                        count--;
                        continue;
                    }

                    target = item.Value.Item2;
                    Canvas canvas = target.gameObject.GetOrAddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = "UI";
                    canvas.sortingOrder = 9;
                    target.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    target.GetComponent<TileDelayButton>().onClick.AddListener(OnClose);
                    targetItems.Add(target);

                    guideFinger.transform.position = target.transform.position;
                    guideFinger.gameObject.SetActive(true);
                    tipBox.localPosition = new Vector3(0, guideFinger.transform.localPosition.y + 300, 0);
                    var anim = guideFinger.AnimationState?.SetAnimation(0, "02", true);
                    if (anim != null) anim.TimeScale = 0.9f;

                    break;
                }
            }

            if (target == null)
            {
                GameManager.UI.HideUIForm(this);
            }
        }
        else
        {
            GameManager.UI.HideUIForm(this);
        }
    }
}
