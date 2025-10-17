using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GetHitState : CharacterBaseState
{
    [SerializeField] AirStateResource airStateHelper;
    [SerializeField] float knockbackDistance = 8.0f;
    [SerializeField] AirStateResource.JumpInfo jumpInfo;

    [SerializeField] BufferHelper jumpBuffer;

    const int DEFAULT_BALL_HITSTUN = 15;

    int hitstunFrames = 0;

    Rigidbody _rb;

    Vector2 flinchDir;

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = cha.GetComponent<Rigidbody>();
    }
    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        hitstunFrames = 0;
        flinchDir = new Vector2(1, 1);
        if (msg != null)
        {
            if (msg.TryGetValue("Hitstun", out object stun))
            {
                hitstunFrames = (int)stun;
            }
            if (msg.TryGetValue("KnockbackDir", out object dir ))
            {
                flinchDir = (Vector2)dir;
                flinchDir = flinchDir.normalized;
            }
        }
        if (hitstunFrames == 0.0f)
        {
            hitstunFrames = DEFAULT_BALL_HITSTUN;
        }
        StartCoroutine(HitstunLogic());

        Vector2 newSpeed = _rb.linearVelocity;
        newSpeed.y = jumpInfo.jumpVelocity;
    }

    IEnumerator HitstunLogic()
    {
        yield return new WaitForSeconds(hitstunFrames);
        ExitHitstunState();
    }

    void ExitHitstunState()
    {
        if (jumpBuffer.Buffered)
        {
            if (IsGrounded())
            {
                fsm.TransitionTo<JumpState>();
                return;
            }
        }  
        else
        {
            fsm.TransitionTo<FallState>();
        }
    }

    public override void PhysicsProcess()
    {
        Vector2 horizKnockback = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).normalized * knockbackDistance;

        float vertKnockback = _rb.linearVelocity.y;
        if (!IsGrounded())
        {
            if (vertKnockback > 0)
            {
                vertKnockback -= jumpInfo.jumpGravity;
            }
            else
            {
                vertKnockback -= jumpInfo.fallGravity;
            }
        }
        if (vertKnockback < jumpInfo.maxFallSpeed) { vertKnockback = jumpInfo.maxFallSpeed; }
        _rb.linearVelocity = new Vector3(horizKnockback.x, vertKnockback, horizKnockback.y);


 
    }
}
