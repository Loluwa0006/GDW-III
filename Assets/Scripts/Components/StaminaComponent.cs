using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StaminaComponent : MonoBehaviour
{
    public UnityEvent regainedGrayStamina = new();

    [SerializeField] HealthComponent healthComponent;
    [SerializeField] DeflectManager deflectManager;


    //Regen
    const float STAMINA_REGEN_RATE = 8.5f; //stamina regen per 10 seconds
    const float SUDDEN_DEATH_STAMINA_DRAIN_DELAY = 1.0f;
    const float STAMINA_USAGE_REGEN_DELAY = 1.8f;

    //Ball Damage, numbers represent percent
    const int BALL_MAX_STAMINA_DAMAGE = 25; 
    const int PARTIAL_DEFLECT_STAMINA_DAMAGE = 20;

    //Danger Zone
    const int DANGER_ZONE_THRESHOLD = 25;

    //Initial Values
    const int DEFAULT_MAX_STAMINA = 100;


    float stamina = DEFAULT_MAX_STAMINA;
    float maxStamina = DEFAULT_MAX_STAMINA;
    float grayStamina = 0.0f;

    bool inDangerZone = false;
    bool inSuddenDeath = false;

    float delayTracker = 0.0f;
    float suddenDeathTracker = 0.0f;

    private void Start()
    {
        if (healthComponent == null)
        {
            healthComponent = transform.parent.GetComponentInChildren<HealthComponent>();
        }
        if (deflectManager == null)
        {
            deflectManager = transform.parent.GetComponentInChildren<DeflectManager>();
        }
        healthComponent.entityDamaged.AddListener(HandleDamage); 
        deflectManager.deflectedBall.AddListener(HandleBallDeflect);
    }

    private void Update()
    {
        RegenStamina();
        SetDangerZone();
        HandleStaminaDelay();
        SuddenDeathLogic();
    }

    void SetDangerZone()
    {
        inDangerZone = (stamina <= DANGER_ZONE_THRESHOLD);

    }
    void HandleStaminaDelay()
    {
        delayTracker -= Time.deltaTime;
        if (delayTracker <= 0.0f) { delayTracker = 0.0f; }
    }

    void RegenStamina()
    {
        if (delayTracker > 0.001f) { return; }
        float regenAmount = STAMINA_REGEN_RATE * Time.deltaTime;
        stamina += regenAmount;
        if (stamina > maxStamina) { stamina = maxStamina; }
        grayStamina -= regenAmount; // stamina regen removes gray stamina, so for every point we add to stamina, we remove from graystmina
        if (grayStamina < 0.0f) { grayStamina = 0.0f; }
    }

    void SuddenDeathLogic()
    {
        if (!inSuddenDeath) { return; }
        suddenDeathTracker -= Time.deltaTime; // deduct time if we're in sudden death
        if (suddenDeathTracker <= 0.001f)
        {
            suddenDeathTracker = SUDDEN_DEATH_STAMINA_DRAIN_DELAY; 
            if (maxStamina > 1)
            {
                maxStamina -= 1; //reduce stamina to one over time
            }
            
        }
    }

    public void HandleDamage(DamageInfo info)
    {
        if (info.damageType == DamageType.Ball)
        {
            if (inDangerZone)
            {
                healthComponent.OnEntityDeath(info, healthComponent); //if we're in danger and we got hit by the ball, we're KO'ed
                return;
            }
            maxStamina -= BALL_MAX_STAMINA_DAMAGE; // ball reduces max stamina 
        }
        DamageStamina(info.damage, info.dealsGrayStaminaDamage);
       
    }

    public void HandleBallDeflect(BaseEcho ball, bool partialDeflect)
    {
        if (!partialDeflect)
        {
            if (grayStamina > 0.0f) { regainedGrayStamina.Invoke(); }
            stamina += grayStamina; // since we had gray while we deflected, we convert gray stamina to usable stamina
            grayStamina = 0.0f; // then clear it 
            stamina = Mathf.Clamp(stamina, 1, maxStamina); 
        }
        else
        {
            DamageStamina(PARTIAL_DEFLECT_STAMINA_DAMAGE, true);
        }

    }

    public void DamageStamina(int amount, bool useGrayStamina)
    {
        stamina = Mathf.Clamp(stamina, 1, maxStamina); // make sure stamina is within bounds,
                                                       // if it wasn't we would be subtracting stamina that we would lose anyways from max being reduced
        stamina -= amount;
        if (useGrayStamina) //replace the usable stamina with gray stamina
        {
            grayStamina += amount;
            if (stamina + grayStamina > maxStamina) { grayStamina = maxStamina - stamina; } // make sure gray stamina won't take us over max stamina even if we got it back
        }
        stamina = Mathf.Clamp(stamina, 1, maxStamina);
        ResetStaminaDelay(); 
     
    }

    public void ResetComponent(bool resetSuddenDeath)
    {
        maxStamina = DEFAULT_MAX_STAMINA;
        stamina = DEFAULT_MAX_STAMINA;
        grayStamina = 0.0f;
        delayTracker = 0.0f;
        if (resetSuddenDeath)
        {
            inSuddenDeath = false;
            suddenDeathTracker = 0.0f;
        }
    }

    void ResetStaminaDelay()
    {
        delayTracker = STAMINA_USAGE_REGEN_DELAY;
    }
    public float GetStamina()
    {
        return stamina;
    }

    public float GetGrayStamina()
    {
        return grayStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }
      
    public bool InDangerZone()
    {
        return inDangerZone;
    }

    public void EnterSuddenDeath()
    {
        suddenDeathTracker = SUDDEN_DEATH_STAMINA_DRAIN_DELAY; 
        inSuddenDeath = true;
    }

   
}
