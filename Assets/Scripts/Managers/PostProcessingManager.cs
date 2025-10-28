using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] Animator postprocessingAnimator;
    [SerializeField] Volume BAndWProcessor;
    [SerializeField] Volume suddenDeathProcessor;

    enum AnimatorLayers
    {
        WorldTone = 0,
        BAndWTone = 1,
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
        StopAllCoroutines();
        StartCoroutine(OnSpeakerStruck());
        Debug.Log("Setting screen to black and white");
    }

    public void OnSuddenDeathStarted()
    {
        postprocessingAnimator.Play("SetSuddenDeath", (int) AnimatorLayers.WorldTone, 0.0f);
    }


    IEnumerator OnSpeakerStruck()
    {
        postprocessingAnimator.Play("SetB&W", (int) AnimatorLayers.BAndWTone, 0.0f);
        yield return null;
        Debug.Log("B & W Processor weight == " + BAndWProcessor.weight);
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        postprocessingAnimator.Play("EndB&W", (int) AnimatorLayers.BAndWTone, 0.0f);
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
    }
 }
 