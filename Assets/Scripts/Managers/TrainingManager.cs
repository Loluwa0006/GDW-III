using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

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

    HashSet<BaseCharacter> characterList = new();

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
        BaseCharacter charToRemove = characterList.ElementAt(0);
        if (keyboardPlayer == charToRemove) { keyboardPlayer = null; }
        targetGroup.RemoveMember(charToRemove.transform);
        characterList.Remove(charToRemove);
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

            if (player.playerColors.Count > newPlayer.playerIndex)
            {
                player.playerModel.material = player.playerColors[characterList.Count - 1];
            }
        }
    }

    void UpdateBall()
    {

        Debug.Log("Updating ball state");

        gameBall.UpdateActiveCharacters(characterList);
       
    }
}
