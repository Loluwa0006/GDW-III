using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] HealthUI healthUIPrefab;
    [SerializeField] GameObject UIHolder;

    List<BaseCharacter> characterList = new();
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
        }
    }
}
