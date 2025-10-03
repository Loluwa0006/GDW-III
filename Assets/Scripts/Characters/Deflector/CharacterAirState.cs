using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAirState : CharacterBaseState
{
    [SerializeField] protected AirStateResource airStateHelper;
    [SerializeField] protected BufferHelper skillOneBuffer;
    [SerializeField] protected BufferHelper skillTwoBuffer;
    protected float DAMPING_RATE = 0.985f;
    protected Rigidbody _rb;


    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = cha.GetComponent<Rigidbody>();
    }

    public override void Process()
    {
        if (skillOneBuffer.Buffered)
        {
            Debug.Log("Skill one pressed");
            fsm.TransitionToSkill(1);
        }
        else if (skillTwoBuffer.Buffered) 
        {
            Debug.Log("Skill two pressed");
           // fsm.TransitionToSkill(2);
        }
    }


    protected Vector3 AirStrafeLogic() 
    {
        Vector3 moveDir = GetMovementDir();


        Vector3 moveSpeed = _rb.linearVelocity;
        if (new Vector3(moveSpeed.x, 0, moveSpeed.z).magnitude < airStateHelper.airStrafeSpeed)
        {
            Vector3 strafeSpeed = new Vector3(moveDir.x, 0, moveDir.z).normalized * airStateHelper.airAcceleration;
            //add existing speed
            strafeSpeed.x += moveSpeed.x;
            strafeSpeed.z += moveSpeed.z;
            //clamp strafe speed specficially, so that we can fall as fall as we need to
            strafeSpeed = Vector3.ClampMagnitude(strafeSpeed, airStateHelper.airStrafeSpeed);
            
            moveSpeed = new Vector3(strafeSpeed.x, moveSpeed.y, strafeSpeed.z);
        }
        else
        {
            moveSpeed.x *= DAMPING_RATE;
            moveSpeed.z *= DAMPING_RATE;
        }
        return moveSpeed;
    }

  

    
}
