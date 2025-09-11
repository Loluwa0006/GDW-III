using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeflectManager : MonoBehaviour
{

    [SerializeField] BoxCollider deflectHitbox;

    [SerializeField] CharacterStateMachine fsm;

    [SerializeField] PlayerInput playerInput;

    [SerializeField] BaseCharacter character;


    bool stateAllowsDeflect = true;

    float deflectCooldown = 0.6f;
    float deflectDuration = 1000.4f;

    bool deflectOnCooldown = false;

    Coroutine deflectCoroutine = null;


    Vector2 moveDir = new();

    private void Awake()
    {
        deflectHitbox.enabled = false;
    }

    public void OnStateTransitioned(CharacterStateMachine.StateTransitionInfo transitionInfo)
    {
        stateAllowsDeflect = transitionInfo.currentState.deflectAllowed;
        if (!stateAllowsDeflect)
        {
            deflectHitbox.enabled = false;
        }
    }

    private void Update()
    {

        if (playerInput.actions["Deflect"].WasPerformedThisFrame())
        {
            if (DeflectAvailable())
            {
                Debug.Log("starting deflect logic");
                deflectCoroutine = StartCoroutine(DeflectLogic());
                return;
            }
            if (deflectCoroutine != null)
            {
                StopCoroutine(deflectCoroutine);
                deflectHitbox.enabled = false;
                StartCoroutine(CooldownLogic());
            }
        }
        moveDir.x = playerInput.actions["Right"].ReadValue<float>() - playerInput.actions["Left"].ReadValue<float>();
        moveDir.y = playerInput.actions["Up"].ReadValue<float>() - playerInput.actions["Down"].ReadValue<float>();
    }

    public bool DeflectAvailable()
    {
        return
        stateAllowsDeflect
        && !deflectOnCooldown;
    }

    IEnumerator DeflectLogic()
    {
        deflectOnCooldown = true;
        deflectHitbox.enabled = true;
        yield return new WaitForSeconds(deflectDuration);
        deflectHitbox.enabled = false;
        deflectCoroutine = null;
        StartCoroutine(CooldownLogic());
    }

    IEnumerator CooldownLogic()
    {
        yield return new WaitForSeconds(deflectCooldown);
        deflectOnCooldown = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collider " + collision.name + " ended up in the deflect box");
        if (collision.TryGetComponent(out RicochetBall ball))
        {
            deflectHitbox.enabled = false;
            ball.OnDeflect(character, moveDir);
            deflectOnCooldown = false;
        }
    }

    public bool IsDeflecting()
    {
        return deflectCoroutine != null;
    }
}
