using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class BaseSkill : CharacterBaseState
{
    public int staminaCost = 15;
    public int skillIndex = 1;
   
    protected StaminaComponent staminaComponent;
    protected InputAction skillAction;
    protected int oppositeSkillIndex;

    protected BufferHelper oppositeSkillBuffer;
    protected BufferHelper skillBuffer;


    private void Awake()
    {
        character = GetComponentInParent<BaseCharacter>();
    }

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        staminaComponent = character.staminaComponent;
        InitSkill(skillIndex);
    }

    void InitSkill(int skillIndex)
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
        staminaComponent.DamageStamina(staminaCost, false);
    }


    public virtual bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost;
    }


    private void Update()
    {
        if (skillAction.WasPerformedThisFrame())
        {
            Debug.Log("Skill " + name + " 's control was pressed this frame.");
        }
    }



}
