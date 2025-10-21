using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    const float SUDDEN_DEATH_SLOW_DOWN_DURATION = 2.5f;
    const float SUDDEN_DEATH_SLOW_DOWN_AMOUNT = 0.1f;
    public const float TWEEN_TO_REGULAR_SPEED_DURATION = 0.2f;
    const int DEFAULT_MATCH_LENGTH = 10;

    [HideInInspector] public static bool gamePaused = false;
    [HideInInspector] public static bool inSpecialStop = false; //hitstop, parrystop etc
    public PlayerInputManager inputManager;
    public List<BaseEcho> echoList = new();
    public PostProcessingManager postProcessingManager;
    public HUDAnimator HUDAnimator;

    [Header("Player Prefabs")]
    [SerializeField] protected BaseSpeaker speakerPrefab;

    [Header("UI Objects")]

    [SerializeField] protected StaminaUI healthUIPrefab;
    [SerializeField] protected GameObject UIHolder;
    [SerializeField] protected List<GameObject> spawnPositions = new();
    [SerializeField] protected TMP_Text timerDisplay;
    [SerializeField] protected GameObject winScreen;
    [SerializeField] protected TMP_Text winText;
    [SerializeField] protected CinemachineTargetGroup targetGroup;
    [SerializeField] protected Animator mapAnimator;

    [Header("Match Info")]
    [SerializeField] protected MatchData matchData;

    HashSet<BaseSpeaker> speakerList = new();
    HashSet<BaseSpeaker> activeSpeakers = new();
      
    Dictionary<BaseSpeaker, StaminaUI> characterUI = new();
    protected Queue<MatchData.PlayerInfo> queuedPlayerInfo = new();

    float timerTracker;

    bool inSuddenDeath = false;

    static int stopFrames = 0;


    private void Start()
    {
        if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
        MatchDataHolder holder = FindAnyObjectByType<MatchDataHolder>();
        if (holder != null)
        {
            matchData = holder.GetMatchData();
        }

        InitUI();
        InitTimer();
        InitPlayers();

        foreach (var ball in echoList)
        {
            ball.InitBall(speakerList);
        }
    }

    protected virtual void InitUI()
    {
        foreach (Transform t in UIHolder.transform)
        {
            Destroy(t.gameObject);
        }
        winScreen.SetActive(false);
    }
    protected virtual void InitTimer()
    {
        timerDisplay.gameObject.SetActive(true);
        timerTracker = matchData.gameLength;
    }

    protected virtual void InitPlayers()
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
                    queuedPlayerInfo.Enqueue(member);
                    inputManager.JoinPlayer(pairWithDevice: member.device);
                }
            }
        }
    }


    public virtual void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!playerInput.gameObject.TryGetComponent(out BaseSpeaker character)) { return; }
        if (speakerList.Contains(character)) { return; }
        int index = playerInput.playerIndex + 1;
        if (queuedPlayerInfo.Count > 0)
        {
            character.InitPlayer(queuedPlayerInfo.Dequeue(), index);
        }
        else
        {
            Debug.LogWarning("No queued data for char " + character.name + ", using base speaker KB 1 controls");
        }
      StartCoroutine(InitCharacterSignals(character));
        AddStaminaUIForCharacter(character, index);
        AddCharacterToCameraTargetGroup(character.transform);
        StartCoroutine(SetCharacterPosition(character));
        speakerList.Add(character);
        activeSpeakers.Add(character);

    }
    protected void AddStaminaUIForCharacter(BaseSpeaker character, int index)
    {
        StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        newUI.InitStaminaDisplay(character, index);
        characterUI[character] = newUI;
    }

    protected virtual IEnumerator InitCharacterSignals(BaseSpeaker character)
    {
        yield return null;
        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);

        character.healthComponent.entityDamaged.AddListener(postProcessingManager.OnSpeakerStruck);

        character.healthComponent.entityDamaged.AddListener(HUDAnimator.OnSpeakerStruck);

        character.deflectManager.deflectedBall.AddListener(HUDAnimator.OnEchoDeflected);
    }


    public void AddCharacter(BaseSpeaker character)
    {
      //
    }

    protected virtual IEnumerator SetCharacterPosition(BaseSpeaker character)
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
           characterUI[character].gameObject.SetActive(false);
        }
        targetGroup.RemoveMember(character.transform);
        character.DeactivatePlayer();
        activeSpeakers.Remove(character);
    }
    protected virtual void OnCharacterDefeated(DamageInfo info, HealthComponent victim)
    {
        if (!victim.hurtboxOwner.TryGetComponent(out BaseSpeaker defeated))
        {
            Debug.Log("Couldn't find base char component");
            return;
        }
        RemoveCharacter(defeated);
        Debug.Log(defeated.name + " has been defeated, " + speakerList.Count + " characters remain");
        if (activeSpeakers.Count == 1)
        {
            OnCharacterVictorious();
        }
    }

   protected void OnCharacterVictorious()
    {
        BaseSpeaker winner = activeSpeakers.ElementAt(0);
        winScreen.SetActive(true);
        winText.text = winner.name + " Wins";
        Time.timeScale = 0.0f;
    }

    private void Update()
    {
        TimerLogic();
    }

    protected virtual void TimerLogic()
    {
        timerTracker -= Time.deltaTime;
        if (timerTracker <= 0.0f)
        {
            if (!inSuddenDeath)
            {
                inSuddenDeath = true;
                StartCoroutine(EnterSuddenDeath());
            }
        }
        else
        {
            timerTracker = Mathf.Clamp(timerTracker, 0.0f, matchData.gameLength);
            timerDisplay.text = Mathf.RoundToInt(timerTracker).ToString();
        }
    }

    private void FixedUpdate()
    {
        if (inSpecialStop)
        {
            stopFrames -= 1;
            if (stopFrames <= 0)
            {
                stopFrames = 0;
                inSpecialStop = false;
            }
        }
    }

    protected IEnumerator EnterSuddenDeath()
    {
        Debug.Log("Entering sudden death");
        foreach (var cha in speakerList)
        {
            cha.staminaComponent.EnterSuddenDeath();
        }
        foreach (var ball in echoList)
        {
            ball.EnterSuddenDeath();
        }

        postProcessingManager.OnSuddenDeathStarted();

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

    public void ResetGame()
    {
        if (matchData != null) {
            timerTracker = matchData.gameLength;
        }
        else
        {
            timerTracker = DEFAULT_MATCH_LENGTH;
        }
        Time.timeScale = 1.0f;
        inSpecialStop = false;
        stopFrames = 0;
        foreach (var cha in speakerList)
        {
            ResetPlayer(cha);

            if (mapAnimator)
            {
                mapAnimator.Play("Reset", 0, 0.0f);
            }
        }
        foreach (var ball in echoList)
        {
            ball.InitBall(activeSpeakers);
        }
        winScreen.SetActive(false);

        postProcessingManager.ResetManager();
    }

    void ResetPlayer(BaseSpeaker cha)
    {
        cha.enabled = true;
        cha.ActivatePlayer();
        AddCharacterToCameraTargetGroup(cha.transform);


        cha.staminaComponent.ResetComponent(true);
        cha.healthComponent.ResetComponent();
        cha.velocityManager.ResetComponent();


        StartCoroutine(SetCharacterPosition(cha));
        activeSpeakers.Add(cha);
        characterUI[cha].gameObject.SetActive(true);
    }
    void AddCharacterToCameraTargetGroup(Transform chaTransform)
    {
        targetGroup.AddMember(chaTransform, 1.0f, 5.0f);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneRegistry.MainMenu_Test.ToString());
    }

    

}
