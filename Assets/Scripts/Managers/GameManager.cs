using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    const float SUDDEN_DEATH_SLOW_DOWN_DURATION = 2.5f;
    const float SUDDEN_DEATH_SLOW_DOWN_AMOUNT = 0.1f;
    public const float TWEEN_TO_REGULAR_SPEED_DURATION = 0.2f;

    [HideInInspector] public static bool gamePaused = false;
    [HideInInspector] public static bool inSpecialStop = false; //hitstop, parrystop etc
    public PlayerInputManager inputManager;
    public List<RicochetBall> echoList = new();

    [Header("Player Prefabs")]
    [SerializeField] BaseSpeaker speakerPrefab;

    [Header("UI Objects")]

    [SerializeField] StaminaUI healthUIPrefab;
    [SerializeField] GameObject UIHolder;
    [SerializeField] List<GameObject> spawnPositions = new();
    [SerializeField] TMP_Text timerDisplay;
    [SerializeField] GameObject winScreen;
    [SerializeField] TMP_Text winText;
    [SerializeField] CinemachineTargetGroup targetGroup;

    [Header("Match Info")]
    [SerializeField] MatchData matchData;

    HashSet<BaseSpeaker> characterList = new();
      
    Dictionary<BaseSpeaker, StaminaUI> characterUI = new();
    Queue<MatchData.PlayerInfo> queuedPlayerInfo = new();

    float matchTracker;

    bool inSuddenDeath = false;

    static int stopFrames = 0;


    private void Start()
    {
        if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
        MatchDataHolder holder = FindAnyObjectByType<MatchDataHolder>();
        if (holder != null)
        {
            matchData = FindFirstObjectByType<MatchDataHolder>().GetMatchData();
        }
        foreach (Transform t in UIHolder.transform)
        {
            Destroy(t.gameObject);
        }
        InitPlayers();
        winScreen.SetActive(false);
        timerDisplay.gameObject.SetActive(true);
        matchTracker = matchData.gameLength;

        foreach (var ball in echoList)
        {
            ball.InitBall(characterList);
        }
    }

    void InitPlayers()
    {
        if (matchData == null) { return; }
        int index = 0;
        foreach (MatchData.TeamInfo team in matchData.gameTeams)
        {
            foreach (MatchData.PlayerInfo member in team.teamMembers)
            {
                index++;
                if (member.playerType == MatchData.PlayerType.Speaker)
                {

                    string memberControlScheme = member.keyboardPlayerTwo ? "CombatKeyboardTwo" : "Combat";

                    queuedPlayerInfo.Enqueue(member);

                    //BaseSpeaker speaker = Instantiate(speakerPrefab); 
                    inputManager.JoinPlayer(pairWithDevice: member.device);

                    
                }
            }
        }
    }


    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!playerInput.gameObject.TryGetComponent(out BaseSpeaker character)) { return; }
        if (queuedPlayerInfo.Count <= 0) { Debug.Log("Need queued info to create player."); return; }
        int index = playerInput.playerIndex + 1;
        MatchData.PlayerInfo member = queuedPlayerInfo.Dequeue();
        character.InitPlayer(member, index);
        InitCharacterSignals(character);
        AddStaminaUIForCharacter(character, index);
        targetGroup.AddMember(character.transform, 1.0f, 5.0f);
        StartCoroutine(SetCharacterPosition(character));
        characterList.Add(character);
    }
    void AddStaminaUIForCharacter(BaseSpeaker character, int index)
    {
        StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        newUI.InitStaminaDisplay(character, index);
        characterUI[character] = newUI;
    }

    void InitCharacterSignals(BaseSpeaker character)
    {
        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);
    }


    public void AddCharacter(BaseSpeaker character)
    {
        if (characterUI.ContainsKey(character)) { return; }
        characterList.Add(character);


        StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        newUI.InitStaminaDisplay(character, characterList.Count);
        characterUI[character] = newUI;

        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);

        StartCoroutine(SetCharacterPosition(character));

        
        if (characterList.Count == 2)
        {
            timerDisplay.gameObject.SetActive(true);
            matchTracker = matchData.gameLength;
        }
    }

    IEnumerator SetCharacterPosition(BaseSpeaker character)
    {
        int playerIndex = characterUI.Count;
        int spawnIndex = (character.teamIndex - 1) % spawnPositions.Count;
        yield return new WaitForFixedUpdate();
        character.transform.position = spawnPositions[spawnIndex].transform.position;
    }

    public void RemoveCharacter(BaseSpeaker character)
    {
        if (characterUI.ContainsKey(character))
        {
            Destroy(characterUI[character].gameObject);
        }
    }
    void OnCharacterDefeated(DamageInfo info, HealthComponent victim)
    {
        if (!victim.hurtboxOwner.TryGetComponent(out BaseSpeaker defeated))
        {
            Debug.Log("Couldn't find base char component");
            return;
        }
        characterList.Remove(defeated);
        targetGroup.RemoveMember(defeated.transform);
        Debug.Log(defeated.name + " has been defeated, " + characterList.Count + " characters remain");
        if (characterList.Count == 1)
        {
            OnCharacterVictorious();
        }
    }

    void OnCharacterVictorious()
    {
        BaseSpeaker winner = characterList.ElementAt(0);

        Debug.Log("Character " + winner.name + " wins!");
        winScreen.gameObject.SetActive(true);
        winText.text = winner.name + " Wins";
        Time.timeScale = 0.0f;
    }

    private void Update()
    {
        matchTracker -= Time.deltaTime;
        if (matchTracker <= 0.0f )
        {
            if (!inSuddenDeath)
            {
                inSuddenDeath = true;
                StartCoroutine(EnterSuddenDeath());
            }
        }
        else
        {
            matchTracker = Mathf.Clamp(matchTracker, 0.0f, matchData.gameLength);
            timerDisplay.text = Mathf.RoundToInt(matchTracker).ToString();
        }
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

   
}
