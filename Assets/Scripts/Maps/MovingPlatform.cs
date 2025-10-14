using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out BaseSpeaker speaker)) 
        {
            speaker.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.TryGetComponent(out BaseSpeaker speaker))
        {
            speaker.transform.SetParent(null);
        }
    }
}
