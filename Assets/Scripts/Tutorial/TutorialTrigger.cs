using UnityEngine;


[RequireComponent (typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] TutorialManager manager;
    [SerializeField] Collider triggerCollider;
    [SerializeField] TutorialManager.SectionName sectionName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }
        if (manager == null)
        {
            manager = FindFirstObjectByType<TutorialManager>();
        }
        triggerCollider.isTrigger = true;
        if (manager != null)
        {
            manager.sectionRestarted.AddListener(OnSectionRestarted);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) { return; } 
        if (!other.TryGetComponent(out BaseSpeaker player)) { return; }

        manager.OnTutorialPointGained();
        triggerCollider.enabled = false;
    }

    public void OnSectionRestarted(TutorialManager.TutorialSection section)
    {
        if (section.sectionName == sectionName)
        {
            triggerCollider.enabled = true;
        }
    }

}
