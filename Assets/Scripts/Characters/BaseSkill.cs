using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class BaseSkill : CharacterBaseState
{
    public int staminaCost = 15;
   
    protected StaminaComponent staminaComponent;
    protected InputAction skillAction;
    protected int oppositeSkillIndex;

    protected BufferHelper oppositeSkillBuffer;
    protected BufferHelper skillBuffer;

    int skillIndex;


    private void Awake()
    {
        character = GetComponentInParent<BaseSpeaker>();
    }

    public override void InitState(BaseSpeaker cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        staminaComponent = character.staminaComponent;
        InitSkill();
    }

    public void SetSkillIndex(int index)
    {
        skillIndex = index;
    }
    public void InitSkill()
    {
        switch (skillIndex)
        {
            case 1:
                skillAction = character.playerInput.actions["SkillOne"];
                oppositeSkillIndex = 2;
                oppositeSkillBuffer = fsm.TryGetBuffer("SkillTwoBuffer");
                skillBuffer = fsm.TryGetBuffer("SkillOneBuffer");
                break;
            case 2:
                skillAction = character.playerInput.actions["SkillTwo"];
                oppositeSkillIndex = 1;
                oppositeSkillBuffer = fsm.TryGetBuffer("SkillOneBuffer");
                skillBuffer = fsm.TryGetBuffer("SkillTwoBuffer");
                break;
            case 3:
                skillAction = character.playerInput.actions["SkillThree"];
                break;
            default:
                skillAction = character.playerInput.actions["SkillOne"];
                break;
        }

        Debug.Log("Skill button for " + name + " is " + skillAction.GetBindingDisplayString());
    }
  
    public virtual void OnSkillUsed()
    {
        if (!staminaComponent.HasForesight())
        {
            staminaComponent.DamageStamina(staminaCost, 0, false);
        }
        else
        {
            staminaComponent.ConsumeForesight();
        }
    }


    public virtual bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost || staminaComponent.HasForesight();
    }


    private void Update()
    {
        if (skillAction.WasPerformedThisFrame())
        {
            Debug.Log("Skill " + name + " 's control was pressed this frame.");
        }
    }

    public virtual void ResetSkill()
    {

    }

}
