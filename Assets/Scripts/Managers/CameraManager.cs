using System.Collections.Generic;
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


    [SerializeField] CinemachineCamera cinemachineCam; // May be used in the future, unused for now

    [SerializeField] List<ShakeInfo> shakeList = new();

    readonly Dictionary<ShakeID, ShakeInfo> shakeLookup = new();
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
    }
    public void OnSpeakerStruck(DamageInfo info)
    {
        ShakeInfo echoShake;

        switch (info.damageSource)
        {
            case DamageSource.Ball:
                echoShake = shakeLookup[ShakeID.EchoHitshake];
                TriggerShake(echoShake);
                break;

        }
    }

    public void TriggerShake(ShakeInfo info)
    {
        info.impulseSource.GenerateImpulse(info.shakeAmount);
    }
}