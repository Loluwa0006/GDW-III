using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    [SerializeField] Afterimage afterimageManager;
    public Collider afterimageCollider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null) { return; }
        if (other.transform.parent.TryGetComponent(out RicochetBall ball))
        {
            Debug.Log("Destroying clone, ball hit it");
            ball.OnDeflect(afterimageManager.character);
            afterimageManager.OnCloneDestroyed();
        }
    }
}
