using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class FrogLeafManager : MonoBehaviour
{
    [SerializeField] private Image rewardImage;
    [SerializeField] private Image tickImage;
    [SerializeField] private TextMeshProUGUI numText;
    [SerializeField] private SkeletonGraphic leafSpine;
    [SerializeField] private ParticleSystem leafParticle;
    
    private AsyncOperationHandle _asyncHandle;

    public int Level { get; private set; }
    public KeyValuePair<TotalItemData,int> rewardData;
    private string itemName;
    public void Init(int level)
    {
        Level = level;
        GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.FrogLeafCreated,this,Level));

        if (level == 0)
        {
            rewardImage.gameObject.SetActive(false);
            tickImage.gameObject.SetActive(false);
            numText.gameObject.SetActive(false);
            return;
        }
        
        rewardData = GameManager.PlayerData.FrogJumpData.GetReward(level);
        itemName = UnityUtility.GetRewardSpriteKey(rewardData.Key, rewardData.Value);
        //if (_asyncHandle.IsValid())
        //{
        //    UnityUtility.UnloadAssetAsync(_asyncHandle);
        //}
        Log.Info($"LoadSpriteAsync Success {rewardData.Key.TotalItemType} {rewardData.Value}, { UnityUtility.GetRewardSpriteKey(rewardData.Key, rewardData.Value) }");

        _asyncHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetSpriteKey(itemName, "TotalItemAtlas"),
            sp =>
            {
                var sprite =sp as Sprite;
                if (sprite.name.StartsWith(itemName))
                {
                    rewardImage.sprite = sprite;
                    rewardImage.SetNativeSize();
                }
            });
        
        if (level <= GameManager.PlayerData.CurFrogJumpLevel)
        {
            rewardImage.gameObject.SetActive(false);
            tickImage.gameObject.SetActive(false);
            numText.gameObject.SetActive(false);
        }
        else
        {
            rewardImage.gameObject.SetActive(true);
            tickImage.gameObject.SetActive(level< GameManager.PlayerData.CurFrogJumpLevel);
            numText.gameObject.SetActive(true);

            string otherStr = rewardData.Key.TotalItemType.ToString().Contains("Infinite") ? "m" : string.Empty;
            numText.text = rewardData.Value.ToString()+otherStr;
            if (rewardData.Key == TotalItemData.Coin)
                rewardImage.transform.localScale = Vector3.one * 0.5f;
            else
                rewardImage.transform.localScale = Vector3.one * 0.4f;
            
        }
    }

    public void ShowActiveAnim()
    {
        if (leafSpine.AnimationState == null)
        {
            leafSpine.Initialize(true);
        }
        leafParticle.Play();
        leafSpine.AnimationState.SetAnimation(0, "active", false).Complete+= entry =>
        {
            leafSpine.AnimationState.SetAnimation(0, "idle", true);
        };
    }
    
    public void OnReset()
    {
        GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.FrogLeafDestroy,Level));
        Level = -1;
        if (_asyncHandle.IsValid())
        {
            UnityUtility.UnloadAssetAsync(_asyncHandle);
            _asyncHandle = default;
        }
    }

    public void Refresh(int curLevel)
    {
        if (Level <= curLevel || Level==0)
        {
            rewardImage.gameObject.SetActive(false);
            tickImage.gameObject.SetActive(false);
            numText.gameObject.SetActive(false);
        }
        else
        {
            rewardImage.gameObject.SetActive(true);
            tickImage.gameObject.SetActive(Level< curLevel);
            numText.gameObject.SetActive(true);
            
            string otherStr = rewardData.Key.TotalItemType.ToString().Contains("Infinite") ? "m" : string.Empty;
            numText.text = rewardData.Value.ToString()+otherStr;
        }
    }
}
