using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grapple : BaseSkill
{

    [SerializeField] int activeGrappleStaminaDrain = 8;
    [SerializeField] float grappleDistance = 50.0f;
    [SerializeField] float grappleStrength = 5.0f;
    [SerializeField] GameObject grappleObject;
    [SerializeField] LineRenderer grappleLine;

    LayerMask wallLayer;
    LayerMask ballLayer;

    int timeUntilDrain = 0;

    Transform grappleTarget;
   
    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        wallLayer = LayerMask.GetMask("Wall");
        ballLayer = LayerMask.GetMask("Ball");
        if (grappleLine == null)
        {
            grappleLine = GetComponent<LineRenderer>();
        }
        grappleObject.SetActive(false);
        character.deflectManager.deflectedBall.AddListener(OnBallDeflected);
    }

    void OnBallDeflected(RicochetBall ball, bool isPartial)
    {
        grappleTarget = ball.transform;
    }
    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        skillBuffer.Consume();
        Debug.Log("In grapple state");


        if (!grappleObject.activeSelf)
        {
            CreateGrapple();
        }
        else
        {
            Debug.Log("Destroying grapple");
            DestroyGrapple();
        }

        ExitState();
    }

    void CreateGrapple()
    {

        Debug.Log("Using grapple");
        Vector3 moveDir = GetMovementDir();
        if (moveDir.magnitude > MOVE_DEADZONE)
        {
            GrappleToTerrain();
        }
        else
        {
            GrappleToBall();
        }
       
    }

    void GrappleToTerrain()
    {
        Ray ray = new(character.transform.position, GetMovementDir());

        if (Physics.Raycast(ray, out RaycastHit hit, grappleDistance, wallLayer))
        {
            Vector3 pull = (hit.point - character.transform.position).normalized * grappleStrength;
            character.velocityManager.AddExternalSpeed(pull, "GrapplePull");
            ConfigureGrapple(hit);
            base.OnSkillUsed();

        }
        else
        {
            Debug.Log("Raycast did not hit object");
        }
    }

    void GrappleToBall()
    {
        if (grappleTarget == null) { return; }
        Vector3 targetDir = (grappleTarget.position - character.transform.position).normalized;
        Ray ray = new(character.transform.position, targetDir );

        if (Physics.Raycast(ray, out RaycastHit hit, grappleDistance, ballLayer))
        {
            Vector3 pull = (hit.point - character.transform.position).normalized * grappleStrength;
            character.velocityManager.AddExternalSpeed(pull, "GrapplePull");
            ConfigureGrapple(hit, grappleTarget);

        }
        else
        {
            Debug.Log("Raycast did not hit object");
        }
    }
    public void DestroyGrapple()
    {
        grappleObject.transform.parent = this.transform;
        grappleObject.SetActive(false);
        character.velocityManager.RemoveExternalSpeedSource("GrapplePull");
        grappleLine.enabled = false;
    }

    void ConfigureGrapple(RaycastHit hit, Transform grappleParent = null)
    {
        grappleObject.SetActive(true);
        grappleObject.transform.parent = grappleParent; //grapple should not follow character
        grappleObject.transform.position = hit.point;
        grappleLine.enabled = true;
    }

    public override void InactivePhysicsProcess()
    {
        if (!grappleObject.activeSelf) { return; }

        Vector3 pull = (grappleObject.transform.position - character.transform.position).normalized * grappleStrength;
        character.velocityManager.EditExternalSpeed("GrapplePull", pull);

        grappleLine.SetPosition(0, character.transform.position);
        grappleLine.SetPosition(1, grappleObject.transform.position);

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

    public override bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost;
    }
}
