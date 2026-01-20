using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class MergeFlyItemSlot : MonoBehaviour
    {
        public SpriteRenderer itemImage;

        private AsyncOperationHandle asyncHandle;

        public void Initialize(int propId)
        {
            IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
            var propData = propDataTable.GetDataRow(propId);
            string spriteName = propData.AssetName;

            UnityUtility.UnloadAssetAsync(asyncHandle);
            asyncHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetSpriteKey(spriteName, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            {
                itemImage.sprite = sp;
            });
        }

        public void Release()
        {
            UnityUtility.UnloadAssetAsync(asyncHandle);
            asyncHandle = default;
        }
    }
}
