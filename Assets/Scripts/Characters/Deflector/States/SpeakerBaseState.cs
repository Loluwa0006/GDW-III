using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpeakerBaseState : BaseState
{

    [HideInInspector] public BaseSpeaker speaker;
    public bool deflectAllowed = true;


    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        speaker = cha.GetComponent<BaseSpeaker>();
    }

}
