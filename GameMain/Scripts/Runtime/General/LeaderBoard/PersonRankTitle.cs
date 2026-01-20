using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PersonRankTitle : MonoBehaviour
{
    public Image titleImg;
    public TextMeshProUGUILocalize titleText;

    private AsyncOperationHandle m_LoadAssetHandle;

    private void Start()
    {
        OnInit();
    }

    private void OnDestroy()
    {
        if (m_LoadAssetHandle.IsValid())
        {
            Addressables.Release(m_LoadAssetHandle);
            m_LoadAssetHandle = default;
        }
    }

    private void OnInit()
    {
        if (m_LoadAssetHandle.IsValid())
        {
            Addressables.Release(m_LoadAssetHandle);
        }
        var titleImgName = $"Title{(int)GameManager.Task.PersonRankManager.RankLevel + 1}";
        m_LoadAssetHandle = Addressables.LoadAssetAsync<SpriteAtlas>("LeaderBoard");
        m_LoadAssetHandle.Completed += handle =>
         {
             var atlas = handle.Result as SpriteAtlas;
             if (atlas != null)
             {
                 var sprite = atlas.GetSprite(titleImgName);
                 titleImg.sprite = sprite;
             }
         };
        // loadAssetHandle = UnityUtility.LoadSpriteAsync(titleImgName, "LeaderBoard", sp =>
        //   {
        //       titleImg.sprite = sp;
        //   });
        var presetName = $"Title_{GameManager.Task.PersonRankManager.RankLevel.ToString()}";
        titleText.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()} League");
        if (Enum.TryParse(presetName, out MaterialPresetName preset))
        {
            titleText.SetMaterialPreset(preset);
        }
    }
}
