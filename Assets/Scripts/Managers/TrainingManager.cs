using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInputManager))]
public class TrainingManager : MonoBehaviour
{
    [SerializeField] GameObject trainingScreen;
    [SerializeField] BaseCharacter characterPrefab;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] RicochetBall gameBall;
    [SerializeField] PlayerInputManager playerInputManager;
    [SerializeField] Button displayTraining;
    [SerializeField] GameManager gameManager;

    List<BaseCharacter> characterList = new();

    BaseCharacter keyboardPlayer = null;
    private void Awake()
    {
        trainingScreen.SetActive(false);
        displayTraining.gameObject.SetActive(true);
        if (playerInputManager == null)
        {
            playerInputManager = GetComponent<PlayerInputManager>();
        }
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    public void OnResetPressed()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("SampleScene");
    }

    public void OnAddPlayerPressed()
    {
        if (keyboardPlayer != null) { return; }
        keyboardPlayer = Instantiate(characterPrefab);

        PlayerInput pInput = keyboardPlayer.GetComponent<PlayerInput>();
        pInput.SwitchCurrentActionMap("CombatKeyboardTwo");

        OnPlayerJoined(keyboardPlayer.GetComponent<PlayerInput>());
      
    }

    public void OnRemovePlayerPressed()
    {
        BaseCharacter charToRemove = characterList[0];
        if (keyboardPlayer == charToRemove) { keyboardPlayer = null; }
        targetGroup.RemoveMember(charToRemove.transform);
        characterList.RemoveAt(0);
        Destroy(charToRemove.gameObject);

        UpdateBall();
        gameManager.RemoveCharacter(charToRemove);
    }

    public void OnTimescaleSliderValueChanged(float newValue)
    {
        Time.timeScale = Mathf.Clamp01(newValue);
    }

    public void OnPlayerJoined(PlayerInput newPlayer)
    {
        if (newPlayer.TryGetComponent(out BaseCharacter player))
        {
            characterList.Add(player);
            player.name = "Player " + characterList.Count;
            Debug.Log("Player joined!");
            targetGroup.AddMember(player.transform, 1.0f, 5.0f);
            UpdateBall();

            gameManager.AddCharacter(player);
        }
    }

    void UpdateBall()
    {

        Debug.Log("Updating ball state");

        if (gameBall.ballActive)
        {
            if (characterList.Count < 2)
            {
                gameBall.SuspendBall();
            }
        }
        else
        {
            if (characterList.Count >= 2)
            {
                gameBall.InitBall(characterList);
            }
        }
        
    }
}
