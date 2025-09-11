using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
     HealthComponent healthComponent;
    [SerializeField] TMP_Text healthDisplay; 



    private void Awake()
    {
        if (healthDisplay == null)
        {
            healthDisplay = GetComponent<TMP_Text>();
        } 
    }

    public void InitHealthDisplay(HealthComponent hpComponent)
    {
        healthComponent = hpComponent;
        healthDisplay.text = "HP: " + hpComponent.GetHealth();

        hpComponent.entityDamaged.AddListener(UpdateHealthDisplay);
        hpComponent.entityDefeated.AddListener(OnEntityDefeated);
    }

    public void UpdateHealthDisplay(DamageInfo info)
    {
        healthDisplay.text = "HP: " + healthComponent.GetHealth();
    }

    public void OnEntityDefeated(DamageInfo info, HealthComponent victim)
    {
        healthDisplay.text =  victim.hitboxOwner.name + " DEFEATED BY " + info.damageType.ToString();
    }
}
