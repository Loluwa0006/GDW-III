using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Afterimage : BaseSkill
{
    [SerializeField] int spawnDistance = 1;

    [SerializeField] int activeCloneStaminaDrain = 8;

    [SerializeField] float recoveryDuration = 0.2f;

    
    
    [Header("Clone Variables")]

        [SerializeField] GameObject cloneObject;
        [SerializeField] MeshFilter cloneMesh;

    [Header("Dodge Variables")]

        [SerializeField] AirStateResource.JumpInfo jumpInfo;
        [SerializeField] float jumpDistance = 7.0f;

    float jumpSpeed;

    int timeUntilDrain = 0;
    Vector3 moveDir = Vector3.zero;

    Rigidbody _rb;


    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        cloneObject.transform.parent = null; // it shouldn't follow the player around
        cloneObject.SetActive(false);
        _rb = cha.GetComponent<Rigidbody>();
        jumpInfo.InitJumpInfo();

        jumpSpeed = jumpDistance / (jumpInfo.jumpTimeToPeak + jumpInfo.jumpTimeToDescent);
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        Debug.Log("Entered afterimage state");
        base.Enter(msg);
        if (!cloneObject.activeSelf)
        {
            base.OnSkillUsed();
            SpawnClone();
            PerformJump();
        }
        else
        {
            StartCoroutine(WarpToClone());
        }
    }

    public void PerformJump()
    {
        Vector3 newSpeed = -moveDir * jumpSpeed;
        newSpeed.y = jumpInfo.jumpVelocity;
        _rb.linearVelocity = newSpeed;
    }
    public override void PhysicsProcess()
    {
        base.PhysicsProcess();
        Vector3 newSpeed = _rb.linearVelocity;
        if (newSpeed.y > 0)
        {
            newSpeed.y -= jumpInfo.jumpGravity * Time.fixedDeltaTime;
        }
        else
        {
            newSpeed.y -= jumpInfo.fallGravity * Time.fixedDeltaTime;
        }
        _rb.linearVelocity = newSpeed;
        if (IsGrounded() && newSpeed.y <= 0)
        {
            ExitState();
        }
        if (oppositeSkillBuffer.Buffered)
        {
            oppositeSkillBuffer.Consume();
            fsm.TransitionToSkill(oppositeSkillIndex);
        }
    }

    public void SpawnClone()
    {
        Debug.Log("Spawning clone");
        cloneObject.SetActive(true);
        moveDir = GetMovementDir();
        if (moveDir == Vector3.zero) moveDir = new(0, 0, 1);
        Vector3 spawnPos = moveDir * spawnDistance;

        cloneObject.transform.position = character.transform.position + spawnPos;
        cloneObject.transform.LookAt(moveDir);
    }


    public IEnumerator WarpToClone()
    {
        _rb.position = cloneObject.transform.position;
        yield return null;
        Debug.Log("Destroying clone, warping to it");
        OnCloneDestroyed();
        ExitState();
    }
    public void OnCloneDestroyed()
    {
        cloneObject.SetActive(false);
    }

    void ExitState()
    {
        Debug.Log("Exiting afterimage state");
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
        if (!cloneObject.activeSelf) { return;  }

        timeUntilDrain -= 1;
        if (timeUntilDrain <= 0)
        {
            timeUntilDrain = activeCloneStaminaDrain;
            staminaComponent.DamageStamina(1, false);
            if (staminaComponent.GetStamina() <= staminaCost)
            {
                Debug.Log("Destroying clone, ran outta stamina ");
                OnCloneDestroyed();
            }
        }


    }
   
    
}
