using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInputManager))]
public class TrainingManager : GameManager
{
    [Header("ShadingMaterials")]
    [SerializeField]  List<MeshRenderer> primitiveMeshes = new();

    [SerializeField] Transform respawnPoint;
    [SerializeField] Transform dummySpawnpoint;
    [SerializeField] Material defaultMaterial;

    BaseSpeaker trainingSpeaker = null;
    [HideInInspector] public BaseSpeaker playerSpeaker = null;


    [Header("Correction Objects")]
    [SerializeField] ColorCorrection correctionCamera;
    [SerializeField] Image correctionImage;

    private void Awake()
    {
        if (defaultMaterial != null)
        {
            SetNewMaterial(defaultMaterial);
        }
    }

    public void SetNewMaterial(Material material)
    {
        foreach (var mesh in primitiveMeshes)
        {
            mesh.material = material;
        }
    }

    public void SetNewColorGrade(Material newMaterial)
    {
        correctionCamera.correctionMaterial = newMaterial;
        correctionImage.material = newMaterial;
        Debug.Log("Setting material to " + newMaterial.name);
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
        defeated.velocityManager.ResetComponent();
    }

    public override void ResetGame()
    {
        SceneManager.LoadScene(SceneRegistry.Training.ToString());
    }

    protected override void InitEchoes()
    {
        
    }


    protected override void InitPlayers()
    {
        InputDevice inputDevice = Gamepad.all.Count > 0 ? Gamepad.all[0] : Keyboard.current;


        MatchData.PlayerInfo tutorialPlayer = new ()
        {
            device = inputDevice,
            keyboardPlayerTwo = false,
            playerType = MatchData.PlayerType.Speaker,
            skillOne = MatchData.SkillName.None,
            skillTwo = MatchData.SkillName.None,
        };
        queuedPlayerInfo.Enqueue(tutorialPlayer);
        inputManager.JoinPlayer(pairWithDevice: inputDevice);

        MatchData.PlayerInfo dummy = new ()
        {
            device = Keyboard.current,
            keyboardPlayerTwo = true,
            playerType = MatchData.PlayerType.Speaker,
            skillOne = MatchData.SkillName.None,
            skillTwo = MatchData.SkillName.None,
        };
        queuedPlayerInfo.Enqueue(dummy);
        inputManager.JoinPlayer(pairWithDevice: inputDevice);

    

    }

    
    protected override void InitTimer()
    {

    }

    protected override void TimerLogic()
    {
        
    }

    protected override IEnumerator SetCharacterPosition(BaseSpeaker character)
    {
        yield return new WaitForFixedUpdate();
        character.transform.position = respawnPoint.position;
        Debug.Log("Set char position to " + respawnPoint.gameObject.name + " position");
    }


    public void ActivateTutorialDummy()
    {
        if (trainingSpeaker == null) { return; }
        trainingSpeaker.ActivatePlayer();
        targetGroup.AddMember(trainingSpeaker.transform, 1.0f, 5.0f);
        InvulnerabilityEffect invulnerabilityEffect = new(DamageSource.Ball, int.MaxValue, false);
        trainingSpeaker.healthComponent.AddStatusEffect(invulnerabilityEffect, "trainingDummyImmunity");
        echoList[0].InitProjectile(speakerList);
        StartCoroutine(SetDummyToSpawnPos());

        
    }

    IEnumerator SetDummyToSpawnPos()
    {
        yield return new WaitForFixedUpdate();
        trainingSpeaker.transform.position = dummySpawnpoint.position;
    }
    public void DeactivateTutorialDummy()
    {
        if (trainingSpeaker == null) return;
        targetGroup.RemoveMember(trainingSpeaker.transform);
        trainingSpeaker.DeactivatePlayer();
        echoList[0].SuspendProjectile();
    }
    public override void OnPlayerJoined(PlayerInput playerInput)
    {
        base.OnPlayerJoined(playerInput);

        Debug.Log("Adding training player ");
        if (!playerInput.TryGetComponent(out BaseSpeaker speakerComponent)) return;

        
        if (playerSpeaker == null) playerSpeaker = speakerComponent;
        else trainingSpeaker = speakerComponent;
        if (speakerComponent == trainingSpeaker)
        {
            DeactivateTutorialDummy();
        }
        
    }

    public void AssignDash()
    {
        AssignNewSkill(MatchData.SkillName.Dash);
    }

    public void AssignCounterslash()
    {
        AssignNewSkill(MatchData.SkillName.Counterslash);
    }

    public void AssignAfterimage()
    {
        AssignNewSkill(MatchData.SkillName.Afterimage);
    }

    public void AssignGrapple()
    {
        AssignNewSkill(MatchData.SkillName.Grapple);
    }

    public void AssignNewSkill(MatchData.SkillName name)
    {
        if (playerSpeaker == null) { return; }
        playerSpeaker.characterStateMachine.AddNewSkill(1, name);
    }
}

