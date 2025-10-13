using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class TutorialManager : GameManager
{

    [System.Serializable]
    public enum SectionName
    {
        Introduction,

        //Speaker 
        SpeakerMovement,
        Deflect,
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
        public SectionName nextSection = SectionName.SpeakerMovement;
        public SectionName prevSection;
        public bool failOnDeath = false;
        public List<SectionAddon> sectionAddons = new();
        public Vector3 spawnPos;
    }

    public class SectionAddon
    {
        [SerializeField] protected TutorialSection section;

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
    }

    public UnityEvent<TutorialSection> sectionRestarted = new();
    public DialogueManager dialogueManager;

    [SerializeField] List<TutorialSection> tutorialSections = new();
    [SerializeField] Transform respawnPoint;


    TutorialSection currentSection;
    Dictionary<SectionName, TutorialSection> sectionDict = new();

    private void Awake()
    {
        if (tutorialSections.Count <= 0) { return; }

        currentSection = tutorialSections[0];

        foreach (var section in tutorialSections)
        {
            sectionDict[section.sectionName] = section;
        }
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
    public void OnTutorialPointGained()
    {
        currentSection.tutorialPoints++;
        if (currentSection.tutorialPoints >= currentSection.pointsToContinue)
        {
            currentSection.tutorialPoints = 0;
            foreach (var addon in currentSection.sectionAddons)
            {
                addon.OnSectionEnded();
            }
            if (currentSection.nextSection != SectionName.Complete)
            {
                currentSection = sectionDict[currentSection.nextSection];
                foreach (var addon in currentSection.sectionAddons)
                {
                    addon.OnSectionStarted();
                }
                respawnPoint.position = currentSection.spawnPos;
            }
            else
            {
                OnTutorialCompleted();
            }
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