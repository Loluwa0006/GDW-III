using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialManager : GameManager
{

    [System.Serializable]
    public enum SectionName
    {
        Introduction,

        //Speaker 
        Speaker_Movement,
        Deflect,
        Select_Your_Path,
        Dash,
        Counterslash,
        Afterimage, 
        Grapple,
        SpeakerWinCondition,
        SpeakerFinalBattle,

        //Echo 
        EchoMovement,
        Lighten,
        Crash, 
        FlameWheel,
        Translucent,
        EchoWinCondition,
        EchoFinalBattle,


        //Generic
        Complete
        
    }
    [System.Serializable]
    public class TutorialSection
    {
      [HideInInspector]  public int tutorialPoints = 0;
        public SectionName sectionName;
        public int pointsToContinue = 3;
        public SectionName nextSection = SectionName.Speaker_Movement;
        public SectionName prevSection;
        public bool failOnDeath = false;
        public List<SectionAddon> sectionAddons = new();
        public Vector3 spawnPos;
    }

    public class SectionAddon
    {
        [SerializeField] protected TutorialSection section;
        [SerializeField] protected TutorialManager manager;
        [HideInInspector] public bool sectionCompletable = true; 
        public virtual void OnSectionStarted()
        {

        }

        public virtual void SectionLogic()
        {

        }

        public virtual void SectionPhysicsLogic()
        {

        }

        public virtual void OnSectionEnded()
        {

        }

        public virtual void OnSectionRestarted()
        {

        }
    }

    public class TimerAddon : SectionAddon
    {
        [SerializeField] float timerDuration = 10.0f;
        float timerTracker = 0.0f;
        public override void SectionLogic()
        {
            timerTracker += Time.deltaTime;
            if (timerTracker > timerDuration)
            {
                sectionCompletable = false; //you failed 
            }
        }

        public override void OnSectionRestarted()
        {
            timerTracker = 0.0f;
            sectionCompletable = true;
        }

        public override void OnSectionStarted()
        {
            manager.timerDisplay.gameObject.SetActive(true);
        }

        public override void OnSectionEnded()
        {
            manager.timerDisplay.gameObject.SetActive(false);
        }
    }

    public UnityEvent<TutorialSection> sectionRestarted = new();
    public UnityEvent<TutorialSection> sectionEnded = new();
    public UnityEvent<TutorialSection> sectionStarted = new();
    public DialogueManager dialogueManager;

    [SerializeField] List<TutorialSection> tutorialSections = new();
    [SerializeField] Transform respawnPoint;
    [SerializeField] TMP_Text sectionDisplay;


    TutorialSection currentSection;
    Dictionary<SectionName, TutorialSection> sectionDict = new();

    

    protected override void InitManager()
    {
        base.InitManager();

        if (tutorialSections.Count <= 0) { return; }

        currentSection = tutorialSections[0];

        foreach (var section in tutorialSections)
        {
            sectionDict[section.sectionName] = section;
        }


        InitPlayers();
        StartTutorial();
    }
    protected override void InitPlayers()
    {
        InputDevice inputDevice = Gamepad.all.Count > 0 ? Gamepad.all[0] : Keyboard.current;


        MatchData.PlayerInfo tutorialPlayer = new MatchData.PlayerInfo()
        {
            device = inputDevice,
            keyboardPlayerTwo = false,
            playerType = MatchData.PlayerType.Speaker,
            skillOne = MatchData.SkillName.None,
            skillTwo = MatchData.SkillName.None,
        };
       queuedPlayerInfo.Enqueue(tutorialPlayer);
       inputManager.JoinPlayer(pairWithDevice: inputDevice);
    }

    public override void OnPlayerJoined(PlayerInput playerInput)
    {
        base.OnPlayerJoined(playerInput);
        if (dialogueManager != null)
        {
            dialogueManager.SetPlayerInput(playerInput);
        }
    }
    protected override IEnumerator SetCharacterPosition(BaseSpeaker character)
    {
        yield return new WaitForFixedUpdate();
        character.transform.position = respawnPoint.position;
    }
    public void GainTutorialPoint()
    {
        currentSection.tutorialPoints++;
        if (currentSection.tutorialPoints >= currentSection.pointsToContinue)
        {
            EndSection();
            if (currentSection.nextSection != SectionName.Complete)
            {
               StartSection(currentSection.nextSection);
            }
            else
            {
                OnTutorialCompleted();
            }
        }
    }

    void StartTutorial()
    {
        currentSection = sectionDict[SectionName.Introduction];
        foreach (var addon in currentSection.sectionAddons)
        {
            addon.OnSectionStarted();
        }
        respawnPoint.position = currentSection.spawnPos;
        sectionDisplay.text = currentSection.sectionName.ToString();
        sectionStarted.Invoke(currentSection);
    }
   public void StartSection(SectionName newSection)
    {
        currentSection = sectionDict[newSection];
        currentSection.tutorialPoints = 0;
        foreach (var addon in currentSection.sectionAddons)
        {
            addon.OnSectionStarted();
        }
        respawnPoint.position = currentSection.spawnPos;
        string sectionName = currentSection.sectionName.ToString();
        sectionName = sectionName.Replace("_", " ");
        sectionDisplay.text = sectionName;
        sectionStarted.Invoke(currentSection);
    }

    void EndSection()
    {
        sectionEnded.Invoke(currentSection);
        currentSection.tutorialPoints = 0;
        foreach (var addon in currentSection.sectionAddons)
        {
            addon.OnSectionEnded();
        }
    }

public void RedoSection()
    {
        foreach (var addon in currentSection.sectionAddons)
        {
            addon.OnSectionRestarted();
        }
        currentSection.tutorialPoints = 0;
        sectionRestarted.Invoke(currentSection);
    }

    protected override void OnCharacterDefeated(DamageInfo info, HealthComponent victim)
    {
        if (!victim.hurtboxOwner.TryGetComponent(out BaseSpeaker defeated))
        {
            Debug.Log("Couldn't find base char component");
            return;
        }
        defeated.transform.position = respawnPoint.position;
        defeated.staminaComponent.ResetComponent(false);
        if (currentSection.failOnDeath)
        {
            RedoSection();
        }
    }

    private void OnTutorialCompleted()
    {
        winScreen.SetActive(true);
        winText.text = "Tutorial Complete";
    }

    private void Update()
    {
        foreach (var addon in currentSection.sectionAddons)
        {
            addon.SectionLogic();
            if (!addon.sectionCompletable)
            {
                RedoSection();
                break;
            }
        }
    }
}