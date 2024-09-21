using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public static class AnimatorStateMachineEx
{
#if UNITY_EDITOR
    public static AnimatorState FindState(this AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name.Equals(stateName))
            {
                return childState.state;
            }
        }

        return null;
    }

    public static AnimatorStateTransition FindAnyStateTranslation(this AnimatorStateMachine stateMachine, AnimatorState targetState, string name)
    {
        foreach (var transition in stateMachine.anyStateTransitions)
        {
            if (transition.name == name && transition.destinationState == targetState)
            {
                return transition;
            }
        }
        return null;
    }
#endif
}

public static class AnimatorStateEx
{

#if UNITY_EDITOR
    public static AnimatorStateTransition FindTranslation(this AnimatorState state, AnimatorState targetState, string name)
    {
        foreach (var transition in state.transitions)
        {
            if (transition.name == name && transition.destinationState == targetState)
            {
                return transition;
            }
        }

        return null;
    }
#endif
}
