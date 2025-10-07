using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    [SerializeField] Grapple grapple;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseCharacter cha)) { return; }
        
            if (cha == grapple.character)
            {
            grapple.DestroyGrapple();
            }
        
    }
}
