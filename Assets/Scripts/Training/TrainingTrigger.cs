using UnityEngine;
using UnityEngine.Events;

public class TrainingTrigger : MonoBehaviour
{
    [SerializeField] TrainingManager manager;

    public UnityEvent trainingTriggerActivated = new();

    private void Awake()
    {
        if (manager == null) manager = FindFirstObjectByType<TrainingManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BaseSpeaker speaker)) return;


        if (speaker == manager.playerSpeaker)
        {
            trainingTriggerActivated.Invoke();
        }

    }
}
