using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class SpeakerBaseSkill : BaseSkill
{
    [HideInInspector] public BaseSpeaker speaker;

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        speaker = cha.GetComponent<BaseSpeaker>();
    }
}
