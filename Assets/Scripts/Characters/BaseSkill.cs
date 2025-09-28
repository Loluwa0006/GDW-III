using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class BaseSkill : CharacterBaseState
{
    public int staminaCost = 15;
    [HideInInspector] public bool skillActive = false;

    protected StaminaComponent staminaComponent;
    protected InputAction skillAction;



    private void Awake()
    {
        character = GetComponentInParent<BaseCharacter>();
    }

    public override void InitState(BaseCharacter cha, CharacterStateMachine s_machine)
    {
        base.InitState(cha, s_machine);
        staminaComponent = character.staminaComponent;
        InitSkill(1);
    }

    public void InitSkill(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1:
                skillAction = character.playerInput.actions["SkillOne"];
                break;
            case 2:
                skillAction = character.playerInput.actions["SkillTwo"];
                break;
            case 3:
                skillAction = character.playerInput.actions["SkillThree"];
                break;
            default:
                skillAction = character.playerInput.actions["SkillOne"];
                break;

        }
    }
    public override void Process()
    {
        CheckForSkillButtonPressed();
    }

    protected void CheckForSkillButtonPressed()
    {
        if (skillAction != null)
        {
            if (skillAction.WasPerformedThisFrame() && SkillAvailable())
            {
                OnSkillUsed();
            }
        }

        if (skillActive) { SkillLogic(); }
    }
    public virtual void OnSkillUsed()
    {
        staminaComponent.DamageStamina(staminaCost, false);
        skillActive = true;
    }

    public virtual void OnSkillOver()
    {
        skillActive = false;
    }

    public virtual void SkillLogic()
    {

    }

    public virtual bool SkillAvailable()
    {
        return staminaComponent.GetStamina() > staminaCost;
    }




}
