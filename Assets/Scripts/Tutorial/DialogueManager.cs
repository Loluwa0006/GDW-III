using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{

    public const float DIALOGUE_CHARACTER_DISPLAY_SPEED = 0.02f;
    const int DIALOGUE_CHARACTER_LIMIT = 1000;

   [HideInInspector] public UnityEvent dialogueComplete = new();


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

        string parsedDialogue = ParseDialogue(currentDialogue.dialogue);
        dialogueTextbox.text = "";
        dialogueTextbox.color = displayingColor;
        speakerTextbox.text = currentDialogue.characterName.ToString();
        int index = 0;
        while (dialogueTextbox.text != parsedDialogue)
        {

            dialogueBuilder.Append(parsedDialogue[index]);
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

    string ParseDialogue(string dialogue)
    {
        string parsed = dialogue;

        int maxAttempts = 10;
        int index = 0;


        while (parsed.Contains(DialogueData.actionSymbol))
        {
            var start = dialogue.IndexOf(DialogueData.actionSymbol);
            var end = dialogue.IndexOf(DialogueData.actionSymbol, start + 1);

            if (start == -1 || end == -1) break;

            var action = dialogue.Substring(start + 1, end - (start - 1));

            action = action.Replace(DialogueData.actionSymbol.ToString(), "");
            action = action.Trim();

            var actionName = action;

            Debug.Log("action name is (" + actionName + ")");
            if (playerInput != null)
            {
                if (playerInput.actions.FindAction(actionName) != null)
                {
                    actionName = playerInput.actions[action].GetBindingDisplayString();

                    Debug.Log("display string is (" + actionName + ")");

                }
                else
                {
                    Debug.Log("couldn't find action " + actionName);
                    return dialogue;
                }
            }
            else
            {
                Debug.LogWarning("Playinput is null for " + name);
            }

            string fullTag = parsed.Substring(start, end - start + 1); // includes the $ symbols
            parsed = parsed.Replace(fullTag, actionName);
            index++;
            if (index >= maxAttempts) return dialogue;
        }
        return parsed;
    }

    void EndDialogue()
    {
        dialogueTextbox.text = string.Empty;
        dialogueDisplay.SetActive(false);
        currentDialogue = null;
        if (dialogueQueue.Count == 0)
        {
            dialogueComplete.Invoke();
        }
    }

    private void Update()
    {
        HandlePlayerInput();
    } 
    void SkipToEndOfDialogue()
    {
        string parsed = ParseDialogue(currentDialogue.dialogue);
        dialogueTextbox.text = parsed;
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
