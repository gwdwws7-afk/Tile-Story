using System;
using System.Collections.Generic;

namespace GameFramework.Fsm
{
    /// <summary>
    /// 有限状态机
    /// </summary>
    /// <typeparam name="T">状态机持有者类型</typeparam>
    public sealed class Fsm<T> : FsmBase, IReference, IFsm<T> where T : class
    {
        private T owner;
        private readonly Dictionary<Type, FsmState<T>> states;
        private Dictionary<string, object> datas;
        private FsmState<T> currentState;
        private float currentStateTime;
        private bool isDestroyed;

        public Fsm()
        {
            owner = null;
            states = new Dictionary<Type, FsmState<T>>();
            datas = null;
            currentState = null;
            currentStateTime = 0f;
            isDestroyed = true;
        }

        /// <summary>
        /// 获取有限状态机的持有者
        /// </summary>
        public T Owner
        {
            get
            {
                return owner;
            }
        }

        /// <summary>
        /// 获取状态机持有者类型
        /// </summary>
        public override Type OwnerType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// 获取有限状态机中状态的数量
        /// </summary>
        public override int FsmStateCount
        {
            get
            {
                return states.Count;
            }
        }

        /// <summary>
        /// 获取有限状态机是否正在运行
        /// </summary>
        public override bool IsRunning
        {
            get
            {
                return currentState != null;
            }
        }

        /// <summary>
        /// 获取有限状态机是否被销毁
        /// </summary>
        public override bool IsDestroyed
        {
            get
            {
                return isDestroyed;
            }
        }

        /// <summary>
        /// 获取当前有限状态机状态
        /// </summary>
        public FsmState<T> CurrentState
        {
            get
            {
                return currentState;
            }
        }

        /// <summary>
        /// 获取当前有限状态机状态名称
        /// </summary>
        public override string CurrentStateName
        {
            get
            {
                return currentState != null ? currentState.GetType().FullName : null;
            }
        }

        /// <summary>
        /// 获取当前有限状态机状态持续时间
        /// </summary>
        public override float CurrentStateTime
        {
            get
            {
                return currentStateTime;
            }
        }

        /// <summary>
        /// 创建有限状态机
        /// </summary>
        /// <param name="name">有限状态机名称</param>
        /// <param name="owner">有限状态机持有者</param>
        /// <param name="states">有限状态机状态集合</param>
        /// <returns>创建的有限状态机</returns>
        public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
        {
            if (owner == null)
            {
                throw new Exception("FSM owner is invalid.");
            }

            if (states == null || states.Length < 1)
            {
                throw new Exception("FSM states is invalid.");
            }

            Fsm<T> fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm.Name = name;
            fsm.owner = owner;
            fsm.isDestroyed = false;

            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] == null)
                {
                    throw new Exception("FSM states is invalid.");
                }

                Type stateType = states[i].GetType();
                if (fsm.states.ContainsKey(stateType))
                {
                    throw new Exception(string.Format("FSM '{0}' state '{1}' is already exist.", new TypeNamePair(typeof(T), name), stateType.FullName));
                }

