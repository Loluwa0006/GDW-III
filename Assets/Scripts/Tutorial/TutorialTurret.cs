using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class TutorialTurret : TutorialTrigger
{
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] TutorialProjectile projectile;

    private void Awake()
    {
        if (targetGroup == null)
        {
            targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();
        }


    }
    public void FireProjectile(BaseSpeaker speaker)
    {

        if (!projectile.projectileActive)
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
}
