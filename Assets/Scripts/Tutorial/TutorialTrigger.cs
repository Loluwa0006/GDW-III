using UnityEngine;


public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] TutorialManager manager;
    [SerializeField] Collider triggerCollider;
    [SerializeField] protected TutorialManager.SectionName sectionName;
    [SerializeField] protected BaseDialogue dialogueData;
    [SerializeField] TriggerType triggerType = TriggerType.PointOnEntry;

    [System.Serializable]
    public enum TriggerType
    {
        PointOnEntry,
        DialogueOnly,
        PointOnDialogueComplete,
        Turret
    }

    protected bool dialogueAssignable = true;


    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }
        if (manager == null)
        {
            manager = FindFirstObjectByType<TutorialManager>();
        }
        triggerCollider.isTrigger = true;
       
        if (manager != null)
        {
            ConnectManagerSignals();
        }

        triggerCollider.enabled = false;
        
    }

    void ConnectManagerSignals()
    {
        manager.sectionRestarted.AddListener(OnSectionRestarted);
        manager.sectionEnded.AddListener(OnSectionEnded);
        manager.sectionStarted.AddListener(OnSectionStarted);

        if (triggerType.ToString().Contains("Dialogue")) 
        {
            if (dialogueData == null)
            {
                Debug.LogWarning("Trigger " + name + " missing dialogue data dependancy");
                return;
            }
            manager.dialogueManager.dialogueComplete.AddListener(OnDialogueComplete);
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) { return; } 
        if (!other.TryGetComponent(out BaseSpeaker player)) { return; }

      

        switch (triggerType)
        {
            case TriggerType.PointOnEntry:
                manager.GainTutorialPoint();
                triggerCollider.enabled = false;
                break;
            case TriggerType.DialogueOnly:
            case TriggerType.PointOnDialogueComplete:
                if (!dialogueAssignable) { return; }
                if (dialogueData == null) { Debug.LogWarning("Dialogue data not assigned."); return; }
                manager.dialogueManager.QueueDialogue(dialogueData.GetDialogues());
                dialogueAssignable = false;
                break;
        }
    }


    public virtual void OnSectionStarted(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            triggerCollider.enabled = true;
        }
    }

    public virtual void OnSectionRestarted(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            triggerCollider.enabled = true;
        }
    }

    public virtual void OnSectionEnded(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            triggerCollider.enabled = false;
        }
    }

    public void OnDialogueComplete()
    {
        if (!dialogueAssignable)
        {
            if (triggerType == TriggerType.PointOnDialogueComplete)
            {
                manager.GainTutorialPoint();
                triggerCollider.enabled = false;
            }
            dialogueAssignable = true;
        }
        
    }

}
