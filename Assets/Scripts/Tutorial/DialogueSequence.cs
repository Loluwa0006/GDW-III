using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Scriptable Objects/Dialogue/DialogueSequence")]
public class DialogueSequence : BaseDialogue
{
   public List<DialogueData> dialogueSequence;

    public override DialogueData[] GetDialogues()
    {
       return dialogueSequence.ToArray();
    }
}
