using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] Animator postprocessingAnimator;
    [SerializeField] Volume BAndWProcessor;
    [SerializeField] Volume suddenDeathProcessor;
    [SerializeField] Volume strongAttackProcessor;

    enum AnimatorLayers
    {
        WorldLayer = 0,
        BAndWLayer = 1,
        StrongAttackLayer = 2,
    }

    private void Start()
    {
        if (postprocessingAnimator == null)
        {
            postprocessingAnimator = GetComponent<Animator>();
        }
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
    }

    public void OnSuddenDeathStarted()
    {
        postprocessingAnimator.Play("SetSuddenDeath", (int) AnimatorLayers.WorldLayer, 0.0f);
        Debug.Log("Playing sudden death");
    }


    IEnumerator OnSpeakerStruck()
    {
        postprocessingAnimator.Play("SetB&W", (int) AnimatorLayers.BAndWLayer, 0.0f);
        yield return null;
        Debug.Log("B & W Processor weight == " + BAndWProcessor.weight);
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        postprocessingAnimator.Play("EndB&W", (int) AnimatorLayers.BAndWLayer, 0.0f);
    }

    IEnumerator OnSuperDeflectPerformed()
    {
        Debug.Log("Strong deflect performed");
        postprocessingAnimator.Play("SetStrongAttack", (int) AnimatorLayers.StrongAttackLayer, 0.0f);
        yield return null;
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        postprocessingAnimator.Play("EndStrongAttack", (int)AnimatorLayers.StrongAttackLayer, 0.0f);
    }
 
   public void ResetManager()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(AnimatorLayers)).Length; i++) 
        {
            postprocessingAnimator.Play("Reset", i, 0.0f);
        }
        StopAllCoroutines();
        BAndWProcessor.weight = 0.0f;
        suddenDeathProcessor.weight = 0.0f;
        strongAttackProcessor.weight = 0.0f;
    }
 }
 