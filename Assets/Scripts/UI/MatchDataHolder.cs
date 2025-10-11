using UnityEngine;

public class MatchDataHolder : MonoBehaviour
{
    [SerializeField] MatchData matchData;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public MatchData GetMatchData()
    {
        return matchData;
    }
}
