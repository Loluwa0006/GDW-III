using TMPro;
using UnityEngine;

public class HUDAnimator : MonoBehaviour
{
    int deflectStreak = 0;
    [SerializeField] Animator animator;
    [SerializeField] TMP_Text streakDisplay;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    public void OnEchoDeflected(BaseEcho echo, bool partial)
    {
        deflectStreak++;
        streakDisplay.text = deflectStreak.ToString();
        animator.Play("IncrementDeflectStreak", 0, 0.0f);
    }
    
    public void OnSpeakerStruck(DamageInfo info)
    {
        if (info.damageSource != DamageSource.Ball || deflectStreak == 0) { return; }
        deflectStreak = 0;
        streakDisplay.text = deflectStreak.ToString();
        animator.Play("EndDeflectStreak", 0, 0.0f);
    }

  
}
