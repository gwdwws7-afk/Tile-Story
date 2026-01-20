using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventBGItem : MonoBehaviour
{
    [SerializeField] private DelayButton More_Btn;
    [SerializeField] private RectTransform Parent_Rect,MaskImage_Rect,BGParent_Rect;
    [SerializeField] private Transform MoreImage_Tran;
    [SerializeField] private TextMeshProUGUILocalize More_Text;

    private bool isMore = false;

    List<BGItemData> dataList = new List<BGItemData>();
    //public void Init(int allStoryCount,int maxStoryIdByUnlock)
    public void Init()
    {
        dataList = GameManager.DataTable.GetDataTable<DTBGID>().Data.BGThemeDict[0];
        dataList.Sort((data1, data2) => data1.BGSort.CompareTo(data2.BGSort));

        if (dataList.Count < 6)
        {
            isMore = false;
        }

        int createCount = isMore ? dataList.Count : Math.Min(dataList.Count, 5);
        
        UnityUtility.FillGameObjectWithFirstChild<BGImageCell>(BGParent_Rect.gameObject, createCount,
            (index, comp) =>
            {
                comp.Init(dataList[index].ID, null);
            });
        
        SetMore(isMore);
        More_Btn.SetBtnEvent(() =>
        {
            isMore = !isMore;
            Init();
        });
    }

    private ContentSizeFitter contentSizeFitter = null;
    private ContentSizeFitter ContentSizeFitter =>
        contentSizeFitter ?? (contentSizeFitter = transform.GetComponentInParent<ContentSizeFitter>());

    private void SetMore(bool isMore)
    {
        if (dataList.Count < 6)
        {
            More_Btn.gameObject.SetActive(false);
            Parent_Rect.sizeDelta=new Vector2(1054, 510);
            MaskImage_Rect.sizeDelta = new Vector2(1054, 510);
            BGParent_Rect.sizeDelta = new Vector2(1000,300);
            MoreImage_Tran.gameObject.SetActive(false);
            MoreImage_Tran.DOLocalRotate(Vector3.forward * 180, 0.2f);
            return;
        }
        if (isMore)
        {
            int addSizeY = dataList.Count / 5 * 300 + (dataList.Count % 5 == 0 ? 0 : 1) * 300;
            More_Text.SetTerm("BG.Collapse");
            More_Text.Target.fontSize = 42;
            More_Btn.gameObject.SetActive(true);
            MaskImage_Rect.sizeDelta = new Vector2(1054, 250+addSizeY);
            BGParent_Rect.sizeDelta = new Vector2(1000,addSizeY);
            Parent_Rect.sizeDelta = new Vector2(1054, 250 + addSizeY);
            MoreImage_Tran.gameObject.SetActive(true);
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
            MoreImage_Tran.gameObject.SetActive(true);
            MoreImage_Tran.DOLocalRotate(Vector3.zero, 0.2f);
        }
        if (ContentSizeFitter != null)
        {
            ContentSizeFitter.SetLayoutVertical();
            LayoutRebuilder.MarkLayoutForRebuild(ContentSizeFitter.GetComponent<RectTransform>());
        }
    }
}
