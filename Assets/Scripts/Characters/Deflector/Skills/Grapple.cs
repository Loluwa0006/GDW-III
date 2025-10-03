using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grapple : BaseSkill
{

    [SerializeField] int activeGrappleStaminaDrain = 8;
    [SerializeField] float grappleDistance = 50.0f;
    [SerializeField] GameObject grappleObject;

    LayerMask wallLayer;

    int timeUntilDrain = 0;

    Rigidbody _rb;
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        wallLayer = LayerMask.GetMask("Wall");
        _rb = cha.GetComponent<Rigidbody>();
    }
    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);

        if (grappleObject.activeSelf)
        {
            CreateGrapple();
        }
        else
        {
            DestroyGrapple();
        }

        ExitState();
    }

    void CreateGrapple()
    {
        base.OnSkillUsed();

        Ray ray = new(character.transform.position, GetMovementDir());
        RaycastHit hit = new();

        if (Physics.Raycast(ray, out hit, grappleDistance, wallLayer))
        {
            ConfigureGrapple(hit);
        }
    }

    public void DestroyGrapple()
    {
        grappleObject.transform.parent = this.transform;
        grappleObject.SetActive(false);
    }

    void ConfigureGrapple(RaycastHit hit)
    {
        grappleObject.SetActive(true);
        grappleObject.transform.parent = null; //grapple should not follow character
        grappleObject.transform.position = hit.point;
    }

    public override void InactivePhysicsProcess()
    {
        if (!grappleObject.activeSelf) { return; }

        timeUntilDrain -= 1;
        if (timeUntilDrain <= 0)
        {
            timeUntilDrain = activeGrappleStaminaDrain;
            staminaComponent.DamageStamina(1, false);
            if (staminaComponent.GetStamina() <= staminaCost)
            {
                Debug.Log("Destroying clone, ran outta stamina ");
                DestroyGrapple();
            }
        }

        PullCharacter();
    }

    void PullCharacter()
    {
        
    }

    void ExitState()
    {
        Debug.Log("Exiting afterimage state");
        timeUntilDrain = activeGrappleStaminaDrain;
        if (!IsGrounded())
        {
            fsm.TransitionTo<FallState>();
        }
        else
        {
            if (GetMovementDir().magnitude >= MOVE_DEADZONE)
            {
                fsm.TransitionTo<RunState>();
            }
            else
            {
                fsm.TransitionTo<IdleState>();
            }
        }
    }
}
