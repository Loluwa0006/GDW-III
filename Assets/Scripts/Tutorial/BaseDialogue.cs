using UnityEngine;

[CreateAssetMenu(fileName = "BaseDialogue", menuName = "Scriptable Objects/BaseDialogue")]
public abstract class BaseDialogue : ScriptableObject
{
    public abstract DialogueData[] GetDialogues();
}
