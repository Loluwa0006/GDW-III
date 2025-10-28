using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    [SerializeField] Afterimage afterimageManager;
    [SerializeField] ParticleSystem specialDeflectParticles;
    public Collider afterimageCollider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null) { return; }
        if (other.transform.parent.TryGetComponent(out BaseEcho ball))
        {
            Debug.Log("Destroying clone, ball hit it");
            ball.OnDeflect(afterimageManager.character);
            specialDeflectParticles.transform.position = transform.position;
            specialDeflectParticles.time = 0;
            specialDeflectParticles.Play();
            afterimageManager.OnCloneDestroyed();

        }
    }
}
