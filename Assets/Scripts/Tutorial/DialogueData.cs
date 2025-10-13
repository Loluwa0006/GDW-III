using UnityEngine;
using static DialogueManager;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/Dialogue/DialogueData")]
public class DialogueData : BaseDialogue
{

    public CharacterName characterName = CharacterName.Trainee;
    public VoiceType voiceType = VoiceType.Normal;
    public string dialogue;
    public float textSpeed = DIALOGUE_CHARACTER_DISPLAY_SPEED;

    public override DialogueData[] GetDialogues()
    {
        return new DialogueData[] { this };
    }

    [System.Serializable]
    public enum CharacterName
    {
        Trainee,
        Mentor,
        Boss,
        Crystal,
        Sign,
        Narrator
    }
    [System.Serializable]
    public enum VoiceType //will add voice type functionality later if there's time
    {
        Normal,
        Whisper,
        Yell,
        Other
    }
}
