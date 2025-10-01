using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeflectManager : MonoBehaviour
{

    [SerializeField] BoxCollider deflectHitbox;

    [SerializeField] CharacterStateMachine fsm;

    [SerializeField] PlayerInput playerInput;

    [SerializeField] BaseCharacter character;

    [SerializeField] MeshRenderer mesh;

    [SerializeField] ParticleSystem partialDeflectBrokenParticles;

    [SerializeField] Material baseDeflect;
    [SerializeField] Material partialDeflect;
    [SerializeField] Material failedDeflect;

    public UnityEvent<bool> deflectedBall;

    [HideInInspector] public bool stateAllowsDeflect = true;

    float deflectCooldown = 0.6f;
    float deflectDuration = 1.4f;
    float badDeflectDuration = 0.45f;
    float deflectTracker = 0.0f;

    float cooldownTracker = 0.0f;

    bool deflectOnCooldown = false;

    bool isDeflecting = false;

    bool lockMaterial;


    private void Awake()
    {
        deflectHitbox.enabled = false;
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
        }
        mesh.enabled = false;
        fsm.transitionedStates.AddListener(OnStateTransitioned);
        partialDeflectBrokenParticles.Stop();
    }

    public void OnStateTransitioned(CharacterStateMachine.StateTransitionInfo transitionInfo)
    {
        stateAllowsDeflect = transitionInfo.currentState.deflectAllowed;
        if (!stateAllowsDeflect)
        {
            SetDeflectEnabled(false);
            if (isDeflecting)
            {
                StartCooldown();
            }
        }
    }
    private void Update()
    {


        CooldownLogic();
        DeflectLogic();
        if (!lockMaterial)
        {
            mesh.material = (deflectTracker > 0.0f && IsPartialDeflect()) ? partialDeflect : baseDeflect;
        }
    

        if (playerInput.actions["Deflect"].WasPerformedThisFrame())
        {
            bool isNowDeflecting = false; //if you weren't deflecting before, but you now are
            if (DeflectAvailable() && !isDeflecting)
            {
                Debug.Log("starting deflect logic");
                StartDeflect();
                isNowDeflecting = true;
            }
            if (isDeflecting && !isNowDeflecting) //!isNowDeflecting means you didn't trigger a deflect with this input, so you must be trying to cancel
            {
                StartCooldown();
                SetDeflectEnabled(false);
            }
        }
        

      //Debug.Log("Deflect duration is " + deflectTracker);
    }

    public bool DeflectAvailable()
    {
        return
        stateAllowsDeflect
        && !deflectOnCooldown;
    }

    void StartDeflect()
    {
        SetDeflectEnabled(true);
        deflectTracker = deflectDuration;
    }

    void StartCooldown()
    {
        cooldownTracker = deflectCooldown;
    }

    void CooldownLogic()
    {
        cooldownTracker -= Time.deltaTime;
        if (cooldownTracker < 0.0f) cooldownTracker = 0.0f;
    }

    void DeflectLogic()
    {
        deflectTracker -= Time.deltaTime;

        if (deflectTracker <= 0.0f && isDeflecting)
        {
            deflectTracker = 0.0f;
            SetDeflectEnabled(false);
            StartCooldown();
        }
    }

  
    public bool IsPartialDeflect()
    {
        return deflectTracker <= badDeflectDuration;
    }

    public bool IsDeflecting()
    {
        return isDeflecting;
    }

    public bool DeflectOnCooldown()
    {
        return cooldownTracker <= 0.0f;
    }

    public void SetDeflectEnabled(bool enabled)
    {
        deflectHitbox.enabled = enabled;
        mesh.enabled = enabled;
        isDeflecting = enabled;
    }

    public IEnumerator OnSuccessfulDeflect() //success is true whehter its partial or not, you succcessfuly didn't get hit is what it means
    {
        deflectedBall.Invoke(IsPartialDeflect());
        yield return null;
        SetDeflectEnabled(false);
        cooldownTracker = 0.0f;
    }

    public void OnDeflectBroken()
    {
        partialDeflectBrokenParticles.Play();
    }
}
