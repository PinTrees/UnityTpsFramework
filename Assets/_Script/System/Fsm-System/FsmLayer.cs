using System.Collections.Generic;
using Fsm.State;
using System.Linq;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Fsm
{
    /// <summary>
    /// FSM(Finite State Machine) 오브젝트를 관리하는 베이스 클래스입니다.
    /// </summary>
    public class FsmLayer
    {
        public FsmObjectBase fsmObject;
        public FsmContext context;
        public FsmState currentState;       // 현재 활성화된 FSM 상태

        private readonly Dictionary<string, FsmState> stateMap = new();     // 가능한 모든 상태들을 저장하는 맵   
        private readonly Queue<FsmState> changeStateQueue = new();          // 상태 변경을 위한 대기열

        public bool isAnimatorInitialized { get; set; }             // 애니메이터 초기화 여부 
        public string previousStateType { get; private set; }        // 이전에 활성화되었던 상태

        // Systen value
        private bool isChangeStateLocked = false;       // 상태 변경 상태 잠금

        // state value
        public object param;

        public bool isFsmInitialized { get; private set; }


        /// <summary>
        /// FSM을 초기화하는 메소드입니다.
        /// </summary>
        public void Init()
        {
            if (isFsmInitialized) return;
            isFsmInitialized = false;

            OnInit();

            isFsmInitialized = true;
        }

        // 재정의 가능 초기화자
        protected virtual void OnInit()
        {
            UpdateStateChangeTask();
        }

        public void Update()
        {
            if (isChangeStateLocked)
                return;

            if (isFsmInitialized)
            {
                currentState?.Update();
            }
        }

        public void FixedUpdate()
        {
            if (isChangeStateLocked)
                return;

            if (isFsmInitialized)
            {
                currentState?.FixedUpdate();
            }
        }

        public void LateUpdate()
        {
            if (isChangeStateLocked)
                return;

            if (isFsmInitialized)
            {
                currentState?.LateUpdate();
            }
        }

        public FsmState FindState(string type)
        {
            if (stateMap.ContainsKey(type))
            {
                return stateMap[type];
            }
            else return null;
        }



        /// <summary>
        /// 상태를 FSM에 추가합니다.
        /// </summary>
        /// <param name="state">추가할 상태</param>
        public void AddState(FsmState state)
        {
            stateMap[state.stateId] = state;
            state.layer = this;
            state.owner = fsmObject;
        }

        /// <summary>
        /// 다수의 상태를 FSM에 추가합니다.
        /// </summary>
        /// <param name="states">추가할 상태들의 목록</param>
        public void AddStateRange(List<FsmState> states)
        {
            foreach (var state in states)
            {
                AddState(state);
            }
        }


        #region Contains, Equal
        /// <summary>
        /// 특정 상태를 포함하고 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="type">확인하고자 하는 상태</param>
        /// <returns>상태 포함 여부</returns>
        public bool ContainsState(string type) => currentState != null && type.Equals(currentState.stateId);

        /// <summary>
        /// 주어진 상태 목록 중 하나라도 현재 상태와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="list">확인할 상태 목록</param>
        /// <returns>일치하는 상태 존재 여부</returns>
        public bool ContainsStates(List<TEnum> list)
        {
            return list.Any(state => state.Equals(currentState.stateId));
        }
        #endregion



        #region Change State
        /// <summary>
        /// 지정된 상태로 즉시 변경하며 일부 변수를 다음 스테이트로 넘겨줍니다. 
        /// 변경 대기열을 초기화합니다.
        /// </summary>
        /// <param name="type">변경할 상태</param>
        public void ChangeStateNow(string type, object param = null)
        {
            ChangeStateNowAsync(type, param).Forget();
        }


        struct StateChangeContainer
        {
            public string type;
            public object param;
            public Action onCompelet;
        }

        private async void UpdateStateChangeTask()
        {
            while(true)
            {
                await UniTask.Yield();

                if (stateChangeQueue.Count <= 0)
                    continue;

                var currentChangeState = stateChangeQueue.Dequeue();
                this.param = currentChangeState.param;

                try
                {
                    // 현재 상태가 있는 경우 Exit
                    if (currentState != null)
                    {
                        await currentState.Exit();
                        previousStateType = currentState.stateId;
                    }

                    // 새로운 상태로 전환
                    if (stateMap.ContainsKey(currentChangeState.type))
                    {
                        currentState = stateMap[currentChangeState.type];
                        await currentState.Enter();
                    }

                    currentChangeState.onCompelet();
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private Queue<StateChangeContainer> stateChangeQueue = new();
        public async UniTask ChangeStateNowAsync(string type, object param = null)
        {
            if (stateChangeQueue.Where(e => e.type == type).Count() > 0)
                return;

            bool isCompelet = false;
            StateChangeContainer newChange = new StateChangeContainer()
            {
                type = type,
                param = param,
                onCompelet = () =>
                {
                    isCompelet = true;
                }
            };
            stateChangeQueue.Enqueue(newChange);

            while (true)
            {
                if (isCompelet) return;

                await UniTask.Yield();
            }
        }
        #endregion
    }
}
