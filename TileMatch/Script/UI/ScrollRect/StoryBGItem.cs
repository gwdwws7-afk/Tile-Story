using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class StoryBGItem : MonoBehaviour
{
    [SerializeField] private DelayButton More_Btn;
    [SerializeField] private RectTransform Parent_Rect,MaskImage_Rect,BGParent_Rect;
    [SerializeField] private Transform MoreImage_Tran;
    [SerializeField] private TextMeshProUGUILocalize More_Text;

    private bool isMore = false;
    private int totalStoryCount = 0;
    private int maxStoryIDByUnlock = 0;
    private int maxFinishedDecorationID = 0;
    public void Init(int allStoryCount,int maxStoryIdByUnlock)
    {
        if (allStoryCount < 6)
        {
            isMore = false;
        }

        this.totalStoryCount = allStoryCount;
        this.maxStoryIDByUnlock = maxStoryIdByUnlock;
        this.maxFinishedDecorationID = GameManager.PlayerData.GetHighestFinishedDecorationAreaID();

        allStoryCount = isMore ? allStoryCount : Math.Min(allStoryCount,5);
        
        UnityUtility.FillGameObjectWithFirstChild<BGStoryCell>(BGParent_Rect.gameObject,allStoryCount,
            (index, comp) =>
            {
                int id = index+1;
                bool isUnlock = id <=maxFinishedDecorationID;
                comp.Init(id,isUnlock);
            });
        
        SetMore(isMore);
        More_Btn.SetBtnEvent(() =>
        {
            isMore = !isMore;
            Init(totalStoryCount,maxStoryIDByUnlock);
        });
    }

    private ContentSizeFitter contentSizeFitter = null;
    private ContentSizeFitter ContentSizeFitter =>
        contentSizeFitter ?? (contentSizeFitter = transform.GetComponentInParent<ContentSizeFitter>());

    private void SetMore(bool isMore)
    {
        if (totalStoryCount < 6)
        {
            More_Btn.gameObject.SetActive(false);
            Parent_Rect.sizeDelta=new Vector2(1054, 510);
            MaskImage_Rect.sizeDelta = new Vector2(1054, 510);
            BGParent_Rect.sizeDelta = new Vector2(1000,300);
            MoreImage_Tran.DOLocalRotate(Vector3.forward * 180, 0.2f);
            return;
        }
        if (isMore)
        {
            int addSizeY = totalStoryCount / 5 * 300 + (totalStoryCount % 5 == 0 ? 0 : 1) * 300;
            More_Text.SetTerm("BG.Collapse");
            More_Text.Target.fontSize = 42;
            More_Btn.gameObject.SetActive(true);
            MaskImage_Rect.sizeDelta = new Vector2(1054, 250+addSizeY);
            BGParent_Rect.sizeDelta = new Vector2(1000,addSizeY);
            Parent_Rect.sizeDelta=new Vector2(1054, 250+addSizeY);
            MoreImage_Tran.DOLocalRotate(Vector3.forward * 180, 0.2f);
        }
        else
        {
            More_Btn.gameObject.SetActive(true);
            More_Text.SetTerm("BG.More");
            More_Text.Target.fontSize = 42;
            MaskImage_Rect.sizeDelta = new Vector2(1054, 550);
            BGParent_Rect.sizeDelta = new Vector2(1000,300);
            Parent_Rect.sizeDelta=new Vector2(1054, 550);
            MoreImage_Tran.DOLocalRotate(Vector3.zero, 0.2f);
        }
        if (ContentSizeFitter != null)
        {
            ContentSizeFitter.SetLayoutVertical();
            LayoutRebuilder.MarkLayoutForRebuild(ContentSizeFitter.GetComponent<RectTransform>());
        }
    }
}
