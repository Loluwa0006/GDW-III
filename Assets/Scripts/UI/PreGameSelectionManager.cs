using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class PreGameSelectionManager : MonoBehaviour
{

    [System.Serializable]
    public class MapThumbnails
    {
        public MapRegistry map;
        public Sprite thumbnail;
    }


    public Dictionary<UISelector, MatchData.PlayerInfo> playerSelectors = new();

    [HideInInspector] public SelectionScreen selectionScreen = SelectionScreen.TeamSelect;
    [HideInInspector] public MapRegistry selectedMap = MapRegistry.The_Forum;

    [Header("Team Select Data")]
    [SerializeField] float verticalSpacing = -200;
    [SerializeField] float horizontalSpacing = 400;

    [Header("MenuScreens")]

    [SerializeField] GameObject teamSelectScreen;
    [SerializeField] GameObject skillSelectScreen;
    [SerializeField] GameObject mapSelectScreen;



    [SerializeField] GameObject mapButtonHolder;

    [SerializeField] PlayerInputManager inputManager;

    [SerializeField] TMP_Text mapDisplay;

    [Header("Map Thumbnails")]
    [SerializeField] List<MapThumbnails> mapThumbnails = new();
    [SerializeField] Image thumbnailDisplay;

    bool hasExtraKeyboardPlayer = false;


    Dictionary<MapRegistry, Sprite> mapThumbnailDict = new();
    MatchData matchData;

    private void Start()
    {
        matchData = FindFirstObjectByType<MatchDataHolder>().GetMatchData();
        if (inputManager == null )
        {
            inputManager = GetComponent<PlayerInputManager>();
        }
        verticalSpacing = Mathf.Abs(verticalSpacing) * -1;
        InitSelectionManager();

        InitMatchData();

      
    }


    void InitSelectionManager()
    {
        skillSelectScreen.SetActive(false);
        mapSelectScreen.SetActive(false);
        teamSelectScreen.SetActive(true);


        selectedMap = MapRegistry.The_Forum;


        int index = 0;
        foreach (Transform t in mapButtonHolder.transform)
        {
            Debug.Log("Looking at transform " + t.name);
            if (!t.TryGetComponent(out Button button)) { continue; }
            MapRegistry currentMap = ((MapRegistry)(index));

            string formattedName = currentMap.ToString().Replace("_", " ");
            button.GetComponentInChildren<TMP_Text>().text = formattedName;

            button.onClick.AddListener(() => SetSelectedMap(currentMap));
            index++;
        }

        foreach (var thumbnail in mapThumbnails)
        {
            mapThumbnailDict[thumbnail.map] = thumbnail.thumbnail;
        }
        SetSelectedMap(MapRegistry.The_Forum);
    }

    public void SetSelectedMap(MapRegistry newMap)
    {
        selectedMap = newMap;
        mapDisplay.text = selectedMap.ToString().Replace("_", " ");
        thumbnailDisplay.sprite = mapThumbnailDict[newMap];
    }

    public void StartGame()
    {
      string formattedString = selectedMap.ToString().Replace("_", "");
      SceneManager.LoadScene(formattedString);
    }
    void InitMatchData()
    {
        matchData.gameTeams.Clear();
        for (int i = 0; i < matchData.numberOfTeams; i++)
        {
            matchData.gameTeams.Add(new MatchData.TeamInfo());
        }
    }
    public void OnPlayerJoined(PlayerInput newPlayer)
    {
        if (!newPlayer.gameObject.TryGetComponent(out UISelector selector)) return;

        selector.Init(this, playerSelectors.Count + 1);

        StartCoroutine(InitSelector(selector, newPlayer));
    }
    public void AddNewKeyboardPlayer()
    {
        if (hasExtraKeyboardPlayer) return;
        hasExtraKeyboardPlayer = true;

        var manager = GetComponent<PlayerInputManager>();

         manager.JoinPlayer(pairWithDevice: Keyboard.current);
    }

    IEnumerator InitSelector(UISelector selector,PlayerInput pInput)
    {
        selector.transform.SetParent(transform, false);
        yield return null;
        SetNewTeamPos(selector, 0.0f);
        selector.teamIndex = 0;
        Vector3 spawnPos = Vector3.zero;
        spawnPos.y = verticalSpacing * playerSelectors.Count;
        selector.rectTransform.anchoredPosition = spawnPos;
        if (!playerSelectors.ContainsKey(selector))
        {
            playerSelectors.Add(selector, new MatchData.PlayerInfo());
        }
        selector.selectorLocked.AddListener(ContinueToNextScreen);
        bool keyboardTwo = false;
        foreach (var keys in playerSelectors.Keys)
        {
            if (playerSelectors[keys].device == Keyboard.current && pInput.devices[0] == Keyboard.current)
            {
                keyboardTwo = true;
            }
        }
        selector.gameObject.name = "Player" + pInput.playerIndex + "Selector";

        if (keyboardTwo)
        {
            Debug.Log("Setting player " + pInput.playerIndex + " to keyboard two control scheme");
            playerSelectors[selector].keyboardPlayerTwo = true;
            pInput.SwitchCurrentActionMap("CombatKeyboardTwo");
            playerSelectors[selector].device = Keyboard.current;
        }
        else
        {
            playerSelectors[selector].device = pInput.devices[0];
        }
    }
    public void OnSelectionMoved(UISelector selector, int dir)
    {
        Debug.Log("Team index is " + selector.teamIndex + ", dir is " + dir);
        if (selector.teamIndex == 0)
        {
            if (dir > 0)
            {
                SetNewTeamPos(selector, horizontalSpacing);
                selector.teamIndex = 2;
            }
            else if (dir < 0)
            {
                SetNewTeamPos(selector, -horizontalSpacing);
                selector.teamIndex = 1;
            }
        }
        else
        {
            if (dir > 0 && selector.teamIndex == 1 || dir < 0 && selector.teamIndex == 2)
            {
                SetNewTeamPos(selector, 0.0f);
                selector.teamIndex = 0;
            }
        }
        Debug.Log("New index is " + selector.teamIndex + ", new dir is " + dir);

    }

    public void OnSkillPressed(UISelector selector, int index)
    {
        if (selector.locked) { return; }
        MatchData.SkillName previousSkill;
        MatchData.SkillName nextSkill;
        int totalSkills = Enum.GetValues(typeof(MatchData.SkillName)).Length;
        if (index == 1)
        {
            previousSkill = playerSelectors[selector].skillOne;
            nextSkill = (MatchData.SkillName)(((int)previousSkill + 1) % totalSkills);

            if (nextSkill == playerSelectors[selector].skillTwo || nextSkill == MatchData.SkillName.None)
            {
                nextSkill = (MatchData.SkillName)(((int)nextSkill + 1) % totalSkills);
            }

            playerSelectors[selector].skillOne = nextSkill;
        }
        else if (index == 2)
        {
            previousSkill = playerSelectors[selector].skillTwo;
            nextSkill = (MatchData.SkillName)(((int)previousSkill + 1) % totalSkills);

            if (nextSkill == playerSelectors[selector].skillOne || nextSkill == MatchData.SkillName.None)
            {
                nextSkill = (MatchData.SkillName)(((int)nextSkill + 1) % totalSkills);
            }

            playerSelectors[selector].skillTwo = nextSkill;
        }

        selector.skillOneDisplay.text = playerSelectors[selector].skillOne.ToString();
        selector.skillTwoDisplay.text = playerSelectors[selector].skillTwo.ToString();

    }

    public void SetNewTeamPos(UISelector selector, float xPos)
    {
        Vector3 newPos = selector.rectTransform.anchoredPosition;
        newPos.x = xPos;
        selector.rectTransform.anchoredPosition = newPos;
        
    }

    public void ContinueToNextScreen(UISelector locked)
    {
        if (locked != null) SetPlayerData(locked);
        if (playerSelectors.Keys.Count < 2)
        {
            return;
        }
        foreach (var selector in playerSelectors.Keys)
        {
            if (!selector.locked) return;
        }
        
        switch (selectionScreen)
        {
            case SelectionScreen.TeamSelect:
                inputManager.DisableJoining();
                foreach (var team in matchData.gameTeams)
                {
                    if (team.teamMembers.Count == 0)
                    {
                        return;
                    }
                }
                skillSelectScreen.SetActive(true);
                teamSelectScreen.SetActive(false);
                foreach (var selector in playerSelectors.Keys) { selector.ToggleSkillDisplay(true); }
               
                StartCoroutine(ResetSelectors(SelectionScreen.SkillSelect));
                break;
            case SelectionScreen.SkillSelect:
                inputManager.DisableJoining();
                foreach (var selector in playerSelectors.Keys)
                {
                    selector.Hide(); 
                }
                skillSelectScreen.SetActive(false);
                mapSelectScreen.SetActive(true);
                break;

        }
       
    }

    public void ReturnToPreviousScreen()
    {
        switch (selectionScreen)
        {
            case SelectionScreen.TeamSelect:
                ReturnToMainMenu();
                break;
            case SelectionScreen.SkillSelect:

                inputManager.EnableJoining();
                skillSelectScreen.SetActive(false);
                teamSelectScreen.SetActive(true);
                foreach (var selector in playerSelectors.Keys) { selector.ToggleSkillDisplay(false); }
                StartCoroutine(ResetSelectors(SelectionScreen.TeamSelect));
                break;
            case SelectionScreen.MapSelect:
                mapSelectScreen.SetActive(false);
                skillSelectScreen.SetActive(true);
                foreach (var selector in playerSelectors.Keys)
                {
                    selector.Show();
                    selector.ToggleSkillDisplay(true);

                }
                StartCoroutine(ResetSelectors(SelectionScreen.SkillSelect));
                break;


        }

    }

    IEnumerator ResetSelectors(SelectionScreen newScreen)
    {
        yield return null;
        foreach (var selector in playerSelectors.Keys)
        {
            selector.ResetSelection();
        }
        selectionScreen = newScreen;
    }

    void SetPlayerData(UISelector selector)
    {
        if (selector.teamIndex == 0) { return; }
        switch (selectionScreen)
        {
            case SelectionScreen.TeamSelect:
                foreach (var teams in matchData.gameTeams)
                {
                    if (teams.teamMembers.Contains(playerSelectors[selector]))
                    {
                        teams.teamMembers.Remove(playerSelectors[selector]);
                    }
                }
                Debug.Log("Number of teams: " + matchData.gameTeams.Count + " Team index: " + selector.teamIndex);

                matchData.gameTeams[selector.teamIndex - 1].teamMembers.Add(playerSelectors[selector]);
                break;
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneRegistry.MainMenu_Test.ToString());
    }
}
public enum SelectionScreen
{
    TeamSelect,
    RoleSelect,
    SkillSelect,
    MapSelect
}
