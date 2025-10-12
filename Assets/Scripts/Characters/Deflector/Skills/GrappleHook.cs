using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    [SerializeField] Grapple grapple;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker cha)) { return; }
        
            if (cha == grapple.character)
            {
            grapple.DestroyGrapple();
            }
        
    }
}
