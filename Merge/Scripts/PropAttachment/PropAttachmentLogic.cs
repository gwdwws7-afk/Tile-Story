using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class PropAttachmentLogic
    {
        private int m_AttachmentId;
        private PropLogic m_PropLogic;
        private PropAttachment m_Attachment;
        private AsyncOperationHandle<GameObject> m_AttachmentHandle;
        protected Transform m_SpawnRoot;

        public PropAttachmentLogic()
        {
            m_AttachmentId = 0;
            m_PropLogic = null;
            m_Attachment = null;
            m_AttachmentHandle = default;
            m_SpawnRoot = null;
        }
        
        public PropLogic PropLogic { get { return m_PropLogic; } }
        
        public PropAttachment Attachment { get { return m_Attachment; } }
        
        public void Initialize(int attachmentId, PropLogic prop, Vector3 position, Transform spawnRoot)
        {
            if (prop == null)
            {
                Log.Error("PropAttachmentLogic Initialize() fail - PropLogic is null");
                return;
            }

            m_AttachmentId = attachmentId;
            m_PropLogic = prop;
            m_SpawnRoot = spawnRoot;

            OnInitialize(m_PropLogic);

            SpawnAttachment(position, spawnRoot);
        }
        
        public void Release(bool isShutdown)
        {
            if (m_AttachmentId != 0)
            {
                m_AttachmentId = 0;
                PropLogic propLogic = m_PropLogic;

                if (m_PropLogic != null)
                {
                    m_PropLogic = null;
                    propLogic.ReleaseAttachment();
                }

                UnspawnAttachment(isShutdown);

                OnRelease(propLogic, isShutdown);
            }
        }

        protected virtual void SpawnAttachment(Vector3 position, Transform spawnRoot)
        {
            if (m_AttachmentHandle.IsValid())
                Addressables.ReleaseInstance(m_AttachmentHandle);

            IDataTable<DRAttachment> attachmentDataTable = MergeManager.DataTable.GetDataTable<DRAttachment>(MergeManager.Instance.GetMergeDataTableName());
            DRAttachment m_AttachmentData = attachmentDataTable.GetDataRow(m_AttachmentId);
            if (m_AttachmentData != null)
            {
                m_AttachmentHandle = Addressables.InstantiateAsync(MergeManager.Instance.GetPropAssetName(m_AttachmentData.AssetName), position, Quaternion.identity, spawnRoot);
                m_AttachmentHandle.Completed += handle =>
                {
                    m_Attachment = handle.Result.GetComponent<PropAttachment>();
                    m_Attachment.Initialize(m_PropLogic);
                };
            }
            else
            {
                Log.Error("Attachment Id {0} data row is not exist", m_AttachmentId);
            }
        }

        protected virtual void UnspawnAttachment(bool isShutdown)
        {
            m_Attachment = null;
            Addressables.ReleaseInstance(m_AttachmentHandle);
        }
        
        public virtual void OnInitialize(PropLogic propLogic)
        {
        }
        
        public virtual void OnRelease(PropLogic propLogic, bool isShutdown)
        {
        }
        
        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }
        
        public virtual void OnOperationOccurAround(PropOperation operation)
        {
        }
    }
}