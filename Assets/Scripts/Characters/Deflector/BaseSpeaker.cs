using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.Users;
using System.Collections;

public class BaseSpeaker : MonoBehaviour
{

    public CharacterStateMachine characterStateMachine;
    
    public HealthComponent healthComponent;
    public DeflectManager deflectManager;
    public StaminaComponent staminaComponent;
    public PlayerInput playerInput;
    public MeshRenderer playerModel;
    public VelocityManager velocityManager;
    public GroundIndicator groundIndicator;
    public AudioSource audioSource;

    public List<Material> playerColors = new();

    [HideInInspector] public int teamIndex;
    [HideInInspector] Transform lookTarget = null;
    bool init = false;


    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();  
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void InitPlayer(MatchData.PlayerInfo info, int index)
    {

        teamIndex = index;
        playerModel.material = playerColors[index - 1];
        name = "Player " + index;
        groundIndicator.Init(playerColors[index -1], index);

        StartCoroutine(InitStateMachine(info));
        StartCoroutine(AssignLookTarget());
        AssignPlayerDevice(info);
    }

    IEnumerator InitStateMachine(MatchData.PlayerInfo info)
    {
        yield return new WaitForFixedUpdate();
        AssignPlayerDevice(info); //must do this first for state machine buffers, otherwise they will assume kb 1 speaker controls
        characterStateMachine.CreateSkills(info);
        characterStateMachine.InitMachine();
        init = true;
    }
    void AssignPlayerDevice(MatchData.PlayerInfo info)
    {
        if (!playerInput.user.valid)
        {
            Debug.Log("Invalid user for char " + name);
            return;
        }
        if (info.device is Gamepad)
        {
            playerInput.user.UnpairDevices(); //get rid of other gamepads / the keyboard
            InputUser.PerformPairingWithDevice(info.device, playerInput.user); // add this gamepad to the current player
        }

        Debug.Log("device name is " + info.device.name);
       
        playerInput.SwitchCurrentActionMap(info.controlScheme);
        

    }
    IEnumerator AssignLookTarget()
    {
        yield return new WaitForFixedUpdate();
        lookTarget = FindFirstObjectByType<BaseEcho>().transform;
    }

    private void Update()
    {
        if (GameManager.inSpecialStop || !init) { return; }
        characterStateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        if (GameManager.inSpecialStop || !init) { return; }
        characterStateMachine.FixedUpdateState();
        if (lookTarget != null)
        {
            playerModel.transform.LookAt(lookTarget);
        }
    }

    public void DeactivatePlayer()
    {
        playerModel.gameObject.SetActive(false);
        deflectManager.gameObject.SetActive(false);
        enabled = false;
    }

    public void ActivatePlayer()
    {
        playerModel.gameObject.SetActive(true);
        deflectManager.gameObject.SetActive(true);
        enabled = true;
    }

    public void SetLookTarget(Transform target)
    {
        lookTarget = target;
    }



}
