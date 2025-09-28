using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAirState : CharacterBaseState
{

    [SerializeField] protected AirStateResource airStateHelper;
    protected float DAMPING_RATE = 0.985f;
    protected Rigidbody _rb;

    protected AirStateResource.JumpInfo currentJumpInfo;

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        airStateHelper.InitializeResource();
        _rb = cha.GetComponent<Rigidbody>();
    }

    public override void Process()
    {
        if (playerInput.actions["SkillOne"].WasPerformedThisFrame())
        {
            Debug.Log("Skill one pressed");
            fsm.TransitionToSkill(1);
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
