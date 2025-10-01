using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
public class CharacterStateMachine : MonoBehaviour
{

    public CharacterBaseState currentState;
    public UnityEvent<StateTransitionInfo> transitionedStates = new(); //order is previous state, current state;

    [SerializeField] BaseCharacter character;
    [SerializeField] GameObject bufferHolder;

    List<CharacterBaseState> statesWithInactiveProcess = new();
    List<CharacterBaseState> statesWithInactivePhysicsProcess = new();
    Dictionary<System.Type, CharacterBaseState> stateLookup = new();
    Dictionary<int,  BaseSkill> skillLookup = new();
    CharacterBaseState previousState;


    bool initMachine = false;

    [System.Serializable]
    public class StateTransitionInfo
    {
        public CharacterBaseState prevState;
        public CharacterBaseState currentState;

        public StateTransitionInfo(CharacterBaseState prev, CharacterBaseState current)
        {
            prevState = prev;
            currentState = current;
        }
    }

    public void InitMachine()
    {

        if (currentState == null)
        {
            Debug.LogError("Initial state not set in editor for character");
            return;
        }

        for (int i = 0; i < transform.childCount; i++) 
        {
            Transform child = transform.GetChild(i);
            if (!child.TryGetComponent(out CharacterBaseState state)) { Debug.Log("Child " + child.name + "is not a state."); continue; }
            state.InitState(character, this);
            stateLookup[state.GetType()] = state;

            if (state.hasInactiveProcess)
                statesWithInactiveProcess.Add(state);

            if (state.hasInactivePhysicsProcess)
                statesWithInactivePhysicsProcess.Add(state);
            if (state is BaseSkill skill)
            {
                skillLookup.Add(skillLookup.Count + 1, skill);
            }

            
        }

        foreach (Transform t in bufferHolder.transform)
        {
            if (!t.TryGetComponent<BufferHelper>(out var bufferHelper)) { continue; }
            bufferHelper.InitBuffer(character.playerInput);
        }
        initMachine = true;
        
    }


    public void UpdateState()
    {
        if (!initMachine) { return; }
        currentState.Process();

        foreach (var state in statesWithInactiveProcess)
        {
            if (state == currentState) { continue; }
            state.InactiveProcess();
        }
    }


    public void FixedUpdateState()
    {
        if (!initMachine) { return; }

        currentState.PhysicsProcess();

        foreach (var state in statesWithInactivePhysicsProcess)
        {
            if (state == currentState) { continue; }
            state.InactivePhysicsProcess();
        }
    }

    public void TransitionTo<T>(Dictionary<string, object> msg = null) where T : CharacterBaseState
    {
        if (!initMachine) { return; }
        if (!stateLookup.ContainsKey(typeof(T)))
        {
            Debug.LogError("Could not find state of type " +  typeof(T));
        }
        if (stateLookup[typeof(T)] == currentState)
        {
            Debug.Log("Can't transition to current state again");
            return;
        }

        if (currentState != null)
        {
            previousState = currentState;
            currentState.Exit();
        }
        currentState = stateLookup[typeof(T)];
        currentState.Enter(msg);
        Debug.Log("Transitioning to state " + currentState.name + " from state " + previousState);

        transitionedStates.Invoke(new StateTransitionInfo(previousState, currentState)); ;
    }
    public void TransitionToSkill(int index, Dictionary<string, object> msg = null) 
    {
        if (!initMachine) { return; }
        if (!skillLookup.ContainsKey(index))
        {
            Debug.LogError("Could not find skill " + index);
        }
       var skill = skillLookup[index];
        if (!skill.SkillAvailable())
        {
            Debug.Log("Skill not available.");
            return;
        }
        if (currentState != null)
        {
            previousState = currentState;
            currentState.Exit();
        }
        currentState = skillLookup[index];
        currentState.Enter(msg);
        Debug.Log("Transitioning to state " + currentState.name + " from state " + previousState);

        transitionedStates.Invoke(new StateTransitionInfo(previousState, currentState)); ;
    }



    public CharacterBaseState TryGetState<T>() where T : CharacterBaseState
    {
        if (!stateLookup.ContainsKey(typeof(T)))
        {
            Debug.LogError("Could not find state of type " + typeof (T));
            return null;
        }
        return stateLookup[typeof(T)];
    }

    public BufferHelper TryGetBuffer(string bufferName)
    {
        foreach (Transform t in bufferHolder.transform)
        {
            if (t.name == bufferName)
            {
                return t.GetComponent<BufferHelper>();
            }
        }
        return null;
    }

}
