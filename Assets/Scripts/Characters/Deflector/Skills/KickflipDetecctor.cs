using UnityEngine;

public class KickflipDetecctor : MonoBehaviour
{
    [SerializeField] Pivot manager;

    private void Awake()
    {
        if (manager == null)
        {
            manager = GetComponentInParent<Pivot>();
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
