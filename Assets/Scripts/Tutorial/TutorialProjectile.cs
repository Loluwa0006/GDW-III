
using System;
using System.Collections;
using UnityEngine;

public class TutorialProjectile : BaseEcho
{
    [SerializeField] Transform spawnPos;
    [SerializeField] TutorialManager tutorialManager;
    [SerializeField] bool awardPointOnDeflect = false;
    [HideInInspector]  public bool projectileActive = false;

    public void InitProjectile( BaseSpeaker speaker)
    {
        currentTarget = speaker;
        transform.position = spawnPos.transform.position;

        mesh.enabled = true;
        hitbox.enabled = true;
        ballActive = true;
        _rb.isKinematic = false;
        activeMinSpeed = minSpeed;
        activeMaxSpeed = maxSpeed;
        deflectStreak = 0;
        UpdateSpeed(startingSpeed);

        projectileActive = true;
    }


    protected override IEnumerator PostContactLogic(BaseSpeaker cha, bool landedHit)
    {
        SuspendBall();
        yield return base.PostContactLogic(cha, landedHit);
        projectileActive = false;
        if (awardPointOnDeflect && !landedHit)
        {
            if (tutorialManager == null) { Debug.LogError("Tried to assign point, but tutorial manager is null"); yield break; }
            tutorialManager.GainTutorialPoint();
        }

    }
    public override void FindNewTarget(BaseSpeaker lastHitCharacter)
    {
        //no new target, its only 1 player
    }
}
