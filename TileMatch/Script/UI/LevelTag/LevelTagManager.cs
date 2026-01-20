using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelTagManager : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> m_LevelTagHandle;

    public void Initialize(LevelPlayType type)
    {
        Release();
 
        if (Merge.MergeManager.Instance != null && Merge.MergeManager.Instance.CheckLevelWinCanGetTarget())
        {
            m_LevelTagHandle = UnityUtility.InstantiateAsync("LevelTag_Merge_" + Merge.MergeManager.Instance.Theme.ToString(), transform, obj =>
             {
                 LevelTag levelTag = obj.GetComponent<LevelTag>();
                 levelTag.m_TagSpine.Initialize(false);
                 levelTag.m_TagSpine.AnimationState.SetAnimation(0, "idle", false);

                 if (Merge.MergeManager.Instance.CheckLevelWinGainedTargetNumAffectedByFirstTry())
                 {
                     bool isFirstTry = PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + GameManager.PlayerData.NowLevel, 0) == 0;
                     levelTag.m_RewardsTitle.SetActive(!isFirstTry);
                     levelTag.m_FirstTryTitle.SetActive(isFirstTry);
                     levelTag.m_NumText.text = "x" + Merge.MergeManager.Instance.GetLevelWinCanGetTargetNum(isFirstTry ? 0 : 1, 0).ToString();
                 }
                 else
                 {
                     levelTag.m_RewardsTitle.SetActive(true);
                     levelTag.m_FirstTryTitle.SetActive(false);
                     int nowLevel = GameManager.PlayerData.RealLevel();
                     var hardIndex = DTLevelUtil.GetLevelHard(nowLevel);
                     levelTag.m_NumText.text = "x" + Merge.MergeManager.Instance.GetLevelWinCanGetTargetNum(0, hardIndex).ToString();
                 }
             });
        }
        else if (KitchenManager.Instance != null && KitchenManager.Instance.CheckLevelWinCanGetTarget())
        {
            m_LevelTagHandle = UnityUtility.InstantiateAsync("LevelTag_Kitchen", transform, obj =>
            {
                bool isFirstTry = PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + GameManager.PlayerData.NowLevel, 0) == 0;

                LevelTag levelTag = obj.GetComponent<LevelTag>();
                levelTag.m_TagSpine.Initialize(false);
                levelTag.m_TagSpine.AnimationState.SetAnimation(0, "idle", false);
                levelTag.m_RewardsTitle.SetActive(!isFirstTry);
                levelTag.m_FirstTryTitle.SetActive(isFirstTry);
                levelTag.m_NumText.text = isFirstTry ? "x3" : "x1";
            });
        }
        else if (HarvestKitchenManager.Instance != null && HarvestKitchenManager.Instance.CheckLevelWinCanGetTarget())
        {
            m_LevelTagHandle = UnityUtility.InstantiateAsync("LevelTag_HarvestKitchen", transform, obj =>
            {
                bool isFirstTry = PlayerPrefs.GetInt(Constant.PlayerData.LevelFailTime + GameManager.PlayerData.NowLevel, 0) == 0;

                LevelTag levelTag = obj.GetComponent<LevelTag>();
                levelTag.m_TagSpine.Initialize(false);
                levelTag.m_TagSpine.AnimationState.SetAnimation(0, "Hangtag", false);
                levelTag.m_RewardsTitle.SetActive(!isFirstTry);
                levelTag.m_FirstTryTitle.SetActive(isFirstTry);
                levelTag.m_NumText.text = isFirstTry ? "x3" : "x1";
            });
        }
    }

    public void Release()
    {
        UnityUtility.UnloadInstance(m_LevelTagHandle);
        m_LevelTagHandle = default;
    }
}
