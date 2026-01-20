using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Analytics;

/// <summary>
/// ����DecorationOperationPanel �����ϣ���Ӧ��ÿ���ɽ���Decoration
/// </summary>
public class UnlockDecorationItemBtn : MonoBehaviour
{
    [SerializeField]
    private Image unlockIconImage;
    [SerializeField]
    private TextMeshProUGUI unlockCostText;
    [SerializeField]
    private GameObject availableEffect;//��ť�����Ч ֻ�������㹻ʱ��ʾ

    private AsyncOperationHandle asyncOperationHandle;
    private DelayButton delayButton;
    private DecorateItem decorateItemCache;
    private int indexCache;
    private DecorationOperationPanel parentPanelCache;

    public int StarCost
    {
        get
        {
            if (decorateItemCache != null)
                return decorateItemCache.Cost;
            else
                return -1;
        }
    }

    public void Init(int inputAreaID, int inputIndex, DecorationOperationPanel inputParentPanel)
    {
        decorateItemCache = DecorationModel.Instance.GetTargetDecorationItem(inputAreaID, inputIndex);
        indexCache = inputIndex;
        parentPanelCache = inputParentPanel;

        unlockIconImage.color = new Color(1, 1, 1, 0);
        UnityUtility.UnloadAssetAsync(asyncOperationHandle);
        asyncOperationHandle = UnityUtility.LoadSpriteAsync(decorateItemCache.UnlockIcon, "Decoration", (sp) =>
        {
            unlockIconImage.sprite = sp;
            unlockIconImage.SetNativeSize();
            unlockIconImage.color = Color.white;
        });
        unlockCostText.text = decorateItemCache.Cost.ToString();

        delayButton = GetComponent<DelayButton>();
        delayButton.OnInit(OnButtonClicked);
        delayButton.interactable = true;

        int curNum = ItemModel.Instance.GetItemTotalNum(TotalItemData.Star.TotalItemType);
        availableEffect.SetActive(decorateItemCache.Cost <= curNum);
    }

    public void OnRelease()
    {
        transform.DOKill();
        UnityUtility.UnloadAssetAsync(asyncOperationHandle);
        if (delayButton != null)
            delayButton.onClick.RemoveAllListeners();
    }

    private void OnButtonClicked()
    {
        if (parentPanelCache.InAnim())
            return;

        int cost = decorateItemCache.Cost;
        bool starEnough = GameManager.PlayerData.UseItem(TotalItemData.Star, cost);
        if (!starEnough)
        {
            int curNum = ItemModel.Instance.GetItemTotalNum(TotalItemData.Star.TotalItemType);
            int lackNum = cost - curNum;
            GameManager.UI.ShowUIForm("EarnMoreStarPanel",null, null, lackNum);
        }
        else
        {
            parentPanelCache.RegisterInAnimReason(DecorationOperationPanel.InAnimReason.StarFlyTillShinningFly);

            DecorationOperationPanel operationPanel = (DecorationOperationPanel)GameManager.UI.GetUIForm("DecorationOperationPanel");
            if (operationPanel != null)
            {
                operationPanel.PlayStarFlyingAnim(cost, transform.position, OnStarFlyFinished, OnStarFlyFinished_NotLastOne);
            }
        }
    }

    private void OnStarFlyFinished_NotLastOne()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1).onComplete = () =>
        {
            transform.DOScale(1, 0.2f);
        };
    }

    private void OnStarFlyFinished()
    {
        //delayButton.body.DOKill();
        transform.DOScale(Vector3.one * 1.3f, 0.2f).OnComplete(() =>
        {
            transform.DOScale(Vector3.one * 0.1f, 0.2f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.one;

                DecorationModel.Instance.SetDecorationType(decorateItemCache.Belong, indexCache, 1);
                int finishedCount = DecorationModel.Instance.GetTargetAreaFinishedDecorationCount(decorateItemCache.Belong);
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Decoration_Stage, 
                    new Parameter("ChapterNum", decorateItemCache.Belong),
                    new Parameter("Stage", finishedCount));

                MapDecorationBGPanel decorationBGPanel = (MapDecorationBGPanel)GameManager.UI.GetUIForm("MapDecorationBGPanel");
                decorationBGPanel.FirstTimeDecorate(indexCache, true);
                //decorationBGPanel.PlayCharacterAction();
                //decorationBGPanel.PlayARandomIdleDialogue();
            });
        });
    }
}
