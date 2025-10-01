using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class BaseSkill : CharacterBaseState
{
    public int staminaCost = 15;
    public int skillIndex = 1;
   
    protected StaminaComponent staminaComponent;
    protected InputAction skillAction;
    protected InputAction oppositeSkillAction;
    protected int oppositeSkillIndex;


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
                oppositeSkillAction = character.playerInput.actions["SkillTwo"];
                oppositeSkillIndex = 2;
                break;
            case 2:
                skillAction = character.playerInput.actions["SkillTwo"];
                oppositeSkillAction = character.playerInput.actions["SkillOne"];
                oppositeSkillIndex = 1;
                break;
            case 3:
                skillAction = character.playerInput.actions["SkillThree"];
                break;
            default:
                skillAction = character.playerInput.actions["SkillOne"];
                break;
        }
    }
  
    public virtual void OnSkillUsed()
    {
        staminaComponent.DamageStamina(staminaCost, false);
    }


    public virtual bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost;
    }




}
