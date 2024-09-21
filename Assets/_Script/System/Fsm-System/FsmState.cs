using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fsm.State
{
    /// <summary>
    /// 인터페이스 정의
    /// FSM 상태에 필요한 기본적인 기능을 정의합니다.
    /// </summary>
    public interface IFsmStateBase
    {
        void Update();       // 매 프레임마다 호출됩니다.
        void FixedUpdate();  // 물리 업데이트마다 호출됩니다.
        void LateUpdate();   // 모든 업데이트 이후에 호출됩니다.

        void OnDrawGizmos(); // Gizmos 그리기를 위한 메소드입니다.
    }


    /// <summary>
    /// FSM의 상태를 나타내는 베이스 클래스입니다.
    /// </summary>
    public class FsmState : IFsmStateBase
    {
        public FsmObjectBase owner;
        public FsmLayer layer;                      // FSM 오브젝트의 소유자
        public string stateId { get; private set; }  // 현재 상태의 ID

        public T GetOwner<T>() where T : class  { return owner as T; }

        // Actions
        public Action onUpdate;
        public Action onFixedUpdate;
        public Action onLateUpdate;
        public Action onEnter;
        public Action onExit;

        public float stateEnterTime;
        public string currentAnimationTag;
        public bool isStartAnimation;

        private float stateEnterAnimationNormalizeTime;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="stateId"></param>
        public FsmState(string stateId)
        {
            // 상태 ID를 초기화합니다.
            this.stateId = stateId;
        }


        /// <summary>
        /// 현재 상태와 주어진 상태가 동일한지 확인합니다.
        /// </summary>
        /// <param name="otherState"></param>
        /// <returns></returns>
        public bool IsEquals(string otherState) => stateId.Equals(otherState);

        /// <summary>
        /// 현재 상태와 주어진 상태가 다른지 확인합니다.
        /// </summary>
        /// <param name="otherState"></param>
        /// <returns></returns>
        public bool IsNotEquals(string otherState) => !stateId.Equals(otherState);


        // 아래의 메소드들은 오버라이드 가능하며, 각 상태의 특정 동작을 구현합니다.
        #region Override Methode
        public virtual async UniTask Enter() 
        {
            stateEnterTime = Time.time;
            currentAnimationTag = "";
            isStartAnimation = false;
            stateEnterAnimationNormalizeTime = 0;
            if (onEnter != null) onEnter();
        }
        public virtual async UniTask Exit() 
        {
            if (onExit != null) onExit();
        }


        public virtual void Update() 
        {
            float n;

            if (onUpdate != null) onUpdate();

            if (currentAnimationTag != "")
            {
                if (owner.animator.IsPlaying(currentAnimationTag)) 
                {
                    if(!isStartAnimation)
                    {
                        isStartAnimation = true;
                        stateEnterAnimationNormalizeTime = owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    }

                    OnAnimationUpdate();

                    if(stateEnterAnimationNormalizeTime >= 0.99f)
                    {
                        n = owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                        if(n < stateEnterAnimationNormalizeTime)
                        {
                            stateEnterAnimationNormalizeTime = 0;
                        }
                    }
                    else if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.99f))
                    {
                        OnAnimationExit();
                    }
                }
                else 
                {
                    if (isStartAnimation)
                        OnAnimationExit();
                }
            }
        }
        public virtual void FixedUpdate() { if (onFixedUpdate != null) onFixedUpdate(); }
        public virtual void LateUpdate() { if (onLateUpdate != null) onLateUpdate(); }

        // 현재 스테이트에 할당된 애니메이션이 출력되는 시점에 자동으로 주기적으로 호출됩니다.
        public virtual void OnAnimationUpdate() { }

        /// <summary>
        /// 현재 스테이트에 할당된 애니메이션이 종료되는 시점에 자동호출됩니다.
        /// 루프 애니메이션의 경우 정확한 타이밍에 호출되지 않을 수 있습니다.
        /// </summary>
        public virtual void OnAnimationExit() { }

        public virtual void OnDrawGizmos() { }
        #endregion
    }
}