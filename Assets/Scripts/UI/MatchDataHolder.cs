using UnityEngine;

public class MatchDataHolder : MonoBehaviour
{
    [SerializeField] MatchData matchData;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        matchData.InitSkillPrefabs();
    }
    public MatchData GetMatchData()
    {
        return matchData;
    }
}
