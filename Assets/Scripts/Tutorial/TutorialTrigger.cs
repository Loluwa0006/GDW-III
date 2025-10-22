using NaughtyAttributes;
using UnityEngine;


public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] TutorialManager manager;
    [SerializeField] Collider triggerCollider;
    [SerializeField] protected TutorialManager.SectionName sectionName;
    [SerializeField] TriggerType triggerType = TriggerType.PointOnEntry;
    [SerializeField] DeactivateType deactivateType = DeactivateType.DisableTrigger;
    [SerializeField] bool hideIfDisabled = false;
    [SerializeField] MeshRenderer mesh;

    [SerializeField, ShowIf(nameof(RequiresDialogue))] protected BaseDialogue dialogueData;
    [SerializeField, ShowIf(nameof(RequiresSkill))] MatchData.SkillName skill;
    [SerializeField, ShowIf(nameof(RequiresSkill))] int skillIndex;

    [SerializeField, ShowIf(nameof(RequiresInstantSwap))] TutorialManager.SectionName instantSectionSwap = TutorialManager.SectionName.Introduction;

    

    [System.Serializable]
    public enum TriggerType
    {
        PointOnEntry,
        DialogueOnly,
        PointOnDialogueComplete,
        SetSpecificSection,
        Turret,
        GrantSkill
    }


    [System.Serializable]
    public enum DeactivateType
    {
        DisableTrigger,
        BecomeWall
    }
    protected bool dialogueAssignable = true;

    bool RequiresDialogue() => triggerType == TriggerType.DialogueOnly || triggerType == TriggerType.PointOnDialogueComplete;
    bool RequiresSkill() => triggerType == TriggerType.GrantSkill;

    bool RequiresInstantSwap() => triggerType == TriggerType.SetSpecificSection;


    private void Awake()
    {
        InitTrigger();
    }

    protected virtual void InitTrigger()
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

        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
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
                case TriggerType.SetSpecificSection:
                    manager.StartSection(instantSectionSwap);
                    break;
                case TriggerType.GrantSkill:
                Debug.Log("Granted skill " + skill.ToString());
                player.characterStateMachine.AddNewSkill(skillIndex, skill);
                break;


            }
    }


    public virtual void OnSectionStarted(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            switch (deactivateType)
            {
                case DeactivateType.DisableTrigger:
                    triggerCollider.enabled = true;
                    break;
                case DeactivateType.BecomeWall:
                    triggerCollider.isTrigger = true;
                    break;
            }
        }
    }

    public virtual void OnSectionRestarted(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            switch (deactivateType)
            {
                case DeactivateType.DisableTrigger:
                    triggerCollider.enabled = true;
                    break;
                case DeactivateType.BecomeWall:
                    triggerCollider.isTrigger = true;
                    break;
            }
        }
    }

    public virtual void OnSectionEnded(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            switch (deactivateType)
            {
                case DeactivateType.DisableTrigger:
                    triggerCollider.enabled = false;
                    break;
                case DeactivateType.BecomeWall:
                    triggerCollider.isTrigger = false;
                    break;
            }

            if (mesh != null)
            {
                mesh.enabled = hideIfDisabled;
            }
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
