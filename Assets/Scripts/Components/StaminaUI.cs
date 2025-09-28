using UnityEngine;
using TMPro;

public class StaminaUI : MonoBehaviour
{
    StaminaComponent staminaComponent;
    [SerializeField] TMP_Text staminaDisplay;
    [SerializeField] TMP_Text maxStaminaDisplay;
    [SerializeField] Color healthyStamina;
    [SerializeField] Color dangerStamina;




    private void Awake()
    {
        if (staminaDisplay == null)
        {
            staminaDisplay = GetComponent<TMP_Text>();
        }
    }

    public void InitStaminaDisplay(StaminaComponent sta)
    {
        staminaComponent = sta;
    }

    private void Update()
    {
        if (staminaComponent != null)
        {
            staminaDisplay.text = "STA: " + Mathf.RoundToInt(staminaComponent.GetStamina());
            float gray = staminaComponent.GetGrayStamina();
            if (staminaComponent.GetGrayStamina() > 0) { staminaDisplay.text += " + " + Mathf.RoundToInt(staminaComponent.GetGrayStamina()); }
            staminaDisplay.color = staminaComponent.InDangerZone() ? dangerStamina : healthyStamina;
            maxStaminaDisplay.text = "MAX: " + staminaComponent.GetMaxStamina();
        }
    }
}