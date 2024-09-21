using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using Fsm.State;
using Cysharp.Threading.Tasks;

namespace Fsm
{
    /// <summary>
    /// FSM(유한 상태 기계)의 컨텍스트를 관리하는 클래스입니다.
    /// 상태를 추가, 변경, 업데이트하는 기능을 제공합니다.
    /// </summary> 
    public class FsmContext
    {
        public FsmObjectBase fsmObject;
        [SerializeField] public Dictionary<string, FsmLayer> layers = new();      // FSM 컨텍스트를 소유하는 오브젝트


        #region Init
        /// <summary>
        /// FSM을 초기화합니다.
        /// </summary>
        /// <param name="owner">FSM을 소유하는 객체</param>
        public void Initialize(FsmObjectBase fsmObject)
        {
            this.fsmObject = fsmObject;

            foreach(var layer in layers)
            {
                layer.Value.Init();
            }
        }
        #endregion

        #region Add Layer
        public void AddLayer(FsmLayer layer, string layerName)
        {
            layers[layerName] = layer;
            layers[layerName].context = this;
            layers[layerName].fsmObject = fsmObject;
            layers[layerName].Init();
        }
        public FsmLayer CreateLayer(string layerName)
        {
            layers[layerName] = new FsmLayer();
            layers[layerName].context = this;
            layers[layerName].fsmObject = fsmObject;
            layers[layerName].Init();

            return layers[layerName];
        }
        public FsmLayer FindLayer(string layerName)
        {
            if (!layers.ContainsKey(layerName))
                return null;
            return layers[layerName];   
        }

        /// <summary>
        /// 상태를 FSM에 추가합니다.
        /// </summary>
        /// <param name="state">추가할 상태</param>
        public void AddState(FsmState state)
        {
            layers.First().Value.AddState(state);
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
        #endregion


        #region Update
        /// <summary>
        /// 매 프레임마다 호출되어 현재 상태의 Update 메서드를 실행합니다.
        /// </summary>
        public void Update()
        {
            foreach (var layer in layers)
            {
                layer.Value.Update();
            }
        }

        /// <summary>
        /// 매 물리 업데이트마다 호출됩니다.
        /// </summary>
        public void FixedUpdate()
        {
            foreach (var layer in layers)
            {
                layer.Value.FixedUpdate();
            }
        }

        /// <summary>
        /// 모든 업데이트 이후에 호출됩니다.
        /// </summary>
        public void LateUpdate() 
        {
            foreach (var layer in layers)
            {
                layer.Value.LateUpdate();
            }
        }
        #endregion



        #region Contains, Equal
        /// <summary>
        /// 특정 상태를 포함하고 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="type">확인하고자 하는 상태</param>
        /// <returns>상태 포함 여부</returns>
        public bool ContainsState(string layer, TEnum type)
        {
            if (!layers.ContainsKey(layer))
                return false;
            if (layers[layer].currentState == null)
                return false;
            return layers[layer].currentState.stateId.Equals(type);
        }
        #endregion


        #region Change State
        /// <summary>
        /// 지정된 상태로 즉시 변경합니다. 변경 대기열을 초기화합니다.
        /// </summary>
        /// <param name="type">변경할 상태</param>
        public void ChangeStateNow(string layer, string type, object param = null)
        {
            if (!layers.ContainsKey(layer))
                return;

            ChangeStateNowAsync(layer, type, param).Forget();
        }

        public async UniTask ChangeStateNowAsync(string layer, string type, object param=null)
        {
            if (!layers.ContainsKey(layer))
                return;

            await layers[layer].ChangeStateNowAsync(type, param);

            while(true)
            {
                if (layers[layer].ContainsState(type))
                    return;

                await UniTask.Yield();
            }
        }
        #endregion
    }
}