                fsm.states.Add(stateType, states[i]);
                states[i].OnInit(fsm);
            }

            return fsm;
        }

        /// <summary>
        /// 创建有限状态机
        /// </summary>
        /// <param name="name">有限状态机名称</param>
        /// <param name="owner">有限状态机持有者</param>
        /// <param name="states">有限状态机状态集合</param>
        /// <returns>创建的有限状态机</returns>
        public static Fsm<T> Create(string name, T owner, List<FsmState<T>> states)
        {
            if (owner == null)
            {
                throw new Exception("FSM owner is invalid.");
            }

            if (states == null || states.Count < 1)
            {
                throw new Exception("FSM states is invalid.");
            }

            Fsm<T> fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm.Name = name;
            fsm.owner = owner;
            fsm.isDestroyed = false;
            foreach (FsmState<T> state in states)
            {
                if (state == null)
                {
                    throw new Exception("FSM states is invalid.");
                }

                Type stateType = state.GetType();
                if (fsm.states.ContainsKey(stateType))
                {
                    throw new Exception(string.Format("FSM '{0}' state '{1}' is already exist.", new TypeNamePair(typeof(T), name), stateType.FullName));
                }

                fsm.states.Add(stateType, state);
                state.OnInit(fsm);
            }

            return fsm;
        }

        /// <summary>
        /// 清理有限状态机
        /// </summary>
        public void Clear()
        {
            if (currentState != null)
            {
                currentState.OnLeave(this, true);
            }

            foreach (KeyValuePair<Type, FsmState<T>> state in states)
            {
                state.Value.OnDestroy(this);
            }

            Name = null;
            owner = null;
            states.Clear();

            if (datas != null)
            {
                datas.Clear();
            }

            currentState = null;
            currentStateTime = 0f;
            isDestroyed = true;
        }

        /// <summary>
        /// 开始有限状态机
        /// </summary>
        /// <typeparam name="TState">要开始的有限状态机状态类型</typeparam>
        public void Start<TState>() where TState : FsmState<T>
        {
            if (IsRunning)
            {
                throw new Exception("FSM is running, can not start again.");
            }

            FsmState<T> state = GetState<TState>();
            if (state == null)
            {
                throw new Exception(string.Format("FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), typeof(TState).FullName));
            }

            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }

        /// <summary>
        /// 开始有限状态机
        /// </summary>
        /// <param name="stateType">要开始的有限状态机状态类型</param>
        public void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new Exception("FSM is running, can not start again.");
            }

            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception(string.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new Exception(string.Format("FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), stateType.FullName));
            }

            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }

        /// <summary>
        /// 是否存在有限状态机状态
        /// </summary>
        /// <typeparam name="TState">要检查的有限状态机状态类型</typeparam>
        /// <returns>是否存在有限状态机状态</returns>
        public bool HasState<TState>() where TState : FsmState<T>
        {
            return states.ContainsKey(typeof(TState));
        }

        /// <summary>
        /// 是否存在有限状态机状态
        /// </summary>
        /// <param name="stateType">要检查的有限状态机状态类型</param>
        /// <returns>是否存在有限状态机状态</returns>
        public bool HasState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception(string.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            return states.ContainsKey(stateType);
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
        /// <returns>要获取的有限状态机状态。</returns>
        public TState GetState<TState>() where TState : FsmState<T>
        {
            if (states.TryGetValue(typeof(TState), out FsmState<T> state))
            {
                return (TState)state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机状态。
        /// </summary>
        /// <param name="stateType">要获取的有限状态机状态类型。</param>
        /// <returns>要获取的有限状态机状态。</returns>
        public FsmState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception(string.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            if (states.TryGetValue(stateType, out FsmState<T> state))
            {
                return state;
            }

            return null;
        }

        /// <summary>
        /// 获取有限状态机的所有状态。
        /// </summary>
        /// <returns>有限状态机的所有状态。</returns>
        public FsmState<T>[] GetAllStates()
        {
            int index = 0;
            FsmState<T>[] results = new FsmState<T>[states.Count];
            foreach (KeyValuePair<Type, FsmState<T>> state in states)
            {
                results[index++] = state.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取有限状态机的所有状态。
        /// </summary>
        /// <param name="results">有限状态机的所有状态。</param>
        public void GetAllStates(List<FsmState<T>> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<Type, FsmState<T>> state in states)
            {
                results.Add(state.Value);
            }
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        public void ChangeState<TState>() where TState : FsmState<T>
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        public void ChangeState(Type stateType)
        {
            if (currentState == null)
            {
                throw new Exception("Current state is invalid.");
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new Exception(string.Format("FSM '{0}' can not change state to '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), stateType.FullName));
            }

            currentState.OnLeave(this, false);
            currentStateTime = 0f;
            currentState = state;
            currentState.OnEnter(this);
        }

        /// <summary>
        /// 状态机轮询
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (CurrentState == null)
                return;
            currentStateTime += elapseSeconds;
            currentState.OnUpate(this, elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理状态机
        /// </summary>
        public override void Shutdown()
        {
            ReferencePool.Release(this);
        }

        /// <summary>
        /// 是否存在有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>有限状态机数据是否存在。</returns>
        public bool HasData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (datas == null)
            {
                return false;
            }

            return datas.ContainsKey(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public TDate GetData<TDate>(string name)
        {
            return (TDate)GetData(name);
        }

        /// <summary>
        /// 获取有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public object GetData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (datas.TryGetValue(name, out object data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// 设置状态机数据
        /// </summary>
        public void SetData(string name, object data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (datas == null)
            {
                datas = new Dictionary<string, object>();
            }

            datas[name] = data;
        }

        /// <summary>
        /// 移除状态机数据
        /// </summary>
        public bool RemoveData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Data name is invalid.");
            }

            if (datas == null)
            {
                return false;
            }

            return datas.Remove(name);
        }
    }
}