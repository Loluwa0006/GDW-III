using UnityEngine;


[RequireComponent (typeof(Collider))]
public class TurretDetector : MonoBehaviour
{
    [SerializeField] TutorialTurret turret;

    [SerializeField] Collider detectorArea;


    float reloadTracker = 0.0f;


    private void Awake()
    {
         if (detectorArea == null)
        {
            detectorArea = GetComponent<Collider>();
        }
        detectorArea.isTrigger = true;
        reloadTracker = turret.reloadDuration;
        if (turret == null)
        {
            Debug.Log("turret detector " + name + " created without turret");
            turret = GetComponentInParent<TutorialTurret>();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker speaker)) { return; }


        reloadTracker += Time.deltaTime;
        if (reloadTracker >= turret.reloadDuration)
        {
            reloadTracker = 0.0f;
            turret.FireProjectile(speaker);
        }

        speaker.playerModel.transform.LookAt(turret.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker speaker)) { return; }

        turret.JoinTargetGroup();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker speaker)) { return; }
        turret.LeaveTargetGroup();
    }

    public void Reload()
    {
        reloadTracker = turret.reloadDuration;
    }
}
