
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

        if (tutorialManager == null) { tutorialManager = FindFirstObjectByType<TutorialManager>(); }
    }


    protected override IEnumerator PostContactLogic(BaseSpeaker cha, bool landedHit)
    {

        if (!projectileActive) { yield break; }

        Debug.Log("Struck player " + cha.name);

        RemoveSpeedDuringHitstop();
        yield return null;
        if (landedHit)
        {
            PlayHitsparks();
            GameManager.ApplyHitstop(hitstopAmount);
        }
        else
        {
            float t = deflectStreak / (float)deflectsUntilMaxSpeed;
            deflectStreak += 1;
            UpdateSpeed(Mathf.Lerp(minSpeed, maxSpeed, t));
            GameManager.ApplyHitstop(deflectstopAmount);
        }
        yield return null;
        SuspendBall();
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

    void FixedUpdate()
    {
        if (GameManager.inSpecialStop) { return; }
        foreach (BaseSpeaker character in characterList)
        {
            if (character != null) character.playerModel.transform.LookAt(transform.position);
        }
        if (!ballActive || currentTarget == null) { return; }
        _rb.linearVelocity = (currentTarget.transform.position - transform.position).normalized * currentSpeed;
        transform.rotation = Quaternion.LookRotation((transform.position - GetTarget().transform.position).normalized);
    }

  
}
