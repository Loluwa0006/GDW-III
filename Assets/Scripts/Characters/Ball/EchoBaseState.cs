using UnityEngine;

public class EchoBaseState : BaseState
{
    [HideInInspector] public BaseEcho echo;
    public bool lightenAllowed = false;
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        echo = cha.GetComponent<BaseEcho>();
    }

    public virtual void OnBallIgnited()
    {

    }
}
