using GameFramework;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public sealed class PropLogic : IReference
    {
        private int m_SerialId;
        private int m_PropId;
        private int m_AttachmentId;
        private Prop m_Prop;
        private PropAttachmentLogic m_AttachmentLogic;
        private Square m_Square;

        //DataTable
        private DRProp m_PropData;

        //State
        private bool m_IsPetrified;
        private bool m_IsImmovable;
        private bool m_IsSilenced;

        private AsyncOperationHandle<GameObject> m_PropHandle;
        private PropMovementState m_SpawnPropMovementState;
        private Action<Prop> m_SpawnPropComplete;
        private Action m_SetStaticAction;
        private float m_DoubleClickTimer;

        public PropLogic()
        {
            Clear();
        }
        
        public int SerialId { get { return m_SerialId; } }
        
        public int PropId { get { return m_PropId; } }
        
        public Prop Prop { get { return m_Prop; } }
        
        public Square Square { get { return m_Square; } }
        
        public int AttachmentId { get { return m_AttachmentId; } }
        
        public PropAttachmentLogic AttachmentLogic { get { return m_AttachmentLogic; } }

        #region State

        /// <summary>
        /// �ƶ�״̬
        /// </summary>
        public PropMovementState MovementState
        {
            get
            {
                if (m_Prop == null)
                    return PropMovementState.Static;
                return m_Prop.MovementState;
            }
        }

        /// <summary>
        /// ���ߵȼ�
        /// </summary>
        public int Rank
        {
            get
            {
                return MergeManager.Merge.GetPropRank(m_PropId);
            }
        }

        /// <summary>
        /// �ϳ�·�߱��
        /// </summary>
        public int MergeRouteId
        {
            get
            {
                return MergeManager.Merge.GetMergeRouteId(m_PropId);
            }
        }

        /// <summary>
        /// �Ƿ�ѡ��
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return MergeManager.Merge.SelectedProp == this;
            }
        }

        /// <summary>
        /// �Ƿ�ʯ�����޷�ѡ�У��޷��ϳɣ���ͣ���й��ܣ�
        /// </summary>
        public bool IsPetrified
        {
            get
            {
                return m_IsPetrified;
            }
            set
            {
                m_IsPetrified = value;
            }
        }

        /// <summary>
        /// �Ƿ񲻿��ƶ��������϶������ɱ�����ȥ����ͣ���й��ܣ�
        /// </summary>
        public bool IsImmovable
        {
            get
            {
                return m_IsImmovable;
            }
            set
            {
                m_IsImmovable = value;
            }
        }

        /// <summary>
        /// �Ƿ񱻳�Ĭ���޷��ϳɣ���ͣ���й��ܣ�
        /// </summary>
        public bool IsSilenced
        {
            get
            {
                return m_IsSilenced;
            }
            set
            {
                m_IsSilenced = value;
            }
        }

        #endregion
        
        public event Action<Prop> SpawnPropComplete
        {
            add
            {
                m_SpawnPropComplete += value;
            }
            remove
            {
                m_SpawnPropComplete -= value;
            }
        }
        
        public event Action SetStaticAction
        {
            add
            {
                m_SetStaticAction += value;

                if (MovementState == PropMovementState.Static)
                {
                    m_SetStaticAction?.Invoke();
                    m_SetStaticAction = null;
                }
            }
            remove
            {
                m_SetStaticAction -= value;
            }
        }
        
        public bool Initialize(Vector3 position, Square square, PropMovementState movementState, PropSavedData savedData, Transform spawnRoot)
        {
            IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
            m_PropData = propDataTable.GetDataRow(m_PropId);
            if (m_PropData == null)
                return false;
            
            if (savedData != null)
            {
                Load(savedData);
            }
            else
            {
                Load(ReferencePool.Acquire<PropSavedData>());
            }
            
            if (m_SerialId == 0)
            {
                int savedSerialId = PlayerPrefs.GetInt("MergeSerialId", 0);
                m_SerialId = savedSerialId == int.MaxValue ? 1 : savedSerialId + 1;
                PlayerPrefs.SetInt("MergeSerialId", m_SerialId);
            }

            m_Square = square;
            square.FilledProp = this;

            SpawnProp(position, movementState, spawnRoot);
            SpawnAttachment(m_AttachmentId, position, spawnRoot);

            return true;
        }
        
        public void Reset()
        {
            ClearFilledSquare();

            if (m_Prop != null)
                m_Prop.OnReset();
        }
        
        public void Release(bool isShutdown)
        {
            Reset();

            if (m_AttachmentLogic != null)
                m_AttachmentLogic.Release(isShutdown);

            if (m_Prop != null)
            {
                m_Prop.OnRelease();
            }
            else if (!m_PropHandle.IsDone)
            {
                m_PropHandle.Completed -= OnSpawnPropComplete;
                m_PropHandle.Completed += obj =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        var prop = obj.Result.GetComponent<Prop>();
                        if (prop != null)
                            prop.OnRelease();
                    }
                };
            }

            if (m_PropHandle.IsValid())
            {
                Addressables.ReleaseInstance(m_PropHandle);
            }
            else
            {
                Log.Error("PropHandle is invalid...");
            }
            ReferencePool.Release(this);
        }
        
        public static PropLogic Create(int propId, int attachmentId)
        {
            PropLogic logic = ReferencePool.Acquire<PropLogic>();
            logic.m_PropId = propId;
            logic.m_AttachmentId = attachmentId;
            return logic;
        }
        
        public void Clear()
        {
            m_SerialId = 0;
            m_PropId = 0;
            m_AttachmentId = 0;
            m_Prop = null;
            m_AttachmentLogic = null;
            m_Square = null;

            m_PropData = null;

            m_IsPetrified = false;
            m_IsImmovable = false;
            m_IsSilenced = false;

            m_PropHandle = default;
            m_SpawnPropMovementState = PropMovementState.Static;
            m_SpawnPropComplete = null;
            m_SetStaticAction = null;
            m_DoubleClickTimer = 0;
        }
        
        private void SpawnProp(Vector3 position, PropMovementState movementState, Transform spawnRoot)
        {
            if (!m_PropHandle.IsValid())
            {
                m_SpawnPropMovementState = movementState;
                m_PropHandle = Addressables.InstantiateAsync(MergeManager.Instance.GetPropAssetName(m_PropData.AssetName), position, Quaternion.identity, spawnRoot);
                m_PropHandle.Completed += OnSpawnPropComplete;
            }
        }
        
        private void OnSpawnPropComplete(AsyncOperationHandle<GameObject> result)
        {
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                m_Prop = result.Result.GetComponent<Prop>();
                m_Prop.Initialize(this);
                m_Prop.SetMovementState(m_SpawnPropMovementState);

                if (!m_Prop.gameObject.activeSelf)
                    m_Prop.gameObject.SetActive(true);

                m_SpawnPropComplete?.Invoke(m_Prop);
                m_SpawnPropComplete = null;
            }
        }
        
        public void SpawnAttachment(int attachmentId, Vector3 position, Transform spawnRoot)
        {
            if (attachmentId > 0)
            {
                if (m_AttachmentId != 0 && attachmentId != m_AttachmentId)
                {
                    ReleaseAttachment();
                }
                m_AttachmentId = attachmentId;
                
                IDataTable<DRAttachment> attachmentDataTable = MergeManager.DataTable.GetDataTable<DRAttachment>(MergeManager.Instance.GetMergeDataTableName());
                DRAttachment m_AttachmentData = attachmentDataTable.GetDataRow(attachmentId);
                if (m_AttachmentData != null)
                {
                    string[] splitedString = m_AttachmentData.AssetName.Split('_');
                    m_AttachmentLogic = MergeManager.Instance.GetPropAttachmentLogic(splitedString[0]);
                    if (m_AttachmentLogic != null)
                    {
                        m_AttachmentLogic.Initialize(attachmentId, this, position, spawnRoot);
                    }
                    else
                    {
                        Log.Error("GetPropAttachmentLogic id {0} fail", attachmentId);
                    }
                }
                else
                {
                    Log.Error("Attachment Id {0} data row is not exist", attachmentId);
                }
            }
        }
        
        public void ReleaseAttachment()
        {
            if (m_AttachmentId != 0)
            {
                m_AttachmentId = 0;

                if (m_AttachmentLogic != null)
                {
                    PropAttachmentLogic attachmentLogic = m_AttachmentLogic;
                    m_AttachmentLogic = null;
                    attachmentLogic.Release(false);
                }
            }
        }
        
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_Prop == null)
                return;

            if (m_DoubleClickTimer > 0)
                m_DoubleClickTimer -= realElapseSeconds;

            if (m_AttachmentLogic != null)
                m_AttachmentLogic.Update(elapseSeconds, realElapseSeconds);
        }
        
        public void SetNewSqaure(Square newSquare, PropMovementState movementState)
        {
            ClearFilledSquare();
            m_Square = newSquare;
            newSquare.FilledProp = this;

            m_Prop.SetMovementState(movementState);
        }
        
        public void ClearFilledSquare()
        {
            if (m_Square != null)
            {
                m_Square.FilledProp = null;
            }
            m_Square = null;
        }
        
        public void ShowMergeHintAnim(Vector3 direction)
        {
            if (m_Prop != null)
                m_Prop.ShowMergeHintAnim(direction);
        }
        
        public void OnDragStart()
        {
            m_Prop?.OnDragStart();
        }
        
        public void OnDragEnd(Square endSquare)
        {
            if (endSquare != null)
            {
                if (m_Square != null && m_Square.FilledProp == this)
                {
                    m_Square.FilledProp = null;
                }
                m_Square = endSquare;
                endSquare.FilledProp = this;
            }

            m_Prop?.OnDragEnd();
        }
        
        public void OnMoveStart()
        {
        }
        
        public void OnMoveEnd()
        {
        }
        
        public void OnBounceStart()
        {
        }
        
        public void OnBounceEnd()
        {
        }
        
        public void OnSetStatic()
        {
            if (m_SetStaticAction != null)
            {
                m_SetStaticAction.Invoke();
                m_SetStaticAction = null;
            }
        }
        
        public void OnSelected()
        {
            if (m_Prop != null)
                m_Prop.OnSelected();

            if (MergeManager.Merge.SelectedProp == this)
            {
                MergeManager.Merge.ShowPropSelectedBox(m_Square.transform.position);
            }
        }
        
        public void OnClick()
        {
            if (!m_IsPetrified && !m_IsImmovable && !m_IsSilenced)
            {
                m_Prop.OnClick();
                
                if (m_DoubleClickTimer > 0)
                    OnDoubleClick();
                else
                    m_DoubleClickTimer = 0.5f;
            }
        }
        
        public void OnDoubleClick()
        {
            m_DoubleClickTimer = 0f;

            m_Prop.OnDoubleClick();
        }
        
        public bool OnHoverEnter(PropLogic hoverProp)
        {
            if (!m_IsPetrified && m_Prop != null)
                return m_Prop.OnHoverEnter(hoverProp);

            return false;
        }
        
        public void OnHoverExit()
        {
            if (!m_IsPetrified && m_Prop != null)
                m_Prop.OnHoverExit();
        }
        
        public Square OnPutOn(PropLogic prop)
        {
            Square endSquare = null;

            if (m_Prop != null)
                endSquare = m_Prop.OnPutOn(prop);

            return endSquare;
        }
        
        public void ShowPropGenerateEffect()
        {
            if (m_Prop == null)
            {
                Log.Error("ShowPropGenerateEffect Fail - Prop Instance is null");
                return;
            }

            if (!m_IsPetrified && !m_IsImmovable && !m_IsSilenced)
            {
                m_Prop.ShowPropGenerateEffect();
            }
        }
        
        public void OnOperationOccurAround(PropOperation operation)
        {
            if (m_AttachmentLogic != null)
            {
                m_AttachmentLogic.OnOperationOccurAround(operation);
            }
        }
        
        public void SetGray()
        {
            if (m_PropId != 0)
            {
                if (m_Prop != null)
                    m_Prop.SetGray();
                else
                    m_SpawnPropComplete += p => p.SetGray();
            }
        }
        
        public void SetNormal()
        {
            if (m_PropId != 0)
            {
                if (m_Prop != null)
                    m_Prop.SetNormal();
                else
                    m_SpawnPropComplete += p => p.SetNormal();
            }
        }

        #region Save Data

        public PropSavedData Save()
        {
            PropSavedData savedData = ReferencePool.Acquire<PropSavedData>();

            //����ID
            if (m_SerialId != 0)
                savedData.SetData("SID", m_SerialId.ToString());

            //��ƷID
            savedData.SetData("ID", PropId.ToString());

            if (m_Prop != null)
            {
                m_Prop.Save(savedData);
            }

            return savedData;
        }

        public bool Load(PropSavedData savedData)
        {
            //����ID
            string serialIdString = savedData.GetData("SID");
            if (!string.IsNullOrEmpty(serialIdString))
                m_SerialId = int.Parse(serialIdString);

            if (m_Prop != null)
            {
                m_Prop.Load(savedData);
                ReferencePool.Release(savedData);
            }
            else
            {
                m_SpawnPropComplete += result =>
                {
                    m_Prop.Load(savedData);
                    ReferencePool.Release(savedData);
                };
            }

            return true;
        }

        #endregion
    }
}
