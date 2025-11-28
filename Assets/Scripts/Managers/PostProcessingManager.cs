using DG.Tweening;
using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] Animator postprocessingAnimator;
    [SerializeField] Volume BAndWProcessor;
    [SerializeField] Volume suddenDeathProcessor;
    [SerializeField] Volume strongAttackProcessor;

    [SerializeField] ColorCorrection suddenDeathCorrectionCamera;
    [SerializeField] ColorCorrection strongHitCorrectionCamera;
    [SerializeField] ColorCorrection b_and_w_CorrectionCamera;
    [SerializeField] ColorCorrection postGameCorrectionCamera;

    [SerializeField] RawImage suddenDeathImage;
    [SerializeField] RawImage strongHitImage;
    [SerializeField] RawImage b_and_w_Image;
    [SerializeField] RawImage superContrastImage;

    [SerializeField] Material strongHitMaterial;
    [SerializeField] Material suddenDeathMaterial;
    [SerializeField] Material b_and_w_CorrectionMaterial;
    [SerializeField] Material superContrastMaterial;

    [SerializeField] float strongHitContribution = 0.125f;
    [SerializeField] float suddenDeathContribution = 0.35f;
    [SerializeField] float b_and_w_Contribution = 0.25f;
    [SerializeField] float superContrastContribution = 0.55f;

    bool inSuddenDeath;

    enum AnimatorLayers
    {
        WorldLayer = 0,
        AttackReactionLayer = 1,
    }

    private void Start()
    {
        if (postprocessingAnimator == null)
        {
            postprocessingAnimator = GetComponent<Animator>();
        }
    }

    public void OnMatchEnd()
    {
        ResetManager();
        superContrastMaterial.SetFloat("_Contribution", superContrastContribution);
    }

    public void OnMatchStart()
    {
        ResetManager();
    }
    public void OnSpeakerStruck (DamageInfo info)
    {
        if (info.damageSource != DamageSource.Ball) { return; }
        StartCoroutine(OnSpeakerStruck());
        Debug.Log("Setting screen to black and white");
    }

    public void OnSuperDeflectPerformed(BaseSpeaker speaker)
    {
        StartCoroutine(OnSuperDeflectPerformed());
        Debug.Log("Setting screen to dark blue");

    }

    public void OnSuddenDeathStarted()
    {
        suddenDeathImage.transform.SetAsLastSibling();
        suddenDeathMaterial.SetFloat("_Contribution", 0.0f);
        suddenDeathMaterial.DOFloat(suddenDeathContribution, "_Contribution", 30.0f);

        Debug.Log("Playing sudden death");
        inSuddenDeath = true;
    }


    IEnumerator OnSpeakerStruck()
    {
        b_and_w_Image.transform.SetAsLastSibling();
        b_and_w_CorrectionMaterial.SetFloat("_Contribution", superContrastContribution);
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        b_and_w_CorrectionMaterial.DOFloat(0, "_Contribution", Time.fixedDeltaTime * 5);
        OnPostProcessingOver();
    }

    IEnumerator OnSuperDeflectPerformed()
    {
        Debug.Log("Strong deflect performed");
        superContrastImage.transform.SetAsLastSibling();

        strongHitMaterial.SetFloat("_Contribution", strongHitContribution);
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        strongHitMaterial.DOFloat(0, "_Contribution", Time.fixedDeltaTime * 5);
        OnPostProcessingOver();

    }
    public void ResetManager()
    {
        StopAllCoroutines();
        for (int i = 0; i < System.Enum.GetValues(typeof(AnimatorLayers)).Length; i++) 
        {
            postprocessingAnimator.Play("PostProcessReset", i, 0.0f);
        }
        BAndWProcessor.weight = 0.0f;
        suddenDeathProcessor.weight = 0.0f;
        strongAttackProcessor.weight = 0.0f;

        suddenDeathMaterial.SetFloat("_Contribution", 0.0f);
        strongHitMaterial.SetFloat("_Contribution", 0.0f);
        b_and_w_CorrectionMaterial.SetFloat("_Contribution", 0.0f);
        superContrastMaterial.SetFloat("_Contribution", 0.0f);
        inSuddenDeath = false;
    }

    void OnPostProcessingOver()
    {
        if (inSuddenDeath) suddenDeathImage.transform.SetAsLastSibling();
    }
}
 