using DG.Tweening;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具基类
    /// </summary>
    public abstract class Prop : MonoBehaviour
    {
        public GameObject m_Body;
        private PropLogic m_PropLogic;
        private PropMovementState m_MovementState;

        private PropGenerateEffect m_PropGenerateEffect = null;
        private PropMergeEffect m_PropMergeEffect = null;
        private PropCanMergeEffect m_PropCanMergeEffect = null;

        //Animation
        private float m_BodyScale = 1f;
        private float m_ShowAnimTime = 0;

        //Layer
        private string m_CurLayerName = null;
        private int m_CurSortOrder = 0;

        private bool m_IsLoadingCanMergeEffect = false;
        private int m_HoverEventId = 0;

        /// <summary>
        /// 道具逻辑
        /// </summary>
        public PropLogic PropLogic { get { return m_PropLogic; } }

        /// <summary>
        /// 移动状态
        /// </summary>
        public PropMovementState MovementState
        {
            get
            {
                return m_MovementState;
            }
            private set
            {
                if (value == PropMovementState.Static && m_MovementState != PropMovementState.Static && m_PropLogic != null)
                {
                    m_MovementState = value;
                    m_PropLogic.OnSetStatic();
                    return;
                }
                m_MovementState = value;
            }
        }

        /// <summary>
        /// 主体大小
        /// </summary>
        public float BodyScale
        {
            get
            {
                return m_BodyScale;
            }
            set
            {
                m_BodyScale = value;
            }
        }

        /// <summary>
        /// 初始化道具
        /// </summary>
        public virtual void Initialize(PropLogic propLogic)
        {
            if (propLogic == null)
            {
                Log.Warning("Initialize fail - propLogic is null");
                return;
            }

            m_PropLogic = propLogic;

            SetLayer("UI", 7);
        }

        /// <summary>
        /// 重置道具
        /// </summary>
        public virtual void OnReset()
        {
            ClearAnim();
        }

        /// <summary>
        /// 释放道具
        /// </summary>
        public virtual void OnRelease()
        {
            OnReset();

            transform.DOKill();
            m_Body.transform.DOKill();

            m_PropLogic = null;

            m_MovementState = PropMovementState.Static;

            m_ShowAnimTime = 0;
            m_IsLoadingCanMergeEffect = false;

            RecyclePropGenerateEffect();
            RecyclePropMergeEffect();
            RecyclePropCanMergeEffect();

            if (m_HoverEventId != 0)
                GameManager.Task.RemoveDelayTriggerTask(m_HoverEventId);
        }

        private void Update()
        {
            if (m_ShowAnimTime > 0)
            {
                m_ShowAnimTime -= Time.deltaTime;
            }

            if (m_PropLogic != null)
            {
                if (m_PropLogic.MovementState == PropMovementState.Draging)
                {
                    Vector3 mouseWorldPos = MergeManager.Merge.EyeCamera.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 dragedPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
                    Vector3 targetPos = Vector3.Lerp(transform.position, dragedPos, 0.4f);
                    transform.position = targetPos;

                    if (m_PropLogic.AttachmentId != 0 && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
                    {
                        m_PropLogic.AttachmentLogic.Attachment.transform.position = targetPos;
                    }
                }

                if (m_PropLogic.MovementState == PropMovementState.Moveing)
                {
                    if (m_PropLogic.Square != null)
                    {
                        Vector3 movePos = new Vector3(m_PropLogic.Square.transform.position.x, m_PropLogic.Square.transform.position.y, transform.position.z);
                        Vector3 targetPos = Vector3.Lerp(transform.position, movePos, 0.2f);
                        transform.position = targetPos;

                        if (Vector3.Distance(transform.position, movePos) < 0.01f)
                        {
                            targetPos = m_PropLogic.Square.transform.position;
                            transform.position = targetPos;
                            MovementState = PropMovementState.Static;

                            OnMoveEnd();
                        }

                        if (m_PropLogic.AttachmentId != 0 && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
                        {
                            m_PropLogic.AttachmentLogic.Attachment.transform.position = targetPos;
                        }
                    }
                    else
                    {
                        MovementState = PropMovementState.Static;
                        OnMoveEnd();
                    }
                }

                if (m_PropLogic.MovementState == PropMovementState.Bouncing)
                {
                    if (m_PropLogic.AttachmentId != 0 && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
                    {
                        m_PropLogic.AttachmentLogic.Attachment.transform.position = transform.position;
                    }
                }
            }
        }

        /// <summary>
        /// 设置新的移动状态
        /// </summary>
        /// <param name="movementState">生成时移动状态</param>
        public void SetMovementState(PropMovementState movementState)
        {
            if (movementState == PropMovementState.Static)
            {
                if (m_PropLogic != null && m_PropLogic.Square != null) 
                {
                    transform.position = new Vector3(m_PropLogic.Square.transform.position.x, m_PropLogic.Square.transform.position.y, transform.position.z);
                }
            }
            else if (movementState == PropMovementState.Moveing)
            {
                m_MovementState = PropMovementState.Moveing;
                OnMoveStart();
            }
            else if (movementState == PropMovementState.Bouncing)
            {
                m_MovementState = PropMovementState.Bouncing;
                OnBounceStart();
            }
            else if (movementState == PropMovementState.Flying)
            {
                m_MovementState = PropMovementState.Flying;
                OnFlyStart();
            }
            else if (movementState == PropMovementState.Hide)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 拖动开始时
        /// </summary>
        public virtual void OnDragStart()
        {
            ClearAnim();
            transform.localScale = Vector3.one * m_BodyScale;
            m_MovementState = PropMovementState.Draging;

            SetLayer("UI", 9);
        }

        /// <summary>
        /// 拖动结束时
        /// </summary>
        public virtual void OnDragEnd()
        {
            if (m_PropLogic.Square != null)
            {
                m_MovementState = PropMovementState.Moveing;
                OnMoveStart();
            }
            else
            {
                MovementState = PropMovementState.Static;
            }

            MergeManager.Merge.SavePropDistributedMap();
            GameManager.Event.Fire(this, ChessboardLayoutChangeEventArgs.Create());
        }

        /// <summary>
        /// 移动开始时
        /// </summary>
        public virtual void OnMoveStart()
        {
            transform.DOKill();
            SetLayer("UI", 9);

            m_PropLogic.OnMoveStart();
        }

        /// <summary>
        /// 移动结束时
        /// </summary>
        public virtual void OnMoveEnd()
        {
            SetLayer("UI", 7);

            if (MergeManager.Merge.SelectedProp == m_PropLogic)
            {
                MergeManager.Merge.ShowPropSelectedBox(transform.position);
            }

            m_PropLogic.OnMoveEnd();
        }

        /// <summary>
        /// 弹跳开始时
        /// </summary>
        public virtual void OnBounceStart()
        {
            SetLayer("UI", 9);

            Vector3 startPos = transform.position;
            Vector3 targetPos = m_PropLogic.Square.transform.position;
            ClearAnim();

            float jumpPower = 0.1f;
            if (targetPos.y < startPos.y)
                jumpPower = 0.2f;

            transform.DOJump(targetPos, jumpPower, 1, 0.4f).onComplete = () =>
            {
                transform.position = targetPos;

                if (m_PropLogic.AttachmentId != 0 && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
                {
                    m_PropLogic.AttachmentLogic.Attachment.transform.position = transform.position;
                }

                MovementState = PropMovementState.Static;
                OnBounceEnd();
                ShowBounceEndAnim();
            };

            m_PropLogic.OnBounceStart();
        }

        /// <summary>
        /// 弹跳结束时
        /// </summary>
        public virtual void OnBounceEnd()
        {
            SetLayer("UI", 7);

            if (MergeManager.Merge.SelectedProp == m_PropLogic)
            {
                MergeManager.Merge.ShowPropSelectedBox(transform.position);
            }

            m_PropLogic.ShowPropGenerateEffect();
            m_PropLogic.OnBounceEnd();
        }

        /// <summary>
        /// 飞行开始时
        /// </summary>
        public virtual void OnFlyStart()
        {
            SetLayer("UI", 9);

            Vector3 startPos = transform.position;
            Vector3 targetPos = m_PropLogic.Square.transform.position;
            ClearAnim();

            transform.DOMove(targetPos, 0.3f).onComplete = () =>
            {
                transform.position = targetPos;

                MovementState = PropMovementState.Static;
                OnBounceEnd();
                ShowBounceEndAnim();
            };

            m_PropLogic.OnBounceStart();
        }

        /// <summary>
        /// 被选中时
        /// </summary>
        public virtual void OnSelected()
        {
            if (PropLogic.IsPetrified)
            {
                if (MergeManager.Instance.Theme == MergeTheme.DigTreasure && PropLogic.AttachmentId == 5)
                {
                    MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                    if (mainBoard != null)
                        mainBoard.ShowWeakHint("Merge.Locked");
                }

                return;
            }

            ClearAnim();
            m_ShowAnimTime = 0.8f;

            Transform cachedTrans = transform;
            cachedTrans.DOScale(m_BodyScale * 0.8f, 0.2f).onComplete = () =>
            {
                cachedTrans.DOScale(m_BodyScale * 1.1f, 0.15f).onComplete = () =>
                {
                    cachedTrans.DOScale(m_BodyScale * 0.95f, 0.15f).onComplete = () =>
                    {
                        cachedTrans.DOScale(m_BodyScale * 1.05f, 0.15f).onComplete = () =>
                        {
                            cachedTrans.DOScale(m_BodyScale, 0.15f);
                        };
                    };
                };
            };

            if (m_PropLogic.AttachmentId != 0 && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
            {
                m_PropLogic.AttachmentLogic.Attachment.OnSelected();
            }
        }

        /// <summary>
        /// 选中状态点击时
        /// </summary>
        public virtual void OnClick()
        {
        }

        /// <summary>
        /// 选中状态双击时
        /// </summary>
        public virtual void OnDoubleClick()
        {
        }

        /// <summary>
        /// 当拖动其它道具进入该道具上方时
        /// </summary>
        /// <param name="hoverProp">处于上方的道具</param>
        /// <returns>是否可以合成</returns>
        public virtual bool OnHoverEnter(PropLogic hoverProp)
        {
            if (hoverProp != null && hoverProp.PropId == m_PropLogic.PropId && hoverProp != m_PropLogic && !m_PropLogic.IsSilenced && !hoverProp.IsSilenced)
            {
                var mergeRoute = MergeManager.Merge.GetMergeRouteList(hoverProp.PropId);
                if (mergeRoute != null)
                {
                    int index = mergeRoute.FindIndex(i => i.Equals(hoverProp.PropId));
                    if (index >= 0 && index < mergeRoute.Count - 1)
                    {
                        if (m_HoverEventId != 0)
                            GameManager.Task.RemoveDelayTriggerTask(m_HoverEventId);
                        m_HoverEventId = GameManager.Task.AddDelayTriggerTask(0.2f, ShowPropCanMergeEffect);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 当拖动其它道具离开该道具上方时
        /// </summary>
        public virtual void OnHoverExit()
        {
            HidePropCanMergeEffect();

            if (m_HoverEventId != 0)
                GameManager.Task.RemoveDelayTriggerTask(m_HoverEventId);
        }

        /// <summary>
        /// 当其它物体放到该道具上时
        /// </summary>
        /// <param name="prop">放上的道具</param>
        public virtual Square OnPutOn(PropLogic prop)
        {
            Square endSquare = null;
            if (MovementState == PropMovementState.Static && !m_PropLogic.IsPetrified)
            {
                Square square = m_PropLogic.Square;
                if (!m_PropLogic.IsSilenced && !prop.IsSilenced)
                {
                    //尝试合成道具
                    if (MergeManager.Merge.MergeProp(prop, m_PropLogic, square))
                    {
                        endSquare = square;
                    }
                }

                if (endSquare == null && !m_PropLogic.IsImmovable)
                {
                    Square nearSquare = MergeManager.Merge.GetNearestEmptySquare(square);
                    if (nearSquare != null)
                    {
                        endSquare = square;
                        m_PropLogic.SetNewSqaure(nearSquare, PropMovementState.Moveing);
                    }
                    else
                    {
                        endSquare = square;
                        m_PropLogic.SetNewSqaure(prop.Square, PropMovementState.Moveing);
                    }
                }
            }

            return endSquare;
        }

        /// <summary>
        /// 合成生成时
        /// </summary>
        public virtual void OnGeneratedByMerge()
        {
            //ShowPunchAnim();
            ClearAnim();
            //gameObject.SetActive(false);

            float scale = m_Body.transform.localScale.x;
            m_Body.transform.localScale = new Vector3(scale * 0.9f, scale * 0.9f);
            gameObject.SetActive(true);

            ShowPropMergeEffect(() =>
            {
                m_Body.transform.DOScale(scale * 0.6f, 0.08f).SetEase(Ease.InOutSine).onComplete = () =>
                {
                    m_Body.transform.DOScale(scale * 1.1f, 0.12f).onComplete = () =>
                    {
                        m_Body.transform.DOScale(scale, 0.1f).SetEase(Ease.InQuad);
                    };
                };
            });
        }

        /// <summary>
        /// 展示punch动画
        /// </summary>
        public void ShowPunchAnim()
        {
            ClearAnim();
            transform.localScale = new Vector3(m_BodyScale * 0.7f, m_BodyScale * 0.7f);
            gameObject.SetActive(true);

            transform.DOScale(m_BodyScale * 1.1f, 0.15f).onComplete = () =>
            {
                transform.DOScale(m_BodyScale * 0.95f, 0.15f).onComplete = () =>
                {
                    transform.DOScale(m_BodyScale, 0.15f);
                };
            };
        }

        public void ShowBounceEndAnim()
        {
            if (m_PropLogic != null && m_PropLogic.AttachmentId == 1)
                return;

            ClearAnim();
            gameObject.SetActive(true);

            transform.DOScale(m_BodyScale * 0.8f, 0.15f).onComplete = () =>
            {
                transform.DOScale(m_BodyScale * 1.05f, 0.15f).onComplete = () =>
                {
                    transform.DOScale(m_BodyScale, 0.15f);
                };
            };
        }

        /// <summary>
        /// 设置为灰色状态
        /// </summary>
        public virtual void SetGray()
        {
            var sp = m_Body.GetComponentInChildren<SpriteRenderer>();
            if (sp != null)
            {
                sp.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        /// <summary>
        /// 设置为正常状态
        /// </summary>
        public virtual void SetNormal()
        {
            var sp = m_Body.GetComponentInChildren<SpriteRenderer>();
            if (sp != null)
            {
                sp.color = Color.white;
            }
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        public virtual void SetLayer(string layerName, int sortOrder)
        {
            m_CurLayerName = layerName;
            m_CurSortOrder = sortOrder;

            if (m_PropLogic != null && m_PropLogic.AttachmentLogic != null && m_PropLogic.AttachmentLogic.Attachment != null)
                m_PropLogic.AttachmentLogic.Attachment.SetLayer(layerName, sortOrder);
        }

        #region Animation

        /// <summary>
        /// 清除当前动画
        /// </summary>
        public void ClearAnim()
        {
            if (m_ShowAnimTime > 0)
            {
                transform.DOKill();
                transform.localScale = Vector3.one * m_BodyScale;
                m_Body.transform.DOKill();
                m_Body.transform.localPosition = Vector3.zero;
                m_ShowAnimTime = 0;
            }
        }

        /// <summary>
        /// 显示合成提示的动画
        /// </summary>
        public void ShowMergeHintAnim(Vector3 direction)
        {
            ClearAnim();
            m_ShowAnimTime = 0.9f;

            if (direction != Vector3.zero)
            {
                m_Body.transform.DOBlendableLocalMoveBy(direction * 10f, 0.15f).onComplete = () =>
                {
                    m_Body.transform.DOBlendableLocalMoveBy(-direction * 10f, 0.15f).onComplete = () =>
                    {
                        m_Body.transform.DOBlendableLocalMoveBy(direction * 10f, 0.15f).onComplete = () =>
                        {
                            m_Body.transform.DOBlendableLocalMoveBy(-direction * 10f, 0.15f);
                        };
                    };
                };

                transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.15f).onComplete = () =>
                {
                    transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f).SetEase(Ease.OutSine).onComplete = () =>
                    {
                        transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.15f).onComplete = () =>
                        {
                            transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f).SetEase(Ease.OutSine);
                        };
                    };
                };
            }
            else
            {
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                if (mainBoard != null && PropLogic != null && PropLogic.Square != null)
                    mainBoard.ShowFingerAnim(PropLogic.Square.transform.position);

                //transform.DOScale(m_BodyScale * 1.25f, 0.2f).onComplete = () =>
                //{
                //    transform.DOScale(m_BodyScale, 0.7f).SetEase(Ease.OutBounce);
                //};
            }
        }

        #endregion

        /// <summary>
        /// 显示道具生成特效
        /// </summary>
        public void ShowPropGenerateEffect()
        {
            GameManager.ObjectPool.Spawn<EffectObject>("FX_ItemGenerate", "PropEffectPool", transform.position, Quaternion.identity, transform, res =>
            {
                GameObject effect = (GameObject)res.Target;
                if (m_PropLogic == null)
                {
                    GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", effect);
                    return;
                }

                effect.transform.localPosition = Vector3.zero;
                m_PropGenerateEffect = effect.GetComponent<PropGenerateEffect>();
                m_PropGenerateEffect.Show();

                GameManager.Task.AddDelayTriggerTask(1.3f, RecyclePropGenerateEffect);

                if (!string.IsNullOrEmpty(m_CurLayerName))
                    m_PropGenerateEffect.SetLayer(m_CurLayerName, m_CurSortOrder + 1);
            });
        }

        /// <summary>
        /// 显示道具合成特效
        /// </summary>
        private void ShowPropMergeEffect(System.Action callback)
        {
            //if (m_PropLogic.PropId / 10000 != 2 && m_PropLogic.Rank <= 2)
            //{
            //    callback?.Invoke();
            //    return;
            //}

            GameManager.ObjectPool.Spawn<EffectObject>("PropMergeEffect", "PropEffectPool", transform.position, Quaternion.identity, transform, res =>
            {
                GameObject effect = (GameObject)res.Target;
                if (m_PropLogic == null)
                {
                    GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", effect);
                    callback?.Invoke();
                    return;
                }

                effect.transform.localPosition = Vector3.zero;
                m_PropMergeEffect = effect.GetComponent<PropMergeEffect>();
                m_PropMergeEffect.Show();
                callback?.Invoke();

                GameManager.Task.AddDelayTriggerTask(1.3f, RecyclePropMergeEffect);

                if (!string.IsNullOrEmpty(m_CurLayerName))
                    m_PropMergeEffect.SetLayer(m_CurLayerName, m_CurSortOrder + 1);
            });
        }

        /// <summary>
        /// 显示道具可合成特效
        /// </summary>
        private void ShowPropCanMergeEffect()
        {
            if (m_IsLoadingCanMergeEffect)
                return;

            m_IsLoadingCanMergeEffect = true;
            PropLogic curHoverProp = MergeManager.Merge.CurHoverProp;
            GameManager.ObjectPool.Spawn<EffectObject>("PropCanMergeEffect", "PropEffectPool", transform.position, Quaternion.identity, transform, res =>
            {
                m_IsLoadingCanMergeEffect = false;
                GameObject effect = (GameObject)res.Target;
                if (m_PropLogic == null || curHoverProp != MergeManager.Merge.CurHoverProp)
                {
                    GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", effect);
                    return;
                }

                effect.transform.localPosition = Vector3.zero;
                m_PropCanMergeEffect = effect.GetComponent<PropCanMergeEffect>();
                m_PropCanMergeEffect.Show();

                if (!string.IsNullOrEmpty(m_CurLayerName))
                    m_PropCanMergeEffect.SetLayer(m_CurLayerName, m_CurSortOrder + 1);
            });
        }

        private void HidePropCanMergeEffect()
        {
            if (m_PropCanMergeEffect != null)
            {
                m_PropCanMergeEffect.Hide(RecyclePropCanMergeEffect);
            }
        }

        private void RecyclePropGenerateEffect()
        {
            if (m_PropGenerateEffect != null)
            {
                GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", m_PropGenerateEffect.gameObject);
                m_PropGenerateEffect = null;
            }
        }

        private void RecyclePropMergeEffect()
        {
            if (m_PropMergeEffect != null)
            {
                GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", m_PropMergeEffect.gameObject);
                m_PropMergeEffect = null;
            }
        }

        private void RecyclePropCanMergeEffect()
        {
            if (m_PropCanMergeEffect != null)
            {
                m_PropCanMergeEffect.gameObject.SetActive(false);
                GameManager.ObjectPool.Unspawn<EffectObject>("PropEffectPool", m_PropCanMergeEffect.gameObject);
                m_PropCanMergeEffect = null;
            }
        }

        #region Save Data

        public virtual void Save(PropSavedData savedData)
        {
        }

        public virtual bool Load(PropSavedData savedData)
        {
            return true;
        }

        #endregion
    }
}
