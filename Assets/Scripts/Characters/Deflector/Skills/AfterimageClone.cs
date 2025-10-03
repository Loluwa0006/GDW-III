using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    [SerializeField] Afterimage afterimageManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RicochetBall ball))
        {
            Debug.Log("Destroying clone, ball hit it");
            ball.OnDeflect(afterimageManager.character);
            afterimageManager.OnCloneDestroyed();
        }
    }
}
