using System.Collections;
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
    public void OnPlayerStruck (DamageInfo info)
    {
        Debug.Log("Player struck");
        BAndWProcessor.weight = 1.0f;
        StartCoroutine(RestoreColor());
    }

    IEnumerator RestoreColor()
    {
        yield return null;
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        postprocessingAnimator.Play("EndB&W");
    }
 }
