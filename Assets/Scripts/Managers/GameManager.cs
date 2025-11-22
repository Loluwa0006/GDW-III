using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static ReportManager;

public class GameManager : MonoBehaviour
{

    const float SUDDEN_DEATH_SLOW_DOWN_DURATION = 2.5f;
    const float SUDDEN_DEATH_SLOW_DOWN_AMOUNT = 0.1f;
    public const float TWEEN_TO_REGULAR_SPEED_DURATION = 0.35f;
    const int DEFAULT_MATCH_LENGTH = 60;

    [HideInInspector] public static bool gamePaused = false;
    [HideInInspector] public static bool inSpecialStop = false; //hitstop, parrystop etc
    public PlayerInputManager inputManager;
    public List<BaseEcho> echoList = new();
    public AudioSource bgmPlayer;
    [Header("Managers")]
    public PostProcessingManager postProcessingManager;
    public HUDAnimator HUDAnimator;
    public CameraManager camManager;
    public AnnouncementManager announcementManager;
    public ReportManager reportManager;

    [Header("Player Prefabs")]
    [SerializeField] protected BaseSpeaker speakerPrefab;

    [Header("UI Objects")]

    [SerializeField] protected StaminaUI healthUIPrefab;
    [SerializeField] protected GameObject UIHolder;
    [SerializeField] protected List<GameObject> spawnPositions = new();
    [SerializeField] protected TMP_Text timerDisplay;
    [SerializeField] protected GameObject winScreen;
    [SerializeField] protected TMP_Text winText;
    [SerializeField] protected TMP_Text scoreText;
    [SerializeField] protected CinemachineTargetGroup targetGroup;
    [SerializeField] protected Animator mapAnimator;

    [Header("Match Info")]
    [SerializeField] protected MatchData matchData;

    public HashSet<BaseSpeaker> speakerList = new();
    HashSet<BaseSpeaker> activeSpeakers = new();
      
    Dictionary<BaseSpeaker, StaminaUI> characterUI = new();
    protected Queue<MatchData.PlayerInfo> queuedPlayerInfo = new();

    struct ScoreTracker
    {
        public int teamOneWins;
        public int teamTwoWins;
    }

    ScoreTracker scoreTracker;
    float timerTracker;

    bool inSuddenDeath = false;

    static int stopFrames = 0;


    bool gameStarted = false;


    List<TrackerData> trackerData = new();
    private void Start()
    {
        InitManager();
    }

    protected virtual void InitManager()
    {
        if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
        MatchDataHolder holder = FindAnyObjectByType<MatchDataHolder>();
        if (holder != null)
        {
            matchData = holder.GetMatchData();
        }
        if (announcementManager == null)
        {
            announcementManager = FindFirstObjectByType<AnnouncementManager>();
        }
        InitUI();
        InitTimer();
        InitPlayers();
        InitEchoes();
        StartCoroutine(StartCountdownAnnouncement());
    }

    protected virtual void InitEchoes()
    {
        foreach (var ball in echoList)
        {
            ball.InitProjectile(speakerList);
        }
    }

    protected virtual IEnumerator StartCountdownAnnouncement()
    {
        yield return new WaitForFixedUpdate();
        if (camManager != null) camManager.cinemachineCam.CancelDamping(true); // make sure cam is in right spot before starting
        AnnouncementData countdownDataOne = new()
        {
            announcementDuration = 1.0f,
            announcementText = "3",
            customTimescale = 0.0f,
            priority = 5
        };
        AnnouncementData countdownDataTwo = new(countdownDataOne);
        AnnouncementData countdownDataThree = new(countdownDataTwo);
        AnnouncementData countdownDataFour = new(countdownDataThree);
        countdownDataTwo.announcementText = "2";
        countdownDataThree.announcementText = "1";
        countdownDataFour.announcementText = "BEGIN";
        countdownDataFour.customTimescale = 1.0f;
        announcementManager.QueueNewAnnouncement(countdownDataOne, countdownDataTwo, countdownDataThree, countdownDataFour);
        yield return new WaitUntil(() => announcementManager.annoucementPlaying);
        yield return new WaitUntil(() => !announcementManager.annoucementPlaying);
        if (reportManager != null)
        { 
            reportManager.OnMatchStart();
        }
        gameStarted = true;

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
        Debug.Log("match length is " + matchData.gameLength);
        timerTracker = matchData.gameLength;
    }

