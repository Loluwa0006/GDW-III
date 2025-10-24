using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MatchData", menuName = "Scriptable Objects/MatchData")]
public class MatchData : ScriptableObject
{

    [System.Serializable]
    public enum SkillName
    {
        Dash,
        Counterslash,
        Afterimage,
        Grapple,
        None
    }

    public enum PlayerType
    {
        Speaker,
        Echo
    }

    public enum GameType
    {
        SpeakerDuel, // 1v1 no echo players
        Classic, // 2v2 
        FFA, // multiple speaker players, 1 ball
        OneShotRumble //2v2, permanent danger zone

            //these game modes probably won't make it into the game on release but idk 100%, maybe
    }
    [System.Serializable]
    public class PlayerInfo
    {
        public SkillName skillOne = SkillName.Dash;
        public SkillName skillTwo = SkillName.Counterslash;
        public PlayerType playerType = PlayerType.Speaker;
        public InputDevice device;
        public bool keyboardPlayerTwo = false;

    }

    public class TeamInfo
    {
        public HashSet<PlayerInfo> teamMembers = new();
        public string teamName;
        public int handicapLevel = 0;
    }
    [System.Serializable]
    public class SkillPrefabs
    {
        public SkillName skillName;
        public BaseSkill skillPrefab;
    }

    [HideInInspector] public int numberOfTeams = 2;

    [HideInInspector] public List<TeamInfo> gameTeams = new();

    [HideInInspector] public int numberOfRounds = 2;

    [HideInInspector] public int gameLength = 60;

    [HideInInspector] public bool initPrefabs = false;

    [SerializeField] List<SkillPrefabs> skillPrefabs = new();

    public Dictionary<SkillName, BaseSkill> skillPrefabDictionary = new();

    public static MatchData instance;


    public void InitSkillPrefabs()
    {
        foreach (var kvp in skillPrefabs)
        {
            skillPrefabDictionary[kvp.skillName] = kvp.skillPrefab;
        }
        initPrefabs = true;

    }

}
