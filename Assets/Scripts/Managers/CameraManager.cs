using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public enum ShakeID
    {
        EchoHitshake
    }
    [System.Serializable]
    public class ShakeInfo
    {
        public CinemachineImpulseSource impulseSource;
        public ShakeID id;
        public int shakeAmount;
    }


    public CinemachineCamera cinemachineCam; // May be used in the future, unused for now
    public Camera mainCam;
    public Camera postprocessCam;
    [SerializeField] CinemachineGroupFraming groupFraming;
    [SerializeField] CinemachineTargetGroup targetGroup;
    [SerializeField] float bonusZoomInOnHit = 4.0f;

    [SerializeField] List<ShakeInfo> shakeList = new();

    readonly Dictionary<ShakeID, ShakeInfo> shakeLookup = new();

    public static float DEFAULT_FRAME_SIZE = 8;

    private void Awake()
    {
        foreach (var shake in shakeList)
        {
            if (!shakeLookup.ContainsKey(shake.id))
            {
                shakeLookup[shake.id] = shake;
            }
            else
            {
                Debug.LogWarning("Tried to add duplicate key " + shake.id);
            }
        }

        groupFraming = cinemachineCam.transform.GetComponent<CinemachineGroupFraming>();
        DEFAULT_FRAME_SIZE = groupFraming.FramingSize;

    }
    public void OnSpeakerStruck(BaseSpeaker speaker, DamageInfo info)
    {
        ShakeInfo echoShake;

        switch (info.damageSource)
        {
            case DamageSource.Ball:
                echoShake = shakeLookup[ShakeID.EchoHitshake];
                TriggerShake(echoShake);
                StartCoroutine(ZoomOnVictim(speaker));
                break;


        }
    }

    public void TriggerShake(ShakeInfo info)
    {
        info.impulseSource.GenerateImpulse(info.shakeAmount);
    }

 
    IEnumerator ZoomOnVictim(BaseSpeaker speaker)
    {
        yield return new WaitUntil(() => GameManager.inSpecialStop);
        int index = targetGroup.FindMember(speaker.transform);
        if (index == -1) yield break;
        targetGroup.Targets[index].Weight += bonusZoomInOnHit;
        yield return new WaitUntil(() => !GameManager.inSpecialStop);
        targetGroup.Targets[index].Weight -= bonusZoomInOnHit;

    }

    private void LateUpdate()
    {
        postprocessCam.fieldOfView = mainCam.fieldOfView;
    }
}