using UnityEngine;

public class KickflipDetecctor : MonoBehaviour
{
    [SerializeField] Redirect manager;

    private void Awake()
    {
        if (manager == null)
        {
            manager = GetComponentInParent<Redirect>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent( out BaseSpeaker speaker))
        {
            if (other == manager.character)
            {
                return;
            }
        }
        
    }
}
