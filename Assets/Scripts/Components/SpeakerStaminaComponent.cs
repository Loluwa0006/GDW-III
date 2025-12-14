using UnityEngine;

public class SpeakerStaminaComponent : StaminaComponent
{

    [Header("Speaker Attributes")]
    [SerializeField] DeflectManager deflectManager;
    [SerializeField] HealthComponent healthComponent;

    protected override void InitComponent()
    {
        base.InitComponent();
        healthComponent.entityDamaged.AddListener(HandleDamage);
        deflectManager.deflectedBall.AddListener(HandleBallDeflect);
    }


    public override void HandleDamage(DamageInfo info)
    {
        if (info.damageSource == DamageSource.Ball)
        {
            if (InDangerZone())
            {
                foresightAuraHum.Stop();
                foresightElectricityCrackle.Stop();
                Debug.Log("Stopped aura hum and electrictiy crackle");
                healthComponent.KillEntity(info, healthComponent); //if we're in danger and we got hit by the ball, we're KO'ed
                return;
            }
        }
        DamageStamina(info.damage, info.maxStaminaDamage, info.dealsGrayStaminaDamage);

    }

}
