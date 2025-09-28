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

    public UnityEvent<bool> deflectedBall;

    [HideInInspector] public bool stateAllowsDeflect = true;

    float deflectCooldown = 0.6f;
    float deflectDuration = 1.4f;
    float badDeflectDuration = 0.3f;
    float deflectTracker = 0.0f;

    bool deflectOnCooldown = false;

    bool isDeflecting = false;

    Vector2 moveDir = new();

    private void Awake()
    {
        deflectHitbox.enabled = false;
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
        }
        mesh.enabled = false;
        fsm.transitionedStates.AddListener(OnStateTransitioned);
    }

    public void OnStateTransitioned(CharacterStateMachine.StateTransitionInfo transitionInfo)
    {
        stateAllowsDeflect = transitionInfo.currentState.deflectAllowed;
        if (!stateAllowsDeflect)
        {
            SetDeflectEnabled(false);
            if (isDeflecting)
            {
                StartCoroutine(CooldownLogic());
            }
        }
    }
    private void Update()
    {


        deflectTracker -= Time.deltaTime;
        if (deflectTracker <= 0.0f && isDeflecting)
        {
            deflectTracker = 0.0f;
            SetDeflectEnabled(false);
            StartCoroutine(CooldownLogic());
        }

        if (playerInput.actions["Deflect"].WasPerformedThisFrame())
        {
            bool isNowDeflecting = false; //if you weren't deflecting before, but you now are
            if (DeflectAvailable() && !isDeflecting)
            {
                Debug.Log("starting deflect logic");
                DeflectLogic();
                isNowDeflecting = true;
            }
            if (isDeflecting && !isNowDeflecting) //!isNowDeflecting means you didn't trigger a deflect with this input, so you must be trying to cancel
            {
                SetDeflectEnabled(false);
                StartCoroutine(CooldownLogic());
            }
        }
        moveDir.x = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        moveDir.y = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();


      //Debug.Log("Deflect duration is " + deflectTracker);
    }

    public bool DeflectAvailable()
    {
        return
        stateAllowsDeflect
        && !deflectOnCooldown;
    }

    void DeflectLogic()
    {
        SetDeflectEnabled(true);
        deflectTracker = deflectDuration;
    }

    IEnumerator CooldownLogic()
    {
        deflectOnCooldown = true;
        yield return new WaitForSeconds(deflectCooldown);
        deflectOnCooldown = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collider " + collision.name + " ended up in the deflect box");
        if (collision.TryGetComponent(out RicochetBall ball) && isDeflecting)
        {
            SetDeflectEnabled(false);
            ball.OnDeflect(character, moveDir);
            deflectOnCooldown = false;
            bool partialDeflect = IsPartialDeflect();
            if (partialDeflect)
            {
                Debug.Log(character.name + " did a bad deflect");
            }
            deflectedBall.Invoke(partialDeflect);
            
        }
    }
   
    public bool IsPartialDeflect()
    {
        return deflectTracker <= badDeflectDuration;
    }

    public void SetDeflectEnabled(bool enabled)
    {
        deflectHitbox.enabled = enabled;
        mesh.enabled = enabled;
        isDeflecting = enabled;
    }
}
