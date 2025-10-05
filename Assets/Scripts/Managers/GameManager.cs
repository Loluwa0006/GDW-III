using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    const int MATCH_DURATION = 60;
    const float SUDDEN_DEATH_SLOW_DOWN_DURATION = 2.5f;
    const float SUDDEN_DEATH_SLOW_DOWN_AMOUNT = 0.1f;
    public const float TWEEN_TO_REGULAR_SPEED_DURATION = 0.2f;
    public const float HITSTOP_SPEED = 0.0f;

    [HideInInspector] public static bool gamePaused = false;
    [HideInInspector] public static bool inSpecialStop = false; //hitstop, parrystop etc
    public List<RicochetBall> echoList = new();


    [SerializeField] StaminaUI healthUIPrefab;
    [SerializeField] GameObject UIHolder;
    [SerializeField] List<GameObject> spawnPositions = new();
    [SerializeField] TMP_Text timerDisplay;
    [SerializeField] GameObject winScreen;
    [SerializeField] TMP_Text winText;
    List<BaseCharacter> characterList = new();
      
    Dictionary<BaseCharacter, StaminaUI> characterUI = new();

    float matchTracker = MATCH_DURATION;

    bool inSuddenDeath = false;

    bool useTimer = false;
    static int stopFrames = 0;


    private void Start()
    {
        foreach (Transform t in UIHolder.transform)
        {
            Destroy(t.gameObject);
        }
        characterList = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None).ToList();
        int index = 0;
        foreach (var character in characterList)
        {
            StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
            newUI.InitStaminaDisplay(character, index);
            character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);
            index++;
        }
        winScreen.gameObject.SetActive(false);
        timerDisplay.gameObject.SetActive(false);
        
    }

    public void AddCharacter(BaseCharacter character)
    {
        if (characterUI.ContainsKey(character)) { return; }
        characterList.Add(character);
        StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        characterUI[character] = newUI;
        newUI.InitStaminaDisplay(character, characterList.Count);
        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);

        StartCoroutine(SetCharacterPosition(character));
        if (characterList.Count == 2)
        {
            timerDisplay.gameObject.SetActive(true);
            useTimer = true;
            matchTracker = MATCH_DURATION;
            
        }
    }

    IEnumerator SetCharacterPosition(BaseCharacter character)
    {
        int playerIndex = characterUI.Count;
        int spawnIndex = (playerIndex - 1) % spawnPositions.Count;
        yield return new WaitForFixedUpdate();
        character.transform.position = spawnPositions[spawnIndex].transform.position;
    }

    public void RemoveCharacter(BaseCharacter character)
    {
        if (characterUI.ContainsKey(character))
        {
            Destroy(characterUI[character].gameObject);
        }
    }
    void OnCharacterDefeated(DamageInfo info, HealthComponent victim)
    {
        if (!victim.hurtboxOwner.TryGetComponent(out BaseCharacter defeated))
        {
            Debug.Log("Couldn't find base char component");
            return;
        }
        characterList.Remove(defeated);
        Debug.Log(defeated.name + " has been defeated, " + characterList.Count + " characters remain");
        if (characterList.Count == 1)
        {
            OnCharacterVictorious();
        }
    }

    void OnCharacterVictorious()
    {
        BaseCharacter winner = characterList[0];

        Debug.Log("Character " + winner.name + " wins!");
        winScreen.gameObject.SetActive(true);
        winText.text = winner.name + " Wins";
        Time.timeScale = 0.0f;
    }

    private void Update()
    {
        if (useTimer)
        {
            matchTracker -= Time.deltaTime;
            if (matchTracker < 0.0f )
            {
                if (!inSuddenDeath)
                {
                    inSuddenDeath = true;
                    StartCoroutine(EnterSuddenDeath());
                }
            }
            else
            {
                matchTracker = Mathf.Clamp(matchTracker, 0.0f, MATCH_DURATION);
                timerDisplay.text = Mathf.RoundToInt(matchTracker).ToString();
            }
        }
    }

    IEnumerator EnterSuddenDeath()
    {
        Debug.Log("Entering sudden death");
        foreach (var cha in characterList)
        {
            cha.staminaComponent.EnterSuddenDeath();
        }
        foreach (var ball in echoList)
        {
            ball.EnterSuddenDeath();
        }
        Time.timeScale = SUDDEN_DEATH_SLOW_DOWN_AMOUNT;
        timerDisplay.text = "SUDDEN DEATH";
        yield return new WaitForSeconds(SUDDEN_DEATH_SLOW_DOWN_DURATION * SUDDEN_DEATH_SLOW_DOWN_AMOUNT);

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, TWEEN_TO_REGULAR_SPEED_DURATION) // 1.5s back to normal
            .SetEase(Ease.OutQuad);
    }

    public static void ApplyHitstop(int frames)
    {
        if (gamePaused || frames <= 0) { return; }
        inSpecialStop = true;
        stopFrames = frames;

    }

    private void FixedUpdate()
    {
        if (stopFrames < 0)
        {
            stopFrames = 0;
            inSpecialStop = false;
        }
        stopFrames -= 1;
    }
}