    protected virtual void InitPlayers()
    {
        if (matchData == null) { return; }
        int memberIndex = 0;
        int teamIndex = 0;
        List<ReportManager.TrackerData> speakerData = new();
        foreach (MatchData.TeamInfo team in matchData.gameTeams)
        {
            teamIndex++;
            foreach (MatchData.PlayerInfo member in team.teamMembers)
            {
                member.teamIndex = teamIndex;
                memberIndex++;
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
            var info = queuedPlayerInfo.Dequeue();
            character.InitPlayer(info, index);
            trackerData.Add(new TrackerData()
            {
                speaker = character,
                speakerInfo = info,
            });


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

        if (queuedPlayerInfo.Count == 0 && reportManager != null)
        {
            reportManager.InitManager(trackerData.ToArray());
        }




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


        if (postProcessingManager != null)
        {
            character.healthComponent.entityDamaged.AddListener(postProcessingManager.OnSpeakerStruck);
            character.deflectManager.superDeflectPerformed.AddListener(postProcessingManager.OnSuperDeflectPerformed);
        }

        if (HUDAnimator != null)
        {
            character.healthComponent.entityDamaged.AddListener(HUDAnimator.OnSpeakerStruck);
            character.deflectManager.deflectedBall.AddListener(HUDAnimator.OnEchoDeflected);
        }
        if (camManager != null)
        {
            character.healthComponent.entityDamaged.AddListener((info) => camManager.OnSpeakerStruck(character, info));
        }
        if (reportManager != null)
        {
            character.deflectManager.deflectPerformed.AddListener(reportManager.OnSpeakerDeflect);
            character.staminaComponent.foresightPerformed.AddListener(reportManager.OnForesightUsed);
        }

    }


    public void AddCharacter(BaseSpeaker character)
    {
      
      
    }

    protected virtual IEnumerator SetCharacterPosition(BaseSpeaker character)
    {
        int spawnIndex = (character.teamIndex - 1) % spawnPositions.Count;
        yield return new WaitForFixedUpdate();
        character.transform.position = spawnPositions[spawnIndex].transform.position;
        if (camManager != null) camManager.cinemachineCam.CancelDamping(true);
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
        Debug.Log(defeated.name + " has been defeated, " + activeSpeakers.Count + " characters remain");
        if (activeSpeakers.Count == 1)
        {
            StartCoroutine(OnCharacterVictorious());
        }
    }

   protected IEnumerator OnCharacterVictorious()
    {
        if (reportManager != null)
        {
            reportManager.OnMatchEnd();
        }
        BaseSpeaker winner = activeSpeakers.ElementAt(0);
        winText.text = winner.name + " Wins";
        if (scoreText != null)
        {
            UpdateScoreText(winner);
        }
        bgmPlayer.Stop();
        AnnouncementData winAnnouncement = new()
        {
            announcementDuration = 2.0f,
            announcementText = "VERDICT",
            customTimescale = 0.1f,
            priority = 9999999
        };
        announcementManager.QueueNewAnnouncement(winAnnouncement);
        yield return new WaitUntil(() => announcementManager.annoucementPlaying);
        yield return new WaitUntil(() => !announcementManager.annoucementPlaying);
        winScreen.SetActive(true);
        Time.timeScale = 0.0f;


    }

    void UpdateScoreText(BaseSpeaker winner)
    {
        if (winner.teamIndex == 1)
        {
            scoreTracker.teamOneWins += 1;
        }
        else
        {
            scoreTracker.teamTwoWins += 1;
        }

        scoreText.text = scoreTracker.teamOneWins + "/" + scoreTracker.teamTwoWins;

    }

    private void Update()
    {
        if (gameStarted) TimerLogic();
    }

    protected virtual void TimerLogic()
    {
       
            timerTracker -= Time.deltaTime;
            if (timerTracker <= 0.0f)
            {
                if (!inSuddenDeath)
                {
                    inSuddenDeath = true;
                    EnterSuddenDeath();
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

    protected void EnterSuddenDeath()
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

        AnnouncementData suddenDeathAnnouncement = new ()
        {
            announcementDuration = SUDDEN_DEATH_SLOW_DOWN_DURATION,
            announcementText = "SUDDEN DEATH",
            customTimescale = SUDDEN_DEATH_SLOW_DOWN_AMOUNT,
            priority = 999
        };
        announcementManager.QueueNewAnnouncement(suddenDeathAnnouncement);

        inSuddenDeath = true;

        
    }

    public static void ApplyHitstop(int frames)
    {
        if (gamePaused || frames <= 0) { return; }
        inSpecialStop = true;
        stopFrames =  Mathf.Max(stopFrames, frames);
        Debug.Log("stopping game for " + frames + " frames");

    }

    public virtual void ResetGame()
    {

        gameStarted = false;
        bgmPlayer.time = 0;
        bgmPlayer.Play();
        inSuddenDeath = false;

        postProcessingManager.ResetManager();

        if (matchData != null)
        {
            timerTracker = matchData.gameLength;
        }
        else
        {
            timerTracker = DEFAULT_MATCH_LENGTH;
        }
        timerDisplay.text = Mathf.RoundToInt(timerTracker).ToString();
        inSpecialStop = false;
        stopFrames = 0;
        foreach (var cha in speakerList)
        {
            ResetPlayer(cha);
        }
        foreach (var ball in echoList)
        {
            ball.InitProjectile(speakerList);
            
        }
        if (mapAnimator)
        {
            mapAnimator.Play("Reset", 0, 0.0f);
        }
     
        winScreen.SetActive(false);

        Time.timeScale = 1.0f;
        StartCoroutine(StartCountdownAnnouncement());

        if (reportManager != null)
        {
            reportManager.OnMatchStart();
        }
    }

    void ResetPlayer(BaseSpeaker cha)
    {
        cha.enabled = true;
        cha.ActivatePlayer();
        AddCharacterToCameraTargetGroup(cha.transform);


        cha.staminaComponent.ResetComponent(true);
        cha.healthComponent.ResetComponent();
        cha.velocityManager.ResetComponent();
        cha.characterStateMachine.ResetComponent();
        cha.deflectManager.ResetComponent();

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
