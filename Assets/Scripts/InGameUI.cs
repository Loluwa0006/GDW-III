using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : MonoBehaviour
{
    public TextMeshProUGUI TimerText;
    public GameObject SuddenDeathTextGameObject;
    public GameObject GameUI;
    private float StartTime = 10f;
    public float currentTime;

    public bool SuddenDeathTextIsPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = StartTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            if (SuddenDeathTextIsPlayed == false)
            {
                SuddenDeathTextGameObject.SetActive(true);
                SuddenDeathTextIsPlayed = true;
                StartCoroutine(SuddenDeathTextPlaying());
            }
            currentTime = 0;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        TimerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }

    public IEnumerator SuddenDeathTextPlaying()
    {
        yield return new WaitForSeconds(2);
        SuddenDeathTextGameObject.SetActive(false);
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool GetHasSuddenDeathTextPlayed()
    {
        return SuddenDeathTextIsPlayed;
    }
}
