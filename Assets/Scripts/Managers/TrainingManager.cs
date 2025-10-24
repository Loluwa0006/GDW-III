using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

}

