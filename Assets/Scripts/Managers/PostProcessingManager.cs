using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] Animator postprocessingAnimator;
    [SerializeField] Volume BAndWProcessor;

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


    IEnumerator OnSpeakerStruck()
    {
        postprocessingAnimator.Play("SetB&W", 0, 0.0f);
        yield return null;
        Debug.Log("B & W Processor weight == " + BAndWProcessor.weight);
        if (!GameManager.inSpecialStop) yield return new WaitUntil(() => GameManager.inSpecialStop);
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        postprocessingAnimator.Play("EndB&W", 0, 0.0f);
    }
 }
 