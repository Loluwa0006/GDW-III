using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{

    public const float DIALOGUE_CHARACTER_DISPLAY_SPEED = 0.02f;
    const int DIALOGUE_CHARACTER_LIMIT = 1000;

    [Header("Dialogue Box")]
    [SerializeField] TMP_Text dialogueTextbox;
    [SerializeField] TMP_Text speakerTextbox;
    [SerializeField] GameObject dialogueDisplay;

    [Header("")]
    [SerializeField] Color displayingColor = Color.white;
    [SerializeField] Color finishedColor = Color.green;
    PlayerInput playerInput;

    DialogueData currentDialogue = null;

    Coroutine dialogueCoroutine;


    Queue<DialogueData> dialogueQueue = new();

    private void Awake()
    {
        dialogueDisplay.SetActive(false);
    }
    public void SetPlayerInput(PlayerInput input)
    {
        playerInput = input;
    }

    public void QueueDialogue(params DialogueData[] data)
    {
        foreach (var dialogue in data)
        {
            dialogueQueue.Enqueue(dialogue);
        }
        if (dialogueCoroutine == null)   PlayDialogue();
    }
     void PlayDialogue()
    {
        if (dialogueCoroutine != null) { return; }
        dialogueDisplay.SetActive(true);
        currentDialogue = dialogueQueue.Dequeue();
        dialogueCoroutine = StartCoroutine(DisplayDialogue());
    }

    IEnumerator DisplayDialogue()
    {

        StringBuilder dialogueBuilder = new();
        dialogueTextbox.text = "";
        dialogueTextbox.color = displayingColor;
        speakerTextbox.text = currentDialogue.characterName.ToString();
        int index = 0;
        while (dialogueTextbox.text != currentDialogue.dialogue)
        {

            dialogueBuilder.Append(currentDialogue.dialogue[index]);
            index++;
            if (index >= DIALOGUE_CHARACTER_LIMIT)
            {
                break;
            }
            dialogueTextbox.text = dialogueBuilder.ToString();
            yield return new WaitForSeconds(currentDialogue.textSpeed);
        }
        dialogueTextbox.color = finishedColor;
        dialogueCoroutine = null;
        yield break;
    }

    void EndDialogue()
    {
        dialogueTextbox.text = string.Empty;
        dialogueDisplay.SetActive(false);
        currentDialogue = null;
    }

    private void Update()
    {
        HandlePlayerInput();
    } 
    void SkipToEndOfDialogue()
    {
        dialogueTextbox.text = currentDialogue.dialogue;
        dialogueTextbox.color = finishedColor;
        StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = null;
    }
    void HandlePlayerInput()
    {
        if (playerInput == null || currentDialogue == null) { return; }
        if (playerInput.actions["Deflect"].WasPressedThisFrame())
        {
            if (dialogueCoroutine != null)
            {
                SkipToEndOfDialogue();
                return;
            }
            if (dialogueQueue.Count <= 0)
            {
                EndDialogue();
            }
            else
            {
                PlayDialogue();
            }
        }
    }
     
    
}
