using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] HealthUI healthUIPrefab;
    [SerializeField] GameObject UIHolder;

    List<BaseCharacter> characterList = new();

    Dictionary<BaseCharacter, HealthUI> characterUI = new();
    private void Start()
    {
        foreach (Transform t in UIHolder.transform)
        {
            Destroy(t.gameObject);
        }
        characterList = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None).ToList();
        foreach (var  character in characterList)
        {
            HealthUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
            newUI.InitHealthDisplay(character.healthComponent);
            character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);
        }
    }

    public void AddCharacter(BaseCharacter character)
    {
        if (characterUI.ContainsKey(character)) { return; }
        HealthUI newUI = Instantiate(healthUIPrefab, UIHolder.transform);
        newUI.InitHealthDisplay(character.healthComponent);
        character.healthComponent.entityDefeated.AddListener(OnCharacterDefeated);
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
