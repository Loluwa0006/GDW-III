using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AirStateResource", menuName = "Scriptable Objects/AirStateResource")]
public class AirStateResource : ScriptableObject
{
    public enum JumpTypes
    {
        GroundJump,
        AirJump,
        SpecialJump,
    }

    [System.Serializable]
    public class JumpInfo
    {
        public JumpTypes jumpType;

        public float jumpTimeToPeak = 0.4f;
        public float jumpTimeToDecent = 0.5f;
        public float jumpHeight = 5.0f;

        [HideInInspector] public float jumpVelocity;
        [HideInInspector] public float jumpGravity;
        [HideInInspector] public float fallGravity;

        public float maxFallSpeed = 10.0f;
    }



    [SerializeField] List<JumpInfo> jumpInfo = new();
    public Dictionary<JumpTypes, JumpInfo> jumpMap = new();

    public float airAcceleration = 2.0f;
    public float airStrafeSpeed = 20.0f;




    public void InitializeResource()
    {
        foreach (var info in jumpInfo)
        {
            info.jumpGravity = (2.0f * info.jumpHeight) / (info.jumpTimeToPeak * info.jumpTimeToPeak);
            info.fallGravity = (2.0f * info.jumpHeight) / (info.jumpTimeToDecent * info.jumpTimeToDecent);
            info.jumpVelocity = (2.0f * info.jumpHeight) / info.jumpTimeToPeak;

            jumpMap[info.jumpType] = info;
            info.maxFallSpeed = Mathf.Abs(info.maxFallSpeed) * -1; //force it to be negative
        }
       
    }

    
}
