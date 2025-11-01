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
    [SerializeField] float gravity = 0.2f;
    [SerializeField] float maxFallSpeed = -40;

    [Header("Power")]

    [SerializeField] float redirectPower = 0.1f;

    [Header("QOL")]
    [SerializeField] int bounceCooldown = 6;
    [SerializeField] float redirectRange = 3.0f;


    [SerializeField] LayerMask allowedLayers;

    int frameTracker = 0;
    bool inGrace = false;



    Vector3 moveDir = new();
    Vector3 FAILED_RAYCAST_VALUE = new(-1, -1, -1);
    Vector3 internalVelocity = new();

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        maxFallSpeed = Mathf.Abs(maxFallSpeed) * -1; //make sure its negative;
        gravity = Mathf.Abs(gravity);
    }

    public override void Enter(Dictionary<string, object> msg = null)
    {
        base.Enter(msg);
        inGrace = true;
        frameTracker = 0;
        if (!staminaComponent.HasForesight())
        {
           // staminaComponent.DamageStamina(staminaCost, 0, false);
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
        }

        AddGravity();

        if (!inGrace)
        {
            if (frameTracker % staminaDrain ==0)
            {
             //   staminaComponent.DamageStamina(1, 0, false);
                frameTracker = 0;
                if (staminaComponent.GetStamina() <= staminaCost && !staminaComponent.HasForesight()) 
                {
                    ExitState();
                    return;
                }
            }
        }
        Vector3 normal = PerformRaycast();
        if (normal != FAILED_RAYCAST_VALUE)
        {
            Debug.Log("BONCING YIPPE");
            PerformRedirect(normal);
            ExitState();
            return;
        }
       

        if (IsGrounded() )
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

        if (oppositeSkillBuffer != null)
        {
            if (oppositeSkillBuffer.Buffered)
            {
                oppositeSkillBuffer.Consume();
                fsm.TransitionToSkill(oppositeSkillIndex);
            }
        }
    }

    void AddGravity()
    {
        if (internalVelocity.y > maxFallSpeed)
        {
            Debug.Log("Applying gravity");
            internalVelocity.y -= gravity;
            if (internalVelocity.y < maxFallSpeed)
            {
                internalVelocity.y = maxFallSpeed;
            }
            character.velocityManager.OverwriteInternalSpeed(internalVelocity);
        }
        Debug.Log("added " + gravity + "to create new speed " + internalVelocity.y);

    }



    protected void AirStrafeLogic()
    {
        if (moveDir.magnitude < MOVE_DEADZONE) { return; }
        Vector3 influenceVector = moveDir ; // current dir
        Vector3 adjustedDir;
        Debug.Log("Current redirect speed is " + internalVelocity);
        if (internalVelocity.magnitude > 0.01f)
        {
            adjustedDir = Vector3.Lerp(internalVelocity.normalized, influenceVector, strafeControl); // current moved towards wanted by % strafe control
            Vector3 adjustedVector = adjustedDir * internalVelocity.magnitude;
            Vector3 finalVector = new Vector3(adjustedVector.x, internalVelocity.y, adjustedVector.z).normalized * internalVelocity.magnitude;
            character.velocityManager.OverwriteInternalSpeed(finalVector); // apply new speed
            Debug.Log("New redirect speed is " + finalVector);

        }


        foreach (var velocity in character.velocityManager.GetAllExternalSpeed()) //now do the same thing to every other velocity velocity 
        {
            if (velocity.Value.magnitude < 0.01f) continue;
            adjustedDir = Vector3.Lerp(velocity.Value.normalized, influenceVector, strafeControl);
            character.velocityManager.OverwriteExternalSpeed(velocity.Key, adjustedDir * velocity.Value.magnitude);
        }
    }

    Vector3 PerformRaycast()
    {
        Vector3 raycastDir = character.velocityManager.GetTotalSpeed().normalized;
        Vector3 raycastResult = new();
        Ray ray = new (_rbCollider.bounds.center, raycastDir);

        Debug.Log("Aiming in direction " + raycastDir.ToString());
       
        if (Physics.Raycast(ray, out RaycastHit hit, redirectRange, allowedLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.gameObject == character) //can't hit self
            {
                raycastResult = FAILED_RAYCAST_VALUE;
            }
            else
            {
                Debug.Log("redirected off object " + hit.collider.name + " with normal of " + hit.normal);
                raycastResult = hit.normal;
            }
        }
        else {
            raycastResult = FAILED_RAYCAST_VALUE;
        }
        Color rayColor = raycastResult == FAILED_RAYCAST_VALUE ? Color.red : Color.green;
        Debug.DrawRay(_rbCollider.bounds.center, raycastDir  * redirectRange, rayColor);
        return raycastResult;

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

        character.velocityManager.OverwriteInternalSpeed(reflectedDir * newMagnitude); // then we apply the velocity using the new dir and bonus we calculated

        Debug.Log("Reflected char velocity from " + velocityVector + " to new vector " + reflectedDir * newMagnitude);
        foreach (var velocity in character.velocityManager.GetAllExternalSpeed()) //now do the same thing to every other velocity velocity 
        {
            velocityVector = character.velocityManager.GetExternalSpeed(velocity.Key);
            newMagnitude = velocityVector.magnitude + (velocityVector.magnitude * redirectPower);
            reflectedDir = Vector3.Reflect(velocity.Value, normal).normalized;
           
          if (influenceVector.magnitude > MOVE_DEADZONE)  reflectedDir = Vector3.Lerp(reflectedDir, influenceVector, bounceControl);
            character.velocityManager.OverwriteExternalSpeed(velocity.Key, reflectedDir * newMagnitude);

        }

        staminaComponent.ConsumeForesight();



    }

    void ExitState()
    {
        skillBuffer.Consume();
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
