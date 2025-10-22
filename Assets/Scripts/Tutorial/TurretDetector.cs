using UnityEngine;


[RequireComponent (typeof(Collider))]
public class TurretDetector : MonoBehaviour
{
    [SerializeField] TutorialTurret turret;
    [SerializeField] float reloadDuration = 7.0f;

    [SerializeField] Collider collider;


    float reloadTracker = 0.0f;


    private void Awake()
    {
         if (collider == null)
        {
            collider = GetComponent<Collider>();
        }
        collider.isTrigger = true;
        reloadTracker = reloadDuration / 2.0f;
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
        if (reloadTracker >= reloadDuration)
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
}
