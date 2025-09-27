using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] StaminaUI healthUIPrefab;
    [SerializeField] GameObject UIHolder;
    [SerializeField] List<GameObject> spawnPositions = new();
    List<BaseCharacter> characterList = new();

    Dictionary<BaseCharacter, StaminaUI> characterUI = new();


    private void Start()
    {
        foreach (Transform t in UIHolder.transform)
        {
            Destroy(t.gameObject);
        }
        characterList = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None).ToList();
        foreach (var character in characterList)
        {
            StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
            newUI.InitStaminaDisplay(character.staminaComponent);
            character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);
        }
    }

    public void AddCharacter(BaseCharacter character)
    {
        if (characterUI.ContainsKey(character)) { return; }
        StaminaUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        characterUI[character] = newUI;
        newUI.InitStaminaDisplay(character.staminaComponent);
        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);

        StartCoroutine(SetCharacterPosition(character));
    }

    IEnumerator SetCharacterPosition(BaseCharacter character)
    {
        int playerIndex = characterUI.Count;
        int spawnIndex = (playerIndex - 1) % spawnPositions.Count;
        yield return new WaitForFixedUpdate();
        Debug.Log("Position Before: " + character.transform.position);
        character.transform.position = spawnPositions[spawnIndex].transform.position;
        Debug.Log("Position Now: " + character.transform.position);
    }

    public void RemoveCharacter(BaseCharacter character)
    {
        if (characterUI.ContainsKey(character))
        {
            Destroy(characterUI[character].gameObject);
        }
    }
    void OnCharacterDefeated(DamageInfo info, HealthComponent victim)
    {
        if (!victim.hitboxOwner.TryGetComponent(out BaseCharacter defeated)) { return; }
        characterList.Remove(defeated);
        if (characterList.Count == 1)
        {
            OnCharacterVictorious();
        }
    }

    void OnCharacterVictorious()
    {
        BaseCharacter winner = characterList[0];

        Debug.Log("Character " + winner.name + " wins!");
        Time.timeScale = 0.0f;
    }


}
