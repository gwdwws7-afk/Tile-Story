using System;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class DaliyWatchAdsPanel : UIForm
{
    [SerializeField] private Transform PropItem_Trans;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        var tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

        SetPropItem(tileMatchPanel.GetCanUseSkills(), tileMatchPanel.GetSkillAction);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
        base.OnShow(showSuccessAction, userData);
    }

    private List<Image> recordImages=new List<Image>();
    public override void OnRelease()
    {
        if (recordImages != null&&recordImages.Count>0)
        {
            foreach (var image in recordImages)
            {
                Sprite sprite = image.sprite;
                image.sprite = null;
                Addressables.Release(sprite);
            }
        }
        recordImages.Clear();

        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
        if (arrowSequence != null)
        {
            arrowSequence.Kill();
            arrowSequence = null;
        }

        base.OnRelease();
    }
    
                
    public Sprite GetTargetSprite(string spriteName, string atlasName)
    {
        string atlasedSpriteAddress = $"{atlasName}[{spriteName}]";
        Sprite sprite = AddressableUtils.LoadAsset<Sprite>(atlasedSpriteAddress);
        return sprite;
    }

    private Sequence sequence;
    private Sequence arrowSequence;
    private void SetPropItem(List<TotalItemData> propList, Action<TotalItemData> callBack)
    {
        List<Transform> trans = new List<Transform>();
        List<Transform> arrowTrans = new List<Transform>();
        UnityUtility.FillGameObjectWithFirstChild<UICommonController>(PropItem_Trans.gameObject, propList.Count, (index, comp) =>
        {
            //set btn event  set image
            TotalItemData type = propList[index];
            comp.Btns[0].SetBtnEvent(() =>
            {
                PlayerBehaviorModel.Instance.RecordDailyWatchAdsGuide();
                callBack?.Invoke(type);
                GameManager.UI.HideUIForm(this);
            });

            comp.Images[0].sprite = GetTargetSprite(type.TotalItemType.ToString(),"TotalItemAtlas");
            recordImages.Add(comp.Images[0]);

            trans.Add(comp.Images[0].transform.parent);
            arrowTrans.Add(comp.Images[1].transform);
            comp.Images[1].gameObject.SetActive(!PlayerBehaviorModel.Instance.HasShownDailyWatchAdsGuide());
        });

        if (sequence != null) sequence.Kill();
        sequence = DOTween.Sequence();
        int index = 0;
        foreach (var tran in trans)
        {
            index++;
            sequence.Join(tran.DOShakeRotation(0.8f, Vector3.forward * 8, 8, randomness: 15, fadeOut: true,
               ShakeRandomnessMode.Harmonic).SetEase(DG.Tweening.Ease.Linear).SetDelay(index * 0.1f));
        }
        sequence.AppendInterval(2f);
        sequence.SetAutoKill(false).SetLoops(-1, LoopType.Restart).OnKill(() => sequence = null);

        if (arrowSequence != null) arrowSequence.Kill();
        float delayTime = PlayerBehaviorModel.Instance.HasShownDailyWatchAdsGuide() ? 8.0f : 0.0f;
        GameManager.Task.AddDelayTriggerTask(delayTime, () =>
        {
            for (int i = 0; i < arrowTrans.Count; ++i)
            {
                arrowTrans[i]?.gameObject?.SetActive(true);
            }
            arrowSequence = DOTween.Sequence();
            foreach (var arrowTran in arrowTrans)
            {
                Vector3 localposition = arrowTran.localPosition;
                arrowSequence.Insert(0.0f, arrowTran.DOLocalMoveY(localposition.y - 20.0f, 0.2f))
                    .Insert(0.2f, arrowTran.DOLocalMoveY(localposition.y, 0.2f))
                    .Insert(0.4f, arrowTran.DOLocalMoveY(localposition.y - 20.0f, 0.2f))
                    .Insert(0.6f, arrowTran.DOLocalMoveY(localposition.y, 0.2f));
            }
            arrowSequence.AppendInterval(2.6f);
            arrowSequence.SetAutoKill(false).SetLoops(-1, LoopType.Restart).OnKill(() => sequence = null);
        });
    }
}
