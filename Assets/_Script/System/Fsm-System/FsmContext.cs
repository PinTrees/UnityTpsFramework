using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using Fsm.State;
using Cysharp.Threading.Tasks;

namespace Fsm
{
    public struct LayerStateChangeContainer
    {
        public string LayerName;
        public string StateName;
        public object Param;
    }

    /// <summary>
    /// FSM(유한 상태 기계)의 컨텍스트를 관리하는 클래스입니다.
    /// 상태를 추가, 변경, 업데이트하는 기능을 제공합니다.
    /// </summary> 
    public class FsmContext
    {
        public FsmObjectBase fsmObject;
        public List<string> stateChangeHistory = new();
        [SerializeField] public Dictionary<string, FsmLayer> layers = new();        // FSM 컨텍스트를 소유하는 오브젝트

        // Runtime Value
        private readonly Queue<LayerStateChangeContainer> changeStateQueue = new();      // 상태 변경을 위한 대기열
        private bool isChangeStateLocked = false;                                        // 상태 변경 상태 잠금

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
            layer.context = this;
            layer.fsmObject = fsmObject;
            layer.Init();

            layers[layerName] = layer;
        }
        public FsmLayer CreateLayer(string layerName)
        {
            var layer = new FsmLayer();
            layer.context = this;
            layer.fsmObject = fsmObject;
            layer.Init();

            layers[layerName] = layer;
            return layer;
        }
        public FsmLayer FindLayer(string layerName)
        {
            if (!layers.ContainsKey(layerName))
                return null;
            return layers[layerName];   
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

            fsmObject.StartCoroutine(ChangeStateNowAsync(layer, type, param).ToCoroutine());
        }

        public async UniTask ChangeStateNowAsync(string layer, string type, object param=null)
        {
            if (!layers.ContainsKey(layer))
                return;

            await layers[layer].ChangeStateNowAsync(type, param);
        }
        #endregion
    }
}
