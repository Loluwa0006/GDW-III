using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
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

    [SerializeField] RawImage skillOneIcon;
    [SerializeField] RawImage skillTwoIcon;


    private void Awake()
    {
        if (staminaDisplay == null)
        {
            staminaDisplay = GetComponent<TMP_Text>();
        }
    }

    public void InitDisplay(BaseSpeaker cha, MatchData.PlayerInfo info)
    {
        staminaComponent = cha.staminaComponent;
        UIBackdrop.color = UIColors[cha.teamIndex - 1];
        if (info != null)  SetSkillIcons(info.skillOne, info.skillTwo);
        cha.characterStateMachine.updatedSkills.AddListener(SetSkillIcons);
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
        grayStaminaImage.fillAmount = (usableStamina + staminaComponent.GetGrayStamina()) / StaminaComponent.DEFAULT_MAX_STAMINA;
        if (grayStaminaImage.fillAmount > maxStaminaImage.fillAmount) { grayStaminaImage.fillAmount = maxStaminaImage.fillAmount; }
        if (staminaComponent.HasForesight()) usableStaminaImage.color = foresightStamina;
        else usableStaminaImage.color = staminaComponent.InDangerZone() ? dangerStamina : healthyStamina;
     }

    public void SetSkillIcons(MatchData.SkillName skillOne, MatchData.SkillName skillTwo)
    {
        if (skillOne != MatchData.SkillName.None)
        {
            skillOneIcon.gameObject.SetActive(true);
            skillOneIcon.texture = MatchData.instance.skillIconDictionary[skillOne];
        }
        else skillOneIcon.gameObject.SetActive(false);
        if (skillTwo != MatchData.SkillName.None)
        {
            skillTwoIcon.gameObject.SetActive(true);
            skillTwoIcon.texture = MatchData.instance.skillIconDictionary[skillTwo];
        }
        else skillTwoIcon.gameObject.SetActive(false);


    }
}