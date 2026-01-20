using GameFramework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 合成模块
    /// </summary>
    public sealed class MergeComponent : MonoBehaviour
    {
        private Square[,] squares;
        private HashSet<PropLogic> allProps;
        private Action<bool> deferredSaveAction;

        private PropLogic selectedProp;
        private PropLogic lastSelectedProp;
        private PropLogic curGrabProp;
        private PropLogic curHoverProp;

        private bool startDrag;
        private Vector3 startInputPos;
        private float dragMoveLimit = 15f;
        private bool isInputDown = false;

        private bool isInitialized = false;
        private float showMergeHintTimer = 10f;
        private float showMergeHintInterval = 3f;
        private bool startShowMergeHint = false;

        /// <summary>
        /// 是否初始化完毕
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return isInitialized;
            }
        }

        /// <summary>
        /// 合成摄像机
        /// </summary>
        public Camera EyeCamera
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前选中的道具
        /// </summary>
        public PropLogic SelectedProp
        {
            get
            {
                return selectedProp;
            }
            set
            {
                lastSelectedProp = selectedProp;
                if (selectedProp != value)
                {
                    selectedProp = value;
                }

                GameManager.Event.Fire(this, SelectedPropChangeEventArgs.Create());
            }
        }

        /// <summary>
        /// 上个选中的道具
        /// </summary>
        public PropLogic LastSelectedProp
        {
            get
            {
                return lastSelectedProp;
            }
        }

        /// <summary>
        /// 当前正在拖动的道具
        /// </summary>
        public PropLogic CurGrabProp
        {
            get
            {
                return curGrabProp;
            }
            set
            {
                curGrabProp = value;
            }
        }

        /// <summary>
        /// 当前正在浮空的道具
        /// </summary>
        public PropLogic CurHoverProp
        {
            get
            {
                return curHoverProp;
            }
            set
            {
                curHoverProp = value;
            }
        }

        public bool StartShowMergeHint
        {
            get => startShowMergeHint;
            set => startShowMergeHint = value;
        }

        public void Initialize(Square[,] squareMap)
        {
            Clear();

            EyeCamera = Camera.main;

            squares = squareMap;
            isInitialized = true;
            startShowMergeHint = true;
        }

        public void Release()
        {
            isInitialized = false;

            if (deferredSaveAction != null)
            {
                try
                {
                    deferredSaveAction.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("MergeComponent DeferredSaveAction error - " + e.Message);
                }
                deferredSaveAction = null;
            }

            if (allProps != null)
            {
                foreach (PropLogic prop in allProps)
                {
                    prop.Release(true);
                }
            }

            Clear();
        }

        private void Clear()
        {
            squares = null;
            allProps = null;
            deferredSaveAction = null;

            selectedProp = null;
            lastSelectedProp = null;
            curGrabProp = null;
            curHoverProp = null;

            startDrag = false;
            isInputDown = false;

            isInitialized = false;
            ResetHintTimer();
            startShowMergeHint = false;
        }

        public void RefreshSquareMap(Square[,] squareMap)
        {
            squares = squareMap;
            deferredSaveAction = null;
            selectedProp = null;
            lastSelectedProp = null;
            curGrabProp = null;
            curHoverProp = null;

            startDrag = false;
            isInputDown = false;

            ResetHintTimer();
            startShowMergeHint = false;
        }

        private void Update()
        {
            if (deferredSaveAction != null)
            {
                try
                {
                    deferredSaveAction.Invoke(true);
                }
                catch (Exception e)
                {
                    Log.Error("MergeComponent DeferredSaveAction error - " + e.Message);
                }
                deferredSaveAction = null;
            }

            if (!isInitialized)
                return;

            HandleInput();

            //所有道具轮询
            if (allProps != null)
            {
                foreach (PropLogic prop in allProps)
                {
                    prop.Update(Time.deltaTime, Time.unscaledDeltaTime);
                }
            }

            //显示提示的计时
            if (startShowMergeHint && !isInputDown && MergeGuideMenu.s_CurGuideId == GuideTriggerType.None)
            {
                if (showMergeHintTimer <= 0) 
                {
                    if (MergeManager.Instance.Theme == MergeTheme.LoveGiftBattle && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_DateMerge5))
                    {
                        showMergeHintTimer = showMergeHintInterval;
                        return;
                    }

                    showMergeHintTimer = showMergeHintInterval;
                    ShowPropHint();
                }
                else
                {
                    showMergeHintTimer -= Time.deltaTime;
                }
            }
        }

        #region Input

        private void HandleInput()
        {
            if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    OnInputDown();
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    OnInput();
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
                {
                    OnInputUp();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnInputDown();
                }
                else if (Input.GetMouseButton(0))
                {
                    OnInput();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    OnInputUp();
                }
            }
        }

        List<Vector3> mousePositionHistoryList = new List<Vector3>();
        Vector3 mousePositionUp = new Vector3();
        private PropLogic tempHoverTargetPropLogic;//曾经 在OnInput更新过程中 Hover到的 可以合成的目标
        private float tempHoverTargetRealTimeSinceStartUp;//记录上述PropLogic时的 Time.realtimeSinceStartup
        private float tempHoverTargetLifeTime = 0.1f;//超过这个时间 就认为tempHoverTarget该失效了
        private float squareSize = 0.0744f * 2;

        private void OnInputDown()
        {
            if (isInputDown)
            {
                if (curGrabProp != null)
                {
                    return;
                }
                else
                {
                    isInputDown = false;
                    startDrag = false;
                }
            }

            //防止UI弹窗点击穿透
            IUIGroup popupUIGroup = GameManager.UI.GetUIGroup("PopupUI");
            if (popupUIGroup != null && popupUIGroup.CurrentUIForm != null && popupUIGroup.CurrentUIForm.GetType() != typeof(MergeMainMenuBase))
            {
                return;
            }

            //防止教程界面穿透
            if (MergeManager.Instance.CheckMergeBoardInteractive())
            {
                //试图修复教程点击物体没反应的bug
                MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                if (mainBoard != null && mainBoard.m_GuideMenu.CurGuide != null && mainBoard.m_GuideMenu.CurGuide.TargetProp != null) 
                {
                    Square hittedSquare = GetHitSquare();
                    if (hittedSquare != null && hittedSquare.FilledProp == mainBoard.m_GuideMenu.CurGuide.TargetProp)
                    {
                        SelectedProp = hittedSquare.FilledProp;
                    }
                }

                Log.Error("Merge Input down fail:CurGuideId is " + MergeGuideMenu.s_CurGuideId.ToString());
                return;
            }

            //清空历史记录 并且记录第一个值
            mousePositionHistoryList.Clear();
            mousePositionUp = Vector3.zero;
            mousePositionHistoryList.Add(Input.mousePosition);

            Square hitSquare = GetHitSquare();
            if (hitSquare != null && hitSquare.FilledProp != null && !hitSquare.FilledProp.IsPetrified && hitSquare.FilledProp.MovementState == PropMovementState.Static)
            {
                startInputPos = Input.mousePosition;
                SelectedProp = hitSquare.FilledProp;
                curGrabProp = hitSquare.FilledProp;

                ResetHintTimer();
            }

            isInputDown = true;
        }

        private void OnInput()
        {
            if (!isInputDown)
                return;

            if (!startDrag && curGrabProp != null && !curGrabProp.IsImmovable)
            {
                Vector3 curInputPos = Input.mousePosition;
                if (Vector3.Distance(curInputPos, startInputPos) > dragMoveLimit)
                {
                    startDrag = true;
                    curGrabProp.OnDragStart();

                    HidePropSelectedBox();
                }
            }

            //Hover检测
            if (startDrag)
            {
                //drag时 每次Update都记录一个值
                mousePositionHistoryList.Add(Input.mousePosition);

                Square hitSquare = GetBetterHitSquare(false);
                if (hitSquare != null)
                {
                    if (curHoverProp != hitSquare.FilledProp)
                    {
                        if (curHoverProp != null)
                        {
                            curHoverProp.OnHoverExit();
                            curHoverProp = null;
                        }

                        if (hitSquare.FilledProp != null && hitSquare.FilledProp != curGrabProp)
                        {
                            curHoverProp = hitSquare.FilledProp;
                            bool isSuccess = curHoverProp.OnHoverEnter(curGrabProp);

                            //真的显示了MergeEffect 的话
                            if (isSuccess)
                            {
                                tempHoverTargetPropLogic = curHoverProp;
                                tempHoverTargetRealTimeSinceStartUp = Time.realtimeSinceStartup;
                            }
                        }
                    }
                }
            }
        }

        public void OnInputUp()
        {
            if (startDrag && curGrabProp != null)
            {
                mousePositionUp = Input.mousePosition;

                Square endSquare = null;
                if (curGrabProp != null)
                {
                    Square hitSquare = GetBetterHitSquare(true);
                    if (hitSquare != null && hitSquare != curGrabProp.Square && !hitSquare.IsReserved && !hitSquare.IsLocked)
                    {
                        //教程二进行中时只能指定格子放置
                        if (MergeGuideMenu.s_CurGuideId == GuideTriggerType.Guide_DragMerge)
                        {
                            if (hitSquare.FilledProp != null && hitSquare.FilledProp.PropId == 10101)
                            {
                                endSquare = hitSquare.FilledProp.OnPutOn(curGrabProp);
                            }
                            else
                            {
                                endSquare = null;
                            }
                        }
                        else
                        {
                            endSquare = hitSquare;

                            if (hitSquare.FilledProp != null)
                            {
                                endSquare = hitSquare.FilledProp.OnPutOn(curGrabProp);
                            }
                        }
                    }
                }

                if (curGrabProp != null)
                {
                    curGrabProp.OnDragEnd(endSquare);
                }
            }

            if (curHoverProp != null)
            {
                curHoverProp.OnHoverExit();
                curHoverProp = null;
            }

            curGrabProp = null;
            startDrag = false;
            isInputDown = false;
        }

        private Square GetHitSquare()
        {
            return GetHitSquare(Input.mousePosition);
        }

        private Square GetHitSquare(Vector3 customMousePosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(EyeCamera.ScreenToWorldPoint(customMousePosition), Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"));
            if (hit.collider != null)
            {
                Square hitSquare = hit.collider.GetComponent<Square>();
                return hitSquare;
            }
            return null;
        }

        private Square GetBetterHitSquare(bool takeTempHoverIntoAccount)
        {
            Square formerHitSquare = GetHitSquare();

            //如果在上一步没有获得HitSquare 那么可能是移动到了棋盘的外部
            if (formerHitSquare == null)
            {
                formerHitSquare = GetHitSquareFromBoarderArea();
                return formerHitSquare;
            }

            //如果处于引导中 不要做什么BetterHitSquare了 只有精准格子会返回
            if (MergeGuideMenu.s_CurGuideId != GuideTriggerType.None)
                return formerHitSquare;

            //我们先用旧HitSquare检测一次 
            if (SquareIsGood(formerHitSquare))
            {
                return formerHitSquare;
            }

            //根据滑动路径最后一次记录的鼠标/触碰位置 和 触碰抬起时的位置
            //预言一个 (其实就是保持速度情况下的下一帧) 位置
            bool predictSquareIsGood = false;
            Square predictHitSquare = null;
            Vector3 predictMousPosition = CalculatePredictMousePosition();
            if (predictMousPosition != Vector3.zero)
            {
                predictHitSquare = GetHitSquare(predictMousPosition);
                if (predictHitSquare != null)
                    predictSquareIsGood = SquareIsGood(predictHitSquare);
            }

            bool lastFrameSquareIsGood = false;
            Square lastFrameSquare = null;
            if (mousePositionHistoryList.Count > 0)
            {
                lastFrameSquare = GetHitSquare(mousePositionHistoryList[mousePositionHistoryList.Count - 1]);
                if (lastFrameSquare != null)
                {
                    lastFrameSquareIsGood = SquareIsGood(lastFrameSquare);
                }
            }

            //优先级 formerHitSquare(IfGood) 模糊搜索的临近的Square(IfGood) PredictSquare(IfGood) lastFrameSquare(IfGood) tempHoverTargetPropLogic(如果存在且在lifeTime之内) formerhitSquare(无论是否good)

            Vector3 mouseHitWorldPosition = EyeCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 squareToHitV3 = mouseHitWorldPosition - formerHitSquare.transform.position;
            squareToHitV3.z = 0;

            //在方块的比较中间的部分 仍然返回formerHitSquare
            float deadZoneRatio = 0.7f;
            if ((Mathf.Abs(squareToHitV3.x) < squareSize / 2 * deadZoneRatio)
                && (Mathf.Abs(squareToHitV3.y) < squareSize / 2 * deadZoneRatio))
            {
                if (predictSquareIsGood)
                    return predictHitSquare;
                else if (lastFrameSquareIsGood)
                    return lastFrameSquare;
                else if (takeTempHoverIntoAccount && TempHoverTargetPropLogicIsLegal())
                    return tempHoverTargetPropLogic.Square;
                else
                    return formerHitSquare;
            }

            if (Mathf.Abs(squareToHitV3.x) > Mathf.Abs(squareToHitV3.y))
            {
                if (squareToHitV3.x < 0)
                {
                    //检查左Square
                    Square square = MergeManager.Merge.GetSquare(formerHitSquare.m_Row, formerHitSquare.m_Col - 1);
                    if (SquareIsGood(square))
                    {
                        //Log.Warning("Prefer Left Square");
                        return square;
                    }
                }
                else
                {
                    //检查右Square
                    Square square = MergeManager.Merge.GetSquare(formerHitSquare.m_Row, formerHitSquare.m_Col + 1);
                    if (SquareIsGood(square))
                    {
                        //Log.Warning("Prefer Right Square");
                        return square;
                    }
                }
            }
            else
            {
                if (squareToHitV3.y < 0)
                {
                    //检查下Square
                    Square square = MergeManager.Merge.GetSquare(formerHitSquare.m_Row + 1, formerHitSquare.m_Col);
                    if (SquareIsGood(square))
                    {
                        //Log.Warning("Prefer Down Square");
                        return square;
                    }
                }
                else
                {
                    //检查上Square
                    Square square = MergeManager.Merge.GetSquare(formerHitSquare.m_Row - 1, formerHitSquare.m_Col);
                    if (SquareIsGood(square))
                    {
                        //Log.Warning("Prefer Up Square");
                        return square;
                    }
                }
            }

            if (predictSquareIsGood)
                return predictHitSquare;
            else if (lastFrameSquareIsGood)
                return lastFrameSquare;
            else if (takeTempHoverIntoAccount && TempHoverTargetPropLogicIsLegal())
                return tempHoverTargetPropLogic.Square;
            else
                return formerHitSquare;
        }

        private bool TempHoverTargetPropLogicIsLegal()
        {
            if (tempHoverTargetPropLogic != null && tempHoverTargetPropLogic.Square)
            {
                if (Time.realtimeSinceStartup - tempHoverTargetRealTimeSinceStartUp <= tempHoverTargetLifeTime)
                {
                    return true;
                }
            }
            return false;
        }

        private Square GetHitSquareFromBoarderArea()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                return mainBoard.m_MergeBoard.GetClosestSquare(worldPosition);
            }

            return null;
        }

        //propID相同 或者 可以放入制作器时
        //就认为这个Square “挺好的” 不需要GetBetterHitSquare
        private bool SquareIsGood(Square hitSquare)
        {
            if (hitSquare == null)
                return false;

            if (curGrabProp == null)
                return true;

            if (hitSquare.FilledProp != null && hitSquare.FilledProp != curGrabProp)
            {
                if (hitSquare.FilledProp.PropId == curGrabProp.PropId && !hitSquare.FilledProp.IsPetrified && hitSquare.FilledProp.AttachmentId != 1 && curGrabProp.AttachmentId != 1) 
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 CalculatePredictMousePosition()
        {
            Vector3 predictV3 = Vector3.zero;
            if (mousePositionHistoryList.Count > 0 && mousePositionUp != Vector3.zero)
            {
                Vector3 fromV3 = mousePositionHistoryList[mousePositionHistoryList.Count - 1];
                Vector3 toV3 = mousePositionUp;
                predictV3 = toV3 - fromV3 + toV3;
            }
            return predictV3;
        }

        #endregion

        #region Square

        /// <summary>
        /// 获取随机空格子
        /// </summary>
        /// <returns>随机空格子</returns>
        /// <param name="removeReserved">剔除被预定的格子</param>
        public Square GetRandomEmptySquare(bool removeReserved = true)
        {
            foreach (Square square in squares)
            {
                if (square.FilledProp == null && (!removeReserved || !square.IsReserved))
                {
                    return square;
                }
            }

            return null;
        }

        /// <summary>
        /// 顺时针获取十字内的所有格子
        /// </summary>
        /// <param name="centerSquare">中心的格子</param>
        /// <returns>十字内的所有格子</returns>
        public List<Square> GetSquaresWithinCross(Square centerSquare)
        {
            List<Square> result = new List<Square>();

            int row = centerSquare.m_Row;
            int col = centerSquare.m_Col;

            int[] rowArray = new int[] { -1, 0, 1, 0 };
            int[] colArray = new int[] { 0, 1, 0, -1 };

            for (int i = 0; i < rowArray.Length; i++)
            {
                Square square = GetSquare(row + rowArray[i], col + colArray[i]);
                if (square != null)
                {
                    result.Add(square);
                }
            }

            return result;
        }

        /// <summary>
        /// 顺时针获取半径内的所有格子
        /// </summary>
        /// <param name="centerSquare">中心的格子</param>
        /// <param name="radius">半径</param>
        /// <param name="removeReserved">剔除被预定的格子</param>
        /// <returns>半径内的所有格子</returns>
        public List<Square> GetSquaresWithinRadius(Square centerSquare, int radius = 1, bool removeReserved = true)
        {
            List<Square> result = new List<Square>();

            int row = centerSquare.m_Row;
            int col = centerSquare.m_Col;

            int[] rowArray = new int[] { -1, -1, 0, 1, 1, 1, 0, -1 };
            int[] colArray = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };

            for (int i = 0; i < rowArray.Length; i++)
            {
                Square square = GetSquare(row + rowArray[i], col + colArray[i]);
                if (square != null && (!removeReserved || !square.IsReserved))
                {
                    result.Add(square);
                }
            }

            return result;
        }

        /// <summary>
        /// 顺时针获取起始格子半径内的空格子
        /// </summary>
        /// <param name="startSquare">起始格子</param>
        /// <param name="radius">半径</param>
        /// <param name="removeReserved">剔除被预定的格子</param>
        /// <returns>半径内的空格子</returns>
        public Square GetSurroundingEmptySquare(Square startSquare, int radius = 1, bool removeReserved = true)
        {
            if (startSquare == null)
            {
                return null;
            }

            List<Square> squares = GetSquaresWithinRadius(startSquare, radius, removeReserved);
            for (int i = 0; i < squares.Count; i++)
            {
                if (squares[i].FilledProp == null)
                {
                    return squares[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 获取离起始格子最近的空格子
        /// </summary>
        /// <param name="startSquare">起始格子</param>
        /// <param name="removeReserved">剔除被预定的格子</param>
        /// <returns>最近的空格子</returns>
        public Square GetNearestEmptySquare(Square startSquare, bool removeReserved = true)
        {
            if (startSquare == null)
            {
                return null;
            }

            Square result = null;
            Queue<Square> searchQueue = new Queue<Square>();
            searchQueue.Enqueue(startSquare);
            int[] rowArray = new int[] { -1, -1, 0, 1, 1, 1, 0, -1 };
            int[] colArray = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
            List<Square> searchedSquare = new List<Square>();
            searchedSquare.Add(startSquare);

            while (result == null && searchQueue.Count > 0)
            {
                Square square = searchQueue.Dequeue();
                if (!removeReserved || !square.IsReserved)
                {
                    if (square.FilledProp != null)
                    {
                        int row = square.m_Row;
                        int col = square.m_Col;
                        for (int i = 0; i < 8; i++)
                        {
                            Square nearSquare = GetSquare(row + rowArray[i], col + colArray[i]);
                            if (nearSquare != null && !searchedSquare.Contains(nearSquare))
                            {
                                searchQueue.Enqueue(nearSquare);
                                searchedSquare.Add(nearSquare);
                            }
                        }
                    }
                    else
                    {
                        result = square;
                    }
                }
            }

            if (startSquare == result)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// 获取格子
        /// </summary>
        /// <param name="row">格子的行数</param>
        /// <param name="col">格子的列数</param>
        /// <returns>格子</returns>
        public Square GetSquare(int row, int col)
        {
            if (row < 0 || col < 0 || row >= squares.GetLength(0) || col >= squares.GetLength(1))
            {
                return null;
            }

            return squares[row, col];
        }

        #endregion

        #region Prop

        /// <summary>
        /// 获取道具等级
        /// </summary>
        /// <param name="propId">道具编号</param>
        /// <returns>道具等级</returns>
        public int GetPropRank(int propId)
        {
            DRPropMerge mergeData = GetMergeRouteData(propId);
            return mergeData != null ? mergeData.MergeRoute.IndexOf(propId) + 1 : 1;
        }
        /// <summary>
        /// 获取是否是最大道具等级
        /// </summary>
        /// <param name="propId">道具编号</param>
        public bool GetPropIsMaxRank(int propId)
        {
            DRPropMerge mergeData = GetMergeRouteData(propId);
            return mergeData != null ? mergeData.MergeRoute.IndexOf(propId) + 1 == mergeData.MergeRoute.Count : true;
        }

        /// <summary>
        /// 获取道具合成路线编号
        /// </summary>
        /// <param name="propId">道具编号</param>
        /// <returns>道具合成路线编号</returns>
        public int GetMergeRouteId(int propId)
        {
            DRPropMerge mergeData = GetMergeRouteData(propId);
            return mergeData != null ? mergeData.Id : 0;
        }

        /// <summary>
        /// 获取道具合成路线
        /// </summary>
        /// <param name="propId">道具编号</param>
        /// <returns>道具合成路线</returns>
        public List<int> GetMergeRouteList(int propId)
        {
            DRPropMerge mergeData = GetMergeRouteData(propId);
            return mergeData != null ? mergeData.MergeRoute : null;
        }

        /// <summary>
        /// 获取道具合成路线数据
        /// </summary>
        /// <param name="propId">道具编号</param>
        /// <returns>道具合成数据</returns>
        public DRPropMerge GetMergeRouteData(int propId)
        {
            IDataTable<DRPropMerge> mergeRouteDataTable = MergeManager.DataTable.GetDataTable<DRPropMerge>(MergeManager.Instance.GetMergeDataTableName());
            DRPropMerge[] mergeRouteRows = mergeRouteDataTable.GetAllDataRows();
            for (int i = 0; i < mergeRouteRows.Length; i++)
            {
                if (mergeRouteRows[i].MergeRoute.Contains(propId))
                {
                    return mergeRouteRows[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 获取道具字典
        /// </summary>
        /// <param name="maxSamePropCount">最大相同道具的数量</param>
        public Dictionary<int, List<PropLogic>> GetPropDicOnChessboard(out int maxSamePropCount)
        {
            maxSamePropCount = 1;

            Dictionary<int, List<PropLogic>> propDic = new Dictionary<int, List<PropLogic>>();
            if (allProps != null)
            {
                foreach (PropLogic prop in allProps)
                {
                    if (!prop.IsPetrified && !prop.IsSilenced && prop.MovementState == PropMovementState.Static && prop.Square != null)
                    {
                        if (propDic.TryGetValue(prop.PropId, out List<PropLogic> list))
                        {
                            list.Add(prop);

                            if (maxSamePropCount < list.Count)
                                maxSamePropCount = list.Count;
                        }
                        else
                        {
                            propDic.Add(prop.PropId, new List<PropLogic>() { prop });
                        }
                    }
                }
            }
            return propDic;
        }

        /// <summary>
        /// 获取棋盘上指定道具的数量
        /// </summary>
        /// <param name="propId">指定道具的编号</param>
        /// <param name="isAvailable">道具是否可用</param>
        /// <returns>指定道具的数量</returns>
        public int GetPropNumOnChessboard(int propId, bool isAvailable = true)
        {
            int result = 0;
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                for (int j = 0; j < squares.GetLength(1); j++)
                {
                    PropLogic prop = squares[i, j].FilledProp;
                    if (prop != null && prop.PropId == propId)
                    {
                        if (!isAvailable || (!prop.IsPetrified && !prop.IsImmovable && !prop.IsSilenced))
                            result++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取棋盘上指定道具
        /// </summary>
        /// <param name="propId">指定道具的编号</param>
        /// <param name="isAvailable">道具是否可用</param>
        /// <returns>指定道具</returns>
        public PropLogic GetPropOnChessboard(int propId, bool isAvailable = true)
        {
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                for (int j = 0; j < squares.GetLength(1); j++)
                {
                    PropLogic prop = squares[i, j].FilledProp;
                    if (prop != null && prop.PropId == propId)
                    {
                        if (!isAvailable || (!prop.IsPetrified && !prop.IsImmovable && !prop.IsSilenced))
                            return prop;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取棋盘上指定道具集合
        /// </summary>
        /// <param name="propId">指定道具的编号</param>
        /// <param name="isAvailable">道具是否可用</param>
        /// <returns>指定道具的集合</returns>
        public List<PropLogic> GetPropListOnChessboard(int propId, bool isAvailable = true)
        {
            List<PropLogic> result = new List<PropLogic>();
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                for (int j = 0; j < squares.GetLength(1); j++)
                {
                    PropLogic prop = squares[i, j].FilledProp;
                    if (prop != null && prop.PropId == propId)
                    {
                        if (!isAvailable || (!prop.IsPetrified && !prop.IsImmovable && !prop.IsSilenced))
                            result.Add(prop);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取棋盘上指定道具集合
        /// </summary>
        /// <param name="propsId">指定道具的编号集合</param>
        /// <param name="isAvailable">道具是否可用</param>
        /// <returns>指定道具的集合</returns>
        public List<PropLogic> GetPropListOnChessboard(List<int> propsId, bool isAvailable = true)
        {
            List<PropLogic> result = new List<PropLogic>();
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                for (int j = 0; j < squares.GetLength(1); j++)
                {
                    PropLogic prop = squares[i, j].FilledProp;
                    if (prop != null && propsId.Contains(prop.PropId))
                    {
                        if (!isAvailable || (!prop.IsPetrified && !prop.IsImmovable && !prop.IsSilenced))
                            result.Add(prop);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取棋盘上所有道具的集合
        /// </summary>
        /// <param name="isAvailable">道具是否可用</param>
        /// <returns>所有道具的集合</returns>
        public List<PropLogic> GetAllPropsOnChessboard(bool isAvailable = true)
        {
            List<PropLogic> result = new List<PropLogic>();
            for (int i = 0; i < squares.GetLength(0); i++)
            {
                for (int j = 0; j < squares.GetLength(1); j++)
                {
                    PropLogic prop = squares[i, j].FilledProp;
                    if (prop != null)
                    {
                        if (!isAvailable || (!prop.IsPetrified && !prop.IsImmovable && !prop.IsSilenced))
                            result.Add(prop);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取可以合成的道具集合
        /// </summary>
        public LinkedList<List<PropLogic>> GetCanMergeProps()
        {
            LinkedList<List<PropLogic>> result = new LinkedList<List<PropLogic>>();

            int maxSamePropCount;
            Dictionary<int, List<PropLogic>> propDic = MergeManager.Merge.GetPropDicOnChessboard(out maxSamePropCount);

            if (maxSamePropCount >= 2)
            {
                foreach (KeyValuePair<int, List<PropLogic>> pair in propDic)
                {
                    if (pair.Value.Count >= 2 && !MergeManager.Merge.GetPropIsMaxRank(pair.Key))
                    {
                        bool isMovable = false;
                        foreach (PropLogic propLogic in pair.Value)
                        {
                            if (!propLogic.IsImmovable)
                                isMovable = true;
                        }

                        if (isMovable)
                        {
                            result.AddLast(pair.Value);
                        }
                    }
                }

                if (result.Count > 0)
                    return result;
            }

            return result;
        }

        /// <summary>
        /// 获取可以点击的特殊道具集合
        /// </summary>
        public LinkedList<List<PropLogic>> GetCanClickSpecialProps()
        {
            LinkedList<List<PropLogic>> result = new LinkedList<List<PropLogic>>();

            int maxSamePropCount;
            Dictionary<int, List<PropLogic>> propDic = MergeManager.Merge.GetPropDicOnChessboard(out maxSamePropCount);

            foreach (KeyValuePair<int, List<PropLogic>> pair in propDic)
            {
                if (pair.Value.Count > 0 && MergeManager.Instance.CheckIsCanClickSpecialProp(pair.Key)) 
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        if (!pair.Value[i].IsImmovable)
                            result.AddLast(new List<PropLogic>() { pair.Value[i] });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 显示道具提示
        /// </summary>
        public void ShowPropHint()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
                mainBoard.ShowPropHint();
        }

        /// <summary>
        /// 重置道具提示时间
        /// </summary>
        public void ResetHintTimer()
        {
            showMergeHintTimer = 5f;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
                mainBoard.ClearPropHint();
        }

        /// <summary>
        /// 合成道具
        /// </summary>
        /// <param name="prop1">进行合成的道具</param>
        /// <param name="prop2">另一个进行合成的道具</param>
        /// <param name="fillSquare">合成完填充的格子</param>
        /// <returns>是否合成成功</returns>
        public bool MergeProp(PropLogic prop1, PropLogic prop2, Square fillSquare)
        {
            if (prop1.PropId == prop2.PropId)
            {
                IDataTable<DRPropMerge> mergeRouteDataTable = MergeManager.DataTable.GetDataTable<DRPropMerge>(MergeManager.Instance.GetMergeDataTableName());
                DRPropMerge mergeRouteRow = mergeRouteDataTable.GetDataRow(prop1.MergeRouteId);
                if (mergeRouteRow != null)
                {
                    int index = mergeRouteRow.MergeRoute.FindIndex(i => i.Equals(prop1.PropId));
                    if (index < 0 || index >= mergeRouteRow.MergeRoute.Count - 1)
                    {
                        return false;
                    }

                    int nextIndex = index + 1;
                    int nextPropId = mergeRouteRow.MergeRoute[nextIndex];
                    Square square = fillSquare;
                    int attachmentId = prop2.AttachmentId;

                    SelectedProp = null;
                    ReleaseProp(prop1);
                    ReleaseProp(prop2);

                    PropLogic newProp = GenerateProp(nextPropId, 0, square.transform.position, square, PropMovementState.Static);
                    newProp.SpawnPropComplete += p =>
                    {
                        p.gameObject.SetActive(false);
                        p.OnGeneratedByMerge();
                    };
                    SelectedProp = newProp;
                    ShowPropSelectedBox(square.transform.position);

                    List<Square> nearSquares = GetSquaresWithinCross(square);
                    foreach (Square nearSquare in nearSquares)
                    {
                        if (nearSquare.FilledProp != null)
                        {
                            nearSquare.FilledProp.OnOperationOccurAround(PropOperation.Merge);
                        }
                    }

                    SavePropDistributedMap();

                    MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                    if (mainBoard != null)
                    {
                        mainBoard.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_DragMerge);
                        mainBoard.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_Web);
                    }

                    int soundIndex = Mathf.Clamp(nextIndex, 1, 11);
                    GameManager.Sound.PlayAudio("merge_" + soundIndex.ToString());
                    UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                    
                    return true;
                }
                else
                {
                    Log.Error("TryMergeProp Fail - dont have {0} merge route", prop1.MergeRouteId.ToString());
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 替换道具
        /// </summary>
        /// <param name="propId">替换成的道具编号</param>
        /// <param name="replacedProp">被替换的道具</param>
        /// <param name="fillSquare">占据的格子</param>
        public void ReplaceProp(int propId, PropLogic replacedProp, Square fillSquare, Action<Prop> spawnPropComplete = null)
        {
            bool isSelected = replacedProp == selectedProp;
            ReleaseProp(replacedProp);
            PropLogic newProp = GenerateProp(propId, 0, fillSquare.transform.position, fillSquare, PropMovementState.Static);
            newProp.SpawnPropComplete += p =>
            {
                p.gameObject.SetActive(false);
                p.ShowPunchAnim();

                spawnPropComplete?.Invoke(p);
            };

            if (isSelected)
            {
                SelectedProp = newProp;
                SelectedProp.OnSelected();
            }
        }

        /// <summary>
        /// 生成道具
        /// </summary>
        /// <param name="propId">道具编号</param>
        /// <param name="attachmentId">附属物编号</param>
        /// <param name="position">生成位置</param>
        /// <param name="square">所占据格子</param>
        /// <param name="movementState">生成时的移动状态</param>
        /// <param name="savedData">道具存储的数据</param>
        /// <returns>道具逻辑类</returns>
        public PropLogic GenerateProp(int propId, int attachmentId, Vector3 position, Square square, PropMovementState movementState, PropSavedData savedData = null)
        {
            PropLogic propLogic = InternalGenerateProp(propId, attachmentId, position, square, movementState, savedData);
            if (propLogic != null)
            {
                SavePropDistributedMap();
            }
            else
            {
                Log.Error("GenerateProp fail - propLogic Initialize error");
            }

            return propLogic;
        }

        public PropLogic InternalGenerateProp(int propId, int attachmentId, Vector3 position, Square square, PropMovementState movementState, PropSavedData savedData)
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            PropLogic propLogic = PropLogic.Create(propId, attachmentId);
            bool isSuccess = propLogic.Initialize(position, square, movementState, savedData, mainBoard.m_MergeBoard.m_PropsRoot);
            if (!isSuccess)
            {
                ReferencePool.Release(propLogic);
                return null;
            }

            if (allProps == null)
                allProps = new HashSet<PropLogic>();
            allProps.Add(propLogic);

            return propLogic;
        }

        /// <summary>
        /// 重置道具
        /// </summary>
        /// <param name="prop">要重置的道具</param>
        /// <param name="sendPropEvent">是否发送道具改变事件</param>
        public void ResetProp(PropLogic prop, bool sendPropEvent = true)
        {
            if (prop == null)
            {
                Log.Error("ResetProp Fail - PropLogic is null");
                return;
            }

            if (selectedProp == prop)
            {
                selectedProp = null;
                HidePropSelectedBox();
                GameManager.Event.Fire(this, SelectedPropChangeEventArgs.Create());
            }

            if (curGrabProp == prop)
            {
                if (curGrabProp.MovementState == PropMovementState.Draging)
                {
                    curGrabProp.OnDragEnd(null);
                }
                curGrabProp = null;
            }

            if (curHoverProp == prop)
            {
                curHoverProp = null;
            }

            prop.Reset();
        }

        /// <summary>
        /// 释放道具
        /// </summary>
        /// <param name="prop">要释放的道具</param>
        /// <param name="sendPropEvent">是否发送道具改变事件</param>
        public void ReleaseProp(PropLogic prop, bool sendPropEvent = true)
        {
            if (prop == null)
            {
                Log.Error("ReleaseProp Fail - PropLogic is null");
                return;
            }

            if (allProps != null)
            {
                allProps.Remove(prop);
            }

            ResetProp(prop, sendPropEvent);
            prop.Release(false);

            SavePropDistributedMap();
        }
        
        /// <summary>
        /// 清空道具所有引用，但不释放本体
        /// </summary>
        public void ClearProp(PropLogic prop)
        {
            if (prop == null)
            {
                Log.Error("ClearProp Fail - PropLogic is null");
                return;
            }

            if (allProps != null)
            {
                allProps.Remove(prop);
            }

            ResetProp(prop, false);
            prop.Reset();
        }

        #endregion

        #region Data

        /// <summary>
        /// 保存棋盘布局
        /// </summary>
        /// <param name="saveNow">立刻保存还是延迟一帧保存</param>
        public void SavePropDistributedMap(bool saveNow = false)
        {
            if (saveNow)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < squares.GetLength(0); i++)
                {
                    for (int j = 0; j < squares.GetLength(1); j++)
                    {
                        if (squares[i, j].FilledProp != null)
                        {
                            sb.Append(i.ToString());
                            sb.Append("#");
                            sb.Append(j.ToString());
                            sb.Append("#");
                            sb.Append(squares[i, j].FilledProp.PropId.ToString());
                            sb.Append("#");
                            if (squares[i, j].FilledProp.AttachmentId != 0)
                            {
                                sb.Append(squares[i, j].FilledProp.AttachmentId.ToString());
                            }
                            else
                            {
                                sb.Append("0");
                            }
                            sb.Append("#");
                            PropSavedData savedData = squares[i, j].FilledProp.Save();
                            string savedString = savedData.Save();
                            if (savedString.Contains("#") || savedString.Contains("$"))
                            {
                                Log.Error("SavePropDistributedMap Fail - saved string {0} is invalid", savedString);
                            }
                            else
                            {
                                sb.Append(savedString);
                            }
                            ReferencePool.Release(savedData);
                            sb.Append("$");
                        }
                    }
                }
                sb.Remove(sb.Length - 1, 1);

                MergeManager.PlayerData.SetSavedPropDistributedMap(sb.ToString());
                deferredSaveAction = null;
                Log.Info("Save Map Success");
            }
            else
            {
                deferredSaveAction = s => SavePropDistributedMap(s);
            }
        }

        #endregion

        #region UI

        /// <summary>
        /// 显示道具选中框
        /// </summary>
        /// <param name="worldPos">目标位置</param>
        public void ShowPropSelectedBox(Vector3 worldPos)
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            mainBoard.m_SelectedBox.transform.position = worldPos;
            mainBoard.m_SelectedBox.Show();
        }

        /// <summary>
        /// 隐藏道具选中框
        /// </summary>
        public void HidePropSelectedBox()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
                mainBoard.m_SelectedBox.Hide();
        }

        #endregion

        #region Gizmo or Even GL Debug
        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    for (int i = 0; i < mousePositionHistoryList.Count; ++i)
        //    {
        //        DrawCross(Camera.main.ScreenToWorldPoint(mousePositionHistoryList[i]));
        //    }

        //    Gizmos.color = Color.yellow;
        //    if (mousePositionUp != Vector3.zero)
        //    {
        //        DrawCross(Camera.main.ScreenToWorldPoint(mousePositionUp), 0.015f);

        //        Gizmos.color = Color.green;
        //        Vector3 predictV3 = CalculatePredictMousePosition();
        //        if (predictV3 != Vector3.zero)
        //            DrawCross(Camera.main.ScreenToWorldPoint(predictV3), 0.015f);

        //        if (mousePositionHistoryList.Count > 0)
        //            DrawCross(Camera.main.ScreenToWorldPoint(mousePositionHistoryList[mousePositionHistoryList.Count - 1]), 0.015f);
        //    }

        //    if (tempHoverTargetPropLogic != null && tempHoverTargetPropLogic.Prop != null)
        //    {
        //        if (Time.realtimeSinceStartup - tempHoverTargetRealTimeSinceStartUp > tempHoverTargetLifeTime)
        //            Gizmos.color = Color.yellow;
        //        else
        //            Gizmos.color = Color.green;
        //        Gizmos.DrawSphere(tempHoverTargetPropLogic.Prop.transform.position, 0.02f);
        //    }
        //}

        //private void DrawCross(Vector3 targetPoint, float length = 0.02f)
        //{
        //    targetPoint = new Vector3(targetPoint.x, targetPoint.y, 10);
        //    Gizmos.DrawLine(targetPoint + new Vector3(length, length, 0), targetPoint + new Vector3(-length, -length, 0));
        //    Gizmos.DrawLine(targetPoint + new Vector3(length, -length, 0), targetPoint + new Vector3(-length, length, 0));
        //}
        #endregion
    }
}
