using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AfterimageClone : MonoBehaviour
{

    const float CRACK_REMOVAL_SPEED = 0.1f;

    [SerializeField] Afterimage afterimageManager;
    [SerializeField] ProgressBar chargeMeter;
    public Collider afterimageCollider;
    [Header("Effects")]
    [SerializeField] ParticleSystem specialDeflectParticles;
    [SerializeField] ParticleSystem leftDustParticles;
    [SerializeField] ParticleSystem rightDustParticles;
    [SerializeField] ParticleSystem speedlineParticles;
    [SerializeField] ParticleSystem growingCircleParticles;
    [SerializeField] ParticleSystem windBacktrailParticles;
    [SerializeField] MeshRenderer crackDecal;


    [Header("Meshes")]
    [SerializeField] MeshRenderer mesh;

    [Header("SFX")]
    [SerializeField] AudioClip explosionSFX;
    [SerializeField] AudioClip glassShatterSFX;
    [SerializeField] AudioClip clangSFX;

    [SerializeField] Color initialCrackColor;
    [SerializeField] Color finalCrackColor;

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
            afterimageManager.character.unscaledAudioSource.PlayOneShot(clangSFX);
            afterimageManager.DestroyClone();
        }
        else if (other.transform.parent.TryGetComponent(out BaseSpeaker speaker))
        {
            if (speaker == afterimageManager.character) return;
            afterimageManager.DestroyClone();
            PlayDeflectSound();
        }
    }

    public void Enable()
    {
        crackDecal.material.SetColor("_DecalColor", finalCrackColor);
        afterimageCollider.enabled = true;
        mesh.enabled = true;
        chargeMeter.SetDisplayStatus(true);
        speedlineParticles.Play();
    }

    public void Disable()
    {
        crackDecal.material.SetColor("_DecalColor", finalCrackColor);
        afterimageCollider.enabled = false;
        mesh.enabled = false;
        chargeMeter.SetDisplayStatus(false);
        speedlineParticles.Stop();
    }
    public void ShowMesh()
    {
        crackDecal.material.SetColor("_DecalColor", finalCrackColor);
        mesh.enabled = true;
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
        AddTerrainCrack();
        yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        crackDecal.gameObject.SetActive(true);
        echo.transform.position = echo.GetTarget().transform.position;
        transform.LookAt(echo.transform.position);
        PlayParticles();
        PlayPostHitstopSound();
        crackDecal.gameObject.SetActive(true);
        yield return new WaitUntil(() => leftDustParticles.isEmitting);
        yield return new WaitUntil(() => !leftDustParticles.isEmitting);
        RemoveTerrainCrack();
    }

    void PlayPostHitstopSound()
    {
        afterimageManager.character.unscaledAudioSource.PlayOneShot(explosionSFX, 1.2f);
    }

    void PlayDeflectSound()
    {
        afterimageManager.character.unscaledAudioSource.PlayOneShot(glassShatterSFX, 1.2f);
    }

    void PlayParticles()
    {
        leftDustParticles.Play();
        rightDustParticles.Play();
        growingCircleParticles.Play();
        growingCircleParticles.transform.LookAt(afterimageManager.character.transform);
        windBacktrailParticles.Play();
    }

    void  AddTerrainCrack()
    {
        crackDecal.material.DOColor(initialCrackColor, "_DecalColor", CRACK_REMOVAL_SPEED / 10);
    }

    void RemoveTerrainCrack()
    {
        crackDecal.material.DOColor(finalCrackColor, "_DecalColor", CRACK_REMOVAL_SPEED);
    }

    private void Update()
    {
    }
}
