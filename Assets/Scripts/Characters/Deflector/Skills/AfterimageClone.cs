using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    [SerializeField] Afterimage afterimageManager;
    [SerializeField] ParticleSystem specialDeflectParticles;
    public Collider afterimageCollider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null) { return; }
        if (other.transform.parent.TryGetComponent(out BaseEcho echo))
        {
            Debug.Log("Destroying clone, ball hit it");
            echo.OnDeflect(afterimageManager.character);
            specialDeflectParticles.transform.position = transform.position;
            specialDeflectParticles.time = 0;
            specialDeflectParticles.Play();
            if (afterimageManager.DeflectFullyCharged())
            { 
                echo.FindNewTarget(afterimageManager.character);
                echo.transform.position = echo.GetTarget().transform.position;
            }
            afterimageManager.OnCloneDestroyed();
        }
        else if (other.transform.parent.TryGetComponent(out BaseSpeaker speaker))
        {
            if (speaker == afterimageManager.character) return;
            afterimageManager.OnCloneDestroyed();
        }
    }
}
