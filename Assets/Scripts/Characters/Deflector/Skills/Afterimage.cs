using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Afterimage : BaseSkill
{


    [SerializeField] int activeCloneStaminaDrain = 8;

    [SerializeField] float recoveryDuration = 0.2f;

    
    
    [Header("Clone Variables")]

        [SerializeField] AfterimageClone cloneObject;
        [SerializeField] MeshFilter cloneMesh;
    [SerializeField] float maxChargeDuration = 1.5f;
    [SerializeField] float maxClonePlacement = 125.0f;
    [SerializeField] float minDistanceFromWall = 3.0f; //offset from wall to prevent clipping

    [Header("Run Variables")]

    [SerializeField] float moveSpeed = 12.0f;
    [SerializeField] float moveAcceleration = 12.0f / 7.0f;

   
    int timeUntilDrain = 0;

    float chargeTracker = 0.0f; 

    

    Rigidbody _rb;

    Vector3 moveDir;

    bool placingClone = true;


    LayerMask wallMask;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        cloneObject.transform.parent = null; // it shouldn't follow the player around
        cloneObject.gameObject.SetActive(false);
        _rb = cha.GetComponent<Rigidbody>();
        wallMask = LayerMask.GetMask("Wall");
 
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        chargeTracker = 0.0f;
        Debug.Log("Entered afterimage state");
        base.Enter(msg);
        placingClone = !cloneObject.gameObject.activeSelf;
        
        if (!placingClone)
        {
            StartCoroutine(WarpToClone());
        }
        else
        {
            cloneObject.gameObject.SetActive(true);
        }
    }


    public override void Process()
    {
        moveDir = GetMovementDir();
        if (placingClone)
        {
            chargeTracker += Time.deltaTime;
            if (chargeTracker > maxChargeDuration) { chargeTracker = maxChargeDuration; }


            float maxDistance = maxClonePlacement;
            Ray wallRay = new(character.transform.position, moveDir);
            if (Physics.Raycast(wallRay, out RaycastHit hit, maxDistance, wallMask))
            {
                maxDistance = hit.distance - minDistanceFromWall;
            }

            float t = chargeTracker / maxChargeDuration;
            Vector3 spawnPos = Vector3.Lerp(character.transform.position, character.transform.position + (moveDir * maxDistance), t);

            cloneObject.transform.position = spawnPos;
            cloneObject.transform.forward = moveDir;

            if (!skillAction.IsPressed())
            {
                cloneObject.afterimageCollider.enabled = true;
                ExitState();
            }
        }
      
    }
    public override void PhysicsProcess()
    {
        base.PhysicsProcess();
        Vector3 newSpeed = character.velocityManager.GetInternalSpeed();
        newSpeed += moveDir.normalized * moveAcceleration;

        newSpeed = Vector3.ClampMagnitude(newSpeed, moveSpeed);
        if (placingClone)
        {
            newSpeed.y = 0;
        }
        character.velocityManager.OverwriteInternalSpeed(newSpeed);
        if (oppositeSkillBuffer.Buffered)
        {
            oppositeSkillBuffer.Consume();
            fsm.TransitionToSkill(oppositeSkillIndex);
        }
        DrainStamina();
    }





    public IEnumerator WarpToClone()
    {
        _rb.position = cloneObject.transform.position;
        yield return null;
        staminaComponent.ConsumeForesight();
        OnCloneDestroyed();
        ExitState();
    }
    public void OnCloneDestroyed()
    {
        cloneObject.afterimageCollider.enabled = false;
        cloneObject.gameObject.SetActive(false);
    }

    void ExitState()
    {
        timeUntilDrain = activeCloneStaminaDrain;
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

    public override void InactivePhysicsProcess()
    {
        if (!cloneObject.gameObject.activeSelf) { return;  }

        DrainStamina();
    }
   
    void DrainStamina()
    {
        timeUntilDrain -= 1;
        if (timeUntilDrain <= 0)
        {
            timeUntilDrain = activeCloneStaminaDrain;
           if (!staminaComponent.HasForesight()) staminaComponent.DamageStamina(1, false);
            if (staminaComponent.GetStamina() <= staminaCost && !staminaComponent.HasForesight())
            {
                Debug.Log("Destroying clone, ran outta stamina ");
                OnCloneDestroyed();
            }
        }
    }
    
}
