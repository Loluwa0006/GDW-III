using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class StaminaUI : MonoBehaviour
{
    StaminaComponent staminaComponent;
    [SerializeField] TMP_Text staminaDisplay;
    [SerializeField] TMP_Text maxStaminaDisplay;
    [SerializeField] RawImage UIBackdrop;
    [SerializeField] Color healthyStamina;
    [SerializeField] Color dangerStamina;
    [SerializeField] Color foresightStamina = Color.lightBlue;

   [SerializeField] List<Color> UIColors = new();

    [SerializeField] Image maxStaminaImage;
    [SerializeField] Image usableStaminaImage;
    [SerializeField] Image grayStaminaImage;



    private void Awake()
    {
        if (staminaDisplay == null)
        {
            staminaDisplay = GetComponent<TMP_Text>();
        }
    }

    public void InitStaminaDisplay(BaseSpeaker cha, int index)
    {
        staminaComponent = cha.staminaComponent;
        UIBackdrop.color = UIColors[index - 1];
    }

    private void Update()
    {
        if (staminaComponent != null)
        {
            //staminaDisplay.text = "STA: " + Mathf.RoundToInt(staminaComponent.GetStamina());
            //float gray = staminaComponent.GetGrayStamina();
            //if (staminaComponent.GetGrayStamina() > 0) { staminaDisplay.text += " + " + Mathf.RoundToInt(staminaComponent.GetGrayStamina()); }
            //staminaDisplay.color = staminaComponent.InDangerZone() ? dangerStamina : healthyStamina;
            //maxStaminaDisplay.text = "MAX: " + staminaComponent.GetMaxStamina();
        }
        SetStaminaWheelValues();
    }

    void SetStaminaWheelValues()
    {
        if (staminaComponent == null) { return; }
        float usableStamina = staminaComponent.GetStamina();
        maxStaminaImage.fillAmount = staminaComponent.GetMaxStamina() / StaminaComponent.DEFAULT_MAX_STAMINA;
        usableStaminaImage.fillAmount = usableStamina / StaminaComponent.DEFAULT_MAX_STAMINA;
        grayStaminaImage.fillAmount = usableStamina + staminaComponent.GetGrayStamina() / StaminaComponent.DEFAULT_MAX_STAMINA;
        if (grayStaminaImage.fillAmount > maxStaminaImage.fillAmount) { grayStaminaImage.fillAmount = maxStaminaImage.fillAmount; }
        if (staminaComponent.HasForesight()) usableStaminaImage.color = foresightStamina;
        else usableStaminaImage.color = staminaComponent.InDangerZone() ? dangerStamina : healthyStamina;

    }
}