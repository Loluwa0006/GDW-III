using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaController : MonoBehaviour
{
    float maxStamina = 100f;
    public float CurrentStamina = 100f;

    public Image StaminaBar;
    public InGameUI InGameUIController;
    public GameObject GameOverUI;

    //Start is called before the first frame update
    public void Start()
    {
        CurrentStamina = maxStamina;
        StaminaBar = GetComponent<Image>();
        StaminaBar.fillAmount = maxStamina;
    }

    //Update is called once per frame
    void Update()
    {
        if (CurrentStamina <= 0)
        {
            GameOver();
        }

        if (InGameUIController.SuddenDeathTextIsPlayed == true)
        {
            CurrentStamina -= Time.deltaTime * 2;
        }
        StaminaBar.fillAmount = CurrentStamina / maxStamina;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        InGameUIController.GameUI.SetActive(false);
        GameOverUI.SetActive(true);
    }
}
