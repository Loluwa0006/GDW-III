using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Scripting.APIUpdating;

public class Redirect : BaseSkill
{

    [Header("Stamina")]
    [SerializeField] int staminaDrain = 12;
    [SerializeField] int staminaDrainGracePeriod = 8;



    [Header("Air Control")]
    [SerializeField] float additionalControl = 1.3f;
    [SerializeField] float bounceControl = 0.65f;
    [SerializeField] float strafeControl = 0.7f;
    [SerializeField] AirStateResource.JumpInfo redirectAirInfo;

    [Header("Power")]

    [SerializeField] float redirectPower = 1.1f;

    [Header("QOL")]
    [SerializeField] int bounceCooldown = 6;
    [SerializeField] float redirectRange = 3.0f;


    [SerializeField] LayerMask allowedLayers;

    int frameTracker = 0;
    bool inGrace = false;



    Vector3 moveDir = new();
    Vector3 FAILED_RAYCAST_VALUE = new(-1, -1, -1);
    Vector3 internalVelocity = new();

    int cooldownTracker = 0;

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        inGrace = true;
        frameTracker = 0;
        redirectAirInfo.InitJumpInfo();
        if (!staminaComponent.HasForesight())
        {
            staminaComponent.DamageStamina(staminaCost, 0, false);
        }
     
    }

    public override void Process()
    {
        moveDir = GetMovementDir();
    }

    public override void PhysicsProcess()
    {
        internalVelocity = character.velocityManager.GetInternalSpeed();
        base.PhysicsProcess();
        frameTracker++;
        if (frameTracker >= staminaDrainGracePeriod && inGrace)
        {
            inGrace = false;
            frameTracker = 0;
        }

        if (!inGrace)
        {
            if (frameTracker == staminaDrain)
            {
                staminaComponent.DamageStamina(1, 0, false);
                frameTracker = 0;
                if (staminaComponent.GetStamina() <= staminaCost && !staminaComponent.HasForesight()) 
                {
                    ExitState();
                    return;
                }
            }
            AddGravity();
        }
        Vector3 normal = PerformRaycast();
        if (normal != FAILED_RAYCAST_VALUE && skillAction.IsPressed() && cooldownTracker <= 0)
        {
            PerformRedirect(normal);
            ExitState();
            return;
        }
        if (cooldownTracker > 0)
        {
            cooldownTracker -= 1;
        }

        if (IsGrounded() && !skillAction.IsPressed())
        {
            Debug.Log("Exiting to move states because you let go of the button while grounded ");
            if (moveDir.magnitude > MOVE_DEADZONE)
            {
                fsm.TransitionTo<RunState>();
            }
            else
            {
                fsm.TransitionTo<IdleState>();
            }
            return;
        }

        if (moveDir.magnitude > MOVE_DEADZONE)
        {
            AirStrafeLogic();
        }     
    }

    void AddGravity()
    {
        float currentY = internalVelocity.y;
        float gravityToAdd = currentY > 0 ? redirectAirInfo.fallGravity : redirectAirInfo.jumpGravity;

        // Compute the new Y but clamp so we don't exceed maxFallSpeed
        if (currentY - gravityToAdd < redirectAirInfo.maxFallSpeed) // remember maxFallSpeed is negative
        {
            // only add the portion that doesn't exceed maxFallSpeed
            gravityToAdd = redirectAirInfo.maxFallSpeed - currentY;
        }

        internalVelocity.y += gravityToAdd * Time.fixedDeltaTime;
        character.velocityManager.OverwriteInternalSpeed(internalVelocity);
    }


    protected void AirStrafeLogic()
    {
        if (moveDir.magnitude < MOVE_DEADZONE) { return; }
        Vector3 influenceVector = moveDir ; // current dir
        Vector3 adjustedDir;
        Debug.Log("Current redirect speed is " + internalVelocity);
        if (internalVelocity.magnitude > 0.01f)
        {
            adjustedDir = Vector3.Lerp(influenceVector, internalVelocity.normalized, strafeControl); // current moved towards wanted by % strafe control
            Vector3 finalVector = adjustedDir * internalVelocity.magnitude;
            character.velocityManager.OverwriteInternalSpeed(finalVector); // apply new speed
            Debug.Log("New redirect speed is " + finalVector);

        }


        foreach (var velocity in character.velocityManager.GetAllExternalSpeed()) //now do the same thing to every other velocity velocity 
        {
            if (velocity.Value.magnitude < 0.01f) continue;
            adjustedDir = Vector3.Lerp(influenceVector, velocity.Value, strafeControl);
            character.velocityManager.OverwriteExternalSpeed(velocity.Key, adjustedDir * velocity.Value.magnitude);
        }
    }

    Vector3 PerformRaycast()
    {
        Vector3 raycastDir = character.velocityManager.GetTotalSpeed().normalized;
        Ray ray = new (_rbCollider.bounds.center, raycastDir);
        if (Physics.Raycast(ray, out RaycastHit hit, redirectRange, allowedLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.gameObject == character) //can't hit self
            {
                return FAILED_RAYCAST_VALUE;
            }
            Debug.Log("redirected off object " + hit.collider.name + " with normal of " + hit.normal);
            return hit.normal;
        }
        return FAILED_RAYCAST_VALUE;

    }
    public void PerformRedirect(Vector3 normal)
    {

        Vector3 influenceVector = moveDir; // direction player wants to go
        Vector3 velocityVector = internalVelocity; //direction player is currently going
        float newMagnitude = velocityVector.magnitude + (velocityVector.magnitude * redirectPower); // new speed is current speed gets redirectPower% faster
        Vector3 reflectedDir = Vector3.Reflect(velocityVector, normal).normalized; // get the bounce angle using velocity vector
        if (influenceVector.magnitude > MOVE_DEADZONE)
        {
            reflectedDir = Vector3.Lerp(reflectedDir, influenceVector, bounceControl); //now we get a value inbetween player dir and wanted dir using bounce control as a percent. 
        }                                                                              //i.e. if bounce control is 65%, we're gonna make the velocity vector 65% closer to wanted dir

        //int useMoveDirToFlipVelocity = moveDir.magnitude > MOVE_DEADZONE ? 1 : 0;
        //reflectedDir.z = Mathf.Sign(moveDir.z * useMoveDirToFlipVelocity); // player has compelte control over whether they bounce forward or backwards though
        character.velocityManager.OverwriteInternalSpeed(reflectedDir * newMagnitude); // then we apply the velocity using the new dir and bonus we calculated

        Debug.Log("Reflected char velocity from " + velocityVector + " to new vector " + reflectedDir * newMagnitude);
        foreach (var velocity in character.velocityManager.GetAllExternalSpeed()) //now do the same thing to every other velocity velocity 
        {
            velocityVector = character.velocityManager.GetExternalSpeed(velocity.Key);
            newMagnitude = velocityVector.magnitude + (velocityVector.magnitude * redirectPower);
            reflectedDir = Vector3.Reflect(velocity.Value, normal).normalized;
           
          if (influenceVector.magnitude > MOVE_DEADZONE)  reflectedDir = Vector3.Lerp(reflectedDir, influenceVector, bounceControl);
          //  reflectedDir.z = Mathf.Sign(moveDir.z * useMoveDirToFlipVelocity);
            character.velocityManager.OverwriteExternalSpeed(velocity.Key, reflectedDir * newMagnitude);

        }

        cooldownTracker = bounceCooldown;
        staminaComponent.ConsumeForesight();



    }

    void ExitState()
    {
        if (!IsGrounded())
        {
            fsm.TransitionTo<FallState>();
        }
        else
        {
            if (moveDir.magnitude > MOVE_DEADZONE)
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
