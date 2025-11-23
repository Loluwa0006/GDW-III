using System.Collections;
using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    [SerializeField] Afterimage afterimageManager;
    [SerializeField] ProgressBar chargeMeter;
    public Collider afterimageCollider;
    [Header("Particles")]
    [SerializeField] ParticleSystem specialDeflectParticles;
    [SerializeField] ParticleSystem leftDustParticles;
    [SerializeField] ParticleSystem rightDustParticles;
    [SerializeField] ParticleSystem speedlineParticles;
    [SerializeField] ParticleSystem growingCircleParticles;

    [Header("Meshes")]
    [SerializeField] MeshRenderer mesh;
    [SerializeField] MeshRenderer barrierDisplay;




    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null) { return; }
        if (other.transform.parent.TryGetComponent(out BaseEcho echo))
        {
            if (echo.GetTarget() != afterimageManager.character) { return; }
            Debug.Log("Destroying clone, ball hit it");
            Disable();
            echo.OnDeflect(afterimageManager.character);
            specialDeflectParticles.transform.position = transform.position;
            specialDeflectParticles.time = 0;
            specialDeflectParticles.Play();
            if (afterimageManager.DeflectFullyCharged())
            {
              StartCoroutine(OnCloneChargedDeflect(echo));
            }
            afterimageManager.OnCloneDestroyed();
        }
        else if (other.transform.parent.TryGetComponent(out BaseSpeaker speaker))
        {
            if (speaker == afterimageManager.character) return;
            afterimageManager.OnCloneDestroyed();
        }
    }

    public void Enable()
    {
        afterimageCollider.enabled = true;
        mesh.enabled = true;
        chargeMeter.SetDisplayStatus(true);
        speedlineParticles.Play();
        barrierDisplay.enabled = true;
    }

    public void Disable()
    {
        afterimageCollider.enabled = false;
        mesh.enabled = false;
        chargeMeter.SetDisplayStatus(false);
        speedlineParticles.Stop();
        barrierDisplay.enabled = false;
    }

    public void ShowMesh()
    {
        mesh.enabled = true;
        barrierDisplay.enabled = true;
    }
    public bool IsActive()
    {
        return afterimageCollider.enabled;
    }

    IEnumerator OnCloneChargedDeflect(BaseEcho echo)
    {
        afterimageManager.character.deflectManager.superDeflectPerformed.Invoke(afterimageManager.character);
        GameManager.ApplyHitstop(afterimageManager.chargedDeflectParrystop);
        echo.FindNewTarget(afterimageManager.character);
        yield return null;
        yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        echo.transform.position = echo.GetTarget().transform.position;
        transform.LookAt(echo.transform.position);
        leftDustParticles.Play();
        rightDustParticles.Play();
        growingCircleParticles.transform.LookAt(echo.transform.position);
        growingCircleParticles.Play();
    }
}
