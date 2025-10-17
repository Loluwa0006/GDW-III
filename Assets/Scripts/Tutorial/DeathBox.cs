using UnityEngine;
public class DeathBox : MonoBehaviour

{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker speaker)) { return; }

        speaker.healthComponent.OnEntityDeath(null, speaker.healthComponent);
    }
}
