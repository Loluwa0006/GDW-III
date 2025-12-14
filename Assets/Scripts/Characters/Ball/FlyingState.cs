using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FlyingState : EchoBaseState
{


    [SerializeField] protected HitboxComponent hitbox;
    [SerializeField] LayerMask speakerMask;
    [SerializeField] LayerMask terrainMask;
    [SerializeField] EchoDataResource echoData;
    public Rigidbody _rb;


    [SerializeField] float terrainCheckerDistance = 1.1f;
    [Header("Steer Settings")]
    [SerializeField] protected float minSteerForce;
    [SerializeField] protected float maxSteerForce;

    [Header("Colors")]
    [SerializeField] Material normalColor;
    [SerializeField] Material igniteColor;

    [Header("Particles")]
    [SerializeField] protected TrailRenderer echoTrail;
    [SerializeField] protected Gradient regularGradient;
    [SerializeField] protected Gradient ignitionGradient;
    [SerializeField] protected ParticleSystem ignitionTravelParticles;

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        _rb = echo.GetComponent<Rigidbody>();
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
       
        echo.velocityManager.OverwriteInternalSpeed(( echo.GetTarget().transform.position - transform.position).normalized * echo.GetSpeed());
        if (echo.isIgnited)
        {
            echoTrail.colorGradient = ignitionGradient;
        }
        else
        {
            echoTrail.colorGradient = regularGradient;
        }

    }

    protected bool HitboxCollisionLogic()
    {
        var overlap = Physics.OverlapBox(hitbox.hitboxCollider.bounds.center, hitbox.hitboxCollider.bounds.size, hitbox.transform.rotation, speakerMask, QueryTriggerInteraction.Collide);
        HealthComponent victim = null;
        foreach (var obj in overlap)
        {
            if (!obj.transform.TryGetComponent(out HealthComponent hp)) continue;
            victim = hp;
        }
        if (victim != null)
        {
            if (victim.hurtboxOwner.transform != echo.GetTarget()) 
                {
                    return false;
                } 
            Dictionary<string, object> msg = new Dictionary<string, object>();
            bool hitTarget = true;
            if (victim.hurtboxOwner.transform.TryGetComponent(out BaseSpeaker victimSpeaker))
            {
                if (victimSpeaker.deflectManager.IsDeflecting())
                {
                    if (victimSpeaker.deflectManager.IsPartialDeflect() && echo.isIgnited)
                    {
                        victimSpeaker.deflectManager.OnDeflectBroken();
                    }
                    else
                    {
                        msg["deflector"] = victimSpeaker;
                        hitTarget = false;
                        fsm.TransitionTo<DeflectionBounceState>(msg);
                    }
                }
            }

            if (hitTarget)
            {
                msg["victim"] = victim;
                fsm.TransitionTo<StrikeBounceState>(msg);
            }
        }

        return victim != null; //true means hit, false means no hit
    }

    override public void PhysicsProcess()
    {
        base.PhysicsProcess();
        if (GameManager.inSpecialStop || !echo.ballActive || echo.GetTarget() == null) { return; }
        if (HitboxCollisionLogic()) return;

        TerrainCollisionLogic();

        CurveLogic();
    }

    void CurveLogic()
    {
        var currentDir = echo.velocityManager.GetTotalSpeed().normalized;
        var desiredDir = (echo.GetTarget().transform.position - echo.transform.position).normalized;

        float speedAsPercent = Mathf.Clamp01(echo.GetSpeed() - echoData.minSpeed /  echoData.maxSpeed - echoData.minSpeed);

        float steerForce = Mathf.Lerp(minSteerForce, maxSteerForce, speedAsPercent) * Time.fixedDeltaTime;

        var newDir = Vector3.Slerp(currentDir, desiredDir, steerForce);

        var currentSpeed = echo.velocityManager.GetTotalSpeed().magnitude;

        echo.velocityManager.OverwriteInternalSpeed(newDir * currentSpeed);
    }

    void TerrainCollisionLogic()
    {
        Ray terrainRay = new(echo.transform.position, echo.velocityManager.GetTotalSpeed().normalized);
        if (Physics.Raycast(terrainRay, terrainCheckerDistance, terrainMask))
        {
            fsm.TransitionTo<TerrainBounceState>();
        }
    }


    public override void Exit()
    {
        base.Exit();
    }

 


}
