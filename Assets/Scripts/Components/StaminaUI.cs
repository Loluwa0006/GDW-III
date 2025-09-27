using UnityEngine;
using TMPro;

public class StaminaUI : MonoBehaviour
{
    StaminaComponent staminaComponent;
    [SerializeField] TMP_Text staminaDisplay;



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
            staminaDisplay.text = "STA: " + staminaComponent.GetStamina();
        }
    }
}