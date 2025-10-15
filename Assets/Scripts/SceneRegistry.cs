using UnityEngine.UI;

[System.Serializable]
public enum SceneRegistry
{
    MainMenu_Test,
    GameSelection,
    SpeakerDuelTest,
    Tutorial
}

[System.Serializable]
public class TransitionButton
{
    public Button button;
    public SceneRegistry value;
}