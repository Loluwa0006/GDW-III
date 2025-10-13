using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeflectManager : MonoBehaviour
{
    


    [SerializeField] BoxCollider deflectHitbox;


    [SerializeField] PlayerInput playerInput;

    [SerializeField] BaseSpeaker character;

    [SerializeField] MeshRenderer mesh;

    [SerializeField] ParticleSystem partialDeflectBrokenParticles;
    [SerializeField] BufferHelper deflectBuffer;


    [Header("Materials")]
    [SerializeField] Material baseDeflect;
    [SerializeField] Material partialDeflect;
    [SerializeField] Material failedDeflect;


    public UnityEvent<BaseEcho, bool> deflectedBall;

    [HideInInspector] public bool stateAllowsDeflect = true;

    [Header("Deflect Settings")]
     [SerializeField]  float deflectCooldown = 0.6f;
     [SerializeField]  float deflectDuration = 1.4f;
     [SerializeField]  float badDeflectDuration = 0.45f;
    [Header("Deflect Gamefeel")]
    [SerializeField] ParticleSystem parryParticles;
    float deflectTracker = 0.0f;

    float cooldownTracker = 0.0f;

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
        character.characterStateMachine.transitionedStates.AddListener(OnStateTransitioned);
        partialDeflectBrokenParticles.Stop();
        cooldownTracker = 0.0f;
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
    

        if (deflectBuffer.Buffered)
        {
            deflectBuffer.Consume();
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
        && !DeflectOnCooldown();
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
        return cooldownTracker > 0.0f;
    }

    public void SetDeflectEnabled(bool enabled)
    {
        deflectHitbox.enabled = enabled;
        mesh.enabled = enabled;
        isDeflecting = enabled;
    }

    public IEnumerator OnSuccessfulDeflect(BaseEcho ball) //success is true whehter its partial or not, you succcessfuly didn't get hit is what it means
    {
        deflectedBall.Invoke(ball, IsPartialDeflect());
        yield return null;
        SetDeflectEnabled(false);
        cooldownTracker = 0.0f;

        parryParticles.transform.rotation = transform.rotation;
        parryParticles.Play();
    }

    public void OnDeflectBroken()
    {
        partialDeflectBrokenParticles.Play();
    }
}
