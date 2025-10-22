using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class TutorialTurret : TutorialTrigger
{
    public float reloadDuration = 7.0f;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] TutorialProjectile projectile;
    [SerializeField] TurretDetector detector;

    bool active = false;


    protected override void InitTrigger()
    {
        base.InitTrigger();
        if (targetGroup == null)
        {
            targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();
        }
    }

    public void FireProjectile(BaseSpeaker speaker)
    {

        if (!projectile.projectileActive && active)
        {
            Debug.Log("Firing projectile");
            projectile.InitProjectile(speaker);
        }
    }

    public void JoinTargetGroup()
    { 
        targetGroup.AddMember(transform, 1.0f, 5.0f);
    }

    public void LeaveTargetGroup()
    {
        targetGroup.RemoveMember(transform);
    }

    public override void OnSectionEnded(TutorialManager.TutorialSection section)
    {
        base.OnSectionEnded(section);
        LeaveTargetGroup();
        projectile.SuspendBall();
        active = false;
    }
    public override void OnSectionStarted(TutorialManager.TutorialSection section)
    {
        base.OnSectionStarted(section);
        
        active = sectionName == section.sectionName;

        Debug.Log("Active == " + active);
    }

    public override void OnSectionRestarted(TutorialManager.TutorialSection section)
    {
        base.OnSectionRestarted(section);

        active = sectionName == section.sectionName;

        if (active)
        {
            projectile.SuspendBall();
            detector.Reload();
        }
    }
}
