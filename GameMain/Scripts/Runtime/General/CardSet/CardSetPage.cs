using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyFrameWork.Framework;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CardSetPage : FancyScrollRectCell<int, Context>
{
    public Image bg, cover, banner;
    public TextMeshProUGUILocalize setName;
    public GameObject completeArea, incompleteArea;
    public SkeletonGraphic effect;
    public GameObject rewardArea;
    // public SimpleSlider simpleSlider;
    public Transform cardArea;
    public CanvasGroup canvas;
    
    private CardSet _cardSet;
    private int _cardSetID;
    private int _index = 0;
    private List<CardItem> _cardItemList = new List<CardItem>();
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    
    private void Init(CardSet cardSet)
    {
        canvas.alpha = 0;
        _cardSet = cardSet;
        _cardSetID = cardSet.CardSetID;
        // Debug.LogError("Init" + _cardSetID);
        transform.name = _cardSetID.ToString();
        
        setName.SetTerm($"Card.{cardSet.ActivityID}_{cardSet.CardSetID}");
        SetUpArea();

        // yield return null;
        
        string bgStr = $"BookAtlas{CardModel.Instance.CardActivityID}[TOPbg{cardSet.CardSetID % 5}]";
        _index++;
        _assetHandleList.Add(
            UnityUtility.LoadAssetAsync<Sprite>(bgStr, asset =>
            {
                bg.sprite = asset as Sprite;
                CheckAssetHandleDone();
            }));
        
        string coverStr = $"Card.{cardSet.ActivityID}_{cardSet.CardSetID}";
        _index++;
        _assetHandleList.Add(
            UnityUtility.LoadAssetAsync<Sprite>(coverStr, asset =>
            {
                cover.sprite = asset as Sprite;
                CheckAssetHandleDone();
            }));
        
        string bannerStr = $"CommonAtlas{CardModel.Instance.CardActivityID}[条幅{cardSet.CardSetID % 5}]";
        _index++;
        _assetHandleList.Add(
            UnityUtility.LoadAssetAsync<Sprite>(bannerStr, asset =>
            {
                banner.sprite = asset as Sprite;
                CheckAssetHandleDone();
            }));
        
        foreach (var card in cardSet.CardDict)
        {
            AsyncOperationHandle assetHandle = UnityUtility.InstantiateAsync(
                $"CardItem{CardModel.Instance.CardActivityID}", cardArea, asset =>
                {
                    // asset.transform.localScale = new Vector3(0.66667f, 0.66667f, 0.66667f);
                    CardItem cardItem = asset.GetComponent<CardItem>();
                    cardItem.Init(card.Value);
                    _cardItemList.Add(cardItem);
                });
            _assetHandleList.Add(assetHandle);
            // yield return assetHandle;
        }
        
        effect.AnimationState.TimeScale = 0;
        // effect.AnimationState.SetAnimation(0, "active", false);
        // effect.Update(0);
    }
    
    private void CheckAssetHandleDone()
    {
        _index--;
        if (_index <= 0)
        {
            canvas.alpha = 1;
        }
    }
    
    private void SetUpArea()
    {
        if (CardModel.Instance.CompletedCardSets.Contains(_cardSetID))
        {
            completeArea.SetActive(true);
            incompleteArea.SetActive(false);
            
            //release
            foreach (Transform child in rewardArea.transform)
            {
                child.GetComponent<CardSetRewardSlot>().Release();
            }
            // simpleSlider.OnReset();
        }
        else
        {
            completeArea.SetActive(false);
            
            UnityUtility.FillGameObjectWithFirstChild<CardSetRewardSlot>(rewardArea, _cardSet.RewardTypeList.Count, (index, comp) =>
            {
                comp.Init(_cardSet.RewardTypeList[index], _cardSet.RewardNumList[index]);
            });

            // for (int i = 0; i < _cardSet.RewardTypeList.Count; i++)
            // {
            //     int index = i;
            //     UnityUtility.InstantiateAsync($"CardSetRewardSlot{CardModel.Instance.CardActivityID}", rewardArea.transform, o =>
            //     {
            //         o.GetComponent<CardSetRewardSlot>()
            //             .Init(_cardSet.RewardTypeList[index], _cardSet.RewardNumList[index]);
            //     });
            // }

            // simpleSlider.OnReset();
            // simpleSlider.TotalNum = _cardSet.CardDict.Count;
            // simpleSlider.CurrentNum = _cardSet.CardSetCollectNum();
            
            incompleteArea.SetActive(true);
        }
    }

    public void Refresh()
    {
        StopAllCoroutines();
        
        // Debug.LogError("RefreshPage" + transform.name);
        if (CardModel.Instance.NewCardDict.Remove(_cardSetID))
        {
            CardModel.Instance.SaveToLocal();
        }
        foreach (var cardItem in _cardItemList)
        {
            cardItem.newTag.SetActive(false);
            cardItem.spineLight.enabled = false;
            // cardItem.spineLight.AnimationState.ClearTracks();
            // cardItem.spineLight.Skeleton.SetToSetupPose();
        }
    }

    public void ShowCardSetReward()
    {
        if (CardModel.Instance.CompletedCardSets.Contains(_cardSetID)) return;

        StartCoroutine(ShowCardSetRewardAnim());
    }

    IEnumerator ShowCardSetRewardAnim()
    {
        bool isComplete = _cardSet.CardSetCollectNum() == _cardSet.CardDict.Count;
        //如果complete加屏蔽
        if (isComplete)
            GameManager.UI.ShowUIForm("GlobalMaskPanel");

        //扫光
        yield return new WaitUntil(() => _cardItemList.Count == _cardSet.CardDict.Count);
        foreach (var card in _cardItemList)
        {
            if (card.newTag.activeSelf)
            {
                card.spineLight.AnimationState.TimeScale = 1;
            }
        }

        if (isComplete)
        {
            //发奖
            for (int i = 0; i < _cardSet.RewardTypeList.Count; i++)
            {
                RewardManager.Instance.AddNeedGetReward(_cardSet.RewardTypeList[i], _cardSet.RewardNumList[i]);
            }

            CardModel.Instance.NewCardDict.Remove(_cardSetID);
            CardModel.Instance.CompletedCardSets.Add(_cardSet.CardSetID);
            CardModel.Instance.SaveToLocal();

            yield return new WaitForSeconds(1.5f);
            
            SetUpArea();
            effect.AnimationState.TimeScale = 1;
            GameManager.Sound.PlayAudio("Card_Collection_Set_Completed_Shrink_Tips");

            yield return new WaitForSeconds(0.5f);
            
            CardModel.Instance.CurrentCardSet = _cardSet.CardSetID;
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardSetRewardPanel, false, null,
                onCreatePanelSuccess: () =>
                {
                    GameManager.Sound.PlayAudio("Card_Collection_Complete_One_Set");
                    GameManager.UI.HideUIForm("GlobalMaskPanel");
                });

        }
    }

    public void Release()
    {
        StopAllCoroutines();
        
        if (_cardSetID == 0) return;
        // Debug.LogError("Release" + _cardSetID);
        
        
        foreach (Transform child in rewardArea.transform)
        {
            child.GetComponent<CardSetRewardSlot>().Release();
        }
        // simpleSlider.OnReset();

        _cardSet = null;
        _cardSetID = 0;
        foreach (var cardItem in _cardItemList)
        {
            cardItem.Release();
        }
        _cardItemList.Clear();
        foreach (var assetHandle in _assetHandleList)
        {
            if (!assetHandle.IsDone)
            {
                assetHandle.Completed += (a) =>
                {
                    SafeUnload(assetHandle);
                };
            }
            else
            {
                SafeUnload(assetHandle);
            }

            //UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
    }
    
    public static void SafeUnload(AsyncOperationHandle handle)
    {
        if (!handle.IsValid())
            return;

        // 如果是失败的异步操作，也 release handle
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            UnityUtility.UnloadInstance(handle);
            return;
        }

        // 成功完成
        var resultObj = handle.Result as UnityEngine.Object;
        if (resultObj != null)
        {
            // 如果 result 是 GameObject 实例（通过 InstantiateAsync 创建）
            GameObject go = resultObj as GameObject;
            if (go != null)
            {
                // 用 Addressables.ReleaseInstance 卸载实例 + 资源引用
                UnityUtility.UnloadInstance(go);
            }
            else
            {
                // 普通资源（Texture, Sprite, ScriptableObject 等）
                UnityUtility.UnloadInstance(handle);
            }
        }
        else
        {
            // handle.Result 不是 UnityEngine.Object —— 例如某些非资源操作 (Scene load, data only)  
            // 仅释放 handle 即可
            UnityUtility.UnloadInstance(handle);
        }
    }

    public override void UpdateContent(int index)
    {
        if (index + 1 != _cardSetID)
        {
            // Debug.LogError($"ResetPage{_cardSetID}to{index + 1}");
            Release();
            CardSet cardSet = CardModel.Instance.CardSetDict[index + 1];
            Init(cardSet);
        }
    }
}
